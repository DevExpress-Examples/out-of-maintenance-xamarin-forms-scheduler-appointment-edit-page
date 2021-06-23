using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XamarinForms.Scheduler;
using DevExpress.XamarinForms.Scheduler.Internal;

namespace SchedulerExample.AppointmentPages {
    public class EnumItem<T> where T : Enum {
        public T Value { get; }
        public string DisplayName { get; }

        public EnumItem(T value, string displayName) {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    static class EnumTextHelper {
        public static List<EnumItem<WeekOfMonth>> CreateWeekOfMonthItems() {
            return new List<EnumItem<WeekOfMonth>> {
                new EnumItem<WeekOfMonth>(WeekOfMonth.First, "first"),
                new EnumItem<WeekOfMonth>(WeekOfMonth.Second, "second"),
                new EnumItem<WeekOfMonth>(WeekOfMonth.Third, "third"),
                new EnumItem<WeekOfMonth>(WeekOfMonth.Fourth, "fourth"),
                new EnumItem<WeekOfMonth>(WeekOfMonth.Last, "last")
            };
        }
        public static List<EnumItem<WeekDays>> CreateWeekDaysItems() {
            return new List<EnumItem<WeekDays>> {
                new EnumItem<WeekDays>(WeekDays.EveryDay, "day"),
                new EnumItem<WeekDays>(WeekDays.WeekendDays, "weekend"),
                new EnumItem<WeekDays>(WeekDays.WorkDays, "work day"),
                new EnumItem<WeekDays>(WeekDays.Monday, "Monday"),
                new EnumItem<WeekDays>(WeekDays.Friday, "Friday"),
                new EnumItem<WeekDays>(WeekDays.Saturday, "Saturday"),
                new EnumItem<WeekDays>(WeekDays.Sunday, "Sunday"),
                new EnumItem<WeekDays>(WeekDays.Thursday, "Thursday"),
                new EnumItem<WeekDays>(WeekDays.Wednesday, "Wednesday")
            };
        }
    }
}
