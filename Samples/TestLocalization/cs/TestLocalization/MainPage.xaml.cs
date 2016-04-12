using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestLocalization
{
    public class LanguageValue
    {
        public string DisplayName { get; set; }
        public string LanguageTag { get; set; }
        public override string ToString() { return LanguageTag; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<LanguageValue> LanguageValues;
        int lastSelectionIndex;
        public MainPage()
        {
            this.InitializeComponent();

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LanguageValues = new List<LanguageValue>();

            // First show the default setting
          //  LanguageValues.Add(new LanguageValue() { DisplayName = "Use language preferences (recommended)", LanguageTag = "" });


            // If there are app languages that the user speaks, show them next

            // Note: the first (non-override) language, if set as the primary language override
            // would give the same result as not having any primary language override. There's
            // still a difference, though: If the user changes their language preferences, the 
            // default setting (no override) would mean that the actual primary app language
            // could change. But if it's set as an override, then it will remain the primary
            // app language after the user changes their language preferences.

            // Add machine preferred languages
            /*
            for (var i = 0; i < Windows.Globalization.ApplicationLanguages.Languages.Count; i++)
            {
                var lang = new Windows.Globalization.Language(Windows.Globalization.ApplicationLanguages.Languages[i]);
                LanguageValues.Add(new LanguageValue() { DisplayName = lang.NativeName, LanguageTag = lang.LanguageTag });
            }*/
            // Add application manifest languages
            for (var i = 0; i < Windows.Globalization.ApplicationLanguages.ManifestLanguages.Count; i++)
            {
                var lang = new Windows.Globalization.Language(Windows.Globalization.ApplicationLanguages.ManifestLanguages[i]);
                LanguageValues.Add(new LanguageValue() { DisplayName = lang.NativeName, LanguageTag = lang.LanguageTag });
            }
            MachineLanguages.Text = GetAppLanguagesAsFormattedString(Windows.Globalization.ApplicationLanguages.Languages);
            ManifestLanguages.Text = GetAppLanguagesAsFormattedString(Windows.Globalization.ApplicationLanguages.ManifestLanguages);

            int selectIndex = -1;
            int index = 0;
            foreach (var l in LanguageValues)
            {
                LanguageCombo.Items.Add(l);
                if (FindCurrent(l) == true)
                    selectIndex = index;
                index++;
            }
            if (LanguageCombo.Items.Count > 0)
            {
                if (selectIndex != -1)
                    LanguageCombo.SelectedIndex = selectIndex;
                else
                    // if PrimaryLanguageOverride not set, select first language in the list
                    LanguageCombo.SelectedIndex = 0;
            }
            lastSelectionIndex = LanguageCombo.SelectedIndex;
            LanguageCombo.SelectionChanged += LanguageCombo_SelectionChanged;

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            this.AppDescription.Text = resourceLoader.GetString("appDescription");

        }
        private static bool FindCurrent(LanguageValue value)
        {

            if (value.LanguageTag == Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride)
            {
                return true;
            }
            return false;

        }
        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var l = LanguageCombo.SelectedItem as LanguageValue;
            if (l != null) {
                if (LanguageCombo.SelectedValue.ToString() == "-")
                {
                    LanguageCombo.SelectedIndex = lastSelectionIndex;
                }
                else
                {
                    lastSelectionIndex = LanguageCombo.SelectedIndex;
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = LanguageCombo.SelectedValue.ToString() ;


                }
            }
        }

        private string GetAppLanguagesAsFormattedString(IReadOnlyList<string> languages)
        {
            if (languages == null)
                return String.Empty;
            var countLanguages = languages.Count;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (var i = 0; i < countLanguages - 1; i++)
            {
                sb.Append(languages[i]);
                sb.Append(", ");
            }
            sb.Append(languages[countLanguages - 1]);
            return sb.ToString();
        }

        private void buttonRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
                Frame.Navigate(typeof(MainPage));
        }

    }
}
