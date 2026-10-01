using PPDCoreModel.Data;
using PPDEditor.Command.PPDSheet;
using PPDEditorCommon;
using System.Numerics;
using PPDMath;
using System.Collections.Generic;

namespace PPDEditor.FlowScript
{
    class ChangeMarkPropertyManager(PPDSheet sheet, Layer layer, IDProvider idProvider) : IChangeMarkPropertyManager
    {
        private readonly List<TaskBase> tasks = [];

        public PPDSheet Sheet
        {
            get;
            private set;
        } = sheet;

        public Layer Layer
        {
            get;
            private set;
        } = layer;

        public IDProvider IDProvider
        {
            get;
            private set;
        } = idProvider;

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

        public void ChangeMarkPosition(IEditorMarkInfo mark, System.Numerics.Vector2 position)
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

        abstract class TaskBase(EditorMarkInfo editorMarkInfo)
        {
            public EditorMarkInfo EditorMarkInfo
            {
                get;
                private set;
            } = editorMarkInfo;

            public abstract void Execute(PPDSheet sheet);
        }

        class ChangeMarkPositionTask(EditorMarkInfo editorMarkInfo, Vector2 position) : TaskBase(editorMarkInfo)
        {
            public Vector2 Position
            {
                get;
                private set;
            } = position;

            public override void Execute(PPDSheet sheet)
            {
                sheet.MoveMark(EditorMarkInfo.Mark, Position);
            }
        }

        class ChangeMarkRotationTask(EditorMarkInfo editorMarkInfo, float rotation) : TaskBase(editorMarkInfo)
        {
            public float Rotation
            {
                get;
                private set;
            } = rotation;

            public override void Execute(PPDSheet sheet)
            {
                sheet.RotateMark(EditorMarkInfo.Mark, Rotation);
            }
        }

        class ChangeMarkTypeTask(EditorMarkInfo editorMarkInfo, MarkType markType) : TaskBase(editorMarkInfo)
        {
            public MarkType MarkType
            {
                get;
                private set;
            } = markType;

            public override void Execute(PPDSheet sheet)
            {
                sheet.ChangeMarkType(EditorMarkInfo.Mark, (int)MarkType);
            }
        }

        class ChangeMarkTimeTask(EditorMarkInfo editorMarkInfo, float time) : TaskBase(editorMarkInfo)
        {
            public float Time
            {
                get;
                private set;
            } = time;

            public override void Execute(PPDSheet sheet)
            {
                sheet.MoveMarkInSame(EditorMarkInfo.Mark, Time);
            }
        }

        class ChangeMarkParameterTask(EditorMarkInfo editorMarkInfo, string key, string value) : TaskBase(editorMarkInfo)
        {
            public string Key
            {
                get;
                private set;
            } = key;

            public string Value
            {
                get;
                private set;
            } = value;

            public override void Execute(PPDSheet sheet)
            {
                sheet.ChangeParameter(EditorMarkInfo.Mark, Key, Value);
            }
        }

        class RemoveMarkParameterTask(EditorMarkInfo editorMarkInfo, string key) : TaskBase(editorMarkInfo)
        {
            public string Key
            {
                private get;
                set;
            } = key;

            public override void Execute(PPDSheet sheet)
            {
                sheet.RemoveParameter(EditorMarkInfo.Mark, Key);
            }
        }

        class AddMarkTask(ILayer layer, Vector2 position, float angle, float time, PPDCoreModel.Data.MarkType markType) : TaskBase(null)
        {
            public ILayer Layer
            {
                get;
                private set;
            } = layer;

            public Vector2 Position
            {
                get;
                private set;
            } = position;

            public float Angle
            {
                get;
                private set;
            } = angle;

            public float Time
            {
                get;
                private set;
            } = time;

            public MarkType MarkType
            {
                get;
                private set;
            } = markType;

            public override void Execute(PPDSheet sheet)
            {
                sheet.AddMark(Time, Position.X, Position.Y, Angle, (int)MarkType, 0, false);
            }
        }

        class AddExMarkTask(ILayer layer, Vector2 position, float angle, float time, float releaseTime, PPDCoreModel.Data.MarkType markType) : TaskBase(null)
        {
            public ILayer Layer
            {
                get;
                private set;
            } = layer;

            public Vector2 Position
            {
                get;
                private set;
            } = position;

            public float Angle
            {
                get;
                private set;
            } = angle;

            public float Time
            {
                get;
                private set;
            } = time;

            public float ReleaseTime
            {
                get;
                private set;
            } = releaseTime;

            public MarkType MarkType
            {
                get;
                private set;
            } = markType;

            public override void Execute(PPDSheet sheet)
            {
                sheet.AddExMark(Time, ReleaseTime, Position.X, Position.Y, Angle, (int)MarkType, 0, false);
            }
        }

        class AssignMarkIdTask(EditorMarkInfo editorMarkInfo) : TaskBase(editorMarkInfo)
        {
            public override void Execute(PPDSheet sheet)
            {
                sheet.AssignID(EditorMarkInfo.Mark);
            }
        }

        class UnassignMarkIdTask(EditorMarkInfo editorMarkInfo) : TaskBase(editorMarkInfo)
        {
            public override void Execute(PPDSheet sheet)
            {
                sheet.UnassignID(EditorMarkInfo.Mark);
            }
        }

        class RemoveMarkTask(EditorMarkInfo editorMarkInfo) : TaskBase(editorMarkInfo)
        {
            public override void Execute(PPDSheet sheet)
            {
                sheet.RemoveMark(EditorMarkInfo.Mark);
            }
        }
    }
}
