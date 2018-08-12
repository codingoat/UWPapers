using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinSplash.Datatypes;
using WinSplash.Pages;
using MUXC = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinSplash
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IList<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("search", typeof(SearchPage)),
            ("wallpaper", typeof(WallpaperPage))
        };


        public NavigationView theNavView;
        public Frame theContentFrame;

        public ObservableCollection<UnsplashImage> unsplashImages = new ObservableCollection<UnsplashImage>();
        public List<Image> storedImages = new List<Image>();


        public MainPage()
        {
            this.InitializeComponent();
            CoreWindow.GetForCurrentThread().KeyDown += MyPage_KeyDown;
        }

        void MyPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if(args.VirtualKey == VirtualKey.Escape)
                On_BackRequested();
        }

        public void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigated += On_Navigated;
            NavView_Navigate("search");

            NavView.Header = "Browse";

            /*var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);*/

            theNavView = NavView;
            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            theContentFrame = ContentFrame;

            //titlebar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            //TopBar.Height = coreTitleBar.Height;
            Window.Current.SetTitleBar(TopBar);

        }

        private void NavView_Navigate(string navItemTag)
        {
            var item = _pages.First(p => p.Tag.Equals(navItemTag));
            ContentFrame.Navigate(item.Page);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if(ContentFrame.CurrentSourcePageType == typeof(SearchPage)) //save loaded images
            {
                SearchPage sp = (SearchPage)ContentFrame.Content;
                unsplashImages = sp.images;
            }


            if (args.IsSettingsInvoked)
                ContentFrame.Navigate(typeof(SettingsPage));
            else
            {
                // Getting the Tag from Content (args.InvokedItem is the content of NavigationViewItem)
                var navItemTag = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(i => args.InvokedItem.Equals(i.Content))
                    .Tag.ToString();

                NavView_Navigate(navItemTag);

                if (navItemTag == "search")
                    NavView.Header = "Browse";
                if (navItemTag == "wallpaper")
                    NavView.Header = "Set Wallpaper";
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
            Debug.WriteLine("virtual back");
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            Debug.WriteLine("back pressed");
            /*if ((string)NavView.Header == "Browse")
            {
                Debug.WriteLine("wanna close flip");
                SearchPage sp = (SearchPage)ContentFrame.Content;
                if(sp.theFlipView.Visibility == Visibility.Visible)
                {
                    sp.theFlipView.Visibility = Visibility.Collapsed;
                    NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
                    Debug.WriteLine("closed flip");
                }
            }*/

            if (ContentFrame.CurrentSourcePageType == typeof(ImagePage))
            {
                Debug.WriteLine("wanna close flip");
                ContentFrame.GoBack();
                NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            }

            return true;
            /*if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;*/
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            }
            else if (ContentFrame.SourcePageType == typeof(ImagePage))
            {
                //do nothing
            }
            else
            {
                var item = _pages.First(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

    }
}
