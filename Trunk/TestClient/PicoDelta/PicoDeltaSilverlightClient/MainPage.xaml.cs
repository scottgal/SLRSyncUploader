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
            
                diagnosticOutputBlock.Text = "Starting file scan...";

                var picoDeltaClient = new PicoDeltaService.PicoDeltaClient("BasicHttpBinding_IPicoDelta");
     

                picoDeltaClient.GetHashesForFileAsync();


                picoDeltaClient.GetHashesForFileCompleted +=picoDeltaClient_GetHashesForFileCompleted;
                
  




            
        }

        void picoDeltaClient_GetHashesForFileCompleted(object sender, PicoDeltaService.GetHashesForFileCompletedEventArgs e)
        {
            diagnosticOutputBlock.Text += e.Result.Count;
        }
    }
}
