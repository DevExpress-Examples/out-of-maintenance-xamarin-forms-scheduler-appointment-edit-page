using System;
using Xamarin.Forms;
using DevExpress.XamarinForms.Core.Themes;
using DevExpress.XamarinForms.Scheduler.Internal;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using DevExpress.XamarinForms.Scheduler;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomRecurrenceEditPage : ContentPage {
        const string SaveIconName = "check";
        const string LightThemePostfix = "_light";
        const string DarkThemePostfix = "_dark";
        const string FileResolution = ".png";

        readonly CustomRecurrenceEditViewModel viewModel;
        readonly bool useThemeableToolbarIcons;

        public CustomRecurrenceEditPage(CustomRecurrenceEditViewModel viewModel, bool useThemeableToolbarIcons) {
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

        async void OnSaveTapped(object sender, EventArgs e) {
            if (viewModel.SelectRecurrenceCommand.CanExecute(null)) {
                viewModel.SelectRecurrenceCommand.Execute(null);
            }
            await Navigation.PopAsync();
        }

        void OnFrequencyTapped(object sender, EventArgs e) {
            frequencyPicker.Focus();
        }

        void OnSizeChanged(object sender, EventArgs e) {
            ApplySafeInsets();
        }

        void ApplySafeInsets() {
            var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            this.RecreateStyleWithHorizontalInsets("FormItemStyle", "FormItemStyleBase", typeof(RippleStackLayout), safeInsets);
            this.RecreateStyleWithHorizontalInsets("ContainerStyle", "ContainerStyleBase", typeof(StackLayout), safeInsets);
            this.root.Margin = new Thickness(0, safeInsets.Top, 0, safeInsets.Bottom);
        }

        void UpdateToolbarItems() {
            string actualPostfix = String.Empty;
            if (useThemeableToolbarIcons) {
                switch (ThemeManager.ThemeName) {
                    case "Light":
                        actualPostfix = LightThemePostfix;
                        break;
                    case "Dark":
                        actualPostfix = DarkThemePostfix;
                        break;
                    default:
                        break;
                }
            }
            saveToolbarItem.Icon = new FileImageSource { File = SaveIconName + actualPostfix + FileResolution };
        }
    }

    class RecurrenceSettingsViewSelector : DataTemplateSelector {
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) {
            if (item is DailyRecurrenceViewModel)
                return new DataTemplate(() => new DayRadioGroup());
            if (item is WeeklyRecurrenceViewModel)
                return new DataTemplate(() => new WeekRadioGroup());
            if (item is MonthlyRecurrenceViewModel)
                return new DataTemplate(() => new MonthRadioGroup());
            if (item is YearlyRecurrenceViewModel)
                return new DataTemplate(() => new YearRadioGroup());
            return null;
        }
    }
}