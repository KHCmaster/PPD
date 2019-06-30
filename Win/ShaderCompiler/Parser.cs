using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShaderCompiler
{
    class Parser
    {
        List<Element> elements;
        Type[] parsableElementTypes = { typeof(Params), typeof(Foreach) };
        Type[] parsableExpTypes = { typeof(ArrayExp), typeof(HashExp) };

        public Parser()
        {
            elements = new List<Element>();
        }

        public void Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var lineIndex = 0;
            while (lineIndex < lines.Length)
            {
                var nextLineIndex = -1;
                var foundElement = Parse(lines, lineIndex, out nextLineIndex);
                elements.Add(foundElement);
                lineIndex = nextLineIndex;
            }
        }

        public Element Parse(string[] lines, int lineIndex, out int nextLineIndex)
        {
            nextLineIndex = -1;
            ParsableElement foundParsableElement = null;
            foreach (var type in parsableElementTypes)
            {
                var parsableElement = Activator.CreateInstance(type, this) as ParsableElement;
                if (parsableElement.Parse(lines, lineIndex, out nextLineIndex))
                {
                    foundParsableElement = parsableElement;
                    break;
                }
            }
            if (foundParsableElement != null)
            {
                return foundParsableElement;
            }
            var ret = new TextElement(this, lines[lineIndex]);
            nextLineIndex = lineIndex + 1;
            return ret;
        }

        public Exp GetExp(Type valueType, string content)
        {
            foreach (var type in parsableExpTypes)
            {
                var exp = Activator.CreateInstance(type) as Exp;
                if (exp.ExpectedType == valueType)
                {
                    exp.Parse(content);
                    return exp;
                }
            }
            throw new Exception(String.Format("No matched exp\n{0}", content));
        }

        public string ToStr()
        {
            var scope = new Scope();
            return String.Join("\n", elements.Select(e => e.ToStr(scope)));
        }

        public IEnumerable<T> GetElements<T>() where T : Element
        {
            return elements.OfType<T>();
        }
    }

    abstract class Element
    {
        protected Parser parser;

        protected Element(Parser parser)
        {
            this.parser = parser;
        }

        public virtual string ToStr(Scope scope)
        {
            return "";
        }
    }

    abstract class Value
    {
        public abstract string ToStr();
    }

    class Text : Value
    {
        public string Value
        {
            get;
            private set;
        }

        public Text(string text)
        {
            Value = text;
        }

        public override bool Equals(object obj)
        {
            if (obj is Text)
            {
                return Value == ((Text)obj).Value;
            }
#pragma warning disable RECS0149 // Finds potentially erroneous calls to Object.Equals
            return base.Equals(obj);
#pragma warning restore RECS0149 // Finds potentially erroneous calls to Object.Equals
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToStr()
        {
            return Value;
        }
    }

    class Array : Value
    {
        public Value[] Values
        {
            get;
            private set;
        }

        public Array(Value[] values)
        {
            Values = values;
        }

        public override string ToStr()
        {
            return String.Format("[{0}]", String.Join(", ", Values.Select(v => v.ToStr())));
        }
    }

    class Hash : Value
    {
        public Dictionary<Value, Value> Values
        {
            get;
            private set;
        }

        public Hash(Dictionary<Value, Value> values)
        {
            Values = values;
        }

        public override string ToStr()
        {
            return String.Format("{{{0}}}", String.Join(", ", Values.Select(v => String.Format("{0} => {1}", v.Key.ToStr(), v.Value.ToStr()))));
        }
    }

    class TextElement : Element
    {
        string text;
        Regex replaceRegex = new Regex(@"\$\((?<key>[_a-zA-Z0-9][_a-zA-Z0-9]*)\)");

        public TextElement(Parser parser, string text) : base(parser)
        {
            this.text = text;
        }

        public override string ToStr(Scope scope)
        {
            return replaceRegex.Replace(text, m =>
            {
                var v = scope.Get(m.Groups["key"].Value);
                if (v != null)
                {
                    return v.ToStr();
                }
                return m.Value;
            });
        }
    }

    abstract class Exp
    {
        public abstract Type ExpectedType { get; }
        public abstract void Parse(string content);
        public abstract IEnumerable<KeyValuePair<string, Value>[]> Expand(Value value);
    }

    class ArrayExp : Exp
    {
        public override Type ExpectedType
        {
            get
            {
                return typeof(Array);
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public override void Parse(string content)
        {
            Name = content.Trim();
        }

        public override IEnumerable<KeyValuePair<string, Value>[]> Expand(Value value)
        {
            var array = (Array)value;
            foreach (var v in array.Values)
            {
                yield return new KeyValuePair<string, Value>[] { new KeyValuePair<string, Value>(Name, v) };
            }
        }
    }

    class HashExp : Exp
    {
        static Regex regex = new Regex(@"^\s*(?<key>[_a-zA-Z][_a-zA-Z0-9]*)\s*=>\s*(?<value>[_a-zA-Z][_a-zA-Z0-9]*)\s*$");

        public override Type ExpectedType
        {
            get
            {
                return typeof(Hash);
            }
        }

        public string Key
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }

        public override void Parse(string content)
        {
            var match = regex.Match(content);
            if (!match.Success)
            {
                throw new Exception(String.Format("Error\n{0}", content));
            }
            Key = match.Groups["key"].Value;
            Value = match.Groups["value"].Value;
        }

        public override IEnumerable<KeyValuePair<string, Value>[]> Expand(Value value)
        {
            var hash = (Hash)value;
            foreach (var p in hash.Values)
            {
                yield return new KeyValuePair<string, Value>[] {
                    new KeyValuePair<string, Value>(Key, p.Key),
                    new KeyValuePair<string, Value>(Value, p.Value)
                };
            }
        }
    }

    abstract class ParsableElement : Element
    {
        protected ParsableElement(Parser parser) : base(parser)
        {

        }

        public abstract bool Parse(string[] lines, int lineIndex, out int nextLineIndex);
    }

    class Params : ParsableElement
    {
        static Regex startRegex = new Regex(@"^#params\s+(?<name>[_a-zA-Z][_a-zA-Z0-9]*)\s+(?<value>.+)\s*$");

        public string Name
        {
            get;
            private set;
        }

        public Value Value
        {
            get;
            private set;
        }

        public Params(Parser parser) : base(parser)
        {

        }

        public override bool Parse(string[] lines, int lineIndex, out int nextLineIndex)
        {
            nextLineIndex = -1;
            var currentLine = lines[lineIndex];
            var startMatch = startRegex.Match(currentLine);
            if (!startMatch.Success)
            {
                return false;
            }
            Name = startMatch.Groups["name"].Value;
            var value = startMatch.Groups["value"].Value;
            Value = ParseValue(value, 0, new char[0], out int nextIndex);
            lineIndex++;
            nextLineIndex = lineIndex;
            return true;
        }

        private Value ParseValue(string str, int index, char[] terminaters, out int nextIndex)
        {
            nextIndex = -1;
            switch (str[index])
            {
                case '{':
                    var dict = new Dictionary<Value, Value>();
                    while (index + 1 < str.Length)
                    {
                        var key = ParseValue(str, index + 1, new[] { ':', ',', '}' }, out nextIndex);
                        if (nextIndex >= str.Length)
                        {
                            dict[key] = key;
                            break;
                        }
                        var c = str[nextIndex];
                        switch (c)
                        {
                            case ':':
                                var value = ParseValue(str, nextIndex + 1, new[] { ',', '}' }, out nextIndex);
                                dict[key] = value;
                                break;
                            case ',':
                                dict[key] = key;
                                break;
                            case '}':
                                while (nextIndex < str.Length && str[nextIndex] == ' ')
                                {
                                    nextIndex++;
                                }
                                break;
                            default:
                                throw new Exception(String.Format("Error\n{0}", str));
                        }
                        index = nextIndex;
                    }
                    return new Hash(dict);
                case '[':
                    var arr = new List<Value>();
                    while (index + 1 < str.Length)
                    {
                        var value = ParseValue(str, index + 1, new[] { ',', ']' }, out nextIndex);
                        var c = str[nextIndex];
                        switch (c)
                        {
                            case ',':
                                arr.Add(value);
                                break;
                            case ']':
                                arr.Add(value);
                                while (nextIndex < str.Length && str[nextIndex] == ' ')
                                {
                                    nextIndex++;
                                }
                                break;
                            default:
                                throw new Exception(String.Format("Error\n{0}", str));
                        }
                        index = nextIndex;
                    }
                    return new Array(arr.ToArray());
                default:
                    return new Text(ReadASCIIOrNumber(str, index, terminaters, out nextIndex));
            }
        }

        private string ReadASCIIOrNumber(string str, int index, char[] terminaters, out int nextIndex)
        {
            var num = ReadNumber(str, index, terminaters, out nextIndex);
            if (num != null)
            {
                return num;
            }
            var ascii = ReadASCII(str, index, terminaters, out nextIndex);
            if (ascii != null)
            {
                return ascii;
            }
            throw new Exception(String.Format("Error\n{0}", str));
        }

        private string ReadNumber(string str, int index, char[] terminaters, out int nextIndex)
        {
            nextIndex = -1;
            var current = index;
            while (current < str.Length)
            {
                var c = str[current];
                if (c != ' ')
                {
                    break;
                }
                current++;
            }
            var first = true;
            while (current < str.Length)
            {
                var c = str[current];
                if (first && (Char.IsDigit(c) || c == '-'))
                {

                }
                else if (Char.IsDigit(c))
                {

                }
                else if (System.Array.IndexOf(terminaters, c) >= 0)
                {
                    nextIndex = current;
                    break;
                }
                else
                {
                    return null;
                }
                first = false;
                current++;
            }
            return str.Substring(index, nextIndex - index).Trim();
        }

        private string ReadASCII(string str, int index, char[] terminaters, out int nextIndex)
        {
            nextIndex = -1;
            var current = index;
            while (current < str.Length)
            {
                var c = str[current];
                if (c != ' ')
                {
                    break;
                }
                current++;
            }
            var first = true;
            while (current < str.Length)
            {
                var c = str[current];
                if (first && (Char.IsLetter(c) || c == '_'))
                {

                }
                else if (Char.IsLetter(c) || c == '_' || Char.IsDigit(c))
                {

                }
                else if (System.Array.IndexOf(terminaters, c) >= 0)
                {
                    nextIndex = current;
                    break;
                }
                else
                {
                    return null;
                }
                first = false;
                current++;
            }
            return str.Substring(index, nextIndex - index).Trim();
        }
    }

    class Foreach : ParsableElement
    {
        static Regex startRegex = new Regex(@"^#foreach\s*\(\s*(?<arg>[_a-zA-Z][_a-zA-Z0-9]*)\s*,\s*(?<exp>.*)\s*\)\s*$");
        static Regex endRegex = new Regex(@"^\s*#end\s*$");

        public Params Arg
        {
            get;
            private set;
        }

        private Exp Expression
        {
            get;
            set;
        }

        public Element[] Elements
        {
            get;
            private set;
        }

        public Foreach(Parser parser) : base(parser)
        {

        }

        public override bool Parse(string[] lines, int lineIndex, out int nextLineIndex)
        {
            nextLineIndex = -1;
            var currentLine = lines[lineIndex];
            var startMatch = startRegex.Match(currentLine);
            if (!startMatch.Success)
            {
                return false;
            }
            Arg = parser.GetElements<Params>().First(p => p.Name == startMatch.Groups["arg"].Value);
            Expression = parser.GetExp(Arg.Value.GetType(), startMatch.Groups["exp"].Value);
            var contentElements = new List<Element>();
            lineIndex++;
            while (lineIndex < lines.Length)
            {
                if (endRegex.IsMatch(lines[lineIndex]))
                {
                    lineIndex++;
                    break;
                }
                else
                {
                    contentElements.Add(parser.Parse(lines, lineIndex, out nextLineIndex));
                }
                lineIndex = nextLineIndex;
            }
            Elements = contentElements.ToArray();
            nextLineIndex = lineIndex;
            return true;
        }

        public override string ToStr(Scope scope)
        {
            var strs = new List<string>();
            foreach (var p in Expression.Expand(Arg.Value))
            {
                var newScope = new Scope(scope, p);
                foreach (var element in Elements)
                {
                    strs.Add(element.ToStr(newScope));
                }
            }
            return String.Join("\n", strs);
        }
    }

    class Scope
    {
        Dictionary<string, Value> values;
        Scope parent;

        public Scope()
        {

        }

        public Scope(KeyValuePair<string, Value>[] values)
        {
            this.values = values.ToDictionary(v => v.Key, v => v.Value);
        }

        public Scope(Scope parent, KeyValuePair<string, Value>[] values)
        {
            this.parent = parent;
            this.values = values.ToDictionary(v => v.Key, v => v.Value);
        }

        public Value Get(string key)
        {
            if (values.TryGetValue(key, out Value v))
            {
                return v;
            }
            if (parent != null)
            {
                return parent.Get(key);
            }
            return null;
        }
    }
}
