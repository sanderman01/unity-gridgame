// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerSystem : GameSystemBase, IGameSystem {

        public Player LocalPlayer { get; private set; }

        private List<Player> players = new List<Player>();

        public static PlayerSystem Create(TileRegistry registry) {
            PlayerSystem sys = Create<PlayerSystem>();

            // Add Player
            Player player = new Player();

            PlayerCharacter characterPrefab = Resources.Load<PlayerCharacter>("PlayerCharacter");
            PlayerCharacter playerCharacter = Instantiate(characterPrefab);
            UnityEngine.Assertions.Assert.IsNotNull(playerCharacter, "character is null!");
            player.Possess(playerCharacter);
            sys.LocalPlayer = player;
            sys.players.Add(player);

            // Populate inventory with Itemstacks for testing.
            IInventory inv = player.HotbarInventory;
            for(int i = 0; i < inv.Count && i < registry.GetTileCount(); i++) {
                Tile tile = registry.GetTileById(i);
                Item item = registry.GetItem(tile);
                ItemStack stack = new ItemStack(item, 24, 0);
                IItemSlot slot = inv.GetSlot(i);
                slot.PutStack(stack);
            }

            Camera.main.GetComponent<Camera2D>().Target = playerCharacter.transform;
            return sys;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            foreach (Player p in players) {
                p.Update();
            }
        }

        protected override void Disable() {
            foreach (Player p in players) {
                if(p.Character != null) p.Character.enabled = false;
            }
        }

        protected override void Enable() {
            foreach (Player p in players) { p.Character.enabled = true; }
        }
    }
}
