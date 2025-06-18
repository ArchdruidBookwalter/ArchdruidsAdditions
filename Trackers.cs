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
        public float force;
        public bool eu;

        public ThrowTracker(Weapon weapon, Room room, float force, bool eu)
        {
            this.weapon = weapon;
            this.room = room;
            this.force = force;
            this.eu = eu;
        }
        bool oneShot;
        public override void Update(bool eu)
        {
            base.Update(eu);
            
            if (weapon is Spear spear)
            {
                spear.rotation = shootDir;
                if (shootDir.x >= 0.5f || shootDir.x <= -0.5f)
                { spear.throwDir = new IntVector2(Math.Sign(shootDir.x), 0); }
                else
                { spear.throwDir = new IntVector2(0, Math.Sign(shootDir.y)); }

                if (spear.firstChunk.ContactPoint.x != 0 || spear.firstChunk.ContactPoint.y != 0)
                {
                    Debug.Log("Spear Hit Something?");
                    if (Custom.Angle(spear.throwDir.ToVector2(), spear.rotation) > 10f)
                    {
                        Debug.Log("Spear Bounced!");
                        spear.HitWall();
                    }
                    Destroy();
                }
                else
                { spear.firstChunk.vel = shootDir * 60f; }
            }
        }
    }
    public class LimbTracker : UpdatableAndDeletable
    {
        public Limb limb;
        public Vector2 huntPos;
        public float extraSpeed;
        public bool decay;
        int lifeTime = 0;
        int maxLifeTime = 10;

        public LimbTracker(Limb limb, Room room, Vector2 huntPos, float extraSpeed, bool decay)
        {
            this.limb = limb;
            this.room = room;
            this.huntPos = huntPos;
            this.extraSpeed = 10f;
            this.decay = decay;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (lifeTime > maxLifeTime)
            { }
            else { lifeTime++; }

            if (limb.reachedSnapPosition || limb.mode != Limb.Mode.HuntAbsolutePosition)
            { Destroy(); }
        }
    }
}
