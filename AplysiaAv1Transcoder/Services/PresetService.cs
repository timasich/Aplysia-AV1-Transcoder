using System;
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

            return NormalizePresets(presets);
        }
        catch
        {
            return GetDefaultPresets();
        }
    }

    public void SavePresets(IEnumerable<Preset> presets)
    {
        Directory.CreateDirectory(_storage.DataFolder);
        var json = JsonSerializer.Serialize(presets, _jsonOptions);
        File.WriteAllText(_storage.GetPresetsPath(), json);
    }

    public static List<Preset> GetDefaultPresets()
    {
        return new List<Preset>
        {
            new()
            {
                Name = "H264 Safe",
                TargetCodec = TargetCodec.H264,
                BitrateMode = BitrateMode.MultiplierFromSource,
                Multiplier = 2.0,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            },
            new()
            {
                Name = "H264 Balanced",
                TargetCodec = TargetCodec.H264,
                BitrateMode = BitrateMode.MultiplierFromSource,
                Multiplier = 1.6,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            },
            new()
            {
                Name = "H265 Safe",
                TargetCodec = TargetCodec.H265,
                BitrateMode = BitrateMode.MultiplierFromSource,
                Multiplier = 1.6,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            },
            new()
            {
                Name = "H265 Balanced",
                TargetCodec = TargetCodec.H265,
                BitrateMode = BitrateMode.MultiplierFromSource,
                Multiplier = 1.3,
                NvencPreset = "p5",
                PixelFormat = "yuv420p",
                AudioMode = AudioMode.Copy,
                ForceDav1d = true
            }
        };
    }

    private static List<Preset> NormalizePresets(List<Preset> presets)
    {
        foreach (var preset in presets)
        {
            if (preset.BitrateMode == BitrateMode.MultiplierFromSource)
            {
                if (preset.Multiplier <= 0)
                {
                    preset.Multiplier = 1.0;
                }
                preset.Multiplier = Math.Clamp(preset.Multiplier, 1.0, 3.0);
            }
        }

        return presets;
    }
}
