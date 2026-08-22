using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;
using IL.Menu;
using JetBrains.Annotations;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Decoration;

public class RopeObject : UpdatableAndDeletable
{
    public RopeSegment[] ropeSegments;
    public float segmentLength;
    public Vector2 endPos1, endPos2;
    public bool stuck1, stuck2;
    public int startupTimer;
    public bool decorative;

    public int StartIndex
    { get { return 0; } }
    public int EndIndex
    { get { return ropeSegments.Length - 1; } }

    public RopeObject(int segments, float segmentLength, Vector2 endPos1, Vector2 endPos2, bool decorative = false, bool stuck1 = true, bool stuck2 = true)
    {
        ropeSegments = new RopeSegment[segments];
        for (int i = 0; i < segments; i++)
        {
            ropeSegments[i] = new RopeSegment(this)
            {
                pos = Vector2.Lerp(endPos1, endPos2, (float)i / (segments - 1)),
                mass = (i == StartIndex && stuck1) || (i == EndIndex && stuck2) ? 5f : 1f
            };
        }

        this.segmentLength = segmentLength;
        this.endPos1 = endPos1;
        this.endPos2 = endPos2;
        this.decorative = decorative;
        this.stuck1 = stuck1;
        this.stuck2 = stuck2;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (decorative)
        {
            if (startupTimer > 50)
            {
                foreach (RopeSegment segment in ropeSegments)
                { segment.lastPos = segment.pos; }
                return;
            }
            else
            { startupTimer++; }
        }

        foreach (RopeSegment segment in ropeSegments)
        {
            segment.Update(eu);
        }

        AttachEndSegments();
        ConnectSegments(1);
        ConnectSegments(-1);
        AttachEndSegments();

        foreach (RopeSegment segment in ropeSegments)
        {
            Create_Square(room, segment.pos, 2f, 2f, Vec(45), "Red", 0);
        }
    }

    public void AttachEndSegments()
    {
        if (stuck1)
        {
            ropeSegments[StartIndex].AttachToPos(endPos1);
            Create_Square(room, endPos1, 4f, 4f, Vec(45), "Blue", 0);
        }
        if (stuck2)
        {
            ropeSegments[EndIndex].AttachToPos(endPos2);
            Create_Square(room, endPos2, 4f, 4f, Vec(45), "Green", 0);
        }
    }
    public void ConnectSegments(int dir)
    {
        if (dir == 1)
        {
            for (int i = 0; i < ropeSegments.Length; i++)
            {
                ropeSegments[i].ConnectToObject();
                if (i == StartIndex)
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i + 1]);
                }
                else if (i == EndIndex)
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i - 1]);
                }
                else
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i + 1]);
                    ropeSegments[i].ConnectToSegment(ropeSegments[i - 1]);
                }
                ropeSegments[i].ConnectToObject();
            }
        }
        else
        {
            for (int i = EndIndex; i >= 0; i--)
            {
                ropeSegments[i].ConnectToObject();
                if (i == StartIndex)
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i + 1]);
                }
                else if (i == EndIndex)
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i - 1]);
                }
                else
                {
                    ropeSegments[i].ConnectToSegment(ropeSegments[i + 1]);
                    ropeSegments[i].ConnectToSegment(ropeSegments[i - 1]);
                }
                ropeSegments[i].ConnectToObject();
            }
        }
    }
    public virtual void DetachObject(RopeAttachedObject obj)
    { 
    }

    public class RopeSegment
    {
        public RopeObject owner;
        public Vector2 pos, lastPos, vel;
        public float mass;

        public RopeAttachedObject obj;

        public RopeSegment(RopeObject owner)
        {
            this.owner = owner;
        }

        public void Update(bool eu)
        {
            lastPos = pos;
            pos += vel;
            vel *= 0.95f;
            vel.y -= owner.room.gravity * 0.9f;

            obj?.Update();
        }

        public void ConnectToSegment(RopeSegment otherSegment)
        {
            Vector2 dirVec = Custom.DirVec(pos, otherSegment.pos);
            float dist = Custom.Dist(pos, otherSegment.pos);
            float segmentMass = this.mass + (obj != null ? obj.obj.TotalMass : 0f);
            float mass = segmentMass / (segmentMass + otherSegment.mass);

            Vector2 newDir1 = dirVec * Mathf.Max(dist - owner.segmentLength, 0f) * (1f - mass) * 1f;
            Vector2 newDir2 = -dirVec * Mathf.Max(dist - owner.segmentLength, 0f) * mass * 1f;

            pos += newDir1;
            vel += newDir1;
            otherSegment.pos += newDir2;
            otherSegment.vel += newDir2;
        }

        public void ConnectToObject()
        {
            if (obj != null)
            {
                Vector2 dirVec = Custom.DirVec(pos, obj.AttachedChunk.pos);
                float dist = Custom.Dist(pos, obj.AttachedChunk.pos);

                float mass = this.mass / (this.mass + obj.obj.TotalMass);

                Vector2 newDir1 = dirVec * dist * mass * 1f;
                Vector2 newDir2 = -dirVec * dist * (1f - mass) * 1f;

                pos += newDir1;
                vel += newDir1;
                obj.AttachedChunk.pos += newDir2;
                obj.AttachedChunk.vel += newDir2;

                if (dist > 15f)
                {
                    DetachObject();
                }
            }
        }

        public void AttachObject(RopeAttachedObject newObj)
        {
            obj = newObj;
            newObj.obj.firstChunk.HardSetPosition(pos);
        }

        public void DetachObject()
        {
            if (obj != null)
            {
                owner.DetachObject(obj);
            }

            obj = null;
        }

        public void AttachToPos(Vector2 pos)
        {
            this.pos = pos;
            vel *= 0f;
        }
    }
    public class RopeAttachedObject
    {
        public RopeSegment segment;
        public PhysicalObject obj;
        public int chunkIndex;
        public BodyChunk AttachedChunk
        {
            get { return obj.bodyChunks[chunkIndex]; }
        }

        public RopeAttachedObject(RopeSegment segment, PhysicalObject obj, int chunkIndex)
        {
            this.segment = segment;
            this.obj = obj;
            this.chunkIndex = chunkIndex;
        }

        public void Update()
        {
            obj.bodyChunks[chunkIndex].vel *= 0.95f;
        }
    }
}

public class HangingRope : RopeObject
{
    public HangingRope(int segments, float length, Vector2 endPos1, Vector2 endPos2) : base(segments, length, endPos1, endPos2, true, false)
    {
    }
}
