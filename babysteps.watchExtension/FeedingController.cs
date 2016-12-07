using Foundation;
using System;
using WatchKit;
using System.Collections.Generic;

namespace babysteps.watchExtension
{
    public partial class FeedingController : WKInterfaceController
    {
		bool _feedingInSession;
		DateTime _startDate;
        public FeedingController (IntPtr handle) : base (handle)
        {
        }

		partial void OnStart()
		{
			if (_feedingInSession) return;
			_feedingInSession = true;
			_startDate = DateTime.Now;
			Timer.SetDate(new NSDate());
			Timer.Start();
		}

		partial void OnEnd()
		{
			if (!_feedingInSession) return;
			_feedingInSession = false;
			Timer.Stop();
			SessionManager.SharedManager.UpdateApplicationContext(
				new Dictionary<string, object>()
				{
				//{ "isFeeding", true},
				{ "startDate", _startDate.ToString("HH:mm:ss")},
				{ "endDate", DateTime.Now.ToString("HH:mm:ss")}
				});
		}
	}
}