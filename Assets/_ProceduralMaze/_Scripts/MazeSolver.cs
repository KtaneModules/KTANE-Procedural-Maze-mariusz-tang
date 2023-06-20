using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MazeSolver {

    private static readonly IEnumerable<MazeDirection> _directions = Enumerable.Range(0, 4).Select(i => (MazeDirection)i);
    private static MazeHandler _traverser;

    public static bool TrySolve(MazeHandler traverser, out string solution) {
        _traverser = traverser;
        solution = string.Empty;

        Stack<List<RevealMove>> moveTree = new Stack<List<RevealMove>>();
        Stack<RevealMove> currentPath = new Stack<RevealMove>();

        moveTree.Push(GetNextMoves());

        int count = 0;
        while (moveTree.Any() && count < 2000) {
            List<RevealMove> currentMoveSet = moveTree.Peek();
            if (currentMoveSet.Any()) {
                RevealMove nextMove = currentMoveSet[0];
                currentMoveSet.RemoveAt(0);
                currentPath.Push(nextMove);

                _traverser.Teleport(nextMove.FromCell);
                _traverser.TryMove(nextMove.Direction);
                moveTree.Push(GetNextMoves());

                if (_traverser.CurrentPosition == _traverser.Maze.GoalCell.Position) {
                    solution = GetSolutionString(currentPath);
                    _traverser.IsReady = true;
                    return true;
                }
            }
            else {
                moveTree.Pop();

                if (moveTree.Any()) {
                    currentPath.Pop();
                    _traverser.UndoMove();
                    _traverser.UndoMove();
                }
            }
            count++;
        }
        return false;
    }

    private static List<RevealMove> GetNextMoves() {
        var moves = new List<RevealMove>();

        foreach (BitMaze6x6.Cell cell in _traverser.VisitedCells) {
            foreach (MazeDirection direction in _directions) {
                if (!cell.GetAdjacentWall(direction).IsPresent && !_traverser.HasVisited(cell.GetNeighbour(direction))) {
                    RevealMove move = new RevealMove(cell, direction);
                    moves.Add(new RevealMove(cell, direction));
                }
            }
        }
        moves.Sort((x, y) => SortMovesByDirectness(x, y));
        return moves;
    }

    private static int SortMovesByDirectness(RevealMove lhs, RevealMove rhs) {
        int lhsDistanceToGoal = Math.Abs(lhs.ToCell.Position.x - _traverser.Maze.GoalCell.Position.x) + Math.Abs(lhs.ToCell.Position.y - _traverser.Maze.GoalCell.Position.y);
        int rhsDistanceToGoal = Math.Abs(rhs.ToCell.Position.x - _traverser.Maze.GoalCell.Position.x) + Math.Abs(rhs.ToCell.Position.y - _traverser.Maze.GoalCell.Position.y);

        if (lhsDistanceToGoal != rhsDistanceToGoal) {
            return lhsDistanceToGoal - rhsDistanceToGoal;
        }
        else {
            int lhsDistanceToCurrent = Math.Abs(lhs.FromCell.Position.x - _traverser.CurrentPosition.x) + Math.Abs(lhs.FromCell.Position.y - _traverser.CurrentPosition.y);
            int rhsDistanceToCurrent = Math.Abs(rhs.FromCell.Position.x - _traverser.CurrentPosition.x) + Math.Abs(rhs.FromCell.Position.y - _traverser.CurrentPosition.y);
            return lhsDistanceToCurrent - rhsDistanceToCurrent;
        }
    }

    private static string GetSolutionString(Stack<RevealMove> path) {
        RevealMove move = path.Pop();
        RevealMove previousMove = new RevealMove(move.FromCell, move.Direction);
        string solution = $"{move.Direction.ToString()[0]}";
        _traverser.UndoMove();
        _traverser.UndoMove();

        while (path.Any()) {
            move = path.Pop();
            if (move.ToCell == previousMove.FromCell) {
                solution = $"{move.Direction.ToString()[0]}{solution}";
            }
            else {
                solution = $"{move.Direction.ToString()[0]} | {"ABCDEF"[previousMove.FromCell.Position.x]}{previousMove.FromCell.Position.y + 1} {solution}";
            }
            _traverser.UndoMove();
            _traverser.UndoMove();
            previousMove = new RevealMove(move.FromCell, move.Direction);
        }

        return solution;
    }

    public class RevealMove {

        public RevealMove(BitMaze6x6.Cell fromCell, MazeDirection direction) {
            FromCell = fromCell;
            Direction = direction;
            ToCell = fromCell.GetNeighbour(direction);
        }

        public BitMaze6x6.Cell FromCell { get; private set; }
        public BitMaze6x6.Cell ToCell { get; private set; }
        public MazeDirection Direction { get; private set; }
    }

}