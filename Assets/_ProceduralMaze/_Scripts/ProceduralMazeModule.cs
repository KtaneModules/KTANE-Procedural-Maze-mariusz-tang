using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private KMSelectable[] _buttons;
    private MazeHandler _mazeHandler;

    private void Awake() {
        _mazeHandler = new MazeHandler(this);

        _buttons = GetComponent<KMSelectable>().Children;
    }

    private void Start() {
        foreach (KMSelectable button in _buttons) {
            button.OnInteract += delegate () { _mazeHandler.Move((MazeDirection)Enum.Parse(typeof(MazeDirection), button.name)); return false; };
        }
    }

}
