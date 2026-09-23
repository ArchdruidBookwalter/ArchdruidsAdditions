using System;
using HUD;

namespace ArchdruidsAdditions.Objects;

public class SpiceMeter
{
    public PlayerData.AAPlayerState playerState;
    public FoodMeter foodMeter;
    public Player player;
    public HUD.HUD hud;

    public int pulseTimer = 0;
    public float pulse;

    public int foodPips = 0;
    public int maxPips = 0;
    public int spicePips = 0;

    public int activeSpicePipes = 0;

    public SpiceMeterCircle[] circles;
    public float[] vectorCircleRads;

    public SpiceMeter(HUD.HUD hud, int maxFood)
    {
        player = hud.owner as Player;
        this.hud = hud;
        playerState = PlayerData.GetPlayerState(player.abstractCreature.ID.number);

        circles = new SpiceMeterCircle[maxFood];
        for (int i = 0; i < circles.Length; i++)
        { circles[i] = new(this, (float)i / maxFood); }
    }

    public void Update(FoodMeter foodMeter)
    {
        this.foodMeter = foodMeter;

        foodPips = player.FoodInStomach;
        maxPips = player.slugcatStats.maxFood;
        PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.abstractCreature.ID.number);
        if (playerState != null)
        { spicePips = playerState.spiceAmount; }

        if (foodMeter.fade > 0)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();

                if (i < foodMeter.showCount)
                { circles[i].plop = false; }
                else if (i < foodMeter.showCount + spicePips)
                { circles[i].plop = true; }
                else
                { circles[i].plop = false; }
            }
        }

        if (pulseTimer < 200)
        { pulseTimer++; }
        else
        { pulseTimer = 0; }

        pulse = (Mathf.Cos(2 * Mathf.PI * ((float)pulseTimer / 200)) + 1) * 0.1f;
    }

    public class SpiceMeterCircle
    {
        public SpiceMeter meter;
        public FCustomShaderSprite newCircle;

        public float circleAlpha = 0f;
        public bool plopped = false;
        public bool plop = false;

        public FShader customShader;
        public Color spiceColor;

        public SpiceMeterCircle(SpiceMeter meter, float startRad)
        {
            this.meter = meter;
            customShader = meter.hud.rainWorld.Shaders["ArchAdds.CustomVectorCircle"];

            newCircle = new("Futile_White")
            {
                scale = 2f,
                color = new(1f, 0f, 0f),
                shader = customShader,
            };
            meter.hud.fContainers[1].AddChild(newCircle);

            spiceColor = Custom.HSL2RGB(0f, 0.6f, 0.5f);
        }
        public void Update()
        {
            if (!plop && circleAlpha != 0)
            {
                circleAlpha -= 0.1f;
                if (circleAlpha < 0)
                {
                    circleAlpha = 0;
                    plopped = false;
                }
            }

            if (plop && circleAlpha != 1)
            {
                circleAlpha += 0.1f;
                if (circleAlpha > 1)
                {
                    circleAlpha = 1;
                    plopped = true;
                }
            }
        }
        public void Draw(HUDCircle foodCircle, float timeStacker)
        {
            float section = 0;

            try
            {
                newCircle.x = foodCircle.sprite.x;
                newCircle.y = foodCircle.sprite.y;
                newCircle.color = spiceColor;
                newCircle.alpha = Mathf.Min(circleAlpha, Mathf.Lerp(foodCircle.lastFade, foodCircle.fade, timeStacker));
                newCircle.MoveBehindOtherNode(foodCircle.sprite);

                if (foodCircle.sprite.shader == foodCircle.circleShader)
                {
                    newCircle.scale = foodCircle.sprite.scale + meter.pulse + 0.3f;
                    newCircle.SetUVs(new Vector2(1f - foodCircle.sprite.alpha, 0), 3);
                }
                else
                {
                    newCircle.scale = foodCircle.snapRad / 8f + meter.pulse + 0.3f;
                    newCircle.SetUVs(new Vector2(1f - foodCircle.snapThickness / foodCircle.snapRad, 0), 3);
                }

                section = 1;
            }
            catch (Exception e)
            {
                Log_Exception(e, "SPICEMETERCIRCLE_DRAW", section);
            }
        }
    }
}
