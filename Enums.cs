using System.Collections.Generic;
using System.Linq;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Decoration;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ArchdruidsAdditions.Enums;

public class AAEnums
{
    public static void RegisterAllEnums()
    {
        Debug.Log("<Archduid's Additions> REGISTERING ENUMS");

        AbstractObjectType.RegisterValues();
        MiscItemType.RegisterValues();
        MultiplayerItemType.RegisterValues();
        PlacedObjectType.RegisterValues();
        SandboxUnlockID.RegisterValues();
        ScavengerAnimationID.RegisterValues();
        ScavengerBehavior.RegisterValues();
        CreatureTemplateType.RegisterValues();
        NewSoundID.RegisterValues();

        Debug.Log("<Archduid's Additions> REGISTERED ENUMS");
    }
    public static void UnregisterAllEnums()
    {
        Debug.Log("<Archduid's Additions> UNREGISTERING ENUMS");

        AbstractObjectType.UnregisterValues();
        MiscItemType.UnregisterValues();
        MultiplayerItemType.UnregisterValues();
        PlacedObjectType.UnregisterValues();
        SandboxUnlockID.UnregisterValues();
        ScavengerAnimationID.UnregisterValues();
        ScavengerBehavior.UnregisterValues();
        CreatureTemplateType.UnregisterValues();
        NewSoundID.UnregisterValues();

        Debug.Log("<Archduid's Additions> UNREGISTERED ENUMS");
    }
}

public class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType
        Bow,
        ScarletFlowerBulb, 
        ParrySword,
        Potato,
        LightningFruit,
        AshPepper,
        ParasiteEgg,
        CrabShell;

    public static List<AbstractPhysicalObject.AbstractObjectType> values = [];

    public static void RegisterValues()
    {
        Bow = Register(nameof(Bow));
        ScarletFlowerBulb = Register(nameof(ScarletFlowerBulb));
        ParrySword = Register(nameof(ParrySword));
        Potato = Register(nameof(Potato));
        LightningFruit = Register(nameof(LightningFruit));
        AshPepper = Register(nameof(AshPepper));
        ParasiteEgg = Register(nameof(ParasiteEgg));
        CrabShell = Register(nameof(CrabShell));

        values.Add(Bow);
        values.Add(ScarletFlowerBulb);
        values.Add(ParrySword);
        values.Add(Potato);
        values.Add(LightningFruit);
        values.Add(AshPepper);
        values.Add(ParasiteEgg);
        values.Add(CrabShell);
    }
    public static void UnregisterValues()
    {
        Bow = Unregister(Bow);
        ScarletFlowerBulb = Unregister(ScarletFlowerBulb);
        ParrySword = Unregister(ParrySword);
        Potato = Unregister(Potato);
        LightningFruit = Unregister(LightningFruit);
        AshPepper = Unregister(AshPepper);
        ParasiteEgg = Unregister(ParasiteEgg);
        CrabShell = Unregister(CrabShell);

        values.Clear();
    }

    public static AbstractPhysicalObject.AbstractObjectType Register(string name)
    { return new AbstractPhysicalObject.AbstractObjectType(name, true); }
    public static AbstractPhysicalObject.AbstractObjectType Unregister(AbstractPhysicalObject.AbstractObjectType type)
    { type?.Unregister(); return null; }
}

public class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType
        Bow,
        ScarletFlowerBulb,
        ParrySword,
        Potato,
        LightningFruit,
        AshPepper,
        CrabShell;

    public static List<SLOracleBehaviorHasMark.MiscItemType> values = [];

    public static void RegisterValues()
    {
        Bow = Register(nameof(Bow));
        ScarletFlowerBulb = Register(nameof(ScarletFlowerBulb));
        ParrySword = Register(nameof(ParrySword));
        Potato = Register(nameof(Potato));
        LightningFruit = Register(nameof(LightningFruit));
        AshPepper = Register(nameof(AshPepper));
        CrabShell = Register(nameof(CrabShell));

        values.Add(Bow);
        values.Add(ScarletFlowerBulb);
        values.Add(ParrySword);
        values.Add(Potato);
        values.Add(LightningFruit);
        values.Add(AshPepper);
        values.Add(CrabShell);
    }
    public static void UnregisterValues()
    {
        Bow = Unregister(Bow);
        ScarletFlowerBulb = Unregister(ScarletFlowerBulb);
        ParrySword = Unregister(ParrySword);
        Potato = Unregister(Potato);
        LightningFruit = Unregister(LightningFruit);
        AshPepper = Unregister(AshPepper);
        CrabShell = Unregister(CrabShell);

        values.Clear();
    }

    public static SLOracleBehaviorHasMark.MiscItemType Register(string name)
    { return new SLOracleBehaviorHasMark.MiscItemType(name, true); }
    public static SLOracleBehaviorHasMark.MiscItemType Unregister(SLOracleBehaviorHasMark.MiscItemType type)
    { type?.Unregister(); return null; }
}

public class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type
        Bow,
        ScarletFlowerBulb,
        ParrySword,
        Potato,
        LightningFruit,
        AshPepper,
        CrabShell;

    public static List<PlacedObject.MultiplayerItemData.Type> values = [];

    public static void RegisterValues()
    {
        Bow = Register(nameof(Bow));
        ScarletFlowerBulb = Register(nameof(ScarletFlowerBulb));
        ParrySword = Register(nameof(ParrySword));
        Potato = Register(nameof(Potato));
        LightningFruit = Register(nameof(LightningFruit));
        AshPepper = Register(nameof(AshPepper));
        CrabShell = Register(nameof(CrabShell));

        values.Add(Bow);
        values.Add(ScarletFlowerBulb);
        values.Add(ParrySword);
        values.Add(Potato);
        values.Add(LightningFruit);
        values.Add(AshPepper);
        values.Add(CrabShell);
    }
    public static void UnregisterValues()
    {
        Bow = Unregister(Bow);
        ScarletFlowerBulb = Unregister(ScarletFlowerBulb);
        ParrySword = Unregister(ParrySword);
        Potato = Unregister(Potato);
        LightningFruit = Unregister(LightningFruit);
        AshPepper = Unregister(AshPepper);
        CrabShell = Unregister(CrabShell);

        values.Clear();
    }

    public static PlacedObject.MultiplayerItemData.Type Register(string name)
    { return new PlacedObject.MultiplayerItemData.Type(name, true); }
    public static PlacedObject.MultiplayerItemData.Type Unregister(PlacedObject.MultiplayerItemData.Type type)
    { type?.Unregister(); return null; }
}

public class PlacedObjectType
{
    public static PlacedObject.Type 
        ScarletFlower,
        Potato,
        LightningFruit,
        DecoLightningVine,
        AshPepperBush,
        InfectedCorpse,
        CrabShellCircle,
        RopeObject;

    public static List<PlacedObject.Type> values = [];

    public static void RegisterValues()
    {
        ScarletFlower = Register(nameof(ScarletFlower));
        Potato = Register(nameof(Potato));
        LightningFruit = Register(nameof(LightningFruit));
        DecoLightningVine = Register(nameof(DecoLightningVine));
        AshPepperBush = Register(nameof(AshPepperBush));
        InfectedCorpse = Register(nameof(InfectedCorpse));
        CrabShellCircle = Register(nameof(CrabShellCircle));
        RopeObject = Register(nameof(RopeObject));

        values.Add(ScarletFlower);
        values.Add(Potato);
        values.Add(LightningFruit);
        values.Add(DecoLightningVine);
        values.Add(AshPepperBush);
        values.Add(InfectedCorpse);
        values.Add(CrabShellCircle);
        values.Add(RopeObject);
    }
    public static void UnregisterValues()
    {
        ScarletFlower = Unregister(ScarletFlower);
        Potato = Unregister(Potato);
        LightningFruit = Unregister(LightningFruit);
        DecoLightningVine = Unregister(DecoLightningVine);
        AshPepperBush = Unregister(AshPepperBush);
        InfectedCorpse = Unregister(InfectedCorpse);
        CrabShellCircle = Unregister(CrabShellCircle);
        RopeObject = Unregister(RopeObject);

        values.Clear();
    }

    public static PlacedObject.Type Register(string name)
    { return new PlacedObject.Type(name, true); }
    public static PlacedObject.Type Unregister(PlacedObject.Type type)
    { type?.Unregister(); return null; }
}

public class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID
        Bow,
        ScarletFlowerBulb,
        ParrySword,
        Potato,
        LightningFruit,
        AshPepper,
        CrabShell;

    public static List<MultiplayerUnlocks.SandboxUnlockID> values = [];

    public static void RegisterValues()
    {
        Bow = Register(nameof(Bow));
        ScarletFlowerBulb = Register(nameof(ScarletFlowerBulb));
        ParrySword = Register(nameof(ParrySword));
        Potato = Register(nameof(Potato));
        LightningFruit = Register(nameof(LightningFruit));
        AshPepper = Register(nameof(AshPepper));
        CrabShell = Register(nameof(CrabShell));

        values.Add(Bow);
        values.Add(ScarletFlowerBulb);
        values.Add(ParrySword);
        values.Add(Potato);
        values.Add(LightningFruit);
        values.Add(AshPepper);
        values.Add(CrabShell);
    }
    public static void UnregisterValues()
    {
        Bow = Unregister(Bow);
        ScarletFlowerBulb = Unregister(ScarletFlowerBulb);
        ParrySword = Unregister(ParrySword);
        Potato = Unregister(Potato);
        LightningFruit = Unregister(LightningFruit);
        AshPepper = Unregister(AshPepper);
        CrabShell = Unregister(CrabShell);

        values.Clear();
    }

    public static MultiplayerUnlocks.SandboxUnlockID Register(string name)
    { return new MultiplayerUnlocks.SandboxUnlockID(name, true); }
    public static MultiplayerUnlocks.SandboxUnlockID Unregister(MultiplayerUnlocks.SandboxUnlockID type)
    { type?.Unregister(); return null; }
}

public class ScavengerAnimationID
{
    public static Scavenger.ScavengerAnimation.ID AimBow;

    public static void RegisterValues()
    {
        AimBow = Register(nameof(AimBow));
    }
    public static void UnregisterValues()
    {
        AimBow = Unregister(AimBow);
    }

    public static Scavenger.ScavengerAnimation.ID Register(string name)
    { return new Scavenger.ScavengerAnimation.ID(name, true); }
    public static Scavenger.ScavengerAnimation.ID Unregister(Scavenger.ScavengerAnimation.ID type)
    { type?.Unregister(); return null; }
}

public class ScavengerBehavior
{
    public static ScavengerAI.Behavior AttackWithBow;

    public static void RegisterValues()
    {
        AttackWithBow = Register(nameof(AttackWithBow));
    }
    public static void UnregisterValues()
    {
        AttackWithBow = Unregister(AttackWithBow);
    }

    public static ScavengerAI.Behavior Register(string name)
    { return new ScavengerAI.Behavior(name, true); }
    public static ScavengerAI.Behavior Unregister(ScavengerAI.Behavior type)
    { type?.Unregister(); return null; }
}

public class CreatureTemplateType
{
    public static CreatureTemplate.Type CloudFish;
    public static CreatureTemplate.Type Parasite;
    public static CreatureTemplate.Type MimicCrab;

    public static List<CreatureTemplate.Type> values = [];

    public static void RegisterValues()
    {
        CloudFish = Register(nameof(CloudFish));
        Parasite = Register(nameof(Parasite));
        MimicCrab = Register(nameof(MimicCrab));

        values.Add(CloudFish);
        values.Add(Parasite);
        values.Add(MimicCrab);
    }
    public static void UnregisterValues()
    {
        CloudFish = Unregister(CloudFish);
        Parasite = Unregister(Parasite);
        MimicCrab = Unregister(MimicCrab);

        values.Clear();
    }

    public static CreatureTemplate.Type Register(string name)
    { return new CreatureTemplate.Type(name, true); }
    public static CreatureTemplate.Type Unregister(CreatureTemplate.Type type)
    { type?.Unregister(); return null; }
}

public class NewSoundID
{
    public static SoundID AA_CloudFishWhistle1;
    public static SoundID AA_CloudFishWhistle2;
    public static SoundID AA_CloudFishWhistle3;
    public static SoundID AA_CloudFishScream;
    public static SoundID AA_CloudFishDeath;

    public static List<SoundID> values =
    [
        AA_CloudFishWhistle1,
        AA_CloudFishWhistle2,
        AA_CloudFishWhistle3,
        AA_CloudFishScream,
        AA_CloudFishDeath
    ];

    public static void RegisterValues()
    {
        AA_CloudFishWhistle1 = Register(nameof(AA_CloudFishWhistle1));
        AA_CloudFishWhistle2 = Register(nameof(AA_CloudFishWhistle2));
        AA_CloudFishWhistle3 = Register(nameof(AA_CloudFishWhistle3));
        AA_CloudFishScream = Register(nameof(AA_CloudFishScream));
        AA_CloudFishDeath = Register(nameof(AA_CloudFishDeath));

        values.Add(AA_CloudFishWhistle1);
        values.Add(AA_CloudFishWhistle2);
        values.Add(AA_CloudFishWhistle3);
        values.Add(AA_CloudFishScream);
        values.Add(AA_CloudFishDeath);
    }
    public static void UnregisterValues()
    {
        AA_CloudFishWhistle1 = Unregister(AA_CloudFishWhistle1);
        AA_CloudFishWhistle2 = Unregister(AA_CloudFishWhistle2);
        AA_CloudFishWhistle3 = Unregister(AA_CloudFishWhistle3);
        AA_CloudFishScream = Unregister(AA_CloudFishScream);
        AA_CloudFishDeath = Unregister(AA_CloudFishDeath);

        values.Clear();
    }

    public static SoundID Register(string name)
    { return new SoundID(name, true); }
    public static SoundID Unregister(SoundID type)
    { type?.Unregister(); return null; }

    public static SoundID RandomCloudFishWhistle()
    {
        float random = Random.value;
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
}
