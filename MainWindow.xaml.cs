using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LifeGame
{
    public partial class MainWindow : Window
    {
        private const int CellSize = 40;
        private GameEngine engine;
        private DispatcherTimer timer;
        private bool isRunning;
        private Dictionary<string, Rectangle> cellRectangles = new Dictionary<string, Rectangle>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            SetupTimer();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded && engine != null)
            {
                InitializeGame();
            }
        }

        private void InitializeGame()
        {
            gameCanvas.Children.Clear();
            cellRectangles.Clear();

            int columns = Math.Max(1, (int)(ActualWidth / CellSize));
            int rows = Math.Max(1, (int)(ActualHeight / CellSize));

            engine = new GameEngine(rows, columns);
            RenderGameBoard();
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(sliderSpeed.Value);
            sliderSpeed.ValueChanged += (s, e) =>
            {
                timer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
            };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            bool changed = engine.NextGeneration();
            tbGeneration.Text = engine.Generation.ToString();

            if (changed)
            {
                UpdateChangedCells();
            }
            else
            {
                isRunning = false;
                btnStartPause.Content = "Start";
                timer.IsEnabled = false;
                tbStatus.Text = "СТАБИЛЬНОЕ СОСТОЯНИЕ";
            }

            sw.Stop();
            tbPerformance.Text = $"{sw.ElapsedMilliseconds} ms";
        }

        private void RenderGameBoard()
        {
            for (int row = 0; row < engine.Rows; row++)
            {
                for (int col = 0; col < engine.Columns; col++)
                {
                    var rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = Brushes.Black,
                        Stroke = Brushes.DarkGray,
                        StrokeThickness = 0.3
                    };

                    Canvas.SetLeft(rect, col * CellSize);
                    Canvas.SetTop(rect, row * CellSize);

                    gameCanvas.Children.Add(rect);
                    cellRectangles[$"{row},{col}"] = rect;
                }
            }
        }

        private void UpdateChangedCells()
        {
            foreach (var change in engine.GetChangedCells())
            {
                string key = $"{change.Row},{change.Column}";
                if (cellRectangles.TryGetValue(key, out var rect))
                {
                    rect.Fill = change.IsAlive ?
                        new SolidColorBrush(Color.FromRgb(255, 200, 0)) :
                        Brushes.Black;
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isRunning)
            {
                Point position = e.GetPosition(gameCanvas);
                int row = (int)(position.Y / CellSize);
                int col = (int)(position.X / CellSize);

                if (row >= 0 && row < engine.Rows && col >= 0 && col < engine.Columns)
                {
                    engine.ToggleCell(row, col);
                    UpdateSingleCell(row, col);
                }
            }
        }

        private void UpdateSingleCell(int row, int col)
        {
            string key = $"{row},{col}";
            if (cellRectangles.TryGetValue(key, out var rect))
            {
                bool isAlive = engine.IsCellAlive(row, col);
                rect.Fill = isAlive ?
                    new SolidColorBrush(Color.FromRgb(255, 200, 0)) :
                    Brushes.Black;
            }
        }

        private void StartPause_Click(object sender, RoutedEventArgs e)
        {
            isRunning = !isRunning;
            btnStartPause.Content = isRunning ? "Pause" : "Start";
            timer.IsEnabled = isRunning;
            tbStatus.Text = isRunning ? "" : "ПАУЗА";
        }

        private void Step_Click(object sender, RoutedEventArgs e)
        {
            engine.NextGeneration();
            tbGeneration.Text = engine.Generation.ToString();
            UpdateChangedCells();
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            engine.Randomize();
            UpdateAllCells();
            tbStatus.Text = "";
        }

        private void UpdateAllCells()
        {
            foreach (var cell in engine.GetAllCells())
            {
                string key = $"{cell.Row},{cell.Column}";
                if (cellRectangles.TryGetValue(key, out var rect))
                {
                    rect.Fill = cell.IsAlive ?
                        new SolidColorBrush(Color.FromRgb(255, 200, 0)) :
                        Brushes.Black;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            engine.Clear();
            UpdateAllCells();
            tbGeneration.Text = "0";
            tbStatus.Text = "";
        }
    }
}