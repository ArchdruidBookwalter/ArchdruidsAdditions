using ArchdruidsAdditions.Objects.PhysicalObjects.Items;

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

            if (scavenger.graphicsModule != null)
            {
                bowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[0];
                arrowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[1];
            }

            holdTimer = 0;
        }

        public override void Update()
        {
            base.Update();

            if (scavenger.graphicsModule != null)
            {
                bowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[0];
                arrowHand = (scavenger.graphicsModule as ScavengerGraphics).hands[1];

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
    }
}
