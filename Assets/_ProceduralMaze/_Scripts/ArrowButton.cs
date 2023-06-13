using System.Collections;
using System.Collections.Generic;
using KModkit;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class ArrowButton : MonoBehaviour {

    [SerializeField] private MazeDirection _direction;

    public KMSelectable Selectable { get; private set; }
    public MazeDirection Direction { get { return _direction; } }

    public void Awake() {
        Selectable = GetComponent<KMSelectable>();
    }
}
