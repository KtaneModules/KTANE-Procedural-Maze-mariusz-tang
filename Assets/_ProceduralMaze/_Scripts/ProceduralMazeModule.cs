using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private KMSelectable[] _buttons;
    private MazeRenderer _mazeRenderer;

    private BitMaze6x6 _maze;

    private void Awake() {
        _maze = MazeGenerator.GenerateNewMaze();
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
        _mazeRenderer.AssignMaze(_maze);
        _mazeRenderer.RenderRings();

        _buttons = GetComponent<KMSelectable>().Children;
    }

    private void Start() {
        Array.ForEach(_buttons, b => b.OnInteract += delegate () { MazeGenerator.AssignRandomBitmap(_maze); _mazeRenderer.RenderRings(); return false; });
    }

}
