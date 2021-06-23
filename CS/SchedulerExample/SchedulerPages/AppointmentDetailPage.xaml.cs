using System;
using System.Threading.Tasks;
using DevExpress.XamarinForms.Core.Themes;
using DevExpress.XamarinForms.Scheduler;
using DevExpress.XamarinForms.Scheduler.Themes;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using XFApplication = Xamarin.Forms.Application;
using XFPage = Xamarin.Forms.Page;


namespace SchedulerExample.AppointmentPages {
    public partial class CustomAppointmentDetailPage : ContentPage, IThemeChangingHandler, IDialogService {
        const string EditIconName = "edit";
        const string DeleteIconName = "delete";
        const string LightThemePostfix = "_light";
        const string DarkThemePostfix = "_dark";
        const string FileResolution = ".png";


        readonly CustomAppointmentDetailViewModel viewModel;
        readonly bool useThemeableToolbarIcons;
        bool inNavigation = false;

        public CustomAppointmentDetailPage(AppointmentItem appointment, SchedulerDataStorage storage, bool useThemeableToolbarIcons = false) : this() {
            BindingContext = viewModel = new CustomAppointmentDetailViewModel(appointment, storage);
            viewModel.DialogService = this;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        public CustomAppointmentDetailPage(CustomAppointmentDetailViewModel viewModel, bool useThemeableToolbarIcons = false) : this() {
            BindingContext = this.viewModel = viewModel;
            viewModel.DialogService = this;
            this.useThemeableToolbarIcons = useThemeableToolbarIcons;
            UpdateToolbarItems();
        }

        public XFPage CreateAppointmentEditPage(CustomAppointmentEditViewModel viewModel) {
            return new CustomAppointmentEditPage(viewModel, true);
        }

        CustomAppointmentDetailPage() {
            InitializeComponent();
            LoadTheme();
        }

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

        async void OnEditTapped(object sender, EventArgs e) {
            if (Navigation == null || inNavigation)
                return;

            inNavigation = true;
            CustomAppointmentEditViewModel editViewModel = viewModel.CreateAppointmentEditViewModel();
            XFPage editPage = CreateAppointmentEditPage(editViewModel);
            await Navigation.PushAsync(editPage);
            Navigation.RemovePage(this);
        }

        async void OnDeleteTapped(object sender, EventArgs e) {
            if (await viewModel.RemoveAppointment()) {
                await Navigation?.PopAsync();
            }
        }

        void IThemeChangingHandler.OnThemeChanged() {
            LoadTheme();
            UpdateToolbarItems();
        }

        void ApplySafeInsets() {
            var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();
            this.root.Margin = new Thickness(safeInsets.Left, safeInsets.Top, safeInsets.Left, safeInsets.Bottom);
        }

        void LoadTheme() {
            if (ThemeManager.ThemeName == "Light")
                XFApplication.Current.Resources.MergedDictionaries.Add(new EditorLightTheme());
            if (ThemeManager.ThemeName == "Dark")
                XFApplication.Current.Resources.MergedDictionaries.Add(new EditorDarkTheme());
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
            editToolbarItem.Icon = new FileImageSource { File = EditIconName + actualPostfix + FileResolution };
            deleteToolbarItem.Icon = new FileImageSource { File = DeleteIconName + actualPostfix + FileResolution };
        }

        Task<bool> IDialogService.DisplayAlertMessage(string title, string message, string accept, string cancel) => DisplayAlert(title, message, accept, cancel);
        Task<string> IDialogService.DisplaySelectItemDialog(string title, string cancel, params string[] options) => DisplayActionSheet(title, cancel, null, options);
    }
}
