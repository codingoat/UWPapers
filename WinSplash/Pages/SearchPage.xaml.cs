using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinSplash.Datatypes;
using WinSplash.Pages;
using Windows.UI.Xaml.Media.Animation;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using Microsoft.QueryStringDotNET; // QueryString.NET
using Windows.Storage.Streams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WinSplash
{
    public sealed partial class SearchPage : Page
    {
        public ObservableCollection<UnsplashImage> images = new ObservableCollection<UnsplashImage>();
        ObservableCollection<int> imageNum = new ObservableCollection<int>();
        ObservableCollection<ImageOption> imageOptions = new ObservableCollection<ImageOption>();

        MainPage mainPage;

        ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        string res;
        int selectedImage = 0;
        FrameworkElement flyoutBase;

        public SearchPage()
        {
            this.InitializeComponent();

            imageNum.Add(5);
            imageNum.Add(10);
            imageNum.Add(20);
            imageNum.Add(50);
            imageNum.Add(100);
            ImageNumBox.SelectedIndex = 0;

            imageOptions.Add(new ImageOption("&#xE74E;", "Save Image"));
            imageOptions.Add(new ImageOption("&#xE8C8;", "Copy Image"));
            imageOptions.Add(new ImageOption("&#xE71B;", "Copy Link"));

            var windowframe = (Frame)Window.Current.Content;
            mainPage = (MainPage)windowframe.Content;

            if (roamingSettings.Values["res"] != null)
                res = roamingSettings.Values["res"] as string;
            else
                res = "1920x1080";

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500,500));
        }


        public async void GetImages()
        {
            Spinner.IsActive = true;
            images = new ObservableCollection<UnsplashImage>();
            ImageGrid.ItemsSource = images;

            Debug.WriteLine("started downloading" + DateTime.Now);
            images = await AddImages();

            //ImageGrid.ItemsSource = images;
            Spinner.IsActive = false;
        }

        async Task<ObservableCollection<UnsplashImage>> AddImages (){
            ObservableCollection<UnsplashImage> myImages = new ObservableCollection<UnsplashImage>();

            int amount;
            string search = SearchBox.Text;

            if (ImageNumBox.SelectedIndex != -1)
                amount = imageNum[ImageNumBox.SelectedIndex];
            else amount = 5;

            for (int i = 0; i < amount; i++)
            {
                string url;
                if (search == "")
                    url = await GetRedirectedUrl("https://source.unsplash.com/random/" + res + "?sig=" + i);
                else
                    url = await GetRedirectedUrl("https://source.unsplash.com/" + res + "/?" + search + "&sig=" + i);
                Debug.WriteLine(DateTime.Now + url);
                myImages.Add(new UnsplashImage(i, url));

                images.Add(new UnsplashImage(i, url));
                ImageGrid.ItemsSource = images;
            }
            images = myImages;
            return myImages;
        }

        async Task<string> GetRedirectedUrl(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "HEAD";
            req.AllowAutoRedirect = true;

            WebResponse wr = await req.GetResponseAsync();
            return  wr.ResponseUri.ToString();
        }

        public void Refresh(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            if (images.Count == 0 && mainPage.unsplashImages.Count != 0) //first load
            {
                images = mainPage.unsplashImages;
                ImageGrid.ItemsSource = images;
                Spinner.IsActive = false;
            }
            else GetImages();
        }

        public void Search(object sender, RoutedEventArgs e)
        {
            GetImages();
        }


        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                GetImages();
        }

        private void ImageClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Image img = VisualTreeHelper.GetChild(btn, 0) as Image;
            List<Image> flipimgs = new List<Image>();

            StackPanel btnstck = (StackPanel)btn.Content;
            Image btnimg = (Image)VisualTreeHelper.GetChild(btnstck, 0);
            //int tindex = 0;
            //int index = 0;

            foreach (UnsplashImage ui in images)
            {
                Image timg = new Image();
                timg.Source = new BitmapImage(new Uri(ui.url, UriKind.Absolute));
                flipimgs.Add(timg);

                /*if ((string)btnimg.Tag == ui.url)
                    index = tindex;
                tindex++;*/
            }

            Debug.WriteLine("selected " + btn.Tag);

            mainPage.unsplashImages = images;
            mainPage.storedImages = flipimgs;
            mainPage.theNavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            mainPage.theNavView.IsBackEnabled = true;
            mainPage.theContentFrame.BackStack.Clear();
            mainPage.theContentFrame.BackStack.Add(new PageStackEntry(typeof(SearchPage), null, new DrillInNavigationTransitionInfo()));
            mainPage.theContentFrame.Navigate(typeof(ImagePage), (int) btn.Tag);
        }

        private void ImageClickR(object sender, RightTappedRoutedEventArgs e)
        {
;
            Button btn = (Button)sender;
            selectedImage = (int) btn.Tag;
            //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            //flyoutBase = (FrameworkElement)sender;
        }


        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            string url = images[selectedImage].url;
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(url);
            Clipboard.SetContent(dataPackage);

            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual(){BindingGeneric = new ToastBindingGeneric(){Children ={
                new AdaptiveText(){Text = "Link copied to clipboard"},
                new AdaptiveImage(){Source = url}
            }}};
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual
            };
            var toast = new ToastNotification(toastContent.GetXml());
            toast.ExpirationTime = DateTime.Now.AddSeconds(10);
            ToastNotificationManager.CreateToastNotifier().Show(toast);


            //await Windows.System.Launcher.LaunchUriAsync(new Uri(btn.Tag.ToString(), UriKind.Absolute));
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            string url = images[selectedImage].url;
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
            string url = images[selectedImage].url;
            byte[] data;
            string filename = DateTime.Now.ToString();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));
            string mediaType = response.Content.Headers.ContentType.MediaType.Split('/')[1];
            data = await response.Content.ReadAsByteArrayAsync();
            filename += "." + mediaType;


            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.SuggestedFileName = "unsplash " + DateTime.Now.ToShortTimeString() + "_" + DateTime.Now.Second.ToString();
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });

            StorageFile file = await savePicker.PickSaveFileAsync();
            if(file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                await FileIO.WriteBytesAsync(file, data);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
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

        /*private void ImageOptionClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase f = FlyoutBase.GetAttachedFlyout(flyoutBase);

            Flyout.

            switch (e.ClickedItem.ToString())
            {
                case "Copy Link":
                    CopyUrl(images[selectedImage].url);
                    break;
                case "Copy Image":
                    CopyUrl(images[selectedImage].url);
                    break;
                case "Save Image":
                    SaveImage(images[selectedImage].url);
                    break;
            }
        }*/
    }
}
