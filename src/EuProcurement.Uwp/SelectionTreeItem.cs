using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.Foundation;

namespace EuProcurement.Uwp
{
    public class DescendantIsSelectedChangedEventArgs
    {
        public DescendantIsSelectedChangedEventArgs(SelectionTreeItem source)
        {
            Source = source;
        }

        public SelectionTreeItem Source { get; }
    }

    public class SelectionTreeItem : SelectionItem
    {
        public SelectionTreeItem(string displayName, string value, ImmutableArray<SelectionTreeItem> children) : this(displayName, value)
        {
            Children = children;

            foreach (var child in children)
            {
                child.IsSelectedChanged += OnChildIsSelectedChanged;
                child.AnyDescendantIsSelectedChanged += OnChildAnyDescendantIsSelectedChanged;
            }
        }

        public SelectionTreeItem(string displayName, string value) : base(displayName, value)
        {
        }

        public event TypedEventHandler<SelectionTreeItem, DescendantIsSelectedChangedEventArgs> AnyDescendantIsSelectedChanged;

        public ImmutableArray<SelectionTreeItem> Children { get; } = ImmutableArray<SelectionTreeItem>.Empty;

        private bool IsSelectedChangedEventInvoked { get; set; }

        protected override void OnIsSelectedChanged()
        {
            if (IsSelectedChangedEventInvoked)
            {
                return;
            }
            // aaaa kurwa życie jest ciężkie
            IsSelectedChangedEventInvoked = true;
            try
            {
                // try harder
                UpdateChildrenSelectionState();
                base.OnIsSelectedChanged();
            }
            finally
            {
                IsSelectedChangedEventInvoked = false;
            }
        }

        private void UpdateChildrenSelectionState()
        {
            if (IsSelected == null)
            {
                return;
            }
            var state = IsSelected;
            foreach (var child in Children)
            {
                child.IsSelected = state;
            }
        }

        private void OnChildIsSelectedChanged(SelectionItem sender, EventArgs args)
        {
            if (IsSelectedChangedEventInvoked)
            {
                return;
            }
            RaiseAnyDescendantIsSelectedChanged(new DescendantIsSelectedChangedEventArgs((SelectionTreeItem) sender));
            switch (sender.IsSelected)
            {
                case true:
                    IsSelected = Children.All(item => item.IsSelected == true) ? (bool?)true : null;
                    break;
                case false:
                    IsSelected = Children.All(item => item.IsSelected == false) ? (bool?)false : null;
                    break;
                case null:
                    IsSelected = null;
                    break;
            }
        }

        private void OnChildAnyDescendantIsSelectedChanged(SelectionTreeItem sender, DescendantIsSelectedChangedEventArgs args)
        {
            RaiseAnyDescendantIsSelectedChanged(args);
        }

        protected virtual void RaiseAnyDescendantIsSelectedChanged(DescendantIsSelectedChangedEventArgs e)
        {
            AnyDescendantIsSelectedChanged?.Invoke(this, e);
        }
    }
}
