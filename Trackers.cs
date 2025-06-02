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

            lifetime--;
            if (lifetime == 0) { Destroy(); }
            
            if (weapon is Spear loadedSpear)
            {
                loadedSpear.setRotation = shootDir;
                loadedSpear.rotation = shootDir;
                loadedSpear.lastRotation = shootDir;

                IntVector2 contactPoint = loadedSpear.firstChunk.ContactPoint;
                if (contactPoint.x != 0 || contactPoint.y != 0)
                {
                    if (loadedSpear is ExplosiveSpear)
                    { (loadedSpear as ExplosiveSpear).Ignite(); }
                    if (Methods.Methods.CanSpearStick(loadedSpear))
                    {
                        IntVector2 newRotationInt = new((int)Mathf.Round(shootDir.x), (int)Mathf.Round(shootDir.y));
                        Vector2 newRotationFloat = newRotationInt.ToVector2();

                        loadedSpear.throwDir = newRotationInt;
                        loadedSpear.firstFrameTraceFromPos = loadedSpear.firstChunk.pos + newRotationFloat * 20f;
                        loadedSpear.setRotation = newRotationFloat;
                        loadedSpear.rotation = newRotationFloat;
                        loadedSpear.lastRotation = newRotationFloat;

                        loadedSpear.stuckInWall = new Vector2?(loadedSpear.room.MiddleOfTile(loadedSpear.firstChunk.pos));
                        loadedSpear.vibrate = 10;
                        loadedSpear.ChangeMode(Weapon.Mode.StuckInWall);
                        loadedSpear.room.PlaySound(SoundID.Spear_Stick_In_Wall, loadedSpear.firstChunk.pos);
                        loadedSpear.firstChunk.collideWithTerrain = false;

                        if (Math.Abs(shootDir.x) > Math.Abs(shootDir.y))
                        { loadedSpear.abstractSpear.stuckInWallCycles = 3; }
                        else
                        { loadedSpear.abstractSpear.stuckInWallCycles = -3; }
                    }
                    else
                    { loadedSpear.HitWall(); }
                    Destroy();
                }
                else
                {
                    if (lifetime > 45)
                    { loadedSpear.firstChunk.vel = shootDir * force; }
                    else
                    { shootDir = loadedSpear.firstChunk.vel.normalized; }
                }
                
                if (loadedSpear.mode == Weapon.Mode.Free || 
                    loadedSpear.mode == Weapon.Mode.OnBack ||
                    loadedSpear.mode == Weapon.Mode.StuckInCreature)
                { Destroy(); }

                //room.AddObject(new ColoredShapes.Rectangle(room, loadedSpear.firstChunk.pos, 0.1f, 20f, Custom.VecToDeg(shootDir), new(1f, 0f, 0f), 1));
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
            { //Destroy(); 
            }
            else { lifeTime++; }

            if (limb.reachedSnapPosition || limb.mode != Limb.Mode.HuntAbsolutePosition)
            { Destroy(); 
            }
        }
    }
}
