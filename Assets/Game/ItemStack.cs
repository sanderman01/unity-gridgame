// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public class ItemStack {

        public Item Item { get; set; }

        private uint quantity;
        public uint Quantity {
            get { return quantity; }
            set {
                quantity = value;
                QuantityString = quantity.ToString();
            }
        }

        public string QuantityString { get; private set; }

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
