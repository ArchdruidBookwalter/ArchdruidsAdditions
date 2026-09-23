using System.Collections.Generic;

namespace ArchdruidsAdditions.Data;

public static class MiscData
{
    #region Shaders

    public static int CircleFade;
    public static int CircleColor;

    #endregion

    public static bool stopAbsStkDeactivation;

    public static Dictionary<Room, List<RopeObject>> ropeObjects = [];

    //public static Dictionary<Room, List<CollisionBox>> boxesInRooms = [];

    public static Dictionary<Room, CollisionBoxHandler> boxHandlers = [];
}
