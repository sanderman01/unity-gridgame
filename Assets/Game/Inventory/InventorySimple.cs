// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AmarokGames.GridGame {

    public class InventorySimple : IInventory, IEnumerable<ItemStack> {

        private List<ItemStack> contents;

        public ItemStack this[int index] {
            get { return contents[index]; }
            set { contents[index] = value; }
        }

        public int Count { get { return contents.Count; } }

        public InventorySimple(int size) {
            contents = new List<ItemStack>(Enumerable.Repeat<ItemStack>(ItemStack.Empty, size));
        }
        
        public IInventorySlot GetSlot(int index) {
            InventorySlot slot = new InventorySlot(this, index);
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
