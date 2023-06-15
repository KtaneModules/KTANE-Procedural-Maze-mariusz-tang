using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeSolver {

    private static MazeTraverser _traverser;

    public static bool TrySolve(MazeTraverser traverser, out string solution) {
        solution = "breh";
        return false;
        // _traverser = new MazeTraverser(traverser);
        // solution = string.Empty;

        // var moveTree = new Stack<List<Move>>();
        // List<Move> currentMoveSet = GetSortedNextMoves();
        // Stack<Move> moveChain = new Stack<Move>();

        // //!
        // int count = 0;

        // while ((currentMoveSet.Any() || moveTree.Any()) && count < 50) {
        //     Debug.Log($"Set: {currentMoveSet.Count()} | Tree: {moveTree.Count()}");
        //     if (!currentMoveSet.Any()) {
        //         currentMoveSet = moveTree.Pop();
        //         moveChain.Pop();
        //         _traverser.UndoMove();
        //         Debug.Log("breh");
        //     }
        //     else {
        //         Move nextMove = currentMoveSet[0];
        //         currentMoveSet.RemoveAt(0);
        //         moveChain.Push(nextMove);

        //         if (nextMove.ToCell == _traverser.Maze.GoalPosition) {
        //             solution = StringifySolution(moveChain);
        //             return true;
        //         }

        //         _traverser.CurrentPosition = nextMove.FromCell;
        //         _traverser.TryMove(nextMove.GetDirection());

        //         if (MazeIsBlocked()) {
        //             _traverser.UndoMove();
        //             moveChain.Pop();
        //         }
        //         else {
        //             moveTree.Push(currentMoveSet);
        //             currentMoveSet = GetSortedNextMoves();
        //         }
        //         Debug.Log("neh");
        //     }
        //     count++;
        // }

        // return false;
    }

    private static List<Move> GetSortedNextMoves() {
        var nextMoves = new List<Move>();

        for (int row = 0; row < 6; row++) {
            for (int col = 0; col < 6; col++) {
                Vector2Int cell = new Vector2Int(row, col);
                TryGetMovesFromCell(cell, nextMoves);
            }
        }
        Debug.Log($"NextMoves: {nextMoves.Count()}");
        nextMoves.Sort((x, y) => SortMovesByDirectness(x, y));
        return nextMoves;
    }

    private static void TryGetMovesFromCell(Vector2Int cell, List<Move> moveList) {
        if (!_traverser.HasVisited(cell)) {
            return;
        }

        for (int d = 0; d < 4; d++) {
            MazeDirection direction = (MazeDirection)d;
            if (!_traverser.Maze.GetAdjacentWallInDirection(cell, direction).IsPresent) {
                Vector2Int newCell = cell + MazeTraverser.DirectionVectors[direction];
                if (!_traverser.HasVisited(newCell)) {
                    moveList.Add(new Move(cell, newCell));
                }
            }
        }
    }

    private static bool MazeIsBlocked() {
        var accessibleCells = new List<Vector2Int> { _traverser.CurrentPosition };
        var currentDepth = new List<Vector2Int> { _traverser.CurrentPosition };
        var nextDepth = new List<Vector2Int>();

        while (currentDepth.Count != 0) {
            foreach (Vector2Int cell in currentDepth) {
                foreach (int direction in Enum.GetValues(typeof(MazeDirection))) {
                    BitMaze6x6.Wall wall = _traverser.Maze.GetAdjacentWallInDirection(cell, direction);

                    if (!wall.IsDecided || !wall.IsPresent) {
                        Vector2Int newCell = cell + MazeTraverser.DirectionVectors[(MazeDirection)direction];
                        if (newCell == _traverser.Maze.GoalPosition) {
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

    private static int SortMovesByDirectness(Move lhs, Move rhs) {
        int lhsDistanceToCurrent = Math.Abs(lhs.FromCell.x - _traverser.CurrentPosition.x) + Math.Abs(lhs.FromCell.y - _traverser.CurrentPosition.y);
        int rhsDistanceToCurrent = Math.Abs(rhs.FromCell.x - _traverser.CurrentPosition.x) + Math.Abs(rhs.FromCell.y - _traverser.CurrentPosition.y);

        if (lhsDistanceToCurrent != rhsDistanceToCurrent) {
            return lhsDistanceToCurrent - rhsDistanceToCurrent;
        }
        else {
            int lhsDistanceToGoal = Math.Abs(lhs.FromCell.x - _traverser.Maze.GoalPosition.x) + Math.Abs(lhs.FromCell.y - _traverser.Maze.GoalPosition.y);
            int rhsDistanceToGoal = Math.Abs(rhs.FromCell.x - _traverser.Maze.GoalPosition.x) + Math.Abs(rhs.FromCell.y - _traverser.Maze.GoalPosition.y);
            return lhsDistanceToGoal - rhsDistanceToGoal;
        }
    }

    private static string StringifySolution(Stack<Move> moves) {
        moves.Reverse();
        return string.Empty;
    }

    private struct Move : IEquatable<Move> {
        private static Dictionary<Vector2Int, MazeDirection> _toMazeDirection = new Dictionary<Vector2Int, MazeDirection> {
            { Vector2Int.up, MazeDirection.Down },
            { Vector2Int.right, MazeDirection.Right },
            { Vector2Int.down, MazeDirection.Up },
            { Vector2Int.left, MazeDirection.Left },
        };

        public Vector2Int FromCell { get; private set; }
        public Vector2Int ToCell { get; private set; }

        public Move(Vector2Int fromCell, Vector2Int toCell) {
            FromCell = fromCell;
            ToCell = toCell;
        }

        public MazeDirection GetDirection() => _toMazeDirection[ToCell - FromCell];

        public bool Equals(Move other) => FromCell == other.FromCell && ToCell == other.ToCell;
    }
}