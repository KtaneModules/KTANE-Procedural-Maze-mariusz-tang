using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    [SerializeField] private KMBombModule _module;

    private KMAudio _audio;

    private ArrowButton[] _arrows;
    private MazeHandler _mazeHandler;
    private MazeRenderer _mazeRenderer;

    private static bool _usingExtraThread;
    private static int _moduleCount;
    private int _moduleId;

    private bool _loadingMaze;

    private Coroutine _holdTracker;
    private bool _trackingHold = false;
    private KMAudio.KMAudioRef _holdSound;

    private void Awake() {
        _moduleId = _moduleCount++;

        _audio = GetComponent<KMAudio>();

        _arrows = GetComponent<KMSelectable>().Children.Select(c => c.GetComponent<ArrowButton>()).ToArray();
        _mazeHandler = new MazeHandler();
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();

        Log("To read the example solutions, follow the directions reaching a coordinate, then go to that coordinate without visiting any new cells, then continue following the directions, and so on.");
    }

    private void Start() {
        foreach (ArrowButton arrow in _arrows) {
            arrow.Selectable.OnInteract += () => { _holdTracker = StartCoroutine(HandleHold(arrow)); return false; };
            arrow.Selectable.OnInteractEnded += () => { HandleRelease(arrow); };
        }

        _mazeRenderer.AssignMaze(_mazeHandler.Maze);
        StartCoroutine(LoadMaze());
    }

    private IEnumerator LoadMaze() {
        _loadingMaze = true;
        _mazeHandler.IsReady = false;

        _mazeRenderer.RenderMovementTo(_mazeHandler.Maze.StartCell.Position);
        StartCoroutine(_mazeRenderer.HideRings());
        // Adapted from Obvi's Raster Prime source code.
        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, 0.5f));
        do {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
        } while (_usingExtraThread);
        // Thread start
        _usingExtraThread = true;
        string solution;
        var thread = new Thread(() => {
            do {
                _mazeHandler.ResetMaze();
            } while (!MazeSolver.TrySolve(_mazeHandler, out solution));
            Log($"One possible solution is {solution}");
        });
        thread.Start();

        int count = 0;
        while (!_mazeHandler.IsReady) {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
            count++;
        }
        // Thread finish
        _usingExtraThread = false;
        _audio.PlaySoundAtTransform("MazeGeneration", transform);
        StartCoroutine(_mazeRenderer.ShowRings());
        while (count < 2) {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
            count++;
        }
        _loadingMaze = false;
    }

    private IEnumerator HandleHold(ArrowButton arrow) {
        if (_loadingMaze) {
            yield break;
        }
        _holdSound = _audio.PlaySoundAtTransformWithRef("ButtonHold", transform);
        _trackingHold = true;

        float heldTime = 0;
        int level = 0;
        while (heldTime < 1) {
            yield return null;
            heldTime += Time.deltaTime;
        }
        while (heldTime < 3) {
            if (Mathf.FloorToInt((heldTime - 1) / 2 * 11) >= level) {
                level += 1;
                _mazeRenderer.ShowHoldProgress(level);
            }
            heldTime += Time.deltaTime;
            yield return null;
        }
        _trackingHold = false;
        _holdSound.StopSound();
        _mazeRenderer.ShowHoldProgress(0);
        _audio.PlaySoundAtTransform("ButtonHoldSuccess", transform);
        StartCoroutine(LoadMaze());
    }

    private void HandleRelease(ArrowButton arrow) {
        if (!_trackingHold) {
            return;
        }
        _trackingHold = false;
        if (_holdTracker != null) {
            StopCoroutine(_holdTracker);
        }
        _mazeRenderer.ShowHoldProgress(0);
        _holdSound?.StopSound();
        HandleMovePress(arrow);
    }

    private void HandleMovePress(ArrowButton arrow) {
        if (_mazeHandler.TryMove(arrow.Direction)) {
            _mazeRenderer.RenderMovementTo(_mazeHandler.CurrentPosition);
            if (_mazeHandler.CurrentPosition == _mazeHandler.Maze.GoalCell.Position) {
                Solve();
            }
        }
        else {
            Strike("breh");
        }
    }

    public void Log(string message) {
        Debug.Log($"[Procedural Maze #{_moduleId}] {message}");
    }

    private void Strike(string message) {
        _mazeHandler.IsReady = false;
        Log(message);
        _module.HandleStrike();
        StartCoroutine(StrikeAnimation());
    }

    private IEnumerator StrikeAnimation() {
        _audio.PlaySoundAtTransform("StrikeInitial", transform);
        StartCoroutine(_mazeRenderer.ShowWalls());
        yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.StrikeX, Color.red, 0.6f));
        _audio.PlaySoundAtTransform("StrikeAnimation", transform);
        StartCoroutine(_mazeRenderer.HideWalls());
        yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.Layers, Color.red, 1f));
        StartCoroutine(LoadMaze());
    }

    private void Solve() {
        _mazeHandler.IsReady = false;
        _module.HandlePass();
        PlaySolveAnimation();
    }

    private void PlaySolveAnimation() {
        _audio.PlaySoundAtTransform("Solve", transform);
        StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.SpiralOut, Color.green, 0.5f));
        StartCoroutine(WallsFlash());
    }

    private IEnumerator WallsFlash() {
        while (true) {
            yield return StartCoroutine(_mazeRenderer.ShowWalls());
            yield return StartCoroutine(_mazeRenderer.HideWalls());
        }
    }
}
