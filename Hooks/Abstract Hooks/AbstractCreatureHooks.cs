using System;
using System.Collections.Generic;
using System.Linq;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractCreatureHooks
{
    internal static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
    {
        //Debug.Log("");
        //Debug.Log("CREATURE \'" + self.creatureTemplate.values.ToString() + "\' TRIED TO REALIZE IN ROOM");

        float section = 0;

        try
        {
            section = 1;

            if (self.Room != null && self.realizedCreature == null)
            {
                bool AAcreature = false;
                if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
                {
                    self.realizedCreature = new CloudFish(self, self.world);
                    AAcreature = true;
                }
                else if (self.creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
                {
                    self.realizedCreature = new Parasite(self, self.world);
                    AAcreature = true;
                }
                else if (self.creatureTemplate.type == Enums.CreatureTemplateType.MimicCrab)
                {
                    self.realizedCreature = new MimicCrab(self, self.world);
                    AAcreature = true;
                }

                if (AAcreature)
                {
                    self.InitiateAI();

                    foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.stuckObjects)
                    {
                        if (stick.A.realizedObject == null) { stick.A.Realize(); }
                        if (stick.B.realizedObject == null) { stick.B.Realize(); }
                    }
                }
            }

            section = 2;

            orig(self);

            if (self.realizedCreature != null && self.realizedCreature is Player player)
            {
                Debug.Log("ABSTRACT PLAYER WAS REALIZED AT LOCATION: " + self.Room.name + " - " + player.coord.Tile.ToString());
            }

            section = 4;

            if (self.realizedCreature != null && self.Room.realizedRoom != null)
            {
                Room room = self.Room.realizedRoom;

                bool rotten = false;
                if (self.unrecognizedAttributes != null && self.unrecognizedAttributes.Length > 0)
                {
                    foreach (string unrecognizedString in self.unrecognizedAttributes)
                    {
                        if (unrecognizedString == "INFECTED")
                        {
                            rotten = true;
                        }
                    }
                }

                section = 4.1f;

                List<AbstractPhysicalObject> eggs = [];
                foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.stuckObjects)
                {
                    if (stick is AbstractParasiteEggStick eggStick)
                    {
                        eggs.Add(eggStick.Egg);
                    }
                }

                section = 4.2f;

                if (rotten || eggs.Count > 0)
                {
                    InfectedCorpse corpse = new(self, eggs, false);
                    room.AddObject(corpse);

                    section = 4.21f;

                    foreach (AbstractPhysicalObject egg in eggs)
                    {
                        if (egg.realizedObject != null)
                        {
                            (egg.realizedObject as ParasiteEgg).StickInCorpse(corpse);
                        }
                    }

                    section = 4.22f;

                    if (!rotten)
                    {
                        //Debug.Log("CREATED UNRECOGNIZED ATTRIBUTE");

                        if (self.unrecognizedAttributes == null)
                        {
                            self.unrecognizedAttributes = new string[1];
                            self.unrecognizedAttributes[0] = "INFECTED";
                        }
                        else
                        {
                            List<string> strings = [];
                            foreach (string attribute in self.unrecognizedAttributes)
                            {
                                strings.Add(attribute);
                            }
                            strings.Add("INFECTED");

                            self.unrecognizedAttributes = [.. strings];
                        }
                    }

                    section = 4.23f;

                    self.realizedCreature.Die();
                }

                section = 4.3f;

                /*
                if (self.realizedCreature is MimicCrab crab && self.abstractAI is MimicCrabAbstractAI AI && AI.hidden)
                {
                    section = 4.31f;

                    IntVector2 startPos = self.realizedCreature.coord.Tile;
                    for (int i = startPos.y; i > 0; i--)
                    {
                        section = 4.32f;

                        IntVector2 testPos = new(startPos.x, i);
                        if (room.GetTile(testPos.x, testPos.y - 1).Solid || (room.terrain != null && room.terrain.ObstructsTile(testPos.x, testPos.y - 1)))
                        {
                            section = 4.33f;

                            crab.ForceHidden(room.GetWorldCoordinate(testPos));
                            break;
                        }
                    }
                }*/
            }

        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "ABSTRACTCREATURE_REALIZE", section);
        }

        //Methods.Methods.LogMethodEnd();
    }
    internal static void AbstractCreature_Abstractize(On.AbstractCreature.orig_Abstractize orig, AbstractCreature self, WorldCoordinate coord)
    {
        /*
        foreach (UpdatableAndDeletable updel in self.Room.realizedRoom.updateList)
        {
            if (updel is InfectedCorpse corpse && corpse.deadCreature == self)
            {
                self.unrecognizedAttributes ??= [];

                self.unrecognizedAttributes.Append("INFECTED");
            }
        }
        */

        orig(self, coord);
    }
    internal static void AbstractCreature_InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
    {
        if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            CloudFishAI newAI = new(self, self.world);
            self.abstractAI.RealAI = newAI;
        }
        else if (self.creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
        {
            ParasiteAI newAI = new(self, self.world);
            self.abstractAI.RealAI = newAI;
        }
        else if (self.creatureTemplate.type == Enums.CreatureTemplateType.MimicCrab)
        {
            MimicCrabAI newAI = new(self, self.world);
            self.abstractAI.RealAI = newAI;
        }
        orig(self);
    }
    internal static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            self.abstractAI = new CloudFishAbstractAI(world, self);
        }
        else if (creatureTemplate.type == Enums.CreatureTemplateType.MimicCrab)
        {
            self.abstractAI = new MimicCrabAbstractAI(world, self);
        }
        else if (creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
        {
            ParasiteState newState = new(self);
            self.state = newState;
        }
    }
    internal static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);
    }
    internal static void AbstractCreature_InDenUpdate(On.AbstractCreature.orig_InDenUpdate orig, AbstractCreature self, int time)
    {
        orig(self, time);
    }
    internal static void AbstractCreature_ChangeRooms(On.AbstractCreature.orig_ChangeRooms orig, AbstractCreature self, WorldCoordinate newCoord)
    {
        orig(self, newCoord);

        /*
        Data.PlayerData.AAPlayerState playerState = Data.PlayerData.GetPlayerStateFromAbstractCreature(self);
        if (playerState != null && playerState.parasiteIllnessEffect != null)
        {
            AbstractRoom newRoom = self.world.GetAbstractRoom(newCoord);
            if (newRoom != null && newRoom.realizedRoom != null)
            {
                playerState.parasiteIllnessEffect.NewRoom(newRoom.realizedRoom);
            }
        }*/
    }
    internal static void AbstractCreature_DropCarriedObject(On.AbstractCreature.orig_DropCarriedObject orig, AbstractCreature self, int graspIndex)
    {
        orig(self, graspIndex);
    }
    internal static void AbstractCreature_SetCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
    {
        orig(self);

        if (self.creatureTemplate.type == Enums.CreatureTemplateType.MimicCrab && self.unrecognizedFlags.Count > 0)
        {
            AbstractPhysicalObject.AbstractObjectType type = new(self.unrecognizedFlags[0]);
            if (type.index != -1)
            {
                AbstractPhysicalObject shell = new(self.world, type, null, self.pos, self.world.game.GetNewID());
                self.Room.AddEntity(shell);

                new AbstractCrabShellStick(self, shell);
            }
            else
            {
                Debug.Log("<Archdruid's Additions> Error while spawning Shell for Mimic Crab. Type was, \'" + type.value + "\'. Is the Type formatted correctly? Check the Steam Workshop page for more information.");
            }
        }
    }
}
