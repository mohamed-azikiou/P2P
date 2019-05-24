using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using P2PShare.Droid;
using P2PShare.Droid.Implementations;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Application = Android.App.Application;
using Permission = Plugin.Permissions.Abstractions.Permission;
using File = P2PShare.Classes.File;
using Android.Support.V4.App;

[assembly: Dependency(typeof(PlatformDependent))]
namespace P2PShare.Droid
{
    [Activity(Label = "PlatformDependent")]
    public class PlatformDependent : Java.Lang.Object, IPlatformDependent
    {
        // File(s) Picker
        async Task<List<File>> IPlatformDependent.PickMultipleFiles()
        {
            try
            {
                // Check Storage Permission
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    // Request Permition from User
                    await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                }

                if (status == PermissionStatus.Granted)
                {
                    // Crearing File Picker Intent 
                    var fileIntent = new Intent(Intent.ActionPick);

                    // Pick all file types
                    fileIntent.SetType("*/*");

                    // Allow Multiple File Picking
                    fileIntent.PutExtra(Intent.ExtraAllowMultiple, true);

                    // Setting Intent for picking files
                    fileIntent.SetAction(Intent.ActionGetContent);

                    // Start Intent from Context
                    // 1 is Request Code it identifies result
                    ((Activity)Forms.Context).StartActivityForResult(Intent.CreateChooser(fileIntent, "Select files"), 1);
                }
                else if (status != PermissionStatus.Unknown)
                {
                    Toast.MakeText(Application.Context, "Permission Denied. Can not continue, try again.", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context,": " +ex.Message, ToastLength.Long).Show();
            }
            // Result is in MainActivity.cs in OnActivityResult
            return null;
        }

        // Start Searching for WFDDevices
        public void FindWFDDevices()
        {
            WifiDirectHandler.GetDevices();
        }

        // Start connecting
        public void Connect(string DeviceID,bool IsReceiver)
        {
            WifiDirectHandler.Connect(DeviceID);
        }

        // Initialise WFD
        public void Init(bool IsReceiver)
        {
            WifiDirectHandler.Initialize();
        }

        public async Task<object> CreateFileAsync(string path, string fileName)
        {
            FileStream fs = null;
            await Task.Run(() => { fs=System.IO.File.Create(GetPath() + Java.IO.File.Separator + fileName); });
            return fs;
        }

        public string GetPath() => Android.OS.Environment.ExternalStorageDirectory.Path;

        [Obsolete]
        public List<File> GetApps()
        {
            var pm = ((Activity)Forms.Context).PackageManager;
            var app=pm.GetInstalledApplications(PackageInfoFlags.Activities);
            List<File> apps=new List<File>();
            foreach (var s in app) {
                // Get app Name
                var appName = s.LoadLabel(pm);

                // Get FileStream
                var apk = System.IO.File.OpenRead(s.SourceDir);

                // Create returning file and add it to apps
                apps.Add(new File(appName, apk, Convertor.DrawabeToTempFile(s.LoadIcon(pm), appName, false),apk.Length));
            }
            pm.Dispose();
            return apps;
        }

        public async Task<bool> CheckPermissionAsync()
        {
            return await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage)==PermissionStatus.Granted;
        }

        public async Task<bool> RequestPermissionAsync()
        {
            var p = new Permission[1];
            p[0] = Permission.Storage;
            await CrossPermissions.Current.RequestPermissionsAsync(p);
            return await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage) == PermissionStatus.Granted;
        }

        public void Notify(string FilesProgress, string FileName, double DataProgress, Classes.TransferState.State State)
        {
            var builder = new NotificationCompat.Builder(Application.Context,"ID");
            builder.SetProgress(1000, (int)(DataProgress * 1000), false)
                .SetPriority(NotificationCompat.PriorityMax)
                .SetContentText(FileName)
                .SetSubText(State.ToString() + " • " + FilesProgress)
                .SetSmallIcon(Resource.Drawable.xamarin_logo)
                .SetOnlyAlertOnce(true);
            NotificationManager notificationManager = NotificationManager.FromContext(Application.Context);
            var notification = builder.Build();
            notification.Flags |= NotificationFlags.NoClear;
            notificationManager.Notify(5475, notification);
        }
    }
}