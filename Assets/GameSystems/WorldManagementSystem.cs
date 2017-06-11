using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {
    class WorldManagementSystem : GameSystemBase, IGameSystem {

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
