using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private ArrowButton[] _arrows;
    private MazeHandler _mazeHandler;
    private MazeRenderer _mazeRenderer;

    private static bool _usingExtraThread;
    private static int _moduleCount;
    private int _moduleId;

    private void Awake() {
        _moduleId = _moduleCount++;

        _arrows = GetComponent<KMSelectable>().Children.Select(c => c.GetComponent<ArrowButton>()).ToArray();
        _mazeHandler = new MazeHandler();
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
        _mazeRenderer.AssignMaze(_mazeHandler.Maze);

        Log("To read the example solutions, follow the directions reaching a coordinate, then go to that coordinate without visiting any new cells, then continue following the directions, and so on.");
    }

    private void Start() {
        foreach (ArrowButton arrow in _arrows) {
            arrow.Selectable.OnInteract += delegate () { HandlePress(arrow); return false; };
        }

        StartCoroutine(LoadMaze());
    }

    private IEnumerator LoadMaze() {
        _mazeHandler.IsReady = false;

        // Adapted from Obvi's Raster Prime source code.
        do {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, 0.5f));
        } while (_usingExtraThread);
        // Thread start
        _usingExtraThread = true;
        string solution;
        Log("bruh");
        var thread = new Thread(() => {
            while (!MazeSolver.TrySolve(_mazeHandler, out solution)) {
                _mazeHandler.ResetMaze();
            }
            Log($"One possible solution is {solution}");
        });
        thread.Start();
        yield return new WaitUntil(() => _mazeHandler.IsReady);
        // Thread finish

        _usingExtraThread = false;

        _mazeRenderer.RenderRings();
    }

    private void HandlePress(ArrowButton arrow) {
        if (_mazeHandler.TryMove(arrow.Direction)) {
            _mazeRenderer.RenderMovementTo(_mazeHandler.CurrentPosition);
            _mazeRenderer.RenderWalls();
        }
        else {
            Log("breh");
        }
    }

    public void Log(string message) {
        Debug.Log($"[Procedural Maze #{_moduleId}] {message}");
    }
}
