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
            Byte[] pageData = client.DownloadData("https://www.deezer.com/de/show/57033");
            string html = Encoding.UTF8.GetString(pageData);

            var urlMatchesCollection = Regex.Matches(html, @"EPISODE_DIRECT_STREAM_URL.:.[\w:\\/\.]*mp3");
            foreach (Match match in urlMatchesCollection)
            {
                var url = $"{match.Value.Split(':')[1]}:{match.Value.Split(':')[2]}";
                url = url.Replace("\\","");
                url = url.Remove(0,1);
                urlList.Add(url);
            }

            var matchesCollection = Regex.Matches(html, @"EPISODE_TITLE.:.#\d*[\s\w\\\(/\)]*");
            foreach (Match match in matchesCollection)
            {
                var title = match.Value.Split(':')[1];
                title = title.Remove(0,1);
                title = title.Replace("\\","");
                title = title.Replace("/","-");

                var character = Regex.Match(title, "u00\\w{2}").Value;

                if(character != string.Empty)
                {
                    character = $"0x{character.Remove(0,1)}";
                    int iCharacter = Convert.ToInt32(character, 16);
                    string c = ((char)iCharacter).ToString();
                    title = Regex.Replace(title, "u00\\w{2}", c);
                }
                
                titleList.Add(title);
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