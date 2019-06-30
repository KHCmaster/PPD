namespace PPDFramework.Vertex
{
    interface IVertexDeclarationFactory
    {
        VertexDeclarationBase Create(PPDDevice device, Effect.EffectBase effect);
    }
}
