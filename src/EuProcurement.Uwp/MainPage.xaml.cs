using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EuProcurement.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public MainViewModel ViewModel { get; } = new MainViewModel();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                await ViewModel.InitializeAsync();
            }
        }

        public Visibility InvertVisibility(Visibility original)
        {
            return original == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility IsNotNullToVisibility(object obj) => obj != null ? Visibility.Visible : Visibility.Collapsed;

        private bool NullableToBool(bool? value) => value ?? false;

        private bool? BoolToNullable(bool value) => value;
        
        private void CpvDivisionsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CloseClasses();
            if (e.AddedItems?.Count > 0)
            {
                CpvGroupsBlade.OpenIfNotNull();
            }
        }

        private void CpvGroupsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CloseCategories();
            if (e.AddedItems?.Count > 0)
            {
                CpvClassesBlade.OpenIfNotNull();
            }
        }

        private void CpvClassesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CloseSubcategories();
            if (e.AddedItems?.Count > 0)
            {
                CpvCategoriesBlade.OpenIfNotNull();
            }
        }

        private void CpvCategoriesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                CpvSubcategoriesBlade.OpenIfNotNull();
            }
        }

        private void CpvDivisionsBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                CloseGroups();
            }
            else
            {
                YearsBlade.CloseIfNotNull();
                CountriesBlade.CloseIfNotNull();
            }
        }

        private void CpvGroupsBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                CloseClasses();
            }
        }

        private void CpvClassesBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                CloseCategories();
            }
        }

        private void CpvCategoriesBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                CloseSubcategories();
            }
        }

        private void CloseDivisions()
        {
            CpvDivisionsBlade.CloseIfNotNull();
            CloseGroups();
        }

        private void CloseGroups()
        {
            CpvGroupsBlade.CloseIfNotNull();
            CloseClasses();
        }

        private void CloseClasses()
        {
            CpvClassesBlade.CloseIfNotNull();
            CloseCategories();
        }

        private void CloseCategories()
        {
            CpvCategoriesBlade.CloseIfNotNull();
            CloseSubcategories();
        }

        private void CloseSubcategories()
        {
            CpvSubcategoriesBlade.CloseIfNotNull();
        }

        private void YearsBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                return;
            }
            CloseDivisions();
            CountriesBlade.CloseIfNotNull();
        }

        private void CountriesBlade_OnVisibilityChanged(object sender, Visibility e)
        {
            if (e != Visibility.Visible)
            {
                return;
            }
            CloseDivisions();
            YearsBlade.CloseIfNotNull();
        }

        private async void AboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AboutDialog.ShowAsync();
        }
    }

    public static class BladeExtensions
    {
        public static void CloseIfNotNull(this BladeItem blade)
        {
            if (blade != null)
            {
                blade.IsOpen = false;
            }
        }
        public static void OpenIfNotNull(this BladeItem blade)
        {
            if (blade != null)
            {
                blade.IsOpen = true;
            }
        }
    }
}
