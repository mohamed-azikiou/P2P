using P2PShare.Classes;
using P2PShare.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace P2PShare
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectPage : ContentPage
	{
        public static List<MyImage> lst = new List<MyImage>();
        // Using RootPage we can call the Main Page
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        // Files contain selected files
        public static ObservableCollection<File> Files { get; set; } = new ObservableCollection<File>();

        public SelectPage()
        {
            // Initialize Components
            InitializeComponent();


            // Set FolderView items Source to Files this will bind properties between them
            FolderView.ItemsSource = Files;
            //DependencyService.Get<IPlatformDependent>().Notify("9/15", "Icon.png", 0.36, "Sending", true);

            // Receive Files
            MessagingCenter.Subscribe("MyApp", "FilesSent", (string sender, List<File> files) =>
            {
                foreach (File f in files)
                {
                    Files.Add(f);
                }
            });
        }

        // When AddBtn Clicked
        private async void AddBtnClicked(object sender, EventArgs e)
        {
            //FolderView.
            popup.Dismiss();

            // Start Picking Files
            var files = await DependencyService.Get<IPlatformDependent>().PickMultipleFiles();

            // If Android files will be null
            if (files != null)
                foreach (File f in files)
                {
                    // Add received File
                    Files.Add(f);
                }
        }

        // When NextButton Clicked Continue to next View
        private void NextButton_Clicked(object sender, EventArgs e)
        {
            RootPage.Detail = new NavigationPage(new Send());
        }

        private void CircleButton_Clicked(object sender, EventArgs e)
        {
            //.ShowRelativeToView(AddBtn,Syncfusion.XForms.PopupLayout.RelativePosition.AlignTop);
            popup.Show(false);
        }

        private void FolderView_SwipeEnded(object sender, Syncfusion.ListView.XForms.SwipeEndedEventArgs e)
        {
            if(e.SwipeOffset>0) Files.RemoveAt(e.ItemIndex);
        }
        int p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5=0;

        private void Image_SizeChanged(object sender, EventArgs e)
        {
        }
    }
}