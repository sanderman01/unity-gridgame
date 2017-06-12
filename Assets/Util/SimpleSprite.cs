// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public struct SimpleSprite {

        public readonly Rect uv;
        public readonly Texture2D texture;

        public SimpleSprite(Rect uv, Texture2D texture) {
            this.uv = uv;
            this.texture = texture;
        }
    }
}
