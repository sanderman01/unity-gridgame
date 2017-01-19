// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;
using AmarokGames.Grids.Data;

namespace AmarokGames.Grids.Examples {

    public class World : MonoBehaviour {
        public Int2 worldSize = new Int2(1024, 1024);
        public Int2 worldChunkSize = new Int2(64, 64);

        public Grid2DBehaviour gridPrefab;
        public GridChunkBehaviour chunkPrefab;

        private Grid2D worldGrid;

        // Use this for initialization
        void Start() {
            worldGrid = CreateWorldGrid();
        }

        private Grid2D CreateWorldGrid() {

            int chunkWidth = worldChunkSize.x;
            int chunkHeight = worldChunkSize.y;

            int solidLayerIndex;
            int tileForegroundLayerIndex;
            int tileBackgroundLayerIndex;

            LayerConfig layers = new LayerConfig()
                .AddLayer("solid", BufferType.Boolean, out solidLayerIndex)
                .AddLayer("tileforeground", BufferType.UShort, out tileForegroundLayerIndex)
                .AddLayer("tilebackground", BufferType.UShort, out tileBackgroundLayerIndex);

            Grid2D grid = new Grid2D(chunkWidth, chunkHeight, layers);

            Grid2DBehaviour worldGridBehaviour = Instantiate<Grid2DBehaviour>(gridPrefab);
            worldGridBehaviour.Setup(grid, chunkPrefab);

            // create chunks
            for (int y = 0; y < worldSize.y / chunkHeight; ++y) {
                for (int x = 0; x < worldSize.x / chunkWidth; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    Chunk chunk = grid.CreateChunk(chunkCoord);

                    // fill chunk with some random data

                    BooleanBuffer solidBuffer = (BooleanBuffer)chunk.GetBuffer(solidLayerIndex);
                    UShortBuffer foregroundBuffer = (UShortBuffer)chunk.GetBuffer(tileForegroundLayerIndex);
                    for (int i = 0; i < solidBuffer.Length; ++i) {
                        //Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);

                        // Calculate value based on gridCoord
                        // Blahblah
                        bool solidValue = Random.value < 0.5f;
                        solidBuffer.SetValue(solidValue, i);
                        ushort foreground = solidValue ? (ushort)1 : (ushort)0;
                        foregroundBuffer.SetValue(foreground, i);
                    }
                }
            }

            return grid;
        }

        void LateUpdate() {
            worldGrid.ClearRecent();
        }
    }
}