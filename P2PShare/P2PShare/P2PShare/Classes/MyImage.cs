using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace P2PShare.Classes
{
    public class MyImage:Image
    {
        public MyImage()
        {
            SelectPage.lst.Add(this);
            System.Diagnostics.Debug.WriteLine("Proptychnged: " + SelectPage.lst.Count);
        }
    }
}
