using Xamarin.Forms;
using DevExpress.XamarinForms.Scheduler;
using SchedulerExample.AppointmentPages;


namespace SchedulerExample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void DayView_Tap(System.Object sender, SchedulerGestureEventArgs e)
        {
            if (e.AppointmentInfo == null)
            {
                Navigation.PushAsync(new CustomAppointmentEditPage(e.IntervalInfo.Start, e.IntervalInfo.End, e.IntervalInfo.AllDay, this.dataStorage, true));
                return;
            }
            Navigation.PushAsync(new CustomAppointmentDetailPage(e.AppointmentInfo.Appointment, this.dataStorage, true));
        }
    }
}
