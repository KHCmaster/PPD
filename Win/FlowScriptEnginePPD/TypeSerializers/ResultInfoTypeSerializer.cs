using FlowScriptEngine;
using PPDFramework;
using System;
using System.Xml.Linq;

namespace FlowScriptEnginePPD.TypeSerializers
{
    public class ResultInfoTypeSerializer : TypeSerializerBase
    {
        public override Type Type
        {
            get { return typeof(ResultInfo); }
        }

        public override void Serialize(Serializer serializer, XElement element, object value)
        {
            var resultInfo = (ResultInfo)value;
            AddNewElement(serializer, element, "ID", resultInfo.ID);
            AddNewElement(serializer, element, "Difficulty", resultInfo.Difficulty);
            AddNewElement(serializer, element, "ResultType", resultInfo.ResultEvaluate);
            AddNewElement(serializer, element, "Score", resultInfo.Score);
            AddNewElement(serializer, element, "CoolCount", resultInfo.CoolCount);
            AddNewElement(serializer, element, "GoodCount", resultInfo.GoodCount);
            AddNewElement(serializer, element, "SafeCount", resultInfo.SafeCount);
            AddNewElement(serializer, element, "SadCount", resultInfo.SadCount);
            AddNewElement(serializer, element, "WorstCount", resultInfo.WorstCount);
            AddNewElement(serializer, element, "MaxCombo", resultInfo.MaxCombo);
            AddNewElement(serializer, element, "FinishTime", resultInfo.FinishTime);
            AddNewElement(serializer, element, "PlayDateTime", resultInfo.Date);
        }
    }
}
