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

        private bool refreshed=false;

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

            ListBoxDevices.Focus();
        }

        private void ModifySelectedItems()
        {
            if (ListBoxDevices.Items.Count == 0)
            {
                if (refreshed)
                {
                    MessageBox.Show
                    (
                        "您已经执行过刷新操作，但是好像您没有任何设备！",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                else
                {
                    MessageBox.Show
                    (
                        "您还没有执行过刷新操作！\n将执行刷新，并处理程序自动选择的项目！",
                        "温馨提醒",
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
                    "您还没有选中任何项目！",
                    "温馨提醒",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }
            else
            {

                MessageBoxResult result = MessageBox.Show
                (
                    $"即将开始处理选中的{selectedItemsCount}个设备",
                    "温馨提醒",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.OK)
                {
                    Debug.WriteLine("用户确定执行！");
                }
                else
                {
                    Debug.WriteLine("取消执行！");
                    return;
                }

            }

            var gpuList = new List<string>();

            foreach (ListBoxItem listBoxItem in selectedItems)
            {
                var gpuId = listBoxItem.Tag.ToString();
                var gpuName = listBoxItem.Content.ToString();

                Debug.WriteLine($"GpuId:{gpuId} GpuName:{gpuName}");
                //string selectedValue = listBoxItem.ToString();
                gpuList.Add(gpuId);
            }

            //ToolsClass.ModifyGpuReg(gpuList);
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

        }

        private void MenuItemAboutProgram_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

    }
}
