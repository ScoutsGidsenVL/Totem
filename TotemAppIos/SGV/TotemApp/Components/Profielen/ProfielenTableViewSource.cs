using Foundation;

using System;
using System.Collections.Generic;

using TotemAppCore;

using UIKit;

namespace TotemAppIos {
	public class ProfielenTableViewSource : UITableViewSource {

		AppController _appController = AppController.Instance;

		public ProfielenTableViewSource (List<Profiel> profielen) {
			Profielen = profielen;
		}

		public List<Profiel> Profielen { get; set; }

		public bool check { get; set; }

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath) {
			ProfielenTableViewCell cell;

			cell = tableView.DequeueReusableCell (ProfielenTableViewCell.Key) as ProfielenTableViewCell;
			if (cell == null)
				cell = ProfielenTableViewCell.Create ();

			cell.Profiel = Profielen [indexPath.Row];

			//make sperator full width
			cell.PreservesSuperviewLayoutMargins = false;
			cell.SeparatorInset = UIEdgeInsets.Zero;
			cell.LayoutMargins = UIEdgeInsets.Zero;

			cell.setData (check);

			return cell;
		}

		public override nint RowsInSection (UITableView tableview, nint section) {
			return Profielen.Count;
		}

		public override nint NumberOfSections (UITableView tableView) {
			return 1;
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) {
			return 50;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath) {
			if(check)
				(tableView.CellAt (indexPath) as ProfielenTableViewCell).toggleCheckbox ();
			else
				_appController.ProfileSelected (Profielen[indexPath.Row].Name);

			tableView.DeselectRow (indexPath,true);
		}
	}
}