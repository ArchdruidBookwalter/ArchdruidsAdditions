using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Color = UnityEngine.Color;

namespace ArchdruidsAdditions.Objects;

public class ColoredShapes
{
    public class Rectangle : UpdatableAndDeletable, IDrawable
    {
        public Vector2 center;
        public float width;
        public float height;
        public float rotation;
        public Color color;
        public int life;
        public int maxLife;

        public Rectangle(Room room, Vector2 center, float width, float height, float rotation, Color color, int maxLife)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            this.rotation = rotation;
            this.color = color;
            this.room = room;
            this.maxLife = maxLife;
            this.life = 0;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            life++;
            if (life > maxLife)
            {
                Destroy();
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("HUD");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(center - camPos + Custom.rotateVectorDeg(new Vector2(0f, height / 2), rotation));
            sLeaser.sprites[0].scaleX = width;
            sLeaser.sprites[0].scaleY = 1f;

            sLeaser.sprites[1].SetPosition(center - camPos + Custom.rotateVectorDeg(new Vector2(0f, -height / 2), rotation));
            sLeaser.sprites[1].scaleX = width;
            sLeaser.sprites[1].scaleY = 1f;

            sLeaser.sprites[2].SetPosition(center - camPos + Custom.rotateVectorDeg(new Vector2(width / 2, 0f), rotation));
            sLeaser.sprites[2].scaleX = 1f;
            sLeaser.sprites[2].scaleY = height;

            sLeaser.sprites[3].SetPosition(center - camPos + Custom.rotateVectorDeg(new Vector2(-width / 2, 0f), rotation));
            sLeaser.sprites[3].scaleX = 1f;
            sLeaser.sprites[3].scaleY = height;

            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.color = color;
                sprite.rotation = rotation;
                if (room != null && room.game.devToolsActive)
                { sprite.alpha = 1; }
                else { sprite.alpha = 0; }
            }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];

            for (int i = 0; i < 4; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
            }

            AddToContainer(sLeaser, rCam, null);
        }
    }
    public class Triangle : UpdatableAndDeletable, IDrawable
    {
        TriangleMesh mesh;
        public Vector2 vertex1;
        public Vector2 vertex2;
        public Vector2 vertex3;
        public Color color;
        float alpha;
        public int life;
        public int maxLife;
        public Triangle(Room room, Vector2 vertex1, Vector2 vertex2, Vector2 vertex3, Color color, float alpha, int maxLife)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertex3 = vertex3;
            this.color = color;
            this.alpha = alpha;
            this.room = room;
            this.maxLife = maxLife;
            life = 0;
        }
        int countInt = 0;
        int shaderIndex = 0;
        public override void Update(bool eu)
        {
            base.Update(eu);
            life++;
            if (life > maxLife)
            {
                Destroy();
            }
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Water");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            mesh = sLeaser.sprites[0] as TriangleMesh;
            mesh.MoveVertice(0, vertex1);
            mesh.MoveVertice(1, vertex2);
            mesh.MoveVertice(2, vertex3);
            mesh.color = color;

            if (room != null && room.game.devToolsActive)
            { mesh.alpha = 1; }
            else { mesh.alpha = 0; }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            TriangleMesh.Triangle[] array = [new TriangleMesh.Triangle(0, 1, 2)];
            sLeaser.sprites[0] = new TriangleMesh("Futile_White", array, false, false);
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];

            AddToContainer(sLeaser, rCam, null);
        }
    }
    public class Text : UpdatableAndDeletable, IDrawable
    {
        public Vector2 pos;
        public string text;
        public Color color;
        public int maxLife;

        public FLabel label;
        public int lifeTime = 0;

        public Text(Room room, Vector2 pos, String text, Color color, int maxLife)
        {
            this.room = room;
            this.pos = pos;
            this.maxLife = maxLife;

            label = new FLabel(Custom.GetFont(), "TEST");
            label.alignment = FLabelAlignment.Center;
            label.text = text;
            label.color = color;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            lifeTime++;
            if (lifeTime > maxLife)
            {
                Destroy();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            label.SetPosition(pos - camPos);

            if (room != null && room.game.devToolsActive)
            { label.alpha = 1; }
            else { label.alpha = 0; }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
                label.RemoveFromContainer();
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("HUD");
            newContainer.AddChild(label);
        }
    }
}
