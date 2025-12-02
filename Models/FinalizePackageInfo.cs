using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CustomControlLibrary.CustomControl.Attribute.DataGrid;

namespace PackageManager.Models
{
    public class FinalizePackageInfo : INotifyPropertyChanged
    {
        private string _productName = string.Empty;
        private string _version = string.Empty;
        private string _time = string.Empty;
        private string _ftpServerPath = string.Empty;
        private string _ftpBasePath = string.Empty;
        private string _localPath = string.Empty;
        private string _uploadPackageName = string.Empty;
        private ObservableCollection<string> _availableVersions;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action<FinalizePackageInfo>? DownloadRequested;
        public event Action<FinalizePackageInfo, string>? VersionChanged;

        [DataGridColumn(0, DisplayName = "产品名称", Width = "180", IsReadOnly = true)]
        public string ProductName
        {
            get => _productName;
            set
            {
                if (_productName == value) return;
                _productName = value ?? string.Empty;
                OnPropertyChanged(nameof(ProductName));
            }
        }

        [DataGridComboBox(1, "版本", "AvailableVersions", Width = "220",TextAlign = TextAlignment.Left, ContentAlign = HorizontalAlignment.Left)]
        public string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;
                _version = value ?? string.Empty;
                OnPropertyChanged(nameof(Version));
                // 选择版本后，同步选中版本对应的服务器路径
                if (!string.IsNullOrWhiteSpace(_ftpBasePath) && !string.IsNullOrWhiteSpace(_version))
                {
                    FtpServerPath = _ftpBasePath.TrimEnd('/') + "/" + _version + "/";
                }
                VersionChanged?.Invoke(this, _version);
            }
        }

        [DataGridColumn(2, DisplayName = "时间", Width = "150", IsReadOnly = true)]
        public string Time
        {
            get => _time;
            set
            {
                if (_time == value) return;
                _time = value ?? string.Empty;
                OnPropertyChanged(nameof(Time));
            }
        }

        // 源地址与本地目录不显示在表格，但参与逻辑
        public string FtpServerPath
        {
            get => _ftpServerPath;
            set
            {
                if (_ftpServerPath == value) return;
                _ftpServerPath = value ?? string.Empty;
                OnPropertyChanged(nameof(FtpServerPath));
            }
        }

        // 服务器基础路径（不显示在表格，用于组合选中版本目录）
        public string FtpBasePath
        {
            get => _ftpBasePath;
            set
            {
                if (_ftpBasePath == value) return;
                _ftpBasePath = value ?? string.Empty;
                OnPropertyChanged(nameof(FtpBasePath));
            }
        }

        public string LocalPath
        {
            get => _localPath;
            set
            {
                if (_localPath == value) return;
                _localPath = value ?? string.Empty;
                OnPropertyChanged(nameof(LocalPath));
            }
        }

        public string UploadPackageName
        {
            get => _uploadPackageName;
            set
            {
                if (_uploadPackageName == value) return;
                _uploadPackageName = value ?? string.Empty;
                OnPropertyChanged(nameof(UploadPackageName));
            }
        }

        // 可用版本列表（供下拉选择使用）
        public ObservableCollection<string> AvailableVersions
        {
            get => _availableVersions ??= new ObservableCollection<string>();
            set
            {
                if (_availableVersions == value) return;
                _availableVersions = value ?? new ObservableCollection<string>();
                OnPropertyChanged(nameof(AvailableVersions));
            }
        }

        // 便捷方法：更新版本列表，并设置默认选中项
        public void UpdateAvailableVersions(System.Collections.Generic.IEnumerable<string> versions)
        {
            AvailableVersions.Clear();
            if (versions == null) return;
            foreach (var v in versions)
            {
                if (!string.IsNullOrWhiteSpace(v))
                {
                    AvailableVersions.Add(v);
                }
            }
            if (string.IsNullOrWhiteSpace(Version) && AvailableVersions.Count > 0)
            {
                Version = AvailableVersions[AvailableVersions.Count - 1];
            }
        }

        [DataGridButton(3, DisplayName = "下载", Width = "110", ButtonText = "下载", ButtonWidth = 90, ButtonHeight = 26, ButtonCommandProperty = nameof(DownloadCommand))]
        public string DownloadOp => "下载";

        public ICommand DownloadCommand => new RelayCommand(() => DownloadRequested?.Invoke(this));

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}

