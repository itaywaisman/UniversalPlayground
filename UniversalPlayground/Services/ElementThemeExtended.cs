using Windows.UI.Xaml;

namespace UniversalPlayground.Services
{
    /// <summary>
    /// Defines the type of theme
    /// </summary>
    public enum ElementThemeExtended
    {
        Default = ElementTheme.Default,
        Light = ElementTheme.Light,
        Dark = ElementTheme.Dark,
        Custom = 1000
    }

    /// <summary>
    /// Defines the different Picture-in-picture modes
    /// </summary>
    public enum PipModes
    {
        SingleView,
        MultiView
    }
}