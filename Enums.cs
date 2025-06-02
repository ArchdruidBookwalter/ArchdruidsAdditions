using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using DevInterface;

namespace ArchdruidsAdditions.Enums;

public static class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType Bow = new("Bow", true);
    public static AbstractPhysicalObject.AbstractObjectType ScarletFlowerBulb = new("ScarletFlowerBulb", true);
    public static AbstractPhysicalObject.AbstractObjectType ParrySword = new("ParrySword", true);
    public static AbstractPhysicalObject.AbstractObjectType Potato = new("Potato", true);
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
public static class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType Bow = new("Bow", true);
    public static SLOracleBehaviorHasMark.MiscItemType ScarletFlowerBulb = new("ScarletFlowerBulb", true);
    public static SLOracleBehaviorHasMark.MiscItemType ParrySword = new("ParrySword", true);
    public static SLOracleBehaviorHasMark.MiscItemType Potato = new("Potato", true);

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
public static class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type Bow = new("Bow", true);
    public static PlacedObject.MultiplayerItemData.Type ScarletFlowerBulb = new("ScarletFlowerBulb", true);
    public static PlacedObject.MultiplayerItemData.Type ParrySword = new("ParrySword", true);
    public static PlacedObject.MultiplayerItemData.Type Potato = new("Potato", true);
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
public static class PlacedObjectType
{
    public static PlacedObject.Type ScarletFlower = new("ScarletFlower", true);
    public static PlacedObject.Type Potato = new("Potato", true);
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
public static class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID Bow = new("Bow", true);
    public static MultiplayerUnlocks.SandboxUnlockID ScarletFlowerBulb = new("ScarletFlowerBulb", true);
    public static MultiplayerUnlocks.SandboxUnlockID ParrySword = new("ParrySword", true);
    public static MultiplayerUnlocks.SandboxUnlockID Potato = new("Potato", true);
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
