using EffExt;

namespace ArchdruidsAdditions.Objects;

public static class RandomShells
{
    public static void EffectSpawner(Room room, EffectExtraData data, bool firstTimeRealized)
    {
    }

    public static void ActuallySpawnEffect(Room room, RoomSettings.RoomEffect effect, bool firstTimeRealized)
    {
        if (firstTimeRealized)
        {
            int shells = Mathf.RoundToInt((room.abstractRoom.size.x + room.abstractRoom.size.y) * effect.amount);

            for (int i = 0; i < shells; i++)
            {
                IntVector2 randomTile = room.RandomTile();

                for (int j = randomTile.y; j > 0; j--)
                {
                    IntVector2 testTile = new(randomTile.x, j);
                    if (!room.HasAnySolid(testTile) && room.HasAnySolid(testTile.x, testTile.y - 1))
                    {
                        AbstractPhysicalObject newShell = new(room.world, AbstractObjectType.CrabShell, null, room.GetWorldCoordinate(testTile), room.game.GetNewID());
                        room.abstractRoom.AddEntity(newShell);
                        break;
                    }
                }
            }
        }
    }
}
