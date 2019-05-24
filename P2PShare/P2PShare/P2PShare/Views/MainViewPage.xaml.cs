using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Collections.ObjectModel;
using P2PShare.Classes;
using System.Threading.Tasks;

namespace P2PShare.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainViewPage : ContentPage
	{
        private ViewCell FocusedCell = null;
        public static ObservableCollection<File> Files { get; set; } = new ObservableCollection<File>();
        public MainViewPage()
        {
            InitializeComponent();
            DeviceName.Text = DeviceInfo.Name;
            if(Xamarin.Forms.Device.RuntimePlatform== Xamarin.Forms.Device.Android) {
                AppsView.ItemsSource = Files;
                Task.Run(() => {
                    foreach (var s in DependencyService.Get<IPlatformDependent>().GetApps()) Files.Add(s);
                });
            }
        }

        private async void Storage_Clicked(object sender, System.EventArgs e)
        {
            StorageState.Text= (await DependencyService.Get<IPlatformDependent>().RequestPermissionAsync())?"Granted":"Denied";
        }

        private void ViewCell_Tapped(object sender, System.EventArgs e)
        {
            if(FocusedCell!=null) FocusedCell.View.BackgroundColor = Color.Transparent;
            FocusedCell=(ViewCell)sender;
            FocusedCell.View.BackgroundColor = Color.FromHex("444444");
            AppsView.SelectedItem = null;
        }
    }
}