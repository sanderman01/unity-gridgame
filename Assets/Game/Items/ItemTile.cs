// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame.Items {

    public class ItemTile : Item {

        Tile Tile { get; set; }

        public ItemTile(Tile tile) {
            this.Tile = tile;
            this.HumanName = tile.HumanName;
        }

        public override SimpleSprite GetIcon(uint quantity, uint meta) {
            return base.GetIcon(quantity, meta);
        }
    }
}
