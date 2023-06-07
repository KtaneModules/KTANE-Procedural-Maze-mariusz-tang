using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMazeModule : MonoBehaviour {

    private MazeRenderer _mazeRenderer;

    private void Start() {
        _mazeRenderer = GetComponentInChildren<MazeRenderer>();
        _mazeRenderer.RenderCellsAndGenerateRings(1, 1, 5, 3);
    }

}
