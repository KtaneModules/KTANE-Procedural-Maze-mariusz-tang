using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    [SerializeField] private KMBombModule _module;

    private KMAudio _audio;

    private ArrowButton[] _arrows;
    private MazeHandler _mazeHandler;
    private MazeRenderer _mazeRenderer;

    private static bool _usingExtraThread = false;
    private static int _moduleCount;
    private int _moduleId;
    private bool _isSolved = false;

    private bool _isLoadingMaze;
    // For souvenir.
    private string _initialSeed;

    private Coroutine _holdTracker;
    private bool _trackingHold = false;
    private KMAudio.KMAudioRef _holdSound;

    private void Awake() {
        _moduleId = _moduleCount++;

        _audio = GetComponent<KMAudio>();

        _arrows = GetComponent<KMSelectable>().Children.Select(c => c.GetComponent<ArrowButton>()).ToArray();
        _mazeHandler = new MazeHandler(this);
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
    }

    private void Start() {
        foreach (ArrowButton arrow in _arrows) {
            arrow.Selectable.OnInteract += () => { _holdTracker = StartCoroutine(HandleHold()); return false; };
            arrow.Selectable.OnInteractEnded += () => { HandleRelease(arrow); };
        }

        _mazeRenderer.AssignMaze(_mazeHandler.Maze);
        Log($"Mazes on this module will start at {"ABCDEF"[_mazeHandler.Maze.StartCell.Position.x]}{_mazeHandler.Maze.StartCell.Position.y + 1} and finish at {"ABCDEF"[_mazeHandler.Maze.GoalCell.Position.x]}{_mazeHandler.Maze.GoalCell.Position.y + 1}.");
        StartCoroutine(LoadMaze(isInitialGeneration: true));
    }

    private IEnumerator LoadMaze(bool isInitialGeneration = false) {
        _isLoadingMaze = true;
        _mazeHandler.IsReady = false;

        Log(isInitialGeneration ? "Generating maze:" : "Regenerating maze:");
        _mazeRenderer.RenderMovementTo(_mazeHandler.Maze.StartCell.Position);
        StartCoroutine(_mazeRenderer.HideRings());

        // Adapted from Obvi's Raster Prime source code.
        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, 0.5f));
        do {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
        } while (_usingExtraThread);

        // Thread start
        _usingExtraThread = true;

        var thread = new Thread(() => {
            string solution;
            do {
                _mazeHandler.ResetMaze();
            } while (!MazeSolver.TrySolve(_mazeHandler, out solution));
            _mazeHandler.Solution = solution;
        });
        thread.Start();

        int count = 0;
        while (!_mazeHandler.IsReady) {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
            count++;
        }
        _usingExtraThread = false;
        // Thread finish

        _audio.PlaySoundAtTransform("MazeGeneration", transform);
        StartCoroutine(_mazeRenderer.ShowRings());
        LogRings();
        Log($"The initial seed is {_mazeHandler.CurrentSeed}.");
        _initialSeed = _mazeHandler.CurrentSeed;
        Log($"One possible solution is {_mazeHandler.Solution}.");
        while (count < 2) {
            yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.GetRandomSingle(), new Color(1, 0.5f, 1), 0.5f));
            count++;
        }
        _isLoadingMaze = false;
    }

    private void LogRings() {
        var rings = string.Empty;
        for (int row = 0; row < 6; row++) {
            for (int col = 0; col < 6; col++) {
                rings += _mazeHandler.Maze.Cells[col, row].Bit;
            }
        }
        Log($"Rings, in reading order, are: {rings}.");
    }

    private IEnumerator HandleHold() {
        if (_isLoadingMaze || _isSolved) {
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
        Log("You held a button for three seconds.");
        StartCoroutine(FlashWalls());
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
    }

    public void Log(string message) {
        Debug.Log($"[Procedural Maze #{_moduleId}] {message}");
    }

    public void Strike(string message) {
        _isLoadingMaze = true;
        Log($"✕ {message}");
        _module.HandleStrike();
        StartCoroutine(StrikeAnimation());
    }

    private IEnumerator StrikeAnimation() {
        _audio.PlaySoundAtTransform("StrikeInitial", transform);
        StartCoroutine(FlashWalls());
        yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.StrikeX, Color.red, 0.6f));
        _audio.PlaySoundAtTransform("StrikeAnimation", transform);
        yield return StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.Layers, Color.red, 1f));
        StartCoroutine(LoadMaze());
    }

    private IEnumerator FlashWalls() {
        StartCoroutine(_mazeRenderer.ShowWalls());
        yield return new WaitForSeconds(0.6f);
        StartCoroutine(_mazeRenderer.HideWalls());
    }

    private void Solve() {
        Log("◯ Reached the goal and solved the module!");
        _isSolved = true;
        _module.HandlePass();
        PlaySolveAnimation();
    }

    private void PlaySolveAnimation() {
        _audio.PlaySoundAtTransform("Solve", transform);
        StartCoroutine(_mazeRenderer.FlashAnimation(AnimationData.SpiralOut, Color.green, 0.5f));
        StartCoroutine(FlashWallsIndefinitely());
    }

    private IEnumerator FlashWallsIndefinitely() {
        while (true) {
            yield return StartCoroutine(_mazeRenderer.ShowWalls());
            yield return StartCoroutine(_mazeRenderer.HideWalls());
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use '!{0} press UDLR' to press up, down, left, then right; chain with or without spaces | '!{0} <regenerate/hold>' to regenerate the maze.";
#pragma warning restore 414

    private WaitForSeconds _tpPressInterval = new WaitForSeconds(0.05f);

    private IEnumerator ProcessTwitchCommand(string command) {
        command = command.Trim().ToUpperInvariant();

        if (_isLoadingMaze) {
            yield return "sendtochaterror Are you trying to intentionally cause a strike? Wait for the maze to generate before inputting commands!";
        }

        Match match = Regex.Match(command, @"^press\s+([UDLR][UDLR ]*)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (match.Success) {
            int presses = 0;

            yield return null;
            foreach (char direction in match.Groups[1].Value) {
                if (direction != ' ') {
                    yield return $"trycancel The press command was cancelled after {presses} {(presses == 1 ? "press" : "presses")}.";
                    yield return Press(direction);
                    presses += 1;
                }
            }
            yield break;
        }
        match = Regex.Match(command, @"^regenerate|hold$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (match.Success) {
            yield return null;
            _arrows[0].Selectable.OnInteract();
            yield return new WaitForSeconds(3.1f);
            _arrows[0].Selectable.OnInteractEnded();
            yield break;
        }

        yield return "sendtochaterror Invalid command!";
    }

    private IEnumerator Press(char direction) {
        _arrows["ULDR".IndexOf(direction)].Selectable.OnInteract();
        yield return _tpPressInterval;
        _arrows["ULDR".IndexOf(direction)].Selectable.OnInteractEnded();
        yield return _tpPressInterval;
    }

    private IEnumerator TwitchHandleForcedSolve() {
        while (_usingExtraThread || _isLoadingMaze) {
            yield return true;
        }

        // Thread start
        _usingExtraThread = true;
        _mazeHandler.Solution = "breh";

        var thread = new Thread(() => {
            string solution;
            MazeSolver.TrySolve(_mazeHandler, out solution);
            _mazeHandler.Solution = solution;
        });
        thread.Start();

        while (_mazeHandler.Solution == "breh") {
            yield return true;
        }
        _usingExtraThread = false;
        // Thread finish

        if (_mazeHandler.Solution == string.Empty) {
            yield return ProcessTwitchCommand("regenerate");
            while (_isLoadingMaze) {
                yield return true;
            }
        }
        yield return ProcessTwitchCommand($"press {_mazeHandler.Solution}");
    }
}
