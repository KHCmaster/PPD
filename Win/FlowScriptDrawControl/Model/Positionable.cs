using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowScriptDrawControl.Model
{
    public class Positionable : ObservableObject
    {
        private Point position;
        private Transform transform = Transform.Identity;
        private ICommand sizeChangedCommand;

        public event Action SizeChanged;

        public Point Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    Transform = new TranslateTransform(position.X, position.Y);
                    RaisePropertyChanged("Position");
                }
            }
        }

        public Transform Transform
        {
            get { return transform; }
            private set
            {
                if (transform != value)
                {
                    transform = value;
                    RaisePropertyChanged("Transform");
                    RaisePropertyChanged("Transform2");
                }
            }
        }

        public ICommand SizeChangedCommand
        {
            get
            {
                return sizeChangedCommand ?? (sizeChangedCommand = new RelayCommand(
                    SizeChangedCommand_Execute));
            }
        }

        private void SizeChangedCommand_Execute()
        {
            SizeChanged?.Invoke();
        }
    }
}