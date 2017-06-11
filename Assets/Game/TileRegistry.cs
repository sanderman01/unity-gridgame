// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class TileRegistry {

        DynamicTextureAtlas atlas;

        List<Tile> tilesByIndex;
        Dictionary<string, Tile> tilesByName;
        Dictionary<Tile, int> tileToIdNumeric;
        Dictionary<Tile, string> tileToIdName;
        
        public TileRegistry() {
            tilesByIndex = new List<Tile>();
            tilesByName = new Dictionary<string, Tile>();
            tileToIdNumeric = new Dictionary<Tile, int>();
            tileToIdName = new Dictionary<Tile, string>();

            atlas = new DynamicTextureAtlas();
        }

        public int RegisterTile(string uniqueModIdName, string uniqueTileIdName, Tile tile, Texture2D texture) {
            int tileIndex = tilesByIndex.Count;
            tilesByIndex.Add(tile);
            tileToIdNumeric.Add(tile, tileIndex);

            uniqueModIdName = uniqueModIdName.ToLowerInvariant();
            uniqueTileIdName = uniqueTileIdName.ToLowerInvariant();
            var registeredName = string.Format("{0}.{1}", uniqueModIdName, uniqueTileIdName);
            tilesByName.Add(registeredName, tile);
            tileToIdName.Add(tile, registeredName);

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

        public Tile GetTileById(int tileId) {
            return tilesByIndex[tileId];
        }

        public Tile GetTileByName(string tileName) {
            return tilesByName[tileName];
        }

        public string GetTileIdName(Tile tile) {
            return tileToIdName[tile];
        }

        public int GetTileId(Tile tile) {
            return tileToIdNumeric[tile];
        }

        public int GetTileCount() {
            return tilesByIndex.Count;
        }
    }
}