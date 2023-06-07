using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour {

    private const float _cellXyOrigin = 0.005f;
    private const float _cellXyOffset = -0.002f;
    private const float _mazeZPosition = 0.00067f;

    [SerializeField] private GameObject _cell;
    [SerializeField] private GameObject _triangle;
    [SerializeField] private GameObject _wall;
    [SerializeField] private GameObject _ring;
    [SerializeField] private GameObject _grid;

    private Color _cellOffColour;
    private MeshRenderer[,] _cellRenderers = new MeshRenderer[6, 6];

    private void Start() {
        _cellOffColour = _cell.GetComponent<MeshRenderer>().sharedMaterial.color;
    }

    public void RenderCells(int startX, int startY, int endX, int endY) {
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 6; j++) {
                Vector3 position = new Vector3(_cellXyOrigin + i * _cellXyOffset, _cellXyOrigin + j * _cellXyOffset, _mazeZPosition);
                GameObject mazeObjectPrefab = (i == endX && j == endY) ? _triangle : _cell;
                GameObject mazeObject = Instantiate(mazeObjectPrefab, _grid.transform);
                mazeObject.transform.localPosition = position;
                _cellRenderers[i, j] = mazeObject.GetComponent<MeshRenderer>();
                if (i == startX && j == startY) {
                    _cellRenderers[i, j].material.color = Color.white;
                }
            }
        }
    }

}
