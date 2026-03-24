using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using static ArchdruidsAdditions.Methods.Methods;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

public static class ShelterHooks
{
    internal static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        List<EntityID> doomedCreatures = [];
        List<Creature> creaturesInRoom = [];
        List<Player> playersInRoom = [];

        //Methods.Methods.LogMethodStart("SHELTERDOOR_DOORCLOSED");

        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                PhysicalObject obj = self.room.physicalObjects[i][j];
                if (obj is Parasite parasite && parasite.ParasiteState.creatureAttachedTo.HasValue && parasite.ParasiteState.growth == 6)
                {
                    doomedCreatures.Add(parasite.ParasiteState.creatureAttachedTo.Value);
                }
                else if (obj is Creature)
                {
                    creaturesInRoom.Add(obj as Creature);
                    if (obj is Player)
                    {
                        playersInRoom.Add(obj as Player);
                    }
                }
            }
        }

        if (doomedCreatures.Count > 0)
        {
            bool allPlayersDead = true;
            foreach (Player player in playersInRoom)
            {
                if (!doomedCreatures.Contains(player.abstractCreature.ID))
                {
                    allPlayersDead = false;
                    break;
                }
            }

            if (allPlayersDead)
            {
                self.room.game.GoToDeathScreen();

                //Methods.Methods.LogMessage("PLAYER WAS KILLED BY PARASITE!");
                //Methods.Methods.LogMethodEnd();

                return;
            }
        }

        orig(self);

        //Methods.Methods.LogMethodEnd();
    }
}
