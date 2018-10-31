using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace WinSplash.Pages
{
    public sealed partial class SettingsPage : Page
    {
        ObservableCollection<string> resList = new ObservableCollection<string>();

        Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
            resList.Add("1920x1080");
            resList.Add("2560x1440");
            resList.Add("3840x2160");

            if (roamingSettings.Values["res"] != null)
                resBox.SelectedIndex = resList.IndexOf(roamingSettings.Values["res"] as string);

            if(roamingSettings.Values["theme"] != null)
            {
                if ((string)roamingSettings.Values["theme"] == "light")
                    radioLight.IsChecked = true;
                else
                {
                    radioDark.IsChecked = true;

                    BitmapImage i1 = new BitmapImage();
                    i1.UriSource = new Uri("ms-appx:///Assets/github_white.png", UriKind.Absolute);
                    githubImage.Source = i1;
                    BitmapImage i2 = new BitmapImage();
                    i2.UriSource = new Uri("ms-appx:///Assets/pixabay_square_white.png", UriKind.Absolute);
                    unsplashImage.Source = i2;
                }     
            }
            else
                radioLight.IsChecked = true;
        }

        private void ResChanged(object sender, SelectionChangedEventArgs e)
        {
            roamingSettings.Values["res"] = resBox.SelectedValue as string;
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if(rb.Tag.ToString() == "light" && (string)roamingSettings.Values["theme"] == "dark" || rb.Tag.ToString() == "dark" && (string)roamingSettings.Values["theme"] == "light")
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Theme changed",
                    Content = "Please restart the application to apply theme changes.",
                    CloseButtonText = "Ok",
                };
                ContentDialogResult result = await dialog.ShowAsync();
            }

            switch (rb.Tag.ToString())
            {
                case "light":
                    roamingSettings.Values["theme"] = "light";
                    break;
                case "dark":
                    roamingSettings.Values["theme"] = "dark";
                    break;
            }   
        }
    }
}
