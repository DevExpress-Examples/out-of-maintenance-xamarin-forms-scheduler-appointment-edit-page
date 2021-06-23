using DevExpress.XamarinForms.Scheduler;
using Xamarin.Forms;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomColorItemSelectPage : ContentPage {
        ColorItemSelectViewModel viewModel;

        public CustomColorItemSelectPage(ColorItemSelectViewModel viewModel) {
            InitializeComponent();
            this.viewModel = viewModel;
            this.BindingContext = viewModel;
        }

        void OnLabelTapped(object sender, ItemTappedEventArgs e) {
            if (viewModel == null)
                return;
            if (viewModel.LabelSelectedCommand == null)
                return;
            if (viewModel.LabelSelectedCommand.CanExecute(e.Item)) {
                viewModel.LabelSelectedCommand.Execute(e.Item);
            }
            Navigation?.PopAsync();
        }
    }
}
