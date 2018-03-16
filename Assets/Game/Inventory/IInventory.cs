// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame {

    public interface IInventory {

         ItemStack this[int index] { get; set; }

        int Count { get; }

        IInventorySlot GetSlot(int index);
    }
}
