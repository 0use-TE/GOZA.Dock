# DocFX 文档系统 — AI 工作提示词（GOZA.Dock）

> 复制给其他 AI，用于维护本仓库 DocFX 文档。  
> 官方文档：https://dotnet.github.io/docfx/

---

## 一、给 AI 的总提示词（可直接复制）

```
你在维护 GOZA.Dock 的 DocFX 文档时，必须遵循：

1. 版本切换：只用顶栏 navbar 的 Version 下拉（docfx/template + goza-switcher.js）。根 toc.yml 仅保留 Home / v1.0 / API，不要加「Documentation (English)」「文档 (简体中文)」等第二套语言入口。
2. 语言切换：只用顶栏 navbar 的 Lang 下拉（English / 简体中文）。正文里不要写「English · 简体中文」行内链接，不要语言对照表。
3. 双语结构：docs/v1.0/（英文）与 docs/v1.0/zh-CN/（中文）镜像；文件名一一对应（getting-started.md、crystal-avalonia.md 等）。
4. GitHub Pages 项目站必须设置 globalMetadata._appBasePath: "/GOZA.Dock/"（线上根 URL：https://0use.net/GOZA.Dock/）。
5. docfx.json 的 template 顺序：["default", "modern", "docfx/template"]；自定义顶栏在 docfx/template/layout/_master.tmpl，切换脚本在 docfx/template/public/goza-switcher.js。
6. 旧链接兼容：保留 docs/v1.0/introduction.md（及 zh-CN 版），front matter redirect_url: getting-started.html。
7. 文档风格：简洁、可复制粘贴；Quick Start 用 CommunityToolkit.Mvvm 示例（左右两区域）；完整五区域见 crystal-avalonia.md 或 samples。
8. MVVM / 序列化：在 Quick Start 用 blockquote 说明「用户自选」——库不绑定 MVVM 框架、不内置布局持久化；Demo 仅作 JSON 示例。
9. 侧边栏 toc：每语言各一份 toc.yml，只放文档章节，不放版本/语言切换。
10. api/ 与 _site/ 不提交 Git；改完运行 docfx docfx.json，0 error；本地 docfx serve _site --port 8080。
11. 禁止在根 toc 用 dropdown 父节点 + href 同时存在；禁止 docs/toc.yml 重复顶栏职责。
12. 跨语言同页切换由 goza-switcher.js 按当前 html 文件名映射（getting-started、crystal-avalonia、architecture、aot-compatibility、recipes、introduction、index）。
```

---

## 二、本仓库目录结构

```
仓库根/
├── docfx.json
├── toc.yml                 # 顶栏：Home | v1.0 | API Reference
├── index.md                # 首页 landing
├── docfx/template/         # 自定义顶栏 Version/Lang
│   ├── layout/_master.tmpl
│   └── public/goza-switcher.{js,css}
├── docs/
│   └── v1.0/
│       ├── toc.yml         # 英文侧边栏
│       ├── getting-started.md
│       ├── crystal-avalonia.md
│       ├── architecture.md
│       ├── aot-compatibility.md
│       ├── recipes.md
│       ├── introduction.md # → redirect getting-started
│       └── zh-CN/          # 中文镜像 + toc.yml
├── api/                    # 构建产物，.gitignore
└── _site/                  # 输出，.gitignore
```

**没有** `docs/toc.yml`。版本只在顶栏 `v1.0` 节点 + Version 下拉（当前仅 v1.0）。

---

## 三、docfx.json 要点

```json
{
  "build": {
    "template": ["default", "modern", "docfx/template"],
    "globalMetadata": {
      "_appBasePath": "/GOZA.Dock/",
      "_appName": "GOZA.Dock",
      "_enableSearch": true,
      "_gitContribute": {
        "repo": "https://github.com/0use-TE/GOZA.Dock",
        "branch": "master"
      }
    },
    "content": [
      { "files": ["index.md", "toc.yml"] },
      { "files": ["**/*.{md,yml}"], "src": "docs", "dest": "docs" },
      { "files": ["**/*.yml"], "src": "api", "dest": "api" }
    ]
  }
}
```

| 字段 | 说明 |
|------|------|
| `_appBasePath` | GitHub Pages 项目站子路径，缺则资源 404 |
| `dest: docs` | 避免多个 toc.yml 覆盖同一 toc.html |
| `docfx/template` | 覆盖 modern 主布局，注入 Version/Lang |

---

## 四、根 toc.yml（勿冗余）

```yaml
- name: Home
  href: index.md
- name: v1.0
  href: docs/v1.0/
  topicHref: docs/v1.0/getting-started.md
- name: API Reference
  href: api/
```

❌ 不要写：

```yaml
- name: Documentation (English)
  href: docs/v1.0/
- name: 文档 (简体中文)
  href: docs/v1.0/zh-CN/
```

语言由顶栏 **Lang** 下拉处理。

---

## 五、顶栏 Version / Lang 实现

| 文件 | 作用 |
|------|------|
| `docfx/template/layout/_master.tmpl` | navbar 内 `<select id="goza-version">`、`<select id="goza-lang">` |
| `docfx/template/public/goza-switcher.js` | 读当前 URL，切换 `docs/v1.0/` ↔ `docs/v1.0/zh-CN/` 同页 |
| `docfx/template/public/goza-switcher.css` | 下拉框布局 |

新增文档页时：英文 + 中文各建同名 `.md`，并加入 `goza-switcher.js` 的 `docPages` 集合。

---

## 六、文档页规范

### 6.1 禁止冗余

- ❌ 正文标题下 `English · 简体中文`
- ❌ 首页 / index 语言对照表（已有顶栏 Lang）
- ❌ 侧边栏 toc 语言分组
- ❌ 根 toc 双语并列入口

### 6.2 内容原则

| 页面 | 写什么 |
|------|--------|
| getting-started.md | 可运行完整代码；CommunityToolkit.Mvvm；左右两区域；公开 API 表 |
| crystal-avalonia.md | 完整 MainView.axaml（五区域）+ Crystal DI |
| architecture.md | 视觉树 + 公开 API + 协调器 |
| aot-compatibility.md | StyleInclude + publish 命令 |
| recipes.md | 按需代码块；JSON 说明「序列化自选」 |

### 6.3 重定向页

```markdown
---
redirect_url: getting-started.html
---

# Introduction

Moved to Quick Start.
```

用于 `introduction.md` 兼容旧 URL。

---

## 七、部署

| 项目 | 值 |
|------|-----|
| 仓库 | https://github.com/0use-TE/GOZA.Dock |
| CI | `.github/workflows/docs.yml` — push master → GitHub Pages |
| 线上根 URL | https://0use.net/GOZA.Dock/ |
| 文档入口 | /docs/v1.0/getting-started.html |
| 本地预览 | `docfx docfx.json && docfx serve _site --port 8080` |

NuGet 手动上传；Release Desktop zip 见 `release-desktop.yml`。

---

## 八、AI 改文档检查清单

```bash
docfx docfx.json
docfx serve _site --port 8080
```

1. 0 error  
2. 顶栏只有 **Version** + **Lang**，无重复语言链接  
3. 正文无 `English · 简体中文`  
4. `_site/docs/v1.0/introduction.html` 存在且跳转  
5. Lang 切换后 URL 在 `docs/v1.0/` 与 `docs/v1.0/zh-CN/` 间正确  
6. 中英文新增页同步更新两边 toc.yml  

---

## 九、精简版（短任务）

```
GOZA.Dock DocFX：顶栏 Version/Lang 唯一入口；根 toc 仅 Home/v1.0/API；
docs/v1.0 + zh-CN 镜像；_appBasePath=/GOZA.Dock/；正文禁止语言行内链接；
改 docfx/template 时同步 goza-switcher.js 的 docPages；docfx docfx.json 验证。
```

---

*GOZA.Dock v1.0 · 2026-05*
