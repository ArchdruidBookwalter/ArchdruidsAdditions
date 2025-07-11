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

        public ScavengerAimBowAnimation(Scavenger scavenger, Bow bow, Vector2 lookPos) : base(scavenger, null, lookPos, true, Enums.ScavengerAnimationID.AimBow)
        {
            this.bow = bow;
            this.lookPos = lookPos;

            bowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[0];
            arrowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[1];
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
    internal class ScavengerHoldBowAnimation : Scavenger.ScavengerAnimation
    {
        public Bow bow;
        public Vector2 lookPos;

        public ScavengerHoldBowAnimation(Scavenger scavenger, Bow bow, Vector2 lookPos) : base(scavenger, Enums.ScavengerAnimationID.HoldBow)
        {
            this.bow = bow;
            this.lookPos = lookPos;
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
