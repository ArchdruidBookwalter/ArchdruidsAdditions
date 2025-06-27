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
    }
}
