using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Methods
{
    public class ObjectMethods
    {
        public static void AttractInsects(PhysicalObject attObj, Vector2 hoverPos, bool attractFlies, bool attractSwarmers, bool attractBeetles, float attractDist)
        {
            foreach (IDrawable obj in attObj.room.drawableObjects)
            {
                if (obj is CosmeticInsect)
                {
                    if (attractFlies && obj is MiniFly fly && Custom.DistLess(fly.pos, hoverPos, attractDist) && fly.buzzAroundCorpse == null && fly.wantToBurrow == false)
                    {
                        fly.vel += Custom.DirVec(fly.pos, hoverPos + Custom.RNV() * 5f);
                        if (Custom.DistLess(hoverPos, fly.pos, 20f))
                        {
                            (attObj as Objects.Potato).pollinated++;
                        }
                    }
                    if (attractSwarmers && obj is RedSwarmer swarmer && Custom.DistLess(swarmer.pos, hoverPos, attractDist) && swarmer.wantToBurrow == false)
                    {
                        swarmer.hoverPos = hoverPos;
                        if (Custom.DistLess(hoverPos, swarmer.pos, 20f))
                        {
                            (attObj as Objects.Potato).pollinated++;
                        }
                    }
                }
            }
        }
    }
}
