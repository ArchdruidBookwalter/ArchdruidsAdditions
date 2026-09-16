using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using DevInterface;
using JetBrains.Annotations;

namespace ArchdruidsAdditions.Objects.Decoration;

public class RopeObject
{
    public UpdatableAndDeletable owner;
    //public Room room;
    public RopeSegment[] ropeSegments;
    public float ropeLength, segmentLength;
    public Vector2 endPos1, endPos2;
    public bool stuck1, stuck2;
    public int startupTimer;
    public bool freeze;
    public float rigidity;

    public float adjustSegmentLength;

    public int StartIndex
    { get { return 0; } }
    public int EndIndex
    { get { return ropeSegments.Length - 1; } }

    public RopeObject(UpdatableAndDeletable owner, float segmentLength, float segmentMass, float elasticity, Vector2 endPos1, Vector2 endPos2, float rigidity = 1f, bool freezeOnInit = false, bool stuck1 = true, bool stuck2 = true)
    {
        float ropeLength = Custom.Dist(endPos1, endPos2) + (stuck1 && stuck2 ? elasticity * 300 : 0);

        adjustSegmentLength = segmentLength * 0.1f * (ropeLength / 200);

        int numOfSegments = Math.Max(2, Mathf.RoundToInt(ropeLength / segmentLength));

        ropeSegments = new RopeSegment[numOfSegments];

        if (freezeOnInit)
        {
            float floor = Mathf.Sin(0.25f * Mathf.PI);
            for (int i = 0; i < numOfSegments; i++)
            {
                float segmentPos = (float)i / (numOfSegments - 1);

                float arcPos = Mathf.Sin(Mathf.Lerp(0.25f, 0.75f, segmentPos) * Mathf.PI) - floor;

                ropeSegments[i] = new RopeSegment(this)
                {
                    pos = Vector2.Lerp(endPos1, endPos2, segmentPos) + Vector2.down * arcPos * elasticity * 300,
                    mass = segmentMass //((i == StartIndex && stuck1) || (i == EndIndex && stuck2)) ? segmentMass * 5f : segmentMass
                };

                ropeSegments[i].lastPos = ropeSegments[i].pos;
            }
        }
        else
        {
            for (int i = 0; i < numOfSegments; i++)
            {
                float segmentPos = (float)i / (numOfSegments - 1);

                float mass = Mathf.Lerp(10f, 0.5f, segmentPos);

                ropeSegments[i] = new RopeSegment(this)
                {
                    pos = Vector2.Lerp(endPos1, endPos2, segmentPos),
                    mass = segmentMass //mass
                };
            }
        }

        this.owner = owner;
        this.ropeLength = ropeLength;
        this.segmentLength = segmentLength;
        this.endPos1 = endPos1;
        this.endPos2 = endPos2;
        freeze = freezeOnInit;
        this.stuck1 = stuck1;
        this.stuck2 = stuck2;
        this.rigidity = rigidity;
    }

    public virtual void Update()
    {
        //Create_Text(owner.room, ropeSegments[0].pos, "ACTUAL LENGTH: " + Custom.Dist(ropeSegments[0].pos, ropeSegments[EndIndex].pos), "Red", 0);
        //Create_Text(owner.room, ropeSegments[0].pos + new Vector2(0f, -20f), "ROPE LENGTH: " + ropeLength, "Yellow", 0);

        if (owner == null || owner.room == null)
        {
            if (MiscData.ropeObjects.Contains(this))
            { MiscData.ropeObjects.Remove(this); }
        }
        else
        {
            if (!MiscData.ropeObjects.Contains(this))
            { MiscData.ropeObjects.Add(this); }
        }

        if (freeze)
        {
            return;
        }

        foreach (RopeSegment segment in ropeSegments)
        {
            segment.Update();
        }

        AttachEndSegments();
        ConnectSegments(1);
        ConnectSegments(-1);
        AttachEndSegments();

        float totalLength = 0;
        for (int i = 1; i < ropeSegments.Length; i++)
        {
            totalLength += Custom.Dist(ropeSegments[i - 1].pos, ropeSegments[i].pos);
        }

        float averageSegmentLength = totalLength / ropeSegments.Length;

        //Create_Text(owner.room, ropeSegments[EndIndex].pos + new Vector2(-100f, 0f), "AVERAGE SEGMENT LENGTH: " + averageSegmentLength, "Red", 0);
        //Create_Text(owner.room, ropeSegments[EndIndex].pos + new Vector2(-100f, -20f), "DESIRED SEGMENT LENGTH: " + segmentLength, "Red", 0);
    }

    public virtual void AttachEndSegments()
    {
        if (stuck1)
        {
            ropeSegments[StartIndex].AttachToPos(endPos1);
            Create_Square(owner.room, endPos1, 4f, 4f, Vec(45), "Blue", 0);
        }
        if (stuck2)
        {
            ropeSegments[EndIndex].AttachToPos(endPos2);
            Create_Square(owner.room, endPos2, 4f, 4f, Vec(45), "Green", 0);
        }
    }
    public virtual void ConnectSegments(int dir)
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
                    ropeSegments[i].ConnectToSegment(ropeSegments[i - 1]);
                    ropeSegments[i].ConnectToSegment(ropeSegments[i + 1]);
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

        public bool StartSegment
        { get { return owner.ropeSegments.IndexOf(this) == 0; } }

        public bool EndSegment
        { get { return owner.ropeSegments.IndexOf(this) == owner.EndIndex; } }

        public bool Stuck
        { get { return (StartSegment && owner.stuck1) || (EndSegment && owner.stuck2); } }

        public RopeAttachedObject obj;

        public RopeSegment(RopeObject owner)
        {
            this.owner = owner;
        }

        public void Update()
        {
            lastPos = pos;
            pos += vel;
            //vel *= 0.99f;

            if (owner.owner.room.PointSubmerged(pos))
            {
                vel.y += 0.05f;
                vel += Custom.RNV() * 0.1f;
            }
            else
            {
                vel.y -= owner.owner.room.gravity * 0.9f;
            }

            obj?.Update();
        }

        public void ConnectToSegment(RopeSegment otherSegment)
        {
            Vector2 dirVec = Custom.DirVec(pos, otherSegment.pos);
            float dist = Custom.Dist(pos, otherSegment.pos);
            float segmentMass = this.mass + (obj != null ? obj.obj.TotalMass : 0f);
            float mass = segmentMass / (segmentMass + otherSegment.mass);

            float adjustSegmentLength = pos.y - Mathf.Min(owner.ropeSegments[0].pos.y, owner.ropeSegments[owner.EndIndex].pos.y);

            float segmentLength = owner.segmentLength - adjustSegmentLength * 0.008f;

            Vector2 newDir1 = dirVec * Mathf.Max(dist - segmentLength, 0f) * (1f - mass) * owner.rigidity;
            Vector2 newDir2 = -dirVec * Mathf.Max(dist - segmentLength, 0f) * mass * owner.rigidity;

            if (!Stuck)
            {
                pos += newDir1;
                vel += newDir1;
            }

            if (!otherSegment.Stuck)
            {
                otherSegment.pos += newDir2;
                otherSegment.vel += newDir2;
            }
        }

        public void ConnectToObject()
        {
            if (obj != null && obj.obj != null && obj.obj.room != null)
            {
                Vector2 dirVec = Custom.DirVec(pos, obj.AttachedChunk.pos);
                float dist = Custom.Dist(pos, obj.AttachedChunk.pos);

                float mass = this.mass / (this.mass + obj.obj.TotalMass);

                Vector2 newDir1 = dirVec * dist * mass * obj.strength;
                Vector2 newDir2 = -dirVec * dist * (1f - mass) * obj.strength;

                pos += newDir1;
                vel += newDir1;
                obj.AttachedChunk.pos += newDir2;
                obj.AttachedChunk.vel += newDir2;

                if (dist > obj.detachDist)
                {
                    DetachObject();
                }
                else if (obj.obj.grabbedBy.Count > 0)
                {
                    BodyChunk grabberChunk = obj.obj.grabbedBy[0].grabber.mainBodyChunk;

                    Vector2 dirVec2 = Custom.DirVec(pos, grabberChunk.pos);

                    Vector2 newDir3 = -dirVec * dist * 0.5f;

                    grabberChunk.pos += newDir3;
                    grabberChunk.vel += newDir3;
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

        public void MoveInWind(Vector2 windDir, float strength)
        {
            vel += windDir * strength;
            vel += Custom.PerpendicularVector(windDir) * Random.Range(-1, 1) * strength * 0.5f;
        }
    }
    public class RopeAttachedObject
    {
        public RopeSegment segment;
        public PhysicalObject obj;
        public int chunkIndex;
        public float detachDist;
        public float strength;
        public BodyChunk AttachedChunk
        {
            get { return obj.bodyChunks[chunkIndex]; }
        }

        public RopeAttachedObject(RopeSegment segment, PhysicalObject obj, int chunkIndex, float strength = 1f, float detachDist = 15f)
        {
            this.segment = segment;
            this.obj = obj;
            this.chunkIndex = chunkIndex;
            this.strength = strength;
            this.detachDist = detachDist;
        }

        public void Update()
        {
            obj.bodyChunks[chunkIndex].vel *= 0.95f;
        }
    }
}

public class RopeObjectData : PlacedObject.ResizableObjectData
{
    new public Vector2 handlePos;
    public Vector2 panelPos;
    public float elasticity;

    public RopeObjectData(PlacedObject owner) : base(owner)
    {
        handlePos = new Vector2(0f, 100f);
        panelPos = new Vector2(0f, 100f);
        elasticity = 1;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", new object[]
        {
            handlePos.x,
            handlePos.y,
            panelPos.x,
            panelPos.y,
            elasticity
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");

        int failIndex = 0;
        try
        {
            handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            elasticity = Mathf.Min(float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), 10f); failIndex++;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
        }
        catch
        {
            if (failIndex < 1)
            {
                handlePos.x = 0f;
            }
            if (failIndex < 2)
            {
                handlePos.y = 100f;
            }
            if (failIndex < 3)
            {
                panelPos.x = 0f;
            }
            if (failIndex < 4)
            {
                panelPos.y = 100f;
            }
            if (failIndex < 5)
            {
                elasticity = 1f;
            }
            if (failIndex < 6)
            {
                unrecognizedAttributes = [];
            }
        }
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class RopeObjectRepresentation : ResizeableObjectRepresentation
{
    public RopeObjectData data;
    public Handle handle;
    public RopeObjectControlPanel controlPanel;
    public FSprite line;
    public RopeObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name, false)
    {
        data = pobj.data as RopeObjectData;
        controlPanel = new(owner, "Lightning_Fruit_Panel", this, data.panelPos, new Vector2(250f, 45f), "Decorative Lightning Vine");

        handle = subNodes[0] as Handle;
        handle.pos = data.handlePos;

        subNodes.Add(controlPanel);

        line = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(line);
        owner.placedObjectsContainer.AddChild(line);
    }

    public class RopeObjectControlPanel : Panel
    {
        public RopeObjectData data;
        public Button newSeedButton;
        public ElasticitySlider elasticitySlider;
        public RopeObjectControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string name) :
            base(owner, IDstring, parentNode, pos, size, name)
        {
            data = (parentNode as RopeObjectRepresentation).data;

            subNodes.Add(elasticitySlider = new(owner, "Elasticity_Slider", this, new Vector2(5f, 25f), "ELASTICITY"));
        }
        public override void Refresh()
        {
            data = (parentNode as RopeObjectRepresentation).data;

            base.Refresh();
        }
        public class ElasticitySlider : Slider
        {
            public RopeObjectData data;
            public ElasticitySlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                data = (parentNode as RopeObjectControlPanel).data;
            }
            public override void Refresh()
            {
                base.Refresh();
                string newText = data.elasticity.ToString();

                if (newText.Length > 3)
                {
                    newText = newText.Substring(0, 3);
                }

                NumberText = newText;
                RefreshNubPos(data.elasticity / 10);
            }
            public override void NubDragged(float nubPos)
            {
                data.elasticity = nubPos * 10;
                parentNode.parentNode.Refresh();
                Refresh();
            }
        }
    }

    public override void Refresh()
    {
        base.Refresh();

        (pObj.data as RopeObjectData).panelPos = controlPanel.pos;
        (pObj.data as RopeObjectData).handlePos = handle.pos;
        (pObj.data as RopeObjectData).elasticity = controlPanel.elasticitySlider.data.elasticity;

        data.elasticity = (pObj.data as RopeObjectData).elasticity;

        MoveSprite(fSprites.IndexOf(line), absPos);
        line.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        line.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
    }
}
