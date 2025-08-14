using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Objects
{
    internal class ScavengerAimBowAnimation : Scavenger.AttentiveAnimation
    {
        public Bow bow;
        public Vector2 lookPos;
        public ScavengerGraphics.ScavengerHand bowHand;
        public ScavengerGraphics.ScavengerHand arrowHand;
        public int holdTimer;
        new public bool Continue = true;

        public ScavengerAimBowAnimation(Scavenger scavenger, Bow bow, Vector2 lookPos) : base(scavenger, null, lookPos, true, Enums.ScavengerAnimationID.AimBow)
        {
            this.bow = bow;
            this.lookPos = lookPos;

            bowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[0];
            arrowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[1];

            holdTimer = 0;
        }

        public override void Update()
        {
            base.Update();

            //scavenger.bodyChunks[2].vel *= 0.5f;
            //scavenger.bodyChunks[2].vel += new Vector2(0, 1) * 30f;

            bool scavHasBow = false;
            foreach (Creature.Grasp grasp in scavenger.grasps)
            {
                if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow)
                {
                    scavHasBow = true;
                    this.bow = bow;
                }
            }

            if (!scavHasBow)
            { Continue = false; }
            else
            {
                this.lookPos = scavenger.lookPoint;
            }
        }
    }
    internal class ScavengerHoldBowAnimation : Scavenger.ScavengerAnimation
    {
        public Bow bow;
        public Vector2 lookPos;
        public int holdTimer;

        public ScavengerHoldBowAnimation(Scavenger scavenger, Bow bow, Vector2 lookPos) : base(scavenger, Enums.ScavengerAnimationID.HoldBow)
        {
            this.bow = bow;
            this.lookPos = lookPos;

            holdTimer = 0;
        }

        public override void Update()
        {
            base.Update();
            if (bow != null)
            {
                lookPos = bow.firstChunk.pos + bow.aimDirection * 50;
            }
        }
    }
}
