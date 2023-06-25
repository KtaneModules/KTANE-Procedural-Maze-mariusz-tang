using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour {

    private const float _cellXyOrigin = 0.005f;
    private const float _cellXyOffset = -0.002f;
    private const float _mazeZPosition = 0.00067f;
    private const float _transitionTime = 0.2f;

    [SerializeField] private GameObject _cell;
    [SerializeField] private GameObject _triangle;
    [SerializeField] private GameObject _wall;
    [SerializeField] private GameObject _ring;
    [SerializeField] private GameObject _grid;

    [SerializeField] private Material _cellOffMat;
    [SerializeField] private Material _cellOnMat;

    // Stored as [column, row].
    private IMazeCell[,] _cells = new IMazeCell[6, 6];
    private MeshRenderer[,] _ringRenderers = new MeshRenderer[6, 6];
    // These are the walls in each row and column. Stored as [column/row, wall].
    private MeshRenderer[,] _columnWallRenderers = new MeshRenderer[6, 7];
    private MeshRenderer[,] _rowWallRenderers = new MeshRenderer[6, 7];

    private BitMaze6x6 _maze;
    private Vector2Int _currentRenderedPosition;

    private void Awake() {
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
        RenderCellsAndGenerateRings(maze.StartCell.Position, maze.GoalCell.Position);
        _currentRenderedPosition = maze.StartCell.Position;
    }

    private void RenderCellsAndGenerateRings(Vector2Int start, Vector2Int goal) {
        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 6; j++) {
                Vector3 position = new Vector3(_cellXyOrigin + i * _cellXyOffset, _cellXyOrigin + j * _cellXyOffset, _mazeZPosition);

                if (i == goal.x && j == goal.y) {
                    _cells[i, j] = new GoalCell(this, _triangle, position, _grid.transform);
                }
                else {
                    _cells[i, j] = new NormalCell(this, _cell, position, _grid.transform, _cellOffMat, _cellOnMat);
                    if (i == start.x && j == start.y) {
                        _cells[i, j].SetLightState(Color.white);
                    }
                }

                GameObject newRing = Instantiate(_ring, _grid.transform);
                newRing.transform.localPosition = position;
                _ringRenderers[i, j] = newRing.GetComponent<MeshRenderer>();
            }
        }
    }

    public IEnumerator ShowRings() {
        var interval = new WaitForSeconds(0.005f);
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                _ringRenderers[col, row].enabled = _maze.Cells[col, row].Bit == 1;
                yield return interval;
            }
        }
    }

    public IEnumerator HideRings() {
        var interval = new WaitForSeconds(0.005f);
        for (int col = 0; col < 6; col++) {
            for (int row = 0; row < 6; row++) {
                _ringRenderers[col, row].enabled = false;
                yield return interval;
            }
        }
    }

    public void RenderMovementTo(Vector2Int position) {
        _cells[position.x, position.y].SetOccupiedColour();
        _cells[_currentRenderedPosition.x, _currentRenderedPosition.y].SetDefaultColour();
        _currentRenderedPosition = position;
    }

    public IEnumerator ShowWalls() {
        var interval = new WaitForSeconds(0.005f);
        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                BitMaze6x6.Wall colWall = _maze.ColumnWalls[line, wall];
                _columnWallRenderers[line, wall].enabled = colWall.IsDecided && colWall.IsPresent;

                BitMaze6x6.Wall rowWall = _maze.RowWalls[line, wall];
                _rowWallRenderers[line, wall].enabled = rowWall.IsDecided && rowWall.IsPresent;
                yield return interval;
            }
        }
    }

    public IEnumerator HideWalls() {
        var interval = new WaitForSeconds(0.005f);
        for (int line = 0; line < 6; line++) {
            for (int wall = 0; wall < 7; wall++) {
                _columnWallRenderers[line, wall].enabled = false;
                _rowWallRenderers[line, wall].enabled = false;
                yield return interval;
            }
        }
    }

    public IEnumerator FlashAnimation(int[] lightPositions, Color colour, float time, int count = 1) {
        var waitInterval = new WaitForSeconds(time / lightPositions.Length);
        for (int i = 0; i < count; i++) {
            foreach (int light in lightPositions) {
                StartCoroutine(FlashLight(_cells[light % 6, light / 6], colour));
                yield return waitInterval;
            }
            yield return new WaitForSeconds(2 * _transitionTime - time);
        }
    }

    public IEnumerator FlashAnimation(int[][] lightPositionGroups, Color colour, float time, int count = 1) {
        var waitInterval = new WaitForSeconds(time / lightPositionGroups.Length);
        for (int i = 0; i < count; i++) {
            foreach (int[] lights in lightPositionGroups) {
                foreach (int light in lights) {
                    StartCoroutine(FlashLight(_cells[light % 6, light / 6], colour));
                }
                yield return waitInterval;
            }
            yield return new WaitForSeconds(2 * _transitionTime - time);
        }
    }

    private IEnumerator FlashLight(IMazeCell cell, Color colour) {
        cell.SetLightState(colour);
        yield return new WaitForSeconds(_transitionTime);
        if (cell == _cells[_currentRenderedPosition.x, _currentRenderedPosition.y]) {
            cell.SetOccupiedColour();
        }
        else {
            cell.SetDefaultColour();
        }
    }

    public void ShowHoldProgress(int level) {
        for (int row = 0; row < 6; row++) {
            for (int col = 0; col < 6; col++) {
                if (row + col < level) {
                    _cells[col, row].SetLightState(Color.white * 0.75f);
                }
                else {
                    _cells[col, row].SetDefaultColour();
                }
            }
        }
        _cells[_currentRenderedPosition.x, _currentRenderedPosition.y].SetLightState(Color.white);
    }

    private interface IMazeCell {
        void SetLightState(Color colour);
        void SetDefaultColour();
        void SetOccupiedColour();
    }

    private class NormalCell : IMazeCell {

        private MazeRenderer _parentRenderer;
        private Coroutine _transition;

        private Material _offMat;
        private Material _onMat;

        private MeshRenderer _renderer;

        public NormalCell(MazeRenderer parentRenderer, GameObject prefab, Vector3 position, Transform parent, Material offMat, Material onMat) {
            GameObject cell = Instantiate(prefab, parent);
            cell.transform.localPosition = position;
            _renderer = cell.GetComponent<MeshRenderer>();
            _offMat = new Material(offMat);
            _onMat = new Material(onMat);
            _parentRenderer = parentRenderer;
        }

        public void SetDefaultColour() {
            if (_transition != null) {
                _parentRenderer.StopCoroutine(_transition);
            }
            _transition = _parentRenderer.StartCoroutine(TransitionToOff());
        }

        public void SetOccupiedColour() {
            SetLightState(Color.white);
        }

        public void SetLightState(Color colour) {
            if (_transition != null) {
                _parentRenderer.StopCoroutine(_transition);
            }
            _transition = _parentRenderer.StartCoroutine(TransitionToColour(colour));
        }

        private IEnumerator TransitionToColour(Color newColour) {
            Color oldColour = _renderer.material.color;
            float elapsedTime = 0;

            _renderer.material = _onMat;
            while (elapsedTime < _transitionTime) {
                elapsedTime += Time.deltaTime;
                _renderer.material.color = Color.Lerp(oldColour, newColour, elapsedTime / _transitionTime);
                yield return null;
            }
            _onMat.color = newColour;
        }

        private IEnumerator TransitionToOff() {
            Color oldMatColour = _renderer.material.color;
            float elapsedTime = 0;

            yield return null;
            _renderer.material = _offMat;

            while (elapsedTime < _transitionTime) {
                elapsedTime += Time.deltaTime;
                _renderer.material.color = Color.Lerp(oldMatColour, _offMat.color, elapsedTime / _transitionTime);
                yield return null;
            }
            _renderer.material = _offMat;
        }
    }

    private class GoalCell : IMazeCell {

        private MazeRenderer _parentRenderer;
        private Coroutine _transition;

        private static readonly Color _goalBlue = new Color(0, 0.79f, 1);

        private MeshRenderer _renderer;

        public GoalCell(MazeRenderer parentRenderer, GameObject prefab, Vector3 position, Transform parent) {
            GameObject cell = Instantiate(prefab, parent);
            cell.transform.localPosition = position;
            _renderer = cell.GetComponent<MeshRenderer>();
            _parentRenderer = parentRenderer;
        }

        public void SetDefaultColour() {
            SetLightState(_goalBlue);
        }

        public void SetOccupiedColour() {
            SetLightState(Color.green);
        }

        // Setting the light state to false on the goal cell actually makes it blue (unoccupied).
        public void SetLightState(Color colour) {
            if (_transition != null) {
                _parentRenderer.StopCoroutine(_transition);
            }
            _transition = _parentRenderer.StartCoroutine(Transition(colour));
        }

        private IEnumerator Transition(Color newColour) {
            Color oldColour = _renderer.material.color;
            float elapsedTime = 0;

            while (elapsedTime < _transitionTime) {
                elapsedTime += Time.deltaTime;
                _renderer.material.color = Color.Lerp(oldColour, newColour, elapsedTime / _transitionTime);
                yield return null;
            }
            _renderer.material.color = newColour;
        }
    }
}
