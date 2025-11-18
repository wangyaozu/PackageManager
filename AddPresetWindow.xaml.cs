using System;
using System.Windows;
using PackageManager.Models;

namespace PackageManager
{
    public partial class AddPresetWindow : Window
    {
        public ConfigPreset ResultPreset { get; private set; }

        public AddPresetWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameText?.Text?.Trim();
            var server = ServerDomainText?.Text?.Trim();
            var common = CommonServerDomainText?.Text?.Trim();
            var iep = (IEProxyCombo?.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "yes";

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(common))
            {
                MessageBox.Show("请填写名称、ServerDomain 和 CommonServerDomain", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int rt = ParseIntOrDefault(RequestTimeoutText?.Text, 5000);
            int st = ParseIntOrDefault(ResponseTimeoutText?.Text, 5000);
            int retry = ParseIntOrDefault(RetryTimesText?.Text, 0);

            ResultPreset = new ConfigPreset
            {
                Name = name,
                ServerDomain = server,
                CommonServerDomain = common,
                IEProxyAvailable = iep,
                requestTimeout = rt,
                responseTimeout = st,
                requestRetryTimes = retry
            };

            DialogResult = true;
            Close();
        }

        private int ParseIntOrDefault(string text, int def)
        {
            if (int.TryParse(text, out var val) && val >= 0) return val;
            return def;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}