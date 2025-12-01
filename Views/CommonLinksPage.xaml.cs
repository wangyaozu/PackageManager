using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PackageManager.Views
{
    public partial class CommonLinksPage : Page, ICentralPage
    {
        public ObservableCollection<PackageManager.CommonLinkItem> Links { get; }

        public event Action RequestExit;

        public CommonLinksPage(ObservableCollection<PackageManager.CommonLinkItem> links)
        {
            InitializeComponent();
            Links = links ?? new ObservableCollection<PackageManager.CommonLinkItem>();
            DataContext = this;
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PackageManager.CommonLinkItem item)
            {
                OpenUrl(item.Url);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    MessageBox.Show("目标链接为空", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                try
                {
                    Process.Start("explorer.exe", url);
                }
                catch (Exception ex)
                {
                    PackageManager.Services.LoggingService.LogError(ex, "打开网址失败");
                    MessageBox.Show($"打开网址失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RequestExit?.Invoke();
        }
    }
}
