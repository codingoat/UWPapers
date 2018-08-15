using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WinSplash.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        int index;
        MainPage mainPage;

        public ImagePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            index = (int) e.Parameter;
        }

        private void flipView_Loaded(object sender, RoutedEventArgs e)
        {
            var windowframe = (Frame)Window.Current.Content;
            mainPage = (MainPage)windowframe.Content;
            flipView.ItemsSource = mainPage.storedImages;
            flipView.SelectedIndex = index;
            flipView.Focus(FocusState.Programmatic);
        }

        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(mainPage.unsplashImages[flipView.SelectedIndex].url);
            Clipboard.SetContent(dataPackage);


            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children ={
                new AdaptiveText(){Text = "Link copied to clipboard"},
                new AdaptiveImage(){Source = mainPage.unsplashImages[flipView.SelectedIndex].url}
            }
                }
            };

            ToastContent toastContent = new ToastContent()
            {
                Visual = visual
            };

            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(10);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            string url = mainPage.unsplashImages[flipView.SelectedIndex].url;
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(url, UriKind.Absolute)));
            Clipboard.SetContent(dataPackage);

            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children ={
                new AdaptiveText(){Text = "Image copied to clipboard"},
                new AdaptiveImage(){Source = url}
            }
                }
            };
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual
            };
            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(10);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void SaveImage(object sender, RoutedEventArgs e)
        {
            string url = mainPage.unsplashImages[flipView.SelectedIndex].url;
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
                        SaveImage(sender, e);
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
                    SaveImage(sender, e);
            }
        }

        private async void SetWallpaper(object sender, RoutedEventArgs e)
        {
            string url = mainPage.unsplashImages[flipView.SelectedIndex].url;
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
    }
}
