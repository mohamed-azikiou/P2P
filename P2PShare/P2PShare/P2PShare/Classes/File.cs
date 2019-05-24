using System;
using System.ComponentModel;
using System.IO;
using Xamarin.Forms;

namespace P2PShare.Classes
{
    public class File : IDisposable, INotifyPropertyChanged
    {
        private double progress=0;
        public double Progress { get => progress; set { progress = value; OnPropertyChanged("Progress"); } }
        public string Name { get; set; }
        public long Length { get; set; }
        public ImageSource Image { get; set; }
        public Stream FileStream { get; set; }
        public File() { }
        public File(string Name, Stream FileStream, long Length)
        {
            this.Name = Name;
            Image = "fileIcon.png";
            this.FileStream = FileStream;
            this.Length = Length;
        }
        public File(string Name, Stream FileStream, ImageSource Image,long Length)
        {
            this.Name = Name;
            this.Image = Image;
            this.FileStream = FileStream;
            this.Length = Length;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose ()
        {
            Image=null;
            FileStream.Close();
            FileStream.Dispose();
        }
    }
}
