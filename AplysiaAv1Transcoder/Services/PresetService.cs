using System.Text.Json;
using AplysiaAv1Transcoder.Models;

namespace AplysiaAv1Transcoder.Services;

public sealed class PresetService
{
    private readonly StorageService _storage;
    private readonly JsonSerializerOptions _jsonOptions;

    public PresetService(StorageService storage)
    {
        _storage = storage;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public List<Preset> LoadPresets()
    {
        var path = _storage.GetPresetsPath();
        if (!File.Exists(path))
        {
            return GetDefaultPresets();
        }

        try
        {
            var json = File.ReadAllText(path);
            var presets = JsonSerializer.Deserialize<List<Preset>>(json, _jsonOptions);
            if (presets == null || presets.Count == 0)
            {
                return GetDefaultPresets();
            }

            return presets;
        }
        catch
        {
            return GetDefaultPresets();
        }
    }

    public void SavePresets(IEnumerable<Preset> presets)
    {
        var toSave = presets.Where(p => !p.IsBuiltInAuto).ToList();
        Directory.CreateDirectory(_storage.DataFolder);
        var json = JsonSerializer.Serialize(toSave, _jsonOptions);
        File.WriteAllText(_storage.GetPresetsPath(), json);
    }

    public static List<Preset> GetDefaultPresets()
    {
        return new List<Preset>
        {
            new()
            {
                Name = "H264 8 Mbps",
                TargetCodec = TargetCodec.H264,
                EncoderPriority = EncoderPriority.AutoHW,
                BitrateKbps = 8000,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            },
            new()
            {
                Name = "H265 6 Mbps",
                TargetCodec = TargetCodec.H265,
                EncoderPriority = EncoderPriority.AutoHW,
                BitrateKbps = 6000,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            }
        };
    }

    public static List<Preset> GetBuiltInAutoPresets()
    {
        return new List<Preset>
        {
            new()
            {
                Name = "Auto H264 (match)",
                TargetCodec = TargetCodec.H264,
                EncoderPriority = EncoderPriority.AutoHW,
                BitrateKbps = 0,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true,
                IsBuiltInAuto = true
            },
            new()
            {
                Name = "Auto H265 (match)",
                TargetCodec = TargetCodec.H265,
                EncoderPriority = EncoderPriority.AutoHW,
                BitrateKbps = 0,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true,
                IsBuiltInAuto = true
            }
        };
    }
}
