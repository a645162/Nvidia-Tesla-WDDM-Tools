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

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            ModifySelectedItems();
        }

        #endregion

        #region GPU

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

            }
        }

        private void ModifySelectedItems()
        {
            var selectedItems = ListBoxDevices.SelectedItems;

            if (selectedItems.Count == 0)
            {
                MessageBox.Show
                (
                    "您还没有选中任何项目！",
                    "温馨提醒",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

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
    }
}
