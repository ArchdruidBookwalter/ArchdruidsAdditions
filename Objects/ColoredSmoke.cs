using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArchdruidsAdditions.Objects;

public class ColoredVultureSmoke : Smoke.NewVultureSmoke
{
    public Color startColor;
    public Color endColor;
    public ColoredVultureSmoke(Room room, Vector2 pos, Vulture vulture, Color startColor, Color endColor) : base(room, pos, vulture)
    {
        this.startColor = startColor;
        this.endColor = endColor;
    }
    public override SmokeSystemParticle CreateParticle()
    {
        return new ColoredVultureSmokeSegment(startColor, endColor);
    }
    new public void EmitSmoke(Vector2 vel, float power)
    {
        ColoredVultureSmokeSegment newVultureSmokeSegment = AddParticle(pos, vel * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, Random.value), Mathf.Lerp(60f, 100f, Random.value))) as ColoredVultureSmokeSegment;
        if (newVultureSmokeSegment != null)
        {
            newVultureSmokeSegment.power = power;
            newVultureSmokeSegment.startColor = startColor;
            newVultureSmokeSegment.endColor = endColor;
        }
    }
    public class ColoredVultureSmokeSegment : Smoke.NewVultureSmoke.NewVultureSmokeSegment
    {
        public Color startColor;
        public Color endColor;
        public ColoredVultureSmokeSegment(Color startColor, Color endColor)
        {
            this.startColor = startColor;
            this.endColor = endColor;
        }
        public override Color MyColor(float timeStacker)
        {
            float x = Mathf.InverseLerp(1f, 5f + 15f * power, age + timeStacker);
            return Color.Lerp(startColor, endColor, x);
        }
    }
}
