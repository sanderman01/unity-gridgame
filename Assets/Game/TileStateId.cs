// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame {

    /// <summary>
    /// Combines a tile type id and a meta value in one single unsigned integer.
    /// The 24 most significant bits are used for the tile type id.
    /// The 8 least significant bits are used for the meta value.
    /// The range of valid meta values is 0-255.
    /// </summary>
    public struct TileStateId {

        private readonly uint state;

        public uint TileId {
            get { return ToTileType(state); }
        }
        public uint Meta {
            get { return ToMeta(state); }
        }

        public TileStateId(uint stateId) {
            this.state = stateId;
        }

        public TileStateId(uint tileTypeId, uint metaData) {
            UnityEngine.Assertions.Assert.IsTrue(metaData < 256);
            this.state = (uint)((tileTypeId << 8) | metaData);
        }

        public static uint ToTileType(uint stateId) {
            return stateId >> 8;
        }

        public static uint ToMeta(uint stateId) {
            const uint maxMeta = 255;
            return stateId & maxMeta;
        }

        public static explicit operator TileStateId(uint stateId) {
            return new TileStateId(stateId);
        }

        public static explicit operator uint(TileStateId stateId) {
            return stateId.state;
        }
    }
}
