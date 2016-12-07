using System;

using WatchKit;
using Foundation;
using System.Collections.Generic;
using UserNotifications;

namespace babysteps.watchExtension
{
	public partial class InterfaceController : WKInterfaceController
	{
		List<string> rows = new List<string>();

		protected InterfaceController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void Awake(NSObject context)
		{
			base.Awake(context);

			// Configure interface objects here.
			Console.WriteLine("{0} awake with context", this);
			rows.Add("Feeding");
			rows.Add("Workout");
		}

		public override void WillActivate()
		{
			// This method is called when the watch view controller is about to be visible to the user.
			Console.WriteLine("{0} will activate", this);
			LoadTableRows();
			RegisterNotification();
		}

		void RegisterNotification()
		{
			UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
			{
				var content = new UNMutableNotificationContent();
				content.Title = "Diaper!";
				content.Body = "It's time!";

				//content.CategoryIdentifier = "ChangeDiaper";
				var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(10.0, false);
				var request = UNNotificationRequest.FromIdentifier("ChangeDiaper", content, trigger);

				UNUserNotificationCenter.Current.AddNotificationRequest(request, Completion);
				UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();
			});	
		}

		void Completion(NSError obj)
		{
		}

		void LoadTableRows()
		{

			menuTable.SetNumberOfRows(rows.Count, "default");
			for (int i = 0; i < rows.Count; i++)
			{
				var row = (RowController)menuTable.GetRowController(i);
				row.titleLabel.SetText(rows[i]);
			}

		}

		public override void DidDeactivate()
		{
			// This method is called when the watch view controller is no longer visible to the user.
			Console.WriteLine("{0} did deactivate", this);
		}

		public override void DidSelectRow(WKInterfaceTable table, nint rowIndex)
		{
			var rowData = rows[(int)rowIndex];
			Console.WriteLine("Row selected:" + rowData);
			PushController($"{rowData}Controller", string.Empty);
		}
	}

	public class UserNotificationCenterDelegate : UNUserNotificationCenterDelegate
	{
		#region Constructors
		public UserNotificationCenterDelegate()
		{
		}
		#endregion

		#region Override Methods
		public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			// Do something with the notification
			Console.WriteLine("Active Notification: {0}", notification);

			// Tell system to display the notification anyway or use
			// `None` to say we have handled the display locally.
			completionHandler(UNNotificationPresentationOptions.Alert);
		}
		#endregion
	}
}
