﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;
using RWCustom;
using HUD;
using ArchdruidsAdditions.Hooks;
using System.Drawing;

namespace ArchdruidsAdditions.Objects;

public static class Cursors
{
    public static FContainer container;
    public class PointCursor : HudPart
    {
        public Player player;
        public PlayerSpecificMultiplayerHud multihud;
        public Color arrowColor = new(1f, 1f, 1f);
        public Color shadowColor = new(0f, 0f, 0f);
        public Vector2 pos = new(0, 0);
        public Vector2 lastPos = new(0, 0);
        public Vector2 rotation = new(0, 1);
        public FSprite arrowSprite;
        public FSprite shadowSprite;
        public TriangleMesh triangle1Sprite;
        public TriangleMesh triangle2Sprite;
        public float visibility = 0;
        public float standStillCounter = 0;
        public float hue, sat, val;

        public PointCursor(HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud) : base(hud)
        {
            if (multiHud != null)
            { player = multiHud.RealizedPlayer; }
            else { player = hud.owner as Player; }

            arrowColor = new(1f, 1f, 1f);

            this.hud = hud;

            container = new FContainer();
            Futile.stage.AddChild(container);

            shadowSprite = new("Futile_White", true);
            shadowSprite.color = shadowColor;
            shadowSprite.shader = player.room.game.rainWorld.Shaders["FlatLight"];
            shadowSprite.scale = 4f;
            container.AddChild(shadowSprite);

            arrowSprite = new("Cursor", true);
            arrowSprite.color = arrowColor;
            arrowSprite.anchorX = 0f;
            arrowSprite.anchorY = 1f;
            container.AddChild(arrowSprite);

            TriangleMesh.Triangle[] array1 = [new TriangleMesh.Triangle(0, 1, 2)];
            triangle1Sprite = new TriangleMesh("Futile_White", array1, false, false);
            triangle1Sprite.color = arrowColor;
            container.AddChild(triangle1Sprite);

            TriangleMesh.Triangle[] array2 = [new TriangleMesh.Triangle(0, 1, 2)];
            triangle2Sprite = new TriangleMesh("Futile_White", array1, false, false);
            triangle2Sprite.color = arrowColor;
            container.AddChild(triangle2Sprite);
        }

        public override void Update()
        {
            base.Update();
            rotation = Custom.rotateVectorDeg(rotation, 5f);

            container.MoveToFront();

            pos = Futile.mousePosition;
            if (pos == lastPos)
            {
                if (standStillCounter < 100)
                { standStillCounter++; }
            }
            else
            { standStillCounter = 0; }
            lastPos = pos;
        }
        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            Bow bow = null;
            if (player.grasps[0] != null && player.grasps[0].grabbed is Bow)
            { bow = player.grasps[0].grabbed as Bow; }
            else if (player.grasps[1] != null && player.grasps[1].grabbed is Bow)
            { bow = player.grasps[1].grabbed as Bow; }

            if (player.room == null)
            {
                visibility = 0;
            }

            if (bow != null && bow.aiming)
            {
                Vector2 cursorPos = new(Futile.mousePosition.x, Futile.mousePosition.y);

                Vector2 mesh1Pos = cursorPos + rotation * 5f;
                triangle1Sprite.MoveVertice(0, mesh1Pos);
                triangle1Sprite.MoveVertice(1, mesh1Pos + Custom.rotateVectorDeg(rotation, 45) * 5f);
                triangle1Sprite.MoveVertice(2, mesh1Pos + Custom.rotateVectorDeg(rotation, -45) * 5f);

                Vector2 rotation2 = Custom.rotateVectorDeg(rotation, 180);
                Vector2 mesh2Pos = cursorPos + rotation2 * 5f;
                triangle2Sprite.MoveVertice(0, mesh2Pos);
                triangle2Sprite.MoveVertice(1, mesh2Pos + Custom.rotateVectorDeg(rotation2, 45) * 5f);
                triangle2Sprite.MoveVertice(2, mesh2Pos + Custom.rotateVectorDeg(rotation2, -45) * 5f);

                if (player.room != null && !player.room.game.GamePaused)
                { visibility = 1f; }
                else
                { visibility = 0f; }

                triangle1Sprite.alpha = visibility;
                triangle2Sprite.alpha = visibility;
                arrowSprite.alpha = 0;

                shadowSprite.SetPosition(cursorPos);
                shadowSprite.alpha = 0.1f;
            }
            else
            {
                Vector2 arrowPos = new(Futile.mousePosition.x + 0.01f, Futile.mousePosition.y + 0.01f);
                arrowSprite.SetPosition(arrowPos);

                Vector2 shadowPos = new(Futile.mousePosition.x + 3.01f, Futile.mousePosition.y - 8.01f);
                shadowSprite.SetPosition(shadowPos);

                float maxVisibility = 5f;
                if ((BeastmasterHooks.beastMasterMenuOpen || MainHooks.mouseDragActive)
                    && standStillCounter <= 50
                    && player.room != null && !player.room.game.GamePaused /*&& player.room.game.devUI == null*/)
                {
                    if (visibility < maxVisibility)
                    { visibility++; }

                    if (visibility > maxVisibility)
                    { visibility = maxVisibility; }
                }
                else if (visibility > 0)
                { visibility--; }

                if (visibility < 0)
                { visibility = 0; }

                float visibility2 = visibility / maxVisibility;
                arrowSprite.alpha = visibility2;
                shadowSprite.alpha = visibility2 * 0.1f;
                triangle1Sprite.alpha = 0;
                triangle2Sprite.alpha = 0;
            }
        }
        public override void ClearSprites()
        {
            base.ClearSprites();
            arrowSprite.RemoveFromContainer();
            shadowSprite.RemoveFromContainer();
        }
    }
}
