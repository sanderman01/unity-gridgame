// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids {

    public struct TilePosition {

        public readonly int worldId;
        public readonly int gridId;
        public readonly Int2 gridCoord;

        public TilePosition(int worldId, int gridId, Int2 gridCoord) {
            this.worldId = worldId;
            this.gridId = gridId;
            this.gridCoord = gridCoord;
        }
    }

}
