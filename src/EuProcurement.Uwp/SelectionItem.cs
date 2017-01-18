using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace EuProcurement.Uwp
{
    public class SelectionItem : ViewModelBase
    {
        private bool _isSelected;
        private bool _isSelectable = true;

        public SelectionItem(string displayName, string value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public string DisplayName { get; }

        public string Value { get; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (Set(ref _isSelected, value))
                {
                    IsSelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsSelectable
        {
            get { return _isSelectable; }
            set { Set(ref _isSelectable, value); }
        }

        public event TypedEventHandler<SelectionItem, EventArgs> IsSelectedChanged;
    }
}
