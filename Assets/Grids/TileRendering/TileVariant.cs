// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.Grids {

    /// <summary>
    /// Contains uv coordinates for all the different sprite segments of a tile variant.
    /// A type of tile may eventually be able to have multiple variants. Eg. Dyed windows, blue/green/yellow/short/long grass, etc.
    /// Or alternatively, we could display different variants on different tile coordinates semi-randomly to introduce irregularity/natural variation for tile like stone or grass.
    /// In the tilesheet texture, each TileVariant represents one fully drawn tile sprite area, which is divided into a grid of 4x6 segments,
    /// where the bottom 4x4 represent the main tile and the top-left 2x2 are used in situations where neigbouring tiles form an inside corner.
    /// </summary>
    public struct TileVariant {

        public readonly SpriteSegment uvMiddle;
        public readonly SpriteSegment uvTop;
        public readonly SpriteSegment uvTop1;
        public readonly SpriteSegment uvTop2;
        public readonly SpriteSegment uvBottom;
        public readonly SpriteSegment uvBottom1;
        public readonly SpriteSegment uvBottom2;
        public readonly SpriteSegment uvLeft;
        public readonly SpriteSegment uvLeft1;
        public readonly SpriteSegment uvLeft2;
        public readonly SpriteSegment uvRight;
        public readonly SpriteSegment uvRight1;
        public readonly SpriteSegment uvRight2;
        public readonly SpriteSegment uvOutsideTopLeft;
        public readonly SpriteSegment uvOutsideTopRight;
        public readonly SpriteSegment uvOutsideBottomLeft;
        public readonly SpriteSegment uvOutsideBottomRight;
        public readonly SpriteSegment uvInsideTopLeft;
        public readonly SpriteSegment uvInsideTopRight;
        public readonly SpriteSegment uvInsideBottomLeft;
        public readonly SpriteSegment uvInsideBottomRight;

        public TileVariant(Vector2 uv00, Vector2 uv11) {

            // Assume the following: 
            // Each tile variant sprite is 16 pixels wide and 24 high. (Or other size with same ratio)
            // The bottom 16x16 area contains the main tile plus its borders.
            // The topleft 8x8 area contains the inside corners for adjacent tiles.

            // Calculate the uvs of all tile segments
            Vector2 size = uv11 - uv00;
            Vector3 main = new Vector2(size.x, (2f / 3f) * size.y);
            Vector2 a = 0.25f * main;

            // Since our sprite is divided in segments, we'll use these common values. 
            float x0 = uv00.x + 0 * a.x;
            float x1 = uv00.x + 1 * a.x;
            float x2 = uv00.x + 2 * a.x;
            float x3 = uv00.x + 3 * a.x;
            float x4 = uv00.x + 4 * a.x;
            float y0 = uv00.y + 0 * a.y;
            float y1 = uv00.y + 1 * a.y;
            float y2 = uv00.y + 2 * a.y;
            float y3 = uv00.y + 3 * a.y;
            float y4 = uv00.y + 4 * a.y;
            float y5 = uv00.y + 5 * a.y;
            float y6 = uv00.y + 6 * a.y;

            uvMiddle  = new SpriteSegment(x1, y1, x3, y3);
            uvTop     = new SpriteSegment(x1, y3, x3, y4);
            uvTop1    = new SpriteSegment(x1, y3, x2, y4);
            uvTop2    = new SpriteSegment(x2, y3, x3, y4);
            uvBottom  = new SpriteSegment(x1, y0, x3, y1);
            uvBottom1 = new SpriteSegment(x1, y0, x2, y1);
            uvBottom2 = new SpriteSegment(x2, y0, x3, y1);
            uvLeft    = new SpriteSegment(x0, y1, x1, y3);
            uvLeft1   = new SpriteSegment(x0, y1, x1, y2);
            uvLeft2   = new SpriteSegment(x0, y2, x1, y3);
            uvRight   = new SpriteSegment(x3, y1, x4, y3);
            uvRight1  = new SpriteSegment(x3, y1, x4, y2);
            uvRight2  = new SpriteSegment(x3, y2, x4, y3);

            uvOutsideTopLeft     = new SpriteSegment(x0, y3, x1, y4);
            uvOutsideTopRight    = new SpriteSegment(x3, y3, x4, y4);
            uvOutsideBottomLeft  = new SpriteSegment(x0, y0, x1, y1);
            uvOutsideBottomRight = new SpriteSegment(x3, y0, x4, y1);

            uvInsideTopLeft     = new SpriteSegment(x0, y5, x1, y6);
            uvInsideTopRight    = new SpriteSegment(x1, y5, x2, y6);
            uvInsideBottomLeft  = new SpriteSegment(x0, y4, x1, y5);
            uvInsideBottomRight = new SpriteSegment(x1, y4, x2, y5);
        }
    }

}
