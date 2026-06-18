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
        ScavengerBehavior.RegisterValues();
        CreatureTemplateType.RegisterValues();
        NewSoundID.RegisterValues();
    }
    public static void UnregisterAllEnums()
    {
        AbstractObjectType.UnregisterValues();
        MiscItemType.UnregisterValues();
        MultiplayerItemType.UnregisterValues();
        PlacedObjectType.UnregisterValues();
        SandboxUnlockID.UnregisterValues();
        ScavengerAnimationID.UnregisterValues();
        ScavengerBehavior.UnregisterValues();
        CreatureTemplateType.UnregisterValues();
        NewSoundID.UnregisterValues();
    }
}

public class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType Bow;
    public static AbstractPhysicalObject.AbstractObjectType ScarletFlowerBulb;
    public static AbstractPhysicalObject.AbstractObjectType ParrySword;
    public static AbstractPhysicalObject.AbstractObjectType Potato;
    public static AbstractPhysicalObject.AbstractObjectType LightningFruit;
    public static AbstractPhysicalObject.AbstractObjectType AshPepper;
    public static AbstractPhysicalObject.AbstractObjectType ParasiteEgg;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
        LightningFruit = new("LightningFruit", true);
        AshPepper = new("AshPepper", true);
        ParasiteEgg = new("ParasiteEgg", true);
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
        if (LightningFruit != null)
        {
            LightningFruit.Unregister();
            LightningFruit = null;
        }
        if (AshPepper != null)
        {
            AshPepper.Unregister();
            AshPepper = null;
        }
        if (ParasiteEgg != null)
        {
            ParasiteEgg.Unregister();
            ParasiteEgg = null;
        }
    }
}
public class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType Bow;
    public static SLOracleBehaviorHasMark.MiscItemType ScarletFlowerBulb;
    public static SLOracleBehaviorHasMark.MiscItemType ParrySword;
    public static SLOracleBehaviorHasMark.MiscItemType Potato;
    public static SLOracleBehaviorHasMark.MiscItemType LightningFruit;
    public static SLOracleBehaviorHasMark.MiscItemType AshPepper;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
        LightningFruit = new("LightningFruit", true);
        AshPepper = new("AshPepper", true);
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
        if (LightningFruit != null)
        {
            LightningFruit.Unregister();
            LightningFruit = null;
        }
        if (AshPepper != null)
        {
            AshPepper.Unregister();
            AshPepper = null;
        }
    }
}
public class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type Bow;
    public static PlacedObject.MultiplayerItemData.Type ScarletFlowerBulb;
    public static PlacedObject.MultiplayerItemData.Type ParrySword;
    public static PlacedObject.MultiplayerItemData.Type Potato;
    public static PlacedObject.MultiplayerItemData.Type LightningFruit;
    public static PlacedObject.MultiplayerItemData.Type AshPepper;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
        LightningFruit = new("LightningFruit", true);
        AshPepper = new("AshPepper", true);
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
        if (LightningFruit != null)
        {
            LightningFruit.Unregister();
            LightningFruit = null;
        }
        if (AshPepper != null)
        {
            AshPepper.Unregister();
            AshPepper = null;
        }
    }
}
public class PlacedObjectType
{
    public static PlacedObject.Type ScarletFlower;
    public static PlacedObject.Type Potato;
    public static PlacedObject.Type LightningFruit;
    public static PlacedObject.Type DecoLightningVine;
    public static PlacedObject.Type AshPepperBush;
    public static PlacedObject.Type InfectedCorpse;

    public static void RegisterValues()
    {
        ScarletFlower = new("ScarletFlower", true);
        Potato = new("Potato", true);
        LightningFruit = new("LightningFruit", true);
        DecoLightningVine = new("DecoLightningVine", true);
        AshPepperBush = new("AshPepperBush", true);
        InfectedCorpse = new("InfectedCorpse", true);
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
        if (LightningFruit != null)
        {
            LightningFruit.Unregister();
            LightningFruit = null;
        }
        if (DecoLightningVine != null)
        {
            DecoLightningVine.Unregister();
            DecoLightningVine = null;
        }
        if (AshPepperBush != null)
        {
            AshPepperBush.Unregister();
            AshPepperBush = null;
        }
        if (InfectedCorpse != null)
        {
            InfectedCorpse.Unregister();
            InfectedCorpse = null;
        }
    }
}
public class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID Bow;
    public static MultiplayerUnlocks.SandboxUnlockID ScarletFlowerBulb;
    public static MultiplayerUnlocks.SandboxUnlockID ParrySword;
    public static MultiplayerUnlocks.SandboxUnlockID Potato;
    public static MultiplayerUnlocks.SandboxUnlockID LightningFruit;
    public static MultiplayerUnlocks.SandboxUnlockID AshPepper;

    public static void RegisterValues()
    {
        Bow = new("Bow", true);
        ScarletFlowerBulb = new("ScarletFlowerBulb", true);
        ParrySword = new("ParrySword", true);
        Potato = new("Potato", true);
        LightningFruit = new("LightningFruit", true);
        AshPepper = new("AshPepper", true);
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
        if (LightningFruit != null)
        {
            LightningFruit.Unregister();
            LightningFruit = null;
        }
        if (AshPepper != null)
        {
            AshPepper.Unregister();
            AshPepper = null;
        }
    }
}
public class ScavengerAnimationID
{
    public static Scavenger.ScavengerAnimation.ID AimBow;

    public static void RegisterValues()
    {
        AimBow = new("LoadSpearIntoBow", true);
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
public class ScavengerBehavior
{
    public static ScavengerAI.Behavior AttackWithBow;
    public static void RegisterValues()
    {
        AttackWithBow = new("AttackWithBow", true);
    }

    public static void UnregisterValues()
    {
        if (AttackWithBow != null)
        {
            AttackWithBow.Unregister();
            AttackWithBow = null;
        }
    }
}
public class CreatureTemplateType
{
    public static CreatureTemplate.Type CloudFish;
    public static CreatureTemplate.Type Parasite;

    public static void RegisterValues()
    {
        CloudFish = new("CloudFish", true);
        Parasite = new("Parasite", true);
    }

    public static void UnregisterValues()
    {
        if (CloudFish != null)
        {
            CloudFish.Unregister();
            CloudFish = null;
        }
        if (Parasite != null)
        {
            Parasite.Unregister();
            Parasite = null;
        }
    }
}
public class NewSoundID
{
    public static SoundID AA_CloudFishWhistle1;
    public static SoundID AA_CloudFishWhistle2;
    public static SoundID AA_CloudFishWhistle3;
    public static SoundID AA_CloudFishScream;
    public static SoundID AA_CloudFishDeath;

    public static void RegisterValues()
    {
        AA_CloudFishWhistle1 = new("AA_CloudFishWhistle1", true);
        AA_CloudFishWhistle2 = new("AA_CloudFishWhistle2", true);
        AA_CloudFishWhistle3 = new("AA_CloudFishWhistle3", true);
        AA_CloudFishScream = new("AA_CloudFishScream", true);
        AA_CloudFishDeath = new("AA_CloudFishDeath", true);
    }

    public static SoundID RandomCloudFishWhistle()
    {
        float random = UnityEngine.Random.value;
        if (random < 0.33)
        {
            return AA_CloudFishWhistle1;
        }
        else if (random < 0.66)
        {
            return AA_CloudFishWhistle2;
        }
        else
        {
            return AA_CloudFishWhistle3;
        }
    }

    public static void UnregisterValues()
    {
        if (AA_CloudFishWhistle1 != null)
        {
            AA_CloudFishWhistle1.Unregister();
            AA_CloudFishWhistle1 = null;
        }
        if (AA_CloudFishWhistle2 != null)
        {
            AA_CloudFishWhistle2.Unregister();
            AA_CloudFishWhistle2 = null;
        }
        if (AA_CloudFishWhistle3 != null)
        {
            AA_CloudFishWhistle3.Unregister();
            AA_CloudFishWhistle3 = null;
        }
        if (AA_CloudFishScream != null)
        {
            AA_CloudFishScream.Unregister();
            AA_CloudFishScream = null;
        }
        if (AA_CloudFishDeath != null)
        {
            AA_CloudFishDeath.Unregister();
            AA_CloudFishDeath = null;
        }
    }
}
