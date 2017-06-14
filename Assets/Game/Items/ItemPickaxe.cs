// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using UnityEngine;

namespace AmarokGames.GridGame.Items {

    public class ItemPickaxe : ItemTool {

        WorldManagementSystem worldMgr;

        public override void PostInit(Main game) {
            this.worldMgr = (WorldManagementSystem)game.GetSystem(typeof(WorldManagementSystem));
        }

        public override void MouseDown(Player player, int mouseButton, Vector2 screenPos, Vector2 worldPos) {
            Debug.Log("MouseDown");
            worldMgr.StartBreakingTile(player, worldPos);
        }

        public override void MouseUp(Player player, int mouseButton, Vector2 screenPos, Vector2 worldPos) {
            Debug.Log("MouseUp");
            worldMgr.StopBreakingTile(player, worldPos);
        }
    }
}
