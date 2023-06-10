using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeSolver {

    private static BitMaze6x6 _maze;

    public static bool TrySolve(BitMaze6x6 maze, Vector2Int currentPosition, out string solution) {
        _maze = maze;
        solution = string.Empty;
        return true;
    }

    private static bool MazeIsBlocked(Vector2Int currentPosition) {
        var accessibleCells = new List<Vector2Int> { currentPosition };
        var currentDepth = new List<Vector2Int> { currentPosition };
        var nextDepth = new List<Vector2Int>();

        while (currentDepth.Count != 0) {
            foreach (Vector2Int cell in currentDepth) {
                foreach (int direction in Enum.GetValues(typeof(MazeDirection))) {
                    BitMaze6x6.Wall wall = _maze.GetAdjacentWallInDirection(cell, direction);

                    if (!wall.IsDecided || !wall.IsPresent) {
                        Vector2Int newCell = cell + MazeHandler.DirectionVectors[(MazeDirection)direction];
                        if (newCell == _maze.GoalPosition) {
                            return false;
                        }
                        
                        if (!accessibleCells.Contains(newCell)) {
                            accessibleCells.Add(newCell);
                            nextDepth.Add(newCell);
                        }
                    }
                }
            }

            currentDepth = nextDepth.ToList();
            nextDepth.Clear();
        }

        return true;
    }

    private struct Move : IEquatable<Move> {
        public Vector2Int FromCell { get; private set; }
        public Vector2Int ToCell { get; private set; }

        public Move(Vector2Int fromCell, Vector2Int toCell) {
            FromCell = fromCell;
            ToCell = toCell;
        }

        public bool Equals(Move other) => FromCell == other.FromCell && ToCell == other.ToCell;
    }
}