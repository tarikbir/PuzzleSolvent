﻿using Microsoft.Win32;
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

//160202103 Tarık Bir
//150202040 Yasin Emir Kutlu

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
        private System.Drawing.Image[] PuzzleImageSplits; 
        private ImageSplit GameStatePickedBox;
        private int ButtonsCompleted;
        private string FilePath;

        public MainWindow()
        {
            InitializeComponent();
            Rows = (int)Math.Sqrt(BoxCount); //BoxCount must be square!
            CurrentGameState = GameState.Starting;
            Grid.SetRow(gPictureGrid, Rows);
            Grid.SetColumn(gPictureGrid, Rows);
            var getHighScore = GetMaximumScore();
            lbHighestScore.Content = (getHighScore == -99 ? "No Score" : getHighScore.ToString());
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            bool? fileOpened;
            if (String.IsNullOrEmpty(FilePath))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
                openFileDialog.Title = "Select an image file...";
                openFileDialog.CheckFileExists = true;
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fileOpened = openFileDialog.ShowDialog();
                FilePath = openFileDialog.FileName;
            }
            else
            {
                fileOpened = true;
            }

            if (fileOpened == true)
            {
                ButtonsCompleted = 0;
                GameStatePickedBox = null;

                if (CurrentGameState != GameState.Starting)
                {
                    gPictureGrid.Children.Clear();
                    btnShuffle.IsEnabled = true;
                }

                var image = System.Drawing.Image.FromFile(FilePath);
                SpawnBoxes(image, gPictureGrid);

                CurrentGameState = GameState.Playing;
                ShuffleBoxes();
            }

            FilePath = String.Empty;
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
            int randomShuffle = (random.Next()%30)+10;
            ButtonsCompleted = 0;
            foreach (var item in PuzzleImageBoxes)
            {
                item.IsEnabled = true;
            }
            for (int i = 0; i < randomShuffle; i++)
            {
                int randomSecondBox = (random.Next() % BoxCount);
                SwapBoxes(PuzzleImageBoxes[i % BoxCount], PuzzleImageBoxes[randomSecondBox], false);
            }
            ChangeScore(100);
            if (CheckGameForCompletion(true))
            {
                btnShuffle.IsEnabled = false;
                ButtonsCompleted++;
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
            PuzzleImageSplits = images;
        }

        private void PickBoxes(object sender, RoutedEventArgs e)
        {
            ImageSplit pickedBox = (ImageSplit) e.Source;
            if (GameStatePickedBox != null) //User has picked a box before.
            {
                SwapBoxes(pickedBox, GameStatePickedBox, true);
                CheckGameForCompletion(false);
                GameStatePickedBox = null;
            }
            else //User has picked a new box.
            {
                GameStatePickedBox = pickedBox;
            }
        }

        private void SwapBoxes(ImageSplit firstBox, ImageSplit secondBox, bool CheckForImage)
        {
            firstBox.swapChildren(secondBox);
            if (CheckForImage)
            {
                var firstBoxCorrect = CheckImage(firstBox);
                var secondBoxCorrect = CheckImage(secondBox);
                int deduction = (firstBoxCorrect ? 0 : 1) + (secondBoxCorrect ? 0 : 1);
                if (firstBox.buttonID == secondBox.buttonID) return;
                ChangeScore(Score - deduction);
            }
        }

        private System.Drawing.Image[] SplitImage(System.Drawing.Image image)
        {
            var imageArray = new System.Drawing.Image[BoxCount];
            int w = image.Width / Rows, h = image.Height / Rows;
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

        private bool CheckImage(ImageSplit split)
        {
            if (split.CompareTo(PuzzleImageSplits[split.buttonID]) == 1)
            {
                split.IsEnabled = false;
                ButtonsCompleted++;
                return true;
            }
            return false;
        }

        private bool CheckGameForCompletion(bool isShuffle)
        {
            bool gameReady = false;
            if (isShuffle)
            {
                foreach (var item in PuzzleImageBoxes)
                {
                    CheckImage(item);
                }

                if (ButtonsCompleted >= 1)
                {
                    gameReady = true;
                }
            }

            if (ButtonsCompleted >= BoxCount) //Game is over.
            {
                MessageBox.Show("Puzzle completed. Score: " + Score, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrentGameState = GameState.End;
                WriteHighScores();
                var getHighScore = GetMaximumScore();
                lbHighestScore.Content = (getHighScore == -99 ? "No Score" : getHighScore.ToString());
                return false;
            }
            return gameReady;
        }

        private int GetMaximumScore()
        {
            int max;
            try
            {
                max = (int) File.ReadAllLines("Highscores.txt").Select(Int32.Parse).OrderByDescending(x=>x).Take(1).ToArray()[0];
            }
            catch
            {
                max = -99;
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
            Score = score < 0 ? 0 : score % 101;
            lbHighScore.Content = Score;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FilePath = files[0];
                BtnOpen_Click(sender, e);
            }
        }
    }
}
