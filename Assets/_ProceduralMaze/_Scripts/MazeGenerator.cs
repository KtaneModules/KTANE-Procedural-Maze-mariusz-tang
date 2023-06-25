using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeGenerator {

    // Use System.Random because UnityEngine.Random cannot be used in threads other than the main thread.
    private static System.Random _rnd = new System.Random();

    public static BitMaze6x6 GenerateNewMaze(out string seed) {
        Vector2Int start = new Vector2Int(_rnd.Next(0, 2) * 5, _rnd.Next(0, 2) * 5);
        Vector2Int end = Vector2Int.one * 5 - start;
        var maze = new BitMaze6x6(start, end);
        AssignRandomBitmap(maze, out seed);
        return maze;
    }

    public static void AssignRandomBitmap(BitMaze6x6 maze, out string seed) {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                maze.Cells[col, row].Bit = _rnd.Next(0, 3) == 0 ? 1 : 0;
            }
        }

        for (int line = 0; line < 6; line++) {
            if (!maze.GetBitColumn(line).Contains(1)) {
                maze.Cells[line, _rnd.Next(0, 6)].Bit = 1;
            }
            if (!maze.GetBitRow(line).Contains(1)) {
                maze.Cells[_rnd.Next(0, 6), line].Bit = 1;
            }
        }
        seed = string.Empty;

        for (int col = 0; col < 6; col++) {
            seed += ((maze.GetBitColumn(col).Count(b => b == 1) + 1) % 2).ToString();
        }
    }

    public static BitMaze6x6.Wall[] DecideWallsAroundCell(BitMaze6x6 maze, BitMaze6x6.Cell cell, MazeDirection directionMoved, ref string seed, out string logging) {
        // This is messy but we are getting the directions one and two clockwise from directionMoved.
        int[] bitmask = maze.GetBitLineInDirection(cell.Position, (MazeDirection)((int)(directionMoved + 1) % 4));
        
        var walls = new List<BitMaze6x6.Wall>();
        logging = string.Empty;

        for (int i = 0; i < 4; i++) {
            MazeDirection direction = (MazeDirection)i;
            BitMaze6x6.Wall wall = cell.GetAdjacentWall(direction);
            if (!wall.IsDecided) {
                bool even = true;

                for (int pos = 0; pos < 6; pos++) {
                    if (seed[pos] == '1' && bitmask[pos] == 1) {
                        even = !even;
                    }
                }

                wall.IsPresent = even;
                seed = seed.Insert(0, even ? "1" : "0").Remove(6, 1);
                walls.Add(wall);
                if (logging != string.Empty) {
                    logging += " | ";
                }
                logging += $"{direction}: {(wall.IsPresent ? "present" : "absent")}";
            }
        }

        return walls.ToArray();
    }
}
