// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames {
    static class CameraExtensions {

        /// <summary>
        /// Calculates the bounds of this orthographic Camera.
        /// </summary>
        public static Bounds CalcOrthographicCameraBounds(this Camera cam) {
            float orthoSize = cam.orthographicSize;
            float ratio = (float)Screen.width / (float)Screen.height;
            Vector3 size = new Vector3(2f * orthoSize * ratio, cam.orthographicSize * 2f, cam.farClipPlane);
            Vector3 center = cam.transform.position;
            Bounds bounds = new Bounds(center, size);
            return bounds;
        }

    }
}