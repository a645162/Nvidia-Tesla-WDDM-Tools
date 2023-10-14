using HandyControl.Themes;
using System.Windows;
using System.Windows.Media;
namespace NVIDIA_Tesla_WDDM_Tools_WPF_DotNetFramework
{
    public partial class App : Application
{
        internal void UpdateTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
            }
        }

        internal void UpdateAccent(Brush accent)
        {
            if (ThemeManager.Current.AccentColor != accent)
            {
                ThemeManager.Current.AccentColor = accent;
            }
        }
    }
}
