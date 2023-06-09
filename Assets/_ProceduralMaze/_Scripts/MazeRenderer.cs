using System;
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

    // Stored as [column, row].
    private MeshRenderer[,] _cellRenderers = new MeshRenderer[6, 6];
    private MeshRenderer[,] _ringRenderers = new MeshRenderer[6, 6];
    // These are the walls in each row and column. Stored as [column/row, wall].
    private MeshRenderer[,] _columnWallRenderers = new MeshRenderer[6, 7];
    private MeshRenderer[,] _rowWallRenderers = new MeshRenderer[6, 7];

    private BitMaze6x6 _maze;

    private void Start() {
        _cellOffColour = _cell.GetComponent<MeshRenderer>().sharedMaterial.color;
        GenerateWalls();
    }

    private void GenerateWalls() {
        var colRotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
        var rowRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);

        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                var colWallPos = new Vector3(_cellXyOrigin + line * _cellXyOffset, _cellXyOrigin + (-0.5f + wall) * _cellXyOffset, _mazeZPosition);
                GameObject newColWall = Instantiate(_wall, _grid.transform);
                newColWall.transform.localPosition = colWallPos;
                newColWall.transform.rotation = colRotation;
                _columnWallRenderers[line, wall] = newColWall.GetComponent<MeshRenderer>();

                var rowWallPos = new Vector3(_cellXyOrigin + (-0.5f + wall) * _cellXyOffset, _cellXyOrigin + line * _cellXyOffset, _mazeZPosition);
                GameObject newRowWall = Instantiate(_wall, _grid.transform);
                newRowWall.transform.localPosition = rowWallPos;
                newRowWall.transform.rotation = rowRotation;
                _rowWallRenderers[line, wall] = newRowWall.GetComponent<MeshRenderer>();
            }
        }
    }

    public void AssignMaze(BitMaze6x6 maze) {
        _maze = maze;
        RenderCellsAndGenerateRings(maze.StartPosition, maze.GoalPosition);
    }

    private void RenderCellsAndGenerateRings(Vector2Int start, Vector2Int goal) {
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 6; j++) {
                Vector3 position = new Vector3(_cellXyOrigin + i * _cellXyOffset, _cellXyOrigin + j * _cellXyOffset, _mazeZPosition);

                GameObject mazeObjectPrefab = (i == goal.x && j == goal.y) ? _triangle : _cell;
                GameObject mazeObject = Instantiate(mazeObjectPrefab, _grid.transform);
                mazeObject.transform.localPosition = position;
                _cellRenderers[i, j] = mazeObject.GetComponent<MeshRenderer>();
                if (i == start.x && j == start.y) {
                    _cellRenderers[i, j].material.color = Color.white;
                }

                GameObject newRing = Instantiate(_ring, _grid.transform);
                newRing.transform.localPosition = position;
                _ringRenderers[i, j] = newRing.GetComponent<MeshRenderer>();
            }
        }
    }

    public void RenderRings() {
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                _ringRenderers[col, row].enabled = _maze.Bitmap[col, row] == 1;
            }
        }
    }

    public void RenderMovement(Vector2Int from, Vector2Int to) {
        _cellRenderers[from.x, from.y].material.color = _cellOffColour;
        _cellRenderers[to.x, to.y].material.color = Color.white;
    }

    public void RenderWalls() {
        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                BitMaze6x6.Wall colWall = _maze.ColumnWalls[line, wall];
                if (colWall.IsDecided && colWall.IsPresent) {
                    _columnWallRenderers[line, wall].enabled = true;
                }
                else {
                    _columnWallRenderers[line, wall].enabled = false;
                }

                BitMaze6x6.Wall rowWall = _maze.RowWalls[line, wall];
                if (rowWall.IsDecided && rowWall.IsPresent) {
                    _rowWallRenderers[line, wall].enabled = true;
                }
                else {
                    _rowWallRenderers[line, wall].enabled = false;
                }
            }
        }
    }
}
