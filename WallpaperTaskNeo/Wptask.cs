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

namespace WallpaperTaskNeo
{
    public sealed class Wptask : IBackgroundTask
    {
        ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
        BackgroundTaskDeferral _deferral; // Note: defined at class scope so that we can mark it complete inside the OnCancel() callback if we choose to support cancellation

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("bgtask");

            _deferral = taskInstance.GetDeferral();

            if ((bool)roamingSettings.Values["wpTask"])
            {
                await ChangeWallpaper();

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

                builder.SetTrigger(new TimeTrigger(15, true)); //TESTING
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                BackgroundTaskRegistration task = builder.Register();
            }

            _deferral.Complete();
        }

        async Task ChangeWallpaper()
        {
            string search = (string)roamingSettings.Values["wpTaskSearch"];
            string url;
            string res;

            if (roamingSettings.Values["res"] != null)
                res = roamingSettings.Values["res"] as string;
            else
                res = "1920x1080";


            if (search == "")
                url = await GetRedirectedUrl("https://source.unsplash.com/random/" + res + "?sig=" + 1);
            else
                url = await GetRedirectedUrl("https://source.unsplash.com/" + res + "/?" + search + "&sig=" + 1);
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
        }

        async Task<string> GetRedirectedUrl(string url)
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true });
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            return response.RequestMessage.RequestUri.ToString();
        }
    }
}
