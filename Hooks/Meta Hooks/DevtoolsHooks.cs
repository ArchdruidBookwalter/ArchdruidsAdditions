using DevInterface;

namespace ArchdruidsAdditions.Hooks;

public static class DevtoolsHooks
{
    internal static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type type, PlacedObject pobj)
    {
        if (type == PlacedObjectType.ScarletFlower)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new ScarletFlowerRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.Potato)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new PotatoRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.LightningFruit)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new LightningFruitRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.DecoLightningVine)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new DecoVineRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.AshPepperBush)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new AshPepperBushRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.InfectedCorpse)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new InfectedCorpseRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.CrabShellCircle)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new CrabShellCircleRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.DecoChain)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new ChainRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.ShrineBowl)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new ShrineBowlRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.BigChandelier)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new PlacedObjectRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else if (type == PlacedObjectType.LootCrate)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683, 384), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new LootCrateRep(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
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
        if (self.type == PlacedObjectType.ScarletFlower)
        {
            self.data = new ScarletFlowerData(self);
            return;
        }
        if (self.type == PlacedObjectType.Potato)
        {
            self.data = new PotatoData(self);
            return;
        }
        if (self.type == PlacedObjectType.LightningFruit)
        {
            self.data = new LightningFruitData(self);
            return;
        }
        if (self.type == PlacedObjectType.DecoLightningVine)
        {
            self.data = new DecoVineData(self);
            return;
        }
        if (self.type == PlacedObjectType.AshPepperBush)
        {
            self.data = new AshPepperBushData(self);
            return;
        }
        if (self.type == PlacedObjectType.InfectedCorpse)
        {
            self.data = new InfectedCorpseData(self);
            return;
        }
        if (self.type == PlacedObjectType.CrabShellCircle)
        {
            self.data = new CrabShellCircleData(self);
            return;
        }
        if (self.type == PlacedObjectType.DecoChain)
        {
            self.data = new ChainData(self);
            return;
        }
        if (self.type == PlacedObjectType.ShrineBowl)
        {
            self.data = new ShrineBowlData(self);
            return;
        }
        if (self.type == PlacedObjectType.BigChandelier)
        {
            self.data = new HangingPlatformData(self);
        }
        if (self.type == PlacedObjectType.LootCrate)
        {
            self.data = new LootCrateData(self);
        }
        orig(self);
    }
    internal static void Panel_CopyToClipboard(On.DevInterface.Panel.orig_CopyToClipboard orig, Panel self)
    {
        orig(self);
    }
    internal static void Panel_PasteFromClipboard(On.DevInterface.Panel.orig_PasteFromClipboard orig, Panel self)
    {
        orig(self);
        if (self is PotatoRepresentation.PotatoPanel panel)
        {
            try
            {
                PotatoRepresentation rep = panel.parentNode as PotatoRepresentation;
                PotatoData data = rep.pObj.data as PotatoData;
                data.FromString(GUIUtility.systemCopyBuffer);
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
    internal static string MapPage_CreatureVis_CritString(On.DevInterface.MapPage.CreatureVis.orig_CritString orig, AbstractCreature creature)
    {
        string baseCritString = orig(creature);

        if (creature.creatureTemplate.type == CreatureTemplateType.CloudFish)
        {
            return "h";
        }
        if (creature.creatureTemplate.type == CreatureTemplateType.Parasite)
        {
            return "p";
        }
        if (creature.creatureTemplate.type == CreatureTemplateType.MimicCrab)
        {
            return "c";
        }

        return baseCritString;
    }
    internal static Color MapPage_CreatureVis_CritCol(On.DevInterface.MapPage.CreatureVis.orig_CritCol orig, AbstractCreature creature)
    {
        Color baseCritColor = orig(creature);

        if (creature.creatureTemplate.type == CreatureTemplateType.CloudFish)
        {
            return Custom.HSL2RGB(0.52f, 1f, 0.5f);
        }
        if (creature.creatureTemplate.type == CreatureTemplateType.Parasite)
        {
            return Custom.HSL2RGB(0.2f, 1f, 0.5f);
        }
        if (creature.creatureTemplate.type == CreatureTemplateType.MimicCrab)
        {
            return Custom.HSL2RGB(0f, 1f, 0.5f);
        }

        return baseCritColor;
    }
    internal static void Handle_Update(On.DevInterface.Handle.orig_Update orig, Handle self)
    {
        orig(self);

        if (PlayerData.TextBeingInputted)
        {
            self.SetColor(self.defaultColor);
        }
    }

}
