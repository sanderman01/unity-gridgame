// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using UnityEngine;

namespace AmarokGames.GridGame.Items {

    public class ItemPickaxe : ItemTool {

        WorldManagementSystem worldMgr;

        public int TileDamage { get; set; }

        public override void PostInit(Main game) {
            this.worldMgr = (WorldManagementSystem)game.GetSystem(typeof(WorldManagementSystem));
        }

        public override void MouseDown(Player player, ItemStack stack, int mouseButton, Vector2 screenPos, Vector2 worldPos) {
            //Debug.Log("MouseDown");
        }

        public override void MousePressed(Player player, ItemStack stack, int mouseButton, Vector2 screenPos, Vector2 worldPos) {
            //Debug.Log("MousePressed");
            Grid2D grid = player.CurrentWorld.GetGrid(worldPos);
            if(grid != null) {
                Int2 gridCoord = grid.GetGridCoord(worldPos);
                if(worldMgr.HasTile(grid, gridCoord)) {
                    worldMgr.BreakTileOverTime(player, player.CurrentWorld, player.CurrentWorld.GetGrid(worldPos), gridCoord, worldPos);
                }
            }
        }

        public override void MouseUp(Player player, ItemStack stack, int mouseButton, Vector2 screenPos, Vector2 worldPos) {
            //Debug.Log("MouseUp");
        }
    }
}
