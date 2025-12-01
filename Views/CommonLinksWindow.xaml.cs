using System.Collections.ObjectModel;
using System.Windows;

namespace PackageManager.Views
{
    public partial class CommonLinksWindow : Window
    {
        public CommonLinksWindow(ObservableCollection<PackageManager.CommonLinkItem> links)
        {
            InitializeComponent();
            // 以页面形式承载常用网址列表
            ContentHost.Content = new CommonLinksPage(links);
        }
    }
}

