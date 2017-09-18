using ServerServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Server
{

    public partial class AddWorkersWindow : Window
    {
        private class MyItem
        {
            public string NameValue { get; set; }
            public string UriValue { get; set; }

            public MyItem(string uri)
            {
                NameValue = WorkerInfo.getIpFromUri(uri) + ":" + WorkerInfo.getPortFromUri(uri);
                UriValue = uri;
            }

            public override string ToString()
            {
                return NameValue;
            }
        }

        private WorkerManager workerManager;


        public AddWorkersWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            workerManager = WorkerManager.getInstance();

            List<string> foundWorkers = workerManager.searchWorkers();

            foreach (String fWorker in foundWorkers)
            {
                this.foundWorkersListBox.Items.Add(new MyItem(fWorker));
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            List<MyItem> list = new List<MyItem>();

            foreach (MyItem selectedItem in this.foundWorkersListBox.SelectedItems)
            {
                list.Add(selectedItem);
            }

            foreach (MyItem selectedItem in list)
            {
                this.addedWorkersListBox.Items.Add(selectedItem);
                this.foundWorkersListBox.Items.Remove(selectedItem);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            List<MyItem> list = new List<MyItem>();

            foreach (MyItem selectedItem in this.addedWorkersListBox.SelectedItems)
            {
                list.Add(selectedItem);
            }

            foreach (MyItem selectedItem in list)
            {
                this.addedWorkersListBox.Items.Remove(selectedItem);
                this.foundWorkersListBox.Items.Add(selectedItem);
            }
        }


        private void SelectAllFound_Click(object sender, RoutedEventArgs e)
        {
            foreach (MyItem item in this.foundWorkersListBox.Items)
            {
                this.foundWorkersListBox.SelectedItems.Add(item);
            }
        }

        private void SelectAllAdded_Click(object sender, RoutedEventArgs e)
        {
            foreach (MyItem item in this.addedWorkersListBox.Items)
            {
                this.addedWorkersListBox.SelectedItems.Add(item);
            }
        }

        private void UnselectAllFound_Click(object sender, RoutedEventArgs e)
        {
            this.foundWorkersListBox.SelectedItems.Clear();
        }

        private void UnselectAllAdded_Click(object sender, RoutedEventArgs e)
        {
            this.addedWorkersListBox.SelectedItems.Clear();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.addedWorkersListBox.Items.Count > 0)
            {
                List<string> workersUri = new List<string>();

                foreach (MyItem item in this.addedWorkersListBox.Items)
                {
                    workersUri.Add(item.UriValue);
                }

                workerManager.connectToWorkers(workersUri);
            }
            else
            {
                
            }

            Close();
        }
    }
}
