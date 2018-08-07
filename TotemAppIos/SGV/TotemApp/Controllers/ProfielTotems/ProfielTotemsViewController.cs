using System;
using System.Collections.Generic;
using System.Drawing;

using TotemAppCore;

using UIKit;

namespace TotemAppIos {
	public partial class ProfielTotemsViewController : BaseViewController {

		List<Totem> totems;
		Profiel currProfiel;

		public ProfielTotemsViewController () : base ("ProfielTotemsViewController", null) {}

		public override void ViewDidAppear (bool animated) {
			base.ViewDidAppear (animated);
			currProfiel = _appController.CurrentProfiel;
			if (tblTotems.Source != null) {
				var list = _appController.GetTotemsFromProfiel (currProfiel.Name);
				var empty = (list.Count == 0);
				tblTotems.Hidden = empty;
				btnDelete.Hidden = empty;
				(tblTotems.Source as ProfielTotemsTableViewSource).Totems = list;
				tblTotems.ReloadSections (new Foundation.NSIndexSet (0), UITableViewRowAnimation.None);
			}

			btnReturn.TouchUpInside += btnReturnTouchUpInside;
			btnDelete.TouchUpInside += deleteProfileTotems;
			btnMore.TouchUpInside += btnMoreTouchUpInside;

			_appController.NavigationController.GotoTotemDetailEvent += gotoTotemDetailHandler;
			_appController.NavigationController.GotoEigenschapListEvent += gotoEigenschappenListHandler;
		}

		public override void ViewWillDisappear (bool animated) {
			base.ViewWillDisappear (animated);
			btnReturn.TouchUpInside -= btnReturnTouchUpInside;
			btnDelete.TouchUpInside -= deleteProfileTotems;
			btnMore.TouchUpInside -= btnMoreTouchUpInside;

			_appController.NavigationController.GotoTotemDetailEvent -= gotoTotemDetailHandler;
			_appController.NavigationController.GotoEigenschapListEvent -= gotoEigenschappenListHandler;
		}

		public override void setData() {
			lblTitle.Text = "Totems";

			imgReturn.Image = UIImage.FromBundle ("SharedAssets/arrow_back_white");
			imgDelete.Image = UIImage.FromBundle ("SharedAssets/delete_white");
			imgMore.Image = UIImage.FromBundle ("SharedAssets/more_vert_white");

			totems = _appController.GetTotemsFromProfiel (_appController.CurrentProfiel.Name);
			tblTotems.Source = new ProfielTotemsTableViewSource (totems);

			//hide necessary UI elements
			var empty = (totems.Count == 0);
			tblTotems.Hidden = empty;
			btnDelete.Hidden = empty;

			//empty view at footer to prevent empty cells at the bottom
			tblTotems.TableFooterView = new UIView ();
		}

		//creates options menu
		void btnMoreTouchUpInside (object sender, EventArgs e) {
			UIAlertController actionSheetAlert = UIAlertController.Create(null,null,UIAlertControllerStyle.ActionSheet);

			actionSheetAlert.AddAction(UIAlertAction.Create("Selectie weergeven",UIAlertActionStyle.Default, action => ViewSelection ()));
			actionSheetAlert.AddAction(UIAlertAction.Create("Annuleer",UIAlertActionStyle.Cancel, null));

			// Required for iPad - You must specify a source for the Action Sheet since it is
			// displayed as a popover
			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover!=null) {
				presentationPopover.SourceView = imgMore;
				presentationPopover.SourceRect = new RectangleF(0, 0, 25, 25);
				presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}

			PresentViewController(actionSheetAlert,true,null);
		}

		void ViewSelection() {
			_appController.ProfileEigenschappenSelected (_appController.CurrentProfiel.Name);
		}
			
		void gotoTotemDetailHandler() {
			NavigationController.PushViewController (new TotemDetailViewController(), true);
		}

		void gotoEigenschappenListHandler() {
			NavigationController.PushViewController (new EigenschappenViewController(), true);
		}

		//updates data of TableView and handles necessary UI changes
		void updateListSource() {
			totems = _appController.GetTotemsFromProfiel (_appController.CurrentProfiel.Name);
			(tblTotems.Source as ProfielTotemsTableViewSource).Totems = totems;
			tblTotems.ReloadSections (new Foundation.NSIndexSet (0), UITableViewRowAnimation.None);
			var empty = totems.Count == 0;
			tblTotems.Hidden = empty;
			btnDelete.Hidden = empty;
		}

		//handles UI changes for deleting totems
		void deleteProfileTotems(object sender, EventArgs e) {
			imgReturn.Image = UIImage.FromBundle ("SharedAssets/close_white");
			btnReturn.TouchUpInside -= btnReturnTouchUpInside;
			btnReturn.TouchUpInside += exitDelete;
			lblTitle.Hidden = true;
			((ProfielTotemsTableViewSource)tblTotems.Source).check = true;
			tblTotems.ReloadSections (new Foundation.NSIndexSet (0), UITableViewRowAnimation.None);
			btnDelete.TouchUpInside -= deleteProfileTotems;
			btnDelete.TouchUpInside += deleteDialog;
		}

		//reverts UI changes for deleting totems
		void exitDelete(object sender, EventArgs e) {
			imgReturn.Image = UIImage.FromBundle ("SharedAssets/arrow_back_white");
			btnReturn.TouchUpInside -= exitDelete;
			btnReturn.TouchUpInside += btnReturnTouchUpInside;
			lblTitle.Hidden = false;
			deselectAndUpdate ();
			((ProfielTotemsTableViewSource)tblTotems.Source).check = false;
			tblTotems.ReloadSections (new Foundation.NSIndexSet (0), UITableViewRowAnimation.None);
			btnDelete.TouchUpInside -= deleteDialog;
			btnDelete.TouchUpInside += deleteProfileTotems;
		}

		//delete confirmation dialog
		void deleteDialog(object sender, EventArgs e) {
			var okCancelAlertController = UIAlertController.Create(null, "Geselecteerde totems verwijderen?", UIAlertControllerStyle.Alert);

			okCancelAlertController.AddAction(UIAlertAction.Create("Ja", UIAlertActionStyle.Default, alert => deleteSelected(sender, e)));
			okCancelAlertController.AddAction(UIAlertAction.Create("Nee", UIAlertActionStyle.Cancel, null));

			PresentViewController(okCancelAlertController, true, null);
		}

		//the actual deletion of the selected profiles
		void deleteSelected(object sender, EventArgs e) {
			var deleteList = totems.FindAll (x => x.Selected);
			foreach (Totem t in deleteList)
				_appController.DeleteTotemFromProfile (t.Nid, _appController.CurrentProfiel.Name);

			updateListSource ();
			exitDelete (sender, e);
		}

		//resets selection and updates list
		void deselectAndUpdate() {
			totems = _appController.GetTotemsFromProfiel (_appController.CurrentProfiel.Name);
			var deselect = totems.FindAll (x => x.Selected);
			foreach (Totem t in deselect)
				t.Selected = false;

			(tblTotems.Source as ProfielTotemsTableViewSource).Totems = totems;
		}
	}
}