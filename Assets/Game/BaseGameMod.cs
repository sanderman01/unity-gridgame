// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using AmarokGames.Grids;

namespace AmarokGames.GridGame {

    public class BaseGameMod {

        public LayerId SolidLayerBool { get; private set; }
        public LayerId TileForegroundLayerUShort { get; private set; }
        public LayerId TileBackgroundLayerUShort { get; private set; }
        public LayerId TerrainGenDebugLayerFloat { get; private set; }

        public Tile TileEmpty { get; private set; }
        public Tile TileStone { get; private set; }
        public Tile TileDirt { get; private set; }
        public Tile TileGrass { get; private set; }

        public void Init(ref LayerConfig layers, TileRegistry tileRegistry, List<IGameSystem> gameSystems) {

            SolidLayerBool = layers.AddLayer("solid", BufferType.Boolean);
            TileForegroundLayerUShort = layers.AddLayer("tileforeground", BufferType.UShort);
            TileBackgroundLayerUShort = layers.AddLayer("tilebackground", BufferType.UShort);
            TerrainGenDebugLayerFloat = layers.AddLayer("terrainGenDebugLayer", BufferType.Float);

            RegisterTiles(tileRegistry);
        }

        public void PostInit(TileRegistry tileRegistry, List<IGameSystem> gameSystems) {
            RegisterGameSystems(tileRegistry, gameSystems);
        }

        private void RegisterTiles(TileRegistry tileRegistry) {
            TileEmpty = new Tile();
            TileEmpty.CollisionSolid = false;
            TileEmpty.BatchedRendering = false;
            TileEmpty.HumanName = "Empty";
            Texture2D emptyTex = Resources.Load<Texture2D>("Tiles/tile-empty");

            TileStone = new Tile();
            TileStone.CollisionSolid = true;
            TileStone.BatchedRendering = true;
            TileStone.HumanName = "Stone";
            Texture2D stoneTex = Resources.Load<Texture2D>("Tiles/tile-stone");

            Tile concrete = new Tile();
            concrete.CollisionSolid = true;
            concrete.BatchedRendering = true;
            concrete.HumanName = "Concrete";
            Texture2D concreteTex = Resources.Load<Texture2D>("Tiles/tile-concrete");

            TileDirt = new Tile();
            TileDirt.CollisionSolid = true;
            TileDirt.BatchedRendering = true;
            TileDirt.HumanName = "Dirt";
            Texture2D dirtTex = Resources.Load<Texture2D>("Tiles/tile-dirt");

            TileGrass = new Tile();
            TileGrass.CollisionSolid = true;
            TileGrass.BatchedRendering = true;
            TileGrass.HumanName = "Grass";
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

            tileRegistry.RegisterTile("vanilla", "empty", TileEmpty, emptyTex);
            tileRegistry.RegisterTile("vanilla", "stone", TileStone, stoneTex);
            tileRegistry.RegisterTile("vanilla", "concrete", concrete, concreteTex);
            tileRegistry.RegisterTile("vanilla", "dirt", TileDirt, dirtTex);
            tileRegistry.RegisterTile("vanilla", "grass", TileGrass, grassTex);
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
                GridSolidRendererSystem solidRenderer = GridSolidRendererSystem.Create(mat, SolidLayerBool);
                gameSystems.Add(solidRenderer);
                solidRenderer.Enabled = false;
            }

            {
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                BufferVisualiserFloat sys = BufferVisualiserFloat.Create(mat, TerrainGenDebugLayerFloat);
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
                    Rect[] iconUVs = new Rect[nVariants];
                    for(int variantIndex = 0; variantIndex < nVariants; variantIndex++) 
                    {
                        TileVariant variant = new TileVariant(
                            new Vector2(tile.SpriteUV.x + variantWidth * variantIndex, tile.SpriteUV.y),
                            new Vector2(tile.SpriteUV.x + variantWidth * (variantIndex + 1), tile.SpriteUV.yMax));
                        d.variants[variantIndex] = variant;

                        Rect iconUV = new Rect();
                        iconUV.xMin = variant.uvLeft.uv00.x;
                        iconUV.yMin = variant.uvBottom.uv00.y;
                        iconUV.xMax = variant.uvRight.uv11.x;
                        iconUV.yMax = variant.uvTop.uv11.y;
                        iconUVs[variantIndex] = iconUV;
                    }
                    tileData[i] = d;
                    tile.IconUV = iconUVs;
                }

                gameSystems.Add(GridTileRenderSystem.Create(tileData, foregroundMaterial, TileForegroundLayerUShort, 0));
                gameSystems.Add(GridTileRenderSystem.Create(tileData, backgroundMaterial, TileBackgroundLayerUShort, 1));

                gameSystems.Add(GridCollisionSystem.Create(new Grids.Data.LayerId(0)));

                WorldManagementSystem worldMgr = WorldManagementSystem.Create(tileRegistry, SolidLayerBool, TileForegroundLayerUShort, TileBackgroundLayerUShort);
                gameSystems.Add(worldMgr);
                GridEditorSystem gridEditor = GridEditorSystem.Create(tileRegistry, worldMgr);
                gameSystems.Add(gridEditor);
            }

            // Player system
            {
                PlayerSystem sys = PlayerSystem.Create();
                gameSystems.Add(sys);
            }
        }

        public WorldGenerator GetWorldGenerator() {
            WorldGenerator worldGen = new WorldGenerator(SolidLayerBool, TileForegroundLayerUShort, TileBackgroundLayerUShort, TerrainGenDebugLayerFloat, 
                TileEmpty, TileStone, TileDirt, TileGrass
                );
            return worldGen;
        }
    }
}
