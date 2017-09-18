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

namespace Client
{
    public partial class EndGameWindow : Window
    {
        public bool NewGameClick { get; private set; }

        public EndGameWindow()
        {
            InitializeComponent();

            NewGameClick = false;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameClick = true;

            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
