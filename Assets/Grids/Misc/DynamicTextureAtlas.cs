// Copyright(C) 2018 Alexander Verbeek

using System.Collections.Generic;
using UnityEngine;

namespace GridGame
{
    /// <summary>
    /// Allows creating a texture atlas at runtime from many smaller textures.
    /// After adding textures. Finalise needs to be called once, to create the atlas texture.
    /// After Finalise was called once, the resulting sprites data can be accessed by index.
    /// </summary>
    [System.Serializable]
    public class DynamicTextureAtlas
    {
        public readonly int Padding = 0;
        public readonly int MaxAtlasSize = 8192;
        public readonly bool MakeNoLongerReadable = false;

        private List<Texture2D> _textures;
        private List<Vector2> _pivots;

        [SerializeField]
        private Texture2D _atlas;
        [SerializeField]
        private Rect[] _spriteUV;
        [SerializeField]
        private Sprite[] _sprites;

        private readonly Vector2 DefaultPivot = new Vector2(0.5f, 0.5f);

        public DynamicTextureAtlas(int padding = 0, int maxAtlasSize = 8192, bool makeNoLongerReadable = false)
        {
            this.Padding = padding;
            this.MaxAtlasSize = maxAtlasSize;
            this.MakeNoLongerReadable = makeNoLongerReadable;
            this._textures = new List<Texture2D>();
            this._pivots = new List<Vector2>();
        }

        public int AddSprite(Texture2D texture)
        {
            return AddSprite(texture, DefaultPivot);
        }

        public int AddSprite(Texture2D texture, Vector2 pivot)
        {
            Debug.Assert(_atlas == null, "Tried to add sprite to atlas, but Finalise was already called on this TextureAtlas!");

            int index = _textures.Count;
            _textures.Add(texture);
            _pivots.Add(pivot);
            return index;
        }

        public void Finalise()
        {
            Debug.Assert(_atlas == null, "Tried to call Finalise, but Finalise was already called previously on this TextureAtlas!");

            // pack all textures into one atlas
            _atlas = new Texture2D(0, 0);
            _spriteUV = _atlas.PackTextures(_textures.ToArray(), Padding, MaxAtlasSize, MakeNoLongerReadable);

            // Create list of sprites referencing into this atlas.
            _sprites = new Sprite[_spriteUV.Length];
            int w = _atlas.width;
            int h = _atlas.height;
            for (int i = 0; i < _sprites.Length; i++)
            {
                Rect uvRect = _spriteUV[i];
                Vector2 position = uvRect.position;
                Vector2 size = uvRect.size;
                Rect pxRect = new Rect(position.x * w, position.y * h, size.x * w, size.y * h);
                _sprites[i] = Sprite.Create(_atlas, pxRect, _pivots[i]);
            }

            // cleanup stuff we no longer need
            _textures.Clear();
            _pivots.Clear();
            _textures = null;
            _pivots = null;
        }

        public Rect GetSpriteUVRect(int index)
        {
            Debug.Assert(_sprites != null, "Tried to access sprite rect in atlas, but atlas was not yet finalised!");
            return _spriteUV[index];
        }

        public Sprite GetSprite(int index)
        {
            Debug.Assert(_sprites != null, "Tried to access sprite in atlas, but atlas was not yet finalised!");
            return _sprites[index];
        }

        public Texture2D GetTexture()
        {
            Debug.Assert(_atlas != null, "Tried to access atlas texture, but atlas was not yet finalised!");
            return _atlas;
        }
    }
}