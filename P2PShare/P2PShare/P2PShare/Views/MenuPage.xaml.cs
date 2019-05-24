using P2PShare.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace P2PShare.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        // Using RootPage we can call the Main Page
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        // Master Items
        List<HomeMenuItem> menuItems;

        // Constructor
        public MenuPage()
        {
            InitializeComponent();

            // Add menu items
            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Main, Title="Browse" },
                new HomeMenuItem {Id = MenuItemType.Send, Title="Send" },
                new HomeMenuItem {Id = MenuItemType.Receive, Title="Receive" },
                new HomeMenuItem {Id = MenuItemType.About, Title="About" }
            };

            // Bind ListViewMenu binding properties with HomeMenuItem And add them to the ListView 
            ListViewMenu.ItemsSource = menuItems;

            // Make the first item selected by default
            ListViewMenu.SelectedItem = menuItems[0];

            // Handle Click Event
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                // if error
                if (e.SelectedItem == null) return;

                // Handle each Menu item case
                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };
        }
    }
}