using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

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

        public Rectangle(Room room, Vector2 center, float width, float height, float rotation, string color, int maxLife)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            this.rotation = rotation;
            this.room = room;
            this.maxLife = maxLife;
            this.life = 0;

            if (color == "Red")
            {
                this.color = new(1f, 0f, 0f);
            }
            else if (color == "Orange")
            {
                this.color = new(1f, 0.5f, 0f);
            }
            else if (color == "Green")
            {
                this.color = new(0f, 1f, 0f);
            }
            else if (color == "Blue")
            {
                this.color = new(0f, 0f, 1f);
            }
            else if (color == "Yellow")
            {
                this.color = new(1f, 1f, 0f);
            }
            else if (color == "Purple")
            {
                this.color = new(1f, 0f, 1f);
            }
            else if (color == "Cyan")
            {
                this.color = new(0f, 1f, 1f);
            }
            else if (color == "White")
            {
                this.color = new(1f, 1f, 1f);
            }
            else if (color == "Black")
            {
                this.color = new(0.1f, 0.1f, 0.1f);
            }
            else
            {
                this.color = Custom.HSL2RGB(Random.Range(0f, 1f), 1f, 1f);
            }
        }

        public Rectangle(Room room, Vector2 center, float width, float height, float rotation, Color color, int maxLife)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            this.rotation = rotation;
            this.room = room;
            this.maxLife = maxLife;
            this.life = 0;
            this.color = color;
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
    public class SmallRectangle : UpdatableAndDeletable, IDrawable
    {
        public Vector2 center;
        public float width;
        public float height;
        public float rotation;
        public Color color;
        public int life;
        public int maxLife;

        public SmallRectangle(Room room, Vector2 center, string color, int maxLife)
        {
            this.center = center;
            this.width = 5f;
            this.height = 5f;
            this.rotation = 45f;
            this.color = new(1f, 1f, 0f);
            this.room = room;
            this.maxLife = maxLife;
            this.life = 0;

            if (color == "Red")
            {
                this.color = new(1f, 0f, 0f);
            }
            else if (color == "Green")
            {
                this.color = new(0f, 1f, 0f);
            }
            else if (color == "Blue")
            {
                this.color = new(0f, 0f, 1f);
            }
            else if (color == "Yellow")
            {
                this.color = new(1f, 1f, 0f);
            }
            else if (color == "Purple")
            {
                this.color = new(1f, 0f, 1f);
            }
            else if (color == "White")
            {
                this.color = new(1f, 1f, 1f);
            }
            else if (color == "Black")
            {
                this.color = new(0.1f, 0.1f, 0.1f);
            }
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

        public Color color2;

        public Text(Room room, Vector2 pos, String text, string color, string color2, int maxLife)
        {
            Player player = room.game.FirstRealizedPlayer;
            if (player.room == null)
            {
                return;
            }

            this.room = room;
            this.pos = pos;
            this.maxLife = maxLife;

            label = new FLabel(Custom.GetFont(), "TEST");
            label.alignment = FLabelAlignment.Center;
            label.text = text;

            if (color == "Red")
            {
                this.color = new(1f, 0f, 0f);
            }
            else if (color == "Green")
            {
                this.color = new(0f, 1f, 0f);
            }
            else if (color == "Blue")
            {
                this.color = new(0f, 0f, 1f);
            }
            else if (color == "Yellow")
            {
                this.color = new(1f, 1f, 0f);
            }
            else if (color == "Purple")
            {
                this.color = new(1f, 0f, 1f);
            }
            else if (color == "Cyan")
            {
                this.color = new(0f, 1f, 1f);
            }
            else if (color == "White")
            {
                this.color = new(1f, 1f, 1f);
            }
            else if (color == "Black")
            {
                this.color = new(0.1f, 0.1f, 0.1f);
            }

            if (color == "Red")
            {
                this.color2 = new(1f, 0f, 0f);
            }
            else if (color == "White")
            {
                this.color2 = new(1f, 1f, 1f);
            }
        }
        public Text(Room room, Vector2 pos, String text, Color color, string color2, int maxLife)
        {
            Player player = room.game.FirstRealizedPlayer;
            if (player.room == null)
            {
                return;
            }

            this.room = room;
            this.pos = pos;
            this.maxLife = maxLife;

            label = new FLabel(Custom.GetFont(), "TEST");
            label.alignment = FLabelAlignment.Center;
            label.text = text;
            this.color = color;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lifeTime++;

            if (lifeTime > maxLife || !room.BeingViewed)
            {
                Destroy();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true);

            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            //Debug.Log("DrawSprites       Method was called!");
            label.color = color;
            label.SetPosition(pos - camPos);

            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[0].scale = 10f;
            sLeaser.sprites[0].alpha = 0;

            if (slatedForDeletetion || room != rCam.room)
            {
                label.RemoveFromContainer();
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            //Debug.Log("ApplyPalette      Method was called!");
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            //Debug.Log("AddToContainer    Method was called!");

            newContainer ??= rCam.ReturnFContainer("HUD");

            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContainer.AddChild(sprite);
            }

            label.RemoveFromContainer();
            newContainer.AddChild(label);
        }
    }
    public class LineAndDot : UpdatableAndDeletable, IDrawable
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public Color color;
        public int life;
        public int maxLife;
        public LineAndDot(Room room, Vector2 startPos, Vector2 endPos, string color, int maxLife)
        {
            this.room = room;
            this.startPos = startPos;
            this.endPos = endPos;
            this.maxLife = maxLife;
            this.life = 0;

            if (color == "Red")
            {
                this.color = new(1f, 0f, 0f);
            }
            else if (color == "Orange")
            {
                this.color = new(1f, 0.5f, 0f);
            }
            else if (color == "Green")
            {
                this.color = new(0f, 1f, 0f);
            }
            else if (color == "Blue")
            {
                this.color = new(0f, 0f, 1f);
            }
            else if (color == "Yellow")
            {
                this.color = new(1f, 1f, 0f);
            }
            else if (color == "Purple")
            {
                this.color = new(1f, 0f, 1f);
            }
            else if (color == "Cyan")
            {
                this.color = new(0f, 1f, 1f);
            }
            else if (color == "White")
            {
                this.color = new(1f, 1f, 1f);
            }
            else if (color == "Black")
            {
                this.color = new(0.1f, 0.1f, 0.1f);
            }
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
            Vector2 middlePos = Vector2.Lerp(startPos, endPos, 0.5f);
            float angle = Custom.VecToDeg(Custom.DirVec(startPos, endPos));
            float length = Custom.Dist(startPos, endPos);

            sLeaser.sprites[0].SetPosition(middlePos - camPos);
            sLeaser.sprites[0].rotation = angle;
            sLeaser.sprites[0].scaleY = length;
            sLeaser.sprites[0].color = color;

            sLeaser.sprites[1].SetPosition(endPos - camPos);
            sLeaser.sprites[1].scale = 0.5f;
            sLeaser.sprites[1].color = color;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[1] = new FSprite("Circle20", true);

            AddToContainer(sLeaser, rCam, null);
        }
    }
    public class Dot : UpdatableAndDeletable, IDrawable
    {
        public Vector2 pos;
        public Color color;
        public int life;
        public int maxLife;
        public Dot(Room room, Vector2 pos, string color, int maxLife)
        {
            this.room = room;
            this.pos = pos;
            this.maxLife = maxLife;
            this.life = 0;

            if (color == "Red")
            {
                this.color = new(1f, 0f, 0f);
            }
            else if (color == "Orange")
            {
                this.color = new(1f, 0.5f, 0f);
            }
            else if (color == "Green")
            {
                this.color = new(0f, 1f, 0f);
            }
            else if (color == "Blue")
            {
                this.color = new(0f, 0f, 1f);
            }
            else if (color == "Yellow")
            {
                this.color = new(1f, 1f, 0f);
            }
            else if (color == "Purple")
            {
                this.color = new(1f, 0f, 1f);
            }
            else if (color == "Cyan")
            {
                this.color = new(0f, 1f, 1f);
            }
            else if (color == "White")
            {
                this.color = new(1f, 1f, 1f);
            }
            else if (color == "Black")
            {
                this.color = new(0.1f, 0.1f, 0.1f);
            }
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
            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[0].scale = 0.5f;
            sLeaser.sprites[0].color = color;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("Circle20", true);

            AddToContainer(sLeaser, rCam, null);
        }
    }
}
