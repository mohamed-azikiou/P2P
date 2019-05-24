using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel;
using Syncfusion.SfChart.XForms;
using System.Collections.ObjectModel;
using Syncfusion.XForms.ProgressBar;

namespace P2PShare
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Sending : ContentPage
    {
        int idxx = 0;
        private readonly ObservableCollection<ChartDataPoint> AverageSpeedPoints = new ObservableCollection<ChartDataPoint>();
        private readonly ObservableCollection<ChartDataPoint> InstantSpeedPoints = new ObservableCollection<ChartDataPoint>();

        private readonly Classes.SocketHandler socket = null;

        public Sending()
        {
            InitializeComponent();
            SendBtn.IsEnabled = false;
        }
        public Sending(Classes.SocketHandler socket)
        {
            InitializeComponent();
            SendBtn.IsEnabled = false;
            this.socket = socket;
            AverageSpeedPoints.Add(new ChartDataPoint(0, 0));
            InstantSpeedPoints.Add(new ChartDataPoint(0, 0));
            AverageSpeed.ItemsSource = AverageSpeedPoints;
            InstantSpeed.ItemsSource = InstantSpeedPoints;
            if (typeof(Classes.ServerSocketHandler) == socket.GetType())
            {
                FileList.ItemsSource = socket.FileList;
            }
            else
            {
                FileList.ItemsSource = SelectPage.Files;
                SendBtn.IsEnabled = true;
            }
            socket.Worker.ProgressChanged += (object sender, ProgressChangedEventArgs args) =>
            {
                if (args.ProgressPercentage >= 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //((SfLinearProgressBar)args).
                        var state = (Classes.TransferState)args.UserState;
                        MessageEditor.Text = state.GetFormattedInstantSpeed();
                        if(SelectPage.Files.Count>0)
                            SelectPage.Files[state.CurrentFileIndex].Progress = state.CurrentFileProgress;
                        else
                            socket.FileList[state.CurrentFileIndex].Progress = state.CurrentFileProgress;
                        FileList.ItemsSource= FileList.ItemsSource;
                        AverageSpeedPoints.Add(new ChartDataPoint(state.Time, state.AverageSpeed));
                        InstantSpeedPoints.Add(new ChartDataPoint(state.Time, state.InstantSpeed));
                        DependencyService.Get<IPlatformDependent>().Notify(
                            (state.CurrentFileIndex + 1).ToString() + "/" + state.FileCount,
                            state.CurrentFileName,
                            state.CurrentFileProgress,
                            state.CurrentState
                        );
                    });
                }
            };
        }

        private void SendBtn_Clicked(object sender, EventArgs e)
        {
            socket.SendFiles();
            socket.Worker.RunWorkerCompleted += (object sendr, RunWorkerCompletedEventArgs args) =>
            {
                if (++idxx < SelectPage.Files.Count) socket.Worker.RunWorkerAsync(idxx);
            };
            socket.Worker.RunWorkerAsync(idxx);
        }

        private void SfLinearProgressBar_PropertyChanged(object sender, ValueChangedEventArgs e)
        {
            //var pbar = (ProgressBar)sender;
            //if (e.PropertyName == "Progress");
            //pbar.ProgressTo(pbar.Progress, 1000, Easing.Linear);
            System.Diagnostics.Debug.WriteLine("yghjedrtfyghujiokkrtgyhuji "+e.NewValue+e.NewValue);
        }
    }
}
