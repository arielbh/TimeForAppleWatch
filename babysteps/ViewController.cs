using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using WatchConnectivity;

namespace babysteps
{
	public partial class ViewController : UIViewController
	{
		List<Message> _feedings = new List<Message>();

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			//_feedings.Add(new Feeding()
			//{
			//	Start = "12:34",
			//	End = "12:55"
			//});
			Table.Source = new TableSource(_feedings);

			SessionManager.SharedManager.ApplicationContextUpdated += SharedManager_ApplicationContextUpdated;
			// Perform any additional setup after loading the view, typically from a nib.
		}

		void SharedManager_ApplicationContextUpdated(WCSession session, Dictionary<string, object> applicationContext)
		{
			if (applicationContext.ContainsKey("startDate") && applicationContext.ContainsKey("endDate"))
			{
				_feedings.Add(new Feeding
				{
					Start = applicationContext["startDate"] as string,
					End = applicationContext["endDate"] as string,

				});
				InvokeOnMainThread(() =>
				{
					this.Table.ReloadData();
				});
			}
			if (applicationContext.ContainsKey("HeartRate"))
			{
				_feedings.Add(new Message
				{
					Text = $"Check up Baby it got Heart Rate: {applicationContext["HeartRate"] as string}"

				});
				InvokeOnMainThread(() =>
				{
					this.Table.ReloadData();
				});
			}
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}


	public class TableSource : UITableViewSource
	{

		List<Message> TableItems;
		string CellIdentifier = "TableCell";

		public TableSource(List<Message> items)
		{
			TableItems = items;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return TableItems.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
			var item = TableItems.ElementAt(indexPath.Row);

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier); }

			cell.TextLabel.Text = item.ToString();

			return cell;
		}
	}

	public class Message
	{

		public string Text
		{
			get;
			set;
		}
		public override string ToString()
		{
			return Text;
		}
		
	}

	public class Feeding : Message
	{
		public string Start
		{
			get;
			set;
		}
		public string End
		{
			get;
			set;
		}

		public override string ToString()
		{
			return $"Feeding from {Start} and ended at {End}";
		}

	}

}
