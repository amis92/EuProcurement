using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Storage;
using EuProcurement.Records;

namespace EuProcurement.Uwp
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isLoadingData = true;
        private ImmutableArray<TedAggregateRecord> _records = ImmutableArray<TedAggregateRecord>.Empty;
        private Exception _loadingException;
        private SelectionTreeItem _yearSelectionRoot;
        private CpvCatalogue _cpvCatalogue;
        private SelectionTreeItem _cpvSelectableTreeRoot;
        private CountrySelectorViewModel _countrySelector;
        private ImmutableArray<AnalysisResult> _analysisResults = ImmutableArray<AnalysisResult>.Empty;

        public bool IsLoadingData
        {
            get { return _isLoadingData; }
            private set { Set(ref _isLoadingData, value); }
        }

        public ImmutableArray<TedAggregateRecord> Records
        {
            get { return _records; }
            private set { Set(ref _records, value); }
        }

        public Exception LoadingException
        {
            get { return _loadingException; }
            private set { Set(ref _loadingException, value); }
        }

        public SelectionTreeItem YearSelectionRoot
        {
            get { return _yearSelectionRoot; }
            private set { Set(ref _yearSelectionRoot, value); }
        }
        
        public CpvCatalogue CpvCatalogue
        {
            get { return _cpvCatalogue; }
            private set { Set(ref _cpvCatalogue, value); }
        }

        public SelectionTreeItem CpvSelectableTreeRoot
        {
            get { return _cpvSelectableTreeRoot; }
            private set { Set(ref _cpvSelectableTreeRoot, value); }
        }

        public CountrySelectorViewModel CountrySelector
        {
            get { return _countrySelector; }
            private set { Set(ref _countrySelector, value); }
        }

        public ImmutableArray<AnalysisResult> AnalysisResults
        {
            get { return _analysisResults; }
            private set { Set(ref _analysisResults, value); }
        }

        private bool Initialized { get; set; }

        public async Task InitializeAsync()
        {
            if (Initialized)
            {
                return;
            }
            Initialized = true;
            try
            {
                await LoadDataAsync();
            }
            catch (Exception e)
            {
                LoadingException = e;
            }
            IsLoadingData = false;
        }

        public void UpdateAnalysisResults()
        {
            var results = GetAnalysisResults();
            AnalysisResults = results.ToImmutableArray();
        }

        private async Task LoadDataAsync()
        {
            var installFolder = Package.Current.InstalledLocation;
            var tedDataFolder = await installFolder.GetFolderAsync(DatafileConstants.TedDataFolderName);
            var aggregatedCsvFile = await tedDataFolder.GetFileAsync(DatafileConstants.TedAggregatedCsvFilename);
            using (var fileStream = await aggregatedCsvFile.OpenStreamForReadAsync())
            {
                var csvReader = new CsvHelper.CsvReader(new StreamReader(fileStream));
                Records = csvReader.GetRecords<TedAggregateRecord>().ToImmutableArray();
            }
            var cpvXmlFile = await tedDataFolder.GetFileAsync(DatafileConstants.CpvCatalogueFilename);
            using (var fileStream = await cpvXmlFile.OpenStreamForReadAsync())
            {
                var xmlSerializer = new XmlSerializer(typeof(CpvCatalogueXml));
                var cpvCatalogueXml = (CpvCatalogueXml)xmlSerializer.Deserialize(fileStream);
                CpvCatalogue = CpvCatalogueFactory.CreateCatalogueFromXml(cpvCatalogueXml);
            }
            CreateFilters();
            UpdateAnalysisResults();
        }

        private void CreateFilters()
        {
            var cpvSelectableDivisions = ConvertCpvNodesToSelectionItems(CpvCatalogue.Divisions.Values);
            CpvSelectableTreeRoot = new SelectionTreeItem("CPV", null, cpvSelectableDivisions)
            {
                IsSelected = true
            };
            CpvSelectableTreeRoot.IsSelectedChanged += (s, e) => UpdateAnalysisResults();
            CpvSelectableTreeRoot.AnyDescendantIsSelectedChanged += CpvTreeDescendantIsSelectedChanged;

            var yearFilters =
                Records
                .Select(x => x.Year)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectionTreeItem(x, x))
                .ToImmutableArray();
            YearSelectionRoot = new SelectionTreeItem("Years", null, yearFilters)
            {
                IsSelected = true
            };
            YearSelectionRoot.IsSelectedChanged += (s, e) => UpdateAnalysisResults();
            YearSelectionRoot.AnyDescendantIsSelectedChanged += OnYearIsSelectedChanged;

            var countries =
                Records
                .SelectMany(RecordCountries)
                .Distinct()
                .OrderBy(x => x)
                .ToImmutableArray();
            CountrySelector = new CountrySelectorViewModel(countries)
            {
                SelectedPairs =
                {
                    new CountrySelection("PL", CountrySelection.AnyCountry)
                }
            };
            CountrySelector.SelectedPairs.CollectionChanged += OnSelectedPairsCollectionChanged;
        }

        private void CpvTreeDescendantIsSelectedChanged(SelectionTreeItem sender, DescendantIsSelectedChangedEventArgs e)
        {
            UpdateAnalysisResults();
        }

        private void OnSelectedPairsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAnalysisResults();
        }
        
        private void OnYearIsSelectedChanged(SelectionTreeItem sender, DescendantIsSelectedChangedEventArgs e)
        {
            UpdateAnalysisResults();
        }

        private IEnumerable<AnalysisResult> GetAnalysisResults()
        {
            var selectedYears = (from yearSelection in YearSelectionRoot.Children
                                where yearSelection.IsSelected == true
                                select yearSelection.Value)
                                .ToImmutableHashSet();
            var selectedCountryPairs = CountrySelector.SelectedPairs.ToImmutableHashSet();
            var countrySelectionLookup = selectedCountryPairs
                .GroupBy(pair => pair.CountryFrom)
                .ToImmutableDictionary(group => group.Key, group => group.Select(pair => pair.CountryTo).ToImmutableHashSet());
            var filteredRecords = 
                (from record in Records
                where selectedYears.Contains(record.Year)
                where IsCpvSelected(record.Cpv, CpvSelectableTreeRoot)
                select record)
                .ToImmutableArray();
            var countryPairRecords =
                selectedCountryPairs.ToImmutableDictionary(x => x, x => new HashSet<TedAggregateRecord>());
            foreach (var record in filteredRecords)
            {
                foreach (var countrySelection in GetFittingCountrySelections(record, selectedCountryPairs))
                {
                    countryPairRecords[countrySelection].Add(record);
                }
            }
            var results =
                from x in countryPairRecords
                let amountPaid = x.Value.Aggregate(new Tuple<decimal, decimal>(0M,0M), (sum, record) =>
                {
                    const string any = CountrySelection.AnyCountry;
                    var keyFrom = x.Key.CountryFrom;
                    var keyTo = x.Key.CountryTo;
                    var isSwitched = (keyFrom != any && keyFrom != record.CountryFrom) || (keyTo != any && keyTo != record.CountryTo);
                    var amountPaid = (isSwitched ? -record.EuroTotal : record.EuroTotal);
                    var sumPaid = amountPaid > 0 ? sum.Item1 + amountPaid : sum.Item1;
                    var sumReceived = amountPaid < 0 ? sum.Item2 - amountPaid : sum.Item2;
                    return new Tuple<decimal, decimal>(sumPaid, sumReceived);
                })
                select new AnalysisResult(x.Key, amountPaid.Item1, amountPaid.Item2);
            return results;
        }

        private bool IsCpvSelected(CpvCode recordCpv, SelectionTreeItem cpvRoot)
        {
            if (recordCpv.Group == CpvCode.EmptyGroup)
            {
                return cpvRoot.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Division)
                    ?.IsSelected == true;
            }
            if (recordCpv.Class == CpvCode.EmptyClass)
            {
                return cpvRoot.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Division)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Group)
                           ?.IsSelected == true;
            }
            if (recordCpv.Category == CpvCode.EmptyCategory)
            {
                return cpvRoot.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Division)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Group)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Class)
                           ?.IsSelected == true;
            }
            if (recordCpv.Subcategory == CpvCode.EmptySubcategory)
            {
                return cpvRoot.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Division)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Group)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Class)
                           ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Category)
                           ?.IsSelected == true;
            }
            return cpvRoot.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Division)
                       ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Group)
                       ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Class)
                       ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Category)
                       ?.GetMaybeSelectedTreeItemForRecordCpv(recordCpv, code => code.Subcategory)
                       ?.IsSelected == true;
        }

        private IEnumerable<CountrySelection> GetFittingCountrySelections(TedAggregateRecord record,
            ImmutableHashSet<CountrySelection> selectedCountryPairs)
        {
            var exactRecordPair = new CountrySelection(record.CountryFrom, record.CountryTo);
            if (selectedCountryPairs.Contains(exactRecordPair))
            {
                yield return exactRecordPair;
            }
            var invertedRecordPair = new CountrySelection(record.CountryTo, record.CountryFrom);
            if (selectedCountryPairs.Contains(invertedRecordPair))
            {
                yield return invertedRecordPair;
            }
            var fromAnyPair = new CountrySelection(CountrySelection.AnyCountry, record.CountryTo);
            if (selectedCountryPairs.Contains(fromAnyPair))
            {
                yield return fromAnyPair;
            }
            var toAnyPair = new CountrySelection(record.CountryFrom, CountrySelection.AnyCountry);
            if (selectedCountryPairs.Contains(toAnyPair))
            {
                yield return toAnyPair;
            }
        }

        private static IEnumerable<string> RecordCountries(TedAggregateRecord record)
        {
            yield return record.CountryFrom;
            yield return record.CountryTo;
        }

        private ImmutableArray<SelectionTreeItem> ConvertCpvNodesToSelectionItems(IEnumerable<CpvTreeNode> nodes)
        {
            return (from node in nodes
                    orderby node.Code
                    select ConvertCpvNodeToSelectionItem(node))
                    .ToImmutableArray();
        }

        private SelectionTreeItem ConvertCpvNodeToSelectionItem(CpvTreeNode node)
        {
            var displayName = node.TextTranslations["EN"];
            var children = ConvertCpvNodesToSelectionItems(node.Children.Values);
            return new SelectionTreeItem(displayName, node.Code, children);
        }
    }

    public static class CpvSelectionTreeExtensions
    {
        public static SelectionTreeItem GetMaybeSelectedTreeItemForRecordCpv(this SelectionTreeItem node, CpvCode recordCpv, Func<CpvCode, string> keySelector)
        {
            return node.Children.FirstOrDefault(
                                item => item.IsSelected != false && keySelector(new CpvCode(item.Value)) == keySelector(recordCpv));
        }

        public static SelectionTreeItem GetMaybeSelectedTreeItemForRecordCpv(this SelectionTreeItem node, CpvCode recordCpv, Func<CpvCode, char> keySelector)
        {
            return node.Children.FirstOrDefault(
                                item => item.IsSelected != false && keySelector(new CpvCode(item.Value)) == keySelector(recordCpv));
        }

    }

    public class AnalysisResult
    {
        public AnalysisResult(CountrySelection countrySelection, decimal euroPaid, decimal euroReceived)
        {
            CountrySelection = countrySelection;
            EuroPaid = euroPaid;
            EuroReceived = euroReceived;
            TotalEuroAsPaid = EuroPaid - EuroReceived;
            const string euroSymbol = "€"; // Euro symbol
            EuroPaidString = $"{EuroPaid:N} {euroSymbol}";
            EuroReceivedString = $"{EuroReceived:N} {euroSymbol}";
            TotalEuroAsPaidString = $"{TotalEuroAsPaid:N} {euroSymbol}";
        }

        public CountrySelection CountrySelection { get; }

        public decimal TotalEuroAsPaid { get; }

        public string TotalEuroAsPaidString { get; }

        public decimal EuroPaid { get; }

        public string EuroPaidString { get; }

        public decimal EuroReceived { get; }

        public string EuroReceivedString { get; }
    }
}

