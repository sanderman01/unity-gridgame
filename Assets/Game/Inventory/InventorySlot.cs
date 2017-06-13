// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame.Items;

namespace AmarokGames.GridGame.Inventory {

    public class InventorySlot : IInventorySlot {
        private int index;
        private IInventory inventory;

        public InventorySlot(InventorySimple inventory, int index) {
            this.inventory = inventory;
            this.index = index;
        }

        public bool IsAllowedInSlot(ItemStack stack) {
            return true;
        }

        public bool CanTakeFromSlot { get { return true; } }

        public ItemStack GetStack() {
            return inventory[index];
        }

        public void PutStack(ItemStack stack) {
            if(GetStack() != ItemStack.Empty) {
                throw new System.Exception("Tried to use PutStack but there was already a stack in the slot!");
            } else if(!IsAllowedInSlot(stack)) {
                throw new System.Exception("Tried to use PutStack but the itemstack was not allowed in the slot!");
            } else {
                inventory[index] = stack;
            }
        }

        public ItemStack TakeStack() {
            if(!CanTakeFromSlot) {
                throw new System.Exception("Tried to use TakeStack but this slot does not allow taking from the slot!");
            }
            else {
                ItemStack stack = inventory[index];
                inventory[index] = ItemStack.Empty;
                return stack;
            }
        }

        public static void SwapStacks(IInventorySlot slotA, IInventorySlot slotB) {
            ItemStack stackA = slotA.TakeStack();
            ItemStack stackB = slotB.TakeStack();

            slotA.PutStack(stackB);
            slotB.PutStack(stackA);
        }
    }
}
