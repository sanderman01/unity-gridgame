// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames {
    static class CameraExtensions {

        /// <summary>
        /// Calculates the bounds of this orthographic Camera.
        /// </summary>
        public static Rect OrthoBounds2D(this Camera cam) {
            float orthoSize = cam.orthographicSize;
            float ratio = (float)Screen.width / (float)Screen.height;
            Vector2 size = new Vector3(2f * orthoSize * ratio, cam.orthographicSize * 2f);
            Vector2 center = cam.transform.position;
            Rect bounds = new Rect(center - 0.5f * size, size);
            return bounds;
        }

    }
}