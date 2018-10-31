using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WinSplash.Pages
{
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
            flipView.Focus(FocusState.Programmatic); //TODO: fix focus
        }

        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            Utils.CopyUrl(mainPage.pixaImages[flipView.SelectedIndex].url);
            Utils.NotifyImage("Link copied to clipboard", mainPage.pixaImages[flipView.SelectedIndex].smallUrl, 10);
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            Utils.CopyImage(mainPage.pixaImages[flipView.SelectedIndex].url);
            Utils.NotifyImage("Image copied to clipboard", mainPage.pixaImages[flipView.SelectedIndex].smallUrl, 10);
        }

        private async void SaveImage(object sender, RoutedEventArgs e)
        {
            await Utils.SaveImage(mainPage.pixaImages[flipView.SelectedIndex].url);
        }

        private async void SetWallpaper(object sender, RoutedEventArgs e)
        {
            await Utils.SetWallpaper(mainPage.pixaImages[flipView.SelectedIndex].url);
        }
    }
}
