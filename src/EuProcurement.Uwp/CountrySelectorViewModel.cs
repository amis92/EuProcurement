using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EuProcurement.Uwp
{
    public class CountrySelectorViewModel : ViewModelBase
    {
        private string _newCountryFrom;
        private string _newCountryTo;

        public CountrySelectorViewModel(ImmutableArray<string> countries)
        {
            Countries = countries.Insert(0, CountrySelection.AnyCountry);
            CreatePair = new DelegateCommand(CreatePairFromProperties, CanCreatePairFromProperties);
            RemovePair = new DelegateCommand<CountrySelection>(pair => SelectedPairs.Remove(pair));
            SelectedPairs.CollectionChanged += (s, e) => CreatePair.RaiseCanExecuteChanged();
            PropertyChanged += (s, e) => CreatePair.RaiseCanExecuteChanged();
        }

        public ImmutableArray<string> Countries { get; }

        public ObservableCollection<CountrySelection> SelectedPairs { get; } = new ObservableCollection<CountrySelection>();

        public string NewCountryFrom
        {
            get { return _newCountryFrom; }
            set { Set(ref _newCountryFrom, value); }
        }

        public string NewCountryTo
        {
            get { return _newCountryTo; }
            set { Set(ref _newCountryTo, value); }
        }

        public ICommand RemovePair { get; }

        public DelegateCommand CreatePair { get; }

        private void CreatePairFromProperties()
        {
            var pair = new CountrySelection(NewCountryFrom, NewCountryTo);
            SelectedPairs.Add(pair);
        }

        private bool CanCreatePairFromProperties()
        {
            var pair = new CountrySelection(NewCountryFrom, NewCountryTo);
            return NewCountryFrom != null
                && NewCountryTo != null
                && NewCountryFrom != NewCountryTo
                && !SelectedPairs.Contains(pair);
        }
    }
}
