using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using P2PShare.Views;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace P2PShare
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(@"OTY1NTdAMzEzNzJlMzEyZTMwVDRGWDBQdlRyNERzbU04aSt2NHVwNUNBYmVIOXhiRVZ3eElqRDdLWEYydz0=");
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
