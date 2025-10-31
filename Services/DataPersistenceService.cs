using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PackageManager.Models;

namespace PackageManager.Services
{
    /// <summary>
    /// 数据持久化服务，用于保存和加载应用程序查询结果
    /// </summary>
    public class DataPersistenceService
    {
        private readonly string _dataFilePath;
        private readonly JsonSerializerSettings _jsonSettings;

        public DataPersistenceService()
        {
            // 数据文件保存在应用程序数据目录
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "PackageManager");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _dataFilePath = Path.Combine(appFolder, "application_cache.json");
            
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// 保存应用程序查询结果到文件
        /// </summary>
        /// <param name="applicationData">应用程序数据字典，键为程序名称，值为版本列表</param>
        /// <returns>是否保存成功</returns>
        public bool SaveApplicationData(Dictionary<string, List<ApplicationVersion>> applicationData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(applicationData, _jsonSettings);
                File.WriteAllText(_dataFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                // 记录错误（这里简单忽略，实际应用中应该记录日志）
                System.Diagnostics.Debug.WriteLine($"保存数据失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从文件加载应用程序查询结果
        /// </summary>
        /// <returns>应用程序数据字典，如果加载失败则返回空字典</returns>
        public Dictionary<string, List<ApplicationVersion>> LoadApplicationData()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new Dictionary<string, List<ApplicationVersion>>();
                }

                var json = File.ReadAllText(_dataFilePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, List<ApplicationVersion>>>(json, _jsonSettings);
                return data ?? new Dictionary<string, List<ApplicationVersion>>();
            }
            catch (Exception ex)
            {
                // 记录错误（这里简单忽略，实际应用中应该记录日志）
                System.Diagnostics.Debug.WriteLine($"加载数据失败: {ex.Message}");
                return new Dictionary<string, List<ApplicationVersion>>();
            }
        }

        /// <summary>
        /// 检查指定程序是否已有缓存数据
        /// </summary>
        /// <param name="programName">程序名称</param>
        /// <returns>是否存在缓存数据</returns>
        public bool HasCachedData(string programName)
        {
            var data = LoadApplicationData();
            return data.ContainsKey(programName) && data[programName].Count > 0;
        }

        /// <summary>
        /// 获取指定程序的缓存数据
        /// </summary>
        /// <param name="programName">程序名称</param>
        /// <returns>缓存的版本列表，如果不存在则返回空列表</returns>
        public List<ApplicationVersion> GetCachedData(string programName)
        {
            var data = LoadApplicationData();
            return data.ContainsKey(programName) ? data[programName] : new List<ApplicationVersion>();
        }

        /// <summary>
        /// 更新指定程序的缓存数据
        /// </summary>
        /// <param name="programName">程序名称</param>
        /// <param name="versions">版本列表</param>
        /// <returns>是否更新成功</returns>
        public bool UpdateCachedData(string programName, List<ApplicationVersion> versions)
        {
            var data = LoadApplicationData();
            data[programName] = versions;
            return SaveApplicationData(data);
        }

        /// <summary>
        /// 清除所有缓存数据
        /// </summary>
        /// <returns>是否清除成功</returns>
        public bool ClearAllData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    File.Delete(_dataFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清除数据失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取缓存文件的最后修改时间
        /// </summary>
        /// <returns>最后修改时间，如果文件不存在则返回null</returns>
        public DateTime? GetCacheLastModified()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    return File.GetLastWriteTime(_dataFilePath);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}