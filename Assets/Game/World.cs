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

        private int currentGridId = 0;

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

        public Grid2D GetGrid(int id) {
            return Grids[id];
        }

        public Grid2D GetGrid(Vector2 worldPos) {
            // TODO Fix this to more accurately return the right grid.
            // Prioritize dynamic grids before static grids
            // Prioritize grids which have a chunk that contains this worldPos in its bounds.
            // Prioritize grids that have a chunk containing a filled tile on the worldPos.
            foreach(Grid2D grid in Grids) {
                if(grid.GetBounds2D().Contains(worldPos)) {
                    return grid;
                }
            }
            return null;
        }

        public Grid2D CreateGrid(string name, Grid2D.GridType gridType) {
            GameObject gridGameObject = new GameObject(name);
            gridGameObject.transform.SetParent(this.transform, false);
            Grid2D grid = gridGameObject.AddComponent<Grid2D>();
            Grids.Add(grid);
            grid.Setup(currentGridId, Layers, gridType);
            currentGridId++;
            return grid;
        }
    }
}