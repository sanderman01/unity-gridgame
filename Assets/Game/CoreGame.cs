// Copyright(C) 2018 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame
{

    public class CoreGame : IGameMod {

        public const string CoreGameModId = "CoreGame";

        public LayerId SolidLayerBool { get; private set; }
        public LayerId TileForegroundLayerUInt { get; private set; }
        public LayerId TileBackgroundLayerUInt { get; private set; }
        public LayerId TerrainGenDebugLayerFloat { get; private set; }

        public GridTileRenderSystem TileRendererForeground { get; private set; }
        public GridTileBackgroundRenderSystem TileRendererBackground { get; private set; }
        public PlayerSystem PlayerSystem { get; private set; }

        public Tile TileEmpty { get; private set; }
        public Tile TileStone { get; private set; }
        public Tile TileDirt { get; private set; }
        public Tile TileGrass { get; private set; }

        public Item ItemPickaxe { get; private set; }

        public void PreInit(Main game, GameRegistry gameRegistry) {
            RegisterSystems(game, gameRegistry);
            RegisterContent(game, gameRegistry);
        }

        private void RegisterSystems(Main game, GameRegistry gameRegistry) {
            LayerConfig layers = game.Layers;
            SolidLayerBool = layers.AddLayer("solid", BufferType.Boolean);
            TileForegroundLayerUInt = layers.AddLayer("tileforeground", BufferType.UnsignedInt32);
            TileBackgroundLayerUInt = layers.AddLayer("tilebackground", BufferType.UnsignedInt32);
            TerrainGenDebugLayerFloat = layers.AddLayer("terrainGenDebugLayer", BufferType.Float);

            {
                // Solid Renderer
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                GridSolidRendererSystem solidRenderer = GridSolidRendererSystem.Create(mat, SolidLayerBool);
                game.AddSystem(solidRenderer);
                solidRenderer.Enabled = false;
            }

            {
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                BufferVisualiserFloat sys = BufferVisualiserFloat.Create(mat, TerrainGenDebugLayerFloat);
                game.AddSystem(sys);
                sys.Enabled = false;
            }

            {
                TileRendererForeground = GridTileRenderSystem.Create(TileForegroundLayerUInt, 0);
                TileRendererBackground = GridTileBackgroundRenderSystem.Create(TileBackgroundLayerUInt, 1);
                game.AddSystem(TileRendererForeground);
                game.AddSystem(TileRendererBackground);
            }

            game.AddSystem(GridColliderSystem.Create(SolidLayerBool));

            WorldManagementSystem worldMgr = WorldManagementSystem.Create(gameRegistry, SolidLayerBool, TileForegroundLayerUInt, TileBackgroundLayerUInt);
            game.AddSystem(worldMgr);

            PlayerSystem = PlayerSystem.Create(game, gameRegistry);
            game.AddSystem(PlayerSystem);

            //GridEditorSystem gridEditor = GridEditorSystem.Create(gameRegistry, worldMgr, playerSys.LocalPlayer);
            //gameSystems.Add(gridEditor);

            PlayerInventoryUISystem inventoryUI = PlayerInventoryUISystem.Create(PlayerSystem);
            game.AddSystem(inventoryUI);
        }

        private void RegisterContent(Main game, GameRegistry gameRegistry) {
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

            gameRegistry.RegisterTile(CoreGameModId, "empty", TileEmpty, emptyTex);
            gameRegistry.RegisterTile(CoreGameModId, "stone", TileStone, stoneTex);
            gameRegistry.RegisterTile(CoreGameModId, "concrete", concrete, concreteTex);
            gameRegistry.RegisterTile(CoreGameModId, "dirt", TileDirt, dirtTex);
            gameRegistry.RegisterTile(CoreGameModId, "grass", TileGrass, grassTex);
            gameRegistry.RegisterTile(CoreGameModId, "gravel", gravel, gravelTex);
            gameRegistry.RegisterTile(CoreGameModId, "sand", sand, sandTex);
            gameRegistry.RegisterTile(CoreGameModId, "wood", wood, woodTex);

            ItemPickaxe = new ItemPickaxe();
            ItemPickaxe.HumanName = "Pickaxe";
            Texture2D pickaxeTex = Resources.Load<Texture2D>("Items/item-pickaxe");
            gameRegistry.RegisterItem(CoreGameModId, "pickaxe", ItemPickaxe, pickaxeTex);
        }

        public void Init(Main game, GameRegistry gameRegistry) {
        }

        public void PostInit(Main game, GameRegistry gameRegistry) {

            // Tile Renderer
            Texture2D textureAtlas = gameRegistry.GetAtlas().GetTexture();

            Shader foregroundShader = Shader.Find("Unlit/Transparent Cutout");
            Material foregroundMaterial = new Material(foregroundShader);
            foregroundMaterial.mainTexture = textureAtlas;
            foregroundMaterial.color = new Color(1, 1, 1, 1);

            Shader backgroundShader = Shader.Find("Sprites/Default");
            Material backgroundMaterial = new Material(backgroundShader);
            backgroundMaterial.mainTexture = textureAtlas;
            backgroundMaterial.color = new Color(0.5f, 0.5f, 0.5f, 1);

            TileRenderData[] tileRenderData = GridTileRenderSystem.CreateTileRenderData(gameRegistry);
            TileRendererForeground.PostInit(tileRenderData, foregroundMaterial);
            TileRendererBackground.PostInit(tileRenderData, backgroundMaterial);

            // Player starting inventory
            ItemStack[] playerStartEquipment = new ItemStack[] { new ItemStack(ItemPickaxe, 1, 0) };
            PlayerSystem.PostInit(playerStartEquipment);

            foreach (Item item in gameRegistry.GetItems()) item.PostInit(game);
        }

        public WorldGenerator GetWorldGenerator(GameRegistry tileReg) {
            WorldGenerator worldGen = new WorldGenerator(SolidLayerBool, TileForegroundLayerUInt, TileBackgroundLayerUInt, TerrainGenDebugLayerFloat, 
                tileReg.GetTileId(TileEmpty), tileReg.GetTileId(TileStone), tileReg.GetTileId(TileDirt), tileReg.GetTileId(TileGrass)
                );
            return worldGen;
        }
    }
}
