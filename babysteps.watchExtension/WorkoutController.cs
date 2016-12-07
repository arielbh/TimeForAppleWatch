using Foundation;
using System;
using WatchKit;

namespace babysteps.watchExtension
{
    public partial class WorkoutController : WKInterfaceController
    {
        public WorkoutController (IntPtr handle) : base (handle)
        {
        }

		partial void OnToggleWorkout()
		{
		}
	}
}