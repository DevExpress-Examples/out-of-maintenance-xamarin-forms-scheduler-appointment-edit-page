using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.XamarinForms.Scheduler;
using DevExpress.XamarinForms.Scheduler.Internal;

namespace SchedulerExample.AppointmentPages {
    public class CustomAppointmentEditViewModel : NotifyPropertyChangedBase {
        const string SelectAppointmentTypeTitle = "This is a recurring appointment.";
        const string EditOccurrenceAction = "Change this appointment only";
        const string EditFutureOccurrencesAction = "Change this and all future appointments";
        const string EditPatternAction = "Change all appointments in the series";
        const string CancelAction = "Cancel";
        const string NewAppointmentTitle = "New Appointment";
        const string EditAppointmentTitle = "Edit Appointment";

        readonly SchedulerDataStorage storage;
        readonly RecurrenceViewModelBase neverRecurrence;
        readonly IDictionary<RecurrenceType, RecurrenceViewModelBase> typeToViewModelMappings;
        AppointmentItem appointment;
        DateTime start, end;
        string subject;
        bool allDay;
        ColorItemViewModel label;
        ColorItemViewModel status;
        RecurrenceViewModelBase recurrence;
        TimeZoneViewModel timeZone;
        bool allowTimeZone;
        bool allowReminders;
        bool allowRecurrence;
        bool hasReminders = false;
        bool isRecurrenceChanged = false;
        bool isDateChanged = false;

        public CustomAppointmentEditViewModel(AppointmentItem appointment, SchedulerDataStorage storage)
            : this(storage) {
            this.appointment = appointment;
            Title = EditAppointmentTitle;
            UpdateTimeZonesOnItemChange();
            AssignAppointmentData();
        }

        public CustomAppointmentEditViewModel(DateTime startDate, DateTime endDate, bool allDay, SchedulerDataStorage storage)
            : this(storage) {
            Title = NewAppointmentTitle;
            UpdateTimeZonesOnItemChange();
            AssignAppointmentData(startDate, endDate, allDay);
        }

        CustomAppointmentEditViewModel(SchedulerDataStorage storage) {
            this.storage = storage;
            this.allowTimeZone = CheckAllowTimeZones(storage.DataSource);
            this.allowReminders = CheckAllowReminders(storage.DataSource);
            this.allowRecurrence = CheckAllowRecurrence(storage.DataSource);

            TimeZones = GetTimeZones().Select(tz => new TimeZoneViewModel(tz)).ToList();
            Labels = storage.LabelItems.Select(l => new ColorItemViewModel(l.Id, l.Color, l.Caption)).ToList();
            Statuses = storage.StatusItems.Select(s => new ColorItemViewModel(s.Id, s.Color, s.Caption)).ToList();
            DefaultReminders = new List<ReminderViewModel> {
                new ReminderViewModel(TimeSpan.FromMinutes(0)),
                new ReminderViewModel(TimeSpan.FromMinutes(5)),
                new ReminderViewModel(TimeSpan.FromMinutes(10)),
                new ReminderViewModel(TimeSpan.FromMinutes(15)),
                new ReminderViewModel(TimeSpan.FromMinutes(30))
            };
            Reminders.CollectionChanged += OnRemindersCollectionChanged;

            neverRecurrence = new NeverRecurrenceViewModel();
            DailyRecurrenceViewModel dailyRecurrence = new DailyRecurrenceViewModel();
            WeeklyRecurrenceViewModel weeklyRecurrence = new WeeklyRecurrenceViewModel();
            MonthlyRecurrenceViewModel monthlyRecurrence = new MonthlyRecurrenceViewModel();
            YearlyRecurrenceViewModel yearlyRecurrence = new YearlyRecurrenceViewModel();

            Recurrences = new List<RecurrenceViewModelBase>() {
                neverRecurrence, dailyRecurrence, weeklyRecurrence, monthlyRecurrence, yearlyRecurrence
            };
            typeToViewModelMappings = new Dictionary<RecurrenceType, RecurrenceViewModelBase> {
                { RecurrenceType.Daily, dailyRecurrence },
                { RecurrenceType.Weekly, weeklyRecurrence },
                { RecurrenceType.Monthly, monthlyRecurrence },
                { RecurrenceType.Yearly, yearlyRecurrence }
            };
        }

        public DateTime Start {
            get => start;
            set => SetProperty(ref start, value, onChanged: (oldVal, newVal) => {
                RaisePropertyChanged(nameof(StartDate), null);
                RaisePropertyChanged(nameof(StartTime), null);
            });
        }

        public DateTime End {
            get => end;
            set => SetProperty(ref end, value, onChanged: (oldVal, newVal) => {
                RaisePropertyChanged(nameof(EndDate), null);
                RaisePropertyChanged(nameof(EndTime), null);
            });
        }

        public string Subject {
            get => subject;
            set => SetProperty(ref subject, value);
        }

        public bool AllDay {
            get => allDay;
            set => SetProperty(ref allDay, value, onChanged: (oldVal, newVal) => {
                if (EndTime == TimeSpan.Zero) {
                    if (newVal)
                        this.end = this.end.AddDays(-1);
                    else
                        this.end = this.end.AddDays(1);
                    RaisePropertyChanged(nameof(EndDate), null);
                }
                RaisePropertyChanged(nameof(ActualAllowTimeZone), null);
            });
        }

        public ColorItemViewModel Label {
            get => label;
            set => SetProperty(ref label, value, onChanged: (oldValue, newValue) => {
                if (oldValue != null)
                    oldValue.IsSelected = false;
                if (newValue != null)
                    newValue.IsSelected = true;
            });

        }
        public ColorItemViewModel Status {
            get => status;
            set => SetProperty(ref status, value, onChanged: (oldValue, newValue) => {
                if (oldValue != null)
                    oldValue.IsSelected = false;
                if (newValue != null)
                    newValue.IsSelected = true;
            });
        }

        public TimeZoneViewModel TimeZone {
            get => timeZone;
            set => SetProperty(ref timeZone, value, onChanged: (oldValue, newValue) => {
                if (oldValue != null)
                    oldValue.IsSelected = false;
                if (newValue != null)
                    newValue.IsSelected = true;
            });
        }

        public ObservableCollection<ReminderViewModel> Reminders { get; } = new ObservableCollection<ReminderViewModel>();
        public bool HasReminders {
            get => hasReminders;
            protected set => SetProperty(ref hasReminders, value);
        }

        public RecurrenceViewModelBase Recurrence {
            get => recurrence;
            set => SetProperty(ref recurrence, value, onChanged: (oldVal, newVal) => {
                IsRecurrenceChanged = true;
            });
        }
        public RecurrenceEndingViewModel RecurrenceEndingModel { get; } = new RecurrenceEndingViewModel();
        public bool IsRecurrenceChanged {
            get => isRecurrenceChanged;
            set => SetProperty(ref isRecurrenceChanged, value);
        }

        public bool AllowRecurrence { get { return this.allowRecurrence; } }
        public bool AllowReminders { get { return this.allowReminders; } }
        public bool ActualAllowTimeZone { get { return this.allowTimeZone && !AllDay; } }

        public DateTime StartDate {
            get => start.Date;
            set {
                DateTime newStart = value.Date + StartTime;
                if (start == newStart)
                    return;
                TimeSpan duration = Duration;
                Start = newStart;
                ValidateEnd(duration);
                IsDateChanged = true;
            }
        }

        public TimeSpan StartTime {
            get => start.TimeOfDay;
            set {
                DateTime newStart = Start.Date + value;
                if (start == newStart)
                    return;
                TimeSpan duration = Duration;
                Start = newStart;
                ValidateEnd(duration);
            }
        }

        public DateTime EndDate {
            get => end.Date;
            set {
                DateTime newEnd = value.Date + EndTime;
                if (end == newEnd)
                    return;
                TimeSpan duration = Duration;
                End = newEnd;
                ValidateStart(duration);
                IsDateChanged = true;
            }
        }

        public TimeSpan EndTime {
            get => end.TimeOfDay;
            set {
                DateTime newEnd = End.Date + value;
                if (end == newEnd)
                    return;
                TimeSpan duration = Duration;
                End = newEnd;
                ValidateStart(duration);
            }
        }

        public bool IsDateChanged {
            get => isDateChanged;
            private set => SetProperty(ref isDateChanged, value);
        }

        public string Title { get; }
        public IDialogService DialogService { get; set; }
        protected IList<ColorItemViewModel> Labels { get; }
        protected IList<ColorItemViewModel> Statuses { get; }
        protected IList<TimeZoneViewModel> TimeZones { get; }
        protected IList<ReminderViewModel> DefaultReminders { get; }
        protected IList<RecurrenceViewModelBase> Recurrences { get; }
        TimeSpan Duration { get => End - Start; }

        public virtual async Task<bool> SaveChanges() {
            if (appointment == null) {
                CreateNewAppointment();
                return true;
            }
            if (appointment.Type != AppointmentType.Occurrence) {
                PopulateActualAppointment();
                return true;
            }
            if (IsRecurrenceChanged) {
                if (IsDateChanged) {
                    CreateNewPatternByOccurrence();
                    storage.RemoveAppointment(storage.GetPattern(appointment));
                } else {
                    PopulateActualAppointmentPattern();
                }
                return true;
            }
            if (DialogService == null) {
                throw new Exception("The DialogService property value must be injected.");
            }
            string action = await DialogService.DisplaySelectItemDialog(SelectAppointmentTypeTitle, CancelAction, EditOccurrenceAction, EditFutureOccurrencesAction, EditPatternAction);
            switch (action) {
                case EditOccurrenceAction:
                PopulateActualAppointment();
                    return true;
                case EditFutureOccurrencesAction:
                CreateNewPatternByOccurrence();
                    return true;
                case EditPatternAction:
                PopulateActualAppointmentPattern();
                    return true;
                case CancelAction:
                return false;
            }
            return false;
        }

        public virtual TimeZoneSelectViewModel CreateTimeZoneSelectViewModel() => new TimeZoneSelectViewModel(TimeZones, OnTimeZoneSelected);
        public virtual ColorItemSelectViewModel CreateLabelSelectViewModel() => new ColorItemSelectViewModel("Label", Labels, OnLabelSelected);
        public virtual ColorItemSelectViewModel CreateStatusSelectViewModel() => new ColorItemSelectViewModel("Status", Statuses, OnStatusSelected);
        public virtual CustomReminderAddViewModel CreateReminderAddViewModel() => new CustomReminderAddViewModel(DefaultReminders.Where(r => !Reminders.Contains(r)), OnReminderSelected);
        public virtual CustomRecurrenceEditViewModel CreateRecurrenceEditViewModel() => new CustomRecurrenceEditViewModel(Recurrences, RecurrenceEndingModel, Recurrence, OnRecurrenceTypeChanged);

        protected virtual void AssignAppointmentData() {
            TimeZoneInfo aptTimeZone = String.IsNullOrEmpty(appointment.TimeZoneId) ? storage.TimeZoneEngine.OperationTimeZone : TimeZoneEngine.FindSystemTimeZoneById(appointment.TimeZoneId);
            Start = appointment.AllDay ? appointment.Start : FromClientTime(appointment.Start, aptTimeZone);
            End = appointment.AllDay ? appointment.End : FromClientTime(appointment.End, aptTimeZone);
            AllDay = appointment.AllDay;
            Subject = appointment.Subject;
            TimeZone = TimeZones.FirstOrDefault(tz => object.Equals(tz.TimeZone.Id, aptTimeZone.Id));
            Label = Labels.FirstOrDefault(l => l.Id.Equals(appointment.LabelId));
            Status = Statuses.FirstOrDefault(s => s.Id.Equals(appointment.StatusId));
            AssignReminders(appointment);
            AssignRecurrence(appointment.RecurrenceInfo);
        }

        protected virtual void AssignAppointmentData(DateTime start, DateTime end, bool isAllDay) {
            Start = start;
            End = end;
            AllDay = isAllDay;
            TimeZoneInfo storageTimeZone = storage.TimeZone ?? TimeZoneEngine.Local;
            TimeZone = TimeZones.FirstOrDefault(vm => vm.TimeZone.Id == storageTimeZone.Id);
            Label = Labels.FirstOrDefault();
            Status = Statuses.FirstOrDefault();
            Recurrence = neverRecurrence;
        }

        internal void SetRecurrenceInternal(RecurrenceViewModelBase repeatModel) {
            SetProperty(ref recurrence, repeatModel, onChanged: (oldVal, newVal) => {
                if (oldVal != null) {
                    oldVal.PropertyChanged -= OnRecurrencePropertyChanged;
                }
                if (newVal != null) {
                    newVal.PropertyChanged += OnRecurrencePropertyChanged;
                }
            });
        }

        void ValidateStart(TimeSpan duration) {
            if (End < Start)
                Start = End - duration;
        }

        void ValidateEnd(TimeSpan duration) {
            End = Start + duration;
        }

        void OnRecurrencePropertyChanged(object sender, PropertyChangedEventArgs e) {
            IsRecurrenceChanged = true;
        }

        void OnRemindersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (!HasReminders) {
                        HasReminders = true;
                    }
                    if (e.NewStartingIndex == 0) {
                        Reminders[0].IsFirst = true;
                        if (Reminders.Count > 1) {
                            Reminders[1].IsFirst = false;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (HasReminders && Reminders.Count == 0) {
                        HasReminders = false;
                    }
                    if (e.OldStartingIndex == 0) {
                        foreach (ReminderViewModel reminder in e.OldItems) {
                            reminder.IsFirst = false;
                        }
                        if (Reminders.Count > 0) {
                            Reminders[0].IsFirst = true;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (HasReminders) {
                        HasReminders = false;
                    }
                    foreach (ReminderViewModel reminder in e.OldItems) {
                        reminder.IsFirst = false;
                    }
                    break;
            }
        }

        void CreateNewAppointment() {
            AppointmentItem appointmentItem = storage.CreateAppointmentItem();
            PopulateAppointmentValues(appointmentItem);
            storage.AppointmentItems.Add(appointmentItem);
        }

        void PopulateActualAppointment() {
            PopulateAppointmentValues(appointment);
        }

        void PopulateActualAppointmentPattern() {
            PopulateAppointmentValues(storage.GetPattern(appointment));
        }

        void CreateNewPatternByOccurrence() {
            AppointmentItem oldPattern = storage.GetPattern(appointment);
            if (appointment.RecurrenceIndex <= 0) {
                PopulateAppointmentValues(oldPattern);
            } else {
                storage.StopPatternBeforeOccurrence(appointment);
                AppointmentItem actualPattern = storage.CreateAppointmentItem();
                PopulateAppointmentValues(actualPattern);
                storage.AppointmentItems.Add(actualPattern);
            }
        }

        void PopulateAppointmentValues(AppointmentItem model) {
            model.AllDay = AllDay;
            TimeZoneInfo timeZoneInfo = TimeZone?.TimeZone;
            if (model.Type == AppointmentType.Pattern) {
                model.Start = allDay ? model.Start : ToClientTime(model.Start, timeZoneInfo).Date + StartTime;
                model.End = allDay ? model.End : ToClientTime(model.End, timeZoneInfo).Date + EndTime;
            } else {
                model.Start = allDay ? Start : ToClientTime(Start, timeZoneInfo);
                if (allDay)
                    model.End = EndTime == TimeSpan.Zero ? End.AddDays(1) : End;
                else
                    model.End = ToClientTime(End, timeZoneInfo);
            }
            model.Subject = Subject;
            model.LabelId = Label.Id;
            model.StatusId = Status.Id;
            if (!(IsDefaultTimeZone(timeZoneInfo) && string.IsNullOrEmpty(model.TimeZoneId))) {
                model.TimeZoneId = IsDefaultTimeZone(timeZoneInfo) ? string.Empty : timeZoneInfo.Id;
            }

            IRecurrenceInfo recurrenceInfo = PopulateRecurrence(model.RecurrenceInfo, GetNewStartValueForRecurrenceOf(model));
            if (!model.IsOccurrence && AllowRecurrence && recurrenceInfo != null) {
                model.Type = AppointmentType.Pattern;
                model.AssignRecurrenceInfo(recurrenceInfo);
            }
            if (model.Type == AppointmentType.Pattern && recurrenceInfo == null) {
                model.Type = AppointmentType.Normal;
            }

            PopulateReminders(model);
        }

        DateTime? GetNewStartValueForRecurrenceOf(AppointmentItem model) {
            if (model.Type != AppointmentType.Pattern)
                return null;
            return IsDateChanged ? Start : model.Start;
        }

        void UpdateTimeZonesOnItemChange() {
            if (TimeZone == null) {
                foreach (TimeZoneViewModel timeZone in TimeZones) {
                    timeZone.IsSelected = false;
                }
            } else {
                foreach (TimeZoneViewModel timeZone in TimeZones) {
                    timeZone.IsSelected = timeZone == TimeZone;
                }
            }
        }

        void OnTimeZoneSelected(TimeZoneViewModel selectedTimeZone) {
            TimeZone = selectedTimeZone;
        }

        void OnLabelSelected(ColorItemViewModel selectedLabel) {
            Label = selectedLabel;
        }

        void OnStatusSelected(ColorItemViewModel selectedStatus) {
            Status = selectedStatus;
        }
        void OnReminderSelected(ReminderViewModel reminder) {
            if (Reminders.Contains(reminder))
                return;
            Reminders.Add(reminder);
        }

        void OnRecurrenceTypeChanged(RecurrenceViewModelBase recurrenceSettings) {
            if (recurrenceSettings == null)
                return;
            Recurrence = recurrenceSettings;
        }

        void AssignRecurrence(IRecurrenceInfo recurrenceInfo) {
            if (recurrenceInfo == null) {
                SetRecurrenceInternal(neverRecurrence);
            } else {
                SetRecurrenceInternal(AssignRecurrenceType(recurrenceInfo));
                RecurrenceEndingModel.Assign(recurrenceInfo);
            }
        }

        RecurrenceViewModelBase AssignRecurrenceType(IRecurrenceInfo recurrenceInfo) {
            RecurrenceViewModelBase recurrenceViewModel = null;
            if (!typeToViewModelMappings.TryGetValue(recurrenceInfo.Type, out recurrenceViewModel)) {
                // return neverRecurrence;
                throw new Exception("The edited appointment item has recurrence the editor does not support.");
            }
            recurrenceViewModel.Assign(recurrenceInfo);
            return recurrenceViewModel;
        }

        IRecurrenceInfo PopulateRecurrence(IRecurrenceInfo recurrenceInfo, DateTime? newStart) {
            if (Recurrence == neverRecurrence)
                return null;

            if (recurrenceInfo == null) {
                recurrenceInfo = new RecurrenceInfo();
                recurrenceInfo.Start = Start;
            } else if (newStart.HasValue) {
                recurrenceInfo.Start = newStart.Value;
            }

            Recurrence.Populate(recurrenceInfo);
            RecurrenceEndingModel.Populate(recurrenceInfo);
            return recurrenceInfo;
        }

        DateTime FromClientTime(DateTime dateTime, TimeZoneInfo timeZone) {
            return storage.TimeZoneEngine.FromOperationTime(dateTime, GetTimeZoneId(timeZone));
        }
        DateTime ToClientTime(DateTime dateTime, TimeZoneInfo timeZone) {
            return storage.TimeZoneEngine.ToOperationTime(dateTime, GetTimeZoneId(timeZone));
        }
        string GetTimeZoneId(TimeZoneInfo timeZone) {
            return timeZone == null ? storage.TimeZoneEngine.OperationTimeZone.Id : timeZone.Id;
        }
        bool IsDefaultTimeZone(TimeZoneInfo timeZone) {
            return timeZone == null || storage.TimeZoneEngine.OperationTimeZone.Equals(timeZone);
        }

        void AssignReminders(AppointmentItem appointment) {
            if (appointment.Reminders.Count == 0)
                return;
            foreach (TimeSpan timeBeforeStart in appointment.Reminders) {
                Reminders.Add(new ReminderViewModel(timeBeforeStart));
            }
            Reminders[0].IsFirst = true;
        }

        void PopulateReminders(AppointmentItem appointment) {
            List<TimeSpan> oldReminders = new List<TimeSpan>();
            List<TimeSpan> newReminders = new List<TimeSpan>();
            foreach (ReminderViewModel reminderViewModel in Reminders) {
                if (!appointment.Reminders.Contains(reminderViewModel.TimeBeforeStart)) {
                    newReminders.Add(reminderViewModel.TimeBeforeStart);
                } else {
                    oldReminders.Add(reminderViewModel.TimeBeforeStart);
                }
            }

            if (oldReminders.Count != appointment.Reminders.Count || newReminders.Count != 0) {
                IEnumerable<TimeSpan> remindersToRemove = appointment.Reminders
                    .Where(r => !oldReminders.Contains(r))
                    .ToList();
                foreach (TimeSpan oldReminder in remindersToRemove) {
                    appointment.Reminders.Remove(oldReminder);
                }
                appointment.Reminders.AddRange(newReminders);
            }
        }

        bool IsBoundMode(DataSource dataSource) {
            return dataSource != null && dataSource.AppointmentsSource != null && dataSource.AppointmentMappings != null;
        }

        bool CheckAllowTimeZones(DataSource dataSource) {
            return !(IsBoundMode(dataSource) && dataSource.AppointmentMappings.TimeZoneId == null);
        }

        bool CheckAllowReminders(DataSource dataSource) {
            return !(IsBoundMode(dataSource) && dataSource.AppointmentMappings.Reminder == null);
        }

        bool CheckAllowRecurrence(DataSource dataSource) {
            return !(IsBoundMode(dataSource) && !dataSource.AppointmentMappings.SupportsRecurrence);
        }

        IEnumerable<TimeZoneInfoWrapper> GetTimeZones() {
            return (IEnumerable<TimeZoneInfoWrapper>)TimeZoneEngine.TimeZones.Values ?? new List<TimeZoneInfoWrapper>();
        }
    }
}
