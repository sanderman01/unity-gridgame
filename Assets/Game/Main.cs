// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
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

        private GridTileRenderSystem foregroundTileRenderer;

        private GridCollisionSystem collisionSystem;

        private GridSolidRenderer solidRenderSystem;

        private Player player;
        private PlayerCharacter playerCharacter;

        private LayerConfig layers;
        LayerId solidLayerIndex;
        LayerId tileForegroundLayerIndex;
        LayerId tileBackgroundLayerIndex;

        public void Start() {
            tileRegistry = new TileRegistry();

            layers = new LayerConfig()
                .AddLayer("solid", BufferType.Boolean, out solidLayerIndex)
                .AddLayer("tileforeground", BufferType.UShort, out tileForegroundLayerIndex)
                .AddLayer("tilebackground", BufferType.UShort, out tileBackgroundLayerIndex);

            RegisterTiles(tileRegistry);
            CreateWorld(0);
            CreateRenderers();
            collisionSystem = new GridCollisionSystem(new Grids.Data.LayerId(0));
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

            // Add Player
            player = new Player();
            PlayerCharacter characterPrefab = Resources.Load<PlayerCharacter>("PlayerCharacter");
            playerCharacter = Instantiate(characterPrefab);
            UnityEngine.Assertions.Assert.IsNotNull(playerCharacter, "character is null!");
            playerCharacter.Possess(player);

            Camera.main.GetComponent<Camera2D>().Target = playerCharacter.transform;
        }

        private void CreateWorld(int seed) {
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, seed, layers);
        }

        private void CreateRenderers() {

            // TODO Clean up this mess. Where should we put this renderer setup stuff?
            {
                // Solid Renderer
                Shader shader = Shader.Find("Particles/Additive");
                Material mat = new Material(shader);
                solidRenderSystem = new GridSolidRenderer("solidRenderer", mat, world.WorldGrid);
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

                foregroundTileRenderer = new GridTileRenderSystem(tileData, mat, new Grids.Data.LayerId(1));
            }
        }

        void Update() {
            Grid2D[] grids = new Grid2D[] { world.WorldGrid };
            foregroundTileRenderer.Update(world, grids);
            collisionSystem.Update(world, grids );
            //solidRenderSystem.Update(world, grids);
            player.Update();

            const int buttonLeft = 0;
            const int buttonRight = 1;
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if(Input.GetMouseButton(buttonLeft)) {
                PlaceTile(mouseWorldPos, 0);
            }
            else if(Input.GetMouseButton(buttonRight)) {
                PlaceTile(mouseWorldPos, 1);
            }
        }

        private void PlaceTile(Vector2 worldPos, ushort tileValue) {
            // right now we only have the worldgrid, but we'll eventually have more grids
            Grid2D[] grids = new Grid2D[] { world.WorldGrid };

            // find the first grid that overlaps with this world position.
            foreach (Grid2D grid in grids) {
                Bounds bounds = grid.GetBounds();
                if(bounds.Contains(worldPos)) {
                    // Found a valid grid
                    Vector2 localPos = grid.gameObject.transform.worldToLocalMatrix * worldPos;
                    Int2 gridCoord = new Int2(localPos.x, localPos.y);
                    PlaceTile(grid, gridCoord, tileValue);
                }
            }
        }

        private void PlaceTile(Grid2D grid, Int2 gridCoord, ushort tileValue) {
            grid.SetCellValue(gridCoord, tileForegroundLayerIndex, tileValue);

            bool solid = tileRegistry.GetTiles()[tileValue].CollisionSolid;
            grid.SetCellValue(gridCoord, solidLayerIndex, solid);
        }
    }

}

