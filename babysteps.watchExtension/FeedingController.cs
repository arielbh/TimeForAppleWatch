using Foundation;
using System;
using WatchKit;

namespace babysteps.watchExtension
{
    public partial class FeedingController : WKInterfaceController
    {
        public FeedingController (IntPtr handle) : base (handle)
        {
        }

		partial void OnStart()
		{
		}

		partial void OnEnd()
		{
		}
	}
}