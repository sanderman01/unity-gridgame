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

        private List<IGameSystem> gameSystems = new List<IGameSystem>();

        private Player player;
        private PlayerCharacter playerCharacter;

        private LayerConfig layers;
        LayerId solidLayerBool;
        LayerId tileForegroundLayerUShort;
        LayerId tileBackgroundLayerUShort;
        LayerId terrainGenDebugLayerFloat;

        public void Start() {
            tileRegistry = new TileRegistry();

            layers = new LayerConfig()
                .AddLayer("solid", BufferType.Boolean, out solidLayerBool)
                .AddLayer("tileforeground", BufferType.UShort, out tileForegroundLayerUShort)
                .AddLayer("tilebackground", BufferType.UShort, out tileBackgroundLayerUShort)
                .AddLayer("terrainGenDebugLayer", BufferType.Float, out terrainGenDebugLayerFloat);

            RegisterTiles(tileRegistry);
            CreateWorld(0);
            CreateRenderers();
            gameSystems.Add(GridCollisionSystem.Create(new Grids.Data.LayerId(0)));
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
            WorldGenerator worldGen = new WorldGenerator(solidLayerBool, tileForegroundLayerUShort, tileBackgroundLayerUShort, terrainGenDebugLayerFloat);
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, layers, worldGen);
            world.WorldGenerator.Init(world);
        }

        private void CreateRenderers() {

            // TODO Clean up this mess. Where should we put this renderer setup stuff?
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
                Shader shader = Shader.Find("Sprites/Default");
                Texture2D textureAtlas = tileRegistry.GetAtlas().GetTexture();

                Material foregroundMaterial = new Material(shader);
                foregroundMaterial.mainTexture = textureAtlas;
                foregroundMaterial.color = new Color(1, 1, 1, 1);

                Material backgroundMaterial = new Material(shader);
                backgroundMaterial.mainTexture = textureAtlas;
                backgroundMaterial.color = new Color(0.5f, 0.5f, 0.5f, 1);

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

                gameSystems.Add(GridTileRenderSystem.Create(tileData, foregroundMaterial, tileForegroundLayerUShort));
                gameSystems.Add(GridTileRenderSystem.Create(tileData, backgroundMaterial, tileBackgroundLayerUShort));
            }
        }

        void Update() {
            foreach (IGameSystem system in gameSystems) {
                if (system.Enabled) system.UpdateWorld(world, Time.deltaTime);
            }

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

            // find the first grid that overlaps with this world position.
            foreach (Grid2D grid in world.Grids) {
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
            grid.SetCellValue(gridCoord, tileForegroundLayerUShort, tileValue);

            bool solid = tileRegistry.GetTiles()[tileValue].CollisionSolid;
            grid.SetCellValue(gridCoord, solidLayerBool, solid);
        }
    }

}

