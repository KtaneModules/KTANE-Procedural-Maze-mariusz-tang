using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BitMaze6x6 {

    public BitMaze6x6(Vector2Int start, Vector2Int goal) {
        StartPosition = start;
        GoalPosition = goal;

        ColumnWalls = new Wall[6, 7];
        RowWalls = new Wall[6, 7];
        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                ColumnWalls[line, wall] = new Wall();
                RowWalls[line, wall] = new Wall();
            }
        }
    }

    public BitMaze6x6(BitMaze6x6 original) {
        StartPosition = original.StartPosition;
        GoalPosition = original.GoalPosition;
        Bitmap = original.Bitmap;
        ColumnWalls = original.ColumnWalls;
        RowWalls = original.RowWalls;
    }

    public string CurrentSeed { get; set; }
    // Stored as [column, row].
    public Vector2Int StartPosition { get; private set; }
    public Vector2Int GoalPosition { get; private set; }
    public int[,] Bitmap { get; private set; } = new int[6, 6];
    // Stored as [column/row, wall]
    public Wall[,] ColumnWalls { get; private set; }
    public Wall[,] RowWalls { get; private set; }

    public int[] GetBitColumn(int colNum, bool topToBottom = false) {
        var column = new int[6];

        for (int rowNum = 0; rowNum < 6; rowNum++) {
            column[rowNum] = Bitmap[colNum, rowNum];
        }
        if (!topToBottom) {
            Array.Reverse(column);
        }

        return column;
    }

    public int[] GetBitRow(int rowNum, bool leftToRight = false) {
        var row = new int[6];

        for (int colNum = 0; colNum < 6; colNum++) {
            row[colNum] = Bitmap[colNum, rowNum];
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

    public Wall GetAdjacentWallInDirection(Vector2Int cell, int direction) {
        if (direction % 2 == 0) {
            return ColumnWalls[cell.x, cell.y + direction / 2];
        }
        else {
            return RowWalls[cell.y, cell.x + (1 - direction / 2)];
        }
    }

    public Wall GetAdjacentWallInDirection(Vector2Int cell, MazeDirection direction) {
        return GetAdjacentWallInDirection(cell, (int)direction);
    }

    public Wall[] GetAdjacentWalls(Vector2Int cell, MazeDirection startingDirection = MazeDirection.Up) {
        var walls = new Wall[4];

        for (int direction = 0; direction < 4; direction++) {
            walls[direction] = GetAdjacentWallInDirection(cell, ((int)startingDirection + direction) % 4);
        }

        return walls;
    }

    public Wall[] GetAdjancentUndecidedWalls(Vector2Int cell, MazeDirection startingDirection = MazeDirection.Up) {
        return GetAdjacentWalls(cell, startingDirection).Where(w => w.IsDecided == false).ToArray();
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
    }
}
