using UnityEngine;

public class Triangle : MonoBehaviour {
    void Update() {
        transform.Rotate(Vector3.forward * Time.deltaTime * 36);
    }
}
