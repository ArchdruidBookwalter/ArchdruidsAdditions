using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EffExt;
using MoreSlugcats;
using UnityEngine;

namespace ArchdruidsAdditions.Effects;

public static class LightRodPowerEffect
{
    public static void EffectSpawner(Room room, EffectExtraData data, bool firstTimeRealized)
    {
        room.AddObject(new LightRodPower(room, data));
        //UnityEngine.Debug.Log("Effect Loaded!");
    }

    public class LightRodPower : UpdatableAndDeletable
    {
        public float startingPower;
        public float currentPower;
        public float goalPower;
        public int runTime;
        public int offTime;

        public int driftMode;
        public float driftStrength;
        public float resetChance;
        public float resetCooldown;

        public LightRodPower(Room room, EffectExtraData data)
        {
            startingPower = data.Amount;
            currentPower = data.Amount;
            goalPower = data.Amount;
            driftMode = data.GetInt("DriftMode");
            driftStrength = data.GetFloat("DriftStrength");
            resetChance = data.GetFloat("ResetChance");
            resetCooldown = data.GetFloat("ResetCooldown");
        }
        public override void Update(bool eu)
        {
            runTime++;

            if (driftStrength >= 0.001 && UnityEngine.Random.value < 0.1f)
            {
                float factor;
                switch (driftMode)
                {
                    case 1:
                        factor = 1f;
                        break;
                    case 2:
                        factor = -1f;
                        break;
                    default:
                        factor = UnityEngine.Random.Range(-1f, 1f);
                        break;
                }
                goalPower += driftStrength * factor;
                goalPower = Mathf.Clamp(goalPower, 0f, 1f);
            }

            if (resetChance > 0.01 && runTime > resetCooldown && UnityEngine.Random.value < resetChance)
            {
                goalPower = startingPower;
                runTime = 0;
            }

            currentPower = Mathf.Lerp(currentPower, goalPower, 0.1f);

            //UnityEngine.Debug.Log("Effect Updated.");
        }
    }

    public class RoomSpecificPowerController : FiltrationPowerController
    {
        Room room;
        public RoomSpecificPowerController(World world, Room room) : base(world)
        {
            this.room = room;
        }
    }
}
