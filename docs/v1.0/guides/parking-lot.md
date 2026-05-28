# Parking Lot

For expensive surfaces (WebView, video, map controls), enable reuse so instances are parked off-screen instead of destroyed when tabs change.

## Enable

```xml
<DockShell EnableParkingLot="True">
  ...
</DockShell>
```

## Tab contract

```csharp
public bool ReuseSurface => true; // on IDockTabItem
public string Id { get; }         // stable cache key
```

## Factory (required for reuse)

Implement `IDockContentFactoryProvider` on a `DataContext` ancestor:

```csharp
public Control CreateContent(IDockTabItem tab) =>
    new MyWebViewPanel { DataContext = tab };
```

## Flow

```
Select tab  → DockViewHost.Activate → attach or create control → ContentHost
Deselect    → DockViewHost.Release  → detach → hidden Parking Lot panel
Reselect    → same instance
```

## Further reading

- [Architecture — content lifecycle](../architecture.md#content-lifecycle)
