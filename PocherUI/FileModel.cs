using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocherUI
{
    public class FileModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string URL { get; set; }
        public bool Download { get; set; }
        private DownloadStatus _status;

        public DownloadStatus Status
        {
            get { return _status; }
            set 
            {  
                _status = value;
                ONC(nameof(Status));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void ONC(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
