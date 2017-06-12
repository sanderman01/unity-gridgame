// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame.Items;
using System;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerInventoryUISystem : GameSystemBase {

        private Player localPlayer;

        public static PlayerInventoryUISystem Create(Player localPlayer) {
            PlayerInventoryUISystem sys = GameSystemBase.Create<PlayerInventoryUISystem>();
            sys.localPlayer = localPlayer;
            return sys;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }

        const int ncolumns = 10;
        const int nrows = 3;
        const float margin = 5;
        Vector2 iconSize = new Vector2(64, 64);
        Vector2 offset = new Vector2(margin, 30);

        Rect inventoryRegion;
        Rect hotbarRegion;

        void OnGUI() {
            // Main inventory window
            {
                float windowWidth = offset.x + ncolumns * (iconSize.x + margin);
                float windowHeight = offset.y + nrows * (iconSize.y + margin);
                inventoryRegion = new Rect(0, 0, windowWidth, windowHeight);
                Vector2 center = inventoryRegion.center;
                center = 0.5f * new Vector2(Screen.width, Screen.height);
                inventoryRegion.center = center;
                GUI.BeginGroup(inventoryRegion, GUI.skin.box);
                OnInventoryGUI(inventoryRegion);
                GUI.EndGroup();
            }
            // Hotbar
            // Main inventory window
            {
                float windowWidth = offset.x + ncolumns * (iconSize.x + margin);
                float windowHeight = offset.y + 1 * (iconSize.y + margin);
                hotbarRegion = new Rect(0, 0, windowWidth, windowHeight);
                Vector2 center = hotbarRegion.center;
                center = 0.5f * new Vector2(Screen.width, Screen.height);
                hotbarRegion.center = center;
                hotbarRegion.y = Screen.height - hotbarRegion.height;
                GUI.BeginGroup(hotbarRegion, GUI.skin.box);
                OnHotbarGUI(hotbarRegion);
                GUI.EndGroup();
            }
            {
                IItemSlot mouseSlot = localPlayer.MouseHeldInventory.GetSlot(0);
                ItemStack stack = mouseSlot.GetStack();
                if (stack != null) {
                    Vector2 size = new Vector2(64, 64);
                    Vector2 mousePos = Event.current.mousePosition;
                    Rect pos = new Rect(mousePos, size);
                    DrawItemStack(pos, stack);
                }
            }

            {
                // Throwing stacks
                Vector2 mousePos = Event.current.mousePosition;
                if (Event.current.clickCount == 1 && !(inventoryRegion.Contains(mousePos) || hotbarRegion.Contains(mousePos))) {
                    IItemSlot mouseSlot = localPlayer.MouseHeldInventory.GetSlot(0);
                    ItemStack stack = mouseSlot.GetStack();
                    if (stack != null) {
                        // We can't throw items into the world yet. But we can remove it from the slot.
                        mouseSlot.TakeStack();
                    }
                }
            }
        }

        private void OnInventoryGUI(Rect containerPos) {
            IInventory inv = localPlayer.MainInventory;
            for (int i = 0; i < inv.Count; i++) {
                Rect iconPosition = new Rect(offset, iconSize);
                int col = (i % ncolumns);
                int row = i / ncolumns;
                iconPosition.x = offset.x + col * (iconSize.x + margin);
                iconPosition.y = offset.y + row * (iconSize.y + margin);
                GUI.Box(iconPosition, "");
                ItemStack stack = inv[i];
                if (stack != null) {

                    bool click = ItemStackButton(iconPosition, stack);
                    if (click) {
                        IItemSlot mouseSlot = localPlayer.MouseHeldInventory.GetSlot(0);
                        ItemSlotGeneric.SwapStacks(mouseSlot, inv.GetSlot(i));
                    }
                }
            }
        }

        private void OnHotbarGUI(Rect containerPos) {
            IInventory inv = localPlayer.HotbarInventory;
            for (int i = 0; i < inv.Count; i++) {
                Rect iconPosition = new Rect(offset, iconSize);
                int col = (i % ncolumns);
                int row = i / ncolumns;
                iconPosition.x = offset.x + col * (iconSize.x + margin);
                iconPosition.y = offset.y + row * (iconSize.y + margin);
                GUI.Box(iconPosition, "");
                ItemStack stack = inv[i];
                if (stack != null) {

                    bool click = ItemStackButton(iconPosition, stack);
                    if (click) {
                        IItemSlot mouseSlot = localPlayer.MouseHeldInventory.GetSlot(0);
                        ItemSlotGeneric.SwapStacks(mouseSlot, inv.GetSlot(i));
                    }
                }
            }
        }



        private static bool ItemStackButton(Rect position, ItemStack stack) {
            DrawItemStack(position, stack);
            Event e = Event.current;
            return e.isMouse && e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 1 && position.Contains(e.mousePosition);
        }

        private static void DrawItemStack(Rect position, ItemStack stack) {
            GUI.DrawTextureWithTexCoords(position, stack.Icon.texture, stack.Icon.uv);

            Rect labelPos = position;
            GUI.Label(labelPos, stack.QuantityString);
        }
    }
}
