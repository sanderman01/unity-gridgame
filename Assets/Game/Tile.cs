// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class Tile {
        public virtual string HumanName { get; set; }

        public virtual bool CollisionSolid { get; set; }

        public virtual bool BatchedRendering { get; set; }
        public Rect SpriteUV { get; set; }
    }
}
