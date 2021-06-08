using PocherUI.Commands;
using PocherUI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PocherUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            LoadFiles();
            ChangeCommand = new DelegateCommand(x => Change());
            DownloadCommand = new DelegateCommand(x => StartDownload());
        }

        #region Properties
        public List<FileModel> Files { get; set; } = new List<FileModel>();

        private string _dd = Directory.GetCurrentDirectory();
        public string DD
        {
            get { return _dd; }
            set 
            { 
                _dd = value;
                OnPropertyChanged(nameof(DD));
            }
        }

        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set 
            { 
                isDownloading = value;
                OnPropertyChanged(nameof(IsDownloading));
            }
        }

        public DelegateCommand ChangeCommand { get; private set; }
        public DelegateCommand DownloadCommand { get; set; }
        #endregion
        private void LoadFiles()
        {
            var urlList = new ArrayList();
            var titleList = new ArrayList();

            var client = new WebClient();
            Byte[] pageData = client.DownloadData("https://audionow.de/podcast/die-pochers-hier");
            string html = Encoding.UTF8.GetString(pageData);

            var urlMatchesCollection = Regex.Matches(html, @"http[\S]*.mp3");
            foreach (Match match in urlMatchesCollection)
            {
                urlList.Add(match.Value);
            }

            var matchesCollection = Regex.Matches(html, "data-audiotitle=\"[\\d]* - [\\w\\säöüÄÖÜß\\!\\?\\.\\+\\*\\-\\:\\#\\,\\;]*\"");
            foreach (Match match in matchesCollection)
            {
                var title = match.Value.Split('=')[1];
                titleList.Add(title.Substring(1,title.Length-2));
            }

            int index = 0;
            foreach(var url in urlList)
            {
                Files.Add(new FileModel { Title = (string)titleList[index], URL = (string)url, Download = false, Status = DownloadStatus.Undefined });
                index++;
            }
        }

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
                file.Status = DownloadStatus.Loading;
                title = System.IO.Path.Combine(DD, $"{file.Title}.mp3");
                client.DownloadFile(new Uri(file.URL), title);
                file.Status = DownloadStatus.Loaded;
            }

            IsDownloading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}