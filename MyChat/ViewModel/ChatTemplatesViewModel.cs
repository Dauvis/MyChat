using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public class ChatTemplatesViewModel : ObservableObject
    {
        private ChatTemplate _currentTemplate;
        private TreeNode? _currentNode;
        private bool _showTreeState = true;
        private bool _showListState = false;
        private ObservableCollection<TreeNode> _treeNodes = [];
        private List<ChatTemplate> _chatTemplateList = [];
        private readonly IDialogUtil _dialogUtil;
        private readonly ISettingsService _settingsService;
        private readonly IChatTemplateRepository _templateRepository;
        private readonly IDocumentService _documentService;

        public ICommand NewTemplateClickCommand { get; }
        public ICommand DeleteTemplateClickCommand { get; }
        public ICommand ShowTreeClickCommand { get; }
        public ICommand ShowListClickCommand { get; }
        public ICommand NewChatClickCommand { get; }
        public ICommand SaveClickCommand { get; }
        public ICommand DiscardClickCommand { get; }
        public ICommand CancelClickCommand { get; }

        public ChatTemplatesViewModel(IDialogUtil dialogUtil, ISettingsService settingsService, 
            IChatTemplateRepository templateRepository, IDocumentService documentService)
        {
            _dialogUtil = dialogUtil;
            _settingsService = settingsService;
            _templateRepository = templateRepository;
            _documentService = documentService;
            NewTemplateClickCommand = new RelayCommand(OnNewTemplateClicked);
            DeleteTemplateClickCommand = new RelayCommand(OnDeleteTemplateClicked);
            ShowTreeClickCommand = new RelayCommand(OnShowTreeClick);
            ShowListClickCommand = new RelayCommand(OnShowListClick);
            NewChatClickCommand = new RelayCommand(OnNewChatClick);
            SaveClickCommand = new RelayCommand(OnSaveClick);
            DiscardClickCommand = new RelayCommand(OnDiscardClick);
            CancelClickCommand = new RelayCommand(OnCancelClick);

            _currentTemplate = new();
            _currentNode = null;
            WeakReferenceMessenger.Default.Register<WindowEventMessage>(this, (r, m) => OnWindowState(m));
        }

        private void OnWindowState(WindowEventMessage m)
        {
            if (m.Type == WindowType.ChatTemplate)
            {
                if (m.State == WindowEventType.Loaded)
                {
                    _chatTemplateList = _templateRepository.Fetch();

                    var settings = _settingsService.GetUserSettings();
                    ShowTreeState = settings.IsTemplateTreeView;
                    ShowListState = !settings.IsTemplateTreeView;
                    LoadTreeView();
                }
                else if (m.State == WindowEventType.Closing)
                {
                    if (CurrentTemplate.EnableSaveDiscard)
                    {
                        bool result = _dialogUtil.PromptForConfirmation("Template has not been saved. Do you wish to continue?");

                        if (!result)
                        {
                            m.Cancel = true;
                            return;
                        }
                    }

                    var settings = _settingsService.GetUserSettings();
                    settings.IsTemplateTreeView = ShowTreeState;
                    _settingsService.SetUserSettings(settings);

                    WeakReferenceMessenger.Default.Unregister<WindowEventMessage>(this);
                }
                else if (m.State == WindowEventType.Selection)
                {
                    var parameters = (dynamic?)m.ObjectParameter;

                    if (parameters is not null)
                    {
                        TreeNode? newNode = parameters.NewItem;
                        TreeNode? oldNode = parameters.OldItem;

                        TreeViewSelectionChanged(newNode, oldNode);
                    }
                }
            }
        }

        private bool _ignoreSelectionChange;

        private void TreeViewSelectionChanged(TreeNode? newNode, TreeNode? oldNode)
        {
            if (_ignoreSelectionChange || newNode is null || (newNode.IsLeaf && newNode.TemplateId == oldNode?.TemplateId))
            {
                return;
            }

            var newTemplate = _chatTemplateList.Where(t => t.Identifier == newNode.TemplateId).FirstOrDefault();
            var oldTemplate = oldNode?.TemplateId == _currentTemplate.Identifier ? _currentTemplate : 
                _chatTemplateList.Where(t => t.Identifier == oldNode?.TemplateId).FirstOrDefault();

            if (oldNode is not null && oldTemplate is not null && oldTemplate.EnableSaveDiscard)
            {
                _dialogUtil.ShowErrorMessage("Current selection has been changed. Either save or discard the changes and try again");

                // Okay, at the point where this logic is being called, the TreeView
                // apparently has some things that it still needs to do and it gets
                // upset and continuously insists that the nodes are changing
                // (infinite recusion). The following delays the change of selection to
                // allow the TreeView to settle down and not be so grumpy.
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _ignoreSelectionChange = true;
                    oldNode.IsSelected = true;
                    _ignoreSelectionChange = false;
                }), System.Windows.Threading.DispatcherPriority.Render);

                return;
            }

            if (newTemplate is not null)
            {
                CurrentTemplate = newNode.IsLeaf ? newTemplate : new();
                CurrentTemplate.Selected = true;
                _currentNode = newNode;

                if (oldNode is not null && oldTemplate is not null)
                {
                    oldTemplate.Selected = false;
                }
            }
            else
            {
                CurrentTemplate = new();
                _currentNode = newNode;
            }

            OnPropertyChanged(nameof(EditingAllowed));
        }

        private void LoadTreeView()
        {
            TreeNodes.Clear();

            foreach (var template in _chatTemplateList.OrderBy(t => t.Name))
            {
                AddTemplateToTree(template);
            }
        }

        public ChatTemplate CurrentTemplate
        {
            get => _currentTemplate;
            set
            {
                _currentTemplate = CloneTemplate(value);
                OnPropertyChanged("");
            }
        }

        public ObservableCollection<TreeNode> TreeNodes
        {
            get => _treeNodes;
            set => SetProperty(ref _treeNodes, value);
        }

        public bool ShowTreeState
        {
            get => _showTreeState;
            set => SetProperty(ref _showTreeState, value);
        }

        public bool ShowListState
        {
            get => _showListState;
            set => SetProperty(ref _showListState, value);
        }

        public bool EditingAllowed
        {
            get
            {
                bool isLeaf = false;

                if (_currentNode is not null)
                {
                    isLeaf = _currentNode.IsLeaf;
                }

                bool emptyIdentifier = false;
                bool isDirty = false;

                if (_currentTemplate is not null)
                {
                    emptyIdentifier = _currentTemplate.Identifier == default;
                    isDirty = _currentTemplate.EnableSaveDiscard;
                }

                return isLeaf || emptyIdentifier && isDirty;
            }
        }

        private void OnNewTemplateClicked()
        {
            ChatTemplate newTemplate = new()
            {
                Name = "New Template",
                Category = _currentNode?.Category ?? ""
            };

            var userSettings = _settingsService.GetUserSettings();

            newTemplate.Tone = userSettings.DefaultTone;
            newTemplate.Instructions = userSettings.DefaultInstructions;

            CurrentTemplate = newTemplate;
            CurrentTemplate.EnableSaveDiscard = true;
            CurrentTemplate.Selected = true;
            OnPropertyChanged(nameof(EditingAllowed));
        }

        private TreeNode AddTemplateToTree(ChatTemplate template)
        {
            var parent = FindParentForTemplate(template);
            TreeNode newNode = new(template.Name, template.Category, template.Identifier, parent);

            if (parent is not null)
            {
                parent.Children.Add(newNode);
            }
            else if (ShowListState)
            {
                TreeNodes.Add(newNode);
            }
            else if (ShowTreeState)
            {
                TreeNode newParent = new(template.Category, template.Category);
                newParent.Children.Add(newNode);
                TreeNodes.Add(newParent);
            }

            return newNode;
        }

        private void AddTemplateToList(ChatTemplate template)
        {
            if (template.Identifier == default)
            {
                template.Identifier = Guid.NewGuid();
            }

            template.Selected = false;
            template.EnableSaveDiscard = false;

            _chatTemplateList.Add(template);
        }

        private void RemoveTemplateFromTree(ChatTemplate template, TreeNode? node)
        {
            if (node is not null)
            {
                var parent = FindParentForTemplate(template);

                if (parent is not null)
                {
                    parent.Children.Remove(node);

                    if (parent.Children.Count == 0)
                    {
                        TreeNodes.Remove(parent);
                    }
                }
                else
                {
                    TreeNodes.Remove(node);
                }
            }
        }

        private void RemoveTemplateFromList(ChatTemplate template)
        {
            if (template.Identifier != default)
            {
                var listTemplate = _chatTemplateList.Where(t => t.Identifier == template.Identifier).FirstOrDefault();

                if (listTemplate is not null)
                {
                    _chatTemplateList.Remove(listTemplate);
                }
            }
        }

        private TreeNode? FindParentForTemplate(ChatTemplate template)
        {
            if (ShowListState)
            {
                return null;
            }

            var parent = TreeNodes.Where(n => n.Category == template.Category).FirstOrDefault();

            return parent;
        }

        private void OnDeleteTemplateClicked()
        {
            if (_currentNode?.IsLeaf == false)
            {
                return;
            }

            RemoveTemplateFromTree(CurrentTemplate, _currentNode);
            RemoveTemplateFromList(CurrentTemplate);
            _templateRepository.Delete(CurrentTemplate);

            CurrentTemplate = new();
        }

        private void OnShowTreeClick()
        {
            ShowTreeState = true;
            ShowListState = false;
            CurrentTemplate = new();
            _currentNode = null;
            LoadTreeView();
        }

        private void OnShowListClick()
        {
            ShowTreeState = false;
            ShowListState = true;
            CurrentTemplate = new();
            _currentNode = null;
            LoadTreeView();
        }

        private void OnNewChatClick()
        {
            if (CurrentTemplate.EnableSaveDiscard)
            {
                _dialogUtil.ShowErrorMessage("Template must be saved before creating a chat");
                return;
            }

            if (!Validate(CurrentTemplate))
            {
                _dialogUtil.ShowErrorMessage("Template is not valid for creating chats");
                return;
            }

            _documentService.CreateDocument(CurrentTemplate.Tone, CurrentTemplate.Instructions, CurrentTemplate.Topic);
            var settings = _settingsService.GetUserSettings();
            settings.UpdateTemplateMRU(CurrentTemplate);
            _settingsService.SetUserSettings(settings);
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.DoClose, WindowType.ChatTemplate));
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Refresh, WindowType.Main));
        }

        private void OnSaveClick()
        {
            if (!Validate(CurrentTemplate))
            {
                _dialogUtil.ShowErrorMessage("Template not valid to be saved. Must have a name, category, and tone");
                return;
            }

            bool addToTree = CurrentTemplate.Identifier == default;

            RemoveTemplateFromList(CurrentTemplate);
            AddTemplateToList(CurrentTemplate);

            if (addToTree)
            {
                var node = AddTemplateToTree(CurrentTemplate);

                if (node is not null)
                {
                    node.IsSelected = true;
                }
            }

            _templateRepository.Save(CurrentTemplate);

            CurrentTemplate.Selected = true;
            CurrentTemplate.EnableSaveDiscard = false;
        }

        private void OnDiscardClick()
        {
            if (!_dialogUtil.PromptForConfirmation("Are you sure that you want to discard your changes?"))
            {
                return;
            }

            var listTemplate = _chatTemplateList.Where(t => t.Identifier == CurrentTemplate.Identifier).FirstOrDefault();

            if (listTemplate != null)
            {
                CurrentTemplate = listTemplate;
                CurrentTemplate.Selected = true;
            }
            else
            {
                CurrentTemplate = new();
            }
        }

        private void OnCancelClick()
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.DoClose, WindowType.ChatTemplate));
        }

        private static ChatTemplate CloneTemplate(ChatTemplate template)
        {
            return new ChatTemplate()
            {
                Identifier = template.Identifier,
                Category = template.Category,
                Description = template.Description,
                Instructions = template.Instructions,
                Name = template.Name,
                Tone = template.Tone,
                Topic = template.Topic
            };
        }

        private bool Validate(ChatTemplate template)
        {
            return !string.IsNullOrEmpty(template.Name) && !string.IsNullOrEmpty(template.Category) && !string.IsNullOrEmpty(template.Tone);
        }

        public class TreeNode : ObservableObject
        {
            private string _title;
            private Guid _templateId;
            private bool _isSelected;
            private string _category;
            private TreeNode? _parent;
            private ObservableCollection<TreeNode> _children = [];

            public TreeNode(string title, string category, Guid templateId = default, TreeNode? parent = null)
            {
                _title = title;
                _templateId = templateId;
                _category = category;
                _parent = parent;
            }

            public string Title
            {
                get => _title;
                set => SetProperty(ref _title, value);
            }

            public Guid TemplateId
            {
                get => _templateId;
                set => SetProperty(ref _templateId, value);
            }

            public bool IsLeaf
            {
                get => _templateId != default;
            }

            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }

            public string Category
            {
                get => _category;
                set => SetProperty(ref _category, value);
            }

            public ObservableCollection<TreeNode> Children 
            { 
                get => _children; 
                set => SetProperty(ref _children, value); 
            }
            public TreeNode? Parent 
            { 
                get => _parent;
                set => SetProperty(ref _parent, value);
            }
        }
    }
}
