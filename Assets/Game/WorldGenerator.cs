// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class WorldGenerator {

        private LayerId solidLayer;
        private LayerId tileForegroundLayer;
        private LayerId tileBackgroundLayer;
        private LayerId debugLayer;

        private uint tileEmpty;
        private uint tileStone;
        private uint tileDirt;
        private uint tileGrass;

        public WorldGenerator(LayerId solidLayer, LayerId tileForegroundLayer, LayerId tileBackgroundLayer, LayerId debugLayer,
            uint tileEmpty, uint tileStone, uint tileDirt, uint tileGrass) {
            this.solidLayer = solidLayer;
            this.tileForegroundLayer = tileForegroundLayer;
            this.tileBackgroundLayer = tileBackgroundLayer;
            this.debugLayer = debugLayer;

            this.tileEmpty = tileEmpty;
            this.tileStone = tileStone;
            this.tileDirt = tileDirt;
            this.tileGrass = tileGrass;
        }

        public void Init(World world) {
            GenerateTerrainGrid(world);
        }

        private Grid2D GenerateTerrainGrid(World world) {
            Grid2D grid = world.CreateGrid("terrain", Grid2D.GridType.Static);
            CreateTerrainChunks(world, grid);

            Grid2D dynamicGrid = world.CreateGrid("dynamic grid", Grid2D.GridType.Dynamic);
            dynamicGrid.transform.position = new Vector3(150, 200);
            CreateTerrainChunk(world, dynamicGrid, new Int2(0, 0));


            return grid;
        }

        private void CreateTerrainChunks(World world, Grid2D grid) {
            int chunkHeight = world.ChunkSize.y;
            int chunkWidth = world.ChunkSize.x;

            // create chunks
            for (int y = 0; y < world.Size.y / chunkHeight; ++y) {
                for (int x = 0; x < world.Size.x / chunkWidth; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    CreateTerrainChunk(world, grid, chunkCoord);
                }
            }
        }

        private void CreateTerrainChunk(World world, Grid2D grid, Int2 chunkCoord) {
            int chunkHeight = world.ChunkSize.y;
            int chunkWidth = world.ChunkSize.x;

            FastNoise noise = new FastNoise(world.Seed);

            LayerConfig layers = world.Layers;

            ChunkData chunk = grid.CreateChunk(chunkCoord);

            // Fill chunk buffers
            BitBuffer solidBuffer = (BitBuffer)chunk.GetBuffer(solidLayer);
            BufferUnsignedInt32 foregroundBuffer = (BufferUnsignedInt32)chunk.GetBuffer(tileForegroundLayer);
            BufferUnsignedInt32 backgroundBuffer = (BufferUnsignedInt32)chunk.GetBuffer(tileBackgroundLayer);
            FloatBuffer debugBuffer = (FloatBuffer)chunk.GetBuffer(debugLayer);

            // Generate base terrain
            for (int i = 0; i < solidBuffer.Length; ++i) {
                // Calculate value based on gridCoord
                Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);
                uint foregroundTileValue;
                uint backgroundTileValue;
                GenerateTile(noise, gridCoord, debugBuffer, i, out foregroundTileValue, out backgroundTileValue);

                bool solid = foregroundTileValue != 0;
                solidBuffer.SetValue(solid, i);

                foregroundBuffer.SetValue(i, (uint)new TileStateId(foregroundTileValue, 0));
                backgroundBuffer.SetValue(i, (uint)new TileStateId(backgroundTileValue, 0));
            }

            Random.InitState(world.Seed);

            GenerateGrass(chunkCoord, chunkHeight, chunkWidth, solidBuffer, foregroundBuffer, backgroundBuffer);

        }

        /// <summary>
        /// Iterates from top to bottom in every column until it finds the first solid tile, then it starts replacing tiles with grass.
        /// </summary>
        private void GenerateGrass(Int2 chunkCoord, int chunkHeight, int chunkWidth, BitBuffer solidBuffer, BufferUnsignedInt32 foregroundBuffer, BufferUnsignedInt32 backgroundBuffer) {
            // Generate grass and dirt
            if (chunkCoord.y == 1 || chunkCoord.y == 2) {
                // Iterate over columns in the chunk
                for (int x = 0; x < chunkWidth; x++) {
                    // Iterate from top to bottom
                    int grassTilesBudget = 0;
                    int dirtTilesBudget = 0;
                    for (int y = chunkHeight - 1; y >= 0; y--) {
                        Int2 localCoord = new Int2(x, y);
                        int bufferIndex = Grid2D.GetCellIndex(localCoord, chunkWidth, chunkHeight);
                        bool solidTile = solidBuffer.GetValue(bufferIndex);

                        if (y == chunkHeight - 1 && !solidTile) {
                            grassTilesBudget = Random.Range(1, 3);
                            dirtTilesBudget = Random.Range(10, 20);
                        }

                        if (solidTile && grassTilesBudget > 0) {
                            foregroundBuffer.SetValue(bufferIndex, (uint)new TileStateId(tileGrass, 0));
                            backgroundBuffer.SetValue(bufferIndex, (uint)new TileStateId(tileDirt, 0));
                            grassTilesBudget--;
                        } else if (solidTile && dirtTilesBudget > 0) {
                            foregroundBuffer.SetValue(bufferIndex, (uint)new TileStateId(tileDirt, 0));
                            backgroundBuffer.SetValue(bufferIndex, (uint)new TileStateId(tileDirt, 0));
                            dirtTilesBudget--;
                        }

                    }
                }
            }
        }

        private void GenerateTile(
            FastNoise noisegen,
            Int2 gridCoordinate,
            FloatBuffer debugBuffer,
            int bufferIndex,
            out uint foregroundTile,
            out uint backgroundTile
            ) 
        {

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
            float caveStrength = Mathf.Clamp01((caveLevel - y) * (1f / caveLevel));
            noisegen.SetFrequency(cavesFreq);
            float caves = caveStrength * cavesAmp * (noisegen.GetPerlin(x, y));

            float final = baseTerrain + hills - caves; // caves;
            debugBuffer.SetValue(baseTerrain, bufferIndex);

            if (final >= 1) {
                foregroundTile = tileStone;
            } else {
                foregroundTile = 0;
            }

            float background = baseTerrain + hills;
            if (background >= 1) {
                backgroundTile = tileStone;
            } else {
                backgroundTile = tileEmpty;
            }
        }
    }
}
