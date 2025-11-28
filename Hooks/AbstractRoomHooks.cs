using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractRoomHooks
{
    internal static int AbstractRoom_ConnectivityCost(On.AbstractRoom.orig_ConnectivityCost orig, AbstractRoom self, int start, int goal, CreatureTemplate template)
    {
        if (template.type == Enums.CreatureTemplateType.CloudFish)
        {
            //Debug.Log("CLOUDFISH CALLED METHOD: \'AbstractRoom_ConnectivityCost\'");
            //Debug.Log("ROOM: " + self.name + " START: " + start + " GOAL: " + goal);
        }
        return orig(self, start, goal, template);
    }
}
