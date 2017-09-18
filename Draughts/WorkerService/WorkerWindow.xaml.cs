using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkerService
{

    public partial class WorkerWindow : Window
    {
        private ServiceHost serviceHost = null;


        public WorkerWindow()
        {
            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.ShowDialog();

            if (connectWindow.getClickConnect())
            {
                string ip = (string) connectWindow.ipComboBox.SelectedValue;
                string port = connectWindow.portTextBox.Text;
                int maxThreads = Int32.Parse((string)connectWindow.threadsComboBox.SelectedValue);

                try
                {
                    Uri baseAddress = new Uri("net.tcp://" + ip + ":" + port + "/DraughtsWorkerService");
                    serviceHost = new ServiceHost(typeof(WorkerServiceLibrary.Worker), baseAddress);

                    serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                    serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

                    WorkerServiceLibrary.Worker.MAX_THREADS = maxThreads;

                    serviceHost.Open();

                    ipLabel.Content = ip;
                    portLabel.Content = port;
                }
                catch (Exception ex)
                {
                    ipLabel.Content = "Błąd";
                    portLabel.Content = "Błąd";

                    MessageBox.Show(ex.Message);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (serviceHost != null)
            {
                try
                {
                    serviceHost.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
