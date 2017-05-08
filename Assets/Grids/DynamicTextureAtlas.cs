// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.Grids {

    /// <summary>
    /// Allows creating a texture atlas at runtime from many smaller textures.
    /// After adding textures. Finalise needs to be called once, to create the atlas texture.
    /// After Finalise was called once, the resulting sprites data can be accessed by index.
    /// </summary>
    public class DynamicTextureAtlas {

        public readonly int Padding = 0;
        public readonly int MaxAtlasSize = 8192;
        public readonly bool MakeNoLongerReadable = false;

        private List<Texture2D> textures;

        private Texture2D atlas;
        private Rect[] sprites;

        public DynamicTextureAtlas(int padding = 0, int maxAtlasSize = 8192, bool makeNoLongerReadable = false) {
            this.Padding = padding;
            this.MaxAtlasSize = maxAtlasSize;
            this.MakeNoLongerReadable = makeNoLongerReadable;
            this.textures = new List<Texture2D>();
        }

        public int AddTexture(Texture2D texture) {
            Debug.Assert(atlas == null, "Tried to add texture to atlas, but Finalise was already called on this TextureAtlas!");

            int index = textures.Count;
            textures.Add(texture);
            return index;
        }

        public void Finalise() {
            Debug.Assert(atlas == null, "Tried to call Finalise, but Finalise was already called previously on this TextureAtlas!");

            // pack all textures into one atlas
            atlas = new Texture2D(0, 0);
            sprites = atlas.PackTextures(textures.ToArray(), Padding, MaxAtlasSize, MakeNoLongerReadable);

            // cleanup textures we no longer need
            textures = null;
        }

        public Rect GetSprite(int index) {
            Debug.Assert(sprites != null, "Tried to access sprite rect in atlas, but atlas was not yet finalised!");
            return sprites[index];
        }

        public Texture2D GetTexture() {
            Debug.Assert(atlas != null, "Tried to access atlas texture, but atlas was not yet finalised!");
            return atlas;
        }
    }
}