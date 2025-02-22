using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class ItemSymbolHooks
{
    // Token: 0x06000022 RID: 34 RVA: 0x00002F80 File Offset: 0x00001180
    internal static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        string text;
        if (itemType == Enums.AbstractObjectType.ScarletFlowerBulb)
        {
            text = "Icon_ScarletFlowerBulb";
        }
        else
        {
            text = orig.Invoke(itemType, intData);
        }
        return text;
    }

    // Token: 0x06000023 RID: 35 RVA: 0x00002FB4 File Offset: 0x000011B4
    internal static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        Color color;
        if (itemType == Enums.AbstractObjectType.ScarletFlowerBulb)
        {
            color = new Color(1f, 0f, 0f);
        }
        else
        {
            color = orig.Invoke(itemType, intData);
        }
        return color;
    }
}
