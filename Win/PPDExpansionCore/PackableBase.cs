using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PPDExpansionCore
{
    public abstract class PackableBase
    {
        public abstract DataType DataType
        {
            get;
        }

        private static Dictionary<DataType, Type> packableDict;

        static PackableBase()
        {
            packableDict = new Dictionary<DataType, Type>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsPublic && !type.IsAbstract && type.IsSubclassOf(typeof(PackableBase)))
                {
                    var packable = (PackableBase)Activator.CreateInstance(type);
                    packableDict.Add(packable.DataType, type);
                }
            }
        }

        public static PackableBase Create(DataType dataType)
        {
            return (PackableBase)Activator.CreateInstance(packableDict[dataType]);
        }

        public void Write(PPDPack.V2.PackWriter writer)
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (!propertyInfo.CanRead || propertyInfo.GetCustomAttributes(false).Any(a => a.GetType() == typeof(IgnoreAttribute)))
                {
                    continue;
                }
                var value = propertyInfo.GetValue(this, null);
                if (propertyInfo.PropertyType.IsArray)
                {
                    var array = (Array)value;
                    var elementType = propertyInfo.PropertyType.GetElementType();
                    writer.Write(propertyInfo.Name, w =>
                    {
                        Write(typeof(int), array.Length, w);
                        for (int i = 0; i < array.Length; i++)
                        {
                            var arrayValue = array.GetValue(i);
                            Write(elementType, arrayValue, w);
                        }
                    });
                }
                else
                {
                    writer.Write(propertyInfo.Name, w =>
                    {
                        Write(propertyInfo.PropertyType, value, w);
                    });
                }
            }
        }

        private void Write(Type type, object value, PPDPackStreamWriter w)
        {
            if (type.IsEnum || type == typeof(int))
            {
                WriteInt(w, (int)value);
            }
            else if (type == typeof(bool))
            {
                WriteBoolean(w, (bool)value);
            }
            else if (type == typeof(float))
            {
                WriteFloat(w, (float)value);
            }
            else if (type == typeof(string))
            {
                WriteString(w, (string)value);
            }
            else if (type == typeof(byte))
            {
                WriteByte(w, (byte)value);
            }
            else
            {
                throw new Exception("Not supported type");
            }
        }

        public void Read(PPDPack.V2.PackReader reader)
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (!propertyInfo.CanWrite || propertyInfo.GetCustomAttributes(false).Any(a => a.GetType() == typeof(IgnoreAttribute)))
                {
                    continue;
                }
                var streamReader = reader.Read(propertyInfo.Name);
                if (propertyInfo.PropertyType.IsArray)
                {
                    var elementType = propertyInfo.PropertyType.GetElementType();
                    var arrayLength = ParseInt(streamReader);
                    var array = Array.CreateInstance(elementType, arrayLength);
                    for (int i = 0; i < arrayLength; i++)
                    {
                        array.SetValue(Read(elementType, streamReader), i);
                    }
                    propertyInfo.SetValue(this, array, null);
                }
                else
                {
                    propertyInfo.SetValue(this, Read(propertyInfo.PropertyType, streamReader), null);
                }
            }
        }

        private object Read(Type type, PPDPackStreamReader reader)
        {
            if (type.IsEnum || type == typeof(int))
            {
                return ParseInt(reader);
            }
            else if (type == typeof(float))
            {
                return ParseFloat(reader);
            }
            else if (type == typeof(string))
            {
                return ParseString(reader);
            }
            else if (type == typeof(byte))
            {
                return ParseByte(reader);
            }
            else if (type == typeof(bool))
            {
                return ParseBoolean(reader);
            }
            else
            {
                throw new Exception("Not supported type");
            }
        }

        protected string ParseString(PPDPackStreamReader packStreamReader)
        {
            using (StreamReader streamReader = new StreamReader(packStreamReader, Encoding.UTF8))
            {
                var ret = streamReader.ReadToEnd();
                return ret;
            }
        }

        protected int ParseInt(PPDPackStreamReader packStreamReader)
        {
            byte[] data = new byte[sizeof(int)];
            packStreamReader.Read(data, 0, data.Length);
            var ret = BitConverter.ToInt32(data, 0);
            return ret;
        }

        protected float ParseFloat(PPDPackStreamReader packStreamReader)
        {
            byte[] data = new byte[sizeof(float)];
            packStreamReader.Read(data, 0, data.Length);
            var ret = BitConverter.ToSingle(data, 0);
            return ret;
        }

        protected byte ParseByte(PPDPackStreamReader packStreamReader)
        {
            return (byte)packStreamReader.ReadByte();
        }

        protected bool ParseBoolean(PPDPackStreamReader packStreamReader)
        {
            byte[] data = new byte[sizeof(bool)];
            packStreamReader.Read(data, 0, data.Length);
            var ret = BitConverter.ToBoolean(data, 0);
            return ret;
        }

        protected void WriteString(PPDPackStreamWriter writer, string value)
        {
            var writeData = Encoding.UTF8.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteInt(PPDPackStreamWriter writer, int value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteBoolean(PPDPackStreamWriter writer, bool value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteFloat(PPDPackStreamWriter writer, float value)
        {
            var writeData = BitConverter.GetBytes(value);
            writer.Write(writeData, 0, writeData.Length);
        }

        protected void WriteByte(PPDPackStreamWriter writer, byte value)
        {
            writer.WriteByte(value);
        }
    }
}
