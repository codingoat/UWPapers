using PixabaySharp;
using PixabaySharp.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
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
        PixabaySharpClient pixabayClient = new PixabaySharpClient("3153915-c1b347f3736d73ef2cd6a0e79");


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

        private void wpSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;

            GetNewImages();
        }


        private async Task GetNewImages()
        {
            roamingSettings.Values["wpTaskSearch"] = wpSearchBox.Text;
            string search = (string)roamingSettings.Values["wpTaskSearch"];

            string[] res;
            if (roamingSettings.Values["res"] != null)
                res = (roamingSettings.Values["res"] as string).Split('x');
            else
                res = new string[] { "1920", "1080" };

            List<string> urls = new List<string>();

            PixabaySharp.Models.ImageResult result = null;
            if (search == "")
            {
                while (result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Page = 1, PerPage = 200, MinWidth = int.Parse(res[0]), MinHeight = int.Parse(res[1])
                    });
                }       
            }
            else
            {
                while (result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Query = search, Page = 1, PerPage = 200, MinWidth = int.Parse(res[0]), MinHeight = int.Parse(res[1])
                    });
                }
            }

            foreach (PixabaySharp.Models.ImageItem img in result.Images)
            {
                urls.Add(img.ImageURL);
            }

            //roamingSettings.Values["wpTaskUrlList"] = String.Join(";", urls);
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("wpTaskUrlList.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, String.Join(";", urls));
        }

        private async Task SetNewWallpaper()
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.GetFileAsync("wpTaskUrlList.txt");
            string url = (await FileIO.ReadTextAsync(sampleFile)).Split(';')[new Random().Next(200)]; //forgive me

            byte[] data;
            //string filename = DateTime.Now.ToString("d");
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            string mediaType = response.Content.Headers.ContentType.MediaType.Split('/')[1];
            data = await response.Content.ReadAsByteArrayAsync();
            string filename = "wallpaper." + mediaType;

            //ApplicationData.Current.LocalFolder
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, data);

            await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);

            roamingSettings.Values["wpTaskUrl"] = url;

            BitmapImage i = new BitmapImage();
            i.UriSource = new Uri((string)roamingSettings.Values["wpTaskUrl"], UriKind.Absolute);
            wpImage.Source = i;
        }



        private async void StartService(object sender, RoutedEventArgs e)
        {
            /*var exampleTaskName = "WallpaperTask";
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    return; //task already registered
                }
            }*/


            if ((bool)roamingSettings.Values["wpTask"])
            {
                wpButton.Content = "Start Service";
                roamingSettings.Values["wpTask"] = false;

                var tasks = BackgroundTaskRegistration.AllTasks;
                foreach (var task in tasks)
                {
                    Debug.WriteLine("Unregistering bgtask: " + task.Value.Name);
                    task.Value.Unregister(true);
                }
            }
            else
            {
                wpButton.Content = "Stop Service";
                roamingSettings.Values["wpTask"] = true;

                await GetNewImages();
                await SetNewWallpaper();

                await BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();
                //builder.Name = "WallpaperTask " + DateTime.Now.ToString("d");
                builder.Name = "WallpaperTask";
                builder.TaskEntryPoint = "WallpaperTaskNeo.Wptask";
                switch ((int)roamingSettings.Values["wpTaskFreq"])
                {
                    default:
                        builder.SetTrigger(new TimeTrigger(60, false));
                        break;
                    case 0:
                        builder.SetTrigger(new TimeTrigger(60, false));
                        break;
                    case 1:
                        builder.SetTrigger(new TimeTrigger(180, false));
                        break;
                    case 2:
                        builder.SetTrigger(new TimeTrigger(1440, false));
                        break;
                    case 3:
                        builder.SetTrigger(new TimeTrigger(10080, false));
                        break;
                }

                //builder.SetTrigger(new TimeTrigger(15, true)); //TESTING
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                BackgroundTaskRegistration task = builder.Register();
                task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
            }
        }

        private async void GetNewWallpaper(object sender, RoutedEventArgs e)
        {
            await GetNewImages();
            await SetNewWallpaper();
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            BitmapImage i = new BitmapImage();
            i.UriSource = new Uri((string)roamingSettings.Values["wpTaskUrl"], UriKind.Absolute);
            wpImage.Source = i;
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
