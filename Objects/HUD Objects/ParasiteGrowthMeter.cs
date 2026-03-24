using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUD;
using UnityEngine;
using RWCustom;
using Menu;

namespace ArchdruidsAdditions.Objects.HUDObjects;

public class ParasiteGrowthMeter : HudPart
{
    public Vector2 lastPos;
    public Vector2 pos;
    public MeterCircle[] circles;
    public float fade;
    public float lastFade;

    public int maxPoints = 6;
    public int points = 0;

    public int startFadeTimer = 0;
    public int startFadeTime = 50;
    public int pointFade = 0;
    public int maxPointFade = 100;

    public float pulseAmount;
    public float pulseVel;

    public ParasiteGrowthMeter(HUD.HUD hud) : base(hud)
    {
        pos = new Vector2(Mathf.Max(50f, hud.rainWorld.screenSize.x - 50), Mathf.Max(25f, hud.rainWorld.screenSize.y / 2));

        circles = new MeterCircle[maxPoints];
        for (int i = 0; i < circles.Length; i++)
        { circles[i] = new MeterCircle(this); }
    }
    public override void Update()
    {
        if ((hud.owner as SleepAndDeathScreen).RippleLadderMode)
        {
            pos = new Vector2(0f, 999f);
        }
        else
        {
            pos = new Vector2(Mathf.Max(50f, hud.rainWorld.screenSize.x - 50), Mathf.Max(25f, (hud.rainWorld.screenSize.y / 2) - 15f * maxPoints));
        }

        for (int i = 0; i < circles.Length; i++)
        {
            if (i == points)
            {
                circles[i].FillingUpdate();
            }
            else if (i < points)
            {
                circles[i].FilledUpdate();
            }
            else
            {
                circles[i].EmptyUpdate();
            }
        }

        if (startFadeTimer < startFadeTime)
        { startFadeTimer++; }
        else if (pointFade < maxPointFade)
        { pointFade++; }

        pulseVel += pulseAmount > 5 ? -0.01f : 0.01f;
        pulseAmount += pulseVel;
    }
    public override void Draw(float timestacker)
    {
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].Draw(timestacker);
        }
    }
    public override void ClearSprites()
    {
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i].ClearSprites();
        }
    }

    public class MeterCircle
    {
        public ParasiteGrowthMeter meter;
        public HUDCircle[] circles;
        public FSprite glow;

        public MeterCircle(ParasiteGrowthMeter meter)
        {
            this.meter = meter;

            circles = new HUDCircle[2];

            circles[0] = new HUDCircle(meter.hud, HUDCircle.SnapToGraphic.FoodCircleA, meter.hud.fContainers[1], 0)
            {
                rad = 0f,
                lastRad = 0f,
                color = 1
            };

            circles[1] = new HUDCircle(meter.hud, HUDCircle.SnapToGraphic.FoodCircleB, meter.hud.fContainers[1], 0)
            {
                rad = 0f,
                lastRad = 0f,
                color = 1
            };

            glow = new FSprite("Futile_White", true)
            {
                shader = meter.hud.rainWorld.Shaders["FlatLight"],
                color = new(1f, 0f, 0f),
                scale = 6f
            };
            meter.hud.fContainers[1].AddChild(glow);
        }

        public void FilledUpdate()
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
            }

            circles[0].rad = circles[0].snapRad + meter.pulseAmount * 0.2f;
            circles[1].rad = circles[1].snapRad;

            circles[0].fade = 1f;
            circles[1].fade = 1f;

            glow.alpha = 0.2f;
        }
        public void EmptyUpdate()
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
            }

            circles[0].rad = circles[0].snapRad;
            circles[1].rad = circles[1].snapRad;

            circles[0].fade = 1f;
            circles[1].fade = 0f;

            glow.alpha = 0f;
        }
        public void FillingUpdate()
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();
            }

            float fade = (float)meter.pointFade / meter.maxPointFade;

            circles[0].rad = circles[0].snapRad + meter.pulseAmount * 0.2f;
            circles[1].rad = circles[1].snapRad;

            circles[0].fade = 1f;
            circles[1].fade = fade;

            glow.alpha = fade / 5;
        }

        public void Draw(float timestacker)
        {
            Vector2 drawPos = meter.pos + Vector2.up * meter.circles.IndexOf(this) * 30;

            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Draw(timestacker);

                circles[i].sprite.x = drawPos.x;
                circles[i].sprite.y = drawPos.y;

                circles[i].sprite.MoveInFrontOfOtherNode(glow);
            }

            glow.SetPosition(drawPos);
        }
        public void ClearSprites()
        {
            HUDCircle[] clearCircles = circles;
            for (int i = 0; i < clearCircles.Length; i++)
            {
                clearCircles[i].ClearSprite();
            }
        }
    }
}
