namespace GOZA.Dock.Tests;

internal sealed class TemporaryDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"goza-tests-{Guid.NewGuid():N}");

    public TemporaryDirectory() => Directory.CreateDirectory(Path);

    public string Write(string relativePath, string contents)
    {
        var file = System.IO.Path.Combine(Path, relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
        File.WriteAllText(file, contents);
        return file;
    }

    public void Dispose()
    {
        var root = System.IO.Path.GetFullPath(Path);
        var temporaryRoot = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()).TrimEnd(System.IO.Path.DirectorySeparatorChar);
        if (System.IO.Path.GetDirectoryName(root) == temporaryRoot && Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
