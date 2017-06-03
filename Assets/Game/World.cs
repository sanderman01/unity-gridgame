// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;
using AmarokGames.Grids.Data;
using AmarokGames.Grids;

namespace AmarokGames.GridGame {

    public class World : MonoBehaviour {
        private Int2 worldSize;
        private Int2 worldChunkSize;

        public Grid2D WorldGrid { get; private set; }

        public static World CreateWorld(string name, Int2 worldSize, Int2 worldChunkSize, int seed, LayerConfig layers) {
            FastNoise noise = new FastNoise(seed);
            GameObject obj = new GameObject(name);
            World world = obj.AddComponent<World>();
            world.worldSize = worldSize;
            world.worldChunkSize = worldChunkSize;
            world.WorldGrid = world.CreateWorldGrid(noise, layers);
            return world;
        }

        private Grid2D CreateWorldGrid(FastNoise noise, LayerConfig layers) {

            int chunkWidth = worldChunkSize.x;
            int chunkHeight = worldChunkSize.y;

            GameObject obj = new GameObject("worldgrid");
            obj.transform.SetParent(this.transform, false);
            Grid2D grid = obj.AddComponent<Grid2D>();
            grid.Setup(0, chunkWidth, chunkHeight, layers);

            CreateChunks(noise, chunkWidth, chunkHeight, layers.GetLayer(0).id, layers.GetLayer(1).id, grid);

            return grid;
        }

        private void CreateChunks(FastNoise noise, int chunkWidth, int chunkHeight, LayerId solidLayerIndex, LayerId tileForegroundLayerIndex, Grid2D grid) {
            // create chunks
            for (int y = 0; y < worldSize.y / chunkHeight; ++y) {
                for (int x = 0; x < worldSize.x / chunkWidth; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    ChunkData chunk = grid.CreateChunk(chunkCoord);

                    // fill chunk with some random data

                    BitBuffer solidBuffer = (BitBuffer)chunk.GetBuffer(solidLayerIndex);
                    UShortBuffer foregroundBuffer = (UShortBuffer)chunk.GetBuffer(tileForegroundLayerIndex);
                    for (int i = 0; i < solidBuffer.Length; ++i) {
                        //Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);

                        // Calculate value based on gridCoord
                        Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);
                        ushort foregroundTileValue = GenerateTile(noise, gridCoord);

                        bool solid = foregroundTileValue != 0;
                        solidBuffer.SetValue(solid, i);

                        foregroundBuffer.SetValue(foregroundTileValue, i);
                    }
                }
            }
        }

        void LateUpdate() {
            WorldGrid.ClearRecent();
        }

        private ushort GenerateTile(FastNoise noisegen, Int2 gridCoordinate) {

            int x = gridCoordinate.x;
            int y = gridCoordinate.y;

            const float hillsFreq = 0.02f;
            const float hillsAmplitude = 0.5f;
            const float groundLevel = 128f;
            noisegen.SetFrequency(hillsFreq);
            float baseTerrain = 1 - y * (1f / groundLevel) + hillsAmplitude * noisegen.GetPerlin(x, y);

            //const float cavesFreq = 0.05f;
            //noisegen.SetFrequency(cavesFreq);
            //float caves = 1 - y * (1f / groundLevel) * 0.2f * (noisegen.GetPerlin(x, y));

            float final = Mathf.Round(baseTerrain);

            if(final >= 1) {
                return 1;
            }
            else {
                return 0;
            }
        }
    }
}