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
    private MeshRenderer[,] _ringRenderers = new MeshRenderer[6, 6];
    private MeshRenderer[,] _horizontalWallRenderers = new MeshRenderer[5, 6];
    private MeshRenderer[,] _verticalWallRenderers = new MeshRenderer[5, 6];

    private void Start() {
        _cellOffColour = _cell.GetComponent<MeshRenderer>().sharedMaterial.color;
        GenerateWalls();
    }

    private void GenerateWalls() {
        var horizRotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
        var vertiRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);

        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 6; j++) {
                Vector3 horizWallPos = new Vector3(_cellXyOrigin + j * _cellXyOffset, _cellXyOrigin + (0.5f + i) * _cellXyOffset, _mazeZPosition);
                GameObject newHorizWall = Instantiate(_wall, _grid.transform);
                newHorizWall.transform.localPosition = horizWallPos;
                newHorizWall.transform.rotation = horizRotation;
                _horizontalWallRenderers[i, j] = newHorizWall.GetComponent<MeshRenderer>();

                Vector3 vertiWallPos = new Vector3(_cellXyOrigin + (0.5f + i) * _cellXyOffset, _cellXyOrigin + j * _cellXyOffset, _mazeZPosition);
                GameObject newVertiWall = Instantiate(_wall, _grid.transform);
                newVertiWall.transform.localPosition = vertiWallPos;
                newVertiWall.transform.rotation = vertiRotation;
                _verticalWallRenderers[i, j] = newVertiWall.GetComponent<MeshRenderer>();
            }
        }
    }

    public void RenderCellsAndGenerateRings(int startX, int startY, int endX, int endY) {
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

                GameObject newRing = Instantiate(_ring, _grid.transform);
                newRing.transform.localPosition = position;
                _ringRenderers[i, j] = newRing.GetComponent<MeshRenderer>();
            }
        }
    }

}
