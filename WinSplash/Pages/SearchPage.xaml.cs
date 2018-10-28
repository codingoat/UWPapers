using Microsoft.Toolkit.Uwp.UI.Animations;
using PixabaySharp;
using PixabaySharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinSplash.Datatypes;
using WinSplash.Pages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WinSplash
{
    public sealed partial class SearchPage : Page
    {
        public ObservableCollection<PixaImage> images = new ObservableCollection<PixaImage>();
        ObservableCollection<int> imageNum = new ObservableCollection<int>();
        ObservableCollection<ImageOption> imageOptions = new ObservableCollection<ImageOption>();
        PixabaySharpClient pixabayClient = new PixabaySharpClient("3153915-c1b347f3736d73ef2cd6a0e79");

        MainPage mainPage;

        ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        string res;
        int selectedImage = 0;

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

            if (roamingSettings.Values["amount"] != null)
            {
                ImageNumBox.SelectedIndex = (int)roamingSettings.Values["amount"];
                Debug.WriteLine("selected in numbox " + (int)roamingSettings.Values["amount"]);
            }
            else Debug.WriteLine("no amount selected");
        }


        public async void GetImages()
        {
            Spinner.IsActive = true;
            images = new ObservableCollection<PixaImage>();
            ImageGrid.ItemsSource = images;

            Debug.WriteLine("started downloading" + DateTime.Now);
            //images = await AddImages();
            await AddImages();

            //ImageGrid.ItemsSource = images;
            Spinner.IsActive = false;
        }

        async Task<int> AddImages()
        {
            //ObservableCollection<PixaImage> myImages = new ObservableCollection<PixaImage>();
            int amount;
            string search = SearchBox.Text;

            if (ImageNumBox.SelectedIndex != -1)
                amount = imageNum[ImageNumBox.SelectedIndex];
            else amount = 5;

            PixabaySharp.Models.ImageResult result = null;
            images = new ObservableCollection<PixaImage>();
            if (search == "")
            {
                while(result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Page = 1,
                        PerPage = 200
                    });
                }

                Random r = new Random();
                int[] pickedImgs = {};

                for (int i=0; i<amount; i++)
                {
                    int ran = r.Next(199);
                    images.Add(new PixaImage(i, result.Images[ran].ImageURL, result.Images[ran].WebformatURL, result.Images[ran].FullHDImageURL, result.Images[ran].PageURL));
                    ImageGrid.ItemsSource = images;
                }

            }
            else
            {
                while (result == null) //the library sometimes returns null for some reason
                {
                    result = await pixabayClient.QueryImagesAsync(new ImageQueryBuilder()
                    {
                        Query = search,
                        Page = 1,
                        PerPage = amount
                    });
                }
                
                int i = 0;
                foreach (PixabaySharp.Models.ImageItem img in result.Images)
                {
                    images.Add(new PixaImage(i++, img.ImageURL, img.WebformatURL, img.FullHDImageURL, img.PageURL));
                    ImageGrid.ItemsSource = images;
                }
            }
            return 0;

        }

        public void Refresh(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            if (images.Count == 0 && mainPage.pixaImages.Count != 0) //first load
            {
                images = mainPage.pixaImages;
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

            foreach (PixaImage ui in images) //sends bitmap images to FlipView
            {
                Image timg = new Image();
                timg.Source = new BitmapImage(new Uri(ui.bigUrl, UriKind.Absolute));
                flipimgs.Add(timg);
            }

            Debug.WriteLine("selected " + btn.Tag);

            mainPage.pixaImages = images;
            mainPage.storedImages = flipimgs;
            mainPage.theNavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            mainPage.theNavView.IsBackEnabled = true;
            mainPage.theContentFrame.BackStack.Clear();
            mainPage.theContentFrame.BackStack.Add(new PageStackEntry(typeof(SearchPage), null, new DrillInNavigationTransitionInfo()));
            mainPage.theContentFrame.Navigate(typeof(ImagePage), (int) btn.Tag);
        }

        private void ImageClickR(object sender, RightTappedRoutedEventArgs e)
        {
            Button btn = (Button)sender;
            selectedImage = (int) btn.Tag;
        }


        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            Utils.CopyUrl(images[selectedImage].url);
            Utils.NotifyImage("Link copied to clipboard", images[selectedImage].url, 10);

            //await Windows.System.Launcher.LaunchUriAsync(new Uri(btn.Tag.ToString(), UriKind.Absolute)); //open browser
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            Utils.CopyImage(images[selectedImage].url);
            Utils.NotifyImage("Image copied to clipboard", images[selectedImage].url, 10);
        }

        private async void SaveImage(object sender, RoutedEventArgs e)
        {
            await Utils.SaveImage(images[selectedImage].url);
        }

        private async void SetWallpaper(object sender, RoutedEventArgs e)
        {
            await Utils.SetWallpaper(images[selectedImage].url);
        }

        private void ImageNumBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            roamingSettings.Values["amount"] = ImageNumBox.SelectedIndex;
            Debug.WriteLine("amount saved " + ImageNumBox.SelectedIndex);
        }

        private async void FadeInImage(object sender, RoutedEventArgs e)
        {
            Image imgw = (Image)sender;
            await imgw.Fade(value: 1, duration: 300, delay: 300, easingType: EasingType.Default).StartAsync();
            //await Utils.SaveImage(images[selectedImage].url);
        }
    }
}
