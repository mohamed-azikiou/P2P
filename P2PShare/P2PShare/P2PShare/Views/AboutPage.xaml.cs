using Syncfusion.SfChart.XForms;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace P2PShare.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        private readonly ObservableCollection<ChartDataPoint> SpeedPoints = new ObservableCollection<ChartDataPoint>();
        public AboutPage()
        {
            InitializeComponent();
        }
    }
}