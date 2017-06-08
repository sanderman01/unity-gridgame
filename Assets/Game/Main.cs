// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class Main : MonoBehaviour {
        [SerializeField]
        private Int2 worldSize = new Int2(1024, 1024);
        [SerializeField]
        private Int2 worldChunkSize = new Int2(64, 64);

        private TileRegistry tileRegistry;
        private World world;

        private List<IGameSystem> gameSystems = new List<IGameSystem>();

        private LayerConfig layers;
        LayerId solidLayerBool;
        LayerId tileForegroundLayerUShort;
        LayerId tileBackgroundLayerUShort;
        LayerId terrainGenDebugLayerFloat;

        BaseGameMod baseGameMod;

        public void Start() {

            layers = new LayerConfig();
            tileRegistry = new TileRegistry();

            baseGameMod = new BaseGameMod();
            baseGameMod.Init(ref layers, tileRegistry, gameSystems);

            solidLayerBool = baseGameMod.solidLayerBool;
            tileForegroundLayerUShort = baseGameMod.tileForegroundLayerUShort;
            tileBackgroundLayerUShort = baseGameMod.tileBackgroundLayerUShort;
            terrainGenDebugLayerFloat = baseGameMod.terrainGenDebugLayerFloat;

            tileRegistry.Finalise();

            baseGameMod.PostInit(tileRegistry, gameSystems);

            CreateWorld(0);
        }

        private void CreateWorld(int seed) {
            WorldGenerator worldGen = baseGameMod.GetWorldGenerator();
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, layers, worldGen);
            world.WorldGenerator.Init(world);
        }

        void Update() {
            foreach (IGameSystem system in gameSystems) {
                if (system.Enabled) system.UpdateWorld(world, Time.deltaTime);
            }

            const int buttonLeft = 0;
            const int buttonRight = 1;
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if(Input.GetMouseButton(buttonLeft)) {
                PlaceTile(mouseWorldPos, 0);
            }
            else if(Input.GetMouseButton(buttonRight)) {
                PlaceTile(mouseWorldPos, 7);
            }
        }

        private void PlaceTile(Vector2 worldPos, ushort tileValue) {

            // find the first grid that overlaps with this world position.
            foreach (Grid2D grid in world.Grids) {
                Bounds bounds = grid.GetBounds();
                if(bounds.Contains(worldPos)) {
                    // Found a valid grid
                    Vector2 localPos = grid.gameObject.transform.worldToLocalMatrix * worldPos;
                    Int2 gridCoord = new Int2(localPos.x, localPos.y);
                    PlaceTile(grid, gridCoord, tileValue);
                }
            }
        }

        private void PlaceTile(Grid2D grid, Int2 gridCoord, ushort tileValue) {
            grid.SetCellValue(gridCoord, tileForegroundLayerUShort, tileValue);

            bool solid = tileRegistry.GetTiles()[tileValue].CollisionSolid;
            grid.SetCellValue(gridCoord, solidLayerBool, solid);
        }
    }

}

