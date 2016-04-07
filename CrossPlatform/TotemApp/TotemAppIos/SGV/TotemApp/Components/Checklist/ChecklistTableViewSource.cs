﻿using System;
using UIKit;
using System.Collections.Generic;
using Foundation;

namespace TotemAppIos {
	public class ChecklistTableViewSource : UITableViewSource {
		List<string> dict;

		public ChecklistTableViewSource (List<string> dict) {
			this.dict = dict;
		}

		/*public override UIView GetViewForHeader (UITableView tableView, nint section) {
			throw new System.NotImplementedException ();
		}

		public override nfloat GetHeightForHeader (UITableView tableView, nint section) {
			throw new System.NotImplementedException ();
		}

		public override nfloat GetHeightForFooter (UITableView tableView, nint section) {
			throw new System.NotImplementedException ();
		}

		public override UIView GetViewForFooter (UITableView tableView, nint section)
		{
			throw new System.NotImplementedException ();
		}*/

		#region implemented abstract members of UITableViewSource

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			BaseChecklistTableViewCell cell;

			var item = dict [indexPath.Row];
			string[] data = item.Split ('_');
			var type = data [0];
			var content = data [1];

			cell = tableView.DequeueReusableCell (new NSString ("NormalTableViewCell")) as BaseChecklistTableViewCell;

			if (type.Equals ("i") && (cell == null || !cell.Key.Equals ("IndentTableViewCell"))) {
				cell = IndentTableViewCell.Create ();
			} else if (type.Equals ("n") && (cell == null || !cell.Key.Equals ("NormalTableViewCell"))) {
				cell = NormalTableViewCell.Create ();
			} else if (type.Equals ("h") && (cell == null || !cell.Key.Equals ("HeadTableViewCell"))) {
				cell = HeadTableViewCell.Create ();
			} else if (cell == null || !cell.Key.Equals ("TitleTableViewCell")) {
				cell = TitleTableViewCell.Create ();
			}

			cell.setData (content);

			return cell;
		}

		public override nint RowsInSection (UITableView tableview, nint section) {
			return dict.Count;
		}

		/*public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath) {
			return 38.0f;
		}*/

		public override nint NumberOfSections (UITableView tableView) {
			return 1;
		}

		#endregion
	}
}