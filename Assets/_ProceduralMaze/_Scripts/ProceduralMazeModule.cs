using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private KMSelectable[] _arrows;
    private MazeHandler _mazeHandler;

    private void Awake() {
        _mazeHandler = new MazeHandler(this);
        _arrows = GetComponent<KMSelectable>().Children;
    }

    private void Start() {
        foreach (KMSelectable arrow in _arrows) {
            arrow.OnInteract += delegate () { _mazeHandler.Move((MazeDirection)Enum.Parse(typeof(MazeDirection), arrow.name)); return false; };
        }
    }
}
