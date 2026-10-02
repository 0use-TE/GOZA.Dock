# 测试指南（供开发者与 AI 使用）

本文是仓库测试的固定入口，不随版本目录变化。修改代码前先查看相关测试，修改后执行对应回归测试，并运行完整测试集。真实平台测试由人工完成。

## 环境与入口

在仓库根目录运行下列命令，需要 .NET 10 SDK。测试入口是 `GOZA.Dock.Tests.slnx`，只包含库和两个测试项目，无需移动平台工作负载或显示服务器。主解决方案 `GOZA.Dock.slnx` 包含移动端示例，不适合作为纯测试入口。

```sh
dotnet test GOZA.Dock.Tests.slnx -c Release
```

此命令会自动还原依赖、编译并运行普通单元测试、无头功能测试和托管内存回归测试。依赖版本由 `Directory.Packages.props` 管理：目前是 Avalonia 12、Avalonia.Headless.XUnit 12 和 xUnit v3，不要混入 xUnit v2。

需要保留可检查的结果时：

```sh
dotnet restore GOZA.Dock.Tests.slnx
dotnet test GOZA.Dock.Tests.slnx -c Release --no-restore --logger trx --results-directory artifacts/test-results
```

仅在还原成功后使用 `--no-restore`。结果目录已忽略，不应提交生成的日志和构建产物。

## 按范围执行

| 范围 | 命令 |
| --- | --- |
| 普通单元测试 | `dotnet test tests/GOZA.Dock.Tests/GOZA.Dock.Tests.csproj -c Release` |
| 全部无头测试（包含内存测试） | `dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Release` |
| 仅托管内存测试 | `dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Release --filter "Category=Memory"` |
| 无头功能测试（排除内存测试） | `dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Release --filter "Category!=Memory"` |
| 指定测试类，例如拖动测试 | `dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Release --filter "FullyQualifiedName~DragTests"` |

涉及生命周期、缓存、事件解绑或拖动卸载时，额外执行 Debug 内存测试，检查不同编译模式下的对象释放：

```sh
dotnet test tests/GOZA.Dock.HeadlessTests/GOZA.Dock.HeadlessTests.csproj -c Debug --filter "Category=Memory"
```

2026-10-02 的本地基线：Release 普通单元测试 27 项、无头测试 32 项（其中功能 24 项、内存 8 项），全部通过；Debug 内存测试 8 项通过。后续新增或调整测试后，以实际运行结果为准，不能沿用本文基线宣称通过。

## 覆盖范围与修改对应关系

| 修改内容 | 首先查看的测试文件 | 验证内容 |
| --- | --- | --- |
| 主题 JSON、JSONC、颜色、嵌套 include | `tests/GOZA.Dock.Tests/ThemeJsonTests.cs` | 相对路径、父目录引用、覆盖顺序、循环检测、深度限制、流所有权 |
| 集合移动或排序 | `tests/GOZA.Dock.Tests/TabCollectionTests.cs` | 引用身份、索引、只读集合、类型不兼容、失败回滚、通知抛异常后的状态 |
| 示例布局持久化 | `tests/GOZA.Dock.Tests/LayoutPersistenceTests.cs` | 损坏或缺失文件的回退、不可读文件、保存与替换、临时文件清理 |
| 拖动、排序、跨区域移动 | `tests/GOZA.Dock.HeadlessTests/DragTests.cs` | 模拟鼠标输入、不同方向布局、失败移动、主题变更取消拖动 |
| 选择、关闭、缓存、最大化 | `tests/GOZA.Dock.HeadlessTests/RegionTests.cs` | 内容更新、生命周期、关闭按钮、异步关闭同意/拒绝/取消、等待时集合变化、最大化恢复 |
| 控件主题与资源切换 | `tests/GOZA.Dock.HeadlessTests/ThemeTests.cs` | 真实模板、资源恢复、无效颜色不部分更新、资源内嵌 include |
| 对象释放与重复生命周期 | `tests/GOZA.Dock.HeadlessTests/ManagedMemoryTests.cs` | 窗口卸载、活动拖动、缓存与视图模型释放、旧主题资源、重复创建关闭 |

普通持久化测试通过源码链接编译示例的实际服务、模型和 JSON 上下文，无需原生 WebView 或依赖注入环境；文件测试使用独立临时目录并清理。它不编译整个示例界面，因此改动示例调用方后仍需构建 Demo。

无头测试通过 `TestApplication.cs` 初始化 Avalonia，`TestWorkspace.cs` 创建内存窗口并加载库的真实 Dock 模板，通过鼠标事件验证行为。测试禁用并行执行，避免共享 UI 和拖动状态相互影响。优先验证公开行为及状态，不要用反射调用私有方法来代替用户操作。

修改公共 API、示例调用方或模板依赖后，可执行桌面示例构建：

```sh
dotnet build samples/GOZA.Dock.Minimal.Desktop/GOZA.Dock.Minimal.Desktop.csproj -c Release
dotnet build samples/GOZA.Dock.Demo.Desktop/GOZA.Dock.Demo.Desktop.csproj -c Release
```

## 内存回归测试的判断与注意事项

内存测试检查弱引用所指对象在释放后是否可回收，结合完整 GC、终结器等待与 UI 队列处理。覆盖窗口关闭后外部仍持有 Tab 集合、活动拖动中关闭、窗口仍开着时移除缓存视图和视图模型、禁用缓存、旧主题画刷，以及 100 次窗口和 Tab 创建/关闭循环。

测试也包含对照：Tab 仍存在时，未选中的缓存视图应保留；移除 Tab 后才应回收。这种正常缓存不能误报为泄漏。

维护这些测试时保留以下约束：

- 创建被跟踪对象的辅助方法使用 `NoInlining`，避免 JIT 局部变量存活造成假阳性。
- 检查视图释放前清理测试自身的 `Created` 强引用记录；不要清理测试要验证的外部持有关系。
- 窗口关闭探针使用真实卸载路径，不要先显式取消全局拖动，否则可能掩盖卸载时的清理缺陷。普通测试清理仍可使用 `TestWorkspace.Dispose`。
- 先处理 Dispatcher 队列和渲染工作，再进行 GC。Avalonia 的长按计时器可能短暂持有按下事件源，因此允许实际计时器到期，并在最多 5 秒内重试。不要用无限等待或任意延长超时掩盖对象一直存活的问题。
- 托管堆字节变化仅作为诊断输出，不能直接用“内存增加若干字节”判定泄漏；框架缓存、测试运行器分配和 GC 保留空间都会影响数字。

通过表示已覆盖场景中的托管对象符合释放预期，不能证明整个应用不存在泄漏。原生 WebView、媒体、GPU 资源和未覆盖的业务对象仍需要真实平台检查。

## 与测试有关的行为约定

Tab 可实现 `IDockTabCloseGuard`，用 `CanCloseAsync` 在移除前异步确认保存或取消；返回 false 保留 Tab。关闭按钮与 `DockRegion.CloseTabAsync` 使用相同流程，后者应在 UI 线程调用。等待期间会防止同一 Tab 重复关闭，批准后重新检查集合归属和可修改性；`TabClosedCommand` 在成功关闭后通知。

切换 `EnableViewCache` 会在新缓存模式下重建当前内容，选中的 Tab 仍应可见。需要保持同一原生视图实例时应保留缓存。禁用缓存应清理旧视图并配对生命周期通知。

切换 `ColorTheme` 应撤销前一主题自己的覆盖，并恢复它替换的宿主局部资源；设为 null 恢复默认或宿主覆盖。无关资源及 `TabStripSize` 等结构参数应保留；无效颜色输入应在替换资源之前报错。

## 失败排查与 AI 交接

先区分环境失败与测试断言失败。缺少 SDK、包源超时、构建目录权限不足、编译失败都不等于测试已运行。用户级私有 NuGet 源导致还原等待时，可以明确使用官方包源：

```sh
dotnet restore GOZA.Dock.Tests.slnx --source https://api.nuget.org/v3/index.json
```

不要为得到绿色结果自动关闭依赖漏洞审计、忽略异常、移除断言或跳过失败测试。依赖警告应与测试结果分开报告；还原失败时不能仅凭旧产物声称新代码通过。

CI 配置位于 `.github/workflows/ci.yml`：Windows 和 Linux 运行完整测试，随后构建桌面示例并打包库。本地成功不代表 CI 已成功，应分别报告。

后续 AI 完成测试后，应说明实际执行的命令、配置、通过/失败/跳过数量，失败原因和未验证范围。修复时先补能复现问题的回归用例，验证对应范围，再执行完整 Release 测试；仅修改本文或导航时无需重新运行代码测试。

真实平台继续人工验证：原生 WebView/媒体行为、系统窗口激活与鼠标捕获、触控/长按、实际显示效果和原生资源释放。不要将无头测试通过写成真实平台已验证。
