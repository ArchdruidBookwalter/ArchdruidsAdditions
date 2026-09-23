using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DevInterface;
using Watcher;

namespace ArchdruidsAdditions.Objects;

public class Chain : UpdatableAndDeletable
{
    public RopeObject chain;

    public PlacedObject pObj;

    public DynamicLevelElement[] chainLinks;

    public int jingleCooldown;
    public SoundID jangleSound;

    public PositionedSoundEmitter soundEmitter;

    public float soundVolume;
    public float depth;
    public bool bothEndsStuck;

    public Chain(Room room, PlacedObject pObj, float depth, float elasticity, Vector2 endPos1, Vector2 endPos2, bool bothEndsStuck = false, float gravity = 0.9f)
    {
        this.room = room;
        this.depth = depth;
        this.pObj = pObj;
        this.bothEndsStuck = bothEndsStuck;

        chain = new(this, 10f, 5f, elasticity, 0.1f, endPos1, endPos2, 1.5f, false, true, bothEndsStuck, gravity);

        int section = 0;

        try
        {
            section = 1;

            string assetName = "ChainLink";

            Texture2D texture = new(1, 1);
            string filePath = AssetManager.ResolveFilePath("atlases" + Path.DirectorySeparatorChar + "leveltextures" + Path.DirectorySeparatorChar + assetName + ".png");
            AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + filePath, true, true);
            HeavyTexturesCache.LoadAndCacheAtlasFromTexture(assetName, texture, false);

            section = 2;

            FAtlas newAtlas = Futile.atlasManager.GetAtlasWithName("ChainLink");
            if (newAtlas == null || newAtlas.texture == null)
            {
                Debug.Log("");
                Debug.Log("ATLAS WAS NULL");
                Debug.Log("");
            }

            section = 3;

            chainLinks = new DynamicLevelElement[chain.ropeSegments.Length - 1];
            for (int i = 0; i < chainLinks.Length; i++)
            {
                Vector2 pos1 = chain.ropeSegments[i].pos;
                Vector2 pos2 = chain.ropeSegments[i + 1].pos;

                chainLinks[i] = new(Vector2.Lerp(pos1, pos2, 0.5f), new Vector2(2f, 2f), newAtlas.texture, Mathf.FloorToInt(5f), null, null);
                room.AddObject(chainLinks[i]);
            }

            section = 3;
        }
        catch (Exception e)
        {
            Log_Exception(e, "CHAIN_CTOR", section);
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (pObj != null)
        {
            chain.endPos1 = pObj.pos;
        }

        chain.Update();

        UpdateGraphics();

        float combinedVel = 0;
        foreach (RopeObject.RopeSegment segment in chain.ropeSegments)
        { combinedVel += Mathf.Abs(segment.vel.x); }

        Create_Text(room, chain.ropeSegments[chain.EndIndex].pos, combinedVel, Color.red, 0);

        RopeObject.RopeSegment middleSegment = chain.ropeSegments[chain.EndIndex / 2];
        if (combinedVel > 10)
        {
            soundVolume = Mathf.Lerp(soundVolume, 1, 0.1f);

            if (soundEmitter == null)
            {
                soundEmitter = new(middleSegment.pos, 0f, 1f);
                room.PlaySound(NewSoundID.RandomChainLoop(), soundEmitter, true, 0f, 1f, false);
            }
        }
        else
        {
            soundVolume = Mathf.Lerp(soundVolume, 0, 0.1f);
        }

        if (soundEmitter != null)
        {
            soundEmitter.Update(eu);
            soundEmitter.lastPos = soundEmitter.pos;
            soundEmitter.pos = middleSegment.pos;
            soundEmitter.volume = soundVolume * 0.5f;
            if (soundEmitter.slatedForDeletetion || !soundEmitter.soundStillPlaying)
            {
                soundEmitter = null;
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        foreach (DynamicLevelElement element in chainLinks)
        { element.Destroy(); }
    }

    public void UpdateGraphics()
    {
        for (int i = 0; i < chainLinks.Length; i++)
        {
            Vector2 pos1 = chain.ropeSegments[i].pos;
            Vector2 pos2 = chain.ropeSegments[i + 1].pos;

            Vector2 segmentDir = Custom.DirVec(pos1, pos2);

            DynamicLevelElement element = chainLinks[i];

            element.pos = Vector2.Lerp(pos1, pos2, 0.5f);
            element.scale = new Vector2(0.8f, Mathf.Max(0.6f, Custom.Dist(pos1, pos2) / 16));
            element.setDepthOffset = Mathf.FloorToInt(depth * 30f);
            element.rotation = Custom.VecToDeg(segmentDir);
        }
    }

    public void JingleSound(Vector2 pos)
    {
        jingleCooldown = Random.Range(50, 100);
        room.PlaySound(NewSoundID.RandomChainSound(), pos, Random.Range(0.2f, 0.5f), 1f);
    }
}

public class ChainData : PlacedObject.ResizableObjectData
{
    new public Vector2 handlePos;
    public Vector2 panelPos;
    public float elasticity;
    public float depth;
    public bool bothEndsStuck;

    public Chain realizedChain;

    public ChainData(PlacedObject owner) : base(owner)
    {
        handlePos = new Vector2(0f, 100f);
        panelPos = new Vector2(0f, 100f);
        elasticity = 1;
        depth = 0;
        bothEndsStuck = false;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", new object[]
        {
            handlePos.x,
            handlePos.y,
            panelPos.x,
            panelPos.y,
            elasticity,
            depth,
            bothEndsStuck ? 1 : 0
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
            elasticity = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            depth = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            bothEndsStuck = int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture) == 1; failIndex++;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
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
                depth = 1f;
            }
            if (failIndex < 7)
            {
                bothEndsStuck = false;
            }
            if (failIndex < 8)
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

public class ChainRepresentation : ResizeableObjectRepresentation
{
    public ChainData data;
    public Handle handle;
    public ChainControlPanel controlPanel;
    public FSprite line;

    public ChainRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name, false)
    {
        data = pobj.data as ChainData;
        controlPanel = new(owner, "Chain_Panel", this, data.panelPos, new Vector2(250f, 65f), "Decorative Chain");

        handle = subNodes[0] as Handle;
        handle.pos = data.handlePos;

        subNodes.Add(controlPanel);

        line = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(line);
        owner.placedObjectsContainer.AddChild(line);
    }

    public class ChainControlPanel : Panel, IDevUISignals
    {
        public ChainData data;
        public Button bothEndsStuckButton;
        public ElasticitySlider elasticitySlider;
        public DepthSlider depthSlider;
        public ChainControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string name) :
            base(owner, IDstring, parentNode, pos, size, name)
        {
            data = (parentNode as ChainRepresentation).data;

            subNodes.Add(elasticitySlider = new(owner, "Elasticity_Slider", this, new Vector2(5f, 45f), "ELASTICITY"));
            subNodes.Add(depthSlider = new(owner, "Depth_Slider", this, new Vector2(5f, 25f), "DEPTH"));
            subNodes.Add(bothEndsStuckButton = new(owner, "BothEndsStuck_Button", this, new Vector2(5f, 5f), 240f, "BOTH ENDS STUCK: " + data.bothEndsStuck));
        }
        public override void Refresh()
        {
            data = (parentNode as ChainRepresentation).data;

            bothEndsStuckButton.Text = "BOTH ENDS STUCK: " + (data.bothEndsStuck ? "TRUE" : "FALSE");

            base.Refresh();
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "BothEndsStuck_Button")
            {
                data.bothEndsStuck = !data.bothEndsStuck;
            }
        }
        public class ElasticitySlider : Slider
        {
            public ChainData data;
            public ElasticitySlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                data = (parentNode as ChainControlPanel).data;
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
        public class DepthSlider : Slider
        {
            public ChainData data;
            public DepthSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                data = (parentNode as ChainControlPanel).data;
            }
            public override void Refresh()
            {
                base.Refresh();
                string newText = Mathf.RoundToInt(data.depth * 30).ToString();

                if (newText.Length > 2)
                {
                    newText = newText.Substring(0, 2);
                }

                NumberText = newText;
                RefreshNubPos(data.depth);
            }
            public override void NubDragged(float nubPos)
            {
                data.depth = nubPos;
                parentNode.parentNode.Refresh();
                Refresh();
            }
        }
    }

    public override void Refresh()
    {
        base.Refresh();

        (pObj.data as ChainData).panelPos = controlPanel.pos;
        (pObj.data as ChainData).handlePos = handle.pos;
        (pObj.data as ChainData).elasticity = controlPanel.elasticitySlider.data.elasticity;
        (pObj.data as ChainData).depth = controlPanel.elasticitySlider.data.depth;

        data.elasticity = (pObj.data as ChainData).elasticity;
        data.depth = (pObj.data as ChainData).depth;
        data.bothEndsStuck = (pObj.data as ChainData).bothEndsStuck;

        MoveSprite(fSprites.IndexOf(line), absPos);
        line.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        line.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);


        if (data.realizedChain != null)
        {
            data.realizedChain.depth = data.depth;
        }
    }
}