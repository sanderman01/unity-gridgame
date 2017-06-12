// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public class Player {

        private PlayerCharacter playerCharacter;
        public PlayerCharacter Character { get { return playerCharacter; } }

        private const KeyCode KeyLeft = KeyCode.A;
        private const KeyCode KeyRight = KeyCode.D;
        private const KeyCode KeyUp = KeyCode.W;
        private const KeyCode KeyDown = KeyCode.S;
        private const KeyCode KeyJump = KeyCode.Space;

        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Down { get; set; }
        public bool Up { get; set; }
        public bool Jump { get; set; }

        public void Update() {
            Left = Input.GetKey(KeyLeft);
            Right = Input.GetKey(KeyRight);
            Up = Input.GetKey(KeyUp);
            Down = Input.GetKey(KeyDown);
            Jump = Input.GetKey(KeyJump);
        }

        public void Possess(PlayerCharacter playerCharacter) {
            if (this.playerCharacter != null) {
                this.playerCharacter.player = null;
            }

            if (playerCharacter != null) {
                playerCharacter.player = this;
            }
            this.playerCharacter = playerCharacter;
        }
    }
}
