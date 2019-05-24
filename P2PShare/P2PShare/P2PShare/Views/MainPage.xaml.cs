using P2PShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace P2PShare.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        // MenuPages contains opened Menu Pages
        static public readonly Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();

        // Constructor
        public MainPage()
        {
            // Initialize Components
            InitializeComponent();

            // When MasterPage get opened it will pop over DetailPage
            MasterBehavior = MasterBehavior.Popover;

            // Add default Detail Page
            MenuPages.Add((int)MenuItemType.Main, (NavigationPage)Detail);
        }

        public async Task NavigateFromMenu(int id)
        {
            // If DetailPage heven't been opened before
            if (!MenuPages.ContainsKey(id))
            {
                // Create DetailPage according to the ID
                switch (id)
                {
                    case (int)MenuItemType.Main:
                        MenuPages.Add(id, new NavigationPage(new MainViewPage()));
                        break;
                    case (int)MenuItemType.Send:
                        MenuPages.Add(id, new NavigationPage(new SelectPage()));
                        break;
                    case (int)MenuItemType.Receive:
                        MenuPages.Add(id, new NavigationPage(new Receive()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}