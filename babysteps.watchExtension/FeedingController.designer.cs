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
    [Register ("FeedingController")]
    partial class FeedingController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceButton EndButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceButton StartButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceTimer Timer { get; set; }

        [Action ("OnEnd")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnEnd ();

        [Action ("OnStart")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnStart ();

        void ReleaseDesignerOutlets ()
        {
            if (EndButton != null) {
                EndButton.Dispose ();
                EndButton = null;
            }

            if (StartButton != null) {
                StartButton.Dispose ();
                StartButton = null;
            }

            if (Timer != null) {
                Timer.Dispose ();
                Timer = null;
            }
        }
    }
}