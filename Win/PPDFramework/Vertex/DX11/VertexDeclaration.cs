namespace PPDFramework.Vertex.DX11
{
    class VertexDeclaration : VertexDeclarationBase
    {
        public Vortice.Direct3D11.ID3D11InputLayout _InputLayout
        {
            get;
            private set;
        }

        public VertexDeclaration(Vortice.Direct3D11.ID3D11InputLayout inputLayout)
        {
            _InputLayout = inputLayout;
        }

        protected override void DisposeResource()
        {
            if (_InputLayout != null)
            {
                _InputLayout.Dispose();
                _InputLayout = null;
            }
        }
    }
}
