using System.Diagnostics;
using HandyControl.Themes;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;

using SharedProject1;
using System.Collections.Generic;
using System;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Windows.Input;
using System.Reflection;

namespace NVIDIA_Tesla_WDDM_Tools_WPF_DotNetFramework
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Change Theme
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false;
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);
                }
                else if (button.Tag is Brush accentTag)
                {
                    ((App)Application.Current).UpdateAccent(accentTag);
                }
                else if (button.Tag is "Picker")
                {
                    var picker = SingleOpenHelper.CreateControl<ColorPicker>();
                    var window = new PopupWindow
                    {
                        PopupElement = picker,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        MinWidth = 0,
                        MinHeight = 0,
                        Title = "Select Accent Color"
                    };

                    picker.SelectedColorChanged += delegate
                    {
                        ((App)Application.Current).UpdateAccent(picker.SelectedBrush);
                        window.Close();
                    };
                    picker.Canceled += delegate { window.Close(); };
                    window.Show();
                }
            }
        }
        #endregion

        #region Component
        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnRefresh.Focus();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            ModifySelectedItems();
        }

        private void ListBoxDevices_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ModifySelectedItems();
            }
        }

        #endregion

        #region GPU

        private static bool _refreshed = false;

        private void RefreshList()
        {
            ListBoxDevices.Items.Clear();

            var gpuList = ToolsClass.ReadGpuList();

            foreach (var gpuItem in gpuList)
            {
                Debug.WriteLine(gpuItem.Item1 + " " + gpuItem.Item2);

                var newItem = new ListBoxItem
                {
                    Tag = gpuItem.Item1,
                    Content = gpuItem.Item2
                };

                ListBoxDevices.Items.Add(newItem);

                if (gpuItem.Item2.ToLower().Contains("tesla"))
                {
                    ListBoxDevices.SelectedItems.Add(newItem);
                }
            }

            _refreshed = true;
            ListBoxDevices.Focus();
        }

        private void ModifySelectedItems()
        {
            if (ListBoxDevices.Items.Count == 0)
            {
                if (_refreshed)
                {
                    MessageBox.Show
                    (
                        "You've done a refresh operation, but it looks like you don't have any devices!",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                else
                {
                    MessageBox.Show
                    (
                        "You haven't performed a refresh operation yet!\n" +
                        "A refresh is performed and the program automatically selects the items!",
                        "Notify",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    RefreshList();
                }
            }

            var selectedItems = ListBoxDevices.SelectedItems;
            var selectedItemsCount = selectedItems.Count;

            if (selectedItemsCount == 0)
            {
                MessageBox.Show
                (
                    "You haven't selected any items!",
                    "Notify",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            var result = MessageBox.Show
            (
                $"Processing of the selected {selectedItemsCount}devices is about to begin.",
                "Notify",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.OK)
            {
                Debug.WriteLine("User confirm.");
            }
            else
            {
                Debug.WriteLine("User cancel.");
                return;
            }


            var gpuList = new List<string>();

            foreach (ListBoxItem listBoxItem in selectedItems)
            {
                var gpuId = listBoxItem.Tag.ToString();
                var gpuName = listBoxItem.Content.ToString();

                Debug.WriteLine($"GpuId:{gpuId} GpuName:{gpuName}");
                gpuList.Add(gpuId);
            }

            ToolsClass.ModifyGpuReg(gpuList);
        }

        #endregion

        #region Menu
        private void MenuItemRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void MenuItemModify_Click(object sender, RoutedEventArgs e)
        {
            ModifySelectedItems();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemGithubRepository_Click(object sender, RoutedEventArgs e)
        {
            if (!Utils.OpenUrl(@"https://github.com/a645162/Nvidia-Tesla-WDDM-Tools"))
            {
                MessageBox.Show
                (
                    "The webpage cannot be successfully opened using the default browser!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void MenuItemAboutProgram_Click(object sender, RoutedEventArgs e)
        {
            string version = Assembly.GetEntryAssembly()?.GetName().Version.ToString();

            Debug.WriteLine("Version:" + version);

            MessageBox.Show
            (
                $"NVIDIA Tesla WDDM Tools\nVersion:{version}",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        #endregion

    }
}
