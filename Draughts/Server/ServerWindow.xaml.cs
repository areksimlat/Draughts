using ServerServiceLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using WorkerServiceLibrary;

namespace Server
{
    public partial class ServerWindow : Window
    {
        private ServiceHost serviceHost = null;
        private List<IWorker> workers = new List<IWorker>();

        public ServerWindow()
        {
            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            initService();
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

            WorkerManager.getInstance().abortWorkers();

                
        }


        private void initService()
        {
            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.ShowDialog();

            if (connectWindow.getClickConnect())
            {
                try
                {
                    string ip = (string)connectWindow.ipComboBox.SelectedValue;
                    string port = connectWindow.portTextBox.Text;

                    Uri baseAddress = new Uri("net.tcp://" + ip + ":" + port + "/DraughtsServerService");
                    serviceHost = new ServiceHost(typeof(ServerServiceLibrary.ServerService), baseAddress);

                    serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                    serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

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


        private void SearchWorkers_Click(object sender, RoutedEventArgs e)
        {
            new AddWorkersWindow().ShowDialog();

            List<WorkerInfo> workersInfo = WorkerManager.getInstance().getWorkersInfo();

            workerListBox.Items.Clear();

            foreach (WorkerInfo wInfo in workersInfo)
            {
                workerListBox.Items.Add(wInfo);
            }
        }

        private void workerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WorkerInfo info = (WorkerInfo) this.workerListBox.SelectedItem;

            this.workerIpLabel.Content = info.getIp();
            this.workerPortLabel.Content = info.getPort();
        }
    }
}
