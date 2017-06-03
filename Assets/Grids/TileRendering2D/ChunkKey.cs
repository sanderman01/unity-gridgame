// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids {

    public struct ChunkKey {
        public int worldId;
        public int gridId;
        public Int2 chunkCoord;

        public ChunkKey(int worldId, int gridId, Int2 chunkCoord) {
            this.worldId = worldId;
            this.gridId = gridId;
            this.chunkCoord = chunkCoord;
        }

        public bool Equals(ChunkKey other) {
            return this.gridId == other.gridId && this.chunkCoord == other.chunkCoord;
        }

        public override bool Equals(object obj) {
            if (obj is ChunkKey) {
                ChunkKey other = (ChunkKey)obj;
                return Equals(other);
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + worldId.GetHashCode();
            hash = hash * 23 + gridId.GetHashCode();
            hash = hash * 23 + chunkCoord.GetHashCode();
            return hash;
        }
    }
}