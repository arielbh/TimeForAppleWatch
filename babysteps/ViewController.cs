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
		List<Feeding> _feedings = new List<Feeding>();

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
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}


	public class TableSource : UITableViewSource
	{

		List<Feeding> TableItems;
		string CellIdentifier = "TableCell";

		public TableSource(List<Feeding> items)
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

			cell.TextLabel.Text = $"Feeding from {item.Start} and ended at {item.End}";

			return cell;
		}
	}

	public class Feeding
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

	}

}
