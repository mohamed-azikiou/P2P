using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace P2PShare.Classes
{
    public class FileListDescriber
    {
        public int Count { get => List.Count; set { } }
        public ObservableCollection<FileInfo> List { get; set; }

        public FileListDescriber()
        {
            List = new ObservableCollection<FileInfo>();
        }

        public void Add(string Name, long Length)
        {
            List.Add(new FileInfo(Name, Length));
        }
    }
    public class FileInfo : INotifyPropertyChanged {
        private double progress = 0;
        public double Progress { get => progress; set { progress = value; OnPropertyChanged("Progress"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public string Name { get; set; }
        public long Length { get; set; }
        public FileInfo() { }
        public FileInfo(string Name, long Length)
        {
            this.Name = Name;
            this.Length = Length;
        }
    }
}
