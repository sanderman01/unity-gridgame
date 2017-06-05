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

            // create chunks
            for (int y = 0; y < world.Size.y / chunkHeight; ++y) {
                for (int x = 0; x < world.Size.x / chunkWidth; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    CreateChunk(world, grid, chunkCoord);
                }
            }
        }

        private void CreateChunk(World world, Grid2D grid, Int2 chunkCoord) {
            int chunkHeight = world.ChunkSize.y;
            int chunkWidth = world.ChunkSize.x;

            FastNoise noise = new FastNoise(world.Seed);

            LayerConfig layers = world.Layers;
            LayerId solidLayerIndex = world.Layers.GetLayer(0).id;
            LayerId tileForegroundLayerIndex = world.Layers.GetLayer(1).id;
            LayerId debugLayer = new LayerId(3);

            ChunkData chunk = grid.CreateChunk(chunkCoord);

            // Fill chunk buffers
            BitBuffer solidBuffer = (BitBuffer)chunk.GetBuffer(solidLayerIndex);
            UShortBuffer foregroundBuffer = (UShortBuffer)chunk.GetBuffer(tileForegroundLayerIndex);
            FloatBuffer debugBuffer = (FloatBuffer)chunk.GetBuffer(debugLayer);
            for (int i = 0; i < solidBuffer.Length; ++i) {
                // Calculate value based on gridCoord
                Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);
                ushort foregroundTileValue = GenerateTile(noise, gridCoord, debugBuffer, i);

                bool solid = foregroundTileValue != 0;
                solidBuffer.SetValue(solid, i);

                foregroundBuffer.SetValue(foregroundTileValue, i);
            }
        }

        private ushort GenerateTile(FastNoise noisegen, Int2 gridCoordinate, FloatBuffer debugBuffer, int bufferIndex) {
            int x = gridCoordinate.x;
            int y = gridCoordinate.y;

            // Terrain where everything below elevation of 128 is solid blocks, and everything above it is air.
            const float groundLevel = 128f;
            float baseTerrain = 2 - y * (1f / groundLevel);

            // Hills modifier
            const float hillsFreq = 0.02f;
            const float hillsAmplitude = 0.5f;
            noisegen.SetFrequency(hillsFreq);
            float hills = hillsAmplitude * noisegen.GetPerlin(x, y);

            // Caves modifier
            const float cavesFreq = 0.05f;
            const float cavesAmp = 8.0f;
            const float caveLevel = groundLevel - 10;
            float caveStrength = Mathf.Clamp01((caveLevel-y) * (1f / caveLevel));
            noisegen.SetFrequency(cavesFreq);
            float caves = caveStrength * cavesAmp * (noisegen.GetPerlin(x, y));

            float final = baseTerrain + hills - caves;
            debugBuffer.SetValue(baseTerrain, bufferIndex);

            if (final >= 1) {
                return 1;
            } else {
                return 0;
            }
        }
    }
}
