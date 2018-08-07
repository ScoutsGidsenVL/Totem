using Foundation;

using System;
using System.Collections.Generic;

using TotemAppCore;

using UIKit;

namespace TotemAppIos {
	public class ProfielTotemsTableViewSource : UITableViewSource {
		public ProfielTotemsTableViewSource () {}

		public ProfielTotemsTableViewSource (List<Totem> totems) {
			Totems = totems;
		}

		AppController _appController = AppController.Instance;

		public List<Totem> Totems { get; set; }

		public bool check { get; set; }

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath) {
			ProfielTotemsTableViewCell cell;

			cell = tableView.DequeueReusableCell (ProfielTotemsTableViewCell.Key) as ProfielTotemsTableViewCell;
			if (cell == null)
				cell = ProfielTotemsTableViewCell.Create ();

			cell.Totem = Totems [indexPath.Row];

			//make sperator full width
			cell.PreservesSuperviewLayoutMargins = false;
			cell.SeparatorInset = UIEdgeInsets.Zero;
			cell.LayoutMargins = UIEdgeInsets.Zero;

			cell.setData (check);

			return cell;
		}

		public override nint RowsInSection (UITableView tableview, nint section) {
			return Totems.Count;
		}

		public override nint NumberOfSections (UITableView tableView) {
			return 1;
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) {
			return 50;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath) {
			if(check)
				(tableView.CellAt (indexPath) as ProfielTotemsTableViewCell).toggleCheckbox ();
			else
				_appController.ProfileTotemSelected (_appController.CurrentProfiel.Name, Totems[indexPath.Row].Nid);
			
			tableView.DeselectRow (indexPath,true);
		}
	}
}