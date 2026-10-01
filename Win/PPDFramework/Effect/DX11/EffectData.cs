using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PPDFramework.Effect.DX11
{
    /// <summary>
    /// ShaderCompiler が出力したエフェクトのバイナリです。
    /// 形式は ShaderCompiler/EffectCompiler.cs と対応しています。
    /// </summary>
    class EffectData
    {
        const string Magic = "PPDFX";
        const int Version = 1;

        public enum VariableBaseType : byte
        {
            Float = 0,
            Int = 1,
            Other = 2,
        }

        public class Variable
        {
            public string Name;
            public int Offset;
            public int Size;
            public int Elements;
            public int Stride;
            public VariableBaseType BaseType;
        }

        public class Pass
        {
            public string Name;
            public int VertexShaderIndex;
            public int PixelShaderIndex;
            public string BlendState;
        }

        public class Technique
        {
            public string Name;
            public Pass[] Passes;
        }

        public int ConstantBufferSize { get; private set; }

        public Dictionary<string, Variable> Variables { get; private set; }

        public Dictionary<string, int> TextureSlots { get; private set; }

        public byte[][] VertexShaders { get; private set; }

        public byte[][] PixelShaders { get; private set; }

        public Dictionary<string, Technique> Techniques { get; private set; }

        public EffectData(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes), Encoding.UTF8);
            if (reader.ReadString() != Magic || reader.ReadInt32() != Version)
            {
                throw new InvalidDataException("Unsupported effect data. Rebuild the shaders with ShaderCompiler.");
            }
            ConstantBufferSize = reader.ReadInt32();
            var variableCount = reader.ReadInt32();
            Variables = new Dictionary<string, Variable>(variableCount);
            for (var i = 0; i < variableCount; i++)
            {
                var variable = new Variable
                {
                    Name = reader.ReadString(),
                    Offset = reader.ReadInt32(),
                    Size = reader.ReadInt32(),
                    Elements = reader.ReadInt32(),
                    Stride = reader.ReadInt32(),
                    BaseType = (VariableBaseType)reader.ReadByte()
                };
                Variables.Add(variable.Name, variable);
            }
            var textureCount = reader.ReadInt32();
            TextureSlots = new Dictionary<string, int>(textureCount);
            for (var i = 0; i < textureCount; i++)
            {
                TextureSlots.Add(reader.ReadString(), reader.ReadInt32());
            }
            VertexShaders = ReadBytecodes(reader);
            PixelShaders = ReadBytecodes(reader);
            var techniqueCount = reader.ReadInt32();
            Techniques = new Dictionary<string, Technique>(techniqueCount);
            for (var i = 0; i < techniqueCount; i++)
            {
                var technique = new Technique
                {
                    Name = reader.ReadString(),
                    Passes = new Pass[reader.ReadInt32()]
                };
                for (var j = 0; j < technique.Passes.Length; j++)
                {
                    technique.Passes[j] = new Pass
                    {
                        Name = reader.ReadString(),
                        VertexShaderIndex = reader.ReadInt32(),
                        PixelShaderIndex = reader.ReadInt32(),
                        BlendState = reader.ReadString()
                    };
                }
                Techniques.Add(technique.Name, technique);
            }
        }

        private static byte[][] ReadBytecodes(BinaryReader reader)
        {
            var bytecodes = new byte[reader.ReadInt32()][];
            for (var i = 0; i < bytecodes.Length; i++)
            {
                bytecodes[i] = reader.ReadBytes(reader.ReadInt32());
            }
            return bytecodes;
        }
    }
}
