using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private ImmutableArray<SelectionItem> _years;
        private ImmutableArray<SelectionItem> _countriesFrom;
        private ImmutableArray<SelectionItem> _countriesTo;

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

        public ImmutableArray<SelectionItem> Years
        {
            get { return _years; }
            private set { Set(ref _years, value); }
        }

        public ImmutableArray<SelectionItem> CountriesFrom
        {
            get { return _countriesFrom; }
            private set { Set(ref _countriesFrom, value); }
        }

        public ImmutableArray<SelectionItem> CountriesTo
        {
            get { return _countriesTo; }
            set { Set(ref _countriesTo, value); }
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
                await Task.Delay(TimeSpan.FromSeconds(1));
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
            AnalyzeData();
        }

        private void AnalyzeData()
        {
            Years = Records.Select(x => x.Year).Distinct().Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in Years)
            {
                item.IsSelectedChanged += OnYearIsSelectedChanged;
            }
            var countries = Records.SelectMany(RecordCountries).Distinct().ToImmutableArray();
            CountriesFrom = countries.Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in CountriesFrom)
            {
                item.IsSelectedChanged += OnCountryIsSelectedChanged;
            }
            CountriesTo = countries.Select(x => new SelectionItem(x, x)).ToImmutableArray();
            foreach (var item in CountriesTo)
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
