using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PuzzleSolvent
{
    public partial class MainWindow : Window
    {
        private enum GameState
        {
            Starting,
            Playing,
            End
        }

        private int BoxCount = 16;
        private int Score = 100;
        private int Rows;
        private GameState CurrentGameState;
        private ImageSplit[] PuzzleImageBoxes;
        private ImageSplit GameStatePickedBox;

        public MainWindow()
        {
            InitializeComponent();
            Rows = (int)Math.Sqrt(BoxCount); //BoxCount must be square!
            CurrentGameState = GameState.Starting;
            Grid.SetRow(gPictureGrid, Rows);
            Grid.SetColumn(gPictureGrid, Rows);
            lbHighestScore.Content = GetMaximumScore();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
            openFileDialog.Title = "Select an image file...";
            openFileDialog.CheckFileExists = true;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                //MessageBox.Show("You have chosen this file :" + filename);
                //MessageBox.Show("üretilen random sayılar :" + GenerateRandom());

                if (CurrentGameState != GameState.Starting)
                {
                    gPictureGrid.Children.Clear();
                    btnShuffle.IsEnabled = true;
                }

                var image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                SpawnBoxes(image, gPictureGrid);

                CurrentGameState = GameState.Playing;
                ShuffleBoxes();
                GameStatePickedBox = null;
            }
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentGameState == GameState.Playing)
            {
                ShuffleBoxes();
            }
            else
            {
                MessageBox.Show("The game is not started.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShuffleBoxes()
        {
            Random random = new Random();
            int randomShuffle = (random.Next()%70)+30;
            foreach(var item in PuzzleImageBoxes)
            {
                item.IsEnabled = true;
            }
            for (int i = 0; i < randomShuffle; i++)
            {
                int randomSecondBox = (random.Next() % BoxCount);
                SwapBoxes(PuzzleImageBoxes[i % BoxCount], PuzzleImageBoxes[randomSecondBox]);
            }
            ChangeScore(100);
            if (CheckGameForCompletion(true))
            {
                btnShuffle.IsEnabled = false;
            }
            GameStatePickedBox = null;
        }

        private void SpawnBoxes(System.Drawing.Image image, System.Windows.Controls.Primitives.UniformGrid parent)
        {
            ImageSplit[] boxes = new ImageSplit[BoxCount];
            var images = SplitImage(image);
            for (int i = 0; i < BoxCount; i++)
            {
                var split = images[i];
                boxes[i] = new ImageSplit(split,i);
                boxes[i].Click += PickBoxes;
                parent.Children.Add(boxes[i]);
            }
            PuzzleImageBoxes = boxes;
        }

        private void PickBoxes(object sender, RoutedEventArgs e)
        {
            ImageSplit pickedBox = (ImageSplit) e.Source;
            if (GameStatePickedBox != null) //User has picked a box before.
            {
                SwapBoxes(pickedBox, GameStatePickedBox);
                CheckGameForCompletion(false);
                GameStatePickedBox = null;
            }
            else //User has picked a new box.
            {
                GameStatePickedBox = pickedBox;
            }
        }

        private void SwapBoxes(ImageSplit firstBox, ImageSplit secondBox)
        {
            firstBox.swapChildren(secondBox);
            ChangeScore(--Score);
        }

        private System.Drawing.Image[] SplitImage(System.Drawing.Image image)
        {
            var imageArray = new System.Drawing.Image[BoxCount];
            int w = image.Width / Rows;
            int h = image.Height / Rows;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    var index = i + j * Rows;
                    imageArray[index] = new Bitmap(w, h);
                    var graphics = Graphics.FromImage(imageArray[index]);
                    graphics.DrawImage(image, new System.Drawing.Rectangle(0, 0, w, h), new System.Drawing.Rectangle(i * w, j * h, w, h), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }
            return imageArray;
        }

        private int CheckPuzzleForCorrectness()
        {
            int j = 0, correctNumberOfBoxes = 0;
            foreach (var item in PuzzleImageBoxes)
            {
                if (item.ID == j) //TODO: Fix Comparison
                {
                    item.IsEnabled = false;
                    correctNumberOfBoxes++;
                }
                j++;
            }
            return correctNumberOfBoxes;
        }

        private bool CheckGameForCompletion(bool isShuffle)
        {
            int correctBoxes = CheckPuzzleForCorrectness();
            bool gameReady = false;
            if (isShuffle)
            {
                if (correctBoxes > 1)
                {
                    gameReady = true;
                }
            }

            if (correctBoxes == BoxCount) //Game is over.
            {
                MessageBox.Show("Puzzle completed. Score: " + Score);
                CurrentGameState = GameState.End;
                WriteHighScores();
                return false;
            }
            return gameReady;
        }

        private int GetMaximumScore()
        {
            int max;
            try
            {
                max = File.ReadAllLines("Highscores.txt").Select(int.Parse).Max();
            }
            catch
            {
                max = -1;
            }
            
            return max;
        }

        private void WriteHighScores()
        {
            using (StreamWriter Writer = new StreamWriter("Highscores.txt", true))
            {
                Writer.WriteLine(Score);
            }
        }

        private void ChangeScore(int score)
        {
            Score = score;
            lbHighScore.Content = score.ToString();
        }
    }
}
