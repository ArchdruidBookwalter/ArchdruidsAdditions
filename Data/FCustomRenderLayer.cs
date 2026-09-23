using System;

namespace ArchdruidsAdditions.Data;

public class FCustomRenderLayer : FQuadRenderLayer
{
    public static readonly FFacetType CustomFacetTypeQ;

    static FCustomRenderLayer()
    {
        CustomFacetTypeQ = FFacetType.CreateFacetType("AACustomRenderLayerQ", 10, 10, 60, new FFacetType.CreateRenderLayerDelegate(FCustomRenderLayer.CreateRenderLayer));
    }

    public static FFacetRenderLayer CreateRenderLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader)
    {
        return new FCustomRenderLayer(stage, facetType, atlas, shader);
    }

    protected Vector2[] _uvs2 = [];
    protected Vector2[] _uvs3 = [];
    protected Vector2[] _uvs4 = [];
    protected Vector2[] _uvs5 = [];
    protected Vector2[] _uvs6 = [];
    protected Vector2[] _uvs7 = [];
    protected Vector2[] _uvs8 = [];

    public Vector2[] uvs2 => _uvs2;
    public Vector2[] uvs3 => _uvs3;
    public Vector2[] uvs4 => _uvs4;
    public Vector2[] uvs5 => _uvs5;
    public Vector2[] uvs6 => _uvs6;
    public Vector2[] uvs7 => _uvs7;
    public Vector2[] uvs8 => _uvs8;

    protected FCustomRenderLayer(FStage stage, FFacetType facetType, FAtlas atlas, FShader shader) : base(stage, facetType, atlas, shader)
    {
    }

    public override void ExpandMaxFacetLimit(int deltaIncrease)
    {
        base.ExpandMaxFacetLimit(deltaIncrease);

        if (deltaIncrease <= 0) return;
        Array.Resize(ref _uvs2, _maxFacetCount * 4);
        Array.Resize(ref _uvs3, _maxFacetCount * 4);
        Array.Resize(ref _uvs4, _maxFacetCount * 4);
        Array.Resize(ref _uvs5, _maxFacetCount * 4);
        Array.Resize(ref _uvs6, _maxFacetCount * 4);
        Array.Resize(ref _uvs7, _maxFacetCount * 4);
        Array.Resize(ref _uvs8, _maxFacetCount * 4);
    }

    public override void ShrinkMaxFacetLimit(int deltaDecrease)
    {
        base.ShrinkMaxFacetLimit(deltaDecrease);

        if (deltaDecrease <= 0) return;
        Array.Resize(ref _uvs2, _maxFacetCount * 4);
        Array.Resize(ref _uvs3, _maxFacetCount * 4);
        Array.Resize(ref _uvs4, _maxFacetCount * 4);
        Array.Resize(ref _uvs5, _maxFacetCount * 4);
        Array.Resize(ref _uvs6, _maxFacetCount * 4);
        Array.Resize(ref _uvs7, _maxFacetCount * 4);
        Array.Resize(ref _uvs8, _maxFacetCount * 4);
    }

    public void OverrideUpdateMeshProperties()
    {
        _mesh.SetUVs(1, _uvs2);
        _mesh.SetUVs(2, _uvs3);
        _mesh.SetUVs(3, _uvs4);
        _mesh.SetUVs(4, _uvs5);
        _mesh.SetUVs(5, _uvs6);
        _mesh.SetUVs(6, _uvs7);
        _mesh.SetUVs(7, _uvs8);
    }
}

public class FCustomShaderSprite : FSprite
{
    protected Vector2[] _uvs2;
    protected Vector2[] _uvs3;
    protected Vector2[] _uvs4;
    protected Vector2[] _uvs5;
    protected Vector2[] _uvs6;
    protected Vector2[] _uvs7;
    protected Vector2[] _uvs8;
    protected FCustomRenderLayer SpecialRenderLayer => (_renderLayer as FCustomRenderLayer)!;

    public FCustomShaderSprite(string elementName) : this(Futile.atlasManager.GetElementWithName(elementName))
    { }

    public FCustomShaderSprite(FAtlasElement element) : base(element, true)
    {
        Init(FCustomRenderLayer.CustomFacetTypeQ, element, 1);
        _uvs2 = new Vector2[4];
        _uvs3 = new Vector2[4];
        _uvs4 = new Vector2[4];
        _uvs5 = new Vector2[4];
        _uvs6 = new Vector2[4];
        _uvs7 = new Vector2[4];
        _uvs8 = new Vector2[4];
    }

    public void SetUVs(Vector2 uv, int channel)
    {
        Vector2[] array = channel switch
        {
            2 => _uvs2,
            3 => _uvs3,
            4 => _uvs4,
            5 => _uvs5,
            6 => _uvs6,
            7 => _uvs7,
            8 => _uvs8,
            _ => throw new IndexOutOfRangeException("channel was not in the specified range!"),
        };

        for (int i = 0; i < array.Length; i++)
        { array[i] = uv; }

        _isMeshDirty = true;
    }

    public void SetUV(Vector2 uv, int index, int channel)
    {
        Vector2[] array = channel switch
        {
            2 => _uvs2,
            3 => _uvs3,
            4 => _uvs4,
            5 => _uvs5,
            6 => _uvs6,
            7 => _uvs7,
            8 => _uvs8,
            _ => throw new IndexOutOfRangeException("channel was not in the specified range!"),
        };

        array[index] = uv;

        _isMeshDirty = true;
    }

    public override void PopulateRenderLayer()
    {
        base.PopulateRenderLayer();

        if (_isOnStage && _firstFacetIndex != -1)
        {
            Vector2[] uvs2 = SpecialRenderLayer.uvs2;
            Vector2[] uvs3 = SpecialRenderLayer.uvs3;
            Vector2[] uvs4 = SpecialRenderLayer.uvs4;
            Vector2[] uvs5 = SpecialRenderLayer.uvs5;
            Vector2[] uvs6 = SpecialRenderLayer.uvs6;
            Vector2[] uvs7 = SpecialRenderLayer.uvs7;
            Vector2[] uvs8 = SpecialRenderLayer.uvs8;

            int firstIndex = _firstFacetIndex * 4;
            for (int i = 0; i < 4; i++)
            {
                uvs2[firstIndex + i] = _uvs2[i];
                uvs3[firstIndex + i] = _uvs3[i];
                uvs4[firstIndex + i] = _uvs4[i];
                uvs5[firstIndex + i] = _uvs5[i];
                uvs6[firstIndex + i] = _uvs6[i];
                uvs7[firstIndex + i] = _uvs7[i];
                uvs8[firstIndex + i] = _uvs8[i];
            }

            _renderLayer.HandleVertsChange();
        }
    }
}
