using P2PShare.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace P2PShare
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Receive : ContentPage
	{
        // Using RootPage we can call the Main Page
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        // Constructor and initializer
		public Receive ()
		{
            // Initialize Components 
            InitializeComponent();
            Task.Run(() =>
            {
                // When connected to Wifi Direct launch Sending Page
                MessagingCenter.Subscribe("P2PShare", "WFDConnected", (Action<string, string>)((sender, ip) =>
                {
                    var socket = new Classes.ServerSocketHandler();
                    socket.ReadyChanged += (object o, EventArgs ev) =>
                        Device.BeginInvokeOnMainThread(() => {  RootPage.Detail = new Sending(socket); });
                }));

                /* We have to dispose recources before we initialize because 
                it is forbidden to start publishing twice we have to stop publishing then relaunch it */
                DependencyService.Get<IPlatformDependent>().Dispose();
                DependencyService.Get<IPlatformDependent>().Init(true);
            });
        }
	}
}