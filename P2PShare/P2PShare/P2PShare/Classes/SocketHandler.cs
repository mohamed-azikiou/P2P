using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Forms;

namespace P2PShare.Classes
{
    public abstract class SocketHandler
    {
        protected XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileListDescriber));
        //public event EventHandler ConnectedChanged;
        public event EventHandler RReady;
        public event EventHandler ReadyChanged
        {
            add { RReady += value; if (Ready) RReady(this, null);}
            remove { RReady -= value; }
        }
        private long globalStartTime = -1;
        int FileIndex = 0;
        private enum Operation { SendFile, SendFileStruct, SendSucceeded, SendFailed, ReportFileSendCancel, ReportSendCancel }
        //private Operation nextOperation = Operation.SendSucceeded;
        public ObservableCollection<FileInfo> FileList = new ObservableCollection<FileInfo>();
        protected bool connected = false;
        protected bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
                if (value)
                {
                    if(GetType() == typeof(ClientSocketHandler))
                    {
                        Task.Run(() => SendFileStruct());
                    }
                }
            }
        }

        protected bool ready=false;
        protected bool Ready
        {
            get
            {
                return ready;
            }
            set
            {
                ready = value;
                System.Diagnostics.Debug.WriteLine("XXXXXXXXXXXXXXX Ready " + Ready.ToString());
                if (value) RReady(this, null);
            }
        }

        // Execute tasks in background and report progress
        public BackgroundWorker Worker{ get; } = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        // How much data to send per time
        protected static readonly int bufferSize = 4 * 1024 * 1024;

        // Default port used
        protected static readonly int port = 15000;

        // Client Socket it is used to communicate data
        protected Socket client;

        // Global buffer to send and receive data
        protected readonly byte[] buf = new byte[bufferSize + sizeof(long)];

        // File buffer to read from files
        private readonly byte[] fileBuf = new byte[bufferSize];

        public void SendFiles()
        {
            Worker.DoWork += (object sendr, DoWorkEventArgs args) => {
                client.Send(BitConverter.GetBytes((int)Operation.SendFile));

                int FileIndex =(int) args.Argument;
                // Getting FileStream
                var file = SelectPage.Files[FileIndex].FileStream;

                // Creating main Header
                var fileLength = BitConverter.GetBytes(SelectPage.Files[FileIndex].Length);
                var fileName = Encoding.UTF8.GetBytes(SelectPage.Files[FileIndex].Name);
                var fileNameLength = BitConverter.GetBytes(fileName.Length);

                // Concatinating Header Information
                var HeaderWithoutCheckSum = fileNameLength.Concat(fileName).Concat(fileLength).ToArray();

                // Adding CheckSum to Header and putting it in the Buffer
                HeaderWithoutCheckSum.Concat(BitConverter.GetBytes(CalculateCheckSum(HeaderWithoutCheckSum, HeaderWithoutCheckSum.Length))).ToArray().CopyTo(buf, 0);

                // Sending Header
                for (int i = 0; i < 10; i++)
                {
                    // Try sending Header
                    client.BeginSend(buf, 0, fileLength.Length + fileName.Length + fileNameLength.Length + sizeof(long), SocketFlags.None, (IAsyncResult result) => { client.EndSend(result); }, null);

                    // Receive Result
                    client.Receive(buf,sizeof(int),SocketFlags.None);

                    // Check if received
                    if ((Operation)BitConverter.ToInt32(buf, 0) == Operation.SendSucceeded)
                    {
                        // Received
                        break;
                    }
                    // Not received
                }

                // Header Sent

                // Calculating startTime
                var startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                
                // Saving globalStartTime
                if(globalStartTime<0) globalStartTime = startTime;

                // Sending File Fragment by Fragment
                for (int index = 0; index <= (SelectPage.Files[FileIndex].Length - 1) / bufferSize; index++)
                {
                    var InstantTime= DateTime.Now.Ticks / (double)TimeSpan.TicksPerMillisecond;
                    // Read data from file
                    int SendingSocketSize = file.Read(fileBuf, 0, bufferSize);
                    for (int i = 0; i < 10; i++)
                    {
                        // Constructing File Fragment to send
                        fileBuf.CopyTo(buf, 0);
                        BitConverter.GetBytes(CalculateCheckSum(buf, SendingSocketSize)).CopyTo(buf, SendingSocketSize);

                        // Send Data to server
                        client.BeginSend(buf, 0, SendingSocketSize + +sizeof(long), SocketFlags.None, (IAsyncResult result) => { client.EndSend(result); }, null);

                        // Recceive Result
                        client.Receive(buf, sizeof(int), SocketFlags.None);

                        // Check if received
                        if ((Operation)BitConverter.ToInt32(buf, 0) == Operation.SendSucceeded)
                        {
                            var time = ((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
                            var averageSpeed = Convert.ToInt32((((double)index * bufferSize + SendingSocketSize) / 1024.0) / (time - startTime / 1000.0));
                            var instantSpeed = Convert.ToInt32((((double)SendingSocketSize) / 1024) / (time - InstantTime / 1000.0));
                            if(averageSpeed<0)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                            var progress = (index * bufferSize + SendingSocketSize) / (double)SelectPage.Files[FileIndex].Length;
                            Worker.ReportProgress(1, new TransferState() {
                                AverageSpeed = averageSpeed,
                                Time = time - globalStartTime / 1000.0,
                                CurrentFileIndex = FileIndex,
                                CurrentFileName = SelectPage.Files[FileIndex].Name,
                                FileCount = SelectPage.Files.Count,
                                CurrentFileProgress = progress,
                                InstantSpeed = instantSpeed,
                                CurrentState = progress == 1 ? TransferState.State.Completed : TransferState.State.Sending
                            });
                            break;
                        }
                        // Not Received
                    }
                }
            };
        }

        protected async void ReceiveData(IAsyncResult result)
        {
            client.EndReceive(result);
            switch ((Operation)BitConverter.ToInt32(buf, 0))
            {
                case Operation.SendFileStruct:
                    try {
                        client.Receive(buf, sizeof(int), SocketFlags.None);
                        var count = BitConverter.ToInt32(buf, 0);
                        int tempCount = 0;
                        while (tempCount < count)
                            tempCount += client.Receive(buf, tempCount, count - tempCount, SocketFlags.None);
                        var s = new MemoryStream(buf, 0, count);
                        var fileListDescriber = (FileListDescriber)xmlSerializer.Deserialize(s);
                        FileList = fileListDescriber.List;
                        client.Send(BitConverter.GetBytes((int)Operation.SendSucceeded));
                        Ready = true;
                    } catch (Exception e) { System.Diagnostics.Debug.WriteLine("EEEEEEEEEEEEEEEEEEEEEEeror " + e.Source + " " + e.Message); }
                    break;

                case Operation.SendFile:
                    await ReceiveFile();
                    FileIndex++;
                    break;
            }
            
            // Start Listening for other Data
            client.BeginReceive(buf, 0, sizeof(int), SocketFlags.None, ReceiveData, null);
        }

        private async Task ReceiveFile() {

            // File Info variables
            string FileName = null;
            long FileLength = 0;

            // Try Getting Header
            for (int i = 0; i < 10; i++)
            {
                // Receiving FileNameLength and adding it to FileHeader (To calulate CheckSum)
                client.Receive(buf, sizeof(int),SocketFlags.None);
                var FileHeader = buf.Take(sizeof(int)).ToArray();

                // Convert buf byte Array into an integer repesenting FileNameLength
                int FileNameLength = BitConverter.ToInt32(buf, 0);

                // Receiving File Name and adding it to FileHeader
                client.Receive(buf, FileNameLength, SocketFlags.None);
                FileHeader = FileHeader.Concat(buf.Take(FileNameLength)).ToArray();

                // Retreive File Name from byte array
                FileName = Encoding.UTF8.GetString(buf.Take(FileNameLength).ToArray());

                // Receiving File Length and adding it to FileHeader
                client.Receive(buf, sizeof(long), SocketFlags.None);
                FileHeader = FileHeader.Concat(buf.Take(sizeof(long))).ToArray();

                // Retreive File Length from byte array
                FileLength = BitConverter.ToInt64(buf, 0);

                // Receiving CheckSum and Storing it
                client.Receive(buf, sizeof(long), SocketFlags.None);
                long CheckSum = BitConverter.ToInt64(buf, 0);

                // Check credibility
                if (CheckSum == CalculateCheckSum(FileHeader, FileHeader.Length))
                {
                    // Confirm receiving
                    client.Send(BitConverter.GetBytes((int)Operation.SendSucceeded));
                    break;
                }
                else
                {
                    // Clear SocketBuffer
                    System.Threading.Thread.Sleep(100);
                    if (client.Available > 0) client.Receive(buf);

                    // Inform client about error
                    client.Send(BitConverter.GetBytes((int)Operation.SendFailed));
                }
            }

            // Header now received
            // Receiving data
            var file = (Stream)await DependencyService.Get<IPlatformDependent>().CreateFileAsync(DependencyService.Get<IPlatformDependent>().GetPath(), FileName);

            // Measuring Time
            var startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // Saving globalStartTime
            if(globalStartTime<0) globalStartTime = startTime;

            // Receiving File Fragment by Fragment
            for (int index = 0; index <= (FileLength - 1) / bufferSize; index++)
            {
                var instantTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                // Calculate FileFragment Length
                int RecievingSocketLength = (int)(FileLength - index * bufferSize < bufferSize ? FileLength - index * bufferSize : bufferSize);

                // Receiving Fragment
                for (int i = 0; i < 10; i++)
                {
                    // Received Data may be truncated so we loop untill receiving entire fragment
                    int received = 0;

                    while (received < RecievingSocketLength)
                    {
                        received += client.Receive(buf, received, RecievingSocketLength - received, SocketFlags.None);
                    }

                    // Receiving CheckSum
                    client.Receive(buf, RecievingSocketLength, sizeof(long), SocketFlags.None);

                    long CheckSum = BitConverter.ToInt64(buf, RecievingSocketLength);

                    // Checking credibility
                    if (CheckSum == CalculateCheckSum(buf, RecievingSocketLength))
                    {
                        var time = ((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
                        var AverageSpeed = Convert.ToInt32((((double)index * bufferSize + RecievingSocketLength) / 1024) / (time - startTime / 1000.0));
                        var IstantSpeed = Convert.ToInt32((RecievingSocketLength / 1024.0) / (time - instantTime / 1000.0));
                        var FileProgress = (index * bufferSize + RecievingSocketLength) / (double)FileList[FileIndex].Length;
                        Worker.ReportProgress(1, new TransferState()
                        {
                            AverageSpeed = AverageSpeed,
                            InstantSpeed = IstantSpeed,
                            CurrentFileIndex = FileIndex,
                            CurrentFileName = FileName,
                            CurrentFileProgress = FileProgress,
                            FileCount = FileList.Count,
                            Time = time - globalStartTime / 1000.0,
                            CurrentState = FileProgress == 1 ? Classes.TransferState.State.Completed : Classes.TransferState.State.Receiving
                        });

                        // Confirm receiving
                        client.Send(BitConverter.GetBytes((int)Operation.SendSucceeded));
                        break;
                    }
                    else
                    {
                        // Clear pending Data
                        System.Threading.Thread.Sleep(100);
                        while (client.Available > 0) client.Receive(buf);

                        // Inform about Error and Fragment index
                        client.Send(BitConverter.GetBytes((int)Operation.SendFailed));
                    }
                }
                // Writing Fragment to File
                file.Write(buf, 0, RecievingSocketLength);
            }

            // Closing File
            file.Close();
        }

        private static long CalculateCheckSum(byte[] buf, int size)
        {
            long checkSum = 0;
            for (int i = 0; i <= size / sizeof(int); i++) checkSum = (checkSum + BitConverter.ToInt32(buf, i)) % ((long)int.MaxValue + 1);
            return checkSum;
        }

        public void SendFileStruct()
        {
            var files = new FileListDescriber();
            foreach (var f in SelectPage.Files) files.Add(f.Name, f.Length);
            client.Send(BitConverter.GetBytes((int)Operation.SendFileStruct));
            var ms = new MemoryStream();
            xmlSerializer.Serialize(ms, files);
            var fldArray = ms.GetBuffer();
            client.Send(BitConverter.GetBytes(fldArray.Length));
            client.Send(fldArray);
            client.Receive(buf, sizeof(int), SocketFlags.None);
            if(BitConverter.ToInt32(buf,0)==(int)Operation.SendSucceeded)
            {
                Ready = true;
            }
        }
    }
}
