# DocFX 文档系统 — AI 工作提示词（GOZA.Dock）

> 复制给其他 AI，用于维护本仓库 DocFX 文档。  
> 官方文档：https://dotnet.github.io/docfx/

---

## 一、给 AI 的总提示词（可直接复制）

```
你在维护 GOZA.Dock 的 DocFX 文档时，必须遵循：

1. 版本切换：顶栏 Version 下拉按 **NuGet 包版本**（3.0.0、2.0.0、1.0.6…）切换，不是 v1.0 这种产品线名。
   - 配置：docfx/template/public/goza-versions.json
   - 脚本：docfx/template/public/goza-switcher.js
   - 每个包版本独立目录：docs/3.0.0/、docs/2.0.0/、docs/1.0.6/…（英文 + zh-CN 镜像）
   - 发新版 NuGet 时：复制上一版 docs 文件夹为新版本号，更新 goza-versions.json、首页 index.md、README 版本号
2. 语言切换：只用顶栏 Lang（English / 简体中文）。正文禁止行内语言链接。
3. 根 toc.yml：Home | Docs（指向最新 docs/3.0.0/）| API Reference — 不要为每个包版本各加一项。
4. GitHub Pages：globalMetadata._appBasePath: "/GOZA.Dock/"
5. template 顺序：["default", "modern", "docfx/template"]
6. 旧 URL：docs/v1.0/* 保留 redirect 到 docs/1.0.6/
7. API Reference：始终从当前源码生成，仅反映最新包；旧版本概念文档可链到 ../../api/
8. 改完 docfx docfx.json，0 error；本地 docfx serve _site --port 8080
9. goza-switcher.js 的 pages 列表与 goza-versions.json 同步（含 release-notes）
10. 各版本 release-notes 以该版本为主；可在顶部加一行指向更新版本的链接，但不展开写未来版正文
```

---

## 二、目录结构

```
docs/
├── 3.0.0/          # 最新包文档（英文）
│   ├── toc.yml
│   ├── getting-started.md
│   └── zh-CN/
├── 2.0.0/
├── 1.0.6/
└── v1.0/           # 旧链接 redirect → 1.0.6
```

---

## 三、发新版文档流程

1. `Copy-Item -Recurse docs/2.0.0 docs/3.0.0`（以当前最新为源）
2. 更新 `docs/3.0.0/` 内版本号、release-notes、getting-started、theming、migration
3. `goza-versions.json` 增加 `{ "id": "3.0.0", "label": "3.0.0 (latest)" }`，旧 latest 去掉 `(latest)`，并设 `default`
4. `toc.yml` topicHref → `docs/3.0.0/getting-started.md`
5. `index.md` 首页表格增加 3.0.0 并标 (latest)
6. `docfx docfx.json` 验证

---

*GOZA.Dock · package-version docs · 2026-09*
