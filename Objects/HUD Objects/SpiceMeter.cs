using System;
using ArchdruidsAdditions.Data;
using HUD;

namespace ArchdruidsAdditions.Objects.HUDObjects;

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
        {
            circles[i] = new(this, (float)i / maxFood);
        }
    }

    public void Update(FoodMeter foodMeter)
    {
        int section = 0;

        this.foodMeter = foodMeter;

        foodPips = player.FoodInStomach;
        maxPips = player.slugcatStats.maxFood;
        PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.abstractCreature.ID.number);
        if (playerState != null)
        {
            spicePips = playerState.spiceAmount;
        }

        if (foodMeter.fade > 0)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].Update();

                if (i < foodMeter.showCount)
                {
                    circles[i].plop = false;
                }
                else if (i < foodMeter.showCount + spicePips)
                {
                    circles[i].plop = true;
                }
                else
                {
                    circles[i].plop = false;
                }

                //FoodMeter.MeterCircle foodCircle = foodMeter.circles[i];
                //Methods.Methods.Create_Text(player.room, player.mainBodyChunk.pos + new Vector2(-50 + (50 * i), -50), foodCircle.circles[0].sprite.shader == foodCircle.circles[0].circleShader, "Red", 1);
                //Methods.Methods.Create_Text(player.room, player.mainBodyChunk.pos + new Vector2(-50 + (50 * i), -70), foodCircle.circles[0].sprite.alpha, "Red", 1);
                //Methods.Methods.Create_Text(player.room, player.mainBodyChunk.pos + new Vector2(-50 + (50 * i), -90), foodCircle.circles[0].sprite.scale, "Red", 1);
            }
        }

        if (pulseTimer < 200)
        {
            pulseTimer++;
        }
        else
        {
            pulseTimer = 0;
        }

        pulse = (Mathf.Cos(2 * Mathf.PI * ((float)pulseTimer / 200)) + 1) * 0.1f;

        /*
        try
        {
            this.foodMeter = foodMeter;

            section = 1;

            if (player != null && playerState != null)
            {
                foodPips = player.FoodInStomach;
                maxPips = player.MaxFoodInStomach;
                spicePips = playerState.spiceAmount;

                section = 2;

                for (int i = 0; i < circles.Count(); i++)
                {
                    FoodMeter.MeterCircle foodCircle = foodMeter.circles[i];
                    SpiceCircle spiceCircle = circles[i];

                    section = 3;

                    if (foodPips >= maxPips - 1 && spicePips > 0)
                    {
                        spiceCircle.hide = true;
                    }
                    else
                    {
                        if (i < foodPips && foodCircle.foodPlopped)
                        {
                            spiceCircle.hide = true;
                        }
                        else if (spicePips > 0 && i < foodPips + spicePips || i < foodPips && !spiceCircle.hide)
                        {
                            spiceCircle.hide = false;
                        }
                        else
                        {
                            spiceCircle.hide = true;
                        }
                    }

                    section = 4;

                    if (spiceCircle.hide)
                    {
                        if (spiceCircle.circleAlpha > 0f)
                        { spiceCircle.circleAlpha -= 0.1f; }
                        else if (spiceCircle.circleAlpha != 0f)
                        { spiceCircle.circleAlpha = 0f; }
                    }
                    else
                    {
                        if (spiceCircle.circleAlpha < 1f)
                        { spiceCircle.circleAlpha += 0.1f; }
                        else if (spiceCircle.circleAlpha != 1f)
                        { spiceCircle.circleAlpha = 1f; }
                    }
                }

                section = 5;

                pulseLength = 50;
                pulseCounter++;
                if (pulseCounter > pulseLength * 2)
                {
                    pulseCounter = 1;
                }
                pulseAmount = Mathf.PingPong(pulseCounter, pulseLength);
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "SPICEMETER_UPDATE", section);
        }
        */
    }

    public void Draw(FoodMeter meter, float timeStacker)
    {
        int section = 0;

        try
        {

            section = 1;

        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "SPICEMETER_DRAW", section);
        }
    }

    public class SpiceMeterCircle
    {
        public SpiceMeter meter;
        public static MaterialPropertyBlock propertyBlock = new();
        public FSprite circle;
        public float rad;

        public float circleAlpha = 0f;
        public bool plopped = false;
        public bool plop = false;

        public FShader customShader;
        public Color spiceColor;

        public SpiceMeterCircle(SpiceMeter meter, float startRad)
        {
            this.meter = meter;
            customShader = meter.hud.rainWorld.Shaders["ArchAdds.CustomVectorCircle"];

            circle = new("Futile_White", true)
            {
                scale = 2f,
                color = new(1f, 0f, 0f),
            };
            meter.hud.fContainers[1].AddChild(circle);

            rad = startRad;
            spiceColor = Custom.HSL2RGB(0f, 0.6f, 0.5f);
        }
        public void Update()
        {
            if (!plop && plopped)
            {
                circleAlpha -= 0.1f;
                if (circleAlpha < 0)
                {
                    circleAlpha = 0;
                    plopped = false;
                }
            }

            if (plop && !plopped)
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
            try
            {
                circle.x = foodCircle.sprite.x;
                circle.y = foodCircle.sprite.y;
                circle.color = spiceColor;
                circle.MoveBehindOtherNode(foodCircle.sprite);

                if (foodCircle.sprite.shader == foodCircle.circleShader || foodCircle.sprite.alpha == 1)
                {
                    circle.element = Futile.atlasManager.GetElementWithName("Futile_White");
                    circle.shader = customShader;

                    if (foodCircle.sprite.shader == foodCircle.circleShader)
                    {
                        circle.scale = foodCircle.sprite.scale + meter.pulse + 0.3f;
                        circle.alpha = Mathf.Min(foodCircle.visible ? 1f : 0f, foodCircle.sprite.alpha, circleAlpha);
                    }
                    else
                    {
                        circle.scale = (foodCircle.snapRad / 8f) + meter.pulse + 0.3f;
                        circle.alpha = Mathf.Min(foodCircle.visible ? 1f : 0f, foodCircle.snapThickness / foodCircle.snapRad, circleAlpha);
                    }
                }
                else
                {
                    circle.element = Futile.atlasManager.GetElementWithName(foodCircle.snapGraphic.ToString());
                    circle.shader = foodCircle.basicShader;

                    circle.scale = foodCircle.sprite.scale + meter.pulse + 0.2f;
                    circle.alpha = Mathf.Min(foodCircle.visible ? 1f : 0f, foodCircle.sprite.alpha, circleAlpha);
                }
            }
            catch (Exception e)
            {
                Methods.Methods.Log_Exception(e, "SPICEMETERCIRCLE_DRAW", 0);
            }
        }
    }
}
