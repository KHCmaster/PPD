using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PPDConfiguration;
using PPDEditorCommon.Dialog.Message;
using PPDEditorCommon.Dialog.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PPDEditorCommon.Dialog.ViewModel
{
    public class NewProjectWindowViewModel : ViewModelBase
    {
        const string ProjectTemplatesPath = "ProjectTemplates";

        private ListViewModel<TemplateInfo> selectedItem;
        private string name;
        private string folderPath;
        private string moviePath;
        private bool isOkEnabled;
        private string validateResultMessage;
        private ICommand loadedCommand;
        private ICommand okCommand;
        private ICommand cancelCommand;
        private ICommand selectedItemChangedCommand;
        private ICommand referLocationCommand;
        private ICommand referMovieCommand;

        public TreeNodeViewModel<TemplateInfo> Root
        {
            get;
            private set;
        }

        public ObservableCollection<ListViewModel<TemplateInfo>> Templates
        {
            get;
            private set;
        }

        public ListViewModel<TemplateInfo> SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    RaisePropertyChanged("SelectedItem");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    RaisePropertyChanged("Name");
                    Validate();
                }
            }
        }

        public string FolderPath
        {
            get { return folderPath; }
            set
            {
                if (folderPath != value)
                {
                    folderPath = value;
                    RaisePropertyChanged("FolderPath");
                    Validate();
                }
            }
        }

        public string MoviePath
        {
            get { return moviePath; }
            set
            {
                if (moviePath != value)
                {
                    moviePath = value;
                    RaisePropertyChanged("MoviePath");
                    Validate();
                }
            }
        }

        public bool IsOkEnabled
        {
            get { return isOkEnabled; }
            private set
            {
                if (isOkEnabled != value)
                {
                    isOkEnabled = value;
                    RaisePropertyChanged("IsOkEnabled");
                }
            }
        }

        public string ValidateResultMessage
        {
            get { return validateResultMessage; }
            private set
            {
                if (validateResultMessage != value)
                {
                    validateResultMessage = value;
                    RaisePropertyChanged("ValidateResultMessage");
                }
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return loadedCommand ?? (loadedCommand = new RelayCommand(LoadedCommand_Execute));
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return okCommand ?? (okCommand = new RelayCommand(OkCommand_Execute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return cancelCommand ?? (cancelCommand = new RelayCommand(CancelCommand_Execute));
            }
        }

        public ICommand SelectedItemChangedCommand
        {
            get
            {
                return selectedItemChangedCommand ?? (selectedItemChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(
                    SelectedItemChangedCommand_Execute));
            }
        }

        public ICommand ReferLocationCommand
        {
            get
            {
                return referLocationCommand ?? (referLocationCommand = new RelayCommand(ReferLocationCommand_Execute));
            }
        }

        public ICommand ReferMovieCommand
        {
            get
            {
                return referMovieCommand ?? (referMovieCommand = new RelayCommand(ReferMovieCommand_Execute));
            }
        }

        public NewProjectWindowViewModel(LanguageReader languageReader)
        {
            TranslateExtension.Language = languageReader;
            Templates = new ObservableCollection<ListViewModel<TemplateInfo>>();
            Root = new TreeNodeViewModel<TemplateInfo>();
        }

        private void InitializeTemplates()
        {
            if (!Directory.Exists(ProjectTemplatesPath))
            {
                return;
            }

            var templates = new List<TemplateInfo>();
            foreach (var path in Directory.GetFiles(ProjectTemplatesPath, "*.xml"))
            {
                try
                {
                    var templateInfo = new TemplateInfo();
                    templateInfo.Load(path);
                    templates.Add(templateInfo);
                }
                catch (Exception ex)
                {
                    Messenger.Default.Send(new ShowMessageBoxMessage(this, String.Format("Error in Loading {0}", path)));
                    Messenger.Default.Send(new ShowMessageBoxMessage(this, ex.Message));
                }
            }

            foreach (var template in templates)
            {
                var nodes = Root.Children;
                TreeNodeViewModel<TemplateInfo> lastNode = null;
                foreach (var split in template.Type.Split('\\'))
                {
                    var foundNode = nodes.FirstOrDefault(n => n.Text == split);
                    if (foundNode == null)
                    {
                        foundNode = new TreeNodeViewModel<TemplateInfo>
                        {
                            Text = split
                        };
                        nodes.Add(foundNode);
                    }
                    lastNode = foundNode;
                    nodes = foundNode.Children;
                }
                if (lastNode != null)
                {
                    var node = new TreeNodeViewModel<TemplateInfo>
                    {
                        Text = template.Name,
                        Value = template
                    };
                    lastNode.Children.Add(node);
                }
            }
            if (Root.Children.Count > 0)
            {
                Root.Children[0].IsExpanded = true;
                Root.Children[0].IsSelected = true;
                UpdateListView(Root.Children[0]);
            }
        }

        private void SetDefaultName()
        {
            if (!Directory.Exists(FolderPath))
            {
                return;
            }

            var baseName = "NewPPDProject";
            var currentName = baseName;
            var name = Path.ChangeExtension(currentName, "ppdproj");
            var dirPath = Path.Combine(FolderPath, currentName);
            var filePath = Path.Combine(FolderPath, name);
            var iter = 2;
            while (Directory.Exists(dirPath) || File.Exists(filePath))
            {
                currentName = String.Format("{0}{1}", baseName, iter);
                name = Path.ChangeExtension(currentName, "ppdproj");
                dirPath = Path.Combine(FolderPath, currentName);
                filePath = Path.Combine(FolderPath, name);
                iter++;
            }
            Name = currentName;
        }

        private void UpdateListView(TreeNodeViewModel<TemplateInfo> selectedTreeNode)
        {
            Templates.Clear();
            foreach (var value in selectedTreeNode.Values)
            {
                Templates.Add(new ListViewModel<TemplateInfo>
                {
                    Value = value
                });
            }
            if (Templates.Count > 0)
            {
                Templates[0].IsSelected = true;
            }
        }

        private void Validate()
        {
            if (!ValidateImpl())
            {
                IsOkEnabled = false;
                return;
            }
            ValidateResultMessage = "";
            IsOkEnabled = true;
        }

        private bool ValidateImpl()
        {
            if (String.IsNullOrEmpty(Name))
            {
                ValidateResultMessage = TranslateExtension.Language["NameNotInput"];
                return false;
            }
            if (!Utility.CheckValidFileName(Name))
            {
                ValidateResultMessage = TranslateExtension.Language["InvalidCharInName"];
                return false;
            }
            if (!Directory.Exists(FolderPath))
            {
                ValidateResultMessage = TranslateExtension.Language["LocationNotExist"];
                return false;
            }
            var projectDir = Path.Combine(FolderPath, Name);
            var projectFilePath = Path.Combine(FolderPath, Path.ChangeExtension(Name, ".ppdproj"));
            if (Directory.Exists(projectDir) || File.Exists(projectFilePath))
            {
                ValidateResultMessage = TranslateExtension.Language["AlreadyExistProject"];
                return false;
            }

            if (String.IsNullOrEmpty(MoviePath))
            {
                ValidateResultMessage = TranslateExtension.Language["MovieNotInput"];
                return false;
            }
            if (!File.Exists(MoviePath))
            {
                ValidateResultMessage = TranslateExtension.Language["MovieNotExist"];
                return false;
            }

            if (SelectedItem == null)
            {
                ValidateResultMessage = TranslateExtension.Language["TemplateNotSelected"];
                return false;
            }

            return true;
        }

        private void LoadedCommand_Execute()
        {
            InitializeTemplates();
            SetDefaultName();
            Validate();
            Messenger.Default.Send(new SelectMessage(this, "nameTextBox"));
        }

        private void OkCommand_Execute()
        {
            Messenger.Default.Send(new CloseWindowMessage(this, true));
        }

        private void CancelCommand_Execute()
        {
            Messenger.Default.Send(new CloseWindowMessage(this, false));
        }

        private void SelectedItemChangedCommand_Execute(RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as TreeNodeViewModel<TemplateInfo>;
            if (node == null)
            {
                return;
            }
            UpdateListView(node);
        }

        private void ReferLocationCommand_Execute()
        {
            var message = new FolderBrowserDialogMessage(this)
            {
                SelectedPath = FolderPath
            };
            Messenger.Default.Send(message);
            if (message.Result)
            {
                FolderPath = message.SelectedPath;
            }
        }

        private void ReferMovieCommand_Execute()
        {
            var message = new OpenFileDialogMessage(this, TranslateExtension.Language["MovieFilter"])
            {
                FileName = MoviePath
            };
            Messenger.Default.Send(message);
            if (message.Result)
            {
                MoviePath = message.FileName;
            }
        }
    }
}
