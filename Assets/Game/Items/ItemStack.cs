// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame.Items {

    public class ItemStack {

        public static readonly ItemStack Empty = new ItemStack(null, 1, 0);

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

        public Sprite Icon { get {
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
