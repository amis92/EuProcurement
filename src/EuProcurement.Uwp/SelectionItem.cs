using System;
using Windows.Foundation;

namespace EuProcurement.Uwp
{
    public class SelectionItem : ViewModelBase
    {
        private bool? _isSelected = false;
        private bool _isSelectable = true;

        public SelectionItem(string displayName, string value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public event TypedEventHandler<SelectionItem, EventArgs> IsSelectedChanged;

        public string DisplayName { get; }

        public string Value { get; }

        public bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (Set(ref _isSelected, value))
                {
                    OnIsSelectedChanged();
                }
            }
        }

        public bool IsSelectable
        {
            get { return _isSelectable; }
            set { Set(ref _isSelectable, value); }
        }

        protected virtual void OnIsSelectedChanged()
        {
            RaiseIsSelectedChanged();
        }

        protected void RaiseIsSelectedChanged()
        {
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
