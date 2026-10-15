---
name: EnhanceVsCodeEditorBridge
overview: 完善 VsCodeEditor 与前端 editor.js 的双向消息桥。①接收 editor.js 的 Outgoing 状态变化消息（TextChanged / IsDirtyChanged / Languages，EditorReady 已处理）并在 VB 侧暴露为 .NET 事件与状态属性；②为 editor.js 的 Incoming 选项（SetReadOnly / MarkAsSaved / GetLanguages / SetLanguage / UpdateOption / RegisterTheme / SetTheme / RegisterLanguage，SetText 已有 SetContent）新增对应的 VB 公开方法，统一经 WebView2CommandChannel 发送，并在编辑器尚未就绪时排队、EditorReady 后冲刷。editor.js 契约已完整，无需改动。
todos:
  - id: add-state-events
    content: 在 VsCodeEditor.vb 新增状态属性与事件(TextChanged/IsDirtyChanged/LanguagesReceived/IsModified/Languages)
    status: completed
  - id: handle-outgoing
    content: 扩展 OnWebMessageReceived 处理 TextChanged/IsDirtyChanged/Languages 并维护状态
    status: completed
    dependencies:
      - add-state-events
  - id: add-send-helper
    content: 新增 SendCommand/FlushPending 助手与 _pending 排队，EditorReady 改调冲刷
    status: completed
  - id: add-option-methods
    content: 为 SetReadOnly/MarkAsSaved/GetLanguages/SetLanguage/UpdateOption/RegisterTheme/SetTheme/RegisterLanguage 新增公开方法
    status: completed
    dependencies:
      - add-send-helper
  - id: rewire-setcontent
    content: 将 SetContent 改为走 SendCommand，移除 FlushContent 单独缓冲
    status: completed
    dependencies:
      - add-send-helper
  - id: build-verify
    content: 构建 RDev 工程确认 0 错误并自检桥接
    status: completed
    dependencies:
      - handle-outgoing
      - add-option-methods
      - rewire-setcontent
---

## 用户需求

完善 `VsCodeEditor.vb` 与前端 `editor.js` 之间的双向消息桥。前端编辑器已通过 `editor.js` 定义了完整的消息契约（Incoming 选项与 Outgoing 状态），但 VB 宿主侧目前仅消费了 `EditorReady` 与 `SetText`（`SetContent`），其余契约均未落地。

## 核心功能

- 接收并响应前端的状态变化通知（Outgoing，`editor.js:21-26`）：`TextChanged`（文本变更）、`IsDirtyChanged`（脏标记变更）、`Languages`（语言列表回传），在 VB 侧暴露为 .NET 事件与只读状态属性，供宿主窗体检查编辑器状态。
- 为前端的全部 Incoming 选项（`editor.js:9-19`）提供对应的 VB 公开方法：`SetReadOnly`、`MarkAsSaved`、`GetLanguages`、`SetLanguage`、`UpdateOption`、`RegisterTheme`、`SetTheme`、`RegisterLanguage`（`SetText` 已由 `SetContent` 覆盖）。
- 统一经 `WebView2CommandChannel` 发送，并在编辑器尚未就绪（`EditorReady` 未到达）时安全排队，就绪后自动冲刷，避免竞态与静默丢失。

## 技术栈

- 宿主：VB.NET（WinForms），Microsoft.Web.WebView2 1.0.4258.31
- 桥接：WebView2 `PostWebMessageAsJson`（经 `galaxy/WebViewHelper/WebView2CommandChannel`）/ `WebMessageReceived`（经 `JsonParser` 解析 `JsonObject`）
- 序列化：`System.Text.Json`（payload 构造、回读），与既有 `Microsoft.VisualBasic.MIME.application.json` 解析并存且各自职责清晰
- 前端：`editor.js`（Monaco Editor，AMD 加载），契约已完整，无需改动

## 实现策略

在既有 `VsCodeEditor` 类内补全双向桥，复用已建立的 `_channel`（单例 `WebView2CommandChannel`，`IsReady=True`）与 `_editorReady`（前端就绪标志）两个门控，不触碰 `galaxy/WebViewHelper` 与 `editor.js`。

1. **状态接收（Outgoing）**：在 `OnWebMessageReceived` 现有 `JsonObject`/`JsonParser` 解析基础上，依据 `msg!type` 增加 `TextChanged`/`IsDirtyChanged`/`Languages` 三个分支；沿用与 `EditorReady` 相同的解析 API（`msg!payload` 动态访问器 + `AsBoolean`/`AsString`/`AsArray`）保持一致性，避免引入第二套 JSON 解析带来回归。各分支维护 `_isDirty`/`_languages` 并触发对应 .NET 事件。
2. **统一发送助手 + 未就绪排队**：新增 `SendCommand(type, payload)` 与 `_pending` 队列；仅在 `_channel.IsReady AndAlso _editorReady` 时真正发送，否则入队，`EditorReady` 分支改为调用 `FlushPending()`。`SetContent` 内部改为 `SendCommand("SetText", code)` 与 `SendCommand("SetLanguage", lang)`，移除原 `FlushContent` 的单独缓冲，统一收敛到 `_pending`，减少重复状态。
3. **选项方法（Incoming）**：为 `editor.js:9-19` 每个选项新增公开方法，payload 用 `Dictionary(Of String, Object)` 承载结构化对象（键名与 editor.js handler 期望一致：`option/value`、`name/base/inherit/rules/colors`、`id/monarch/configuration`）。`rules`/`colors` 以已反序列化的 JSON 对象（`Dictionary`/`JsonElement`）传入，`monarch`/`configuration` 以 JS 源码字符串传入（editor.js 内 `evaluateJsObjectLiteral` 已能消费）。
4. **`GetLanguages` 往返**：`GetLanguagesAsync()` 经 `SendCommand("GetLanguages", Nothing)` 发起请求，并用 `TaskCompletionSource(Of String())` 等待前端 `Languages` 回传（含 2s 超时兜底，未就绪返回空数组），同时 `LanguagesReceived` 事件供非阻塞消费。

## 实现注意（防回归）

- 不修改 `galaxy/src/WebViewHelper/WebView2CommandChannel.vb` 与 `WebViewLoader.vb`；其 `IsReady` 门控语义正确，仅本模块置 `IsReady=True` 即可生效。
- 不修改 `editor.js`（契约完整）。
- 保留 `GetContent`（`ExecuteScriptAsync("window.getEditorContent()")`）作为文本回读通道，与新增 `TextChanged` 事件互为补充。
- 事件触发需在 UI 线程（WebView2 回调线程）进行；若宿主订阅者需更新控件，由订阅者自行 `Invoke`，本模块不强制封送以减小耦合。
- `IsDirtyChanged` 的布尔解析依赖 editor.js `send(Outgoing.IsDirtyChanged, isDirty)` 的 JSON 布尔值；`TextChanged` 的 payload 为字符串，注意 JSON 转义由 `WebView2CommandChannel` 处理。
- `_pending` 仅缓存“设置类”命令（SetText/SetLanguage/SetReadOnly/...），`EditorReady`/`TextChanged`/`IsDirtyChanged`/`Languages` 为前端上行消息，不入队。

## 架构设计

```mermaid
sequenceDiagram
    participant Host as VsCodeEditor(VB)
    participant WV as WebView2
    participant JS as editor.js(Monaco)

    Note over Host,JS: 初始化
    Host->>WV: Init + _channel.IsReady=True + Navigate
    JS->>Host: postMessage(EditorReady)
    Host->>Host: _editorReady=True; FlushPending()(发送排队命令)

    Note over Host,JS: 宿主设置选项(Incoming)
    Host->>WV: SendCommand(SetReadOnly/SetLanguage/UpdateOption/...)
    WV->>JS: message -> 对应 handler

    Note over Host,JS: 前端状态上行(Outgoing)
    JS->>Host: postMessage(TextChanged/IsDirtyChanged)
    Host->>Host: 触发 TextChanged/IsDirtyChanged 事件 + 维护状态
    Host->>WV: GetLanguagesAsync -> SendCommand(GetLanguages)
    JS->>Host: postMessage(Languages)
    Host->>Host: _languagesTcs.TrySetResult + LanguagesReceived 事件
```

## 目录结构

```
g:/mini-R/src/RDev/
└── VsCodeEditor.vb   # [MODIFY] 补全双向桥：
                      #   1) 新增字段 _pending(List)、_isDirty(Boolean)、
                      #      _languages(String())、_languagesTcs(TaskCompletionSource)；
                      #   2) 新增事件 TextChanged/IsDirtyChanged/LanguagesReceived
                      #      与只读属性 IsModified/Languages；
                      #   3) OnWebMessageReceived 增加 TextChanged/IsDirtyChanged/
                      #      Languages 分支，EditorReady 分支改调 FlushPending；
                      #   4) 新增 SendCommand/FlushPending 助手；
                      #   5) 新增 SetReadOnly/MarkAsSaved/GetLanguagesAsync/SetLanguage/
                      #      UpdateOption/RegisterTheme/SetTheme/RegisterLanguage 方法；
                      #   6) SetContent 改为走 SendCommand（移除 FlushContent 单独缓冲）。
g:/mini-R/src/RDev/vscode/editor.js        # 不修改（契约完整）
g:/galaxy/src/WebViewHelper/*.vb           # 不修改（Send 门控语义正确）
```

## 关键代码结构（VB 侧新增成员示意）

```
' 事件与状态
Public Event TextChanged As EventHandler(Of String)
Public Event IsDirtyChanged As EventHandler(Of Boolean)
Public Event LanguagesReceived As EventHandler(Of String())
Public ReadOnly Property IsModified As Boolean
Public ReadOnly Property Languages As String()

' 统一发送与排队
Private _pending As New List(Of KeyValuePair(Of String, Object))
Private Sub SendCommand(type As String, payload As Object)
Private Sub FlushPending()

' 选项方法（对应 editor.js Incoming）
Public Sub SetReadOnly(value As Boolean)
Public Sub MarkAsSaved()
Public Async Function GetLanguagesAsync() As Task(Of String())
Public Sub SetLanguage(id As String)
Public Sub UpdateOption(option As String, value As Object)
Public Sub RegisterTheme(name As String, base As String, inherit As Boolean, rules As Object, colors As Object)
Public Sub SetTheme(themeName As String)
Public Sub RegisterLanguage(id As String, Optional monarch As String = Nothing, Optional configuration As String = Nothing)
```