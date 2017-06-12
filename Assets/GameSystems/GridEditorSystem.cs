// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame.Inventory;
using AmarokGames.GridGame.Items;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.GridGame {
    class GridEditorSystem : GameSystemBase, IGameSystem {

        private TileRegistry tileRegistry;
        private WorldManagementSystem worldMgr;
        private uint tileSelection = 1;

        private Player localPlayer;

        public static GridEditorSystem Create(TileRegistry tileRegistry, WorldManagementSystem worldMgr, Player localPlayer) {
            GridEditorSystem sys = GridEditorSystem.Create<GridEditorSystem>();
            sys.tileRegistry = tileRegistry;
            sys.worldMgr = worldMgr;
            sys.localPlayer = localPlayer;
            return sys;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            const int buttonLeft = 0;
            const int buttonRight = 1;
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if (Input.GetMouseButton(buttonLeft)) {
                worldMgr.PlaceTile(world, mouseWorldPos, 0, 0);
            } else if (Input.GetMouseButton(buttonRight)) {
                worldMgr.PlaceTile(world, mouseWorldPos, tileSelection, 0);
            }
        }

        void OnGUI() {
            float windowHeight = 80;
            Rect windowSize = new Rect(0, Screen.height - windowHeight, Screen.width * 0.5f, windowHeight);
            Vector2 center = windowSize.center;
            center.x = Screen.width * 0.5f;
            windowSize.center = center;
            GUI.Window(0, windowSize, OnGUIHotbar, "Hotbar");
        }

        private void OnGUIHotbar(int id) {
            Vector2 startOffset = new Vector2(10, 15);
            Vector2 iconSize = new Vector2(64, 64);
            float margin = 5;

            IInventory inv = localPlayer.HotbarInventory;
            for (int i = 1; i < inv.Count; i++) {
                Rect iconPosition = new Rect(startOffset, iconSize);
                iconPosition.x += (iconSize.x + margin) * (i - 1);
                ItemStack stack = inv[i];
                if(stack != null) {
                    SimpleSprite icon = stack.Icon;
                    bool click = IconButton(iconPosition, icon.texture, icon.uv);
                    if (click) {
                        Debug.Log("Selected tile: " + i);
                        tileSelection = (uint)i;
                    }

                    Rect labelPos = iconPosition;
                    labelPos.y -= 0;
                    GUI.Label(labelPos, stack.Item.HumanName);
                }
            }
        }

        private static bool IconButton(Rect position, Texture2D texture, Rect uvArea) {
            GUI.DrawTextureWithTexCoords(position, texture, uvArea);
            Event e = Event.current;
            return e.isMouse && e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 1 && position.Contains(e.mousePosition);
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }
    }
}
