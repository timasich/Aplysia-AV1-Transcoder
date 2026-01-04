using System.Text.Json;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class SettingsService
{
    private readonly StorageService _storage;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService(StorageService storage)
    {
        _storage = storage;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public AppSettings Load()
    {
        var path = _storage.GetSettingsPath();
        if (!File.Exists(path))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_storage.DataFolder);
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_storage.GetSettingsPath(), json);
    }
}
