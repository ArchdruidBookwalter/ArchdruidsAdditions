using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Objects;

public class ColoredRectangle : UpdatableAndDeletable, IDrawable
{
    public Vector2 center;
    public float width;
    public float height;
    public float rotation;
    public Color color;
    public int life;
    public int maxLife;

    public ColoredRectangle(Room room, Vector2 center, float width, float height, float rotation, Color color, int maxLife)
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
            this.Destroy();
        }
    }


    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

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

        this.AddToContainer(sLeaser, rCam, null);
    }
}
