using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public static class MazeGenerator {

    public static BitMaze6x6 GenerateNewMaze() {
        Vector2Int start = new Vector2Int(Rnd.Range(0, 2) * 5, Rnd.Range(0, 2) * 5);
        Vector2Int end = Vector2Int.one * 5 - start;
        var maze = new BitMaze6x6(start, end);
        SetOuterWalls(maze);
        AssignRandomBitmap(maze);
        return maze;
    }

    private static void SetOuterWalls(BitMaze6x6 maze) {
        for (int wall = 0; wall < 6; wall++) {
            maze.RowWalls[0, wall].IsPresent = true;
            maze.RowWalls[6, wall].IsPresent = true;
            maze.ColumnWalls[0, wall].IsPresent = true;
            maze.ColumnWalls[6, wall].IsPresent = true;
        }
    }

    public static void AssignRandomBitmap(BitMaze6x6 maze) {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                maze.Bitmap[col, row] = Rnd.Range(0, 2);
            }
        }

        for (int line = 0; line < 6; line++) {
            if (!maze.GetBitColumn(line, false).Contains(1)) {
                maze.Bitmap[line, Rnd.Range(0, 6)] = 1;
            }
            if (!maze.GetBitRow(line, false).Contains(1)) {
                maze.Bitmap[Rnd.Range(0, 6), line] = 1;
            }
        }
    }
}
