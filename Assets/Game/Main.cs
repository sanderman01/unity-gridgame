// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class Main : MonoBehaviour {
        [SerializeField]
        private Int2 worldSize = new Int2(1024, 1024);
        [SerializeField]
        private Int2 worldChunkSize = new Int2(64, 64);

        // TODO REMOVE THIS WHEN NO LONGER NEEDED FOR DEBUGGING
        [SerializeField]
        private Texture2D tempTileAtlasTex;

        private TileRegistry tileRegistry;
        private World world;

        private GridTileRenderer foregroundTileRenderer;

        public void Start() {
            tileRegistry = new TileRegistry();

            RegisterTiles(tileRegistry);
            CreateWorld();
            CreateRenderers();
        }

        private void RegisterTiles(TileRegistry tileRegistry) {

            Tile empty = new Tile();
            empty.CollisionSolid = false;
            empty.BatchedRendering = false;
            empty.HumanName = "Empty";
            Texture2D emptyTex = Resources.Load<Texture2D>("tile-empty");

            Tile stone = new Tile();
            stone.CollisionSolid = true;
            stone.BatchedRendering = true;
            stone.HumanName = "Stone";
            Texture2D stoneTex = Resources.Load<Texture2D>("tile-stone");

            Tile concrete = new Tile();
            concrete.CollisionSolid = true;
            concrete.BatchedRendering = true;
            concrete.HumanName = "Concrete";
            Texture2D concreteTex = Resources.Load<Texture2D>("tile-concrete");

            tileRegistry.RegisterTile("vanilla", "empty", empty, emptyTex);
            tileRegistry.RegisterTile("vanilla", "stone", stone, stoneTex);
            tileRegistry.RegisterTile("vanilla", "concrete", concrete, concreteTex);

            tileRegistry.Finalise();

            tempTileAtlasTex = tileRegistry.GetAtlas().GetTexture();
        }

        private void CreateWorld() {
            world = World.CreateWorld("world", worldSize, worldChunkSize);
        }

        private void CreateRenderers() {

            // TODO Clean up this mess. Where should we put this renderer setup stuff?
            {
                // Solid Renderer
                Shader shader = Shader.Find("Particles/Additive");
                Material mat = new Material(shader);
                GridSolidRenderer solidRenderer = GridSolidRenderer.Create("solidRenderer", mat, world.WorldGrid);
                solidRenderer.gameObject.SetActive(false);
            }

            {
                // Tile Renderer
                Shader shader = Shader.Find("Unlit/Transparent Cutout");
                Material mat = new Material(shader);
                mat.mainTexture = tileRegistry.GetAtlas().GetTexture();

                IList<Tile> tiles = tileRegistry.GetTiles();
                int tileCount = tiles.Count;
                TileRenderData[] tileData = new TileRenderData[tileCount];
                for(int i = 0; i < tileCount; ++i) {
                    Tile tile = tiles[i];
                    TileRenderData d = new TileRenderData();
                    d.draw = tile.BatchedRendering;
                    d.zLayer = (ushort)i;
                    d.variants = new TileVariant[1];
                    d.variants[0] = new TileVariant(
                        new Vector2(tile.SpriteUV.x, tile.SpriteUV.y), 
                        new Vector2(tile.SpriteUV.xMax, tile.SpriteUV.yMax));
                    tileData[i] = d;
                }

                foregroundTileRenderer = new GridTileRenderer(tileData, mat, new Grids.Data.LayerId(1));
            }

        }

        void Update() {
            foregroundTileRenderer.Update(world.WorldGrid);
        }
    }

}

