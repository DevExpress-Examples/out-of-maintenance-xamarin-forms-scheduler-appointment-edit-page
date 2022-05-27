using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SchedulerExample
{
    public partial class App : Application
    {
        public App()
        {
            DevExpress.XamarinForms.Scheduler.Initializer.Init();
            DevExpress.XamarinForms.Editors.Initializer.Init();
            DevExpress.XamarinForms.CollectionView.Initializer.Init();
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
