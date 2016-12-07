using System;

using WatchKit;
using Foundation;
using System.Collections.Generic;

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
}
