using System;
using System.Windows;
using NZ.RdpMaid.App.Core.Services;
using NZ.RdpMaid.App.Settings;

namespace NZ.RdpMaid.App.UiServices
{
    internal class ThemeLoader(AppSettingsProvider settingsProvider, StateStorage stateStorage)
    {
        private const string _defaultThemeName = "PinkLoli";
        private const string _darkThemeName = "NightMistress";
        private const string _necoArcThemeName = "NecoArc";
        private string _currentThemeName = _defaultThemeName;

        public void LoadTheme()
        {
            var state = stateStorage.Load();
            var settings = settingsProvider.Settings;

            var nextThemeName = state.Theme ?? settings.Theme;

            if (nextThemeName != _defaultThemeName)
            {
                var currentTheme = FindThemeByName(_defaultThemeName);

                if (currentTheme is not null)
                {
                    ReplaceTheme(currentTheme, nextThemeName);

                    stateStorage.Save(state with { Theme = nextThemeName });
                }
            }
        }

        public void ToggleTheme()
        {
            var nextThemeName = _currentThemeName switch
            {
                _defaultThemeName => _darkThemeName,
                _darkThemeName => _necoArcThemeName,
                _ => _defaultThemeName,
            };

            var currentTheme = FindThemeByName(_currentThemeName);

            if (currentTheme is not null)
            {
                ReplaceTheme(currentTheme, nextThemeName);

                var state = stateStorage.Load();

                stateStorage.Save(state with { Theme = nextThemeName });
            }
        }

        private static ResourceDictionary? FindThemeByName(string name)
        {
            ResourceDictionary? currentTheme = null;

            foreach (var md in Application.Current.Resources.MergedDictionaries)
            {
                if (md.Source is not null && md.Source.OriginalString.EndsWith($"{name}.xaml"))
                {
                    currentTheme = md;
                    break;
                }
            }

            return currentTheme;
        }

        private void ReplaceTheme(ResourceDictionary currentTheme, string nextThemeName)
        {
            var themeUri = new Uri($"pack://application:,,,/Resources/Themes/{nextThemeName}.xaml");
            var theme = new ResourceDictionary { Source = themeUri };

            Application.Current.Resources.MergedDictionaries.Remove(currentTheme);
            Application.Current.Resources.MergedDictionaries.Add(theme);

            _currentThemeName = nextThemeName;
        }
    }
}