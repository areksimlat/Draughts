using GameCore;
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
    public partial class GameSettingsWindow : Window
    {
        private GameSettings gameSettings = null;
 
        public GameSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String[] boardSize = { "8x8", "10x10" };
            String[] pieceColor = { "Biały", "Czarny" };
            String[] levels = { "Łatwy", "Średni", "Trudny", "Własny" };

            foreach (String bSize in boardSize)
                this.boardSizeComboBox.Items.Add(bSize);

            foreach (String pColor in pieceColor)
                this.pieceColorComboBox.Items.Add(pColor);

            foreach (String level in levels)
                this.levelComboBox.Items.Add(level);

            for (int i = 1; i < 50; i += 2)
                this.depthComboBox.Items.Add(i + "");

            this.timeComboBox.Items.Add("-");

            for (int i = 1; i < 20; i++)
                this.timeComboBox.Items.Add(i + "");

            this.boardSizeComboBox.SelectedIndex = 0;
            this.pieceColorComboBox.SelectedIndex = 0;
            this.levelComboBox.SelectedIndex = 0;
            this.depthComboBox.SelectedIndex = 0;
            this.timeComboBox.SelectedIndex = 4;
        }

        public GameSettings getGameSettings()
        {
            return gameSettings;
        }

        private void levelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;
            String selectItem = (String) comboBox.SelectedItem;

            if (selectItem.Equals("Własny"))
            {
                this.depthComboBox.IsEnabled = true;
            }
            else
            {
                this.depthComboBox.IsEnabled = false;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            gameSettings = new GameSettings();

            if (this.boardSizeComboBox.SelectedValue.Equals("8x8"))
                gameSettings.International = false;
            else
                gameSettings.International = true;


            if (this.pieceColorComboBox.SelectedValue.Equals("Biały"))
                gameSettings.PieceColor = Board.WHITE;
            else
                gameSettings.PieceColor = Board.BLACK;


            String chooseLevel = (String) this.levelComboBox.SelectedValue;

            if (chooseLevel.Equals("Łatwy"))
                gameSettings.Depth = MiniMax.LEVEL_EASY;
            else if (chooseLevel.Equals("Średni"))
                gameSettings.Depth = MiniMax.LEVEL_MEDIUM;
            else if (chooseLevel.Equals("Trudny"))
                gameSettings.Depth = MiniMax.LEVEL_HARD;
            else
            {
                int depth;

                Int32.TryParse((String)this.depthComboBox.SelectedValue, out depth);
                
                gameSettings.Depth = depth;
            }

            String timeValue = (String)this.timeComboBox.SelectedValue;

            if (timeValue.Equals("-"))
            {
                gameSettings.MoveTimeInSec = -1;
            }
            else
            {
                int moveTimeInMin;
                Int32.TryParse(timeValue, out moveTimeInMin);
                gameSettings.MoveTimeInSec = moveTimeInMin * 60;
            }            

            Close();                
        }
    }
}
