using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class WallpaperPage : Page
    {
        ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;


        public WallpaperPage()
        {
            this.InitializeComponent();

            if(roamingSettings.Values["wpTaskFreq"] != null)
            {
                wpFreqBox.SelectedIndex = (int)roamingSettings.Values["wpTaskFreq"];
            }
            else
                wpFreqBox.SelectedIndex = 0;

            if (roamingSettings.Values["wpTaskSearch"] != null)
            {
                wpSearchBox.Text = (string) roamingSettings.Values["wpTaskSearch"];
            }

            if (roamingSettings.Values["wpTask"] != null && (bool)roamingSettings.Values["wpTask"])
                wpButton.Content = "Stop Service";
            else
            {
                wpButton.Content = "Start Service";
                roamingSettings.Values["wpTask"] = false;
            } 

            if (roamingSettings.Values["wpTaskUrl"] != null)
            {
                BitmapImage i = new BitmapImage();
                i.UriSource = new Uri((string)roamingSettings.Values["wpTaskUrl"], UriKind.Absolute);
                wpImage.Source = i;
            }
            else
                wpImage.Visibility = Visibility.Collapsed;

        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if(rb.Tag.ToString() == "on")
            {
                roamingSettings.Values["wpTask"] = true;
                roamingSettings.Values["wpTaskFreq"] = wpFreqBox.SelectedIndex;
                roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;

                var builder = new BackgroundTaskBuilder();
                builder.Name = "WallpaperTask";
                switch((int)roamingSettings.Values["wpTaskFreq"])
                {
                    default:
                        builder.SetTrigger(new TimeTrigger(60, true));
                        break;
                    case 0:
                        builder.SetTrigger(new TimeTrigger(60, true));
                        break;
                    case 1:
                        builder.SetTrigger(new TimeTrigger(180, true));
                        break;
                    case 2:
                        builder.SetTrigger(new TimeTrigger(1440, true));
                        break;
                    case 3:
                        builder.SetTrigger(new TimeTrigger(10080, true));
                        break;
                }

                builder.SetTrigger(new TimeTrigger(15, true)); //DEBUG
                //BackgroundTaskRegistration task = builder.Register();
                //WinSplash.Tasks.WallpaperTask.ChangeWallpaper();
            }
            else
            {
                roamingSettings.Values["wpTask"] = false;
            }

        }

        private void wpFreqBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            roamingSettings.Values["wpTaskFreq"] = wpFreqBox.SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;
            //WinSplash.Tasks.WallpaperTask.ChangeWallpaper();
        }

        private void wpSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;
        }

        private async void wpButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)roamingSettings.Values["wpTask"])
            {
                wpButton.Content = "Start Service";
                roamingSettings.Values["wpTask"] = false;
            }
            else
            {
                wpButton.Content = "Stop Service";
                roamingSettings.Values["wpTask"] = true;

                await BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();
                builder.Name = "WallpaperTask";
                builder.TaskEntryPoint = "Tasks.WallpaperTask";
                switch ((int)roamingSettings.Values["wpTaskFreq"])
                {
                    default:
                        builder.SetTrigger(new TimeTrigger(60, true));
                        break;
                    case 0:
                        builder.SetTrigger(new TimeTrigger(60, true));
                        break;
                    case 1:
                        builder.SetTrigger(new TimeTrigger(180, true));
                        break;
                    case 2:
                        builder.SetTrigger(new TimeTrigger(1440, true));
                        break;
                    case 3:
                        builder.SetTrigger(new TimeTrigger(10080, true));
                        break;
                }

                builder.SetTrigger(new TimeTrigger(15, true)); //TESTING
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                BackgroundTaskRegistration task = builder.Register();
            }
        }
    }
}
