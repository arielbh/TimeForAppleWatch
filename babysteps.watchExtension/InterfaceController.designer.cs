// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace babysteps.watchExtension
{
    [Register ("InterfaceController")]
    partial class InterfaceController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WatchKit.WKInterfaceTable menuTable { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (menuTable != null) {
                menuTable.Dispose ();
                menuTable = null;
            }
        }
    }
}