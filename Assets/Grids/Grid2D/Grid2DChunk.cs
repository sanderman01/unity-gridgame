// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.Grids {

    public class Grid2DChunk : MonoBehaviour {

        [SerializeField]
        private Int2 chunkCoord;
        public Int2 ChunkCoord { get { return chunkCoord; } }

        private Grid2D grid;
        public Grid2D ParentGrid { get { return grid; } }

        private ChunkData chunkData;
        public ChunkData Data { get { return chunkData; } }

        public void Setup(Int2 chunkCoord, Grid2D parentGrid, ChunkData chunkData) {
            this.chunkCoord = chunkCoord;
            this.grid = parentGrid;
            this.chunkData = chunkData;
        }
    }
}