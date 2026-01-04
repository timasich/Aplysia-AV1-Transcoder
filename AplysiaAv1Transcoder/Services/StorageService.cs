namespace AplysiaAv1Transcoder.Services;

public sealed class StorageService
{
    public string AppFolder { get; }
    public string DataFolder { get; }

    public StorageService()
    {
        AppFolder = Path.GetFullPath(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        DataFolder = ResolveWritableFolder();
    }

    public bool IsPortable => string.Equals(AppFolder, DataFolder, StringComparison.OrdinalIgnoreCase);

    public string GetSettingsPath() => Path.Combine(DataFolder, "settings.json");

    public string GetPresetsPath() => Path.Combine(DataFolder, "presets.json");

    public string ResolvePath(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(storedPath))
        {
            return storedPath;
        }

        return Path.GetFullPath(Path.Combine(AppFolder, storedPath));
    }

    public string MakeRelativeIfInAppFolder(string fullPath)
    {
        var normalized = Path.GetFullPath(fullPath);
        if (!normalized.StartsWith(AppFolder, StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        return Path.GetRelativePath(AppFolder, normalized);
    }

    public bool CanWriteTo(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            var testFile = Path.Combine(folder, ".write-test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ResolveWritableFolder()
    {
        if (CanWriteTo(AppFolder))
        {
            return AppFolder;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var fallback = Path.Combine(localAppData, "AplysiaAv1Transcoder");
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}
