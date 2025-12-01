using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PackageManager.Models;
using PackageManager.Services;

namespace PackageManager.Views
{
    public partial class LocalPathSettingsPage : Page, ICentralPage
    {
        private readonly DataPersistenceService dataPersistenceService;
        private readonly ObservableCollection<PackageInfo> packages;

        public event Action RequestExit;
        public event Action Saved;

        public LocalPathSettingsPage(DataPersistenceService dataPersistenceService,
                                      ObservableCollection<PackageInfo> packages)
        {
            InitializeComponent();
            this.dataPersistenceService = dataPersistenceService;
            this.packages = packages;

            var items = new ObservableCollection<LocalPathInfo>();
            foreach (var p in packages)
            {
                var versions = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                if (p.AvailableVersions != null)
                {
                    foreach (var v in p.AvailableVersions)
                    {
                        if (!string.IsNullOrWhiteSpace(v)) versions.Add(v);
                    }
                }
                if (p.VersionLocalPaths != null)
                {
                    foreach (var v in p.VersionLocalPaths.Keys)
                    {
                        if (!string.IsNullOrWhiteSpace(v)) versions.Add(v);
                    }
                }
                foreach (var v in versions)
                {
                    items.Add(new LocalPathInfo
                    {
                        ProductName = p.ProductName,
                        Version = v,
                        LocalPath = p.GetLocalPathForVersion(v),
                    });
                }
            }
            LocalPathItems = items;

            DataContext = this;
        }

        public ObservableCollection<LocalPathInfo> LocalPathItems { get; set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in LocalPathItems)
            {
                var pkg = packages.FirstOrDefault(p => p.ProductName == item.ProductName);
                if (pkg != null)
                {
                    if (!string.IsNullOrWhiteSpace(item.Version))
                    {
                        pkg.VersionLocalPaths[item.Version] = item.LocalPath;
                    }
                }
            }

            dataPersistenceService.SaveMainWindowState(packages);

            try
            {
                Saved?.Invoke();
            }
            catch { }

            RequestExit?.Invoke();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RequestExit?.Invoke();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    var groups = FindVisualChildren<GroupItem>(LocalPathGrid).ToList();
                    for (int i = 0; i < groups.Count; i++)
                    {
                        var expander = FindVisualChildren<Expander>(groups[i]).FirstOrDefault();
                        if (expander != null)
                        {
                            expander.IsExpanded = (i == 0);
                        }
                    }
                }));
            }
            catch
            {
            }
        }

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) yield return t;
                foreach (var c in FindVisualChildren<T>(child)) yield return c;
            }
        }
    }
}
