﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Layers
{
    public class GuidStructureItem : NotifyableObject
    {
        public Guid FolderGuid { get; init; }

        private string name;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<Guid> LayerGuids { get; set; }

        public ObservableCollection<GuidStructureItem> Subfolders { get; set; }

        private bool isExpanded;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        public GuidStructureItem(string name, IEnumerable<Guid> children, IEnumerable<GuidStructureItem> subfolders)
        {
            Name = name;
            LayerGuids = new ObservableCollection<Guid>(children);
            Subfolders = new ObservableCollection<GuidStructureItem>(subfolders);
            FolderGuid = Guid.NewGuid();
        }

        public GuidStructureItem(string name)
        {
            Name = name;
            LayerGuids = new ObservableCollection<Guid>();
            Subfolders = new ObservableCollection<GuidStructureItem>();
            FolderGuid = Guid.NewGuid();
        }
    }
}