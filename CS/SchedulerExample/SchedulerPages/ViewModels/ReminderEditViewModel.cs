using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using DevExpress.XamarinForms.Scheduler.Internal;
using DevExpress.XamarinForms.Scheduler;

namespace SchedulerExample.AppointmentPages {
    public class CustomReminderEditViewModel : NotifyPropertyChangedBase {
        const int DefaultUnitNumber = 10;
        int unitNumber;
        TimeSpanBuilder selectedBuilder;
        public ReminderViewModel reminder;
        Action<TimeSpan> timeBeforeStartSelectedCallback;
        Command timeSpanSelectedCommand;

        public string Title => "Custom Reminder";

        public IEnumerable<TimeSpanBuilder> TimeSpanBuilders { get; }
        public int UnitNumber {
            get => unitNumber;
            set => SetProperty(ref unitNumber, value, onChanged: (oldVal, newVal) => {
                foreach (TimeSpanBuilder buiderVM in TimeSpanBuilders) {
                    buiderVM.UpdateActualDisplayName(newVal);
                }
                timeSpanSelectedCommand.ChangeCanExecute();
            });
        }
        public TimeSpanBuilder SelectedBuilder {
            get => selectedBuilder;
            set => SetProperty(ref selectedBuilder, value, onChanged: (oldVal, newVal) => {
                if (oldVal != null)
                    oldVal.IsSelected = false;
                if (newVal != null)
                    newVal.IsSelected = true;
            });
        }
        public ICommand TimeSpanSelectedCommand => timeSpanSelectedCommand;


        public CustomReminderEditViewModel(IEnumerable<TimeSpanBuilder> timeSpanBuilders = null, Action<TimeSpan> timeBeforeStartSelectedCallback = null) {
            this.timeSpanSelectedCommand = new Command(ExecuteTimeSpanSelectedCommand, CanExecuteTimeSpanSelectedCommand);
            this.timeBeforeStartSelectedCallback = timeBeforeStartSelectedCallback;

            TimeSpanBuilders = timeSpanBuilders != null ? timeSpanBuilders : TimeSpanBuilder.DefaultBuilders;
            foreach (TimeSpanBuilder vm in TimeSpanBuilders) { vm.IsSelected = false; }

            UnitNumber = DefaultUnitNumber;
            SelectedBuilder = TimeSpanBuilders.FirstOrDefault();
        }

        void ExecuteTimeSpanSelectedCommand() {
            timeBeforeStartSelectedCallback?.Invoke(SelectedBuilder.Build(UnitNumber));
        }

        bool CanExecuteTimeSpanSelectedCommand() {
            return unitNumber >= 0;
        }
    }

    public class TimeSpanBuilder : NotifyPropertyChangedBase {
        public static readonly TimeSpanBuilder SecondBuilder = new TimeSpanBuilder("Second", "Seconds", (v) => new TimeSpan(0, 0, v));
        public static readonly TimeSpanBuilder MinuteBuilder = new TimeSpanBuilder("Minute", "Minutes", (v) => new TimeSpan(0, v, 0));
        public static readonly TimeSpanBuilder HourBuilder = new TimeSpanBuilder("Hour", "Hours", (v) => new TimeSpan(v, 0, 0));
        public static readonly TimeSpanBuilder DayBuilder = new TimeSpanBuilder("Day", "Days", (v) => new TimeSpan(v, 0, 0, 0));
        public static readonly IEnumerable<TimeSpanBuilder> DefaultBuilders = new List<TimeSpanBuilder> { SecondBuilder, MinuteBuilder, HourBuilder, DayBuilder };

        public const string SelectedDisplayNameFormat = "{0} before";

        bool isSelected;
        string actualDisplayNameForm;
        Func<int, TimeSpan> buildHandler;

        string ActualDisplayNameForm { get => actualDisplayNameForm; set => SetProperty(ref actualDisplayNameForm, value, onChanged: (oldVal, newVal) => RaisePropertyChanged(nameof(ActualDisplayName), null)); }

        public string SingularDisplayName { get; }
        public string PluralDisplayName { get; }
        public string ActualDisplayName => (isSelected) ? String.Format(SelectedDisplayNameFormat, actualDisplayNameForm) : actualDisplayNameForm;

        public bool IsSelected {
            get => isSelected;
            set => SetProperty(ref isSelected, value, onChanged: (oldVal, newVal) => RaisePropertyChanged(nameof(ActualDisplayName), null));
        }

        public TimeSpanBuilder(string singularDisplayName, string pluralDisplayName, Func<int, TimeSpan> buildHandler) {
            SingularDisplayName = singularDisplayName;
            PluralDisplayName = pluralDisplayName;
            this.buildHandler = buildHandler;
        }

        public TimeSpan Build(int units) {
            return buildHandler(units);
        }

        public void UpdateActualDisplayName(int unitNumber) {
            ActualDisplayNameForm = (unitNumber == 1) ? SingularDisplayName : PluralDisplayName;
        }
    }
}
