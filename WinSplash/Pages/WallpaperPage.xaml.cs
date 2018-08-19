using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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

        private void wpFreqBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            roamingSettings.Values["wpTaskFreq"] = wpFreqBox.SelectedIndex;
        }

        private async void GetNewWallpaper(object sender, RoutedEventArgs e)
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;


            string search = (string)roamingSettings.Values["wpTaskSearch"];
            string url;
            string res;

            if (roamingSettings.Values["res"] != null)
                res = roamingSettings.Values["res"] as string;
            else
                res = "1920x1080";


            if (search == "")
                url = await Utils.GetRedirectedUrl("https://source.unsplash.com/random/" + res + "?sig=" + 1);
            else
                url = await Utils.GetRedirectedUrl("https://source.unsplash.com/" + res + "/?" + search + "&sig=" + 1);
            Debug.WriteLine(DateTime.Now + url);



            byte[] data;
            string filename = DateTime.Now.ToString("d");
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            string mediaType = response.Content.Headers.ContentType.MediaType.Split('/')[1];
            data = await response.Content.ReadAsByteArrayAsync();
            filename += "." + mediaType;

            //ApplicationData.Current.LocalFolder
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, data);

            await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);

            roamingSettings.Values["wpTaskUrl"] = url;

            BitmapImage i = new BitmapImage();
            i.UriSource = new Uri((string)roamingSettings.Values["wpTaskUrl"], UriKind.Absolute);
            wpImage.Source = i;
        }

        private void wpSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;
        }

        private async void StartService(object sender, RoutedEventArgs e)
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
                builder.TaskEntryPoint = "WallpaperTaskNeo.Wptask";
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

                //builder.SetTrigger(new TimeTrigger(15, true)); //TESTING
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                BackgroundTaskRegistration task = builder.Register();
            }
        }

        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            Utils.CopyUrl((String) roamingSettings.Values["wpTaskUrl"]);
            Utils.NotifyImage("Link copied to clipboard", (String)roamingSettings.Values["wpTaskUrl"], 10);
            
            //await Windows.System.Launcher.LaunchUriAsync(new Uri(btn.Tag.ToString(), UriKind.Absolute));
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            Utils.CopyImage((String)roamingSettings.Values["wpTaskUrl"]);
            Utils.NotifyImage("Image copied to clipboard", (String)roamingSettings.Values["wpTaskUrl"], 10);
        }


        private async void SaveImage(object sender, RoutedEventArgs e)
        {
            await Utils.SaveImage((String)roamingSettings.Values["wpTaskUrl"]);
        }

        private async void SetWallpaper(object sender, RoutedEventArgs e)
        {
            await Utils.SetWallpaper((String)roamingSettings.Values["wpTaskUrl"]);
        }
    }
}
