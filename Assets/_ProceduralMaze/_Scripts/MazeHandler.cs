using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeHandler {

    // Up and Down are swapped because we are using Y going down instead of up.
    private Dictionary<MazeDirection, Vector2Int> _directionVectors = new Dictionary<MazeDirection, Vector2Int> {
        { MazeDirection.Up, Vector2Int.down },
        { MazeDirection.Right, Vector2Int.right },
        { MazeDirection.Down, Vector2Int.up },
        { MazeDirection.Left, Vector2Int.left },
    };

    private ProceduralMazeModule _module;
    private MazeRenderer _renderer;
    private BitMaze6x6 _maze;

    private Vector2Int _currentPosition;
    private Stack<Vector2Int> _visitedCells = new Stack<Vector2Int>();
    private Stack<BitMaze6x6.Wall[]> _decidedWalls = new Stack<BitMaze6x6.Wall[]>();
    private Stack<string> _pastSeeds = new Stack<string>();

    public MazeHandler(ProceduralMazeModule module) {
        _module = module;
        _maze = MazeGenerator.GenerateNewMaze();
        _currentPosition = _maze.StartPosition;

        _renderer = module.GetComponentInChildren<MazeRenderer>();
        _renderer.AssignMaze(_maze);
        _renderer.RenderRings();
    }

    public void Move(MazeDirection direction) {
        if (_maze.GetAdjacentWallInDirection(_currentPosition, direction).IsPresent) {
            Debug.Log("Bruh");
        }
        else {
            Vector2Int newPosition = _currentPosition + _directionVectors[direction];
            _renderer.RenderMovement(_currentPosition, newPosition);
            _visitedCells.Push(_currentPosition);
            _pastSeeds.Push(_maze.CurrentSeed);
            _currentPosition = newPosition;

            if (!_visitedCells.Contains(_currentPosition)) {
                _decidedWalls.Push(MazeGenerator.DecideWallsAroundCell(_maze, _currentPosition, direction));
            }


            //!
            Debug.Log(_maze.CurrentSeed);
            _renderer.RenderWalls();
        }
    }

    public void UndoMove() {
        _renderer.RenderMovement(_currentPosition, _visitedCells.Peek());

        if (!_visitedCells.Contains(_currentPosition)) {
            Array.ForEach(_decidedWalls.Pop(), w => w.Reset());
        }
        _currentPosition = _visitedCells.Pop();
        _maze.CurrentSeed = _pastSeeds.Pop();
        _renderer.RenderWalls();
    }
}
