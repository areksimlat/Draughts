using ServerServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Discovery;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{

    public partial class ConnectWindow : Window
    {
        public int FoundServers { get; private set; }


        public ConnectWindow()
        {
            InitializeComponent();

            FoundServers = 0;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            FindCriteria findCriteria = new FindCriteria(typeof(IServerService));
            findCriteria.Duration = TimeSpan.FromSeconds(2);

            FindResponse findResponse = discoveryClient.Find(findCriteria);

            FoundServers = findResponse.Endpoints.Count;

            foreach (EndpointDiscoveryMetadata edm in findResponse.Endpoints)
            {
                serverComboBox.Items.Add(edm.Address.Uri.ToString());
            }

            if (serverComboBox.Items.Count > 0)
            {
                serverComboBox.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Nie odnaleziono serwera");
            }
        }
    }
}
