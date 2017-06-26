using System;
using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;
using System.Collections.Generic;

namespace AmarokGames.GridGame {

    public class WorldManagementSystem : GameSystemBase, IGameSystem {

        private TileRegistry tileRegistry;
        private LayerId solidLayerBool;
        private LayerId tileForegroundLayerUInt;
        private LayerId tileBackgroundLayerUInt;

        // A list of tiles that are currently in the process of being broken, plus associated meta data.
        private Dictionary<TilePosition, TileBreaking> tilesBreaking = new Dictionary<TilePosition, TileBreaking>();

        // Represents a single tile that is currently in the process of being broken.
        public class TileBreaking {

            public readonly TilePosition tilePos;
            public readonly Player player;
            public int tileHealth;
            public int damagePerTick;
            public int lastTouchedFrame;

            public TileBreaking(Player player, TilePosition tilePos, int tileHealth, int damagePerTick, int lastTouchedFrame) {
                this.player = player;
                this.tilePos = tilePos;
                this.tileHealth = tileHealth;
                this.damagePerTick = damagePerTick;
                this.lastTouchedFrame = lastTouchedFrame;
            }
        }

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

        public void BreakTileOverTime(Player player, World world, Grid2D grid, Int2 gridCoord, Vector2 worldPos) {
            // Add tile to collection of tiles that are currently being broken (if it's not already in the list)
            TilePosition tilePos = new TilePosition(world.WorldId, grid.GridId, gridCoord);

            TileBreaking tileBreaking;
            if(tilesBreaking.TryGetValue(tilePos, out tileBreaking)) {
                // Update the breaking of existing tile.
                tileBreaking.lastTouchedFrame = Time.frameCount;
            } else {
                // Start breaking a new tile
                StartBreakingTile(player, tilePos, worldPos);
            }
        }

        private void StartBreakingTile(Player player, TilePosition tilePos, Vector2 worldPos) {
            TileBreaking tileBreaking = new TileBreaking(player, tilePos, 100, 5, Time.frameCount);
            tilesBreaking.Add(tilePos, tileBreaking);
        }

        private void StopBreakingTile(Player player, TilePosition tilePos) {
            tilesBreaking.Remove(tilePos);
        }

        public void BreakTileImmediately(Player player, World world, Grid2D grid, Int2 gridCoord) {
            TileStateId tileStateId = (TileStateId)(uint)grid.GetCellValue(gridCoord, tileForegroundLayerUInt);
            if (tileStateId.TileId == 0) {
                // Empty tile. Don't do anything
            } else {
                // Non-empty tile. Break it.
                TileStateId emptyTile = new TileStateId(0, 0);
                grid.SetCellValue<uint>(gridCoord, tileForegroundLayerUInt, (uint)emptyTile);
                grid.SetCellValue<bool>(gridCoord, solidLayerBool, false);
            }
        }

        // Will be called a fixed number of times per second
        public override void TickWorld(World world, int tickRate) {
            UpdateBreakingTiles(world);
        }

        private void UpdateBreakingTiles(World world) {
            List<TileBreaking> tilesBreakingTemp = new List<TileBreaking>(tilesBreaking.Values);
            foreach (TileBreaking tileBreaking in tilesBreakingTemp) {
                TilePosition tilePos = tileBreaking.tilePos;
                if (IsRecent(tileBreaking.lastTouchedFrame)) {
                    // Continue breaking the tile
                    tileBreaking.tileHealth -= tileBreaking.damagePerTick;
                    if (tileBreaking.tileHealth <= 0) {
                        tilesBreaking.Remove(tilePos);
                        BreakTileImmediately(tileBreaking.player, world, world.Grids[tilePos.gridId], tilePos.gridCoord);
                    }
                } else {
                    // Stop breaking the tile
                    StopBreakingTile(tileBreaking.player, tilePos);
                }

            }
        }

        private static bool IsRecent(int lastFrame) {
            return Time.frameCount - lastFrame <= 5;
        }

        public override void UpdateWorld(World world, float deltaTime) {
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }

        public bool HasTile(Grid2D grid, Int2 gridCoord) {
            TileStateId tileState = (TileStateId)(uint)grid.GetCellValue(gridCoord, tileForegroundLayerUInt);
            return tileState.TileId != 0;
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
