using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PocherUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);

            this.DataContext = ViewModel;
        }

        public MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();

        private void change_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    ViewModel.DD = fbd.SelectedPath;
                }
            }
        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            string title;

            var downloadList = ViewModel.Files.Where(f => f.Download).ToList();
            foreach(var file in downloadList)
            {
                title = System.IO.Path.Combine(ViewModel.DD, $"{file.Title}.mp3");
                Task.Run(() =>
                {
                    client.DownloadFile(new Uri(file.URL), title);
                });
            }

            //System.Windows.MessageBox.Show("Ready");
        }
    }
}
