// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerSystem : GameSystemBase, IGameSystem {

        private List<Player> players = new List<Player>();

        public static PlayerSystem Create() {
            PlayerSystem sys = Create<PlayerSystem>();

            // Add Player
            Player player = new Player();
            PlayerCharacter characterPrefab = Resources.Load<PlayerCharacter>("PlayerCharacter");
            PlayerCharacter playerCharacter = Instantiate(characterPrefab);
            UnityEngine.Assertions.Assert.IsNotNull(playerCharacter, "character is null!");
            player.Possess(playerCharacter);
            sys.players.Add(player);

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
                if(p != null) p.Character.enabled = false;
            }
        }

        protected override void Enable() {
            foreach (Player p in players) { p.Character.enabled = true; }
        }
    }
}
