using System;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.UI.Core;
using Xamarin.Forms;
using Device = P2PShare.Classes.Device;

namespace P2PShare.UWP.Implementations
{
    static class WifiDirectHandler
    {
        // Connection attempt counter
        //private static int ConnectionAttempt = 50;

        //static DeviceInformationCollection devInfoCollection;
        private static void OnConnectionChanged(object sender, object arg)
        {
            WiFiDirectConnectionStatus status = (WiFiDirectConnectionStatus)arg;

            if (status == WiFiDirectConnectionStatus.Connected)
            {
                // Connection successful.
            }
            else
            {
                // Disconnected.
            }
        }

        public static async void GetDevices()
        {
            string deviceSelector = WiFiDirectDevice.GetDeviceSelector(WiFiDirectDeviceSelectorType.AssociationEndpoint);
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(deviceSelector);
            foreach (DeviceInformation device in devices) { Send.Devices.Add(new Device(device.Name, device.Id)); }
            MessagingCenter.Send("P2PShare", "WFDSearchIsEnabled", true);
        }
        public static async System.Threading.Tasks.Task ConnectAsync(string deviceId, bool IsReceiver)
        {
            try
            {
                WiFiDirectConnectionParameters connectionParams = new WiFiDirectConnectionParameters();
                WiFiDirectDevice wfdDevice = null;
                connectionParams.GroupOwnerIntent = (short)(IsReceiver ? 15 : 0);
                connectionParams.PreferredPairingProcedure = WiFiDirectPairingProcedure.GroupOwnerNegotiation;
                connectionParams.PreferenceOrderedConfigurationMethods.Clear();
                connectionParams.PreferenceOrderedConfigurationMethods.Add(WiFiDirectConfigurationMethod.PushButton);
                wfdDevice = await WiFiDirectDevice.FromIdAsync(deviceId, connectionParams);
                // Register for the ConnectionStatusChanged event handler
                wfdDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
                var endpointPairs = wfdDevice.GetConnectionEndpointPairs();
                MessagingCenter.Send("P2PShare", "WFDConnectError", "Connected IP Address: " + endpointPairs[0].RemoteHostName);
                MessagingCenter.Send("P2PShare", "WFDConnected", endpointPairs[0].RemoteHostName.ToString());
            }
            catch (Exception)
            {
                /*MessagingCenter.Send("P2PShare", "WFDConnectError", "Connection to " + deviceId + " failed " + err.Message);
                DependencyService.Get<IPlatformDependent>().Dispose();
                DependencyService.Get<IPlatformDependent>().Init(true);
                System.Threading.Thread.Sleep(1000);

                if (--ConnectionAttempt > 0)
                    await ConnectAsync(deviceId, IsReceiver);
                else
                {
                    ConnectionAttempt = 50;
                }*/
            }
            MessagingCenter.Send("P2PShare", "WFDConnectIsEnabled", true);
        }
        static private void OnConnectionStatusChanged(WiFiDirectDevice sender, object arg)
        {
            //rootPage.NotifyUserFromBackground("Connection status changed: " + sender.ConnectionStatus, NotifyType.StatusMessage);
        }
    }
}
