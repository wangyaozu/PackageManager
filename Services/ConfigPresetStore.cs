using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PackageManager.Models;

namespace PackageManager.Services
{
    public static class ConfigPresetStore
    {
        private static string PresetsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "UserPresets.json");

        public static List<ConfigPreset> Load()
        {
            try
            {
                var path = PresetsFilePath;
                if (!File.Exists(path)) return new List<ConfigPreset>();
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<ConfigPreset>>(json) ?? new List<ConfigPreset>();
                return list;
            }
            catch
            {
                return new List<ConfigPreset>();
            }
        }

        public static void Save(IEnumerable<ConfigPreset> presets)
        {
            var list = presets == null ? new List<ConfigPreset>() : new List<ConfigPreset>(presets);
            var dir = Path.GetDirectoryName(PresetsFilePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(PresetsFilePath, json);
        }
    }
}