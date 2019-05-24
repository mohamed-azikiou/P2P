using P2PShare.Views;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Device = P2PShare.Classes.Device;

namespace P2PShare
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Send : ContentPage
	{
        // Set to true if connection process started else false
        public bool IsConnecting = false;

        // Set to true if shearhing process started else false
        public bool IsSearching = false;

        // Found devices list
        public static ObservableCollection<Device> Devices { get; set; } = new ObservableCollection<Device>();

        // Using RootPage we can call the Main Page
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        // Constructor and initializer
        public Send ()
		{
            // initialize Components 
			InitializeComponent ();

            // Receiving Errors
            MessagingCenter.Subscribe<string,string>("P2PShare", "WFDConnectError", (sender,arg)=> { Lbl.Text = Lbl.Text + arg + ' '; });
            MessagingCenter.Subscribe<string,string>("P2PShare", "WFDSearchError", (sender,arg)=> { Lbl.Text = arg + ' '; } );

            // When connected to Wifi Direct launch Sending Page
            MessagingCenter.Subscribe<string, string>("P2PShare", "WFDConnected", (Action<string, string>)((sender, ip) =>
            {
                var socket = new Classes.ClientSocketHandler(ip);

                socket.ReadyChanged += (object o, global::System.EventArgs e) =>
                {
                    global::Xamarin.Forms.Device.BeginInvokeOnMainThread((global::System.Action)(() => { this.RootPage.Detail = new global::P2PShare.Sending((global::P2PShare.Classes.SocketHandler)socket); }));

                };
            }));

            // When Clicked on connect disable Find and Search buttons
            // When connecting is done reenable Find and Search buttons
            MessagingCenter.Subscribe<string,bool>("P2PShare", "WFDConnectIsEnabled", (sender,arg)=> {
                if (arg)
                {
                    // Enable Find and Search buttons if not searhing else enable connect button only
                    if (IsSearching) ConnectBtn.IsEnabled = true;
                    else
                        FindBtn.IsEnabled =
                        ConnectBtn.IsEnabled = true;
                    IsConnecting = false;
                } else {
                    // Disable Find and Search buttons
                    FindBtn.IsEnabled =
                    ConnectBtn.IsEnabled = false;
                    IsConnecting = true;
                }
            } );

            // When Clicked on search disable Search buttons
            // When searching is done reenable Search button if not connecting
            MessagingCenter.Subscribe<string,bool>("P2PShare", "WFDSearchIsEnabled", (sender, arg) => {
                if (arg)
                {
                    // Enable Search button if not connecting
                    if (!IsConnecting)
                        FindBtn.IsEnabled = true;
                    IsSearching = false;
                }
                else
                {
                    // Disable Searchbutton
                    IsSearching = true;
                    FindBtn.IsEnabled = false;
                }
            });

            /* We have to dispose recources before we initialize because 
            it is fobidden to start publishing twice we have to stop publishing then relaunch it */
            DependencyService.Get<IPlatformDependent>().Dispose();
            DependencyService.Get<IPlatformDependent>().Init(false);

            // List devices in a list view
            LstView.ItemsSource = Devices;
        }

        // Find button clicked Event handler
        private void FindBtnClicked(object sender, EventArgs e)
        {
            // Clear found Devices
            Devices.Clear();

            // Disable search button
            MessagingCenter.Send("P2PShare", "WFDSearchIsEnabled", false);

            // Launch finding
            DependencyService.Get<IPlatformDependent>().FindWFDDevices();
        }

        // Connect button clicked Event handler
        private void ConnecBtnClicked(object sender, EventArgs e)
        {
            // Disable search button
            MessagingCenter.Send("P2PShare", "WFDConnectIsEnabled", false);

            try {
                // Get selected Device if null Exception will be generated
                Device selected =(Device) LstView.SelectedItem;

                // Connect to seleted device
                DependencyService.Get<IPlatformDependent>().Connect(selected.ID,false);
            } catch {
                // Notify that no device is selected
                MessagingCenter.Send("P2PShare", "WFDConnectError","Please select a device");

                // Enable buttons
                MessagingCenter.Send("P2PShare", "WFDConnectIsEnabled", true);
            }
        }
    }
}