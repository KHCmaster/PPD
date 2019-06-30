namespace PPDFramework.Vertex.DX9
{
    class VertexDeclaration : VertexDeclarationBase
    {
        public SharpDX.Direct3D9.VertexDeclaration _VertexDeclaration
        {
            get;
            private set;
        }

        public VertexDeclaration(SharpDX.Direct3D9.VertexDeclaration vertexDeclaration)
        {
            _VertexDeclaration = vertexDeclaration;
        }

        protected override void DisposeResource()
        {
            if (_VertexDeclaration != null)
            {
                _VertexDeclaration.Dispose();
                _VertexDeclaration = null;
            }
        }
    }
}
