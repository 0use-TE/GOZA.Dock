# Publishing

Guide for maintainers: GitHub Pages docs and NuGet.org package.

## GitHub Pages (docs + WASM demo)

Documentation deploys automatically when you push to **`master`** (or **`main`**).

Workflow: [`.github/workflows/docs.yml`](.github/workflows/docs.yml)

1. Builds DocFX site from `docs/` and API XML comments.
2. Publishes `samples/GOZA.Dock.Demo.Browser` into `_site/demo/`.
3. Deploys to GitHub Pages at `https://0use.net/GOZA.Dock/` (custom domain configured in repo settings).

Local preview:

```bash
docfx docfx.json
docfx serve _site --port 8080
```

Ensure **Settings → Pages → Source** is **GitHub Actions** (not legacy branch deploy).

## NuGet package

Package project: `src/GOZA.Dock/GOZA.Dock.csproj`

| Metadata | Value |
|----------|--------|
| Package ID | `GOZA.Dock` |
| Icon | `wwwroot/package-icon.png` → packed to package root (`PackageIcon`) |
| Readme | Root `README.md` (`PackageReadmeFile`) |
| License | MIT (`PackageLicenseExpression`) |

Icon file must stay **under 1 MB** (use `package-icon.png`, not full `GOZA.png`).

### Build the package

```bash
dotnet pack src/GOZA.Dock/GOZA.Dock.csproj -c Release -o ./artifacts
```

Verify contents:

```bash
# PowerShell — list nupkg entries
Add-Type -AssemblyName System.IO.Compression.FileSystem
$z = [System.IO.Compression.ZipFile]::OpenRead("./artifacts/GOZA.Dock.*.nupkg")
$z.Entries | ForEach-Object FullName
$z.Dispose()
```

Expect at least: `README.md`, `package-icon.png`, `lib/net10.0/GOZA.Dock.dll`, `lib/net10.0/GOZA.Dock.xml`.

### Publish to nuget.org

One-time: create an API key at https://www.nuget.org/account/apikeys (scope: `GOZA.Dock`, push).

```bash
dotnet nuget push ./artifacts/GOZA.Dock.1.0.2.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Or with environment variable (no key in shell history):

```bash
$env:NUGET_API_KEY = "..."
dotnet nuget push ./artifacts/GOZA.Dock.1.0.2.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

### Before each release

1. Update `<Version>` and `<PackageReleaseNotes>` in `GOZA.Dock.csproj`.
2. Copy `docs/1.0.2/` to `docs/{newVersion}/` (or edit in place for patch), update release notes (EN + zh-CN).
3. Add the new version to `docfx/template/public/goza-versions.json` and set it as `default`.
4. Update `toc.yml` `topicHref`, root `index.md`, and README version strings.
5. `dotnet build GOZA.Dock.slnx -c Release`
6. `dotnet pack` and smoke-test in a sample app.
7. Push to `master` (docs site updates via GitHub Actions).
8. `dotnet nuget push` the new `.nupkg`.

Release notes for the package summary field should stay short; link to the full markdown in the repo.
