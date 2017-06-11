using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {
    class WorldManagementSystem : GameSystemBase, IGameSystem {

        private TileRegistry tileRegistry;
        private LayerId solidLayerBool;
        private LayerId tileForegroundLayerUShort;
        private LayerId tileBackgroundLayerUShort;

        public static WorldManagementSystem Create(
            TileRegistry tileRegistry,
            LayerId solidLayerBool,
            LayerId tileForegroundLayerUShort, 
            LayerId tileBackgroundLayerUShort) 
        {
            WorldManagementSystem sys = WorldManagementSystem.Create<WorldManagementSystem>();
            sys.tileRegistry = tileRegistry;
            sys.solidLayerBool = solidLayerBool;
            sys.tileForegroundLayerUShort = tileForegroundLayerUShort;
            sys.tileBackgroundLayerUShort = tileBackgroundLayerUShort;
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

        public void PlaceTile(World world, Vector2 worldPos, ushort tileValue) {

            // find the first grid that overlaps with this world position.
            foreach (Grid2D grid in world.Grids) {
                Bounds bounds = grid.GetBounds();
                if (bounds.Contains(worldPos)) {
                    // Found a valid grid
                    Vector2 localPos = grid.gameObject.transform.worldToLocalMatrix * worldPos;
                    Int2 gridCoord = new Int2(localPos.x, localPos.y);
                    PlaceTile(grid, gridCoord, tileValue);
                }
            }
        }

        public void PlaceTile(Grid2D grid, Int2 gridCoord, ushort tileValue) {
            grid.SetCellValue(gridCoord, tileForegroundLayerUShort, tileValue);

            bool solid = tileRegistry.GetTileById(tileValue).CollisionSolid;
            grid.SetCellValue(gridCoord, solidLayerBool, solid);
        }
    }
}
