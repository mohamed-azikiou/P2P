using P2PShare.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P2PShare
{
    public interface IPlatformDependent 
    {
        Task<List<File>> PickMultipleFiles();
        void FindWFDDevices();
        void Connect(string DeviceID, bool IsReceiver);
        void Init(bool IsReceiver);
        void Dispose();
        Task<object> CreateFileAsync(string path,string fileName);
        string GetPath();
        List<File> GetApps();
        Task<bool> CheckPermissionAsync();
        Task<bool> RequestPermissionAsync();
        void Notify(string FilesProgress, string FileName, double DataProgress, TransferState.State State);
    }
}
