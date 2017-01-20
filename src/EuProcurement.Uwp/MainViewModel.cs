using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
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
        }

        private void CreateFilters()
        {
            var cpvSelectableDivisions = ConvertCpvNodesToSelectionItems(CpvCatalogue.Divisions.Values);
            CpvSelectableTreeRoot = new SelectionTreeItem("CPV", null, cpvSelectableDivisions);
            CpvSelectableTreeRoot.AnyDescendantIsSelectedChanged += CpvTreeDescendantIsSelectedChanged;

            var yearFilters = Records.Select(x => x.Year).Distinct().OrderBy(x => x).Select(x => new SelectionTreeItem(x, x)).ToImmutableArray();
            YearSelectionRoot = new SelectionTreeItem("Years", null, yearFilters);
            YearSelectionRoot.AnyDescendantIsSelectedChanged += OnYearIsSelectedChanged;

            var countries = Records.SelectMany(RecordCountries).Distinct().OrderBy(x => x).ToImmutableArray();
            CountrySelector = new CountrySelectorViewModel(countries);
            CountrySelector.SelectedPairs.CollectionChanged += OnSelectedPairsCollectionChanged;
        }

        private void CpvTreeDescendantIsSelectedChanged(SelectionTreeItem sender, DescendantIsSelectedChangedEventArgs e)
        {
            // TODO update data filtering
            UpdateAnalysisResults();
        }

        private void OnSelectedPairsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO update data filtering
            UpdateAnalysisResults();
        }
        
        private void OnYearIsSelectedChanged(SelectionTreeItem sender, DescendantIsSelectedChangedEventArgs e)
        {
            // TODO update data filtering
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
            return from record in Records
                where selectedYears.Contains(record.Year)
                where IsCountryPairSelected(record, countrySelectionLookup)
                where IsCpvSelected(record.Cpv, CpvSelectableTreeRoot)
                select new AnalysisResult(new CountrySelection(record.CountryFrom, record.CountryTo), record.EuroTotal);

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

        private bool IsCountryPairSelected(TedAggregateRecord record,
            ImmutableDictionary<string, ImmutableHashSet<string>> selectedCountryPairs)
        {
            ImmutableHashSet<string> countryToSet;
            if (selectedCountryPairs.TryGetValue(record.CountryFrom, out countryToSet) ||
                selectedCountryPairs.TryGetValue(CountrySelection.AnyCountry, out countryToSet))
            {
                return countryToSet.Contains(record.CountryTo) || countryToSet.Contains(CountrySelection.AnyCountry);
            }
            return false;
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
        public AnalysisResult(CountrySelection countrySelection, decimal netFlowEuroAmount)
        {
            CountrySelection = countrySelection;
            NetFlowEuroAmount = netFlowEuroAmount;
        }

        public CountrySelection CountrySelection { get; }

        public decimal NetFlowEuroAmount { get; }
    }
}

