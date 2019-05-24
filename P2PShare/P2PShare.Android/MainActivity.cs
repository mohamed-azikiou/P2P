using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace P2PShare.Droid
{
    [Activity(Label = "P2PShare", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Need so that permission request get prompted
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Syncfusion.XForms.Android.PopupLayout.SfPopupLayoutRenderer.Init();
            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok)
            {
                // This is used to stock file paths
                var files = new List<Classes.File>();

                // Get file(s) from ClipBoard
                if (data != null)
                {
                    // Get ClipData from Intent, null if not multiple ClipData items
                    ClipData clipData = data.ClipData;
                    if (clipData != null)
                    {
                        // Multiple files returned by user
                        for (int i = 0; i < clipData.ItemCount; i++)
                        {
                            // Get ClipData containin uri
                            ClipData.Item item = clipData.GetItemAt(i);

                            // Get file URI
                            Android.Net.Uri uri = item.Uri;

                            // Retreive Stream from uri and add it to files
                            files.Add(new Classes.File(GetFileName(uri), ContentResolver.OpenInputStream(uri), GetFileLength(uri)));
                        }
                    }
                    else
                    {
                        // Single File
                        // Get URI
                        var uri = data.Data;

                        // Retreive Stream from uri and add it to files
                        files.Add(new Classes.File(GetFileName(uri), ContentResolver.OpenInputStream(uri), GetFileLength(uri)));
                    }

                    // Dispose Recources
                    data.Dispose();

                    // Sending Result to the SelectPage
                    MessagingCenter.Send("MyApp", "FilesSent", files);
                }
            }
        }

        public string GetFileName(Android.Net.Uri uri)
        {
            // returning name
            string result = null;
            if (uri.Scheme.Equals("content"))
            {
                try
                {
                    // Query the source to get File info
                    var cursor = ContentResolver.Query(uri, null, null, null, null);

                    // Get column index of DisplayName
                    int nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);

                    // Move the cursor to the first row
                    cursor.MoveToFirst();

                    // Get file name from DisplayName column
                    result = cursor.GetString(nameIndex);

                    // Dispose
                    cursor.Close();
                }
                catch (Exception)
                {
                }
            }
            // If first method fails Just use provided name (After last / ) this may not be the correct file name
            if (result == null)
            {
                result = uri.Path;
                int cut = result.LastIndexOf('/');
                if (cut != -1)
                {
                    result = result.Substring(cut + 1);
                }
            }

            // return name
            return result;
        }

        public long GetFileLength(Android.Net.Uri uri)
        {
            // returning size
            long result = -1;
            if (uri.Scheme.Equals("content"))
            {
                try
                {
                    // Query the source to get File info
                    var cursor = ContentResolver.Query(uri, null, null, null, null);

                    // Get column index of DisplayName
                    int nameIndex = cursor.GetColumnIndex(OpenableColumns.Size);

                    // Move the cursor to the first row
                    cursor.MoveToFirst();

                    // Get file name from DisplayName column
                    result = cursor.GetLong(nameIndex);

                    // Dispose
                    cursor.Close();
                }
                catch (Exception e)
                {
                }
            }
            // If first method fails
            if (result == -1)
            {
                var juri = new Java.Net.URI(uri.ToString());
                var f = new Java.IO.File(juri);
                result = f.Length();
            }

            // return name
            return result;
        }
    }
}