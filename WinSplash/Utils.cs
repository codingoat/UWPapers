using Microsoft.Toolkit.Uwp.Notifications;
using PixabaySharp;
using PixabaySharp.Utility;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace WinSplash
{
    public static class Utils
    {
        public static async Task<string> GetRedirectedUrl(string url)
        {
            HttpClient httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true });
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            return response.RequestMessage.RequestUri.ToString();
        }

        public static async Task SaveImage(string url)
        {
            byte[] data;
            string filename = DateTime.Now.ToString();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            string mediaType = response.Content.Headers.ContentType.MediaType.Split('/')[1];
            data = await response.Content.ReadAsByteArrayAsync();
            filename += "." + mediaType;


            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.SuggestedFileName = "unsplash " + DateTime.Now.ToString("d") + "_" + DateTime.Now.Second.ToString();
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBytesAsync(file, data);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Image saved",
                        Content = "Image has been saved to " + file.Path + ".",
                        CloseButtonText = "Ok",
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                }
                else
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Image could not be saved.",
                        CloseButtonText = "Ok",
                        PrimaryButtonText = "Try again"
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                        SaveImage(url);
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Saving canceled.",
                    CloseButtonText = "Ok",
                    PrimaryButtonText = "Try again"
                };
                ContentDialogResult result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                    SaveImage(url);
            }
        }

        public static async Task SetWallpaper(string url)
        {
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
        }

        public static void CopyImage(string url)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(url, UriKind.Absolute)));
            Clipboard.SetContent(dataPackage);
        }

        public static void CopyUrl(string url)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(url);
            Clipboard.SetContent(dataPackage);
        }


        public static void NotifyImage(string title, string imgurl, int lengthSec)
        {
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children ={
                new AdaptiveText(){Text = title},
                new AdaptiveImage(){Source = imgurl}
            }
                }
            };
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual
            };
            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(lengthSec);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


        public static async Task ChangeWallpaper()
        {
            PixabaySharpClient pixabayClient = new PixabaySharpClient("3153915-c1b347f3736d73ef2cd6a0e79");
            ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            string search = (string)roamingSettings.Values["wpTaskSearch"];

            string[] res;
            if (roamingSettings.Values["res"] != null)
                res = (roamingSettings.Values["res"] as string).Split('x');
            else
                res = new string[] { "1920", "1080" };

            string url;

            PixabaySharp.Models.ImageResult result = null;
            if (search == "")
            {
                while (result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Page = 1,
                        PerPage = 200,
                        MinWidth = int.Parse(res[0]),
                        MinHeight = int.Parse(res[1])
                    });
                }

                url = result.Images[new Random().Next(199)].ImageURL;
            }
            else
            {
                while (result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Query = search,
                        Page = 1,
                        PerPage = 200,
                        MinWidth = int.Parse(res[0]),
                        MinHeight = int.Parse(res[1])
                    });
                }

                url = result.Images[new Random().Next(199)].ImageURL;
            }

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
