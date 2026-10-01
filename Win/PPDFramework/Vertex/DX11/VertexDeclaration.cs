namespace PPDFramework.Vertex.DX11
{
    class VertexDeclaration(Vortice.Direct3D11.ID3D11InputLayout inputLayout) : VertexDeclarationBase
    {
        public Vortice.Direct3D11.ID3D11InputLayout InputLayout
        {
            get;
            private set;
        } = inputLayout;

        protected override void DisposeResource()
        {
            InputLayout?.Dispose();
            InputLayout = null;
        }
    }
}
