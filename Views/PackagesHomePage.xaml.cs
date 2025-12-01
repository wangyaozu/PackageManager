using System.Windows;
using System.Windows.Controls;
using PackageManager.Function.CsvTool;
using PackageManager.Function.DnsTool;

namespace PackageManager.Views
{
    /// <summary>
    /// 包列表主页：承载包列表与右侧快捷操作面板
    /// 继承 MainWindow 的 DataContext，不在此处重置。
    /// </summary>
    public partial class PackagesHomePage : Page
    {
        public PackagesHomePage()
        {
            InitializeComponent();
        }

        // 公开内部网格以便主窗口进行筛选交互
        public CustomControlLibrary.CustomControl.Controls.DataGrid.CDataGrid PackageGrid => PackageDataGrid;

        private void OpenCsvCryptoWindowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new CsvCryptoWindow();
                win.Owner = Window.GetWindow(this);
                win.Show();
            }
            catch
            {
                MessageBox.Show("打开CSV加解密窗口失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenDnsSettingsWindowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new DnsSettingsWindow { Owner = Window.GetWindow(this) };
                win.ShowDialog();
            }
            catch
            {
                MessageBox.Show("打开DNS设置窗口失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
