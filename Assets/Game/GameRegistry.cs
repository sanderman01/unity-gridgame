// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;
using UnityEngine;
using System;
using AmarokGames.GridGame.Items;

namespace AmarokGames.GridGame {

    public class GameRegistry {

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

        Dictionary<Item, int> itemToAtlasIndex;


        public GameRegistry() {
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
            itemAtlas = new DynamicTextureAtlas();

            itemToAtlasIndex = new Dictionary<Item, int>();
        }

        public IEnumerable<Item> GetItems() {
            return itemsByIndex;
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

        private int RegisterItem(string uniqueModIdName, string uniqueItemIdName, ItemTile item, Tile tile) {
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

        public int RegisterItem(string uniqueModIdName, string uniqueItemIdName, Item item, Texture2D iconTexture) {
            int itemIndex = itemsByIndex.Count;
            itemsByIndex.Add(item);
            itemToIdNumeric.Add(item, (uint)itemIndex);

            uniqueModIdName = uniqueItemIdName.ToLowerInvariant();
            uniqueItemIdName = uniqueItemIdName.ToLowerInvariant();
            string registeredName = string.Format("{0}.{1}", uniqueModIdName, uniqueItemIdName);
            itemsByName.Add(registeredName, item);
            itemToIdName.Add(item, registeredName);

            int tileAtlasIndex = itemAtlas.AddTexture(iconTexture);
            itemToAtlasIndex.Add(item, tileAtlasIndex);

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

            itemAtlas.Finalise();
            Texture2D itemAtlasTex = itemAtlas.GetTexture();
            itemAtlasTex.filterMode = FilterMode.Point;

            // For every item that uses the itemAtlas, set the spriteUV
            foreach (Item item in itemToAtlasIndex.Keys) {
                int atlasIndex = itemToAtlasIndex[item];
                Rect itemSprite = itemAtlas.GetSprite(atlasIndex);
                item.IconUV = new Rect[] { itemSprite };
                item.IconTexture = itemAtlasTex;
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
                Vector2 bottomLeft = variant.uvOutsideBottomLeft.uv00;
                Vector2 topRight = variant.uvOutsideTopRight.uv11;
                // Use a margin to prevent possible pixel bleeding on the edges of the icon.
                Vector2 margin = (1/1024f) * (topRight - bottomLeft);
                Rect iconUV = new Rect();
                iconUV.xMin = bottomLeft.x + margin.x;
                iconUV.yMin = bottomLeft.y + margin.y;
                iconUV.xMax = topRight.x - margin.x;
                iconUV.yMax = topRight.y - margin.y;
                iconUVs[i] = iconUV;
            }
            return iconUVs;
        }
    }
}