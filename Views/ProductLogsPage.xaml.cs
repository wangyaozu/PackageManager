using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PackageManager.Models;
using PackageManager.Services;

namespace PackageManager.Views
{
    public partial class ProductLogsPage : Page, ICentralPage
    {
        private readonly string _baseDir;
        private readonly DataPersistenceService _dataPersistenceService;
        private readonly ApplicationFinderService _applicationFinderService;
        private AppSettings _settings;

        public event Action RequestExit;

        public ObservableCollection<ProductLogInfo> ProductLogs { get; } = new ObservableCollection<ProductLogInfo>();

        public ProductLogsPage(string baseDir)
        {
            InitializeComponent();
            _baseDir = baseDir;
            _dataPersistenceService = new DataPersistenceService();
            _applicationFinderService = new ApplicationFinderService();
            _settings = _dataPersistenceService.LoadSettings();

            BaseDirText.Text = $"日志目录: {_baseDir}";
            RefreshLogs();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogs();
        }

        private void RefreshLogs()
        {
            try
            {
                ProductLogs.Clear();

                if (string.IsNullOrWhiteSpace(_baseDir) || !Directory.Exists(_baseDir))
                {
                    MessageBox.Show("日志目录不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var files = Directory.GetFiles(_baseDir, "*.log", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    var model = new ProductLogInfo
                    {
                        FileName = fi.Name,
                        Directory = fi.DirectoryName,
                        FullPath = fi.FullName,
                        SizeText = FormatSize(fi.Length),
                        ModifiedText = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    };

                    model.OpenWithLogViewProCommand = new RelayCommand(() => OpenWithLogViewPro(model));
                    model.OpenWithVSCodeCommand = new RelayCommand(() => OpenWithVSCode(model));
                    model.OpenWithNotepadCommand = new RelayCommand(() => OpenWithNotepad(model));

                    ProductLogs.Add(model);
                }

                ProductLogGrid.ItemsSource = ProductLogs;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "刷新产品日志失败");
                MessageBox.Show($"刷新产品日志失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatSize(long size)
        {
            var units = new[] { "B", "KB", "MB", "GB" };
            double s = size;
            int idx = 0;
            while (s >= 1024 && idx < units.Length - 1)
            {
                s /= 1024;
                idx++;
            }
            return $"{s:0.##} {units[idx]}";
        }

        private void OpenWithLogViewPro(ProductLogInfo item)
        {
            try
            {
                var path = GetBundledLogViewProPath();
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    MessageBox.Show("未找到内置 LogViewPro，请检查打包资源。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                StartProcess(path, item.FullPath);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "使用LogViewPro打开失败");
            }
        }

        private void OpenWithVSCode(ProductLogInfo item)
        {
            try
            {
                var path = GetOrResolveAppPath(_settings.VsCodePath, "Code", p => _settings.VsCodePath = p);
                path = PreferVSCodeExe(path);
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    MessageBox.Show("未找到 VsCode，请安装或手动配置路径。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                StartProcess(path, item.FullPath);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "使用VsCode打开失败");
            }
        }

        private void OpenWithNotepad(ProductLogInfo item)
        {
            try
            {
                StartProcess("notepad.exe", item.FullPath);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "使用记事本打开失败");
            }
        }

        private void StartProcess(string appPath, string filePath)
        {
            try
            {
                var ext = System.IO.Path.GetExtension(appPath)?.ToLowerInvariant();
                var isScript = ext == ".cmd" || ext == ".bat";
                var psi = new ProcessStartInfo
                {
                    FileName = appPath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = !isScript,
                    CreateNoWindow = isScript,
                    WindowStyle = isScript ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(appPath)
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"启动外部程序失败: {appPath}");
                MessageBox.Show($"启动外部程序失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetOrResolveAppPath(string cachedPath, string programName, Action<string> updateCached)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(cachedPath) && File.Exists(cachedPath))
                {
                    return cachedPath;
                }

                var found = _applicationFinderService.FindApplicationPath(programName);
                if (!string.IsNullOrWhiteSpace(found) && File.Exists(found))
                {
                    updateCached?.Invoke(found);
                    _dataPersistenceService.SaveSettings(_settings);
                    return found;
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"查找程序路径失败: {programName}");
            }
            return cachedPath;
        }

        private string PreferVSCodeExe(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) return null;
                var ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant();
                if (ext == ".exe") return path;
                if (ext == ".cmd" || ext == ".bat")
                {
                    var dir = System.IO.Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        var parent = Directory.GetParent(dir);
                        var candidate = System.IO.Path.Combine(parent?.FullName ?? string.Empty, "Code.exe");
                        if (!string.IsNullOrEmpty(parent?.FullName) && File.Exists(candidate))
                        {
                            _settings.VsCodePath = candidate;
                            _dataPersistenceService.SaveSettings(_settings);
                            return candidate;
                        }
                    }

                    var candidates = _applicationFinderService.FindAllApplicationPaths("Code") ?? new System.Collections.Generic.List<string>();
                    var exe = candidates.FirstOrDefault(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(exe))
                    {
                        _settings.VsCodePath = exe;
                        _dataPersistenceService.SaveSettings(_settings);
                        return exe;
                    }
                }
                return path;
            }
            catch
            {
                return path;
            }
        }

        private string GetBundledLogViewProPath()
        {
            try
            {
                var exe = EnsureEmbeddedToolExtracted("LogViewPro.exe", "LogViewPro.exe");
                if (!string.IsNullOrWhiteSpace(exe) && File.Exists(exe))
                {
                    return exe;
                }

                var fallback = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Tools", "LogViewPro.exe");
                return fallback;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "获取内置 LogViewPro 路径失败");
                return null;
            }
        }

        private string EnsureEmbeddedToolExtracted(string resourceSuffix, string outputFileName)
        {
            try
            {
                var asm = typeof(ProductLogsPage).Assembly;
                var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                var targetDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PackageManager", "tools");
                Directory.CreateDirectory(targetDir);
                var targetPath = System.IO.Path.Combine(targetDir, outputFileName);

                if (File.Exists(targetPath))
                {
                    return targetPath;
                }

                using (var stream = asm.GetManifestResourceStream(name))
                {
                    if (stream == null) return null;
                    using (var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        stream.CopyTo(fs);
                    }
                }

                return targetPath;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "提取内置工具失败");
                return null;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RequestExit?.Invoke();
        }
    }
}
