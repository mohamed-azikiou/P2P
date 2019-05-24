using Android.Content;
using Android.Net.Wifi;
using Android.Net.Wifi.P2p;
using Android.OS;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using Xamarin.Forms;
using static Android.Net.Wifi.P2p.WifiP2pManager;
using Application = Android.App.Application;
using Device = P2PShare.Classes.Device;

namespace P2PShare.Droid.Implementations
{
    //This class is responsible for handeling Android P2P (Initializing, Advertisement, Searching, Connecting)
    static class WifiDirectHandler
    {
        public static readonly WifiP2pManager Manager = (WifiP2pManager)Application.Context.GetSystemService(Context.WifiP2pService);
        public static readonly Channel Channel = Manager.Initialize(Application.Context, Looper.MainLooper, null);

        // Broadcast Receiver for connection state, Peer Disponibility, Information about Connectivity
        private static readonly P2PReceiver Receiver = new P2PReceiver(Manager, Channel);

        //Intent Filter For the Broadcast Receiver above
        private static readonly IntentFilter IntentFilter = new IntentFilter();

        // Connection State Listener Checks if Connection request sent sent successefuly or not Final result is in class P2PReceiver
        private class ConnectActionListener : Java.Lang.Object, IActionListener
        {
            public void OnFailure(WifiP2pFailureReason reason)
            {
                MessagingCenter.Send("P2PShare", "WFDConnectError", "Connection Error: " + reason);
                Toast.MakeText(Application.Context, "Connect failed. Retry.", ToastLength.Short).Show();
                MessagingCenter.Send("P2PShare", "WFDConnectIsEnabled", true);
            }

            public void OnSuccess()
            {
                MessagingCenter.Send("P2PShare", "WFDConnectIsEnabled", true);
            }
        }

        // Peer Discovery Listener checks if wifi discovery started successefuly or not
        private class ActionListener : Java.Lang.Object, IActionListener
        {
            public void OnFailure(WifiP2pFailureReason reason)
            {
                MessagingCenter.Send("P2PShare", "WFDSearchError", " Error: " + reason);
                MessagingCenter.Send("P2PShare", "WFDSearchIsEnabled", true);
            }
            public void OnSuccess()
            {
                MessagingCenter.Send("P2PShare", "WFDSearchIsEnabled", true);
            }
        }

        // Set the intent filter and register the Broadcast receiver to start Discovery 
        public static void Initialize()
        {
            // Indicates a change in the Wi-Fi P2P status.
            IntentFilter.AddAction(WifiP2pStateChangedAction);

            // Indicates a change in the list of available peers.
            IntentFilter.AddAction(WifiP2pPeersChangedAction);

            // Indicates the state of Wi-Fi P2P connectivity has changed.
            IntentFilter.AddAction(WifiP2pConnectionChangedAction);

            // Indicates this device's details have changed.
            IntentFilter.AddAction(WifiP2pThisDeviceChangedAction);

            // Register receiver
            Application.Context.RegisterReceiver(Receiver, IntentFilter);

            // Start Discovery
            GetDevices();
        }

        // Dispose Recources
        public static void Dispose()
        {
            Application.Context.UnregisterReceiver(Receiver);
        }
        
        // Listener for peers It is responsible for adding peers
        public class PeerListener : Java.Lang.Object,IPeerListListener
        {
            public void OnPeersAvailable(WifiP2pDeviceList peers)
            {
                if (peers.DeviceList.Count == 0)
                {
                    return;
                }
                foreach (WifiP2pDevice device in peers.DeviceList) {
                    Send.Devices.Add(new Device(device.DeviceName, device.DeviceAddress));
                }
            }
        }

        // Discover Peer Request Result in PeerListener Class
        public static void GetDevices()
        {
            Manager.DiscoverPeers(Channel,new ActionListener());
        }

        // Set connection configuration and request connection to given MAC address
        public static void Connect(string DeviceAddress)
        {
            WifiP2pConfig config = new WifiP2pConfig { DeviceAddress = DeviceAddress };
            config.Wps.Setup = WpsInfo.Pbc;
            config.GroupOwnerIntent = 0;
            Manager.Connect(Channel, config, new ConnectActionListener());
            //System.Threading.Thread.Sleep(1000);
            //Manager.CancelConnect(Channel, new ConnectActionListener());
        }
    }
}