using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class ItemSymbolHooks
{
    internal static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        string text;
        if (itemType == Enums.AbstractObjectType.ScarletFlowerBulb)
        {
            text = "Icon_ScarletFlowerBulb";
        }
        else if (itemType == Enums.AbstractObjectType.ParrySword)
        {
            text = "Icon_ParrySword";
        }
        else
        {
            text = orig.Invoke(itemType, intData);
        }
        return text;
    }

    internal static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        Color color;
        if (itemType == Enums.AbstractObjectType.ScarletFlowerBulb)
        {
            color = new Color(1f, 0f, 0f);
        }
        else if (itemType == Enums.AbstractObjectType.ParrySword)
        {
            color = new Color(0f, 1f, 0.5f);
        }
        else
        {
            color = orig.Invoke(itemType, intData);
        }
        return color;
    }
}
