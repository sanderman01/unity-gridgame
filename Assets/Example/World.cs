// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;
using AmarokGames.Grids.Data;

namespace AmarokGames.Grids.Examples {

    public class World : MonoBehaviour {
        private Int2 worldSize;
        private Int2 worldChunkSize;

        public Grid2D WorldGrid { get; private set; }
        public Grid2DBehaviour WorldGridBehaviour { get; private set; }

        public static World CreateWorld(string name, Int2 worldSize, Int2 worldChunkSize) {
            GameObject obj = new GameObject(name);
            World world = obj.AddComponent<World>();
            world.worldSize = worldSize;
            world.worldChunkSize = worldChunkSize;
            world.WorldGrid = world.CreateWorldGrid();
            return world;
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

            GameObject obj = new GameObject("worldgrid");
            obj.transform.SetParent(this.transform, false);
            WorldGridBehaviour = obj.AddComponent<Grid2DBehaviour>();
            WorldGridBehaviour.Setup(grid);

            CreateChunks(chunkWidth, chunkHeight, solidLayerIndex, tileForegroundLayerIndex, grid);

            return grid;
        }

        private void CreateChunks(int chunkWidth, int chunkHeight, int solidLayerIndex, int tileForegroundLayerIndex, Grid2D grid) {
            // create chunks
            for (int y = 0; y < worldSize.y / chunkHeight; ++y) {
                for (int x = 0; x < worldSize.x / chunkWidth; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    ChunkData chunk = grid.CreateChunk(chunkCoord);

                    // fill chunk with some random data

                    BooleanBuffer solidBuffer = (BooleanBuffer)chunk.GetBuffer(solidLayerIndex);
                    UShortBuffer foregroundBuffer = (UShortBuffer)chunk.GetBuffer(tileForegroundLayerIndex);
                    for (int i = 0; i < solidBuffer.Length; ++i) {
                        //Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);

                        // Calculate value based on gridCoord
                        // for now simply use a random value
                        bool solidValue = Random.value < 0.5f;
                        solidBuffer.SetValue(solidValue, i);
                        ushort foreground = solidValue ? (ushort)Random.Range(1,3) : (ushort)0;
                        foregroundBuffer.SetValue(foreground, i);
                    }
                }
            }
        }

        void LateUpdate() {
            WorldGrid.ClearRecent();
        }
    }
}