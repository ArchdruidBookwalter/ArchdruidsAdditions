namespace ArchdruidsAdditions.Hooks;

public static class SymbolHooks
{
    internal static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        string baseText = orig(itemType, intData);

        if (itemType == AbstractObjectType.ScarletFlowerBulb)
        {
            return "Icon_ScarletFlowerBulb";
        }
        else if (itemType == AbstractObjectType.ParrySword)
        {
            return "Icon_ParrySword";
        }
        else if (itemType == AbstractObjectType.Potato)
        {
            return "Icon_Potato";
        }
        else if (itemType == AbstractObjectType.Bow)
        {
            return "Icon_Bow";
        }
        else if (itemType == AbstractObjectType.LightningFruit)
        {
            return "Icon_LightningFruit";
        }
        else if (itemType == AbstractObjectType.AshPepper)
        {
            return "Icon_AshPepper";
        }
        else if (itemType == AbstractObjectType.ParasiteEgg)
        {
            return "Icon_ParasiteEgg";
        }
        else if (itemType == AbstractObjectType.CrabShell)
        {
            return "Icon_SnailShell";
        }

        return baseText;
    }

    internal static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        Color baseColor = orig(itemType, intData);

        if (itemType == AbstractObjectType.ScarletFlowerBulb)
        {
            return new Color(1f, 0f, 0f);
        }
        else if (itemType == AbstractObjectType.ParrySword)
        {
            return new Color(1f, 0.79f, 0.3f);
        }
        else if (itemType == AbstractObjectType.Potato)
        {
            return new Color(1f, 0.8f, 0.6f);
        }
        else if (itemType == AbstractObjectType.Bow)
        {
            return new Color(0.5f, 0.5f, 0.5f);
        }
        else if (itemType == AbstractObjectType.LightningFruit)
        {
            return new Color(0f, 0f, 1f);
        }
        else if (itemType == AbstractObjectType.AshPepper)
        {
            return new Color(0.68235296f, 0.15686275f, 0.11764706f);
        }
        else if (itemType == AbstractObjectType.ParasiteEgg)
        {
            return new Color(0.4f, 0.8f, 0f);
        }
        else if (itemType == AbstractObjectType.CrabShell)
        {
            return Custom.HSL2RGB(0.04f, 0.4f, 0.6f);
        }

        return baseColor;
    }

    internal static string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        string baseSpriteName = orig(iconData);

        if (iconData.critType == CreatureTemplateType.CloudFish)
        {
            return "CloudFish";
        }
        else if (iconData.critType == CreatureTemplateType.Parasite)
        {
            return "Parasite";
        }
        else if (iconData.critType == CreatureTemplateType.MimicCrab)
        {
            return "MimicCrab";
        }

        return baseSpriteName;
    }

    internal static Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        Color baseColor = orig(iconData);

        if (iconData.critType == CreatureTemplateType.CloudFish)
        {
            return Custom.HSL2RGB(0.5f, 0.8f, 0.8f);
        }
        if (iconData.critType == CreatureTemplateType.MimicCrab)
        {
            return Custom.HSL2RGB(0f, 0.5f, 0.4f);
        }

        return baseColor;
    }
}
