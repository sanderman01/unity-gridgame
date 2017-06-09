// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class TileRegistry {

        DynamicTextureAtlas atlas;

        List<Tile> tilesByIndex;
        Dictionary<string, Tile> tilesByName;
        

        public TileRegistry() {
            tilesByIndex = new List<Tile>();
            tilesByName = new Dictionary<string, Tile>();
            atlas = new DynamicTextureAtlas();
        }

        public int RegisterTile(string mod, string name, Tile tile, Texture2D texture) {
            int tileIndex = tilesByIndex.Count;
            tile.TileID = (ushort)tileIndex;
            tilesByIndex.Add(tile);

            mod = mod.ToLowerInvariant();
            name = name.ToLowerInvariant();
            tilesByName.Add(string.Format("{0}.{1}", mod, name), tile);

            int tileAtlasIndex = atlas.AddTexture(texture);
            Debug.Assert(tileIndex == tileAtlasIndex);

            return tileIndex;
        }

        /// <summary>
        /// No more tiles should be registered past this point.
        /// </summary>
        public void Finalise() {
            atlas.Finalise();
            Texture2D atlasTex = atlas.GetTexture();
            atlasTex.filterMode = FilterMode.Point;

            for (int i = 0; i < tilesByIndex.Count; ++i) {
                Tile tile = tilesByIndex[i];
                tile.SpriteUV = atlas.GetSprite(i);
            }
        }

        public DynamicTextureAtlas GetAtlas() {
            return atlas;
        }

        public Tile GetTile(int tileId) {
            return tilesByIndex[tileId];
        }

        public Tile GetTile(string tileName) {
            return tilesByName[tileName];
        }

        public int GetTileCount() {
            return tilesByIndex.Count;
        }
    }
}