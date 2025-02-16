using System;
using BepInEx;
using DevInterface;
using UnityEngine;

namespace ArchdruidsAdditions
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "archdruidbookwalter.archdruidsadditions";
        public const string PLUGIN_NAME = "ArchdruidsAdditions";
        public const string PLUGIN_VERSION = "1.0.0";

        public static class PlacedObjectType
        {
            public static void UnregisterValues()
            {
                if (PlacedObjectType.ScarletFlowerStem != null)
                {
                    PlacedObjectType.ScarletFlowerStem.Unregister();
                    PlacedObjectType.ScarletFlowerStem = null;
                }
            }
            public static PlacedObject.Type ScarletFlowerStem = new("ScarletFlowerStem", true);
        }

        public void OnEnable()
        {
            On.Room.Loaded += Room_Loaded;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.UnloadResources += RainWorld_UnloadResources;
        }

        public void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (!Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
            {
                Futile.atlasManager.LoadAtlas("atlases/ScarletFlowerStem");
            }
        }

        public void RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
            {
                Futile.atlasManager.UnloadAtlas("ScarletFlowerStem");
            }
        }

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game == null) return;
            foreach (var pobj in self.roomSettings.placedObjects)
            {
                if (pobj.active && pobj.type == PlacedObjectType.ScarletFlowerStem)
                {
                    self.AddObject(new ScarletFlowerStem(pobj));
                }
            }
        }

        class ScarletFlowerStem : UpdatableAndDeletable, IDrawable
        {
            private PlacedObject pobj;
            public ScarletFlowerStem(PlacedObject pobj)
            {
                this.pobj = pobj;
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
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = palette.blackColor;
            }
        }
    }
}