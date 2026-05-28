# DocFX 文档系统 — AI 工作提示词

> 本文档供复制给其他 AI，用于在本仓库或类似 .NET 项目中正确配置、维护 DocFX 文档。
> 官方文档：https://dotnet.github.io/docfx/

---

## 一、给 AI 的总提示词（可直接复制）

```
你在配置 DocFX 文档时，必须遵循以下规则：

1. 严格采用 DocFX 官方推荐的目录结构，不要自创扁平结构或把多个版本混在一个 toc 里。
2. docfx.json 位于仓库根目录；所有路径相对 docfx.json。
3. 根 toc.yml 做顶部导航（Home / Documentation 版本下拉 / API）；Documentation 用 dropdown: true，子项 href 带末尾 /。
4. 版本切换放在根 toc.yml 的下拉框里，不要用 docs/toc.yml（避免顶栏与侧边栏重复）。
5. docs/ 和 api/ 必须各自有独立的 dest 输出目录，避免多个 toc.yml 生成到同一个 toc.html 互相覆盖。
6. api/ 是 docfx metadata 构建产物，不要提交 Git；CI 每次 docfx docfx.json 会重新生成。
7. _site/ 是最终站点输出，不要提交 Git。
8. dropdown 父节点只能有 dropdown: true + items，不能同时写 href（会 /undefined）。
9. 跨版本链接在 markdown 里用 ~/docs/v2.x/... 绝对路径，不要用 ../v2.x/...（在 tutorials 子目录会解析错误）。
10. 根目录 index.md 与 docs/index.md 不能同时存在（都会生成 index.html，并行构建会文件锁冲突）。
11. 分版本文档用 docs/v2.x/ 文件夹隔离，每版有自己的 toc.yml 作为侧边栏。
12. API Reference 只在根 toc.yml 出现一次，不要嵌进某个版本的 docs toc 里。
13. 改完配置后必须运行 docfx docfx.json 验证 0 error，并检查 _site/toc.json 里 dropdown 子项都有有效 href。
14. 本地预览：docfx serve _site --port 8080；构建前若 _site 被占用，先关浏览器 tab 或删 _site 再构建。
15. 不要用 DocFX groups 做多版本（易出 JSON/链接问题）；用文件夹分版本 + 顶栏 dropdown 是更稳妥的方案。
16. 实现原理集中写在 docs/v2.0/architecture.md；各教程末尾用「Further Reading」链接，不要重复大段原理。
```

---

## 二、DocFX 是什么、怎么工作

DocFX 分两个阶段，一条命令 `docfx docfx.json` 会依次执行：

| 阶段 | 作用 | 输入 | 输出 |
|------|------|------|------|
| **metadata** | 从 C# 项目提取 API | `*.csproj` | `api/*.yml` |
| **build** | 把 markdown + API yml 编译成静态站 | `docs/**/*.md`, `toc.yml`, `api/*.yml` | `_site/` |

常用命令：

```bash
dotnet tool update -g docfx   # 安装/更新
docfx docfx.json              # 生成 api + 构建 _site
docfx serve _site --port 8080   # 本地预览
docfx metadata docfx.json     # 仅生成 api（可选）
docfx build docfx.json        # 仅构建（需 api 已存在）
```

---

## 三、官方推荐目录结构（本仓库实际采用）

```
仓库根/
├── docfx.json              # 唯一配置文件
├── toc.yml                 # 顶部导航栏（含版本下拉）
├── index.md                # 首页（landing）
├── docs/
│   ├── v2.0/               # 当前版文档
│   │   ├── toc.yml         # 该版侧边栏
│   │   ├── introduction.md
│   │   ├── architecture.md # 实现原理（集中一处）
│   │   ├── getting-started.md
│   │   └── tutorials/
│   └── v1.2/               # 旧版文档（Legacy）
│       ├── toc.yml
│       └── ...
├── api/                    # 构建产物，.gitignore，CI 生成
│   └── toc.yml
└── _site/                  # 最终站点，.gitignore
```

**注意：没有 `docs/toc.yml`**。版本选择在根 `toc.yml` 的下拉框完成。

**绝对不要**把 v1.2 和 v2.0 的内容写在同一个 toc 里混在一起。

---

## 四、docfx.json 配置详解

### 4.1 本仓库标准配置

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/docfx/main/schemas/docfx.schema.json",
  "metadata": [
    {
      "src": [{ "src": "./YourProject", "files": ["**/*.csproj"] }],
      "dest": "api"
    }
  ],
  "build": {
    "content": [
      { "files": ["index.md", "toc.yml"] },
      { "files": ["**/*.{md,yml}"], "src": "docs", "dest": "docs" },
      { "files": ["**/*.yml"], "src": "api", "dest": "api" }
    ],
    "resource": [{ "files": ["images/**"] }],
    "output": "_site",
    "template": ["default", "modern"],
    "globalMetadata": {
      "_appName": "项目名",
      "_appTitle": "项目名 Documentation",
      "_enableSearch": true,
      "pdf": true,
      "_appFooter": "© 2024"
    }
  }
}
```

### 4.2 关键字段说明

| 字段 | 说明 |
|------|------|
| `metadata.dest` | API yaml 输出目录，通常 `api` |
| `content[].src` | 源文件夹 |
| `content[].dest` | **必须设置**，决定 _site 下的子路径；不设则文件 flatten 到根目录，链接全乱 |
| `content[].files` | glob 匹配规则 |
| `output` | 站点输出目录，通常 `_site` |
| `template` | `default` + `modern` 为现代 UI（dropdown 需 DocFX ≥ 2.61.2） |
| `globalMetadata` | 全站元数据，见下文自定义节 |

### 4.3 content 三段式的原因

```json
{ "files": ["index.md", "toc.yml"] }                           // 根首页 + 顶栏
{ "files": ["**/*.{md,yml}"], "src": "docs", "dest": "docs" }   // 概念文档
{ "files": ["**/*.yml"], "src": "api", "dest": "api" }          // API 参考
```

这样生成：
- `_site/toc.html` ← 根 toc.yml（含版本下拉）
- `_site/docs/v2.0/toc.html` ← 各版本侧边栏
- `_site/api/toc.html` ← api/toc.yml

**如果不设 dest**，多个 toc.yml 都会写到 `_site/toc.html`，互相覆盖，导航行为不可预测。

---

## 五、toc.yml 用法（官方 + 本仓库）

官方文档：https://dotnet.github.io/docfx/docs/table-of-contents.html

### 5.1 顶部导航 + 版本下拉 — 根 toc.yml（本仓库采用）

```yaml
- name: Home
  href: index.md
- name: Documentation
  dropdown: true              # 父级不写 href！
  items:
    - name: v2.0 (Current)
      href: docs/v2.0/        # 末尾 / 必填
    - name: v1.2 (Legacy)
      href: docs/v1.2/
- name: API Reference
  href: api/
```

规则：
- `dropdown: true` 让 modern 模板在顶栏渲染下拉框（DocFX ≥ 2.61.2）
- **dropdown 父节点不能有 `href`**，否则跳 `/undefined`
- 子项 `href: docs/v2.0/` 带斜杠 = Reference TOC，进入该版本 toc 的第一篇
- API 通常只有当前版一份，不放版本下拉里

构建后 `_site/toc.html` 应出现：

```html
<li class="dropdown">
  <a class="dropdown-toggle" ...>Documentation <span class="caret"></span></a>
  <ul class="dropdown-menu">
    <li><a href="docs/v2.0/introduction.html">v2.0 (Current)</a></li>
    <li><a href="docs/v1.2/introduction.html">v1.2 (Legacy)</a></li>
  </ul>
</li>
```

### 5.2 版本内侧边栏 — docs/v2.0/toc.yml

```yaml
- name: Introduction
  href: introduction.md
- name: Upgrade from v1.2
  href: upgrade-from-1.2.md
- name: Getting Started
  href: getting-started.md
- name: Architecture
  href: architecture.md
- name: Tutorials
  items:
    - name: MVVM Pattern
      href: tutorials/mvvm-pattern.md
- name: AOT Compatibility
  href: aot-compatibility.md
```

规则：
- href 相对当前 toc.yml 所在目录
- 父节点可以只有 `items` 没有 href（纯分组，仅限侧边栏内）
- 侧边栏**不要**再放版本切换（已在顶栏 dropdown）

### 5.3 旧写法（已废弃，勿用）

```yaml
# ❌ docs/toc.yml 做版本切换 — 与顶栏 dropdown 重复，已删除
# ❌ 根 toc 用 items 嵌版本但无 dropdown: true — 会 /undefined
# ❌ 根 toc href: docs/ 单链接 — 无法选版本
```

### 5.4 Nested TOC vs Reference TOC

| 写法 | 效果 | 适用 |
|------|------|------|
| `dropdown: true` + `items` | 顶栏下拉菜单 | **版本选择（推荐）** |
| `href: api/` | 引用，独立侧边栏 | 顶栏 API 入口 |
| `href: api/toc.yml` | 嵌套 API 进文档侧边栏 | 一般不建议 |

---

## 六、分版本文档方案

### 6.1 推荐方案：文件夹分版本 + 顶栏 dropdown

```
docs/
├── v2.0/             # 当前
└── v1.2/             # Legacy

根 toc.yml:
  Documentation ▼
    ├── v2.0 (Current)  → docs/v2.0/
    └── v1.2 (Legacy)   → docs/v1.2/
```

优点：简单、CI 友好、UX 清晰、AI 不易搞乱。

### 6.2 不推荐的方案

| 方案 | 问题 |
|------|------|
| DocFX `groups` 多版本 | 配置复杂，toc 交叉引用易报错 |
| 同一 toc 里用注释区分版本 | 用户和 AI 都会搞混 |
| docs/toc.yml + 根 toc 双入口切换版本 | 重复、混乱 |
| dropdown 父节点带 `href` | modern 模板跳 `/undefined` |
| 所有版本共用一个 getting-started.md | 内容无法真正分离 |

### 6.3 跨版本链接写法

在 `docs/v1.2/tutorials/xxx.md` 里指向 v2.0：

```markdown
> For v2.0+, see [v2.0 MVVM Pattern](~/docs/v2.0/tutorials/mvvm-pattern.md).
```

**错误写法**（DocFX 会解析成 `docs/v1.2/v2.0/...`）：

```markdown
[link](../v2.0/tutorials/mvvm-pattern.md)
```

统一用 `~/docs/v2.0/...` 最安全。

### 6.4 部署后的 URL 规律

设 GitHub Pages 根为 `https://example.com/ProjectName/`，则：

| 页面 | URL |
|------|-----|
| 首页 | `/index.html` |
| v2.0 介绍 | `/docs/v2.0/introduction.html` |
| v2.0 架构 | `/docs/v2.0/architecture.html` |
| v1.2 介绍 | `/docs/v1.2/introduction.html` |
| API | `/api/Crystal.Avalonia.html` |

README 外链必须带 `docs/` 前缀（因为 dest: docs）。

---

## 七、文档内容组织

### 7.1 教程 vs 实现原理

| 文件 | 写什么 |
|------|--------|
| `docs/v2.0/tutorials/*.md` | 怎么用（简洁） |
| `docs/v2.0/architecture.md` | 内部怎么实现（集中一处） |

各教程末尾加短链接即可：

```markdown
## Further Reading

> **How it works:** [Architecture — MVVM Wiring](../architecture.md#mvvm-wiring)
```

不要在每个教程里重复大段实现细节。

### 7.2 globalMetadata 常用项

```json
"globalMetadata": {
  "_appName": "显示在浏览器标题",
  "_appTitle": "完整标题",
  "_appFooter": "页脚文字",
  "_enableSearch": true,
  "pdf": true,
  "_disableNavbar": false,
  "_disableBreadcrumb": false,
  "_gitContribute": {
    "repo": "https://github.com/org/repo",
    "branch": "master"
  }
}
```

### 7.3 首页 landing 布局

根 `index.md` 顶部加 front matter：

```markdown
---
_layout: landing
---

# 项目名

| Version | Link |
|---------|------|
| **v2.0 (Current)** | [Documentation](docs/v2.0/introduction.md) |
| **v1.2 (Legacy)** | [Documentation](docs/v1.2/introduction.md) |
```

### 7.4 模板与资源

```json
"template": ["default", "modern"],
"resource": [{ "files": ["images/**"] }]
```

---

## 八、Git 与 CI

### 8.1 .gitignore 必须包含

```gitignore
_site/
api/
```

### 8.2 GitHub Actions 最小流程

```yaml
- run: dotnet tool update -g docfx
- run: docfx docfx.json
- uses: actions/upload-pages-artifact@v3
  with:
    path: '_site'
- uses: actions/deploy-pages@v4
```

不需要在仓库里预存 `api/` 或 `_site/`。

---

## 九、常见问题与排查

### 9.1 点击导航跳到 `/undefined`

**原因 A**：dropdown 父节点写了 `href` 和 `dropdown: true` 同时存在。

**修复**：

```yaml
# ✅ 正确
- name: Documentation
  dropdown: true
  items: ...

# ❌ 错误
- name: Documentation
  href: docs/
  dropdown: true
  items: ...
```

**原因 B**：普通顶栏节点只有 `items` 没有 `href` 也没有 `dropdown: true`。

### 9.2 DuplicateOutputFiles: toc.yml 冲突

**修复**：content 里给 docs 和 api 加 `"dest": "docs"` / `"dest": "api"`；不要创建多余的 `docs/toc.yml`。

### 9.3 构建报 index.html 文件被占用

**修复**：只保留根 index.md；关 serve / 浏览器；删 `_site` 重建。

### 9.4 InvalidFileLink 跨版本链接

**修复**：改用 `~/docs/v2.0/xxx.md` 绝对路径。

### 9.5 顶栏没有下拉框

**检查**：
- DocFX 版本 ≥ 2.61.2（本仓库 2.78.5）
- `template` 含 `modern`
- 根 toc.yml 有 `dropdown: true`

### 9.6 构建后验证清单

```bash
docfx docfx.json
# 检查：
# 1. 0 error
# 2. _site/docs/v2.0/introduction.html 存在
# 3. _site/docs/v1.2/introduction.html 存在
# 4. _site/api/Crystal.Avalonia.html 存在
# 5. _site/toc.json 里 Documentation 有 "dropdown":true 且子项 href 有效
# 6. _site/toc.html 有 class="dropdown" 和下拉菜单项
docfx serve _site --port 8080
# 7. 顶栏 Documentation ▼ 可切换 v2.0 / v1.2
# 8. 侧边栏只显示当前版本的 toc，无重复版本切换
```

---

## 十、AI 修改文档时的操作规范

### 10.1 新增 v2.0 文档页

1. 在 `docs/v2.0/` 下创建 `.md` 文件
2. 在 `docs/v2.0/toc.yml` 添加条目
3. **不要**改 v1.2 的文件（除非同步 Legacy 说明）
4. 若涉及实现原理，更新 `architecture.md`，教程只加 Further Reading 链接
5. 运行 `docfx docfx.json` 验证

### 10.2 新增版本（如 v2.1）

1. 复制 `docs/v2.0/` → `docs/v2.1/`
2. 更新根 `toc.yml` dropdown 添加 v2.1 入口
3. 更新根 `index.md` 版本表
4. 旧版 v2.0 改为 Legacy（可选）
5. 不要删除旧版文件夹

### 10.3 修改 API 文档

API 页面来自 C# XML 注释。改 `///` 注释后重新 `docfx docfx.json`。

### 10.4 禁止事项

- ❌ 不要把 `api/` 提交 Git
- ❌ 不要创建 `docs/toc.yml`（版本切换已在根 toc dropdown）
- ❌ 不要给 dropdown 父节点加 `href`
- ❌ 不要创建第二个 `docs/index.md`
- ❌ 不要用 DocFX groups 除非非常熟悉
- ❌ 不要把 v1.2 专有 API（如 AddMvvmHybrid）写进 v2.0 文档
- ❌ 不要 flatten docs 输出（去掉 dest）

---

## 十一、本仓库（Crystal.Avalonia）特定说明

| 项目 | 值 |
|------|-----|
| NuGet / 模板版本 | **2.0.0** |
| 当前文档 | **v2.0** — ViewModel-only DI，`CreateShellFromDi`，无 `AddMvvmHybrid` |
| Legacy 文档 | **v1.2** — View+ViewModel 都在 DI，有 `AddMvvmHybrid` |
| 升级指南 | `docs/v2.0/upgrade-from-1.2.md` |
| 实现原理 | `docs/v2.0/architecture.md` |
| C# 项目路径 | `Crystal.Avalonia/Crystal.Avalonia.csproj` |
| 部署 | GitHub Pages，push 到 `master` 触发 |
| 线上根 URL | `https://0use.net/Crystal.Avalonia/` |

---

## 十二、精简版提示词（短任务用）

```
配置 DocFX：根 toc.yml 用 Documentation dropdown（dropdown:true，父级无 href）切换 docs/v2.0/ 与 docs/v1.2/；
不要 docs/toc.yml；每版本独立 docs/vX/toc.yml 做侧边栏；docfx.json 里 docs 和 api 必须设 dest；
api/ 和 _site/ 不进 Git；跨版本链接用 ~/docs/v2.0/...；原理写 architecture.md，教程末尾链过去；
改完 docfx docfx.json，验证 _site/toc.html 有 dropdown 菜单。
```

---

*更新自 Crystal.Avalonia 项目 DocFX 配置实践，2026-05（v2.0 + 顶栏版本下拉）。*
