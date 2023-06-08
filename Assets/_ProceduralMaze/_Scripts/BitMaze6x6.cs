using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitMaze6x6 {

    public BitMaze6x6(Vector2Int start, Vector2Int goal) {
        StartPosition = start;
        GoalPosition = goal;

        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 5; wall++) {
                ColumnWalls[line, wall] = new Wall();
                RowWalls[line, wall] = new Wall();
            }
        }
    }

    public BitMaze6x6(BitMaze6x6 original) {
        StartPosition = original.StartPosition;
        GoalPosition = original.GoalPosition;
        BitMap = original.BitMap;
        ColumnWalls = original.ColumnWalls;
        RowWalls = original.RowWalls;
    }

    // Stored as [column, row].
    public Vector2Int StartPosition { get; private set; }
    public Vector2Int GoalPosition { get; private set; }
    public int[,] BitMap { get; private set; } = new int[6, 6];
    // Stored as [column/row, wall]
    public Wall[,] ColumnWalls { get; private set; } = new Wall[6, 5];
    public Wall[,] RowWalls { get; private set; } = new Wall[6, 5];

    public int[] GetBitColumn(int colNum, bool topToBottom) {
        var column = new int[6];

        for (int rowNum = 0; rowNum < 6; rowNum++) {
            column[rowNum] = BitMap[colNum, rowNum];
        }
        if (!topToBottom) {
            Array.Reverse(column);
        }

        return column;
    }

    public int[] GetBitRow(int rowNum, bool leftToRight) {
        var row = new int[6];

        for (int colNum = 0; colNum < 6; colNum++) {
            row[colNum] = BitMap[colNum, rowNum];
        }
        if (!leftToRight) {
            Array.Reverse(row);
        }

        return row;
    }

    public class Wall {
        private bool _isTraversable;

        public bool IsDecided { get; private set; } = false;
        public bool IsTraversable {
            get {
                if (!IsDecided) {
                    throw new InvalidOperationException("This wall's state has not yet been set!");
                }
                return _isTraversable;
            }
            set {
                if (IsDecided) {
                    throw new InvalidOperationException("This wall's state has already been set!");
                }
                IsDecided = true;
                _isTraversable = value;
            }
        }
    }
}
