English | [简体中文](README.zh-CN.md)

<p align="center">
  <img src="src/GOZA.Dock/wwwroot/GOZA.png" alt="GOZA.Dock" width="320" />
</p>

# GOZA.Dock

Diseño de acoplamiento (docking) ligero para [Avalonia](https://avaloniaui.net/) — componga paneles con `Grid`, `DockRegion` y `DockSplitter`. Funciona en escritorio y WebAssembly.

## Características

- **Diseño flexible** — cualquier topología de cuadrícula; sin cuadrantes fijos ni enums de ranuras.
- **Arrastrar y soltar pestañas** — reordene en la tira, mueva entre regiones, haga doble clic para maximizar una región.
- **Parking lot** — reutilización opcional de la superficie de vista mediante el `Id` de la pestaña (WebView, controles pesados).
- **Pestañas cerrables** — `IDockTabItem.IsClosable`; botón opcional "Add Doc" en una región.
- **Cromo de tira de pestañas** — ubicación predeterminada del shell, menú de ubicación **⋮** opcional, ranura de barra de herramientas final.
- **Selección automática de pestañas** — se selecciona la primera pestaña cuando `SelectedItem` no está definido (vínculo al restaurar el diseño).
- **Tiras de pestañas laterales** — encabezado completo rotado en tiras izquierda/derecha (activación global o por región).
- **Compatible con temas** — incluye `DockShellStyles.axaml`; sobrescriba los pinceles de arrastrar/soltar mediante `DockThemeResources`.
- **MIT** — sin dependencias de Semi, Crystal u otros stacks de UI (solo Avalonia).

## Inicio rápido

```bash
dotnet run --project samples/GOZA.Dock.Demo.Desktop
```

Demo completa (Crystal DI, guardado/carga de diseño, documentos cerrables, temas VS Code): `samples/GOZA.Dock.Demo.Desktop`

Instale el paquete (**Avalonia 12.0.0+** requerido en su aplicación):

```bash
dotnet add package GOZA.Dock --version 3.0.0
```

XAML mínimo:

```xml
<DockShell>
  <Grid ColumnDefinitions="*,8,*">
    <DockRegion Grid.Column="0"
                ItemsSource="{Binding LeftTabs}" />
    <DockSplitter Grid.Column="1" ShowsPreview="True" />
    <DockRegion Grid.Column="2"
                ItemsSource="{Binding RightTabs}" />
  </Grid>
</DockShell>
```

`DockRegion` selecciona automáticamente la primera pestaña cuando `SelectedItem` no está vinculado. Vincule `SelectedItem` para restaurar el diseño o para una selección explícita (Demo).

Incluya un tema de host solo si lo necesita. Cada `DockShell` carga `DockShellStyles` por sí mismo:

```xml
<Application.Styles>
  <FluentTheme />
</Application.Styles>
```

Temas de color: asigne `DockShell.ColorTheme` (por ejemplo `VsCodeThemeJson.LoadFromFile(...)`).

Los elementos de pestaña implementan `IDockTabItem` (`Id`, `Header`, opcionalmente `ReuseSurface`, `IsClosable`). Mapee cada ViewModel de pestaña a una vista con `DataTemplate` o su localizador de vistas/DI.

## Documentación y demos

| Recurso | URL |
|----------|-----|
| Documentación en línea | https://0use.net/GOZA.Dock/ |
| Demo en navegador (WASM) | https://0use.net/GOZA.Dock/demo/ |
| Notas de lanzamiento | [docs/3.0.0/release-notes.md](docs/3.0.0/release-notes.md) |
| Publicación NuGet (mantenedores) | [PUBLISHING.md](PUBLISHING.md) |

Construya la documentación localmente (requiere [DocFX](https://dotnet.github.io/docfx/)):

```bash
docfx docfx.json && docfx serve _site --port 8080
```

Hacer push a `master` activa [GitHub Pages](.github/workflows/docs.yml) (sitio de DocFX + demo WASM).

## Contribuciones

Los problemas (issues) y las solicitudes de extracción (pull requests) son bienvenidos. Notas para desarrolladores: [DEVELOPMENT.md](DEVELOPMENT.md).

## Licencia

MIT — vea [LICENSE.txt](LICENSE.txt).
