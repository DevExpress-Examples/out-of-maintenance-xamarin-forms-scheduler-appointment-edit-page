using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using DevExpress.XamarinForms.Scheduler.Internal;
using DevExpress.XamarinForms.Scheduler;

namespace SchedulerExample.AppointmentPages {
    public class CustomRecurrenceEditViewModel: NotifyPropertyChangedBase {
        RecurrenceViewModelBase selectedRecurrenceType;
        Action<RecurrenceViewModelBase> selectRecurrenceCallback;

        public IEnumerable<RecurrenceViewModelBase> RecurrenceTypes { get; }
        public RecurrenceEndingViewModel RecurrenceEndingSettings { get; }
        public RecurrenceViewModelBase SelectedRecurrenceType {
            get => selectedRecurrenceType;
            set => SetProperty(ref selectedRecurrenceType, value, onChanged: (oldVal, newVal) => {
                if (oldVal != null) oldVal.IsSelected = false;
                if (newVal != null) newVal.IsSelected = true;
                RaisePropertyChanged(nameof(HasRecurrence), null);
            });
        }
        public bool HasRecurrence => !(SelectedRecurrenceType is NeverRecurrenceViewModel);
        
        public ICommand SelectRecurrenceCommand { get; }


        public CustomRecurrenceEditViewModel(
            IEnumerable<RecurrenceViewModelBase> recurrenceTypes,
            RecurrenceEndingViewModel recurrenceEndingSettings,
            RecurrenceViewModelBase selectedRecurrenceType = null,
            Action<RecurrenceViewModelBase> selectCallback = null
        ) {
            foreach (RecurrenceViewModelBase recurrenceType in recurrenceTypes) { recurrenceType.IsSelected = false; }

            RecurrenceTypes = recurrenceTypes;
            RecurrenceEndingSettings = recurrenceEndingSettings;
            SelectedRecurrenceType = selectedRecurrenceType != null ? selectedRecurrenceType : recurrenceTypes.First();

            SelectRecurrenceCommand = new Command(ExecuteSelectRecurrenceCommand);

            selectRecurrenceCallback = selectCallback;
        }

        void ExecuteSelectRecurrenceCommand(object parameter) {
            selectRecurrenceCallback?.Invoke(SelectedRecurrenceType);
        }
    }
}
