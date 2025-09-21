using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using DevInterface;
using static ArchdruidsAdditions.Effects.LightRodPowerEffect;

namespace ArchdruidsAdditions.Enums;

public class AAEnums
{
    public static void RegisterAllEnums()
    {
        AbstractObjectType.RegisterValues();
        MiscItemType.RegisterValues();
        MultiplayerItemType.RegisterValues();
        PlacedObjectType.RegisterValues();
        SandboxUnlockID.RegisterValues();
        ScavengerAnimationID.RegisterValues();
        CreatureTemplateType.RegisterValues();
    }
    public static void UnregisterAllEnums()
    {
        AbstractObjectType.UnregisterValues();
        MiscItemType.UnregisterValues();
        MultiplayerItemType.UnregisterValues();
        PlacedObjectType.UnregisterValues();
        SandboxUnlockID.UnregisterValues();
        ScavengerAnimationID.UnregisterValues();
        CreatureTemplateType.UnregisterValues();
    }
}

public class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType Bow;
    public static AbstractPhysicalObject.AbstractObjectType ScarletFlowerBulb;
    public static AbstractPhysicalObject.AbstractObjectType ParrySword;
    public static AbstractPhysicalObject.AbstractObjectType Potato;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
    }

    public static void UnregisterValues()
    {
        if (Bow != null)
        {
            Bow.Unregister();
            Bow = null;
        }
        if (ScarletFlowerBulb != null)
        {
            ScarletFlowerBulb.Unregister();
            ScarletFlowerBulb = null;
        }
        if (ParrySword != null)
        {
            ParrySword.Unregister();
            ParrySword = null;
        }
        if (Potato != null)
        {
            Potato.Unregister();
            Potato = null;
        }
    }
}
public class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType Bow;
    public static SLOracleBehaviorHasMark.MiscItemType ScarletFlowerBulb;
    public static SLOracleBehaviorHasMark.MiscItemType ParrySword;
    public static SLOracleBehaviorHasMark.MiscItemType Potato;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
    }

    public static void UnregisterValues()
    {
        if (Bow != null)
        {
            Bow.Unregister();
            Bow = null;
        }
        if (ScarletFlowerBulb != null)
        {
            ScarletFlowerBulb.Unregister();
            ScarletFlowerBulb = null;
        }
        if (ParrySword != null)
        {
            ParrySword.Unregister();
            ParrySword = null;
        }
        if (Potato != null)
        {
            Potato.Unregister();
            Potato = null;
        }
    }
}
public class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type Bow;
    public static PlacedObject.MultiplayerItemData.Type ScarletFlowerBulb;
    public static PlacedObject.MultiplayerItemData.Type ParrySword;
    public static PlacedObject.MultiplayerItemData.Type Potato;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
    }

    public static void UnregisterValues()
    {
        if (Bow != null)
        {
            Bow.Unregister();
            Bow = null;
        }
        if (ScarletFlowerBulb != null)
        {
            ScarletFlowerBulb.Unregister();
            ScarletFlowerBulb = null;
        }
        if (ParrySword != null)
        {
            ParrySword.Unregister();
            ParrySword = null;
        }
        if (Potato != null)
        {
            Potato.Unregister();
            Potato = null;
        }
    }
}
public class PlacedObjectType
{
    public static PlacedObject.Type ScarletFlower;
    public static PlacedObject.Type Potato;

    public static void RegisterValues()
    {
        ScarletFlower = new("ScarletFlower", true);
        Potato = new("Potato", true);
    }

    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
        if (Potato != null)
        {
            Potato.Unregister();
            Potato = null;
        }
    }
}
public class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID Bow;
    public static MultiplayerUnlocks.SandboxUnlockID ScarletFlowerBulb;
    public static MultiplayerUnlocks.SandboxUnlockID ParrySword;
    public static MultiplayerUnlocks.SandboxUnlockID Potato;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
    }

    public static void UnregisterValues()
    {
        if (Bow != null)
        {
            Bow.Unregister();
            Bow = null;
        }
        if (ScarletFlowerBulb != null)
        {
            ScarletFlowerBulb.Unregister();
            ScarletFlowerBulb = null;
        }
        if (ParrySword != null)
        {
            ParrySword.Unregister();
            ParrySword = null;
        }
        if (Potato != null)
        {
            Potato.Unregister();
            Potato = null;
        }
    }
}
public class ScavengerAnimationID
{
    public static Scavenger.ScavengerAnimation.ID AimBow;

    public static void RegisterValues()
    {
        AimBow = new("AimBow", true);
    }

    public static void UnregisterValues()
    {
        if (AimBow != null)
        {
            AimBow.Unregister();
            AimBow = null;
        }
    }
}
public class CreatureTemplateType
{
    public static CreatureTemplate.Type Herring;

    public static void RegisterValues()
    {
        Herring = new("Herring", true);
    }

    public static void UnregisterValues()
    {
        if (Herring != null)
        {
            Herring.Unregister();
            Herring = null;
        }
    }
}
