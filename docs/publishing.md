# NuGet 发布指南

发布工作流为 [publish-nuget.yml](../.github/workflows/publish-nuget.yml)，使用 NuGet Trusted Publishing（OIDC），不保存长期 API Key。

## 首次配置

在拥有 `GOZA.Dock` 发布权限的 NuGet 账户中，进入 **Trusted Publishing** 新建策略：

| 字段 | 内容 |
| --- | --- |
| Repository Owner | `0use-TE`（首字符为数字零） |
| Repository | `GOZA.Dock` |
| Workflow File | `publish-nuget.yml`，只填文件名 |
| Environment | 留空；当前工作流没有指定 Environment |
| 包范围 | `GOZA.Dock`，允许发布现有包的新版本 |

在 GitHub 仓库 **Settings → Secrets and variables → Actions → Variables** 添加仓库变量 `NUGET_USER`，值为 NuGet 个人资料用户名，不能填写邮箱。选定策略所有者必须拥有该包。

策略字段应与实际仓库、工作流和环境一致，文件名没有统一标准。规则与配置入口见 [NuGet 官方说明](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)。

## 每次发布

1. 修改 `src/GOZA.Dock/GOZA.Dock.csproj` 的 `Version` 和 `PackageReleaseNotes`。
2. 创建对应版本的中英文文档、发布说明，更新首页、导航和版本选择器。测试入口见 [测试指南](testing.md)。
3. 运行完整 Release 测试，构建桌面示例并打包，确认包元数据、依赖和内容正确。只上传本次生成的精确版本包。
4. 将完成的源码、测试、文档及发布工作流提交到 GitHub，再创建并推送对应版本标签，例如 `v3.0.9`。标签版本必须与项目版本一致。
5. 查看 **Publish NuGet** 工作流：Windows/Linux 测试均成功后才构建示例和打包，最后通过 OIDC 换取临时密钥并上传。
6. 等待 NuGet 索引，检查包页面及版本接口，确认 `GOZA.Dock` 的目标版本已可获取，再报告发布成功。

也可在 GitHub Actions 手动执行工作流并填写精确版本；手动入口需要工作流已存在于默认分支。运行所选分支的版本必须与输入一致。

推送 `v*` 标签还会触发现有的 **Release Desktop** 工作流，生成 Windows 桌面示例发布资产。只有准备正式发布时才推送版本标签。

## 失败处理与边界

- 工作流验证版本和 `NUGET_USER`，不匹配时停止。
- OIDC 登录失败时核对 NuGet 用户名、策略所有者、仓库及文件名和包范围；不能用本地 GitHub 登录代替 NuGet 信任策略。
- 发布不使用 `--skip-duplicate`；已存在版本应明确报错，不能把跳过旧包当成新包发布成功。NuGet 上同一版本不能覆盖重发。
- 不关闭漏洞审计，也不因依赖警告或上传失败自动升级版本重试。先诊断，再说明结果。
- 本地打包、上传工作流文件或取得临时密钥都不代表包已发布；以成功上传及 NuGet 可获取的版本为依据。
