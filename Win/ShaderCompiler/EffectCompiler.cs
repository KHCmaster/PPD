using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11.Shader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShaderCompiler
{
    /// <summary>
    /// エフェクトファイル（.fx）のテクニックを解析し、各パスのシェーダーをコンパイルして 1 つのバイナリにまとめます。
    /// Effects11（fx_5_0）を使わずに描画するためのものです。出力形式は PPDFramework.Effect.DX11.EffectData と対応しています。
    ///
    /// 前提:
    /// - パラメーターは "Parameters" という名前の cbuffer（register(b0)）にまとめる
    /// - テクスチャとサンプラーは register で明示的にスロットを指定する
    /// - パスの中身は SetBlendState / SetVertexShader(CompileShader(...)) / SetPixelShader(CompileShader(...)) のみ
    /// </summary>
    class EffectCompiler
    {
        public const string Magic = "PPDFX";
        public const int Version = 1;
        const string ConstantBufferName = "Parameters";

        static readonly Regex techniqueRegex = new(@"technique11\s+(?<name>\w+)\s*\{", RegexOptions.Compiled);
        static readonly Regex passRegex = new(@"pass\s+(?<name>\w+)\s*\{(?<body>[^{}]*)\}", RegexOptions.Compiled);
        static readonly Regex blendRegex = new(@"SetBlendState\(\s*(?<name>\w+)", RegexOptions.Compiled);
        static readonly Regex shaderRegex = new(@"Set(?<stage>Vertex|Pixel)Shader\(\s*CompileShader\(\s*(?<profile>\w+)\s*,\s*(?<func>\w+)\s*\((?<args>[^()]*)\)\s*\)\s*\)", RegexOptions.Compiled);

        class ShaderCall
        {
            public string Profile;
            public string Function;
            public string Arguments;
            public string Key { get { return Profile + ":" + Function + "(" + Arguments + ")"; } }
        }

        class PassInfo
        {
            public string Name;
            public string BlendState;
            public ShaderCall VertexShader;
            public ShaderCall PixelShader;
        }

        class TechniqueInfo
        {
            public string Name;
            public List<PassInfo> Passes = [];
        }

        public void Compile(string fxPath, string outputPath)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(fxPath));
            var source = File.ReadAllText(fxPath);
            var techniques = ParseTechniques(source, out string sourceWithoutTechniques);
            if (techniques.Count == 0)
            {
                throw new Exception("technique11 が見つかりません: " + fxPath);
            }

            var vertexShaders = new List<ShaderCall>();
            var pixelShaders = new List<ShaderCall>();
            foreach (var pass in techniques.SelectMany(t => t.Passes))
            {
                AddUnique(vertexShaders, pass.VertexShader);
                AddUnique(pixelShaders, pass.PixelShader);
            }

            // 引数付きの関数を呼ぶためのエントリポイントを追加する
            var wrapperSource = new StringBuilder(sourceWithoutTechniques);
            wrapperSource.AppendLine();
            for (var i = 0; i < vertexShaders.Count; i++)
            {
                AppendWrapper(wrapperSource, sourceWithoutTechniques, vertexShaders[i], "__vs" + i, false);
            }
            for (var i = 0; i < pixelShaders.Count; i++)
            {
                AppendWrapper(wrapperSource, sourceWithoutTechniques, pixelShaders[i], "__ps" + i, true);
            }
            var fullSource = wrapperSource.ToString();

            var vertexBytecodes = new List<byte[]>();
            var pixelBytecodes = new List<byte[]>();
            using (var include = new FileInclude(directory))
            {
                for (var i = 0; i < vertexShaders.Count; i++)
                {
                    vertexBytecodes.Add(CompileEntry(fullSource, "__vs" + i, vertexShaders[i], include, fxPath));
                }
                for (var i = 0; i < pixelShaders.Count; i++)
                {
                    pixelBytecodes.Add(CompileEntry(fullSource, "__ps" + i, pixelShaders[i], include, fxPath));
                }
            }

            var variables = new Dictionary<string, VariableInfo>();
            var constantBufferSize = 0;
            var textures = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var bytecode in vertexBytecodes.Concat(pixelBytecodes))
            {
                Reflect(bytecode, variables, ref constantBufferSize, textures);
            }

            using (var stream = File.Create(outputPath))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(constantBufferSize);
                writer.Write(variables.Count);
                foreach (var variable in variables.Values.OrderBy(v => v.Offset))
                {
                    writer.Write(variable.Name);
                    writer.Write(variable.Offset);
                    writer.Write(variable.Size);
                    writer.Write(variable.Elements);
                    writer.Write(variable.Stride);
                    writer.Write((byte)variable.BaseType);
                }
                writer.Write(textures.Count);
                foreach (var texture in textures)
                {
                    writer.Write(texture.Key);
                    writer.Write(texture.Value);
                }
                WriteBytecodes(writer, vertexBytecodes);
                WriteBytecodes(writer, pixelBytecodes);
                writer.Write(techniques.Count);
                foreach (var technique in techniques)
                {
                    writer.Write(technique.Name);
                    writer.Write(technique.Passes.Count);
                    foreach (var pass in technique.Passes)
                    {
                        writer.Write(pass.Name);
                        writer.Write(vertexShaders.FindIndex(s => s.Key == pass.VertexShader.Key));
                        writer.Write(pixelShaders.FindIndex(s => s.Key == pass.PixelShader.Key));
                        writer.Write(pass.BlendState ?? "");
                    }
                }
            }
            Console.WriteLine("{0}: {1} techniques, {2} vertex shaders, {3} pixel shaders, cbuffer {4} bytes ({5} variables), {6} textures",
                Path.GetFileName(outputPath), techniques.Count, vertexShaders.Count, pixelShaders.Count, constantBufferSize, variables.Count, textures.Count);
        }

        private static void AddUnique(List<ShaderCall> list, ShaderCall call)
        {
            if (!list.Any(c => c.Key == call.Key))
            {
                list.Add(call);
            }
        }

        private static List<TechniqueInfo> ParseTechniques(string source, out string sourceWithoutTechniques)
        {
            var techniques = new List<TechniqueInfo>();
            var remaining = new StringBuilder();
            var position = 0;
            foreach (Match match in techniqueRegex.Matches(source))
            {
                if (match.Index < position)
                {
                    continue;
                }
                var bodyStart = match.Index + match.Length;
                var end = FindClosingBrace(source, bodyStart);
                var body = source[bodyStart..end];
                var technique = new TechniqueInfo { Name = match.Groups["name"].Value };
                foreach (Match passMatch in passRegex.Matches(body))
                {
                    var passBody = passMatch.Groups["body"].Value;
                    var pass = new PassInfo { Name = passMatch.Groups["name"].Value };
                    var blend = blendRegex.Match(passBody);
                    if (blend.Success)
                    {
                        pass.BlendState = blend.Groups["name"].Value;
                    }
                    foreach (Match shader in shaderRegex.Matches(passBody))
                    {
                        var call = new ShaderCall
                        {
                            Profile = shader.Groups["profile"].Value,
                            Function = shader.Groups["func"].Value,
                            Arguments = shader.Groups["args"].Value.Trim()
                        };
                        if (shader.Groups["stage"].Value == "Vertex")
                        {
                            pass.VertexShader = call;
                        }
                        else
                        {
                            pass.PixelShader = call;
                        }
                    }
                    if (pass.VertexShader == null || pass.PixelShader == null)
                    {
                        throw new Exception(string.Format("{0}.{1}: 頂点シェーダーとピクセルシェーダーの両方が必要です", technique.Name, pass.Name));
                    }
                    technique.Passes.Add(pass);
                }
                techniques.Add(technique);
                remaining.Append(source, position, match.Index - position);
                position = end + 1;
            }
            remaining.Append(source, position, source.Length - position);
            sourceWithoutTechniques = remaining.ToString();
            return techniques;
        }

        private static int FindClosingBrace(string source, int start)
        {
            var depth = 1;
            for (var i = start; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }
            throw new Exception("technique の閉じ括弧が見つかりません");
        }

        private static void AppendWrapper(StringBuilder builder, string source, ShaderCall call, string entryName, bool isPixelShader)
        {
            // 呼び出す関数の戻り値の型と第 1 引数の型を取得する
            var signature = new Regex(@"(?<ret>\w+)\s+" + Regex.Escape(call.Function) + @"\s*\(\s*(?<type>\w+)\s+\w+");
            var match = signature.Match(source);
            if (!match.Success)
            {
                throw new Exception("関数が見つかりません: " + call.Function);
            }
            var arguments = string.IsNullOrEmpty(call.Arguments) ? "input" : "input, " + call.Arguments;
            builder.AppendFormat("{0} {1}({2} input){3}{{ return {4}({5}); }}",
                match.Groups["ret"].Value, entryName, match.Groups["type"].Value, isPixelShader ? " : SV_TARGET " : " ", call.Function, arguments);
            builder.AppendLine();
        }

        private static byte[] CompileEntry(string source, string entry, ShaderCall call, Include include, string fileName)
        {
            // 日本語のコメントがあっても長さがずれないように、UTF-8 のバイト列で渡す
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            var hr = Compiler.Compile(sourceBytes, null, include, entry, fileName, call.Profile, ShaderFlags.OptimizationLevel3, EffectFlags.None, out Blob blob, out Blob errorBlob);
            try
            {
                if (hr.Failure || blob == null)
                {
                    // 調査用に、実際にコンパイルしたソースを書き出す
                    var dumpPath = Path.ChangeExtension(fileName, ".failed.hlsl");
                    File.WriteAllText(dumpPath, source);
                    throw new Exception(string.Format("{0} のコンパイルに失敗しました（ソース: {1}）:\n{2}", call.Key, dumpPath, errorBlob?.AsString()));
                }
                return blob.AsBytes();
            }
            finally
            {
                blob?.Dispose();
                errorBlob?.Dispose();
            }
        }

        enum BaseType : byte
        {
            Float = 0,
            Int = 1,
            Other = 2,
        }

        class VariableInfo
        {
            public string Name;
            public int Offset;
            public int Size;
            public int Elements;
            public int Stride;
            public BaseType BaseType;
        }

        private static void Reflect(byte[] bytecode, Dictionary<string, VariableInfo> variables, ref int constantBufferSize, SortedDictionary<string, int> textures)
        {
            using var reflection = Compiler.Reflect<ID3D11ShaderReflection>(bytecode);
            var description = reflection.Description;
            for (var i = 0; i < description.BoundResources; i++)
            {
                var binding = reflection.GetResourceBindingDescription((uint)i);
                if (binding.Type == ShaderInputType.Texture)
                {
                    if (textures.TryGetValue(binding.Name, out int slot) && slot != (int)binding.BindPoint)
                    {
                        throw new Exception(binding.Name + " のスロットがシェーダーによって異なります。register で明示してください");
                    }
                    textures[binding.Name] = (int)binding.BindPoint;
                }
                else if (binding.Type == ShaderInputType.ConstantBuffer && binding.Name != ConstantBufferName)
                {
                    throw new Exception("定数は cbuffer " + ConstantBufferName + " にまとめてください: " + binding.Name);
                }
            }
            for (var i = 0; i < description.ConstantBuffers; i++)
            {
                var buffer = reflection.GetConstantBufferByIndex((uint)i);
                var bufferDescription = buffer.Description;
                if (bufferDescription.Name != ConstantBufferName)
                {
                    continue;
                }
                constantBufferSize = Math.Max(constantBufferSize, (int)bufferDescription.Size);
                for (var j = 0; j < bufferDescription.VariableCount; j++)
                {
                    var variable = buffer.GetVariableByIndex((uint)j);
                    var variableDescription = variable.Description;
                    var typeDescription = variable.VariableType.Description;
                    var info = new VariableInfo
                    {
                        Name = variableDescription.Name,
                        Offset = (int)variableDescription.StartOffset,
                        Size = (int)variableDescription.Size,
                        Elements = (int)typeDescription.ElementCount,
                        BaseType = typeDescription.Class == ShaderVariableClass.Struct ? BaseType.Other :
                            typeDescription.Type == ShaderVariableType.Int ? BaseType.Int :
                            typeDescription.Type == ShaderVariableType.Float ? BaseType.Float : BaseType.Other
                    };
                    // cbuffer 内の配列は要素ごとに 16 バイト境界に揃えられる
                    var elementSize = typeDescription.Class == ShaderVariableClass.Struct ? info.Size : (int)(typeDescription.RowCount * typeDescription.ColumnCount) * 4;
                    info.Stride = info.Elements > 0 ? (elementSize + 15) / 16 * 16 : info.Size;
                    if (variables.TryGetValue(info.Name, out VariableInfo existing) && (existing.Offset != info.Offset || existing.Size != info.Size))
                    {
                        throw new Exception(info.Name + " の配置がシェーダーによって異なります");
                    }
                    variables[info.Name] = info;
                }
            }
        }

        private static void WriteBytecodes(BinaryWriter writer, List<byte[]> bytecodes)
        {
            writer.Write(bytecodes.Count);
            foreach (var bytecode in bytecodes)
            {
                writer.Write(bytecode.Length);
                writer.Write(bytecode);
            }
        }

        /// <summary>
        /// #include をエフェクトファイルと同じフォルダから解決します。
        /// </summary>
        class FileInclude(string directory) : CallbackBase, Include
        {
            readonly string directory = directory;

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                return new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
            }

            public void Close(Stream stream)
            {
                stream.Dispose();
            }
        }
    }
}
