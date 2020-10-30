using PocherUI.Commands;
using PocherUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace PocherUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            LoadFiles();
            DD = Directory.GetCurrentDirectory();
            ChangeCommand = new DelegateCommand(x => Change());
            DownloadCommand = new DelegateCommand(x => StartDownload());
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

        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set 
            { 
                isDownloading = value;
                OnNotifyChanged(nameof(IsDownloading));
            }
        }

        public DelegateCommand ChangeCommand { get; private set; }
        public DelegateCommand DownloadCommand { get; set; }

        private void Change()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    DD = fbd.SelectedPath;
                }
            }
        }

        private void StartDownload()
        {
            WebClient client = new WebClient();

            var list = Files.Where(f => f.Download).ToList();
            Thread t = new Thread(new ThreadStart(() => DoDownload(client, list)));
            t.Start();
        }

        private void DoDownload(WebClient client, List<FileModel> downloadList)
        {
            string title;
            IsDownloading = true;

            foreach (var file in downloadList)
            {
                var currentFile = Files.Where(f => f.Title == file.Title).FirstOrDefault();

                currentFile.Status = DownloadStatus.Loading;
                title = System.IO.Path.Combine(DD, $"{file.Title}.mp3");
                client.DownloadFile(new Uri(file.URL), title);
                currentFile.Status = DownloadStatus.Loaded;
            }

            IsDownloading = false;
        }

        private void OnNotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
