﻿using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Main;
using SkiaSharp;
using System.ComponentModel;
using System.Windows;

namespace PixiEditor.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        private string actionDisplay;
        private bool overrideActionDisplay;

        public static ViewModelMain Current { get; set; }

        public IServiceProvider Services { get; private set; }

        public Action CloseAction { get; set; }

        public event EventHandler OnStartupEvent;

        public RelayCommand OnStartupCommand { get; set; }

        public RelayCommand CloseWindowCommand { get; set; }

        public FileViewModel FileSubViewModel { get; set; }

        public UpdateViewModel UpdateSubViewModel { get; set; }

        public ToolsViewModel ToolsSubViewModel { get; set; }

        public IoViewModel IoSubViewModel { get; set; }

        public LayersViewModel LayersSubViewModel { get; set; }

        public ClipboardViewModel ClipboardSubViewModel { get; set; }

        public UndoViewModel UndoSubViewModel { get; set; }

        public SelectionViewModel SelectionSubViewModel { get; set; }

        public ViewportViewModel ViewportSubViewModel { get; set; }

        public ColorsViewModel ColorsSubViewModel { get; set; }

        public DocumentViewModel DocumentSubViewModel { get; set; }

        public MiscViewModel MiscSubViewModel { get; set; }

        public DiscordViewModel DiscordViewModel { get; set; }

        public DebugViewModel DebugSubViewModel { get; set; }

        public BitmapManager BitmapManager { get; set; }

        public CommandController CommandController { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public StylusViewModel StylusSubViewModel { get; set; }

        public WindowViewModel WindowSubViewModel { get; set; }

        public SearchViewModel SearchSubViewModel { get; set; }

        public RegistryViewModel RegistrySubViewModel { get; set; }

        public IPreferences Preferences { get; set; }

        public string ActionDisplay
        {
            get
            {
                if (OverrideActionDisplay)
                {
                    return actionDisplay;
                }

                return ToolsSubViewModel.ActiveTool.ActionDisplay;
            }
            set
            {
                actionDisplay = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a custom action display should be used. If false the action display of the selected tool will be used.
        /// </summary>
        public bool OverrideActionDisplay
        {
            get => overrideActionDisplay;
            set
            {
                SetProperty(ref overrideActionDisplay, value);
                RaisePropertyChanged(nameof(ActionDisplay));
            }
        }

        public ViewModelMain(IServiceProvider serviceProvider)
        {
            Current = this;
        }

        public void Setup(IServiceProvider services)
        {
            Services = services;

            Preferences = services.GetRequiredService<IPreferences>();

            Preferences.Init();
            BitmapManager = services.GetRequiredService<BitmapManager>();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;

            SelectionSubViewModel = services.GetService<SelectionViewModel>();

            OnStartupCommand = new RelayCommand(OnStartup);
            CloseWindowCommand = new RelayCommand(CloseWindow);

            FileSubViewModel = services.GetService<FileViewModel>();
            ToolsSubViewModel = services.GetService<ToolsViewModel>();
            ToolsSubViewModel.SelectedToolChanged += BitmapManager_SelectedToolChanged;

            IoSubViewModel = services.GetService<IoViewModel>();
            LayersSubViewModel = services.GetService<LayersViewModel>();
            ClipboardSubViewModel = services.GetService<ClipboardViewModel>();
            UndoSubViewModel = services.GetService<UndoViewModel>();
            ViewportSubViewModel = services.GetService<ViewportViewModel>();
            ColorsSubViewModel = services.GetService<ColorsViewModel>();
            ColorsSubViewModel?.SetupPaletteParsers(services);

            ToolsSubViewModel?.SetupTools(services);

            DocumentSubViewModel = services.GetService<DocumentViewModel>();
            DiscordViewModel = services.GetService<DiscordViewModel>();
            UpdateSubViewModel = services.GetService<UpdateViewModel>();
            DebugSubViewModel = services.GetService<DebugViewModel>();

            WindowSubViewModel = services.GetService<WindowViewModel>();
            StylusSubViewModel = services.GetService<StylusViewModel>();
            RegistrySubViewModel = services.GetService<RegistryViewModel>();

            MiscSubViewModel = services.GetService<MiscViewModel>();

            BitmapManager.PrimaryColor = ColorsSubViewModel.PrimaryColor;

            CommandController = services.GetService<CommandController>();
            CommandController.Init(services);
            ShortcutController = new ShortcutController();

            ToolsSubViewModel?.SetupToolsTooltipShortcuts(services);

            SearchSubViewModel = services.GetService<SearchViewModel>();
        }

        /// <summary>
        ///     Resets most variables and controller, so new documents can be handled.
        /// </summary>
        public void ResetProgramStateValues()
        {
            foreach (var document in BitmapManager.Documents)
            {
                document.PreviewLayer.Reset();
            }
        }

        public bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }

        public bool DocumentIsNotNull((SKColor oldColor, SKColor newColor) obj)
        {
            return DocumentIsNotNull(null);
        }

        public void CloseWindow(object property)
        {
            if (!(property is CancelEventArgs))
            {
                throw new ArgumentException();
            }

            ((CancelEventArgs)property).Cancel = !RemoveDocumentsWithSaveConfirmation();
        }

        private void BitmapManager_SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.OldTool != null)
                e.OldTool.PropertyChanged -= SelectedTool_PropertyChanged;
            e.NewTool.PropertyChanged += SelectedTool_PropertyChanged;

            NotifyToolActionDisplayChanged();
            BitmapManager.InputTarget.OnToolChange(e.NewTool);
        }

        private void SelectedTool_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Tool.ActionDisplay))
            {
                NotifyToolActionDisplayChanged();
            }
        }

        private void NotifyToolActionDisplayChanged()
        {
            if (!OverrideActionDisplay) RaisePropertyChanged(nameof(ActionDisplay));
        }

        /// <summary>
        /// Removes documents with unsaved changes confirmation dialog.
        /// </summary>
        /// <returns>If documents was removed successfully.</returns>
        private bool RemoveDocumentsWithSaveConfirmation()
        {
            int docCount = BitmapManager.Documents.Count;
            for (int i = 0; i < docCount; i++)
            {
                BitmapManager.ActiveDocument = BitmapManager.Documents.First();
                bool canceled = !RemoveDocumentWithSaveConfirmation();
                if (canceled)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes document with unsaved changes confirmation dialog.
        /// </summary>
        /// <returns>If document was removed successfully.</returns>
        private bool RemoveDocumentWithSaveConfirmation()
        {
            ConfirmationType result = ConfirmationType.No;

            if (!BitmapManager.ActiveDocument.ChangesSaved)
            {
                result = ConfirmationDialog.Show(DocumentViewModel.ConfirmationDialogMessage, DocumentViewModel.ConfirmationDialogTitle);
                if (result == ConfirmationType.Yes)
                {
                    FileSubViewModel.SaveDocument(false);
                    //cancel was pressed in the save file dialog
                    if (!BitmapManager.ActiveDocument.ChangesSaved)
                        return false;
                }
            }

            if (result != ConfirmationType.Canceled)
            {
                var doc = BitmapManager.ActiveDocument;
                BitmapManager.Documents.Remove(doc);
                doc.Dispose();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnStartup(object parameter)
        {
            OnStartupEvent?.Invoke(this, EventArgs.Empty);
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (e.NewDocument != null)
            {
                e.NewDocument.DocumentSizeChanged += ActiveDocument_DocumentSizeChanged;
            }
        }

        private void ActiveDocument_DocumentSizeChanged(object sender, DocumentSizeChangedEventArgs e)
        {
            BitmapManager.ActiveDocument.ActiveSelection = new Selection(Array.Empty<Coordinates>(), new PixelSize(e.NewWidth, e.NewHeight));
            BitmapManager.ActiveDocument.ChangesSaved = false;
            BitmapManager.ActiveDocument.CenterViewportTrigger.Execute(this, new Size(BitmapManager.ActiveDocument.Width, BitmapManager.ActiveDocument.Height));
        }

        private void BitmapUtility_BitmapChanged(object sender, EventArgs e)
        {
            BitmapManager.ActiveDocument.ChangesSaved = false;
            if (ToolsSubViewModel.ActiveTool is BitmapOperationTool)
            {
                ColorsSubViewModel.AddSwatch(ColorsSubViewModel.PrimaryColor);
            }
        }
    }
}
