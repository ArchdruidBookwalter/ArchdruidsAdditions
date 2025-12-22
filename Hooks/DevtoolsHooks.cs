using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using ArchdruidsAdditions.Objects;
using ArchdruidsAdditions.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class DevtoolsHooks
{
    internal static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type type, PlacedObject pobj)
    {
        if (type == Enums.PlacedObjectType.ScarletFlower)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new Objects.ScarletFlowerRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == Enums.PlacedObjectType.Potato)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new Objects.PotatoRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else
        {
            orig(self, type, pobj);
        }
    }
    internal static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
    {
        if (self.type == Enums.PlacedObjectType.ScarletFlower)
        {
            self.data = new Objects.ScarletFlowerData(self);
        }
        if (self.type == Enums.PlacedObjectType.Potato)
        {
            self.data = new Objects.PotatoData(self);
        }
        else
        {
            orig(self);
        }
    }
    internal static void Panel_CopyToClipboard(On.DevInterface.Panel.orig_CopyToClipboard orig, Panel self)
    {
        orig(self);
    }
    internal static void Panel_PasteFromClipboard(On.DevInterface.Panel.orig_PasteFromClipboard orig, Panel self)
    {
        orig(self);
        if (self is PotatoRepresentation.PotatoControlPanel panel)
        {
            try
            {
                PotatoRepresentation rep = panel.parentNode as PotatoRepresentation;
                PotatoData data = rep.pObj.data as PotatoData;
                data.FromString(GUIUtility.systemCopyBuffer);
                Debug.Log(GUIUtility.systemCopyBuffer);
                Debug.Log(data.minRegen);
                Debug.Log(data.maxRegen);
                foreach (DevUINode node in panel.subNodes)
                {
                    if (node is ConsumableRepresentation.ConsumableControlPanel.ConsumableSlider slider)
                    {
                        ((slider.parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen = data.minRegen;
                        ((slider.parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen = data.maxRegen;
                        slider.Refresh();
                    }
                }
            }
            catch
            {
                try
                {
                    PlacedObject.ConsumableObjectData data = new((panel.parentNode as ConsumableRepresentation).pObj);
                    data.FromString(GUIUtility.systemCopyBuffer);
                    Debug.Log(GUIUtility.systemCopyBuffer);
                    Debug.Log(data.minRegen);
                    Debug.Log(data.maxRegen);
                    foreach (DevUINode node in panel.subNodes)
                    {
                        if (node is ConsumableRepresentation.ConsumableControlPanel.ConsumableSlider slider)
                        {
                            ((slider.parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen = data.minRegen;
                            ((slider.parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen = data.maxRegen;
                            slider.Refresh();
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
    internal static string DevInterface_MapPage_CreatureVis_CritString(On.DevInterface.MapPage.CreatureVis.orig_CritString orig, AbstractCreature creature)
    {
        if (creature.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            return "H";
        }
        return orig(creature);
    }
    internal static Color DevInterface_MapPage_CreatureVis_CritCol(On.DevInterface.MapPage.CreatureVis.orig_CritCol orig, AbstractCreature creature)
    {
        if (creature.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            CloudFishAbstractAI AI = creature.abstractAI as CloudFishAbstractAI;
            if (AI.behavior == CloudFishAI.Behavior.Flee)
            {
                return new(0.8f, 0.2f, 0.2f);
            }
            return AI.flock.color;
        }
        return orig(creature);
    }

}
