using PPDCoreModel.Data;
using PPDEditor.Command.PPDSheet;
using PPDEditorCommon;
using SharpDX;
using System.Collections.Generic;

namespace PPDEditor.FlowScript
{
    class ChangeMarkPropertyManager : IChangeMarkPropertyManager
    {
        private List<TaskBase> tasks;

        public PPDSheet Sheet
        {
            get;
            private set;
        }

        public Layer Layer
        {
            get;
            private set;
        }

        public IDProvider IDProvider
        {
            get;
            private set;
        }

        public ChangeMarkPropertyManager(PPDSheet sheet, Layer layer, IDProvider idProvider)
        {
            tasks = new List<TaskBase>();
            Sheet = sheet;
            Layer = layer;
            IDProvider = idProvider;
        }

        public void Execute()
        {
            if (tasks.Count == 0)
            {
                return;
            }

            Sheet.StartGroupCommand();
            foreach (var task in tasks)
            {
                task.Execute(Sheet);
            }
            Sheet.EndGroupCommand();
        }

        #region IChangeMarkPropertyManager メンバー

        public void ChangeMarkAngle(IEditorMarkInfo mark, float angle)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new ChangeMarkRotationTask(editorMarkInfo, angle));
            editorMarkInfo.Angle = angle;
        }

        public void ChangeMarkPosition(IEditorMarkInfo mark, SharpDX.Vector2 position)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new ChangeMarkPositionTask(editorMarkInfo, position));
            editorMarkInfo.Position = position;
        }

        public void ChangeMarkType(IEditorMarkInfo mark, PPDCoreModel.Data.MarkType markType)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new ChangeMarkTypeTask(editorMarkInfo, markType));
            editorMarkInfo.Type = markType;
        }

        public void ChangeMarkTime(IEditorMarkInfo mark, float time)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new ChangeMarkTimeTask(editorMarkInfo, time));
            editorMarkInfo.Time = time;
        }

        public void ChangeParameter(IEditorMarkInfo mark, string key, string value)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new ChangeMarkParameterTask(editorMarkInfo, key, value));
            editorMarkInfo.Parameters[key] = value;
        }

        public void RemoveParameter(IEditorMarkInfo mark, string key)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new RemoveMarkParameterTask(editorMarkInfo, key));
            editorMarkInfo.Parameters.Remove(key);
        }

        public void AddMark(Vector2 position, float angle, float time, PPDCoreModel.Data.MarkType markType)
        {
            tasks.Add(new AddMarkTask(Layer, position, angle, time, markType));
        }

        public void AddExMark(Vector2 position, float angle, float time, float endTime, PPDCoreModel.Data.MarkType markType)
        {
            tasks.Add(new AddExMarkTask(Layer, position, angle, time, endTime, markType));
        }

        public void AssignID(IEditorMarkInfo mark)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new AssignMarkIdTask(editorMarkInfo));
            editorMarkInfo.Mark.ID = IDProvider.Next();
        }

        public void UnassignID(IEditorMarkInfo mark)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new UnassignMarkIdTask(editorMarkInfo));
            editorMarkInfo.Mark.ID = 0;
        }

        public void Remove(IEditorMarkInfo mark)
        {
            var editorMarkInfo = (EditorMarkInfo)mark;
            tasks.Add(new RemoveMarkTask(editorMarkInfo));
        }

        #endregion

        abstract class TaskBase
        {
            public EditorMarkInfo EditorMarkInfo
            {
                get;
                private set;
            }

            protected TaskBase(EditorMarkInfo editorMarkInfo)
            {
                EditorMarkInfo = editorMarkInfo;
            }

            public abstract void Execute(PPDSheet sheet);
        }

        class ChangeMarkPositionTask : TaskBase
        {
            public Vector2 Position
            {
                get;
                private set;
            }

            public ChangeMarkPositionTask(EditorMarkInfo editorMarkInfo, Vector2 position) :
                base(editorMarkInfo)
            {
                Position = position;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.MoveMark(EditorMarkInfo.Mark, Position);
            }
        }

        class ChangeMarkRotationTask : TaskBase
        {
            public float Rotation
            {
                get;
                private set;
            }

            public ChangeMarkRotationTask(EditorMarkInfo editorMarkInfo, float rotation) :
                base(editorMarkInfo)
            {
                Rotation = rotation;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.RotateMark(EditorMarkInfo.Mark, Rotation);
            }
        }

        class ChangeMarkTypeTask : TaskBase
        {
            public MarkType MarkType
            {
                get;
                private set;
            }

            public ChangeMarkTypeTask(EditorMarkInfo editorMarkInfo, MarkType markType)
                : base(editorMarkInfo)
            {
                MarkType = markType;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.ChangeMarkType(EditorMarkInfo.Mark, (int)MarkType);
            }
        }

        class ChangeMarkTimeTask : TaskBase
        {
            public float Time
            {
                get;
                private set;
            }

            public ChangeMarkTimeTask(EditorMarkInfo editorMarkInfo, float time)
                : base(editorMarkInfo)
            {
                Time = time;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.MoveMarkInSame(EditorMarkInfo.Mark, Time);
            }
        }

        class ChangeMarkParameterTask : TaskBase
        {
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

            public ChangeMarkParameterTask(EditorMarkInfo editorMarkInfo, string key, string value)
                : base(editorMarkInfo)
            {
                Key = key;
                Value = value;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.ChangeParameter(EditorMarkInfo.Mark, Key, Value);
            }
        }

        class RemoveMarkParameterTask : TaskBase
        {
            public string Key
            {
                private get;
                set;
            }

            public RemoveMarkParameterTask(EditorMarkInfo editorMarkInfo, string key)
                : base(editorMarkInfo)
            {
                Key = key;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.RemoveParameter(EditorMarkInfo.Mark, Key);
            }
        }

        class AddMarkTask : TaskBase
        {
            public ILayer Layer
            {
                get;
                private set;
            }

            public Vector2 Position
            {
                get;
                private set;
            }

            public float Angle
            {
                get;
                private set;
            }

            public float Time
            {
                get;
                private set;
            }

            public MarkType MarkType
            {
                get;
                private set;
            }

            public AddMarkTask(ILayer layer, Vector2 position, float angle, float time, PPDCoreModel.Data.MarkType markType)
                : base(null)
            {
                Layer = layer;
                Position = position;
                Angle = angle;
                Time = time;
                MarkType = markType;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.AddMark(Time, Position.X, Position.Y, Angle, (int)MarkType, 0, false);
            }
        }

        class AddExMarkTask : TaskBase
        {
            public ILayer Layer
            {
                get;
                private set;
            }

            public Vector2 Position
            {
                get;
                private set;
            }

            public float Angle
            {
                get;
                private set;
            }

            public float Time
            {
                get;
                private set;
            }

            public float ReleaseTime
            {
                get;
                private set;
            }

            public MarkType MarkType
            {
                get;
                private set;
            }

            public AddExMarkTask(ILayer layer, Vector2 position, float angle, float time, float releaseTime, PPDCoreModel.Data.MarkType markType)
                : base(null)
            {
                Layer = layer;
                Position = position;
                Angle = angle;
                Time = time;
                ReleaseTime = releaseTime;
                MarkType = markType;
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.AddExMark(Time, ReleaseTime, Position.X, Position.Y, Angle, (int)MarkType, 0, false);
            }
        }

        class AssignMarkIdTask : TaskBase
        {
            public AssignMarkIdTask(EditorMarkInfo editorMarkInfo)
                : base(editorMarkInfo)
            {

            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.AssignID(EditorMarkInfo.Mark);
            }
        }

        class UnassignMarkIdTask : TaskBase
        {
            public UnassignMarkIdTask(EditorMarkInfo editorMarkInfo)
                : base(editorMarkInfo)
            {
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.UnassignID(EditorMarkInfo.Mark);
            }
        }

        class RemoveMarkTask : TaskBase
        {
            public RemoveMarkTask(EditorMarkInfo editorMarkInfo) :
                base(editorMarkInfo)
            {
            }

            public override void Execute(PPDSheet sheet)
            {
                sheet.RemoveMark(EditorMarkInfo.Mark);
            }
        }
    }
}
