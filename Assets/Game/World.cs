// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class World : MonoBehaviour {
        public int WorldId { get; private set; }
        public Int2 Size { get; private set; }
        public Int2 ChunkSize { get; private set; }
        public int Seed { get; private set; }

        public readonly List<Grid2D> Grids = new List<Grid2D>();

        public LayerConfig Layers { get; private set; }

        public WorldGenerator WorldGenerator { get; private set; }

        public static World CreateWorld(string name, int worldId, Int2 worldSize, Int2 worldChunkSize, LayerConfig layers, WorldGenerator worldGenerator) {
            GameObject worldGameObject = new GameObject(name);
            World world = worldGameObject.AddComponent<World>();
            world.WorldId = worldId;
            world.Size = worldSize;
            world.ChunkSize = worldChunkSize;
            world.Layers = layers;
            world.WorldGenerator = worldGenerator;
            return world;
        }

        public Grid2D CreateGrid(string name) {
            GameObject gridGameObject = new GameObject(name);
            gridGameObject.transform.SetParent(this.transform, false);
            Grid2D grid = gridGameObject.AddComponent<Grid2D>();
            Grids.Add(grid);
            grid.Setup(0, ChunkSize.x, ChunkSize.y, Layers);
            return grid;
        }
    }
}