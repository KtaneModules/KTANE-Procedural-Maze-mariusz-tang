using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BitMaze6x6 {

    public BitMaze6x6(Vector2Int start, Vector2Int goal) {
        GenerateCellsAndWalls();
        StartCell = Cells[start.x, start.y];
        GoalCell = Cells[goal.x, goal.y];

        StartCell.GetAdjacentWalls().Where(w => !w.IsDecided).ToList().ForEach(w => w.IsPresent = false);
    }

    // Positions stored as [column, row].
    public Cell StartCell { get; private set; }
    public Cell GoalCell { get; private set; }
    public Cell[,] Cells { get; private set; }
    // Stored as [column/row, wall]
    public Wall[,] ColumnWalls { get; private set; }
    public Wall[,] RowWalls { get; private set; }

    private void GenerateCellsAndWalls() {
        ColumnWalls = new Wall[6, 7];
        RowWalls = new Wall[6, 7];
        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                ColumnWalls[line, wall] = new Wall();
                RowWalls[line, wall] = new Wall();

                // Set outer walls.
                if (wall == 0 || wall == 6) {
                    ColumnWalls[line, wall].IsPresent = true;
                    RowWalls[line, wall].IsPresent = true;
                }
            }
        }

        Cells = new Cell[6, 6];
        for (int row = 0; row < 6; row++) {
            for (int col = 0; col < 6; col++) {
                Cells[col, row] = new Cell(new Vector2Int(col, row));
            }
        }

        AssignCellWallsAndNeighbours();
    }

    private void AssignCellWallsAndNeighbours() {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                Cell currentCell = Cells[col, row];

                currentCell.AssignAdjacentWall(MazeDirection.Left, RowWalls[row, col]);
                currentCell.AssignAdjacentWall(MazeDirection.Right, RowWalls[row, col + 1]);
                currentCell.AssignAdjacentWall(MazeDirection.Up, ColumnWalls[col, row]);
                currentCell.AssignAdjacentWall(MazeDirection.Down, ColumnWalls[col, row + 1]);

                if (col != 0) {
                    currentCell.AssignNeighbour(MazeDirection.Left, Cells[col - 1, row]);
                }
                if (col != 5) {
                    currentCell.AssignNeighbour(MazeDirection.Right, Cells[col + 1, row]);
                }
                if (row != 0) {
                    currentCell.AssignNeighbour(MazeDirection.Up, Cells[col, row - 1]);
                }
                if (row != 5) {
                    currentCell.AssignNeighbour(MazeDirection.Down, Cells[col, row + 1]);
                }
            }
        }
    }

    public int[] GetBitColumn(int colNum, bool topToBottom = false) {
        var column = new int[6];

        for (int rowNum = 0; rowNum < 6; rowNum++) {
            column[rowNum] = Cells[colNum, rowNum].Bit;
        }
        if (!topToBottom) {
            Array.Reverse(column);
        }

        return column;
    }

    public int[] GetBitRow(int rowNum, bool leftToRight = false) {
        var row = new int[6];

        for (int colNum = 0; colNum < 6; colNum++) {
            row[colNum] = Cells[colNum, rowNum].Bit;
        }
        if (!leftToRight) {
            Array.Reverse(row);
        }

        return row;
    }

    public int[] GetBitLineInDirection(Vector2Int cell, MazeDirection direction) {
        int directionIndex = (int)direction;

        if (directionIndex % 2 == 0) {
            return GetBitColumn(cell.x, direction == MazeDirection.Down);
        }
        else {
            return GetBitRow(cell.y, direction == MazeDirection.Right);
        }
    }

    public class Cell {
        private Dictionary<MazeDirection, Wall> _walls = new Dictionary<MazeDirection, Wall>();
        private Dictionary<MazeDirection, Cell> _neighbours = new Dictionary<MazeDirection, Cell>();

        public Cell(Vector2Int position) {
            Position = position;
        }

        public Vector2Int Position { get; private set; }
        public int Bit { get; set; }

        public void AssignAdjacentWall(MazeDirection inDirection, Wall wall) => _walls.Add(inDirection, wall);
        public Wall GetAdjacentWall(MazeDirection inDirection) => _walls[inDirection];
        public Wall[] GetAdjacentWalls() => _walls.Select(p => p.Value).ToArray();

        public Wall[] GetAdjacentWallsClockFromDirection(MazeDirection direction) {
            var walls = new Wall[4];

            for (int d = 0; d < 4; d++) {
                Debug.Log($"Checking direction {(MazeDirection)(((int)direction + d) % 4)}, which is decided: {GetAdjacentWall((MazeDirection)(((int)direction + d) % 4)).IsDecided}");
                walls[d] = GetAdjacentWall((MazeDirection)(((int)direction + d) % 4));
            }

            return walls;
        }

        public void AssignNeighbour(MazeDirection inDirection, Cell neighbour) => _neighbours.Add(inDirection, neighbour);
        public Cell GetNeighbour(MazeDirection inDirection) => _neighbours[inDirection];
    }

    public class Wall {
        private bool _isPresent;

        public bool IsDecided { get; private set; } = false;
        public bool IsPresent {
            get {
                if (!IsDecided) {
                    throw new InvalidOperationException("This wall's state has not yet been set!");
                }
                return _isPresent;
            }
            set {
                if (IsDecided) {
                    throw new InvalidOperationException("This wall's state has already been set!");
                }
                IsDecided = true;
                _isPresent = value;
            }
        }

        public void Reset() {
            IsDecided = false;
        }
    }
}
