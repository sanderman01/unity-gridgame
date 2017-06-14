// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using UnityEngine;

namespace AmarokGames.GridGame.Items {

    public class Item {

        public virtual string HumanName { get; set; }
        public uint MaxQuantity { get; set; }
        public Rect[] IconUV;
        public Texture2D IconTexture;

        public virtual SimpleSprite GetIcon(uint quantity, uint meta) {
            return new SimpleSprite(IconUV[meta], IconTexture);
        }

        public virtual void PostInit(Main game) { }
    }
}
