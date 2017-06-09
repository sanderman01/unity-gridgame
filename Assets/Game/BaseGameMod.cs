// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using AmarokGames.Grids;

namespace AmarokGames.GridGame {

    public class BaseGameMod {

        public LayerId solidLayerBool;
        public LayerId tileForegroundLayerUShort;
        public LayerId tileBackgroundLayerUShort;
        public LayerId terrainGenDebugLayerFloat;

        public void Init(ref LayerConfig layers, TileRegistry tileRegistry, List<IGameSystem> gameSystems) {

            layers = layers.AddLayer("solid", BufferType.Boolean, out solidLayerBool)
                .AddLayer("tileforeground", BufferType.UShort, out tileForegroundLayerUShort)
                .AddLayer("tilebackground", BufferType.UShort, out tileBackgroundLayerUShort)
                .AddLayer("terrainGenDebugLayer", BufferType.Float, out terrainGenDebugLayerFloat);

            RegisterTiles(tileRegistry);
            
        }

        public void PostInit(TileRegistry tileRegistry, List<IGameSystem> gameSystems) {
            RegisterGameSystems(tileRegistry, gameSystems);
        }

        public WorldGenerator GetWorldGenerator() {
            WorldGenerator worldGen = new WorldGenerator(solidLayerBool, tileForegroundLayerUShort, tileBackgroundLayerUShort, terrainGenDebugLayerFloat);
            return worldGen;
        }

        private void RegisterTiles(TileRegistry tileRegistry) {
            Tile empty = new Tile();
            empty.CollisionSolid = false;
            empty.BatchedRendering = false;
            empty.HumanName = "Empty";
            Texture2D emptyTex = Resources.Load<Texture2D>("Tiles/tile-empty");

            Tile stone = new Tile();
            stone.CollisionSolid = true;
            stone.BatchedRendering = true;
            stone.HumanName = "Stone";
            Texture2D stoneTex = Resources.Load<Texture2D>("Tiles/tile-stone");

            Tile concrete = new Tile();
            concrete.CollisionSolid = true;
            concrete.BatchedRendering = true;
            concrete.HumanName = "Concrete";
            Texture2D concreteTex = Resources.Load<Texture2D>("Tiles/tile-concrete");

            Tile dirt = new Tile();
            dirt.CollisionSolid = true;
            dirt.BatchedRendering = true;
            dirt.HumanName = "Dirt";
            Texture2D dirtTex = Resources.Load<Texture2D>("Tiles/tile-dirt");

            Tile grass = new Tile();
            grass.CollisionSolid = true;
            grass.BatchedRendering = true;
            grass.HumanName = "Grass";
            Texture2D grassTex = Resources.Load<Texture2D>("Tiles/tile-grass");

            Tile gravel = new Tile();
            gravel.CollisionSolid = true;
            gravel.BatchedRendering = true;
            gravel.HumanName = "Gravel";
            Texture2D gravelTex = Resources.Load<Texture2D>("Tiles/tile-gravel");

            Tile sand = new Tile();
            sand.CollisionSolid = true;
            sand.BatchedRendering = true;
            sand.HumanName = "Sand";
            Texture2D sandTex = Resources.Load<Texture2D>("Tiles/tile-sand");

            Tile wood = new Tile();
            wood.CollisionSolid = true;
            wood.BatchedRendering = true;
            wood.HumanName = "Wood";
            Texture2D woodTex = Resources.Load<Texture2D>("Tiles/tile-wood");

            tileRegistry.RegisterTile("vanilla", "empty", empty, emptyTex);
            tileRegistry.RegisterTile("vanilla", "stone", stone, stoneTex);
            tileRegistry.RegisterTile("vanilla", "concrete", concrete, concreteTex);
            tileRegistry.RegisterTile("vanilla", "dirt", dirt, dirtTex);
            tileRegistry.RegisterTile("vanilla", "grass", grass, grassTex);
            tileRegistry.RegisterTile("vanilla", "gravel", gravel, gravelTex);
            tileRegistry.RegisterTile("vanilla", "sand", sand, sandTex);
            tileRegistry.RegisterTile("vanilla", "wood", wood, woodTex);
        }

        private void RegisterGameSystems(TileRegistry tileRegistry, List<IGameSystem> gameSystems) {
            {
                // Solid Renderer
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                GridSolidRendererSystem solidRenderer = GridSolidRendererSystem.Create(mat, solidLayerBool);
                gameSystems.Add(solidRenderer);
                solidRenderer.Enabled = false;
            }

            {
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                BufferVisualiserFloat sys = BufferVisualiserFloat.Create(mat, terrainGenDebugLayerFloat);
                gameSystems.Add(sys);
                sys.Enabled = false;
            }

            {
                // Tile Renderer
                Texture2D textureAtlas = tileRegistry.GetAtlas().GetTexture();

                Shader foregroundShader = Shader.Find("Unlit/Transparent Cutout");
                Material foregroundMaterial = new Material(foregroundShader);
                foregroundMaterial.mainTexture = textureAtlas;
                foregroundMaterial.color = new Color(1, 1, 1, 1);

                Shader backgroundShader = Shader.Find("Sprites/Default");
                Material backgroundMaterial = new Material(backgroundShader);
                backgroundMaterial.mainTexture = textureAtlas;
                backgroundMaterial.color = new Color(0.5f, 0.5f, 0.5f, 1);

                int tileCount = tileRegistry.GetTileCount();
                TileRenderData[] tileData = new TileRenderData[tileCount];
                for (int i = 0; i < tileCount; ++i) {
                    Tile tile = tileRegistry.GetTile(i);
                    TileRenderData d = new TileRenderData();
                    d.draw = tile.BatchedRendering;
                    d.zLayer = (ushort)i;

                    // Calculate the number of tile variants in the sprite from the demensions ratio.
                    float ratio = tile.SpriteUV.width / tile.SpriteUV.height;
                    int nVariants = Mathf.RoundToInt(ratio * (3f/4f));

                    d.variants = new TileVariant[nVariants];

                    float variantWidth = (tile.SpriteUV.width / nVariants);
                    for(int variantIndex = 0; variantIndex < nVariants; variantIndex++) 
                    {
                        d.variants[variantIndex] = new TileVariant(
                            new Vector2(tile.SpriteUV.x + variantWidth * variantIndex, tile.SpriteUV.y),
                            new Vector2(tile.SpriteUV.x + variantWidth * (variantIndex + 1), tile.SpriteUV.yMax));
                    }
                    tileData[i] = d;
                }

                gameSystems.Add(GridTileRenderSystem.Create(tileData, foregroundMaterial, tileForegroundLayerUShort, 0));
                gameSystems.Add(GridTileRenderSystem.Create(tileData, backgroundMaterial, tileBackgroundLayerUShort, 1));

                gameSystems.Add(GridCollisionSystem.Create(new Grids.Data.LayerId(0)));
            }

            // Player system
            {
                PlayerSystem sys = PlayerSystem.Create();
                gameSystems.Add(sys);
            }
        }
    }
}
