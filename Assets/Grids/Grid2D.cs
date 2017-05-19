// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using System;

namespace AmarokGames.Grids {

    public class Grid2D {

        public int ChunkWidth { get { return chunkWidth; } }
        public int ChunkHeight { get { return chunkHeight; } }

        /// <summary>
        /// Contains a list of chunks that were created this frame.
        /// </summary>
        public IEnumerable<Int2> RecentlyCreated { get { return recentlyCreatedChunks; } }


        /// <summary>
        /// Contains a list of chunks that were removed this frame.
        /// </summary>
        public IEnumerable<Int2> RecentlyRemoved { get { return recentlyRemovedChunks; } }

        private List<Int2> recentlyCreatedChunks = new List<Int2>();
        private List<Int2> recentlyRemovedChunks = new List<Int2>();

        private readonly int chunkWidth;
        private readonly int chunkHeight;
        private readonly LayerConfig layers;
        private Dictionary<Int2, ChunkData> chunks = new Dictionary<Int2, ChunkData>();

        /// <summary>
        /// Create a new grid with the specified dimensions and layers. 
        /// Make sure to setup correctly setup the LayerConfig before creating any grids, since this cannot be changed after construction.
        /// </summary>
        public Grid2D(int chunkWidth, int chunkHeight, LayerConfig layers) {
            this.chunkWidth = chunkWidth;
            this.chunkHeight = chunkHeight;
            this.layers = layers;
        }

        /// <summary>
        /// Creates a new empty chunk at the specified chunk coordinate.
        /// </summary>
        public ChunkData CreateChunk(Int2 chunkCoord) {
            if (chunks.ContainsKey(chunkCoord)) {
                throw new System.Exception(string.Format("Chunk with offset {0} already exists in this grid.", chunkCoord));
            }

            ChunkData chunk = new ChunkData(chunkWidth * chunkHeight, layers);
            chunks.Add(chunkCoord, chunk);

            recentlyCreatedChunks.Add(chunkCoord);

            return chunk;
        }

        #region indexing

        /// <summary>
        /// Returns the chunk coordinate associated with the specified grid coordinate. 
        /// The chunk coordinate and the grid coordinate are equal for the bottom-left cell in a chunk.
        /// </summary>
        public static Int2 GetChunkCoord(Int2 gridCoord, int chunkWidth, int chunkHeight) {
            int x = gridCoord.x / chunkWidth;
            int y = gridCoord.y / chunkHeight;

            // in case of negative numbers, we need to subtract by one chunk to get the correct value.
            // eg. for cell (-1,-1) we should get chunk (-64, -64) instead of chunk (0,0)
            // this could probably be optimized by removing the conditional.
            if (gridCoord.x < 0) x -= 1;
            if (gridCoord.y < 0) y -= 1;

            return new Int2(x, y);
        }

        /// <summary>
        /// Returns the grid coordinate associated with the specified chunk coordinate. 
        /// This grid coordinate corresponds to the bottom-left corner of the chunk.
        /// </summary>
        public static Int2 GetGridCoord(Int2 chunkCoord, int chunkWidth, int chunkHeight) {
            return new Int2(chunkCoord.x * chunkWidth, chunkCoord.y * chunkHeight);
        }

        /// <summary>
        /// Returns the index within a chunk buffer for this grid-local coordinate.
        /// </summary>
        public static int GetCellIndex(Int2 gridCoord, int chunkWidth, int chunkHeight) {
            Int2 localCoord = new Int2(Modulo(gridCoord.x, chunkWidth), Modulo(gridCoord.y, chunkHeight));
            return GetCellIndex(localCoord, chunkWidth);
        }

        private static int Modulo(int x, int n) {
            int y = x % n;
            if (y < 0) y = y + n;
            return y;
        }

        /// <summary>
        /// Returns the index within a chunk buffer for this chunk-local coordinate.
        /// </summary>
        public static int GetCellIndex(Int2 localCoord, int chunkWidth) {
            return localCoord.y * chunkWidth + localCoord.x;
        }

        public static Int2 GetLocalGridCoordFromCellIndex(int index, int chunkWidth) {
            int x = index % chunkWidth;
            int y = index / chunkWidth;
            return new Int2(x, y);
        }

        public static Int2 GetGridCoordFromCellIndex(int index, Int2 chunkCoord, int chunkWidth, int chunkHeight) {
            Int2 localCoord = GetLocalGridCoordFromCellIndex(index, chunkWidth);
            Int2 chunkGridCoord = GetGridCoord(chunkCoord, chunkWidth, chunkHeight);
            return chunkGridCoord + localCoord;
        }

        #endregion

        #region data access

        public bool TryGetChunk(Int2 chunkCoord, out ChunkData result) {
            return chunks.TryGetValue(chunkCoord, out result);
        }

        public IDataBuffer TryGetBuffer(Int2 chunkCoord, int layerId) {
            ChunkData chunk;
            if(TryGetChunk(chunkCoord, out chunk)) {
                return chunk.GetBuffer(layerId);
            } else {
                return null;
            }
        }

        public IEnumerable<Int2> GetLoadedChunks() {
            return chunks.Keys;
        }

        public static IEnumerable<Int2> GetChunks(Int2 minGridCoord, Int2 maxGridCoord, int chunkWidth, int chunkHeight) {
            // round down to chunk boundaries.
            Int2 minChunk = GetChunkCoord(minGridCoord, chunkWidth, chunkHeight);
            Int2 maxChunk = GetChunkCoord(maxGridCoord, chunkWidth, chunkHeight);

            List<Int2> result = new List<Int2>();
            for (int y = minChunk.y; y <= maxChunk.y; ++y) {
                for (int x = minChunk.x; x <= maxChunk.x; ++x) {
                    Int2 chunkCoord = new Int2(x, y);
                    result.Add(chunkCoord);
                }
            }
            return result;
        }

        // Temporarily use this until we figure out a better approach to accessing data across chunk boundaries
        public ushort GetUShort(Int2 gridCoord, int layerId) {
            Int2 chunkCoord = GetChunkCoord(gridCoord, chunkWidth, chunkHeight);
            ChunkData chunk;
            if (TryGetChunk(chunkCoord, out chunk)) {
                int index = GetCellIndex(gridCoord, chunkWidth, chunkHeight);
                UShortBuffer buffer = (UShortBuffer)chunk.GetBuffer(layerId);
                return buffer.GetValue(index);
            } else {
                return 0;
            }
        }

        //public bool GetBoolValue(Int2 gridCoord, int layer) {
        //    Int2 chunkCoord = GetChunkCoord(gridCoord, chunkWidth, chunkHeight);
        //    Chunk chunk = GetChunk(chunkCoord);
        //    BooleanBuffer buffer = (BooleanBuffer)chunk.GetBuffer(layer);
        //    int cellIndex = GetCellIndex(gridCoord, chunkWidth);
        //    return buffer.GetValue(cellIndex);
        //}

        //public void SetBoolValue(bool value, Int2 gridCoord, int layer) {
        //    Int2 chunkCoord = GetChunkCoord(gridCoord, chunkWidth, chunkHeight);
        //    Chunk chunk = GetChunk(chunkCoord);
        //    BooleanBuffer buffer = (BooleanBuffer)chunk.GetBuffer(layer);
        //    int cellIndex = GetCellIndex(gridCoord, chunkWidth);
        //    buffer.SetValue(value, cellIndex);
        //}

        //public static bool GetBoolValue(Int2 gridCoord, int layer, int chunkWidth, int chunkHeight, BooleanBuffer buffer) {
        //    Int2 chunkCoord = GetChunkCoord(gridCoord, chunkWidth, chunkHeight);
        //    int cellIndex = GetCellIndex(gridCoord, chunkWidth);
        //    return buffer.GetValue(cellIndex);
        //}

        //public static void SetBoolValue(bool value, Int2 gridCoord, int layer, int chunkWidth, int chunkHeight, BooleanBuffer buffer) {
        //    Int2 chunkCoord = GetChunkCoord(gridCoord, chunkWidth, chunkHeight);
        //    int cellIndex = GetCellIndex(gridCoord, chunkWidth);
        //    buffer.SetValue(value, cellIndex);
        //}

        #endregion

        /// <summary>
        /// Clears the recently created and recently removed chunks lists.
        /// Call this near the end of a frame, after other systems have had a chance to respond to chunk changes. 
        /// (eg. at the end of Update or in LateUpdate)
        /// </summary>
        public void ClearRecent() {
            recentlyCreatedChunks.Clear();
            recentlyRemovedChunks.Clear();
        }
    }
}
