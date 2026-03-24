using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Data;
using UnityEngine;
using HUD;

namespace ArchdruidsAdditions.Objects.HUDObjects;

public class SpiceMeter
{
    public PlayerData.AAPlayerState playerState;
    public FoodMeter foodMeter;
    public Player player;

    public SpiceCircle[] circles;
    public int pulseCounter = 0;
    public float pulseAmount = 0;
    public float pulseLength = 0;

    public int foodPips = 0;
    public int maxPips = 0;
    public int spicePips = 0;

    public int activeSpicePipes = 0;

    public SpiceMeter(HUD.HUD hud, int maxFood)
    {
        player = hud.owner as Player;
        playerState = PlayerData.GetPlayerStateFromAbstractCreature(player.abstractCreature);

        circles = new SpiceCircle[maxFood];
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = new(hud, this);
        }
    }

    public void Update(FoodMeter foodMeter)
    {
        this.foodMeter = foodMeter;

        foodPips = player.FoodInStomach;
        maxPips = player.MaxFoodInStomach;
        spicePips = playerState.spiceAmount;

        for (int i = 0; i < circles.Count(); i++)
        {
            FoodMeter.MeterCircle foodCircle = foodMeter.circles[i];
            SpiceCircle spiceCircle = circles[i];

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

            if (spiceCircle.hide)
            {
                if (spiceCircle.fade > 0f)
                { spiceCircle.fade -= 0.1f; }
                else if (spiceCircle.fade != 0f)
                { spiceCircle.fade = 0f; }
            }
            else
            {
                if (spiceCircle.fade < 1f)
                { spiceCircle.fade += 0.1f; }
                else if (spiceCircle.fade != 1f)
                { spiceCircle.fade = 1f; }
            }
        }

        pulseLength = 50;
        pulseCounter++;
        if (pulseCounter > pulseLength * 2)
        {
            pulseCounter = 1;
        }
        pulseAmount = Mathf.PingPong(pulseCounter, pulseLength);
    }

    public class SpiceCircle : HUDCircle
    {
        public SpiceMeter spiceMeter;
        public FoodMeter foodMeter;
        public FoodMeter.MeterCircle foodCircle;
        public bool hide;
        public SpiceCircle(HUD.HUD hud, SpiceMeter spiceMeter) : base(hud, SnapToGraphic.FoodCircleA, hud.fContainers[1], 0)
        {
            this.spiceMeter = spiceMeter;

            hide = true;
        }

        public void Draw(FoodMeter.MeterCircle foodCircle)
        {
            FSprite foodSprite = foodCircle.circles[0].sprite;
            FSprite spiceSprite = sprite;

            float pulse = spiceMeter.pulseAmount / spiceMeter.pulseLength / 10f;

            spiceSprite.x = foodSprite.x;
            spiceSprite.y = foodSprite.y;
            spiceSprite.shader = foodSprite.shader;
            spiceSprite.alpha = Mathf.Min(foodSprite.alpha, fade);
            spiceSprite.scale = foodSprite.scale + pulse + 0.2f;
            spiceSprite.isVisible = foodSprite.isVisible;

            Vector2 pos = foodSprite.GetPosition();
            spiceSprite.x = pos.x + 0.5f;
            spiceSprite.y = pos.y;

            if (spiceSprite.shader == basicShader)
            {
                spiceSprite.element = Futile.atlasManager.GetElementWithName(snapGraphic.ToString());
                spiceSprite.color = new Color(1f, 0f, 0f);
            }
            else
            {
                spiceSprite.element = Futile.atlasManager.GetElementWithName("Futile_White");
                spiceSprite.color = new Color(1f / 255f, foodSprite.color.g, foodSprite.color.b);
            }

            spiceSprite.MoveBehindOtherNode(foodSprite);
        }
    }
}
