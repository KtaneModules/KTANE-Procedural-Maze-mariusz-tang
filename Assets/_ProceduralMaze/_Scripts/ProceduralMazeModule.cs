using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private ArrowButton[] _arrows;
    private MazeHandler _mazeHandler;
    private MazeRenderer _mazeRenderer;

    private static int _moduleCount;
    private int _moduleId;

    private void Awake() {
        _moduleId = _moduleCount++;

        _arrows = GetComponent<KMSelectable>().Children.Select(c => c.GetComponent<ArrowButton>()).ToArray();
        _mazeHandler = new MazeHandler();
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
        _mazeRenderer.AssignMaze(_mazeHandler.Maze);
    }

    private IEnumerator Start() {
        foreach (ArrowButton arrow in _arrows) {
            arrow.Selectable.OnInteract += delegate () { HandlePress(arrow); return false; };
        }
        yield return new WaitForSeconds(1 + _moduleId);

        string breh;
        while (!MazeSolver.TrySolve(_mazeHandler, out breh)) {
            _mazeHandler.ResetMaze();
            _mazeRenderer.RenderRings();
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log(breh);

    }

    private void HandlePress(ArrowButton arrow) {
        if (_mazeHandler.TryMove(arrow.Direction)) {
            _mazeRenderer.RenderMovementTo(_mazeHandler.CurrentPosition);
            _mazeRenderer.RenderWalls();
        }
        else {
            Debug.Log("breh");
        }
    }
}
