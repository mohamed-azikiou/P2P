using Android.Content;
using Android.Net.Wifi.P2p;
using Xamarin.Forms;
using static Android.Net.Wifi.P2p.WifiP2pManager;

namespace P2PShare.Droid.Implementations
{
    class ConnectionInfoListener : Java.Lang.Object,IConnectionInfoListener
    {
        public void OnConnectionInfoAvailable(WifiP2pInfo info)
        {
            if(info.GroupFormed)
                if (info.IsGroupOwner)
                    MessagingCenter.Send("P2PShare", "WFDConnected",(string) null);
                    
                else
                    MessagingCenter.Send("P2PShare", "WFDConnected", info.GroupOwnerAddress.HostAddress);
        }
    }
    [BroadcastReceiver]
    public class P2PReceiver : BroadcastReceiver
    {
        readonly WifiP2pManager manager;
        readonly Channel channel;
        public P2PReceiver() { }
        public P2PReceiver(WifiP2pManager manager, Channel channel) { this.manager = manager; this.channel = channel; }
        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            if (WifiP2pStateChangedAction.Equals(action))
            {
                // Determine if Wifi P2P mode is enabled or not, alert
                // the Activity.
                int state = intent.GetIntExtra(ExtraWifiState, -1);
                if (WifiP2pState.Enabled.Equals(state))
                {
                    MessagingCenter.Send("P2PShare", "WFDConnectError", "Wifi is on");
                    //activity.setIsWifiP2pEnabled(true);
                }
                else
                {
                    MessagingCenter.Send("P2PShare", "WFDConnectError", "Wifi is off");
                    //activity.setIsWifiP2pEnabled(false);
                }
            }
            else if (WifiP2pPeersChangedAction.Equals(action))
            {
                // The peer list has changed!
                Send.Devices.Clear();
                // Requesting Peers The result will be received via the PeerListener Class
                WifiDirectHandler.Manager.RequestPeers(WifiDirectHandler.Channel,new WifiDirectHandler.PeerListener());
            }
            else if (WifiP2pConnectionChangedAction.Equals(action))
            {
                // Connection state changed!
                manager.RequestConnectionInfo(channel,new ConnectionInfoListener());
            }
            else if (WifiP2pThisDeviceChangedAction.Equals(action))
            {
                /*DeviceListFragment fragment = (DeviceListFragment)activity.getFragmentManager()
                        .findFragmentById(R.id.fraglist);
                fragment.updateThisDevice((WifiP2pDevice)intent.getParcelableExtra(
                        WifiP2pManager.EXTRAWIFIP2PDEVICE));*/

            }
            //Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
        }
    }
}