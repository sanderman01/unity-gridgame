// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AmarokGames.GridGame {

    public class TileRegistry {

        DynamicTextureAtlas atlas;
        DynamicTextureAtlas itemAtlas;

        List<Tile> tilesByIndex;
        Dictionary<string, Tile> tilesByName;
        Dictionary<Tile, uint> tileToIdNumeric;
        Dictionary<Tile, string> tileToIdName;

        List<Item> itemsByIndex;
        Dictionary<string, Item> itemsByName;
        Dictionary<Item, uint> itemToIdNumeric;
        Dictionary<Item, string> itemToIdName;

        Dictionary<Tile, ItemTile> tileToItem;
        Dictionary<ItemTile, Tile> itemToTile;


        public TileRegistry() {
            tilesByIndex = new List<Tile>();
            tilesByName = new Dictionary<string, Tile>();
            tileToIdNumeric = new Dictionary<Tile, uint>();
            tileToIdName = new Dictionary<Tile, string>();

            itemsByIndex = new List<Item>();
            itemsByName = new Dictionary<string, Item>();
            itemToIdNumeric = new Dictionary<Item, uint>();
            itemToIdName = new Dictionary<Item, string>();

            tileToItem = new Dictionary<Tile, ItemTile>();
            itemToTile = new Dictionary<ItemTile, Tile>();

            atlas = new DynamicTextureAtlas();
        }

        public ItemTile GetItem(Tile tile) {
            return tileToItem[tile];
        }

        public int RegisterTile(string uniqueModIdName, string uniqueTileIdName, Tile tile, Texture2D texture) {
            int tileIndex = tilesByIndex.Count;
            tilesByIndex.Add(tile);
            tileToIdNumeric.Add(tile, (uint)tileIndex);

            uniqueModIdName = uniqueModIdName.ToLowerInvariant();
            uniqueTileIdName = uniqueTileIdName.ToLowerInvariant();
            string registeredName = string.Format("{0}.{1}", uniqueModIdName, uniqueTileIdName);
            tilesByName.Add(registeredName, tile);
            tileToIdName.Add(tile, registeredName);

            int tileAtlasIndex = atlas.AddTexture(texture);
            Debug.Assert(tileIndex == tileAtlasIndex);

            // Create default item for block and register it.
            ItemTile item = new ItemTile(tile);
            RegisterItem(uniqueModIdName, uniqueTileIdName, item, tile);

            return tileIndex;
        }

        public int RegisterItem(string uniqueModIdName, string uniqueItemIdName, ItemTile item, Tile tile) {
            int itemIndex = itemsByIndex.Count;
            itemsByIndex.Add(item);
            itemToIdNumeric.Add(item, (uint)itemIndex);

            uniqueModIdName = uniqueItemIdName.ToLowerInvariant();
            uniqueItemIdName = uniqueItemIdName.ToLowerInvariant();
            string registeredName = string.Format("{0}.{1}", uniqueModIdName, uniqueItemIdName);
            itemsByName.Add(registeredName, item);
            itemToIdName.Add(item, registeredName);

            itemToTile.Add(item, tile);
            tileToItem.Add(tile, item);

            return itemIndex;
        }

        /// <summary>
        /// No more tiles should be registered past this point.
        /// </summary>
        public void Finalise() {
            atlas.Finalise();
            Texture2D atlasTex = atlas.GetTexture();
            atlasTex.filterMode = FilterMode.Point;

            // For every tile, set the spriteUV.
            for (int i = 0; i < tilesByIndex.Count; ++i) {
                Tile tile = tilesByIndex[i];
                tile.SpriteUV = atlas.GetSprite(i);

                // Also set the item icon uv.
                ItemTile item = tileToItem[tile];
                item.IconTexture = atlasTex;
                TileVariant[] tileVariants = GetTileVariants(tile.SpriteUV);
                item.IconUV = GetTileVariantIcons(tileVariants);
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

        public uint GetTileId(Tile tile) {
            return tileToIdNumeric[tile];
        }

        public int GetTileCount() {
            return tilesByIndex.Count;
        }

        public static TileVariant[] GetTileVariants(Rect SpriteUV) {
            // Calculate the number of tile variants in the sprite from the demensions ratio.
            float ratio = SpriteUV.width / SpriteUV.height;
            int nVariants = Mathf.RoundToInt(ratio * (3f / 4f));

            TileVariant[] variants = new TileVariant[nVariants];
            float variantWidth = (SpriteUV.width / nVariants);

            for (int variantIndex = 0; variantIndex < nVariants; variantIndex++) {
                TileVariant variant = new TileVariant(
                    new Vector2(SpriteUV.x + variantWidth * variantIndex, SpriteUV.y),
                    new Vector2(SpriteUV.x + variantWidth * (variantIndex + 1), SpriteUV.yMax));
                variants[variantIndex] = variant;
            }
            return variants;
        }

        public static Rect[] GetTileVariantIcons(TileVariant[] variants) {
            Rect[] iconUVs = new Rect[variants.Length];
            for (int i = 0; i < variants.Length; i++) {
                TileVariant variant = variants[i];
                Rect iconUV = new Rect();
                iconUV.xMin = variant.uvLeft.uv00.x;
                iconUV.yMin = variant.uvBottom.uv00.y;
                iconUV.xMax = variant.uvRight.uv11.x;
                iconUV.yMax = variant.uvTop.uv11.y;
                iconUVs[i] = iconUV;
            }
            return iconUVs;
        }
    }
}