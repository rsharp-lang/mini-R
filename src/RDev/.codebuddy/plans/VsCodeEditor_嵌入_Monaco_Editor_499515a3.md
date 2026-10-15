---
name: VsCodeEditor 嵌入 Monaco Editor
overview: "在 WinForms 的 WebView2 控件（WebView21）中加载已嵌入的 monaco-editor.html，并通过 WebView2 的 WebResourceRequested 事件拦截页面发起的所有子资源请求，将 loader.js、editor.main.css/js、各语言模块、worker 等资源从编译时嵌入的 monaco-editor.zip（已加载进共享变量 cdn: ZipStream 虚拟文件系统）中按路径提供，从而构建一个完全自包含、无需外部网络的代码编辑器窗口。同时注入 MonacoEnvironment.getWorkerUrl 以修复 zip 中缺失 workerMain.js 的问题，并实现 C#↔JS 的内容读写桥接。"
todos:
  - id: register-html-resource
    content: 在 RDev.NET5.vbproj 中将 vscode\monaco-editor.html 登记为 EmbeddedResource
    status: completed
  - id: fix-html-worker
    content: 修改 monaco-editor.html，移除硬编码 getWorkerUrl(workerMain.js) 代码块，保留编辑器创建与内容桥接函数
    status: completed
    dependencies:
      - register-html-resource
  - id: implement-resource-interceptor
    content: 在 VsCodeEditor.vb 实现 WebResourceRequested 拦截，从 cdn 按 monaco-editor/ 前缀提供资源并做 MIME 映射与 404
    status: completed
    dependencies:
      - fix-html-worker
  - id: wire-editor-load
    content: 在 VsCodeEditor_Load 中初始化 WebView2、注册拦截、注入 getWorkerUrl(动态发现 zip 内 worker 文件名)、导航虚拟主机
    status: completed
    dependencies:
      - implement-resource-interceptor
  - id: add-content-bridge
    content: 增加 SetContent/GetContent 公共方法与 WebMessageReceived 处理，支持 C#↔JS 内容读写
    status: completed
    dependencies:
      - wire-editor-load
  - id: build-verify
    content: 构建 RDev 并运行，验证 Monaco 加载、语法高亮与 worker 语言服务可用
    status: completed
    dependencies:
      - add-content-bridge
---

## 修订点（相对初版）

1. 资源名更正：monaco-editor.zip 的嵌入资源名为 `RDev.monaco-editor.zip`，monaco-editor.html 的资源名为 `RDev.monaco-editor.html`（**不带 vscode/ 前缀**）。
2. 重新打包后的 zip 采用**标准 `min/vs/` 布局**（非初版的扁平化 `monaco-editor/` 布局），故资源路径映射前缀为 `min/vs`。
3. 重新探查确认：zip 仍**不包含** `min/vs/base/worker/workerMain.js`（无 `base/` 目录），worker 仍需由 C# 注入 `getWorkerUrl` 指向 `min/vs/assets/*worker*.js`。
4. 现有 `VsCodeEditor.Shared Sub New()` 用 `asm.GetName().Name & ".vscode.monaco-editor.zip"` + 回退 `vscode.monaco-editor.zip` 查找，与新资源名 `RDev.monaco-editor.zip` 不符，必须改为**按后缀 `monaco-editor.zip` / `monaco-editor.html` 容错匹配**，否则加载会抛 MissingManifestResourceException。

## 用户需求

在 WinForms 窗体（VsCodeEditor 用户控件）的 WebView2 控件（WebView21）中加载已嵌入程序集的 `monaco-editor.html`，构建一个完全自包含、无需外部网络的代码编辑器窗口；并将编译时已加载到共享变量 `cdn`（ZipStream 虚拟文件系统）的 `monaco-editor.zip` 作为资源文件系统，供该 HTML 页面按需调用其中资源（loader.js、editor.main.css/js、各语言模块、worker 等）。

## 核心功能

- 窗体 Load 时通过 WebView2 加载 Monaco 编辑器页面（HTML 内容取自嵌入资源 `RDev.monaco-editor.html`）。
- 接管 WebView2 的资源请求：页面发起的所有子资源（js/css/字体/图片/worker）请求由 `WebResourceRequested` 事件拦截，并映射到 `cdn`（ZipStream）虚拟文件系统中按路径提供。
- 修复 Worker 加载：zip 内不存在 `base/worker/workerMain.js`，改由 C# 在文档创建时注入 `MonacoEnvironment.getWorkerUrl`，将 label 映射到 zip 中 `min/vs/assets/*worker-*.js`（含哈希文件名）的真实地址，使 TS/JSON/CSS/HTML 等语言服务可用。
- 提供 C# 与 JS 之间的编辑器内容读写桥接（设置/获取文本与语言）。

## 技术栈

- VB.NET（net10.0-windows + UseWindowsForms=true），WinForms 用户控件。
- Microsoft.Web.WebView2（CoreWebView2 资源拦截 + 文档创建脚本注入）。
- `Microsoft.VisualBasic.Core` 的 `ZipStream`（已在 `VsCodeEditor.Shared Sub New` 中从嵌入资源加载到 `Shared ReadOnly cdn`）。
- 资源来源：编译时嵌入的 `monaco-editor.zip`（资源名 `RDev.monaco-editor.zip`）与 `monaco-editor.html`（资源名 `RDev.monaco-editor.html`）。

## zip 内部结构（已重新探查，修订）

条目全部位于 `min/vs/` 前缀下，为标准 monaco 部署布局：

- `min/vs/loader.js`
- `min/vs/editor/editor.main.css`、`min/vs/editor/editor.main.js`
- `min/vs/assets/editor.worker-<hash>.js`、`json.worker-<hash>.js`、`css.worker-<hash>.js`、`html.worker-<hash>.js`、`ts.worker-<hash>.js`
- `min/vs/<lang>-<hash>.js`、`min/vs/basic-languages/`、`min/vs/language/` 等
- **不存在** `min/vs/base/worker/workerMain.js`，也无 `base/` 目录。

HTML 中 `require.config({ paths: { 'vs': '/' } })` 把模块 id 的 `vs/` 段映射为 `/`，即 web 根 = `min/vs/`。因此：

- 请求路径 `/X` → zip 条目 `min/vs` & X（去掉前导 `/`）：
- `/loader.js` → `min/vs/loader.js`
- `/editor/editor.main.css` → `min/vs/editor/editor.main.css`
- `/assets/json.worker-<hash>.js` → `min/vs/assets/json.worker-<hash>.js`
- `/basic-languages/monaco.contribution.js` → `min/vs/basic-languages/monaco.contribution.js`

## 实现方案（修订）

采用「虚拟主机 + 资源请求拦截网关」模式：固定同源虚拟主机（如 `https://monaco.local/`）承载页面，所有 `https://monaco.local/*` 请求经 `CoreWebView2.WebResourceRequested` 拦截，按请求路径映射到 `cdn` 中的 zip 条目（前缀 `min/vs`）；`.html` 文档本身从嵌入的 HTML 资源（`RDev.monaco-editor.html`）返回，其余路径按 `min/vs` & LocalPath 映射。

关键决策与权衡（同初版，事实更新）：

1. 纯内存拦截，不依赖 `SetVirtualHostNameToFolderMapping`；`WebResourceRequested` 在内存中直接返回 `cdn.OpenFile` 的 `MemoryStream`。
2. **Worker 由 C# 注入**：HTML 原 `getWorkerUrl` 返回 data:URL 并 `importScripts('/base/worker/workerMain.js')`，该文件在 zip 中不存在会 404。改为在 `VsCodeEditor_Load` 中先枚举 `cdn.GetFiles()` 找到 `min/vs/assets/*worker*.js`，构建「label→含哈希文件名」映射（editor→editor.worker、json→json.worker、css→css.worker、html→html.worker、typescript|javascript→ts.worker），通过 `AddScriptToExecuteOnDocumentCreatedAsync` 注入 `getWorkerUrl`，返回同源 `https://monaco.local/assets/<文件名>`。同时删除 HTML 中整体覆盖 `MonacoEnvironment` 的 getWorkerUrl 代码块，避免注入被覆盖。注意 worker 请求可能带 `?workerId=...&label=...` 查询串，拦截器须用 `Uri.LocalPath`（已剔除查询）做映射。
3. MIME 由扩展名手写映射（不引入 System.Web）：.js→application/javascript、.css→text/css、.json→application/json、.html→text/html; charset=utf-8、.ttf/.woff/.woff2/.svg/.png/.map 等。
4. 流生命周期：`cdn` 为 Shared 静态、随进程存活；`cdn.OpenFile` 内部已将 deflate 流拷入 `MemoryStream` 并 `Seek(0)`，可直接交给 `CreateWebResourceResponse`。

## 实现注意事项

- 拦截过滤器：`CoreWebView2WebResourceContext.All` + 通配 `https://monaco.local/*`，确保 document/stylesheet/script/worker/font/image 全部命中；过滤限定本虚拟主机。
- 响应头字符串用 CRLF 分隔：`"Content-Type: application/javascript\r\n"`。
- 文档请求 `LocalPath` 为 `/` 或空时返回嵌入的 `RDev.monaco-editor.html`；其余拼成 `min/vs` & LocalPath（去掉前导 `/`）后用 `cdn.FileExists` 判断，存在则 `cdn.OpenFile` 作响应流，否则 404。
- **修正资源名查找（关键）**：`Shared Sub New` 中改为遍历 `asm.GetManifestResourceNames()`，取 `EndsWith("monaco-editor.zip", OrdinalIgnoreCase)` / `EndsWith("monaco-editor.html")` 的项，避免初版写死的 `vscode.monaco-editor.zip` 名称找不到资源。HTML 文档读取同样用此容错方式。
- C#↔JS 桥接：`WebMessageReceived` 接收 `postMessage('initialized')`；`SetContent`/`GetContent` 通过 `CoreWebView2.ExecuteScriptAsync` 调用 HTML 中 `window.setEditorContent`/`window.getEditorContent`，跨线程用 `WebView21.Invoke` 封送到 UI 线程。
- 保留 `WebViewLoader.Init(WebView21)` 完成 `EnsureCoreWebView2Async`，之后再做拦截注册与 `Navigate`。

## 架构设计

```mermaid
flowchart LR
    A[VsCodeEditor 窗体 Load] --> B[WebViewLoader.Init 初始化 CoreWebView2]
    B --> C[注册 WebResourceRequested 拦截 + 注入 getWorkerUrl(min/vs/assets)]
    C --> D[Navigate https://monaco.local/]
    D --> E{WebResourceRequested}
    E -->|LocalPath='/'| F[返回嵌入 RDev.monaco-editor.html]
    E -->|其它路径| G[cdn.FileExists min/vs+path]
    G -->|命中| H[cdn.OpenFile 返回流 + MIME]
    G -->|未命中| I[404]
    F --> J[Monaco 加载 loader.js/editor.main 等 -> 再次经 E]
    J --> K[getWorkerUrl 指向 /assets/*.worker.js -> 经 E 提供]
```

## 目录结构（修改点）

```
g:\mini-R\src\RDev\
├── RDev.NET5.vbproj   # [MODIFY] EmbeddedResource 新增 vscode\monaco-editor.html（zip 已登记）
├── vscode\
│   └── monaco-editor.html  # [MODIFY] 删除整体覆盖 window.MonacoEnvironment 的 getWorkerUrl(data:URL/workerMain.js) 代码块，保留 require.config、编辑器创建、setEditorContent/getEditorContent 与 postMessage('initialized')
└── VsCodeEditor.vb   # [MODIFY] ① Shared Sub New 资源名改为容错后缀匹配（monaco-editor.zip / .html）；② 实现 WebResourceRequested 拦截（min/vs 前缀映射 + MIME + 404）；③ VsCodeEditor_Load 中 Init 后注册过滤、注入 getWorkerUrl、Navigate；④ 新增 SetContent/GetContent 与 WebMessageReceived
```

## 关键代码结构（要点）

- 资源拦截：`Private Sub OnWebResourceRequested(sender As Object, e As CoreWebView2WebResourceRequestedEventArgs)`，取 `e.Request.Uri` 的 `LocalPath`，文档返回嵌入 HTML，其余映射 `cdn`；用 `e.GetDeferral()` 包裹并 `Try/Finally` 中 `def.Complete()`。
- 注入脚本（文档创建时执行，先于 HTML 内联脚本）：设置 `window.MonacoEnvironment.getWorkerUrl = function(workerId, label){ 按 label 在前端已知 assets 文件名数组中匹配前缀，返回 'https://monaco.local/assets/' + 文件名 }`，文件名数组由 C# 枚举 `cdn.GetFiles()` 中以 `min/vs/assets/` 开头、含 `.worker` 的条目生成（取 basename）。
- 扩展名→MIME：`Function MimeFromPath(path As String) As String`，覆盖 js/css/json/html/ttf/woff/woff2/svg/png/map 等。

## 验证

构建 RDev 并运行，确认 Monaco 加载、语法高亮、worker 语言服务（TS/JSON/CSS/HTML 的 F12/报错）可用，WebView2 控制台无 404（除 favicon）。