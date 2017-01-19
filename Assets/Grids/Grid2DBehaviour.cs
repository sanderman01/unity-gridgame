// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.Grids {

    public class Grid2DBehaviour : MonoBehaviour {

        private GridChunkBehaviour chunkPrefab;
        private const bool drawChunkBoundsGizmo = true;

        public Grid2D ParentGrid { get; private set; }

        public void Setup(Grid2D parentGrid, GridChunkBehaviour chunkPrefab) {
            this.ParentGrid = parentGrid;
            this.chunkPrefab = chunkPrefab;
        }

        void Update() {
            Grid2D grid = ParentGrid;
            foreach (Int2 chunkCoord in grid.RecentlyCreated) {
                CreateChunkBehaviour(chunkCoord);
            }
        }

        public void CreateChunkBehaviour(Int2 chunkCoord) {
            // Create and position chunk GameObject
            GridChunkBehaviour chunkBehaviour = Instantiate<GridChunkBehaviour>(chunkPrefab, this.transform, false);
            Vector2 pos = new Vector2(chunkCoord.x * ParentGrid.ChunkWidth, chunkCoord.y * ParentGrid.ChunkHeight);
            chunkBehaviour.transform.localPosition = pos;

            string name = string.Format("chunk {0}", chunkCoord);
            chunkBehaviour.gameObject.name = name;

            chunkBehaviour.Setup(chunkCoord, ParentGrid);
        }

        // Get all chunks that are currently visible in the camera. This assumes an orthographic camera.
        public IEnumerable<Int2> GetChunksWithinCameraBounds(Camera cam) {

            // take the corners of the viewport and convert them to world coordinates
            Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 bottomRight = cam.ViewportToWorldPoint(new Vector2(1, 0));
            Vector2 topLeft = cam.ViewportToWorldPoint(new Vector2(0, 1));
            Vector2 topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));

            // convert those world coordinates to grid local coordinates
            Vector2 localBottomLeft = this.transform.InverseTransformPoint(bottomLeft);
            Vector2 localBottomRight = this.transform.InverseTransformPoint(bottomRight);
            Vector2 localTopLeft = this.transform.InverseTransformPoint(topLeft);
            Vector2 localTopRight = this.transform.InverseTransformPoint(topRight);

            // get the minimums and maximums of those
            float xMin = Mathf.Min(localBottomLeft.x, localBottomRight.x, localTopLeft.x, localTopRight.x);
            float xMax = Mathf.Max(localBottomLeft.x, localBottomRight.x, localTopLeft.x, localTopRight.x);

            float yMin = Mathf.Min(localBottomLeft.y, localBottomRight.y, localTopLeft.y, localTopRight.y);
            float yMax = Mathf.Max(localBottomLeft.y, localBottomRight.y, localTopLeft.y, localTopRight.y);

            // determine required chunks
            Int2 minGridCoord = new Int2(xMin, yMin);
            Int2 maxGridCoord = new Int2(xMax, yMax);
            return Grid2D.GetChunks(minGridCoord, maxGridCoord, ParentGrid.ChunkWidth, ParentGrid.ChunkHeight);
        }

        void OnDrawGizmos() {
            if (drawChunkBoundsGizmo) {
                DrawChunkBoundsGizmo();
            }
        }

        void DrawChunkBoundsGizmo() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Grid2D grid = ParentGrid;
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;
            IEnumerable<Int2> loadedChunks = grid.GetLoadedChunks();

            Vector2 size = new Vector2(chunkWidth, chunkHeight);
            foreach (Int2 chunkCoord in loadedChunks) {
                Vector2 center = new Vector2(chunkCoord.x * chunkWidth + chunkWidth / 2, chunkCoord.y * chunkHeight + chunkHeight / 2);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}