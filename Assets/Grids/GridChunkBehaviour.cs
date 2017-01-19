// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.Grids {

    public class GridChunkBehaviour : MonoBehaviour {

        [SerializeField]
        private Int2 chunkCoord;
        public Int2 ChunkCoord { get { return chunkCoord; } }

        private Grid2D grid;
        public Grid2D ParentGrid { get { return grid; } }

        public void Setup(Int2 chunkCoord, Grid2D parentGrid) {
            this.chunkCoord = chunkCoord;
            this.grid = parentGrid;
        }
    }
}