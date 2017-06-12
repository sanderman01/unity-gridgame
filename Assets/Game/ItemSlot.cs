// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame {

    public class ItemSlotGeneric : IItemSlot {
        private int index;
        private IInventory inventory;

        public ItemSlotGeneric(InventoryGeneric inventory, int index) {
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
            if(GetStack() != null) {
                throw new System.Exception("Tried to use PutStack but there was already a stack in the slot!");
            } else if(!IsAllowedInSlot(stack)) {
                throw new System.Exception("Tried to use PutStack but the itemstack was not allowed in the slot!");
            } else {
                inventory[index] = stack;
            }
        }

        public ItemStack TakeStack() {
            if (GetStack() == null) {
                throw new System.Exception("Tried to use TakeStack but there was no stack in the slot!");
            } else if(CanTakeFromSlot) {
                throw new System.Exception("Tried to use TakeStack but this slot does not allow taking from the slot!");
            }
            else {
                return inventory[index];
            }
        }
    }
}
