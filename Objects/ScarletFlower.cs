using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevInterface;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Objects;

public class ScarletFlower : UpdatableAndDeletable, IDrawable
    {
        private PlacedObject pobj;
        private Vector2 rotation;
        public ScarletFlower(PlacedObject pobj, Vector2 rotation)
        {
            this.pobj = pobj;
            this.rotation = rotation;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            if (newContainer == null)
            {
                newContainer = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("ScarletFlowerStem", true);

            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(this.pobj.pos - camPos);
            sLeaser.sprites[0].rotation = Custom.VecToDeg(rotation);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.blackColor;
        }
    }

public class ScarletFlowerData : PlacedObject.Data
{
    public Vector2 rotation;

    public ScarletFlowerData(PlacedObject owner) : base(owner)
    {
        rotation = new Vector2(0f, 100f);
    }

    protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}", new object[]
        {
            rotation.x,
            rotation.y,
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        rotation.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        rotation.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
    }

    public override string ToString()
    {
        string text = this.BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class ScarletFlowerRepresentation : PlacedObjectRepresentation
{
    public ScarletFlowerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        subNodes.Add(new Handle(owner, "Rotation_Handle", this, new Vector2(40f, 40f)));
        (subNodes[subNodes.Count - 1] as Handle).pos = (pobj.data as ScarletFlowerData).rotation;
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        owner.placedObjectsContainer.AddChild(fSprites[1]);
    }
    public override void Refresh()
    {
        base.Refresh();
        MoveSprite(1, this.absPos);
        fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
        fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
        (pObj.data as ScarletFlowerData).rotation = (subNodes[0] as Handle).pos;
    }
}
