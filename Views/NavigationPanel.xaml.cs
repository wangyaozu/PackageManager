using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PackageManager.Models;

namespace PackageManager.Views
{
    public partial class NavigationPanel : UserControl
    {
        public class NavigationActionItem
        {
            public string Name { get; set; }
            public string Glyph { get; set; }
            public ICommand Command { get; set; }
            public ObservableCollection<NavigationActionItem> Children { get; } = new ObservableCollection<NavigationActionItem>();
        }

        // 统一的系统入口列表数据源
        public ObservableCollection<NavigationActionItem> ActionItems { get; } = new ObservableCollection<NavigationActionItem>();

        public NavigationPanel()
        {
            InitializeComponent();
            Loaded += NavigationPanel_Loaded;
        }

        private void NavigationPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var mw = Window.GetWindow(this) as MainWindow;
            if (mw == null) return;

            // 构建统一的导航动作列表
            ActionItems.Clear();
            
            ActionItems.Add(new NavigationActionItem { Name = "产品分类", Glyph = "\uE8D2", Command = mw.NavigateHomeCommand});
            ActionItems.Add(new NavigationActionItem { Name = "产品日志", Glyph = "\uE7BA", Command = mw.OpenProductLogsCommand });
            ActionItems.Add(new NavigationActionItem { Name = "软件设置", Glyph = "\uE713", Command = mw.SettingsCommand });
            ActionItems.Add(new NavigationActionItem { Name = "路径设置", Glyph = "\uE8B7", Command = mw.LocalPathSettingsCommand });
            ActionItems.Add(new NavigationActionItem { Name = "软件日志", Glyph = "\uE7BA", Command = mw.OpenLogViewerCommand });
            ActionItems.Add(new NavigationActionItem { Name = "包管理配置", Glyph = "\uE8F1", Command = mw.OpenPackageConfigCommand });
        }

        private void ActionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as NavigationActionItem;
            var cmd = item?.Command;
            if (cmd?.CanExecute(null) == true)
            {
                cmd.Execute(null);
            }
        }

        private void CategoryChildListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as NavigationActionItem;
            var cmd = item?.Command;
            if (cmd?.CanExecute(null) == true)
            {
                cmd.Execute(null);
            }
        }
    }
}
