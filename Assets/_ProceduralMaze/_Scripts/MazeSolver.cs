using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeSolver {

    private static BitMaze6x6 _maze;
    private static Vector2Int _currentPosition;


    public static bool TrySolve(BitMaze6x6 maze, Vector2Int currentPosition, out string solution) {
        _maze = maze;
        _currentPosition = currentPosition;
        solution = string.Empty;
        return true;
    }



    private static bool MazeIsBlocked() {
        var accessibleCells = new List<Vector2Int> { _currentPosition };
        var currentDepth = new List<Vector2Int> { _currentPosition };
        var nextDepth = new List<Vector2Int>();

        while (currentDepth.Count != 0) {
            foreach (Vector2Int cell in currentDepth) {
                foreach (int direction in Enum.GetValues(typeof(MazeDirection))) {
                    BitMaze6x6.Wall wall = _maze.GetAdjacentWallInDirection(cell, direction);

                    if (!wall.IsDecided || !wall.IsPresent) {
                        Vector2Int newCell = cell + MazeTraverser.DirectionVectors[(MazeDirection)direction];
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

    private static int SortMoves(Move lhs, Move rhs) {
        int lhsDistanceToCurrent = Math.Abs(lhs.FromCell.x - _currentPosition.x) + Math.Abs(lhs.FromCell.y - _currentPosition.y);
        int rhsDistanceToCurrent = Math.Abs(rhs.FromCell.x - _currentPosition.x) + Math.Abs(rhs.FromCell.y - _currentPosition.y);

        if (lhsDistanceToCurrent != rhsDistanceToCurrent) {
            return lhsDistanceToCurrent - rhsDistanceToCurrent;
        }
        else {
            int lhsDistanceToGoal = Math.Abs(lhs.FromCell.x - _maze.GoalPosition.x) + Math.Abs(lhs.FromCell.y - _maze.GoalPosition.y);
            int rhsDistanceToGoal = Math.Abs(rhs.FromCell.x - _maze.GoalPosition.x) + Math.Abs(rhs.FromCell.y - _maze.GoalPosition.y);
            return lhsDistanceToGoal - rhsDistanceToGoal;
        }
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