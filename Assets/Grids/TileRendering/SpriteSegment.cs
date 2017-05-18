// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.Grids {

    /// <summary>
    /// Contains texture uv coordinates for a single segment of a sprite. In other words, this represents a tiny subsection of a sprite.
    /// </summary>
    public struct SpriteSegment {
        /// <summary>
        /// The uv coordinate of the bottom-left corner of the segment.
        /// </summary>
        public readonly Vector2 uv00;

        /// <summary>
        /// The uv coordinate of the top-right corner of the segment.
        /// </summary>
        public readonly Vector2 uv11;

        /// <summary>
        /// Create a TileSegment with the specified uv coordinates.
        /// </summary>
        /// <param name="uv00">The uv position of the bottom-left corner</param>
        /// <param name="uv11">The uv position of the top-right corner</param>
        public SpriteSegment(Vector2 uv00, Vector2 uv11) {
            this.uv00 = uv00;
            this.uv11 = uv11;
        }

        /// <summary>
        /// Create a TileSegment with the specified uv coordinates
        /// </summary>
        /// <param name="u0">The u coordinate of the left border</param>
        /// <param name="v0">The v coordinate of the bottom border</param>
        /// <param name="u1">The u coordinate of the right border</param>
        /// <param name="v1">The v coordinate of the top border</param>
        public SpriteSegment(float u0, float v0, float u1, float v1) {
            this.uv00 = new Vector2(u0, v0);
            this.uv11 = new Vector2(u1, v1);
        }
    }
}
