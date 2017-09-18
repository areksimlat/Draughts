using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorkerService
{
    public partial class ConnectWindow : Window
    {
        private bool clickConnect = false;

        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string ip in getIP())
            {
                ipComboBox.Items.Add(ip);
            }

            if (ipComboBox.Items.Count > 0)
            {
                ipComboBox.SelectedIndex = 0;
            }

            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                threadsComboBox.Items.Add(i + "");
            }

            threadsComboBox.SelectedIndex = threadsComboBox.Items.Count - 1;

            portTextBox.Text = "8734";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(portTextBox.Text))
            {
                MessageBox.Show("Podaj port!");
                return;
            }

            int intPort;
            if (!int.TryParse(portTextBox.Text, out intPort))
            {
                MessageBox.Show("Niepoprawny numer portu!");
                return;
            }


            clickConnect = true;
            Close();
        }

        public bool getClickConnect()
        {
            return clickConnect;
        }

        private List<string> getIP()
        {
            List<string> ips = new List<string>();
            IPAddress[] ipHostAdresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ipHost in ipHostAdresses)
                if (ipHost.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ips.Add(ipHost.ToString());

            return ips;
        }
    }
}
