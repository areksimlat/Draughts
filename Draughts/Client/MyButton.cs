using GameCore;
using ServerServiceLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Client
{
    class MyButton : Button
    {
        private static MyButton[,] buttonsArray = null;
        private static Game game;
        private static BackgroundWorker compMoveWorker = null;
        private static ClientWindow clientWindow = null;

        private static Style style;
        private static bool isInitialized = false;
        private static SolidColorBrush darkFieldColor = new SolidColorBrush(Color.FromRgb(130, 80, 20));
        private static SolidColorBrush lightFieldColor = new SolidColorBrush(Color.FromRgb(225, 200, 130));
        private static SolidColorBrush hoverColor = new SolidColorBrush(Color.FromRgb(181, 230, 29));
        private static BitmapImage whitePieceImg;
        private static BitmapImage whiteDameImg;
        private static BitmapImage blackPieceImg;
        private static BitmapImage blackDameImg;
        private static bool shownHint;
        private static bool execCompMove;

        public MyButton(int size, bool isDark) : base()
        {
            MyButton.loadImages();

            Thickness margin = new Thickness(1);
            
            this.Style = style;
            this.Margin = margin;
            this.Width = size;
            this.Height = size;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.Tag = isDark;

            this.MouseEnter += my_Button_MouseEnter;
            this.MouseLeave += my_Button_MouseLeave;
            this.Click += my_Button_Click;

            if (MyButton.compMoveWorker == null)
            {
                MyButton.compMoveWorker = new BackgroundWorker();
                MyButton.compMoveWorker.DoWork += new DoWorkEventHandler(computerPlayerWork);
                MyButton.compMoveWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(computerPlayerComplete);
            }
        }

        private static void loadImages()
        {
            if (!MyButton.isInitialized)
            {
                MyButton.whitePieceImg = new BitmapImage(
                    new Uri("pack://application:,,,/Client;component/Images/white_piece.png"));

                MyButton.whiteDameImg = new BitmapImage(
                    new Uri("pack://application:,,,/Client;component/Images/white_dame.png"));

                MyButton.blackPieceImg = new BitmapImage(
                    new Uri("pack://application:,,,/Client;component/Images/black_piece.png"));

                MyButton.blackDameImg = new BitmapImage(
                    new Uri("pack://application:,,,/Client;component/Images/black_dame.png"));

                MyButton.isInitialized = true;
            }
        }

        public static void setGame(Game game)
        {
            MyButton.game = game;
        }

        public static void setStyle(Style style)
        {
            MyButton.style = style;
        }

        public static void setWindow(ClientWindow clientWindow)
        {
            MyButton.clientWindow = clientWindow;
        }

        public static void createButtonsArray()
        {
            int boardSize = game.getBoardSize();
            int boardHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight - 300;
            int btnSize = (boardHeight / boardSize) - 2;
            bool isDarkField = false;

            MyButton.buttonsArray = new MyButton[boardSize, boardSize];

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    MyButton btn = new MyButton(btnSize, isDarkField);
                    btn.SetValue(Grid.ColumnProperty, j);
                    btn.SetValue(Grid.RowProperty, i);

                    MyButton.buttonsArray[i, j] = btn;

                    isDarkField = !isDarkField;
                }
                isDarkField = !isDarkField;
            }
        }

        public static void initFirstMove()
        {
            game.getCountdownTimer().Start(game.getMoveTime());
            
            if (!MyButton.game.isPlayerTurn())
            {
                MyButton.compMoveWorker.RunWorkerAsync();
            }
        }

        public static MyButton[,] getButtonsArray()
        {
            return MyButton.buttonsArray;
        }

        public static void runMoveWorker()
        {
            MyButton.compMoveWorker.RunWorkerAsync();
        }

        private void updateInfo()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MyButton.clientWindow.whiteCountLabel.Content = game.getWhitePiecesCount();
                MyButton.clientWindow.blackCountLabel.Content = game.getBlackPiecesCount();

                if (game.isPlayerTurn())
                {
                    Position info;

                    info = game.getCurrMovesInfo();

                    if (info == null)
                    {
                        info = game.getAllMovesInfo();
                    }

                    MyButton.clientWindow.whosTurnLabel.Content = "Gracz";
                    MyButton.clientWindow.captureCountLabel.Content = info.Col; 
                    MyButton.clientWindow.movesCountLabel.Content = info.Row;
                }
                else
                {
                    MyButton.clientWindow.whosTurnLabel.Content = "Komputer";
                    MyButton.clientWindow.captureCountLabel.Content = "-";
                    MyButton.clientWindow.movesCountLabel.Content = "-";
                }                
            }));
        }

        public static void showAllowMoves()
        {
            foreach (MoveScenarios moveScenario in game.getAllowPlayerMoves())
            {
                if (moveScenario.Count() > 0)
                {
                    Position fromPos = moveScenario.getFromPosition();
                    MyButton.buttonsArray[fromPos.Row, fromPos.Col].Background = hoverColor;

                    foreach (List<Position> posList in moveScenario.getScenarios())
                    {
                        foreach (Position pos in posList)
                        {
                            MyButton.buttonsArray[pos.Row, pos.Col].Background = hoverColor;
                        }
                    }
                }
            }

            shownHint = true;
        }

        private void showBestMove(MoveScenarios bestScenario)
        {
            if (bestScenario != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Position fromPos = bestScenario.getFromPosition();
                    MyButton.buttonsArray[fromPos.Row, fromPos.Col].Background = hoverColor;

                    foreach (Position pos in bestScenario.getScenario(0))
                    {
                        MyButton.buttonsArray[pos.Row, pos.Col].Background = hoverColor;
                    }
                }));

                shownHint = true;
            }
        }

        private void repaint()
        {
            int col = (int)this.GetValue(Grid.ColumnProperty);
            int row = (int)this.GetValue(Grid.RowProperty);
            bool isDarkField = (bool)this.Tag;

            Position pos = new Position(row, col);

            if (game.isFieldUsed(pos))
                return;

            this.Background = isDarkField ? darkFieldColor : lightFieldColor;

            if (MyButton.game != null)
            {
                BitmapImage bitmap = null;
                int piece = game.getPiece(pos);

                if (Board.isWhite(piece))
                {
                    if (Board.isDame(piece))
                    {
                        bitmap = whiteDameImg;
                    }
                    else
                    {
                        bitmap = whitePieceImg;
                    }
                }
                else if (Board.isBlack(piece))
                {
                    if (Board.isDame(piece))
                    {
                        bitmap = blackDameImg;
                    }
                    else
                    {
                        bitmap = blackPieceImg;
                    }
                }

                this.Content = new Image
                {
                    Source = bitmap
                };
            }
        }

        public static void repaintButtons()
        {
            if (MyButton.buttonsArray == null)
                return;

            int boardSize = game.getBoardSize();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    MyButton.buttonsArray[i, j].repaint();
                }
            }
        }


        private void my_Button_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.OriginalSource;
            int col = (int)btn.GetValue(Grid.ColumnProperty);
            int row = (int)btn.GetValue(Grid.RowProperty);

            if (shownHint)
            {
                repaintButtons();
                shownHint = false;
            }

            if (game.isFieldAllow(new Position(row, col)))
            {
                btn.Background = hoverColor;
            }
        }

        private void my_Button_MouseLeave(object sender, RoutedEventArgs e)
        {
            MyButton btn = (MyButton)e.OriginalSource;
            int col = (int)btn.GetValue(Grid.ColumnProperty);
            int row = (int)btn.GetValue(Grid.RowProperty);
            bool isDarkField = (bool)btn.Tag;

            if (!game.isFieldUsed(new Position(row, col)))
            {
                btn.Background = isDarkField ? darkFieldColor : lightFieldColor;
            }
        }

        private void my_Button_Click(object sender, RoutedEventArgs e)
        {
            MyButton btn = (MyButton)e.OriginalSource;
            int col = (int)btn.GetValue(Grid.ColumnProperty);
            int row = (int)btn.GetValue(Grid.RowProperty);
            Position pos = new Position(row, col);

            updateInfo();

            if (shownHint)
            {
                repaintButtons();
                shownHint = false;
            }

            if (game.isFieldUsed(pos))
            {
                game.deleteMoves(pos);
                MyButton.repaintButtons();
                return;
            }

            if (game.isFieldAllow(pos))
            {
                btn.Background = hoverColor;

                if (game.addMove(pos))
                {
                    my_Button_MouseLeave(sender, e);

                    MyButton.repaintButtons();

                    if (game.isEndGame())
                    {
                        showEndGame();
                    }
                    else
                    {
                        MyButton.compMoveWorker.RunWorkerAsync();
                    }
                }
            }
        }


        private void computerPlayerWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MyButton.clientWindow.menuBar.IsEnabled = false;
                MyButton.clientWindow.showAllowMoveButton.IsEnabled = false;
                MyButton.clientWindow.showBestMoveButton.IsEnabled = false;
            }));

            if (!game.isPlayerTurn()) 
            {
                game.getCountdownTimer().Stop();
                game.getCountdownTimer().Start(game.getMoveTime());
            }

            MoveScenarios bestScenario =
                ClientWindow.serverService.GetBestMove(game.getBoard(), game.getMaxDepth(), game.getMoveTime());
            
            if (game.isPlayerTurn())
            {
                showBestMove(bestScenario);
                execCompMove = false;
            }
            else
            {
                game.getCountdownTimer().Stop();

                MyButton.game.execMove(bestScenario);
                execCompMove = true;
            }
        }

        private void computerPlayerComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MyButton.clientWindow.menuBar.IsEnabled = true;
                MyButton.clientWindow.showAllowMoveButton.IsEnabled = true;
                MyButton.clientWindow.showBestMoveButton.IsEnabled = true;
            }));

            if (execCompMove)
            {
                MyButton.repaintButtons();

                updateInfo();

                game.getCountdownTimer().Start(game.getMoveTime());

                notifyPlayer();

                if (game.isEndGame())
                {
                    showEndGame();                    
                }
            }
        }


        private void notifyPlayer()
        {
            SystemSounds.Beep.Play();

            foreach (Position p in game.getPlayerPiecesPosition())
            {
                buttonsArray[p.Row, p.Col].Background = hoverColor;
            }

            shownHint = true;
        }

        private void showEndGame()
        {
            game.getCountdownTimer().Stop();

            string text;

            if (game.isPlayerWin())
            {
                text = "WYGRAŁEŚ";
            }
            else
            {
                text = "PRZEGRAŁEŚ";
            }

            EndGameWindow endGameWindow = new EndGameWindow();
            endGameWindow.winnerLabel.Content = text;

            endGameWindow.ShowDialog();

            if (endGameWindow.NewGameClick)
            {
                clientWindow.NewGame_Click(null, null);
            }
        }
    }
}
