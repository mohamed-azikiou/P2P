using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

namespace P2PShare.Droid.Implementations
{
    public static class Convertor
    {
        public static string DrawabeToTempFile(Drawable image,string Name,bool CreateNew)
        {
            var folder = new Java.IO.File(DependencyService.Get<IPlatformDependent>().GetPath() + Java.IO.File.Separator + "P2PShare/Temp");
            if (!folder.Exists()) folder.Mkdirs();
            FileStream fs = null;
            try
            {
                fs = new FileStream(folder.Path + Java.IO.File.Separator + Name + ".png", FileMode.CreateNew);
                CreateNew = true;
            }
            catch (Exception) {
                if(CreateNew) {
                    var file = new Java.IO.File(folder.Path + Java.IO.File.Separator + Name + ".png");
                    file.Delete();
                    file.Dispose();
                    fs = new FileStream(folder.Path + Java.IO.File.Separator + Name + ".png", FileMode.CreateNew);
                }
            }
            if (CreateNew)
            {
                int w = image.IntrinsicWidth;
                int h = image.IntrinsicHeight;
                Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(b);
                image.SetBounds(0, 0, w, h);
                image.Draw(canvas);
                b.Compress(Bitmap.CompressFormat.Png, 100, fs);
                canvas.Dispose();
                b.Dispose();
                image.Dispose();
                fs.Flush();
                fs.Close();
            }
            folder.Dispose();
            return DependencyService.Get<IPlatformDependent>().GetPath() + Java.IO.File.Separator + "P2PShare/Temp" + Java.IO.File.Separator + Name + ".png";
        }
    }
}