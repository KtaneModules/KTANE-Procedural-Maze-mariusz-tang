using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public static class MazeGenerator {

    public static BitMaze6x6 GenerateNewMaze(out string seed) {
        Vector2Int start = new Vector2Int(Rnd.Range(0, 2) * 5, Rnd.Range(0, 2) * 5);
        Vector2Int end = Vector2Int.one * 5 - start;
        var maze = new BitMaze6x6(start, end);
        AssignRandomBitmap(maze, out seed);
        return maze;
    }

    public static void AssignRandomBitmap(BitMaze6x6 maze, out string seed) {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                maze.Cells[col, row].Bit = Rnd.Range(0, 3) == 0 ? 1 : 0;
            }
        }

        for (int line = 0; line < 6; line++) {
            if (!maze.GetBitColumn(line).Contains(1)) {
                maze.Cells[line, Rnd.Range(0, 6)].Bit = 1;
            }
            if (!maze.GetBitRow(line).Contains(1)) {
                maze.Cells[Rnd.Range(0, 6), line].Bit = 1;
            }
        }
        seed = string.Empty;

        for (int col = 0; col < 6; col++) {
            seed += ((maze.GetBitColumn(col).Count(b => b == 1) + 1) % 2).ToString();
        }
    }

    public static BitMaze6x6.Wall[] DecideWallsAroundCell(BitMaze6x6 maze, BitMaze6x6.Cell cell, MazeDirection directionMoved, ref string seed) {
        // This is messy but we are getting the directions one and two clockwise from directionMoved.
        int[] bitmask = maze.GetBitLineInDirection(cell.Position, (MazeDirection)((int)(directionMoved + 1) % 4));
        MazeDirection fromDirection = (MazeDirection)(((int)directionMoved + 2) % 4);
        BitMaze6x6.Wall[] walls = cell.GetAdjacentWallsClockFromDirection(fromDirection).Where(w => !w.IsDecided).ToArray();

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

        return walls;
    }
}
