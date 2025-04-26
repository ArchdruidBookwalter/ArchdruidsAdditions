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

public class ScarletFlowerRepresentation : ConsumableRepresentation
{
    public ScarletFlowerData data;
    public Handle rotationHandle;
    //public ConsumableControlPanel controlPanel;
    
    public ScarletFlowerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as ScarletFlowerData;
        rotationHandle = new(owner, "Rotation_Handle", this, data.rotation);
        controlPanel = new(owner, "ScarletFlower_Panel", this, data.panelPos, "Consumable: Scarlet Flower");

        subNodes[0].ClearSprites();
        subNodes.RemoveAt(0);

        subNodes.Add(rotationHandle);
        subNodes.Add(controlPanel);
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
        fSprites[2].scaleY = controlPanel.pos.magnitude;
        fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as ScarletFlowerData).panelPos = controlPanel.pos;

        /*
        UnityEngine.Debug.Log("");
        for (int i = 0; i < subNodes.Count; i++)
        {
            UnityEngine.Debug.Log(i + ": " + subNodes[i]);
        }
        UnityEngine.Debug.Log("");
        */
    }
}
