using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D orthographic camera helper.
/// </summary>
public class Camera2D : MonoBehaviour {

    [SerializeField]
    private int pixelsPerMeter = 16; // Eg. setting this to 32 means that an area of 1 meter squared will occupy 32x32 of screen real estate.

    private Camera cam;

    void Start() {
        cam = GetComponent<Camera>();
    }

	void Update () {
        cam.orthographicSize = Screen.height / (pixelsPerMeter*2);
	}
}
