using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeTraverser {

    // Up and Down are swapped because we are using Y going down instead of up.
    public static readonly Dictionary<MazeDirection, Vector2Int> DirectionVectors = new Dictionary<MazeDirection, Vector2Int> {
        { MazeDirection.Up, Vector2Int.down },
        { MazeDirection.Right, Vector2Int.right },
        { MazeDirection.Down, Vector2Int.up },
        { MazeDirection.Left, Vector2Int.left },
    };
    
    private Stack<Vector2Int> _visitedCells = new Stack<Vector2Int>();
    private Stack<BitMaze6x6.Wall[]> _decidedWalls = new Stack<BitMaze6x6.Wall[]>();
    private Stack<string> _pastSeeds = new Stack<string>();

    public MazeTraverser() {
        Maze = MazeGenerator.GenerateNewMaze();
        CurrentPosition = Maze.StartPosition;
    }

    public BitMaze6x6 Maze { get; private set; }
    public Vector2Int CurrentPosition { get; private set; }

    public bool TryMove(MazeDirection direction) {
        if (Maze.GetAdjacentWallInDirection(CurrentPosition, direction).IsPresent) {
            return false;
        }
        else {
            Vector2Int newPosition = CurrentPosition + DirectionVectors[direction];
            _visitedCells.Push(CurrentPosition);
            _pastSeeds.Push(Maze.CurrentSeed);
            CurrentPosition = newPosition;

            if (!_visitedCells.Contains(CurrentPosition)) {
                _decidedWalls.Push(MazeGenerator.DecideWallsAroundCell(Maze, CurrentPosition, direction));
            }

            //!
            Debug.Log(Maze.CurrentSeed);
            return true;
        }
    }

    public void UndoMove() {
        if (!_visitedCells.Contains(CurrentPosition)) {
            Array.ForEach(_decidedWalls.Pop(), w => w.Reset());
        }
        CurrentPosition = _visitedCells.Pop();
        Maze.CurrentSeed = _pastSeeds.Pop();
    }
}
