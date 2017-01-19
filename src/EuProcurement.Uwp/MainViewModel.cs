using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private ImmutableArray<SelectionItem> _yearFilters = ImmutableArray<SelectionItem>.Empty;
        private ImmutableArray<SelectionItem> _countryFromFilters = ImmutableArray<SelectionItem>.Empty;
        private ImmutableArray<SelectionItem> _countryToFilters = ImmutableArray<SelectionItem>.Empty;
        private CpvCatalogue _cpvCatalogue;

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

        public ImmutableArray<SelectionItem> YearFilters
        {
            get { return _yearFilters; }
            private set { Set(ref _yearFilters, value); }
        }

        public ImmutableArray<SelectionItem> CountryFromFilters
        {
            get { return _countryFromFilters; }
            private set { Set(ref _countryFromFilters, value); }
        }

        public ImmutableArray<SelectionItem> CountryToFilters
        {
            get { return _countryToFilters; }
            set { Set(ref _countryToFilters, value); }
        }

        public CpvCatalogue CpvCatalogue
        {
            get { return _cpvCatalogue; }
            set { Set(ref _cpvCatalogue, value); }
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
            YearFilters = Records.Select(x => x.Year).Distinct().OrderBy(x => x).Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in YearFilters)
            {
                item.IsSelectedChanged += OnYearIsSelectedChanged;
            }
            var countries = Records.SelectMany(RecordCountries).Distinct().OrderBy(x => x).ToImmutableArray();
            CountryFromFilters = countries.Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in CountryFromFilters)
            {
                item.IsSelectedChanged += OnCountryIsSelectedChanged;
            }
            CountryToFilters = countries.Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in CountryToFilters)
            {
                item.IsSelectedChanged += OnCountryIsSelectedChanged;
            }
        }

        private void OnCountryIsSelectedChanged(SelectionItem sender, EventArgs args)
        {
        }

        private void OnYearIsSelectedChanged(SelectionItem sender, EventArgs args)
        {
        }

        private static IEnumerable<string> RecordCountries(TedAggregateRecord record)
        {
            yield return record.CountryFrom;
            yield return record.CountryTo;
        }
    }
}
