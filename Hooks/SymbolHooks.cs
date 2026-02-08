using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class SymbolHooks
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
        else if (itemType == Enums.AbstractObjectType.Potato)
        {
            text = "Icon_Potato";
        }
        else if (itemType == Enums.AbstractObjectType.Bow)
        {
            text = "Icon_Bow";
        }
        else if (itemType == Enums.AbstractObjectType.LightningFruit)
        {
            text = "Icon_LightningFruit";
        }
        else if (itemType == Enums.AbstractObjectType.FirePepper)
        {
            text = "Icon_FirePepper";
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
            color = new Color(1f, 0.79f, 0.3f);
        }
        else if (itemType == Enums.AbstractObjectType.Potato)
        {
            color = new Color(1f, 0.8f, 0.6f);
        }
        else if (itemType == Enums.AbstractObjectType.Bow)
        {
            color = new Color(0.5f, 0.5f, 0.5f);
        }
        else if (itemType == Enums.AbstractObjectType.LightningFruit)
        {
            color = new Color(0f, 0f, 1f);
        }
        else if (itemType == Enums.AbstractObjectType.FirePepper)
        {
            color = new Color(1f, 0f, 0f);
        }
        else
        {
            color = orig.Invoke(itemType, intData);
        }
        return color;
    }

    internal static string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        if (iconData.critType == Enums.CreatureTemplateType.CloudFish)
        {
            return "CloudFish";
        }
        return orig(iconData);
    }

    internal static Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        if (iconData.critType == Enums.CreatureTemplateType.CloudFish)
        {
            return new Color(0.6f, 0.8f, 0.8f);
        }
        return orig(iconData);
    }
}
