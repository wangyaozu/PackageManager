using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PackageManager.Models;
using PackageManager.Services;

namespace PackageManager
{
    public partial class LocalPathSettingsWindow : Window
    {
        private readonly DataPersistenceService dataPersistenceService;

        private readonly ObservableCollection<PackageInfo> packages;

        public LocalPathSettingsWindow(DataPersistenceService dataPersistenceService,
                                       ObservableCollection<PackageInfo> packages)
        {
            InitializeComponent();
            this.dataPersistenceService = dataPersistenceService;
            this.packages = packages;

            // 构建本地路径设置项集合
            LocalPathItems = new ObservableCollection<LocalPathInfo>(packages.Select(p => new LocalPathInfo
            {
                ProductName = p.ProductName,
                LocalPath = p.LocalPath,
            }));

            DataContext = this;
        }

        public ObservableCollection<LocalPathInfo> LocalPathItems { get; set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 将设置项写回原始PackageInfo集合
            foreach (var item in LocalPathItems)
            {
                var pkg = packages.FirstOrDefault(p => p.ProductName == item.ProductName);
                if (pkg != null)
                {
                    pkg.LocalPath = item.LocalPath;
                }
            }

            // 保存主界面状态（包含LocalPath）
            dataPersistenceService.SaveMainWindowState(packages);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}