using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

internal static class InsectHooks
{
    internal static void On_MiniFly_Update(On.MiniFly.orig_Update orig, MiniFly self, bool eu)
    {
        if (self.buzzAroundCorpse != null || self.wantToBurrow)
        {
            orig(self, eu);
        }
        else
        {
            Potato potato = null;

            for (int i = 0; i < 3;  i++)
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[i])
                {
                    if (obj is Potato potato1 && potato1.pollinated < 500 && Custom.DistLess(potato1.bodyChunks[1].pos, self.pos, 400f))
                    {
                        Debug.Log("Fly Update Hook!");
                        if (potato == null || Custom.Dist(potato.bodyChunks[1].pos, self.pos) > Custom.Dist(potato1.bodyChunks[1].pos, self.pos))
                        {
                            potato = potato1;
                        }
                    }
                }
            }

            if (potato != null)
            {
                self.vel += Custom.DirVec(self.pos, potato.bodyChunks[1].pos + potato.rotation * 10f + Custom.RNV() * 5f);
                if (Custom.DistLess(potato.bodyChunks[1].pos, self.pos, 20f))
                {
                    potato.pollinated++;
                }
            }
            orig(self, eu);
        }
    }
    internal static void On_RedSwarmer_Update(On.RedSwarmer.orig_Update orig, RedSwarmer self, bool eu)
    {
        Potato potato = null;

        for (int i = 0; i < 3; i++)
        {
            foreach (PhysicalObject obj in self.room.physicalObjects[i])
            {
                if (obj is Potato potato1 && potato1.pollinated < 500 && Custom.DistLess(potato1.bodyChunks[1].pos, self.pos, 400f))
                {
                    Debug.Log("Red Swarmer Update Hook!");
                    if (potato == null || Custom.Dist(potato.bodyChunks[1].pos, self.pos) > Custom.Dist(potato1.bodyChunks[1].pos, self.pos))
                    {
                        potato = potato1;
                    }
                }
            }
        }

        if (potato != null)
        {
            self.hoverPos = potato.bodyChunks[1].pos + potato.rotation * 10f;
            if (Custom.DistLess(potato.bodyChunks[1].pos, self.pos, 20f))
            {
                potato.pollinated++;
            }
        }
        orig(self, eu);
    }
}
