using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class InsectHooks
{
    internal static void MiniFly_Update(On.MiniFly.orig_Update orig, MiniFly self, bool eu)
    {
        orig(self, eu);
        if (self.buzzAroundCorpse == null && self.wantToBurrow == false)
        {
            Potato nearestPotato = GetNearestPotato(self.pos, self.room);
            if (nearestPotato != null)
            {
                self.vel += Custom.DirVec(self.pos, nearestPotato.bodyChunks[1].pos + nearestPotato.rotation * 20f + Custom.RNV() * 5f);
            }
        }
    }
    internal static void RedSwarmer_Update(On.RedSwarmer.orig_Update orig, RedSwarmer self, bool eu)
    {
        orig(self, eu);
        if (self.wantToBurrow == false)
        {
            Potato nearestPotato = GetNearestPotato(self.pos, self.room);
            if (nearestPotato != null)
            {
                self.hoverPos = nearestPotato.bodyChunks[1].pos + nearestPotato.rotation * 20f;
            }
        }
    }
    internal static Potato GetNearestPotato(Vector2 selfPosition, Room room)
    {
        Potato nearestPotato = null;
        for (int i = 0; i < 3; i++)
        {
            foreach (PhysicalObject obj in room.physicalObjects[i])
            {
                if (obj is Potato potato && Custom.DistLess(potato.bodyChunks[1].pos, selfPosition, 400f))
                {
                    if (nearestPotato == null || Custom.Dist(nearestPotato.bodyChunks[1].pos, selfPosition) > Custom.Dist(potato.bodyChunks[1].pos, selfPosition))
                    {
                        nearestPotato = potato;
                    }
                }
            }
        }
        return nearestPotato;
    }
}
