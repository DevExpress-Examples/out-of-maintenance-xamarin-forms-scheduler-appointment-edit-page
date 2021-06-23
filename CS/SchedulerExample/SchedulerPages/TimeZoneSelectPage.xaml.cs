using DevExpress.XamarinForms.Scheduler;
using Xamarin.Forms;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomTimeZoneSelectPage : ContentPage {
        readonly TimeZoneSelectViewModel viewModel;

        public CustomTimeZoneSelectPage(TimeZoneSelectViewModel viewModel) {
            InitializeComponent();
            this.viewModel = viewModel;
            this.BindingContext = viewModel;
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            this.searchBar.Focus();

        }

        void TimeZoneTapped(object sender, ItemTappedEventArgs e) {
            if (viewModel == null)
                return;
            if (viewModel.TimeZoneSelectedCommand == null)
                return;
            if (viewModel.TimeZoneSelectedCommand.CanExecute(e.Item)) {
                viewModel.TimeZoneSelectedCommand.Execute(e.Item);
            }
            Navigation.PopAsync();
        }

        void OnSearchBarTextChanged(object sender, TextChangedEventArgs e) {
            if (viewModel == null)
                return;
            if (viewModel.FilterTimeZonesCommand == null)
                return;
            if (viewModel.FilterTimeZonesCommand.CanExecute(e.NewTextValue)) {
                viewModel.FilterTimeZonesCommand.Execute(e.NewTextValue);
            }
        }
    }
}
