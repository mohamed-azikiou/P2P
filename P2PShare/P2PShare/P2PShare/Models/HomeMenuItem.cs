namespace P2PShare.Models
{
    public enum MenuItemType
    {
        Main,
        Send,
        Receive,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
