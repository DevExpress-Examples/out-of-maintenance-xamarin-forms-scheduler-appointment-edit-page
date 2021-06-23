using System;
using DevExpress.XamarinForms.Core.Themes;
using DevExpress.XamarinForms.Scheduler.Internal;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomReminderEditPage : ContentPage {
        const string SaveIconName = "check";
        const string LightThemePostfix = "_light";
        const string DarkThemePostfix = "_dark";
        const string FileResolution = ".png";

        readonly CustomReminderEditViewModel viewModel;
        readonly bool useThemeableToolbarIcons;

        public CustomReminderEditPage(CustomReminderEditViewModel viewModel, bool useThemeableToolbarIcons) {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;

            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            ApplySafeInsets();
            this.SizeChanged += OnSizeChanged;
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            this.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(object sender, System.EventArgs e) {
            ApplySafeInsets();
        }

        void OnBuilderTapped(object sender, System.EventArgs e) {
            if (!(sender is View view)) return;
            if (!(view.BindingContext is TimeSpanBuilder itemViewModel)) return;
            viewModel.SelectedBuilder = itemViewModel;
        }

        async void Handle_Clicked(object sender, System.EventArgs e) {
            await Navigation.PopAsync();
        }

        void ApplySafeInsets() {
            var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            this.RecreateStyleWithHorizontalInsets("TimeUnitItemStyle", "TimeUnitItemStyleBase", typeof(RippleStackLayout), safeInsets);
            this.RecreateStyleWithHorizontalInsets("UnitNumberWrapperStyle", "UnitNumberWrapperStyleBase", typeof(Frame), safeInsets);
            this.root.Margin = new Thickness(0, safeInsets.Top, 0, safeInsets.Bottom);
        }

        void UpdateToolbarItems() {
            string actualPostfix = String.Empty;
            if (useThemeableToolbarIcons) {
                switch (ThemeManager.ThemeName) {
                    case "Light": actualPostfix = LightThemePostfix; break;
                    case "Dark": actualPostfix = DarkThemePostfix; break;
                    default: break;
                }
            }
            saveToolbarItem.Icon = new FileImageSource { File = SaveIconName + actualPostfix + FileResolution };
        }
    }
}
