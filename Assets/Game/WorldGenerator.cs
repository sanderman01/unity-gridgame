// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class WorldGenerator {

        public WorldGenerator() {
        }

        public void Init(World world) {
            GenerateTerrainGrid(world);
        }

        private Grid2D GenerateTerrainGrid(World world) {
            Grid2D grid = world.CreateGrid("terrain");
            CreateChunks(world, grid);
            return grid;
        }

        private void CreateChunks(World world, Grid2D grid) {

            int chunkHeight = world.ChunkSize.y;
            int chunkWidth = world.ChunkSize.x;
            FastNoise noise = new FastNoise(world.Seed);
            LayerConfig layers = world.Layers;
            LayerId solidLayerIndex = world.Layers.GetLayer(0).id;
            LayerId tileForegroundLayerIndex = world.Layers.GetLayer(1).id;

            // create chunks
            for (int y = 0; y < world.Size.y / chunkHeight; ++y) {
                for (int x = 0; x < world.Size.x / chunkWidth; ++x) {
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

            if (final >= 1) {
                return 1;
            } else {
                return 0;
            }
        }
    }
}
