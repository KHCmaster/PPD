namespace PPDFramework.Vertex.DX11
{
    class VertexDeclaration : VertexDeclarationBase
    {
        public SharpDX.Direct3D11.InputLayout _InputLayout
        {
            get;
            private set;
        }

        public VertexDeclaration(SharpDX.Direct3D11.InputLayout inputLayout)
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
