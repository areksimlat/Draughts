using GameCore;
using ServerServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    public partial class ClientWindow : Window
    {
        private Game game;
        private ChannelFactory<IServerService> factory;
        public static IServerService serverService { get; private set; } 

        public ClientWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            searchAndConnectServer();

            GameSettings gameSettings = getGameSettngsFromUser();

            if (gameSettings == null)
            {
                Application.Current.Shutdown();
            }

            initBoard(gameSettings);
            initBoardPieces();
            initBoardLetters();
            initInfo();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (factory != null)
            {
                try
                {
                    factory.Close();
                }
                catch (Exception ex)
                {

                }
            }  
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            MyButton.initFirstMove();
        }


        private void initBoard(GameSettings gameSettings)
        {
            game = new Game(gameSettings);
            game.getCountdownTimer().addElapsedEventHandler(new ElapsedEventHandler(UpdateTimeEvent));
            game.init();

            int boardSize = game.getBoardSize();

            GridBoard.ColumnDefinitions.Clear();
            GridBoard.RowDefinitions.Clear();

            for (int i = 0; i < boardSize + 1; i++)
            {
                GridBoard.ColumnDefinitions.Add(new ColumnDefinition());
                GridBoard.RowDefinitions.Add(new RowDefinition());
            }
        }

        private void initBoardPieces()
        {
            MyButton.setGame(game);
            MyButton.setStyle(FindResource("MyButtonStyle") as Style);
            MyButton.setWindow(this);
            MyButton.createButtonsArray();

            MyButton[,] btnArray = MyButton.getButtonsArray();

            int boardSize = game.getBoardSize();

            GridBoard.Children.Clear();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    GridBoard.Children.Add(btnArray[i, j]);
                }
            }

            MyButton.repaintButtons();
        }

        private void initBoardLetters()
        {
            int boardSize = game.getBoardSize();

            char colLetter = 'A';
            int rowNumber = boardSize;
            Label label;

            for (int i = 0; i < boardSize; i++)
            {
                label = new Label();
                label.Content = Convert.ToString(rowNumber);
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.SetValue(Grid.RowProperty, i);
                label.SetValue(Grid.ColumnProperty, boardSize);
                GridBoard.Children.Add(label);

                label = new Label();
                label.Content = colLetter;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.SetValue(Grid.RowProperty, boardSize);
                label.SetValue(Grid.ColumnProperty, i);
                GridBoard.Children.Add(label);

                colLetter++;
                rowNumber--;
            }
        }

        private void initInfo()
        {
            this.whiteCountLabel.Content = game.getWhitePiecesCount();
            this.blackCountLabel.Content = game.getBlackPiecesCount();

            this.whosTurnLabel.Content = game.isPlayerTurn() ? "Gracz" : "Komputer";
            this.captureCountLabel.Content = "-";
            this.movesCountLabel.Content = "-";
        }

        private void UpdateTimeEvent(object source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.timeLabel.Content = game.getCountdownTimer().getRemainTimeString();
            }));

            if (game.isPlayerTurn() && game.getCountdownTimer().isTimeout())
            {
                game.execRandomMove();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MyButton.repaintButtons();
                    initInfo();
                    game.getCountdownTimer().Start(game.getMoveTime());
                    MyButton.runMoveWorker();

                }));
            }
        }

        private void searchAndConnectServer()
        {
            ConnectWindow connectWindow = new ConnectWindow();
            connectWindow.ShowDialog();            

            if (connectWindow.FoundServers > 0)
            {
                string serverUri = (string)connectWindow.serverComboBox.SelectedValue;

                try
                {
                    Uri baseAddress = new Uri(serverUri);
                    EndpointAddress address = new EndpointAddress(baseAddress);
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.None;
                    
                    factory = new ChannelFactory<IServerService>(binding, address);
                    serverService = factory.CreateChannel();

                    ((IContextChannel)serverService).OperationTimeout = TimeSpan.MaxValue;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.serverConnectLabel.Content = "Połączenie z serwerem: Połączono";
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private GameSettings getGameSettngsFromUser()
        {
            GameSettingsWindow settingsWindow = new GameSettingsWindow();
            settingsWindow.ShowDialog();

            return settingsWindow.getGameSettings();
        }


        private void ShowAllowMoves_Click(object sender, RoutedEventArgs e)
        {
            MyButton.showAllowMoves();
        }

        private void ShowBestMove_Click(object sender, RoutedEventArgs e)
        {
            MyButton.runMoveWorker();
        }


        public void NewGame_Click(object sender, RoutedEventArgs e)
        {
            GameSettings gameSettings = getGameSettngsFromUser();

            if (gameSettings != null)
            {
                initBoard(gameSettings);
                initBoardPieces();
                initBoardLetters();
                initInfo();

                MyButton.initFirstMove();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

    }
}
