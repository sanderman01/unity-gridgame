// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame.Inventory;
using AmarokGames.GridGame.Items;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerSystem : GameSystemBase, IGameSystem {

        public Player LocalPlayer { get; private set; }

        private List<Player> players = new List<Player>();
        private ItemStack[] playerStartEquipment;

        public static PlayerSystem Create(Main game, GameRegistry registry) {
            PlayerSystem sys = Create<PlayerSystem>();
            return sys;
        }

        public void PostInit(ItemStack[] playerStartEquipment) {
            this.playerStartEquipment = playerStartEquipment;
        }

        public override void OnWorldCreated(World world, GameRegistry registry) {
            // Add Player
            Player player = new Player();

            PlayerCharacter characterPrefab = Resources.Load<PlayerCharacter>("PlayerCharacter");
            PlayerCharacter playerCharacter = Instantiate(characterPrefab);
            UnityEngine.Assertions.Assert.IsNotNull(playerCharacter, "character is null!");
            player.Possess(playerCharacter);
            LocalPlayer = player;
            players.Add(player);

            LocalPlayer.CurrentWorld = world;

            // Populate inventory with Itemstacks for testing.
            IInventory hotbar = player.HotbarInventory;
            for (int i = 0; i < hotbar.Count && i < registry.GetTileCount(); i++) {
                Tile tile = registry.GetTileById(i);
                Item item = registry.GetItem(tile);
                ItemStack stack = new ItemStack(item, 1, 0);
                IInventorySlot slot = hotbar.GetSlot(i);
                slot.PutStack(stack);
            }

            IInventory maininv = player.MainInventory;
            for (int i = 0; i < maininv.Count; i++) {
                Tile tile = registry.GetTileById(1);
                Item item = registry.GetItem(tile);
                ItemStack stack = new ItemStack(item, 24, 0);
                IInventorySlot slot = maininv.GetSlot(i);
                slot.PutStack(stack);
            }

            IInventory mouseInv = player.MouseHeldInventory;
            {
                Tile tile = registry.GetTileById(1);
                Item item = registry.GetItem(tile);
                ItemStack stack = new ItemStack(item, 24, 0);
                IInventorySlot slot = mouseInv.GetSlot(0);
                slot.PutStack(stack);
            }

            // Add player starting equipment
            for (int i = 0; i < playerStartEquipment.Length; i++) {
                hotbar[i] = playerStartEquipment[i];
            }

            Camera.main.GetComponent<Camera2D>().Target = playerCharacter.transform;
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
