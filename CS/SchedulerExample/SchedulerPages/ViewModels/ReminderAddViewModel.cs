using System;
using System.Collections.Generic;
using System.Windows.Input;
using DevExpress.XamarinForms.Scheduler;
using Xamarin.Forms;

namespace SchedulerExample.AppointmentPages {
    public class CustomReminderAddViewModel {
        Action<ReminderViewModel> reminderSelectedCallback;

        public string Title => "Reminder";
        public IEnumerable<ReminderViewModel> Reminders { get; }
        public ICommand SelectReminderCommand { get; }

        public CustomReminderAddViewModel(IEnumerable<ReminderViewModel> reminders, Action<ReminderViewModel> reminderSelectedCallback) {
            Reminders = reminders;
            SelectReminderCommand = new Command(ExecuteSelectReminderCommand);
            this.reminderSelectedCallback = reminderSelectedCallback;
        }

        public virtual CustomReminderEditViewModel CreateEditReminderViewModel(Action onReminderEditedCallback = null) {
            return new CustomReminderEditViewModel(timeBeforeStartSelectedCallback: (ts) => {
                OnReminderEdited(ts);
                onReminderEditedCallback?.Invoke();
            });
        }

        void ExecuteSelectReminderCommand(object param) {
            if (!(param is ReminderViewModel viewModel))
                return;
            reminderSelectedCallback?.Invoke(viewModel);
        }

        void OnReminderEdited(TimeSpan timeSpan) {
            reminderSelectedCallback?.Invoke(new ReminderViewModel(timeSpan));
        }
    }
}
