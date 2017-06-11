// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.Grids {

    public class Grid2DChunk : MonoBehaviour {

        [SerializeField]
        private Int2 chunkCoord;
        public Int2 ChunkCoord { get { return chunkCoord; } }

        private Grid2D grid;
        public Grid2D ParentGrid { get { return grid; } }

        private ChunkData chunkData;
        public ChunkData Data { get { return chunkData; } }

        public Rect Bounds2D {
            get {
                if (transform.hasChanged) {
                    cachedBounds = CalculateChunkAABB();
                    transform.hasChanged = false;
                }
                return cachedBounds;
            }
        }

        private int chunkWidth;
        private int chunkHeight;
        private Rect cachedBounds;

        public void Setup(Int2 chunkCoord, Grid2D parentGrid, ChunkData chunkData) {
            this.chunkCoord = chunkCoord;
            this.grid = parentGrid;
            this.chunkData = chunkData;
            this.chunkWidth = parentGrid.ChunkWidth;
            this.chunkHeight = parentGrid.ChunkHeight;
        }

        public Rect CalculateChunkAABB() {
            UnityEngine.Profiling.Profiler.BeginSample("CalculateChunkAABB");
            Vector3 pos = transform.position;
            Matrix4x4 m = transform.localToWorldMatrix;
            Vector2 p1 = m * new Vector2(0, 0);
            Vector2 p2 = m * new Vector2(0, chunkHeight);
            Vector2 p3 = m * new Vector2(chunkWidth, chunkHeight);
            Vector2 p4 = m * new Vector2(chunkWidth, 0);

            float[] x = { p1.x, p2.x, p3.x, p4.x };
            float[] y = { p1.y, p2.y, p3.y, p4.y };

            float minX = Mathf.Min(x);
            float minY = Mathf.Min(y);
            float maxX = Mathf.Max(x);
            float maxY = Mathf.Max(y);

            Rect bounds = new Rect(pos.x + minX, pos.y + minY, maxX - minX, maxY - minY);
            UnityEngine.Profiling.Profiler.EndSample();
            return bounds;
        }
    }
}