using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;

namespace WallpaperTaskNeo
{
    public sealed class Wptask : IBackgroundTask
    {
        ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
        BackgroundTaskDeferral _deferral; // Note: defined at class scope so that we can mark it complete inside the OnCancel() callback if we choose to support cancellation

        

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Debug.WriteLine("bgtask: " + taskInstance.Task.Name);
            await SetNewWallpaper();
            _deferral.Complete();
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
        }

    }
}
