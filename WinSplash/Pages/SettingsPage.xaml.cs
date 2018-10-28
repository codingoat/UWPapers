using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WinSplash.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
                    i2.UriSource = new Uri("ms-appx:///Assets/pixabay_white.png", UriKind.Absolute);
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

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            
            switch(rb.Tag.ToString())
            {
                case "light":
                    roamingSettings.Values["theme"] = "light";
                    break;
                case "dark":
                    roamingSettings.Values["theme"] = "dark";
                    break;
            }

            /*AppRestartFailureReason result =
            await CoreApplication.RequestRestartAsync("");
            if (result == AppRestartFailureReason.NotInForeground ||
                result == AppRestartFailureReason.RestartPending ||
                result == AppRestartFailureReason.Other)
            {
                Debug.WriteLine("RequestRestartAsync failed: {0}", result);
            }*/
        }
    }
}
