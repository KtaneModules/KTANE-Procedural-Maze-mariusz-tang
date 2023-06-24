using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeHandler {

    // Up and Down are swapped because we are using Y going down instead of up.
    public static readonly Dictionary<MazeDirection, Vector2Int> DirectionVectors = new Dictionary<MazeDirection, Vector2Int> {
        { MazeDirection.Up, Vector2Int.down },
        { MazeDirection.Right, Vector2Int.right },
        { MazeDirection.Down, Vector2Int.up },
        { MazeDirection.Left, Vector2Int.left },
    };

    private BitMaze6x6.Cell _currentCell;
    private string _seed;

    private Stack<Movement> _moveHistory = new Stack<Movement>();
    private Stack<BitMaze6x6.Cell> _visitedCells = new Stack<BitMaze6x6.Cell>();

    public MazeHandler() {
        Maze = MazeGenerator.GenerateNewMaze(out _seed);
        _currentCell = Maze.StartCell;
        _visitedCells.Push(_currentCell);
    }

    public bool IsReady { get; set; }

    public BitMaze6x6 Maze { get; private set; }
    public Vector2Int CurrentPosition {
        get { return _currentCell.Position; }
        set { _currentCell = Maze.Cells[value.x, value.y]; }
    }
    public IEnumerable<BitMaze6x6.Cell> VisitedCells { get { return _visitedCells.ToArray().Distinct(); } }

    public string Solution { get; set; }
    public bool HasMoved { get; set; }

    public bool HasVisited(BitMaze6x6.Cell cell) => _visitedCells.Contains(cell);

    public bool TryMove(MazeDirection direction) {
        if (_currentCell.GetAdjacentWall(direction).IsPresent) {
            return false;
        }

        string oldSeed = _seed;
        BitMaze6x6.Cell newCell = _currentCell.GetNeighbour(direction);
        BitMaze6x6.Wall[] decidedWalls = MazeGenerator.DecideWallsAroundCell(Maze, newCell, direction, ref _seed);
        _moveHistory.Push(new Movement(_currentCell, decidedWalls, oldSeed));
        _currentCell = newCell;
        _visitedCells.Push(_currentCell);

        return true;
    }

    public void Teleport(BitMaze6x6.Cell toCell) {
        if (!HasVisited(toCell)) {
            throw new ArgumentException("Can only teleport to cells that have been visited before!");
        }
        _moveHistory.Push(new Movement(_currentCell, new BitMaze6x6.Wall[0], _seed));
        _currentCell = toCell;
        _visitedCells.Push(_currentCell);
    }

    public void UndoMove() {
        Movement move = _moveHistory.Pop();
        _visitedCells.Pop();
        _currentCell = move.FromCell;
        Array.ForEach(move.RevealedWalls, w => w.Reset());
        _seed = move.OldSeed;
    }

    public void ResetMaze() {
        while (_visitedCells.Count() > 1) {
            UndoMove();
        }
        MazeGenerator.AssignRandomBitmap(Maze, out _seed);
    }

    public class Movement {

        public BitMaze6x6.Cell FromCell { get; private set; }
        public BitMaze6x6.Wall[] RevealedWalls { get; private set; }
        public string OldSeed { get; private set; }

        public Movement(BitMaze6x6.Cell fromCell, BitMaze6x6.Wall[] revealedWalls, string oldSeed) {
            FromCell = fromCell;
            RevealedWalls = revealedWalls;
            OldSeed = oldSeed;
        }
    }
}
