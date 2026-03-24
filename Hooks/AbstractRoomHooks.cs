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
            /*
            Debug.Log("CLOUDFISH CALLED METHOD: \'AbstractRoom_ConnectivityCost\'");

            int cost = orig(self, start, goal, template);
            Debug.Log("Stats: [" + self.name + " - " + start + " : " + goal + " - " + cost + "]");

            return cost;
            */

        }
        return orig(self, start, goal, template);
    }

    internal static int AbstractRoomNode_ConnectionCost(On.AbstractRoomNode.orig_ConnectionCost orig, ref AbstractRoomNode self, int otherNode, CreatureTemplate template)
    {
        /*
        if (template.type == Enums.CreatureTemplateType.CloudFish)
        {
            Debug.Log("CLOUDFISH CALLED METHOD: \'AbstractRoomNode_ConnectionCost\'");
            if (template.PreBakedPathingIndex == -1 || otherNode < 0)
            {
                Debug.Log("Node Type: " + self.type.value + " Connectivity: " + self.connectivity[template.PreBakedPathingIndex, otherNode, 1]);
            }
        }
        */

        return orig(ref self, otherNode, template);
    }
}
