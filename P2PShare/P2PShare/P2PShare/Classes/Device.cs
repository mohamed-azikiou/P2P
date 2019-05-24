namespace P2PShare.Classes
{
    public class Device
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string ImageSource  { get; set; }
        public enum DeviceType { IOS, Android, UWP }
        public Device(string Name,string ID) {
            this.ID = ID;
            this.Name = Name;
        }
    }
}
