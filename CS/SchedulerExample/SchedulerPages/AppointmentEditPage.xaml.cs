using System;
using System.Threading.Tasks;
using DevExpress.XamarinForms.Core.Themes;
using DevExpress.XamarinForms.Scheduler;
using DevExpress.XamarinForms.Scheduler.Internal;
using DevExpress.XamarinForms.Scheduler.Themes;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using XFPage = Xamarin.Forms.Page;

namespace SchedulerExample.AppointmentPages {
    public partial class CustomAppointmentEditPage : ContentPage, IThemeChangingHandler, IDialogService {
        const string SaveIconName = "check";
        const string LightThemePostfix = "_light";
        const string DarkThemePostfix = "_dark";
        const string FileResolution = ".png";

        readonly CustomAppointmentEditViewModel viewModel;
        readonly bool useThemeableToolbarIcons;
        bool inNavigation = false;

        public CustomAppointmentEditPage(DateTime startDate, DateTime endDate, bool allDay, SchedulerDataStorage storage, bool useThemeableToolbarIcons = false) : this() {
            BindingContext = viewModel = new CustomAppointmentEditViewModel(startDate, endDate, allDay, storage);
            viewModel.DialogService = this;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        public CustomAppointmentEditPage(AppointmentItem appointment, SchedulerDataStorage storage, bool useThemeableToolbarIcons = false) : this() {
            BindingContext = viewModel = new CustomAppointmentEditViewModel(appointment, storage);
            viewModel.DialogService = this;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        public CustomAppointmentEditPage(CustomAppointmentEditViewModel viewModel, bool useThemeableToolbarIcons = false) : this() {
            BindingContext = this.viewModel = viewModel;
            viewModel.DialogService = this;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        CustomAppointmentEditPage() {
            InitializeComponent();
            LoadTheme();
            ThemeManager.AddThemeChangedHandler(this);
        }

        public virtual XFPage CreateColorItemSelectPage(ColorItemSelectViewModel viewModel) => new CustomColorItemSelectPage(viewModel);
        public virtual XFPage CreateRecurrenceEditPage(CustomRecurrenceEditViewModel viewModel) => new CustomRecurrenceEditPage(viewModel, useThemeableToolbarIcons);
        public virtual XFPage CreateTimeZoneSelectPage(TimeZoneSelectViewModel viewModel) => new CustomTimeZoneSelectPage(viewModel);
        public virtual XFPage CreateReminderAddPage(CustomReminderAddViewModel viewModel) => new CustomReminderAddPage(viewModel, useThemeableToolbarIcons);

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

        void OnSizeChanged(object sender, EventArgs e) {
            ApplySafeInsets();
        }

        async void OnSaveTapped(object sender, EventArgs e) {
            if (await viewModel.SaveChanges())
                await Navigation.PopAsync();
        }

        void OnLabelTapped(object sender, EventArgs e) {
            if (inNavigation)
                return;
            inNavigation = true;
            ColorItemSelectViewModel selectorViewModel = viewModel.CreateLabelSelectViewModel();
            XFPage editPage = CreateColorItemSelectPage(selectorViewModel);
            Navigation.PushAsync(editPage);
        }

        void OnStatusTapped(object sender, EventArgs e) {
            if (inNavigation)
                return;
            inNavigation = true;
            ColorItemSelectViewModel selectorViewModel = viewModel.CreateStatusSelectViewModel();
            XFPage editPage = CreateColorItemSelectPage(selectorViewModel);
            Navigation.PushAsync(editPage);
        }

        void OnRepeatTapped(object sender, EventArgs e) {
            if (inNavigation)
                return;
            inNavigation = true;
            CustomRecurrenceEditViewModel editRecurrenceViewModel = viewModel.CreateRecurrenceEditViewModel();
            XFPage repeatEditPage = CreateRecurrenceEditPage(editRecurrenceViewModel);
            Navigation.PushAsync(repeatEditPage);
        }

        void OnTimeZoneTapped(object sender, EventArgs e) {
            if (inNavigation)
                return;
            inNavigation = true;
            TimeZoneSelectViewModel selectorViewModel = viewModel.CreateTimeZoneSelectViewModel();
            XFPage timeZoneEditPage = CreateTimeZoneSelectPage(selectorViewModel);
            Navigation.PushAsync(timeZoneEditPage);
        }

        void OnAddReminderTapped(object sender, EventArgs e) {
            if (inNavigation)
                return;
            inNavigation = true;
            CustomReminderAddViewModel addReminderViewModel = viewModel.CreateReminderAddViewModel();
            XFPage addReminderPage = CreateReminderAddPage(addReminderViewModel);
            Navigation.PushAsync(addReminderPage);
        }

        void OnCaptionTapped(object sender, EventArgs e) {
            eventNameEntry.Focus();
        }

        void OnAllDayTapped(object sender, EventArgs e) {
            allDaySwitch.IsToggled = !allDaySwitch.IsToggled;
        }

        async void OnRemoveReminderTapped(object sender, System.EventArgs e) {
            if (!(sender is RippleStackLayout btn))
                return;
            if (!(btn.Parent is View item))
                return;
            if (!(item.BindingContext is ReminderViewModel reminder))
                return;

            await AnimateItemRemoving(item);
            viewModel.Reminders.Remove(reminder);
        }

        void IThemeChangingHandler.OnThemeChanged() {
            LoadTheme();
        }

        void LoadTheme() {
            if (ThemeManager.ThemeName == "Light")
                Xamarin.Forms.Application.Current.Resources.MergedDictionaries.Add(new EditorLightTheme());
            if (ThemeManager.ThemeName == "Dark")
                Xamarin.Forms.Application.Current.Resources.MergedDictionaries.Add(new EditorDarkTheme());
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

        async Task AnimateItemRemoving(View item) {
            reminderContainer.HeightRequest = reminderContainer.Height;
            int index = reminderContainer.Children.IndexOf(item);
            if (index < 0)
                return;
            int viewsToAnimateNumber = reminderContainer.Children.Count - index - 1;
            Task[] animationTasks = new Task[viewsToAnimateNumber + 2];
            animationTasks[0] = item.FadeTo(0, EditPageConstants.AnimationDuration, Easing.CubicIn);
            for (int i = index + 1; i < reminderContainer.Children.Count; ++i) {
                var child = reminderContainer.Children[i];
                animationTasks[i - index] = child.TranslateTo(0, -EditPageConstants.LineHeight - reminderContainer.Spacing, EditPageConstants.AnimationDuration, Easing.CubicInOut);
            }
            animationTasks[viewsToAnimateNumber + 1] = AnimateHeight(reminderContainer, reminderContainer.Height, reminderContainer.Height - EditPageConstants.LineHeight - reminderContainer.Spacing, EditPageConstants.AnimationDuration, Easing.CubicInOut);
            await Task.WhenAll(animationTasks);
        }

        Task AnimateHeight(View container, double oldValue, double newValue, uint length, Easing easing) {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            Animation animation = new Animation(v => container.HeightRequest = v, oldValue, newValue);
            animation.Commit(container, "ContainerResize", length: length, easing: easing, finished: (d, b) => completionSource.SetResult(b));
            return completionSource.Task;
        }

        void ApplySafeInsets() {
            var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            this.RecreateStyleWithHorizontalInsets("FormItemStyle", "FormItemStyleBase", typeof(RippleStackLayout), safeInsets);
            this.RecreateStyleWithHorizontalInsets("FormDateTimeItemStyle", "FormDateTimeItemStyleBase", typeof(StackLayout), safeInsets);
            this.RecreateStyleWithHorizontalInsets("Wrapper", "WrapperBase", typeof(Frame), safeInsets);
            this.root.Margin = new Thickness(0, safeInsets.Top, 0, safeInsets.Bottom);
        }

        Task<bool> IDialogService.DisplayAlertMessage(string title, string message, string accept, string cancel) => DisplayAlert(title, message, accept, cancel);
        Task<string> IDialogService.DisplaySelectItemDialog(string title, string cancel, params string[] options) => DisplayActionSheet(title, cancel, null, options);
    }
}
