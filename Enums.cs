using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;

namespace ArchdruidsAdditions.Enums;

public static class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType ScarletFlower = new("ScarletFlower", true);
    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
    }
}
public static class DevObjectCategories
{
    public static ObjectsPage.DevObjectCategories ArchdruidsAdditions = new("ArchdruidsAdditions", true);
    public static void UnregisterValues()
    {
        if (ArchdruidsAdditions != null)
        {
            ArchdruidsAdditions.Unregister();
            ArchdruidsAdditions = null;
        }
    }
}
public static class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType ScarletFlower = new("ScarletFlower", true);
    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
    }
}
public static class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type ScarletFlower = new("ScarletFlower", true);
    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
    }
}
public static class PlacedObjectType
{
    public static PlacedObject.Type ScarletFlower = new("ScarletFlower", true);
    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
    }
}
public static class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID ScarletFlower = new("ScarletFlower", true);
    public static void UnregisterValues()
    {
        if (ScarletFlower != null)
        {
            ScarletFlower.Unregister();
            ScarletFlower = null;
        }
    }
}
