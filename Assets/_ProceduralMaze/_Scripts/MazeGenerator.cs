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
        SetInitialWalls(maze);
        AssignRandomBitmapAndSeed(maze);
        return maze;
    }

    private static void SetInitialWalls(BitMaze6x6 maze) {
        // Outer walls present
        for (int line = 0; line < 6; line++) {
            maze.RowWalls[line, 0].IsPresent = true;
            maze.RowWalls[line, 6].IsPresent = true;
            maze.ColumnWalls[line, 0].IsPresent = true;
            maze.ColumnWalls[line, 6].IsPresent = true;
        }

        // Walls around start cell absent
        Array.ForEach(maze.GetAdjancentUndecidedWalls(maze.StartPosition), w => w.IsPresent = false);
    }

    public static void AssignRandomBitmapAndSeed(BitMaze6x6 maze) {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                maze.Bitmap[col, row] = Rnd.Range(0, 3) == 0 ? 1 : 0;
            }
        }

        for (int line = 0; line < 6; line++) {
            if (!maze.GetBitColumn(line).Contains(1)) {
                maze.Bitmap[line, Rnd.Range(0, 6)] = 1;
            }
            if (!maze.GetBitRow(line).Contains(1)) {
                maze.Bitmap[Rnd.Range(0, 6), line] = 1;
            }
        }

        var seed = string.Empty;

        for (int col = 0; col < 6; col++) {
            seed += ((maze.GetBitColumn(col).Count(b => b == 1) + 1) % 2).ToString();
        }

        maze.CurrentSeed = seed;
    }

    public static BitMaze6x6.Wall[] DecideWallsAroundCell(BitMaze6x6 maze, Vector2Int cell, MazeDirection directionMoved) {
        string seed = maze.CurrentSeed;
        int[] bitmask = maze.GetBitLineInDirection(cell, (MazeDirection)((int)(directionMoved + 1) % 4));
        BitMaze6x6.Wall[] walls = maze.GetAdjancentUndecidedWalls(cell, (MazeDirection)((int)(directionMoved + 2) % 4));

        foreach (BitMaze6x6.Wall wall in walls) {
            bool even = true;

            for (int pos = 0; pos < 6; pos++) {
                if (seed[pos] == '1' && bitmask[pos] == 1) {
                    even = !even;
                }    
            }

            wall.IsPresent = even;
            seed = seed.Insert(0, even ? "1" : "0").Remove(6, 1);
        }

        maze.CurrentSeed = seed;
        return walls;
    }
}
