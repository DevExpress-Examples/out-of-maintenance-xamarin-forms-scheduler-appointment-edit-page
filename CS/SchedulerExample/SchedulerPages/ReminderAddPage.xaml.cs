using System;
using DevExpress.XamarinForms.Scheduler.Internal;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using XFPage = Xamarin.Forms.Page;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomReminderAddPage : ContentPage {
        readonly CustomReminderAddViewModel viewModel;
        readonly bool useThemeableToolbarIcons;

        bool inNavigation = true;

        public CustomReminderAddPage(CustomReminderAddViewModel viewModel, bool useThemeableToolbarIcons) {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
        }

        public virtual XFPage CreateReminderEditPage(CustomReminderEditViewModel viewModel) => new CustomReminderEditPage(viewModel, useThemeableToolbarIcons);

        protected override void OnAppearing() {
            base.OnAppearing();
            inNavigation = false;
            ApplySafeInsets();
            this.SizeChanged += OnSizeChanged;
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            this.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            ApplySafeInsets();
        }

        void OnReminderTapped(object sender, EventArgs e) {
            if (viewModel == null || inNavigation) return;
            if (!(sender is View view)) return;
            if (viewModel.SelectReminderCommand.CanExecute(view.BindingContext)) {
                viewModel.SelectReminderCommand.Execute(view.BindingContext);
                Navigation?.PopAsync();
            }
        }

        async void OnCustomReminderTapped(object sender, EventArgs e) {
            if (viewModel == null) return;
            inNavigation = true;
            CustomReminderEditViewModel editReminderViewModel = viewModel.CreateEditReminderViewModel(OnReminderConfigured);
            XFPage page = CreateReminderEditPage(editReminderViewModel);
            await Navigation?.PushAsync(page);
        }

        async void OnReminderConfigured() {
            await Navigation.PopAsync();
        }

        void ApplySafeInsets() {
            var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            this.RecreateStyleWithHorizontalInsets("ItemStyle", "ItemStyleBase", typeof(RippleStackLayout), safeInsets);
            this.root.Margin = new Thickness(0, safeInsets.Top, 0, safeInsets.Bottom);
        }
    }
}
