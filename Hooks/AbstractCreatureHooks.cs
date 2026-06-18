using System;
using System.Collections.Generic;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractCreatureHooks
{
    internal static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
    {
        //Methods.Methods.LogMethodStart("ABSTRACTCREATURE_REALIZE");

        float section = 0;

        try
        {

            if (self.Room != null && self.realizedCreature == null)
            {
                //Debug.Log("CREATURE \'" + self.creatureTemplate.name + "\' REALIZED IN ROOM.");
                if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
                {
                    self.realizedCreature = new CloudFish(self, self.world);
                    self.InitiateAI();
                }
                else if (self.creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
                {
                    self.realizedCreature = new Parasite(self, self.world);
                    self.InitiateAI();
                }
            }

            section = 1;

            orig(self);

            section = 2;

            if (self.realizedCreature != null && self.Room.realizedRoom != null)
            {
                bool rotten = false;
                if (self.unrecognizedAttributes != null && self.unrecognizedAttributes.Length > 0)
                {
                    //Debug.Log("");
                    //Debug.Log("UNRECOGNIZED STRING ATTRIBUTES: ");
                    foreach (string unrecognizedString in self.unrecognizedAttributes)
                    {
                        if (unrecognizedString == "INFECTED")
                        {
                            rotten = true;
                        }

                        //Debug.Log(unrecognizedString);
                    }
                }

                section = 3;

                List<AbstractPhysicalObject> eggs = [];
                foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.stuckObjects)
                {
                    if (stick is AbstractParasiteEggStick eggStick)
                    {
                        eggs.Add(eggStick.Egg);
                    }
                }

                section = 4;

                if (rotten || eggs.Count > 0)
                {
                    InfectedCorpse corpse = new(self, eggs, false);
                    self.Room.realizedRoom.AddObject(corpse);

                    section = 5;

                    foreach (AbstractPhysicalObject egg in eggs)
                    {
                        if (egg.realizedObject != null)
                        {
                            (egg.realizedObject as ParasiteEgg).StickInCorpse(corpse);
                        }
                    }

                    section = 6;

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

                    section = 7;

                    self.realizedCreature.Die();
                }
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
        orig(self);
    }
    internal static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            self.abstractAI = new CloudFishAbstractAI(world, self);
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
}
