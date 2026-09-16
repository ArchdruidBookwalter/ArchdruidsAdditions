using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects.Decoration;
using ArchdruidsAdditions.Objects.Physical_Objects;

namespace ArchdruidsAdditions.Data;

public static class MiscData
{
    #region Shaders

    public static int CircleFade;
    public static int CircleColor;

    #endregion

    public static bool stopAbsStkDeactivation;

    public static List<RopeObject> ropeObjects = [];

    //public static Dictionary<Room, List<CollisionBox>> boxesInRooms = [];

    public static Dictionary<Room, CollisionBoxHandler> boxHandlers = [];
}
