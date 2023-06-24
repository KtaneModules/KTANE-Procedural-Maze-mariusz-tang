using System.Collections;
using System.Collections.Generic;
using KModkit;
using UnityEngine;

[RequireComponent(typeof(KMSelectable), typeof(Animator))]
public class ArrowButton : MonoBehaviour {

    [SerializeField] private MazeDirection _direction;

    public KMSelectable Selectable { get; private set; }
    public MazeDirection Direction { get { return _direction; } }

    private KMAudio _audio;
    private Animator _animator;

    public void Awake() {
        Selectable = GetComponent<KMSelectable>();
        Selectable.OnInteract += () => {
            _animator.SetBool("IsPressed", true);
            _audio.PlaySoundAtTransform("ButtonPress", transform);
            return true;
        };
        Selectable.OnInteractEnded += () => {
            _animator.SetBool("IsPressed", false);
            _audio.PlaySoundAtTransform("ButtonRelease", transform);
        };

        _animator = GetComponent<Animator>();
        _audio = GetComponentInParent<KMAudio>();
    }
}
