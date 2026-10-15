---
name: FixMonacoEditorTextLoading
overview: 修复 VsCodeEditor 在 WebView2 中加载 Monaco 编辑器后无法把代码文本推送到前端的问题。根因有两点：(1) VsCodeEditor.vb 通过 `New WebView2CommandChannel(WebView21).Send(...)` 发送 SetText，但 WebView2CommandChannel.Send 仅在 `IsReady=True` 时才真正 PostWebMessageAsJson，而每次都新建实例、IsReady 默认 False，导致消息被静默丢弃；(2) editor.js 把编辑器实例声明为 require 回调内的局部 `const editor`，但全局 `window.setEditorContent`/`window.getEditorContent` 引用的是从未赋值的 `window.editor`，使 SetContent 的 ExecuteScriptAsync 路径也抛错。需修复消息通道的就绪门控、统一走消息契约，并在 editor.js 暴露 editor 实例。
todos:
  - id: patch-editor-js
    content: 在 editor.js 暴露 window.editor = editor 修复脚本访问
    status: completed
  - id: init-channel
    content: VsCodeEditor 新增 _channel 与 _editorReady，Init 后置 IsReady
    status: completed
  - id: route-setcontent
    content: VsCodeEditor 重写 SetContent/EditorReady 走消息通道并缓冲冲刷
    status: completed
    dependencies:
      - init-channel
  - id: build-verify
    content: 构建并打开 .r 文件验证文本加载与回传
    status: completed
    dependencies:
      - patch-editor-js
      - route-setcontent
---

## 用户需求

用户在 WinForms 的 WebView2 控件中集成了 Monaco 编辑器（通过嵌入式 monaco-editor.zip + editor.js 虚拟主机加载），编辑器界面能正常显示，但打开代码文件时文本无法加载进编辑器，编辑框始终为空。用户要求审查 `vscode\monaco-editor.html`、`vscode\editor.js`、`VsCodeEditor.vb` 三个文件，定位原因并提出修复方案，让代码文本能正确加载到前端编辑器页面。

## 产品概述

一个内嵌于 WinForms 窗体的轻量代码编辑器：由 VS 侧通过 WebView2 虚拟主机（https://monaco.local/）提供 Monaco 静态资源，加载完成后由宿主侧把文件文本推送到前端。当前编辑器能渲染，但文本推送通道失效。

## 核心功能

- 宿主侧（VB）通过消息通道将代码文本与语言推送到前端 Monaco 编辑器
- 前端（editor.js）接收消息并写入编辑器模型
- 编辑器内容变更后回传文本与脏标记给宿主
- 前端就绪后通知宿主，宿主据此在就绪态推送待显示文本

## 技术栈

- 前端：HTML + Monaco Editor（AMD `vs/loader.js` 加载），原生 JavaScript
- 宿主：VB.NET（WinForms），Microsoft.Web.WebView2 1.0.4258.31
- 桥接：WebView2 `PostWebMessageAsJson` / `WebMessageReceived` + `ExecuteScriptAsync`
- 共享辅助：`galaxy\src\WebViewHelper`（Shared Project，提供 `WebViewLoader.Init` 与 `WebView2CommandChannel`）

## 根因分析（已验证）

1. **消息发送被静默门控丢弃（主因）**：`VsCodeEditor.vb:123` 每次 `New WebView2CommandChannel(WebView21).Send(...)` 都新建实例，而 `WebView2CommandChannel.IsReady` 默认 `False`，其 `Send` 内部 `If IsReady Then ... PostWebMessageAsJson` 因此永远不发送。`EditorReady` 到达后本应回推 `SetText`，但消息被丢弃，导致编辑器始终空白。
2. **`window.editor` 从未赋值（次因，破坏 `SetContent` 的脚本路径）**：`editor.js:36` 的 `editor` 是 `require` 回调内的局部 `const`，未挂到 `window`；而全局 `window.setEditorContent`/`window.getEditorContent`（:232-243）引用 `window.editor`（始终 `undefined`），`SetContent`→`ExecuteScriptAsync("window.setEditorContent(...)")` 会抛 `Cannot read properties of undefined`。
3. **无前端就绪态跟踪**：`SetContent` 立即尝试推送，若页面尚未就绪则失败且不再重试；`CoreWebView2InitializationCompleted` 为空，未建立任何就绪门控。

说明：`editor.js` 的 `handleSetText`/`handleSetLanguage` 基于闭包 `editor` 的实现本身正确，缺的只是"消息能真正发出"与"`window.editor` 赋值"。

## 实现方案

### 总体策略

不改动 `webview2` 桥接辅助类，仅在消费侧满足其既有契约：用一个**模块级共享 `WebView2CommandChannel` 实例**并设置 `IsReady=True`，引入**前端就绪标志 `_editorReady`**，统一通过 `SetText`/`SetLanguage` 消息契约推送文本（就绪前缓冲，`EditorReady` 到达后冲刷），并补齐 `editor.js` 的 `window.editor` 暴露以修复 `GetContent` 与作为安全网。

### 关键技术决策

- **复用 `WebView2CommandChannel` 而非 `ExecuteScriptAsync`**：消息契约与 `editor.js` 的 `handlers` 完全对齐，且自带 `IsReady` 门控语义，仅需由消费者把 `IsReady` 置真即可生效，改动最小、最贴合现有架构。
- **共享通道实例 + 就绪双标志**：`WebView2` 环境就绪（Init 后）设通道 `IsReady`；JS 侧 `EditorReady` 设 `_editorReady`。只有两者皆真才直接发送，否则仅缓存 `code`/`lang`，待 `EditorReady` 冲刷，避免竞态与重复推送。
- **`editor.js` 暴露实例**：在 `monaco.editor.create` 后加 `window.editor = editor;`（`window.monaco = monaco;` 可选），使 `SetContentInternal`/`GetContent` 的脚本路径与已有的全局函数均可用，互为冗余保障。

### 性能与可靠性

- 推送为单次 JSON 消息，开销可忽略；文本大文件经 `JsonSerializer.Serialize` 直接入 `payload`，无需轮询。
- `ExecuteScriptAsync` 仅在 `GetContent` 读回时使用一次，不阻塞 UI 线程（已 `Await`）。
- 保留 `OnWebResourceRequested` 既有虚拟主机逻辑，不改动资源加载路径，控制改动爆炸半径。

## 实现注意（防回归）

- **不要**改动 `galaxy\src\WebViewHelper\WebView2CommandChannel.vb` 与 `WebViewLoader.vb`（门控语义正确，仅消费侧未置 `IsReady`）。
- `SetContent` 在 `WebView21.CoreWebView2 Is Nothing` 时只存 `code`，不调用通道；待 `Init` 完成后通道建立再由其统一发送，避免空引用。
- `EditorReady` 冲刷时同时发送 `SetLanguage`（若 `lang` 非空），保持语言与文本一致；`handleSetLanguage` 已正确处理。
- 保留 `code.StringEmpty` 判空（`StringEmpty` 为 sciBASIC# 扩展，可安全接收 `Nothing`）。

## 架构设计

```mermaid
sequenceDiagram
    participant Host as VsCodeEditor(VB)
    participant WV as WebView2
    participant JS as editor.js(Monaco)
    Host->>WV: WebViewLoader.Init (await)
    Host->>Host: _channel = New WebView2CommandChannel(WV); IsReady=True
    Host->>WV: Navigate(host) -> 加载 monaco-editor.html/editor.js
    JS->>Host: postMessage(EditorReady)
    Host->>Host: _editorReady = True; 若 code 非空则冲刷
    Host->>WV: PostWebMessageAsJson({type:SetText,payload:code})
    WV->>JS: window.chrome.webview message -> handleSetText
    JS->>JS: editor.setValue(code); baselineVersion 重置
    Note over Host,JS: 之后 SetContent 在就绪态直接 _channel.Send(SetText,...)
    JS->>Host: 内容变更 -> TextChanged/IsDirtyChanged
```

## 目录结构

```
g:/mini-R/src/RDev/
├── vscode/
│   └── editor.js            # [MODIFY] 在 monaco.editor.create 后暴露 window.editor = editor;
│                            #           （可选 window.monaco = monaco），使 window.setEditorContent/
│                            #           window.getEditorContent 可正常访问编辑器实例。
└── VsCodeEditor.vb          # [MODIFY] 新增模块级 _channel As WebView2CommandChannel 与
                             #           _editorReady As Boolean；Init 后建通道并 IsReady=True；
                             #           EditorReady 时置 _editorReady 并冲刷 code/lang；
                             #           SetContent 改为经 _channel.Send("SetText"/"SetLanguage")，
                             #           移除/改写失效的 SetContentInternal 脚本路径。

g:/galaxy/src/WebViewHelper/   # 不修改（契约正确）：WebView2CommandChannel.vb / WebViewLoader.vb
```

## 关键代码结构

`VsCodeEditor.vb` 需新增/调整的成员（示意，非完整实现）：

- 字段：`Private _channel As WebView2CommandChannel`、`Private _editorReady As Boolean`
- 就绪：`_channel = New WebView2CommandChannel(WebView21) : _channel.IsReady = True`（位于 `Await WebViewLoader.Init` 之后）
- 冲刷：`If _editorReady AndAlso _channel IsNot Nothing AndAlso _channel.IsReady Then _channel.Send("SetText", code) : If lang 非空 Then _channel.Send("SetLanguage", lang)`
- `SetContent`：存 `code`/`lang` 后，若已就绪立即 `_channel.Send("SetText", content)`（+语言），否则仅缓存等待 `EditorReady` 冲刷。