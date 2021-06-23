using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XamarinForms.Scheduler;
using DevExpress.XamarinForms.Scheduler.Internal;

namespace SchedulerExample.AppointmentPages {
    public static class AppointmentDetailFormatProvider {
        public static string StatusFormat => "Status: {0}";
        public static string RecurrenceFormat => "Repeat {0}";

        public static string GetIntervalFormat(bool allDay, bool sameDate, bool thisYear) {
            DateTimeFormatInfo dtfi = CultureInfo.CurrentUICulture.DateTimeFormat;
            if (allDay) {
                string datePattern = thisYear ? dtfi.MonthDayPattern.Replace("MMMM", "MMM") : dtfi.LongDatePattern.Replace("MMMM", "MMM");
                return sameDate ? "{0:dddd, " + datePattern + "}" : "{0:dddd, " + datePattern + "} - {1:dddd, " + datePattern + "}";
            } else {
                string datePattern = thisYear ? dtfi.MonthDayPattern.Replace("MMMM", "MMM") : dtfi.LongDatePattern.Replace("MMMM", "MMM");
                string timePattern = dtfi.ShortTimePattern;
                return sameDate ? "{0:dddd, " + datePattern + " • " + timePattern + "} - {1:" + timePattern + "}" : "{0:dddd, " + datePattern + " " + timePattern + "} - {1:dddd, " + datePattern + " " + timePattern + "}";
            }
        }
    }

    public class CustomAppointmentDetailViewModel : NotifyPropertyChangedBase {
        const string RemoveRecurringAppointmentTitle = "This is a recurring appointment.";
        const string RemoveOccurrenceAction = "Delete this appointment only";
        const string RemoveFutureOccurrencesAction = "Delete this and all future appointments";
        const string RemovePatternAction = "Delete all appointments in the series";
        const string CancelRemoveOccurrence = "Cancel";

        const string RemoveAppointmentTitle = "Delete appointment?";
        const string AcceptAppointmentRemoveAction = "Yes";
        const string CancelAppointmentRemoveAction = "No";


        string timeText;
        object labelColor;

        RecurrenceType? recurrenceType;
        ICollection<ReminderViewModel> reminders;
        string timeZoneName;
        string statusCaption;

        AppointmentItem appointment;
        SchedulerDataStorage storage;

        public string Subject => appointment.Subject;

        public string TimeText {
            get => timeText;
            protected set => SetProperty(ref timeText, value);
        }

        public object LabelColor {
            get => labelColor;
            protected set => SetProperty(ref labelColor, value, onChanged: (oldVal, newVal) => RaisePropertyChanged(nameof(HasLabel), null));
        }

        public RecurrenceType? RecurrenceType {
            get => recurrenceType;
            protected set => SetProperty(ref recurrenceType, value,
                onChanged: (oldVal, newVal) => {
                    RaisePropertyChanged(nameof(FormattedRecurrenceType), null);
                    RaisePropertyChanged(nameof(HasRecurrence), null);
                });
        }

        public ICollection<ReminderViewModel> Reminders {
            get => reminders;
            protected set => SetProperty(ref reminders, value, onChanged: (oldVal, newVal) => RaisePropertyChanged(nameof(HasReminders), null));
        }

        public string TimeZoneName {
            get => timeZoneName;
            protected set => SetProperty(ref timeZoneName, value, onChanged: (oldVal, newVal) => RaisePropertyChanged(nameof(HasTimeZone), null));
        }

        public string StatusCaption {
            get => statusCaption;
            protected set => SetProperty(ref statusCaption, value,
                onChanged: (oldVal, newVal) => {
                    RaisePropertyChanged(nameof(FormattedStatusCaption), null);
                    RaisePropertyChanged(nameof(HasStatus), null);
                });
        }

        public IDialogService DialogService { get; set; }

        public string FormattedRecurrenceType => (recurrenceType != null)
            ? string.Format(AppointmentDetailFormatProvider.RecurrenceFormat, recurrenceType.ToString().ToLower())
            : string.Empty;

        public string FormattedStatusCaption => !string.IsNullOrEmpty(statusCaption)
            ? string.Format(AppointmentDetailFormatProvider.StatusFormat, statusCaption)
            : string.Empty;

        public bool HasLabel => labelColor != null;
        public bool HasRecurrence => recurrenceType != null;
        public bool HasReminders => (reminders != null) ? reminders.Count > 0 : false;
        public bool HasTimeZone => !string.IsNullOrEmpty(timeZoneName);
        public bool HasStatus => !string.IsNullOrEmpty(statusCaption);

        public CustomAppointmentDetailViewModel(AppointmentItem appointment, SchedulerDataStorage storage) {
            this.appointment = appointment ?? throw new ArgumentNullException(nameof(appointment), "The passed parameter must not be null.");
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage), "The passed parameter must not be null.");

            UpdateTimeText();
            UpdateLabelColor();
            UpdateStatusCaption();
            UpdateReminders();
            UpdateRecurrenceType();
            UpdateTimeZoneName();

            SubscribeOnAppointmentChanged();
            SubscribeOnRecurrenceChangedIfSpecified();
        }

        public virtual CustomAppointmentEditViewModel CreateAppointmentEditViewModel() => new CustomAppointmentEditViewModel(appointment, storage);

        public async Task<bool> RemoveAppointment() {
            if (DialogService == null) {
                throw new Exception("The DialogService property value must be injected.");
            }
            if (appointment.IsOccurrence) {
                return await RemoveRecurringAppointment();
            }
            return await RemoveNormalAppointment();
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (sender is AppointmentItem) {
                UpdateViewModelFromAppointment(e.PropertyName);
            } else {
                UpdateViewModelFromRecurrenceInfo(e.PropertyName);
            }
        }

        void OnAppointmentRemindersChanged(object sender, EventArgs e) {
            UpdateReminders();
        }

        async Task<bool> RemoveRecurringAppointment() {
            string action = await DialogService.DisplaySelectItemDialog(RemoveRecurringAppointmentTitle, CancelRemoveOccurrence, RemoveOccurrenceAction, RemoveFutureOccurrencesAction, RemovePatternAction);
            switch (action) {
                case RemoveOccurrenceAction:
                    appointment.Type = AppointmentType.DeletedOccurrence;
                    return true;
                case RemoveFutureOccurrencesAction:
                    if (appointment.RecurrenceIndex > 0) {
                        storage.StopPatternBeforeOccurrence(appointment);
                    } else {
                        RemovePattern();
                    }
                    return true;
                case RemovePatternAction:
                    RemovePattern();
                    return true;
                default:
                    return false;
            }
        }

        async Task<bool> RemoveNormalAppointment() {
            bool isConfirmed = await DialogService.DisplayAlertMessage(RemoveAppointmentTitle, null, AcceptAppointmentRemoveAction, CancelAppointmentRemoveAction);
            if (isConfirmed) {
                storage.RemoveAppointment(appointment);
            }
            return isConfirmed;
        }

        void RemovePattern() {
            AppointmentItem pattern = storage.GetPattern(appointment);
            if (pattern != null) {
                storage.RemoveAppointment(pattern);
            }
        }

        void UpdateViewModelFromAppointment(string changedPropertyName) {
            switch (changedPropertyName) {
                case nameof(AppointmentItem.Subject):
                    RaisePropertyChanged(nameof(Subject), null);
                    break;
                case nameof(AppointmentItem.Start):
                case nameof(AppointmentItem.End):
                case nameof(AppointmentItem.AllDay):
                    UpdateTimeText();
                    break;
                case nameof(AppointmentItem.LabelId):
                    UpdateLabelColor();
                    break;
                case nameof(AppointmentItem.StatusId):
                    UpdateStatusCaption();
                    break;
                case nameof(AppointmentItem.TimeZoneId):
                    UpdateTimeZoneName();
                    break;
                case nameof(AppointmentItem.RecurrenceInfo):
                    SubscribeOnRecurrenceChangedIfSpecified();
                    UpdateRecurrenceType();
                    break;
            }
        }

        void UpdateViewModelFromRecurrenceInfo(string changedPropertyName) {
            switch (changedPropertyName) {
                case nameof(IRecurrenceInfo.Type):
                    UpdateRecurrenceType();
                    break;
            }
        }

        void UpdateTimeText() {
            DateTime aptStart;
            DateTime aptEnd;
            if (appointment.AllDay) {
                aptStart = appointment.Start;
                aptEnd = appointment.End;
                if (appointment.End.TimeOfDay == TimeSpan.Zero)
                    aptEnd = aptEnd.AddDays(-1);
            } else if (String.IsNullOrEmpty(appointment.TimeZoneId)) {
                aptStart = appointment.Start;
                aptEnd = appointment.End;
            } else {
                aptStart = this.storage.TimeZoneEngine.FromOperationTime(appointment.Start, appointment.TimeZoneId);
                aptEnd = this.storage.TimeZoneEngine.FromOperationTime(appointment.End, appointment.TimeZoneId);
            }

            TimeText = string.Format(
                AppointmentDetailFormatProvider.GetIntervalFormat(
                    allDay: appointment.AllDay,
                    sameDate: appointment.Start.Date == appointment.End.Date || (appointment.End.Date == appointment.Start.Date.AddDays(1) && appointment.End.TimeOfDay == TimeSpan.Zero),
                    thisYear: appointment.End.Year == DateTime.Now.Year && appointment.Start.Year == DateTime.Now.Year
                ),
                aptStart, aptEnd);
        }

        void UpdateLabelColor() {
            LabelColor = storage.GetLabelItemById(appointment.LabelId)?.Color;
        }

        void UpdateStatusCaption() {
            StatusCaption = storage.GetStatusItemById(appointment.StatusId)?.Caption;
        }

        void UpdateReminders() {
            Reminders = appointment.Reminders.Select(r => new ReminderViewModel(r)).ToList();
            if (Reminders.Count > 0) {
                Reminders.First().IsFirst = true;
            }
        }

        void UpdateRecurrenceType() {
            RecurrenceType = appointment.RecurrenceInfo?.Type;
        }

        void UpdateTimeZoneName() {
            if (appointment.TimeZoneId == null || !TimeZoneEngine.TimeZones.TryGetValue(appointment.TimeZoneId, out TimeZoneInfoWrapper timeZone)) {
                TimeZoneName = null;
                return;
            }
            TimeZoneName = timeZone.DisplayName;
        }

        void SubscribeOnAppointmentChanged() {
            appointment.PropertyChanged += OnPropertyChanged;
            appointment.Reminders.CollectionChanged += OnAppointmentRemindersChanged;
        }

        void SubscribeOnRecurrenceChangedIfSpecified() {
            if (appointment.RecurrenceInfo != null) {
                appointment.RecurrenceInfo.PropertyChanged += OnPropertyChanged;
            }

        }
    }
}
