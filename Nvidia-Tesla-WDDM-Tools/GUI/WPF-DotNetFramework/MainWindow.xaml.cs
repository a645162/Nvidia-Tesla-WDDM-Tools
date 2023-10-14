using System.Diagnostics;
using HandyControl.Themes;
using HandyControl.Tools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;

using SharedProject1;

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

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            var gpuList = ToolsClass.ReadGpuList();

            foreach (var gpuItem in gpuList)
            {
                Debug.WriteLine(gpuItem.Item1+" "+gpuItem.Item2);
            }

            

        }
    }
}
