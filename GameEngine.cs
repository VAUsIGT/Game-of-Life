using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LifeGame
{
    public class CellChange
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsAlive { get; set; }
    }

    public class GameEngine
    {
        public int Rows { get; }
        public int Columns { get; }
        public int Generation { get; private set; }

        private bool[,] currentGeneration;
        private bool[,] nextGeneration;
        private List<CellChange> changedCells = new List<CellChange>();

        public GameEngine(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Initialize();
        }

        private void Initialize()
        {
            currentGeneration = new bool[Rows, Columns];
            nextGeneration = new bool[Rows, Columns];
            Generation = 0;
        }

        public void Clear()
        {
            Initialize();
        }

        public bool IsCellAlive(int row, int col)
        {
            return currentGeneration[row, col];
        }

        public void Randomize()
        {
            var random = new Random();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    currentGeneration[i, j] = random.Next(0, 5) == 0;
                }
            }
            Generation = 0;
        }

        public void ToggleCell(int row, int column)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                currentGeneration[row, column] = !currentGeneration[row, column];
            }
        }

        public IEnumerable<CellChange> GetAllCells()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    yield return new CellChange
                    {
                        Row = i,
                        Column = j,
                        IsAlive = currentGeneration[i, j]
                    };
                }
            }
        }

        public IEnumerable<CellChange> GetChangedCells()
        {
            foreach (var change in changedCells)
            {
                yield return change;
            }
            changedCells.Clear();
        }

        public bool NextGeneration()
        {
            changedCells.Clear();
            bool changesDetected = false;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    int liveNeighbors = CountLiveNeighbors(i, j);
                    bool isAlive = currentGeneration[i, j];
                    bool newState = isAlive;

                    if (isAlive && (liveNeighbors < 2 || liveNeighbors > 3))
                    {
                        newState = false;
                    }
                    else if (!isAlive && liveNeighbors == 3)
                    {
                        newState = true;
                    }

                    nextGeneration[i, j] = newState;

                    if (isAlive != newState)
                    {
                        changedCells.Add(new CellChange
                        {
                            Row = i,
                            Column = j,
                            IsAlive = newState
                        });
                        changesDetected = true;
                    }
                }
            }

            (currentGeneration, nextGeneration) = (nextGeneration, currentGeneration);
            Generation++;

            return changesDetected;
        }

        private int CountLiveNeighbors(int x, int y)
        {
            int count = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = (x + i + Rows) % Rows;
                    int neighborY = (y + j + Columns) % Columns;

                    if (currentGeneration[neighborX, neighborY])
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}