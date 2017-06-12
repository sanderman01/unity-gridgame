// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public class ItemStack {

        public Item Item { get; set; }
        public uint Quantity { get; set; }
        public uint Meta { get; set; }
        ///public TagData Tags { get; set; }

        public SimpleSprite Icon { get {
                return Item.GetIcon(Quantity, Meta);
            }
        }
        
        public ItemStack(Item item, uint quantity, uint meta) {
            this.Item = item;
            this.Quantity = quantity;
            this.Meta = meta;
        } 
    }
}
