// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using WatchKit;

namespace babysteps.watchExtension
{
    [Register ("WorkoutController")]
    partial class WorkoutController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceLabel EnergyLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceLabel HeartRateLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceButton ToggleWorkoutButton { get; set; }

        [Action ("OnToggleWorkout")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnToggleWorkout ();

        void ReleaseDesignerOutlets ()
        {
            if (EnergyLabel != null) {
                EnergyLabel.Dispose ();
                EnergyLabel = null;
            }

            if (HeartRateLabel != null) {
                HeartRateLabel.Dispose ();
                HeartRateLabel = null;
            }

            if (ToggleWorkoutButton != null) {
                ToggleWorkoutButton.Dispose ();
                ToggleWorkoutButton = null;
            }
        }
    }
}