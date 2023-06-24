using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class MazeSolver {

    private static readonly IEnumerable<MazeDirection> _directions = Enumerable.Range(0, 4).Select(i => (MazeDirection)i);
    private static MazeHandler _traverser;
    private static int _initialVisitedSquaresCount;

    private static Dictionary<MazeDirection, string> _directionOppositeLetters = new Dictionary<MazeDirection, string> {
        {MazeDirection.Up, "D"},
        {MazeDirection.Down, "U"},
        {MazeDirection.Left, "R"},
        {MazeDirection.Right, "L"},
    };

    public static bool TrySolve(MazeHandler traverser, out string solution) {
        _traverser = traverser;
        solution = string.Empty;
        _initialVisitedSquaresCount = _traverser.VisitedCells.Count();

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
        var moves = new Stack<RevealMove>();
        var moveString = string.Empty;

        while (path.Any()) {
            moves.Push(path.Pop());
            _traverser.UndoMove();
            _traverser.UndoMove();
        }

        while (moves.Any()) {
            RevealMove move = moves.Pop();
            moveString += PathfindToCell(move.FromCell);
            moveString += "URDL"[(int)move.Direction];
            _traverser.TryMove(move.Direction);
        }

        while (_traverser.VisitedCells.Count() > _initialVisitedSquaresCount) {
            _traverser.UndoMove();
        }
        Debug.Log($"{_traverser.CurrentPosition.x} {_traverser.CurrentPosition.y}");
        return moveString;
    }

    private static string PathfindToCell(BitMaze6x6.Cell cell) {
        var moves = string.Empty;
        if (_traverser.CurrentPosition == cell.Position) {
            return moves;
        }

        var currentDepth = new List<BitMaze6x6.Cell> { cell };
        var moveTreeFromGoal = new Dictionary<BitMaze6x6.Cell, MazeDirection>();
        BitMaze6x6.Cell startCell = _traverser.Maze.Cells[_traverser.CurrentPosition.x, _traverser.CurrentPosition.y];

        while (!moveTreeFromGoal.ContainsKey(startCell)) {
            var newDepth = new List<BitMaze6x6.Cell>();
            foreach (BitMaze6x6.Cell c in currentDepth) {
                for (int i = 0; i < 4; i++) {
                    var direction = (MazeDirection)i;
                    BitMaze6x6.Wall currentWall = c.GetAdjacentWall(direction);
                    if (currentWall.IsDecided && !currentWall.IsPresent) {
                        BitMaze6x6.Cell currentNeighbour = c.GetNeighbour(direction);
                        if (_traverser.HasVisited(currentNeighbour) && !moveTreeFromGoal.ContainsKey(currentNeighbour)) {
                            moveTreeFromGoal.Add(currentNeighbour, direction);
                            newDepth.Add(currentNeighbour);
                        }
                    }
                }
            }
            currentDepth = newDepth.ToList();
        }

        while (_traverser.CurrentPosition != cell.Position) {
            MazeDirection direction = moveTreeFromGoal[_traverser.Maze.Cells[_traverser.CurrentPosition.x, _traverser.CurrentPosition.y]];
            _traverser.TryMove((MazeDirection)(((int)direction + 2) % 4));
            moves += _directionOppositeLetters[direction];
        }
        return moves;
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