// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using System.Collections;
using System.Collections.Generic;

namespace AmarokGames.GridGame {

    public class InventoryGeneric : IInventory, IEnumerable<ItemStack> {

        private List<ItemStack> contents;

        public ItemStack this[int index] {
            get { return contents[index]; }
            set { contents[index] = value; }
        }

        public int Count { get { return contents.Count; } }

        public InventoryGeneric(int size) {
            contents = new List<ItemStack>(new ItemStack[size]);
        }
        
        public IItemSlot GetSlot(int index) {
            ItemSlotGeneric slot = new ItemSlotGeneric(this, index);
            return slot;
        }

        IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() {
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return contents.GetEnumerator();
        }
    }
}
