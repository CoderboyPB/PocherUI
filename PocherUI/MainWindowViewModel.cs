using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace PocherUI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            LoadFiles();
            DD = Directory.GetCurrentDirectory();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadFiles()
        {
            var client = new WebClient();
            Byte[] pageData = client.DownloadData("https://audionow.de/podcast/die-pochers-hier");
            string html = Encoding.UTF8.GetString(pageData);

            var matchesCollection = Regex.Matches(html, "<div class=\"podcast-episode.*>");
            foreach (Match match in matchesCollection)
            {

                var title = Regex.Match(match.Value, @"\d+ - ([\w\süöäß]+)").Value;
                var url = Regex.Match(match.Value, "https:(.+).mp3").Value;

                if(!string.IsNullOrWhiteSpace(title))
                    Files.Add(new FileModel { Title = title, URL = url, Download = false, Status = DownloadStatus.Undefined });
            }
        }

        public List<FileModel> Files { get; set; } = new List<FileModel>();

        private string _dd;

        public string DD
        {
            get { return _dd; }
            set 
            { 
                _dd = value;
                OnNotifyChanged(nameof(DD));
            }
        }

        private void OnNotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
