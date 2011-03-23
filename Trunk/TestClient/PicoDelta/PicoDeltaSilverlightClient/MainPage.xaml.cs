using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PicoDeltaSl;

namespace PicoDeltaSilverlightClient
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();


            bool? userClickedOK = fd.ShowDialog();

            if (userClickedOK == true)
            {

                diagnosticOutputBlock.Text = "Starting file scan...";

                var fileProcessor = new FileProcessor();
                var config = new Config();

                var stream = fd.File.OpenRead();

                var bw = new BackgroundWorker();
                bw.DoWork += (backgroundSender, args) => args.Result = fileProcessor.GetHashesForFile(stream, config);


                bw.RunWorkerCompleted += (backgroundSender, args) =>
                                             {

                                                 var result = args.Result as ConcurrentDictionary<long, FileHash>;

                                                 if (result != null)
                                                 {
                                                     diagnosticOutputBlock.Text = result.Count + Environment.NewLine;
                                                 }

                                             };

                bw.RunWorkerAsync();




            }
        }
    }
}
