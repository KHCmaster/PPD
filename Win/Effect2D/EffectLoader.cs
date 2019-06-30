using BezierCaliculator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Effect2D
{
    /// <summary>
    /// リソース読み取りコールバック
    /// </summary>
    /// <param name="filename">リソース名</param>
    public delegate void ReadResourceCallBack(string filename);

    /// <summary>
    /// ストリーム取得コールバック
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public delegate Stream StreamCallBack(string filename);

    /// <summary>
    /// エフェクトローダー
    /// </summary>
    public static class EffectLoader
    {
        static Hashtable table = new Hashtable(128);
        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        /// <param name="filename">エフェクトパス</param>
        /// <param name="callback">リソースコールバック</param>
        /// <returns></returns>
        public static EffectManager Load(string filename, ReadResourceCallBack callback)
        {
            return Load(filename, true, callback);
        }
        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        /// <param name="filename">エフェクトパス</param>
        /// <param name="BeCached">キャッシュされるか</param>
        /// <param name="callback">リソースコールバック</param>
        /// <returns></returns>
        public static EffectManager Load(string filename, bool BeCached, ReadResourceCallBack callback)
        {
            if (!File.Exists(filename)) return null;
            var cached = CheckCache(filename, callback);
            if (cached != null)
            {
                return cached;
            }
            using (FileStream fs = File.Open(filename, FileMode.Open))
            {
                return Load(fs, filename, BeCached, callback, f => File.Open(f, FileMode.Open));
            }
        }

        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="callback">コールバック</param>
        /// <param name="streamCallBack">ストリームコールバック</param>
        /// <returns></returns>
        public static EffectManager Load(Stream stream, string filename, ReadResourceCallBack callback, StreamCallBack streamCallBack)
        {
            return Load(stream, filename, true, callback, streamCallBack);
        }

        /// <summary>
        /// エフェクトを読み込む
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="callback">コールバック</param>
        /// <param name="BeCached">キャッシュされるかどうか</param>
        /// <param name="streamCallBack">ストリームコールバック</param>
        /// <returns></returns>
        public static EffectManager Load(Stream stream, string filename, bool BeCached, ReadResourceCallBack callback, StreamCallBack streamCallBack)
        {
            var cached = CheckCache(filename, callback);
            if (cached != null)
            {
                return cached;
            }
            var fn = Path.GetFileNameWithoutExtension(filename);
            var dir = Path.Combine(Path.GetDirectoryName(filename), fn);
            try
            {
                var ret = LoadEffect(stream, callback, streamCallBack, dir);
                ret.CheckFrameLength();
                if (BeCached)
                {
                    if (table.ContainsKey(filename))
                    {
                        table[filename] = ret;
                    }
                    else
                    {
                        table.Add(filename, ret);
                    }
                    return (EffectManager)ret.Clone();
                }
                return ret;
            }
            finally
            {
                stream.Close();
            }
        }

        private static EffectManager CheckCache(string filename, ReadResourceCallBack callback)
        {
            if (table.ContainsKey(filename))
            {
                var em = table[filename] as EffectManager;
                CallReadResourceCallback(em, callback);
                return (EffectManager)em.Clone();
            }
            return null;
        }

        private static void CallReadResourceCallback(IEffect em, ReadResourceCallBack callback)
        {
            foreach (BaseEffect effect in em.Effects)
            {
                if (effect.Effects.Count == 0)
                {
                    callback(effect.Filename);
                }
                else CallReadResourceCallback(effect, callback);
            }
        }

        private static EffectManager LoadEffect(Stream stream, ReadResourceCallBack callback, StreamCallBack streamCallBack, string dir)
        {
            var reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.Name == "Effects")
                    {
                        return ReadXMLEffects(reader.ReadSubtree(), callback, streamCallBack, dir);
                    }
                }
            }
            reader.Close();
            return null;
        }

        private static EffectManager ReadXMLEffects(XmlReader reader, ReadResourceCallBack callback, StreamCallBack streamCallBack, string dir)
        {
            var list = new List<BaseEffect>();
            var manager = new EffectManager();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Effects":
                            float val = 0;
                            if (float.TryParse(reader.GetAttribute("FPS"), NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                            {
                                manager.FPS = val;
                            }
                            else
                            {
                                manager.FPS = 60;
                            }

                            break;
                        case "Effect":
                            manager.Effects.Add(ReadXMLEffect(reader.ReadSubtree(), callback, dir));
                            break;
                        case "EffectReference":
                            var filepath = reader.GetAttribute("FilePath");
                            var parentdir = Path.GetDirectoryName(dir);
                            var be = ReadXMLEffect(reader.ReadSubtree(), callback, dir);
                            be.Filename = Path.Combine(parentdir, filepath);
                            list.Add(be);
                            manager.Effects.Add(be);
                            break;
                    }
                }
            }
            reader.Close();

            foreach (BaseEffect baseEffect in list)
            {
                using (Stream stream = streamCallBack(baseEffect.Filename))
                {
                    var em = Load(stream, baseEffect.Filename, callback, streamCallBack);
                    if (em != null)
                    {
                        baseEffect.Effects = em.Effects;
                        baseEffect.FPS = em.FPS;
                    }
                }
            }
            return manager;
        }
        private static BaseEffect ReadXMLEffect(XmlReader reader, ReadResourceCallBack callback, string dir)
        {
            var effect = new BaseEffect();
            EffectStateStructure lastess = null;
            var set = new EffectStateRatioSet();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Effect":
                            effect.Filename = Path.Combine(dir, reader.GetAttribute("FilePath"));
                            callback(effect.Filename);
                            break;
                        case "State":
                            var ess = new EffectStateStructure();
                            BlendMode blendMode;
                            if (!Enum.TryParse<BlendMode>(reader.GetAttribute("Blend"), out blendMode))
                            {
                                blendMode = BlendMode.None;
                            }
                            ess.BlendMode = blendMode;
                            ess.X = float.Parse(reader.GetAttribute("X"), CultureInfo.InvariantCulture);
                            ess.Y = float.Parse(reader.GetAttribute("Y"), CultureInfo.InvariantCulture);
                            ess.Alpha = float.Parse(reader.GetAttribute("Alpha"), CultureInfo.InvariantCulture);
                            ess.Rotation = float.Parse(reader.GetAttribute("Rotation"), CultureInfo.InvariantCulture);
                            ess.ScaleX = float.Parse(reader.GetAttribute("ScaleX"), CultureInfo.InvariantCulture);
                            ess.ScaleY = float.Parse(reader.GetAttribute("ScaleY"), CultureInfo.InvariantCulture);
                            if (lastess != null)
                            {
                                set.StartState = lastess;
                                set.EndState = ess;
                                effect.Sets.Add(set.StartFrame, set);
                                set = new EffectStateRatioSet();
                            }
                            lastess = ess;
                            break;
                        case "RatioMakers":
                            set.StartFrame = int.Parse(reader.GetAttribute("StartFrame"), CultureInfo.InvariantCulture);
                            set.EndFrame = int.Parse(reader.GetAttribute("EndFrame"), CultureInfo.InvariantCulture);
                            ReadXMLRatioMakers(reader.ReadSubtree(), set);
                            break;
                        case "BezierPositions":
                            ReadXMLBezierPositions(reader.ReadSubtree(), set);
                            break;
                    }
                }
            }
            effect.CheckFrameLength();
            reader.Close();
            return effect;
        }
        private static void ReadXMLBezierPositions(XmlReader reader, EffectStateRatioSet set)
        {
            var bcps = new List<BezierControlPoint>(2);
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.Name == "BezierPosition")
                    {
                        try
                        {
                            var pos = reader.GetAttribute("Position");
                            var bcp = BezierControlPoint.Deserialize(pos);
                            bcps.Add(bcp);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (bcps.Count >= 2)
            {
                set.BAnalyzer = new BezierAnalyzer(bcps.ToArray());
            }
            reader.Close();
        }
        private static void ReadXMLRatioMakers(XmlReader reader, EffectStateRatioSet set)
        {
            while (reader.Read())
            {
                if (reader.Name == "RatioMaker")
                {
                    RatioType type = RatioType.X;
                    switch (reader.GetAttribute("Type"))
                    {
                        case "X":
                            type = RatioType.X;
                            break;
                        case "Y":
                            type = RatioType.Y;
                            break;
                        case "Alpha":
                            type = RatioType.Alpha;
                            break;
                        case "Rotation":
                            type = RatioType.Rotation;
                            break;
                        case "ScaleX":
                            type = RatioType.ScaleX;
                            break;
                        case "ScaleY":
                            type = RatioType.ScaleY;
                            break;
                        case "BezierPosition":
                            type = RatioType.BezierPosition;
                            break;
                    }
                    IRatioMaker maker = null;
                    switch (reader.GetAttribute("MakerType"))
                    {
                        case "LinearRatioMaker":
                            maker = new LinearRatioMaker();
                            break;
                        case "ConstantRatioMaker":
                            maker = new ConstantRatioMaker();
                            break;
                        case "BezierRatioMaker":
                            var p1 = BezierControlPoint.Deserialize(reader.GetAttribute("P1"));
                            var p2 = BezierControlPoint.Deserialize(reader.GetAttribute("P2"));
                            maker = new BezierRatioMaker(p1, p2);
                            break;
                    }
                    maker.Set = set;
                    set[type] = maker;
                }
            }
            reader.Close();
            set.SetDefaultToNullMaker();
        }
        /// <summary>
        /// キャッシュをクリアする
        /// </summary>
        public static void ClearCache()
        {
            table.Clear();
        }

        /// <summary>
        /// エフェクトが参照しているエフェクトのファイル名を取得します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string[] GetEffectReference(string filePath)
        {
            var ret = new List<string>();

            var doc = new XmlDocument();
            doc.Load(filePath);

            foreach (XmlNode node in doc.SelectNodes("/Effects/EffectReference"))
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name == "FilePath")
                    {
                        var fileName = attribute.Value.ToLower();
                        if (!ret.Contains(fileName))
                        {
                            ret.Add(fileName);
                        }
                    }
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// キャッシュされているかどうか
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns></returns>
        public static bool IsCached(string filename)
        {
            return table.Contains(filename);
        }
    }
}
