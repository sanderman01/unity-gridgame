﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame.Items {

    public class ItemTool : Item {

        public virtual void OnGUIRenderHeldTool(Player player, ItemStack stack) {

            Vector2 playerPos = player.Character.transform.position;
            Vector2 originPos = new Vector2(playerPos.x, playerPos.y + 2);
            Vector2 screenPos = Camera.main.WorldToScreenPoint(originPos);
            screenPos.y = Screen.height - screenPos.y;

            Rect position = new Rect(0, -50, 50, 50);
            GUI.Box(position, "");
            GUI.DrawTextureWithTexCoords(position, IconTexture, IconUV[0]);

            Matrix4x4 m = Matrix4x4.TRS(screenPos, Quaternion.identity, Vector3.one);
            Matrix4x4 oldM = GUI.matrix;
            GUI.matrix = m;
            GUI.DrawTextureWithTexCoords(position, IconTexture, IconUV[0]);
            GUI.matrix = oldM;
        }

        public virtual void MouseDown(Player player, int mouseButton, Vector2 screenPos, Vector2 worldPos) {}
        public virtual void MouseUp(Player player, int mouseButton, Vector2 screenPos, Vector2 worldPos) {}
    }
}