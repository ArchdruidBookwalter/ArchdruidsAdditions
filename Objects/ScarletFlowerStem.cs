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
using System.Runtime.CompilerServices;

namespace ArchdruidsAdditions.Objects;

public class ScarletFlowerStem : UpdatableAndDeletable, IDrawable
{
    private PlacedObject pobj;
    private Vector2 rotation;

    public AbstractConsumable consumable;

    public ScarletFlowerStem(PlacedObject pobj, Vector2 rotation)
    {
        this.pobj = pobj;
        this.rotation = rotation;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
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

public class ScarletFlowerData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    public Vector2 rotation;
    new public int minRegen;
    new public int maxRegen;

    public ScarletFlowerData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        rotation = new Vector2(0f, 100f);
        minRegen = 2;
        maxRegen = 3;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", new object[]
        {
            panelPos.x,
            panelPos.y,
            minRegen,
            maxRegen,
            rotation.x,
            rotation.y,
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        rotation.x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
        rotation.y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
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
    public ScarletFlowerData data;
    public Handle rotationHandle;
    public ScarletFlowerPanel panel;

    
    public class ScarletFlowerPanel : Panel
    {
        public ScarletFlowerPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string title) :
            base(owner, IDstring, parentNode, pos, new(250f, 55f), title)
        {
            subNodes.Add(new ScarletFlowerSlider(owner, "Min_Regen_Slider", this, new Vector2(5f, 5f), "Min Cycles: "));
            subNodes.Add(new ScarletFlowerSlider(owner, "Max_Regen_Slider", this, new Vector2(5f, 25f), "Max Cycles: "));
        }
        public class ScarletFlowerSlider : Slider
        {
            public ScarletFlowerData data;
            public ScarletFlowerSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) :
                base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                data = (parentNode.parentNode as ScarletFlowerRepresentation).data;
            }
            public override void Refresh()
            {
                base.Refresh();
                float num = 0f;
                if (data.minRegen == 0)
                {
                    NumberText = "N/A";
                    num = 0f;
                }
                else
                {
                    if (IDstring != null)
                    {
                        if (IDstring == "Min_Regen_Slider")
                        {
                            NumberText = data.minRegen.ToString();
                            num = data.minRegen / 50f;
                        }
                        else if (IDstring == "Max_Regen_Slider")
                        {
                            NumberText = data.maxRegen.ToString();
                            num = data.maxRegen / 50f;
                        }
                    }
                }
                base.RefreshNubPos(num);
            }

            public override void NubDragged(float nubPos)
            {
                if (IDstring != null)
                {
                    if (IDstring == "Min_Regen_Slider")
                    {
                        data.minRegen = Math.Min((int)(nubPos * 50f), data.maxRegen);
                    }
                    else if (IDstring == "Max_Regen_Slider")
                    {
                        data.maxRegen = Math.Max((int)(nubPos * 50f), data.minRegen);
                    }
                }
                this.parentNode.parentNode.Refresh();
                this.Refresh();
            }
        }
    }
    
    public ScarletFlowerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as ScarletFlowerData;
        rotationHandle = new(owner, "Rotation_Handle", this, data.rotation);
        panel = new(owner, "ScarletFlower_Panel", this, data.panelPos, new(250f, 55f), "Consumable: Scarlet Flower");

        subNodes.Add(rotationHandle);
        subNodes.Add(panel);
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        owner.placedObjectsContainer.AddChild(fSprites[1]);
        owner.placedObjectsContainer.AddChild(fSprites[2]);
    }
    public override void Refresh()
    {
        base.Refresh();

        MoveSprite(1, absPos);
        fSprites[1].scaleY = rotationHandle.pos.magnitude;
        fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, rotationHandle.absPos);
        (pObj.data as ScarletFlowerData).rotation = rotationHandle.pos;

        
        MoveSprite(2, absPos);
        fSprites[2].scaleY = panel.pos.magnitude;
        fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, panel.absPos);
        (pObj.data as ScarletFlowerData).panelPos = panel.pos;
        
    }
}
