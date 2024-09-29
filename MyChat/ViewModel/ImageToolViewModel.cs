﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyChat.DTO;
using MyChat.Messages;
using MyChat.Model;
using MyChat.Service;
using MyChat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyChat.ViewModel
{
    public class ImageToolViewModel : ObservableObject
    {
        private ImageInformationDTO _imageInformation;
        private int _currentIndex = -1;
        private string _prompt = "";
        private string _quality = "Standard";
        private string _size = "Standard";
        private string _style = "Vivid";
        private Visibility _promptVisibility = Visibility.Collapsed;
        private BitmapImage _currentBitmapImage;
        private bool _inRefineMode = false;
        private Cursor? _currentCursor = null;

        private readonly IImageService _imageService;
        private readonly IDialogUtil _dialogUtil;
        private readonly IToolService _toolService;

        public ICommand GenerateButtonCommand { get; }
        public ICommand NextImageButtonCommand { get; }
        public ICommand PreviousImageButtonCommand { get; }
        public ICommand ShowImageInfoCommand { get; }
        public ICommand MoveImageButtonCommand { get; }
        public ICommand RefineButtonCommand { get; }
        public ICommand EditImageButtonCommand { get; }
        public ICommand DeleteImageButtonCommand { get; }
        public ICommand GoToStartButtonCommand { get; }
        public ICommand GoToEndButtonCommand { get; }
        public ICommand SkipForwardButtonCommand { get; }
        public ICommand SkipBackwardButtonCommand { get; }
        public ImageToolViewModel(IImageService imageService, IDialogUtil dialogUtil, IToolService toolService)
        {
            GenerateButtonCommand = new AsyncRelayCommand(OnGenerateButtonClickedAsync);
            NextImageButtonCommand = new RelayCommand(OnNextImageButtonClicked);
            PreviousImageButtonCommand = new RelayCommand(OnPreviousImageButtonClicked);
            ShowImageInfoCommand = new RelayCommand(OnShowImageInfoClicked);
            MoveImageButtonCommand = new RelayCommand(OnMoveImageButtonClicked);
            RefineButtonCommand = new RelayCommand(OnRefineButtonClicked);
            EditImageButtonCommand = new RelayCommand(OnEditImageButtonClicked);
            DeleteImageButtonCommand = new RelayCommand(OnDeleteImageButtonClicked);
            GoToStartButtonCommand = new RelayCommand(OnGoToStartButtonClicked);
            GoToEndButtonCommand = new RelayCommand(OnGoToEndButtonClicked);
            SkipForwardButtonCommand = new RelayCommand(OnSkipForwardButtonClicked);
            SkipBackwardButtonCommand = new RelayCommand(OnSkipBackwardButtonClicked);

            _imageService = imageService;
            _dialogUtil = dialogUtil;
            _toolService = toolService;
            _imageInformation = _imageService.GetImageInformation();

            if (_imageInformation.ImageFilePaths.Count > 0)
            {
                _currentIndex = _imageInformation.ImageFilePaths.Count - 1;
            }

            var bitmapImage = GetImageSource(GetImagePath(_currentIndex));
            _currentBitmapImage = bitmapImage;

            WeakReferenceMessenger.Default.Register<ImageToolWindowStateMessage>(this, (r, m) => OnImageToolWindowState(m));
        }

        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value);
        }

        public string Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        public string Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public string Style
        {
            get => _style;
            set => SetProperty(ref _style, value);
        }

        public string PageIndicator
        {
            get => $"{(_currentIndex >= 0 ? _currentIndex+1 : 0)} / {_imageInformation.ImageFilePaths.Count}";
        }

        public ImageSource CurrentImageSource
        {
            get => _currentBitmapImage;
            private set
            {
                if (value is BitmapImage bitmapImage)
                {
                    SetProperty(ref _currentBitmapImage, bitmapImage);
                }
                else
                {
                    BitmapImage placeholder = GetImageSource(GetImagePath(-1));
                    SetProperty(ref _currentBitmapImage, placeholder);
                }
            }
        }

        public string CurrentImagePrompt
        {
            get => GetImageMetadata(_currentIndex).Prompt;
        }

        public Visibility PromptTextVisibility
        {
            get => _promptVisibility;
            set => SetProperty(ref _promptVisibility, value);
        }

        public bool ToolbarControlEnableState
        {
            get => !_inRefineMode;
        }

        public double ToolbarControlOpacityState
        {
            get => _inRefineMode ? 0.5 : 1.0;
        }

        public string GenerateButtonCaption
        {
            get => _inRefineMode ? "Refine" : "Generate";
        }

        public Cursor? CurrentCursorState
        {
            get => _currentCursor;
            set => SetProperty(ref _currentCursor, value);
        }

        private string GetImagePath(int index)
        {
            return index >= 0 ? _imageInformation.ImageFilePaths[index] : "Images/placeholder.png";
        }

        private ImageToolMetadata GetImageMetadata(int index)
        {
            ImageToolMetadata? metadata = null;

            if (index >= 0)
            {
                string imagePath = GetImagePath(index);
                _imageInformation.ImageMetadata.TryGetValue(imagePath, out metadata);
            }

            return metadata is not null ? metadata : new("No image prompt available", "Standard", "Standard", "Vivid");
        }

        public static BitmapImage GetImageSource(string imagePath)
        {
            BitmapImage newBitmapImage = new();
            newBitmapImage.BeginInit();
            newBitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            newBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            newBitmapImage.EndInit();
            newBitmapImage.Freeze();

            return newBitmapImage;
        }

        public void SetImageSource(string imagePath)
        {
            _currentBitmapImage.Freeze();            
            var bitmapImage = GetImageSource(imagePath);
            CurrentImageSource = bitmapImage;
        }

        public void SetImageSource(int index)
        {
            SetImageSource(GetImagePath(index));
        }

        public async Task OnGenerateButtonClickedAsync()
        {
            if (string.IsNullOrEmpty(Prompt))
            {
                _dialogUtil.ShowErrorMessage("You need to specify a prompt to generate an image.");
                return;
            }

            CurrentCursorState = Cursors.Wait;
            SetImageSource(-1);

            string imagePath = _inRefineMode && _currentIndex >= 0 ? GetImagePath(_currentIndex) : "";
            var newImagePath = await _imageService.GenerateAsync(Prompt, Quality, Size, Style, imagePath);
            CurrentCursorState = null;

            if (string.IsNullOrEmpty(newImagePath))
            {
                _dialogUtil.ShowErrorMessage("Failed to generate image");
                return;
            }

            if (!_inRefineMode)
            {
                _imageInformation.ImageFilePaths.Add(newImagePath);
                _currentIndex = _imageInformation.ImageFilePaths.Count - 1;
            }

            _imageInformation.ImageMetadata[newImagePath] = new(Prompt, Quality, Size, Style);
            SetImageSource(_currentIndex);
            Prompt = "";
            _inRefineMode = false;

            OnPropertyChanged("");
        }

        public void OnNextImageButtonClicked()
        {
            ChangeCurrentIndex(1);
        }

        public void OnPreviousImageButtonClicked()
        {
            ChangeCurrentIndex(-1);
        }

        private void ChangeCurrentIndex(int offset)
        {
            int pathCount = _imageInformation.ImageFilePaths.Count;

            if (pathCount == 0)
            {
                _currentIndex = -1;
                return;
            }

            int adjustedOffset = offset % pathCount;
            _currentIndex += adjustedOffset;
            _currentIndex = _currentIndex >= 0 ? _currentIndex % pathCount : (pathCount + _currentIndex) % pathCount;
            SetImageSource(_currentIndex);
            _inRefineMode = false;

            OnPropertyChanged(nameof(PageIndicator));
            OnPropertyChanged(nameof(CurrentImagePrompt));
            OnPropertyChanged(nameof(ToolbarControlEnableState));
            OnPropertyChanged(nameof(ToolbarControlOpacityState));
            OnPropertyChanged(nameof(GenerateButtonCaption));
        }

        public void OnGoToStartButtonClicked()
        {
            _currentIndex = 0;
            SetImageSource(_currentIndex);
            _inRefineMode = false;

            OnPropertyChanged(nameof(PageIndicator));
            OnPropertyChanged(nameof(CurrentImagePrompt));
            OnPropertyChanged(nameof(ToolbarControlEnableState));
            OnPropertyChanged(nameof(ToolbarControlOpacityState));
            OnPropertyChanged(nameof(GenerateButtonCaption));
        }

        public void OnGoToEndButtonClicked()
        {
            _currentIndex = _imageInformation.ImageFilePaths.Count - 1;
            SetImageSource(_currentIndex);
            _inRefineMode = false;

            OnPropertyChanged(nameof(PageIndicator));
            OnPropertyChanged(nameof(CurrentImagePrompt));
            OnPropertyChanged(nameof(ToolbarControlEnableState));
            OnPropertyChanged(nameof(ToolbarControlOpacityState));
            OnPropertyChanged(nameof(GenerateButtonCaption));
        }

        public void OnSkipForwardButtonClicked()
        {
            ChangeCurrentIndex(10);
        }

        public void OnSkipBackwardButtonClicked()
        {
            ChangeCurrentIndex(-10);
        }

        public void OnShowImageInfoClicked()
        {
            PromptTextVisibility = PromptTextVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public void OnMoveImageButtonClicked()
        {
            string destinationPath = _dialogUtil.PromptForSaveImagePath();

            if (!string.IsNullOrEmpty(destinationPath))
            {
                int savedIndex = _currentIndex;
                _currentIndex = -1;
                SetImageSource(_currentIndex);
                OnPropertyChanged(nameof(CurrentImageSource));

                string imagePath = GetImagePath(savedIndex);
                _imageService.MoveImage(imagePath, destinationPath);
                _imageInformation = _imageService.GetImageInformation();

                if (savedIndex >= _imageInformation.ImageFilePaths.Count)
                {
                    savedIndex = _imageInformation.ImageFilePaths.Count - 1;
                }

                _currentIndex = savedIndex;
                SetImageSource(savedIndex);
                OnPropertyChanged("");
            }
        }

        public void OnRefineButtonClicked()
        {
            var metadata = GetImageMetadata(_currentIndex);
            Prompt = metadata.Prompt;
            Quality = metadata.Quality;
            Size = metadata.Size;
            Style = metadata.Style;
            PromptTextVisibility = Visibility.Collapsed;

            _inRefineMode = true;
            OnPropertyChanged(nameof(ToolbarControlEnableState));
            OnPropertyChanged(nameof(ToolbarControlOpacityState));
            OnPropertyChanged(nameof(GenerateButtonCaption));
        }

        public void OnEditImageButtonClicked()
        {
            _imageService.OpenImageInPreferredEditor(GetImagePath(_currentIndex));
        }

        public void OnDeleteImageButtonClicked()
        {
            bool confirmed = _dialogUtil.PromptForConfirmation("This image will be permanently deleted. Do you wish to continue?");

            if (confirmed)
            {
                int savedIndex = _currentIndex;
                _currentIndex = -1;
                SetImageSource(_currentIndex);
                OnPropertyChanged(nameof(CurrentImageSource));

                string imagePath = GetImagePath(savedIndex);
                _imageService.DeleteImage(imagePath);
                _imageInformation = _imageService.GetImageInformation();

                if (savedIndex >= _imageInformation.ImageFilePaths.Count)
                {
                    savedIndex = _imageInformation.ImageFilePaths.Count - 1;
                }

                _currentIndex = savedIndex;
                SetImageSource(savedIndex);
                OnPropertyChanged("");
            }
        }

        private void OnImageToolWindowState(ImageToolWindowStateMessage m)
        {
            if (m.StateAction == ImageToolWindowStateAction.Startup)
            {
                _toolService.SubscribeToSetImageGenerationPrompt(ToolService_SetPrompt);
            }
            else if (m.StateAction == ImageToolWindowStateAction.Shutdown)
            {
                _toolService.UnsubscribeFromSetImageGenerationPrompt(ToolService_SetPrompt);
                WeakReferenceMessenger.Default.Unregister<ImageToolWindowStateMessage>(this);
            }
        }

        private void ToolService_SetPrompt(object? sender, ImageGenerationPromptEventArgs e)
        {
            Prompt = e.Prompt;
        }
    }
}
