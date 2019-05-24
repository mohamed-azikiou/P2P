using Microsoft.Toolkit.Uwp.Notifications;
using P2PShare.UWP.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformDependent))]
namespace P2PShare.UWP.Implementations
{
    class PlatformDependent : IPlatformDependent
    {
        private string NotificationTag = "progress";
        private readonly string NotificationGroup = "P2PShare";
        ToastNotification Notification = null;
        private ToastContent ToastContent = null;

        private static WiFiDirectAdvertisementPublisher publisher = null;

        // Pick Multiple Files
        public async Task<List<Classes.File>> PickMultipleFiles()
        {
            // Files will store Files
            var Files = new List<Classes.File>();

            // Creating a file picker and setting preferences
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail, // Parameter
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary // Parameter
            };

            // Picking all file types
            picker.FileTypeFilter.Add("*"); // Parameter

            // Start Picking and wait for result
            var res = await picker.PickMultipleFilesAsync();

            // Open Temp StorageFolder
            StorageFolder Temp = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists); // Parameter

            // Adding Files
            foreach (StorageFile sf in res)
            {
                System.Diagnostics.Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                try
                {
                    // Get file icon and convert it from Thumbnail to ImageSource by saving it as a Temp file
                    var v =(await sf.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.ListView)).AsStream();
                    var IconName = sf.DisplayName;
                    Stream fs = null;
                    while (true)
                        try
                        {
                            fs = await (await Temp.CreateFileAsync(IconName + ".png", CreationCollisionOption.ReplaceExisting)).OpenStreamForWriteAsync();
                            break;
                        } catch (Exception) {
                            IconName += "_1";
                        }
                        
                    v.CopyTo(fs);
                    v.Dispose();
                    fs.Dispose();
                    // Open FileStream
                    var stream = await sf.OpenStreamForReadAsync();
                
                    // Create a file and add it to Files
                    Files.Add(new Classes.File(sf.Name, stream, Temp.Path + @"\" + IconName + ".png", stream.Length));
                } catch (Exception) { }
            }

            // Returning result
            return Files;
        }

        public void FindWFDDevices()
        {
            WifiDirectHandler.GetDevices();
        }

        public async void Connect(string DeviceID, bool IsReceiver)
        {
            await WifiDirectHandler.ConnectAsync(DeviceID, IsReceiver);
        }

        // Initialize advertizing and if Receiver handle connetion request
        public void Init(bool IsReceiver)
        {
            // Create an instance of Wifi Direct advertiser
            if (publisher == null) publisher = new WiFiDirectAdvertisementPublisher();

            // Listen to connection request if receiver
            if (IsReceiver)
            {
                WiFiDirectConnectionListener listener = new WiFiDirectConnectionListener();
                listener.ConnectionRequested += (WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs connectionEventArgs) =>
                {
                    // Because HandleConnectionRequestAsync generates a connection dialog window It request UI Thread
                    WiFiDirectConnectionRequest connectionRequest = connectionEventArgs.GetConnectionRequest();
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                    {
                        // Request a connection to given device
                        var id = connectionRequest.DeviceInformation.Id;
                        //connectionRequest.Dispose();
                        await WifiDirectHandler.ConnectAsync(id, IsReceiver);
                    });
                };
            }
            // Start advertisement with Intensive parameter so that WifiDirect stay enabled even if app is in background
            publisher.Advertisement.ListenStateDiscoverability = WiFiDirectAdvertisementListenStateDiscoverability.Intensive;
            publisher.Start();
        }

        // Stop advertizing
        public void Dispose()
        {
            if (publisher != null) publisher.Stop();
        }

        // Create a File
        public async Task<object> CreateFileAsync(string path, string fileName)
        {
            // Get StorageFolder from path then create the file
            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            StorageFile storageFile = await folder.CreateFileAsync(fileName,CreationCollisionOption.ReplaceExisting); // Parameter
            var fStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            var stream = fStream.AsStream();
            return stream;
        }

        // Get Default Path
        public string GetPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        // Check if storage Permission enabled
        public async Task<bool> CheckPermissionAsync()
        {
            try
            {
                await StorageFolder.GetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<bool> RequestPermissionAsync()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));
            return await CheckPermissionAsync();
        }

        public List<Classes.File> GetApps()
        {
            return new List<Classes.File>();
        }

        public void Notify(string FilesProgress, string FileName , double DataProgress, Classes.TransferState.State State)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            if (Notification == null) {
                ToastContent = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            AppLogoOverride = new ToastGenericAppLogo()
                            {
                                Source = "xamarin_logo.png",
                                HintCrop = ToastGenericAppLogoCrop.Circle
                            },

                            Children = {
                                new AdaptiveText()
                                {
                                    Text = State.ToString(),
                                    HintMaxLines = 1
                                },

                                new AdaptiveProgressBar()
                                {
                                    Title = new BindableString("FileName"),
                                    Value = new BindableProgressBarValue("DataProgress"),
                                    Status = new BindableString("status"),
                                    ValueStringOverride = new BindableString("FilesProgress")
                                }
                            }
                        }
                    }
                };
                Notification = new ToastNotification(ToastContent.GetXml())
                {
                    Priority = ToastNotificationPriority.High,
                    Tag = NotificationTag,
                    Group = NotificationGroup
                };
                Notification.Dismissed += Dismiss;
                Notification.Data = new NotificationData();
                Notification.Data.Values["FilesProgress"] = FilesProgress;
                Notification.Data.Values["FileName"] = FileName;
                Notification.Data.Values["DataProgress"] = DataProgress.ToString();
                Notification.Data.Values["status"] = State.ToString();
                Notification.Data.SequenceNumber = 0;
                toastNotifier.Show(Notification);
            } else {
                Notification.Data.Values["FilesProgress"] = FilesProgress;
                Notification.Data.Values["FileName"] = FileName;
                Notification.Data.Values["DataProgress"] = DataProgress.ToString();
                Notification.Data.Values["status"] = State.ToString();
                toastNotifier.Update(Notification.Data, NotificationTag, NotificationGroup);
            }
        }

        private void Dismiss(ToastNotification n, ToastDismissedEventArgs o)
        {
            ToastNotificationManager.CreateToastNotifier().Update(Notification.Data, NotificationTag);
            /*NotificationTag += "_";
            Notification = new ToastNotification(ToastContent.GetXml())
            {
                Priority = ToastNotificationPriority.High,
                Tag = NotificationTag,
                Group = NotificationGroup
            };
            Notification.SuppressPopup = true;
            Notification.Dismissed += Dismiss;
            ToastNotificationManager.CreateToastNotifier().Show(Notification);*/
        }
    }
}
