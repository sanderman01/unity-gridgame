using System;
using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {
    public class WorldManagementSystem : GameSystemBase, IGameSystem {

        private TileRegistry tileRegistry;
        private LayerId solidLayerBool;
        private LayerId tileForegroundLayerUInt;
        private LayerId tileBackgroundLayerUInt;

        public static WorldManagementSystem Create(
            TileRegistry tileRegistry,
            LayerId solidLayerBool,
            LayerId tileForegroundLayerUInt, 
            LayerId tileBackgroundLayerUInt) 
        {
            WorldManagementSystem sys = WorldManagementSystem.Create<WorldManagementSystem>();
            sys.tileRegistry = tileRegistry;
            sys.solidLayerBool = solidLayerBool;
            sys.tileForegroundLayerUInt = tileForegroundLayerUInt;
            sys.tileBackgroundLayerUInt = tileBackgroundLayerUInt;
            return sys;
        }

        public void StopBreakingTile(Player player, Vector2 worldPos) {
            //Grid2D grid = player.CurrentWorld.GetGrid(worldPos);
        }

        public void StartBreakingTile(Player player, Vector2 worldPos) {
            Grid2D grid = player.CurrentWorld.GetGrid(worldPos);
            if(grid != null) {
                Int2 gridCoord = grid.GetGridCoord(worldPos);
                TileStateId tileStateId = (TileStateId)(uint)grid.GetCellValue(gridCoord, tileForegroundLayerUInt);
                if (tileStateId.TileId == 0) {
                    // Empty tile. Don't do anything
                } else {
                    // Non-empty tile. Break it.
                    TileStateId emptyTile = new TileStateId(0, 0);
                    grid.SetCellValue<uint>(gridCoord, tileForegroundLayerUInt, (uint)emptyTile);
                    grid.SetCellValue<bool>(gridCoord, solidLayerBool, false);
                    Debug.Log("not empty");
                }
            }
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }

        public void PlaceTile(World world, Vector2 worldPos, uint tileValue, uint meta) {

            // find the first grid that overlaps with this world position.
            foreach (Grid2D grid in world.Grids) {
                Bounds bounds = grid.GetBounds();
                if (bounds.Contains(worldPos)) {
                    // Found a valid grid
                    Vector2 localPos = grid.gameObject.transform.worldToLocalMatrix * worldPos;
                    Int2 gridCoord = new Int2(localPos.x, localPos.y);
                    PlaceTile(grid, gridCoord, tileValue, meta);
                }
            }
        }

        public void PlaceTile(Grid2D grid, Int2 gridCoord, uint tileTypeId, uint tileMetaData) {
            uint bufferValue = (uint)new TileStateId(tileTypeId, tileMetaData);
            grid.SetCellValue(gridCoord, tileForegroundLayerUInt, bufferValue);

            bool solid = tileRegistry.GetTileById((int)tileTypeId).CollisionSolid;
            grid.SetCellValue(gridCoord, solidLayerBool, solid);
        }


    }
}
