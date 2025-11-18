using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PackageManager.Services
{
    /// <summary>
    /// 负责读取/写入调试模式配置（config/DebugSetting.json）
    /// </summary>
    public static class DebugSettingsService
    {
        public static bool ReadIsDebugMode(string localPath, bool defaultValue = false)
        {
            try
            {
                if (string.IsNullOrEmpty(localPath)) return defaultValue;
                string debugSettingPath = Path.Combine(localPath, "config", "DebugSetting.json");
                if (!File.Exists(debugSettingPath)) return defaultValue;

                string json = File.ReadAllText(debugSettingPath);
                var match = Regex.Match(json, "\"IsDebugMode\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return string.Equals(match.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void WriteIsDebugMode(string localPath, bool enable)
        {
            try
            {
                if (string.IsNullOrEmpty(localPath)) return;

                string configDir = Path.Combine(localPath, "config");
                string debugSettingPath = Path.Combine(configDir, "DebugSetting.json");
                Directory.CreateDirectory(configDir);

                string newValue = enable ? "true" : "false";
                string content = File.Exists(debugSettingPath) ? File.ReadAllText(debugSettingPath) : "{\n  \"IsDebugMode\": false\n}";

                if (Regex.IsMatch(content, "\"IsDebugMode\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase))
                {
                    content = Regex.Replace(content, "(\"IsDebugMode\"\\s*:\\s*)(true|false)", $"$1{newValue}", RegexOptions.IgnoreCase);
                }
                else
                {
                    content = "{\n  \"IsDebugMode\": " + newValue + "\n}";
                }

                File.WriteAllText(debugSettingPath, content);
            }
            catch
            {
                // 忽略写入异常
            }
        }
    }
}