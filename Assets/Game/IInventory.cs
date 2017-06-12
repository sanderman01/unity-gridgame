// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame.Items;
using System.Collections.Generic;

namespace AmarokGames.GridGame {

    public interface IInventory {

         ItemStack this[int index] { get; set; }

        int Count { get; }

        IItemSlot GetSlot(int index);
    }
}
