﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame {

    public interface IItemSlot {

        bool IsAllowedInSlot(ItemStack stack);
        bool CanTakeFromSlot { get; }

        ItemStack GetStack();

        void PutStack(ItemStack stack);

        ItemStack TakeStack();

    }
}
