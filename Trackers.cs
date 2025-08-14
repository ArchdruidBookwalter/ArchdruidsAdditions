using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using ArchdruidsAdditions.Methods;
using UnityEngine;
using RWCustom;
using DevInterface;

namespace ArchdruidsAdditions;

public class Trackers
{
    public class ThrowTracker : UpdatableAndDeletable
    {
        public Weapon weapon;
        public bool throw360;
        public int lifetime = 50;

        public Creature thrownBy;
        public Vector2 startPos;
        public Vector2 shootDir;
        public Vector2 lastVel;
        public float force;
        public bool eu;
        public bool firstTick = true;

        public int tickCount = 0;

        public ThrowTracker(Weapon weapon, Room room, float force, bool eu)
        {
            this.weapon = weapon;
            this.room = room;
            this.force = force;
            this.eu = eu;
        }

        public void Update()
        {
            tickCount++;
            if (tickCount > 5 && weapon.grabbedBy.Count > 0)
            { Destroy(); }
        }
    }
    public class PlayerWalkSpeedTracker : UpdatableAndDeletable
    {
        public Player player;
        public int speedFactor;
        public int animFrame = 0;
        public int frameCounter = 0;

        public PlayerWalkSpeedTracker(Player player, int speedFactor)
        {
            this.player = player;
            this.speedFactor = speedFactor;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (player.room != room)
            {
                Destroy();
                return;
            }

            frameCounter++;
            if (frameCounter >= speedFactor)
            {
                animFrame++;
                frameCounter = 0;
            }

            player.animationFrame = animFrame;
        }
    }

    public class SpearTracker : UpdatableAndDeletable
    {
        public Spear spear;
        public SpearTracker(Spear spear, Room room)
        {
            this.spear = spear;
            this.room = room;
        }

        public void FastUpdate()
        {
        }
    }
}
