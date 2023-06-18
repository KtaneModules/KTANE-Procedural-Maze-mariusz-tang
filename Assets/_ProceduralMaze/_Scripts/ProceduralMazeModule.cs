using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private ArrowButton[] _arrows;
    private MazeHandler _mazeHandler;
    private MazeRenderer _mazeRenderer;

    private void Awake() {
        _arrows = GetComponent<KMSelectable>().Children.Select(c => c.GetComponent<ArrowButton>()).ToArray();
        _mazeHandler = new MazeHandler();
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
        _mazeRenderer.AssignMaze(_mazeHandler.Maze);

    }

    private void Start() {
        foreach (ArrowButton arrow in _arrows) {
            arrow.Selectable.OnInteract += delegate () { HandlePress(arrow); return false; };
        }
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
