using CoreGraphics;

using Foundation;

using ServiceStack.Text;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using TotemAppCore;

using UIKit;

namespace TotemAppIos {
    public partial class EigenschappenViewController : BaseViewController {

		bool isSearching;
		bool isShowingSelected;

		bool IsProfileNull;
		Profiel currProfiel;

		NSUserDefaults userDefs;

		public EigenschappenViewController () : base ("EigenschappenViewController", null) {}

		public override void ViewDidLoad () {
			base.ViewDidLoad ();
			_appController.ShowSelected += toggleShowSelected;
			userDefs = NSUserDefaults.StandardUserDefaults;

			UIApplication.Notifications.ObserveDidEnterBackground ((sender, args) => {
				var ser = JsonSerializer.SerializeToString (_appController.Eigenschappen);
				if (IsProfileNull) {
					userDefs.SetString (ser, "eigenschappen");
					userDefs.Synchronize ();
				} else {
					_appController.AddOrUpdateEigenschappenSer (currProfiel.Name, ser);
				}
			});
		}

		public override void ViewDidAppear (bool animated) {
			base.ViewDidAppear (animated);

			currProfiel = _appController.CurrentProfiel;
			IsProfileNull = (currProfiel == null);

			btnReturn.TouchUpInside+= btnReturnTouchUpInside;
			btnMore.TouchUpInside+= btnMoreTouchUpInside;
			btnSearch.TouchUpInside+= btnSearchTouchUpInside;
			txtSearch.EditingChanged+= TxtSearchValueChangedHandler;
			btnVind.TouchUpInside += btnVindTouchUpInside;

			_appController.UpdateCounter += updateCounter;

			_appController.NavigationController.GotoTotemResultEvent += gotoResultListHandler;

			string ser;

			if (IsProfileNull)
				ser = userDefs.StringForKey ("eigenschappen");
			else
				ser = _appController.GetSerFromProfile (currProfiel.Name);
			
			if (ser != null) {
				_appController.Eigenschappen = JsonSerializer.DeserializeFromString <List<Eigenschap>> (ser);
				(tblEigenschappen.Source as EigenschappenTableViewSource).Eigenschappen = _appController.Eigenschappen;
			}

			tblEigenschappen.ReloadSections (new NSIndexSet (0), UITableViewRowAnimation.None);
			tblEigenschappen.ScrollRectToVisible (new CGRect(0,0,1,1), false);
			_appController.FireUpdateEvent ();
		}

		public override void ViewWillDisappear (bool animated) {
			base.ViewWillDisappear (animated);
			btnReturn.TouchUpInside-= btnReturnTouchUpInside;
			btnMore.TouchUpInside-= btnMoreTouchUpInside;
			btnSearch.TouchUpInside -= btnSearchTouchUpInside;
			txtSearch.EditingChanged -= TxtSearchValueChangedHandler;
			btnVind.TouchUpInside -= btnVindTouchUpInside;

			_appController.UpdateCounter -= updateCounter;

			_appController.NavigationController.GotoTotemResultEvent -= gotoResultListHandler;

			var ser = JsonSerializer.SerializeToString (_appController.Eigenschappen);
			if (IsProfileNull) {
				userDefs.SetString (ser, "eigenschappen");
				userDefs.Synchronize ();
			} else {
				_appController.AddOrUpdateEigenschappenSer (currProfiel.Name, ser);
			}
		}

		public override void setData() {
			lblTitle.Text = (_appController.CurrentProfiel == null) ? "Eigenschappen" : "Selectie";

			imgReturn.Image = UIImage.FromBundle ("SharedAssets/arrow_back_white");
			imgSearch.Image = UIImage.FromBundle ("SharedAssets/search_white");
			imgMore.Image = UIImage.FromBundle ("SharedAssets/more_vert_white");
			imgVind.Image = UIImage.FromBundle ("SharedAssets/arrow_forward_white");

			//bottombar is initially hidden
			bottomBarHeight.Constant = 0;

			//search field is initially hidden
			txtSearch.Hidden=true;
			txtSearch.TintColor = UIColor.White;
			txtSearch.ReturnKeyType = UIReturnKeyType.Search;
			txtSearch.ShouldReturn = (textfield => {
				textfield.ResignFirstResponder ();
				return true;
			});

			//hide keyboard when tapped outside it
			tblEigenschappen.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;

			UIColor color = UIColor.White;
			txtSearch.AttributedPlaceholder = new NSAttributedString("Zoek eigenschap",foregroundColor: color);

			tblEigenschappen.Source = new EigenschappenTableViewSource (_appController.Eigenschappen);

			//empty view at footer to prevent empty cells at the bottom
			tblEigenschappen.TableFooterView = new UIView ();
		}

		new void btnReturnTouchUpInside (object sender, EventArgs e) {
			NavigationController.PopViewController (true);
		}

		//creates options menu
		void btnMoreTouchUpInside (object sender, EventArgs e) {
			UIAlertController actionSheetAlert = UIAlertController.Create(null,null,UIAlertControllerStyle.ActionSheet);

			actionSheetAlert.AddAction(UIAlertAction.Create("Reset selectie",UIAlertActionStyle.Default, action => resetSelections ()));
			actionSheetAlert.AddAction(UIAlertAction.Create(isShowingSelected?"Toon volledige lijst":"Toon enkel selectie",UIAlertActionStyle.Default, action => toggleShowSelected ()));
            if (IsProfileNull)
                actionSheetAlert.AddAction(UIAlertAction.Create("Selectie opslaan", UIAlertActionStyle.Default, action => SaveSelectionPopup()));
            actionSheetAlert.AddAction(UIAlertAction.Create("Individuele weergave",UIAlertActionStyle.Default, action => gotoTinderHandler ()));
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

		void SaveSelectionPopup() {
			UIAlertController actionSheetAlert = UIAlertController.Create (null, null, UIAlertControllerStyle.ActionSheet);
			foreach (Profiel p in _appController.DistinctProfielen) {
				actionSheetAlert.AddAction (UIAlertAction.Create (p.Name, UIAlertActionStyle.Default, action => saveSelectionToProfile (p.Name)));
			}

			actionSheetAlert.AddAction (UIAlertAction.Create ("Nieuw profiel", UIAlertActionStyle.Default, action => addProfileDialog ()));

			actionSheetAlert.AddAction (UIAlertAction.Create ("Annuleer", UIAlertActionStyle.Cancel, null));

			// Required for iPad - You must specify a source for the Action Sheet since it is
			// displayed as a popover
			UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
			if (presentationPopover != null) {
				presentationPopover.SourceView = imgMore;
				presentationPopover.SourceRect = new RectangleF(0, 0, 25, 25);
				presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
			}

			PresentViewController (actionSheetAlert, true, null);
		}

		void saveSelectionToProfile(string name) {
			_appController.AddOrUpdateEigenschappenSer (name, JsonSerializer.SerializeToString (_appController.Eigenschappen));
		}

		void addProfileDialog () {
			var textInputAlertController = UIAlertController.Create("Nieuw profiel", null, UIAlertControllerStyle.Alert);

			textInputAlertController.AddTextField(textField => {
				textField.AutocapitalizationType = UITextAutocapitalizationType.Words;
				textField.Placeholder = "Naam";
			});

			var cancelAction = UIAlertAction.Create ("Annuleer", UIAlertActionStyle.Cancel, alertAction => Console.WriteLine ());
			var okayAction = UIAlertAction.Create ("OK", UIAlertActionStyle.Default, alertAction => addProfile(textInputAlertController.TextFields[0].Text));

			textInputAlertController.AddAction(cancelAction);
			textInputAlertController.AddAction(okayAction);

			PresentViewController(textInputAlertController, true, null);
		}

		//handles wrong input and adds profile
		void addProfile(string name) {
			if (_appController.GetProfielNamen ().Contains (name)) {
				var okAlertController = UIAlertController.Create (null, "Profiel " + name + " bestaat al", UIAlertControllerStyle.Alert);
				okAlertController.AddAction (UIAlertAction.Create ("Ok", UIAlertActionStyle.Default, null));
				PresentViewController (okAlertController, true, null);
			} else if(name.Replace("'", "").Replace(" ", "").Equals("")) {
				var okAlertController = UIAlertController.Create (null, "Ongeldige naam", UIAlertControllerStyle.Alert);
				okAlertController.AddAction (UIAlertAction.Create ("Ok", UIAlertActionStyle.Default, null));
				PresentViewController (okAlertController, true, null);	
			} else {
				_appController.AddProfile (name);
				_appController.AddOrUpdateEigenschappenSer (name, JsonSerializer.SerializeToString (_appController.Eigenschappen));
			}
		}

		//toggles searchbar and handkes visibility, keyboard,...
		void btnSearchTouchUpInside (object sender, EventArgs e) {
			if (isSearching) {
				txtSearch.Hidden = true;
				btnReturn.Hidden = false;
				lblTitle.Hidden = false;
				txtSearch.Text = "";
				TxtSearchValueChangedHandler (txtSearch,null);
				txtSearch.ResignFirstResponder ();
				imgSearch.Image = UIImage.FromBundle ("SharedAssets/search_white");
			} else {
				txtSearch.Hidden = false;
				btnReturn.Hidden = true;
				lblTitle.Hidden = true;
				imgSearch.Image = UIImage.FromBundle ("SharedAssets/close_white");
				txtSearch.BecomeFirstResponder ();
			}
			isSearching = !isSearching;
		}

		//updates list to match entered query
		void TxtSearchValueChangedHandler (object sender, EventArgs e) {
			(tblEigenschappen.Source as EigenschappenTableViewSource).Eigenschappen = _appController.FindEigenschapOpNaam ((sender as UITextField).Text);
			tblEigenschappen.ReloadSections (new NSIndexSet (0), UITableViewRowAnimation.Automatic);
			tblEigenschappen.ScrollRectToVisible (new CGRect(0,0,1,1), false);
			isShowingSelected = false;
		}

		void gotoResultListHandler () {
			NavigationController.PushViewController (new TotemsResultViewController(),true);
		}

		void gotoTinderHandler () {
			NavigationController.PushViewController (new TinderEigenschappenViewController(),true);
		}

		//resets selection
		void resetSelections() {
			foreach (var eigenschap in _appController.Eigenschappen) 
				eigenschap.Selected = false;
			
			txtSearch.Text = "";
			TxtSearchValueChangedHandler (txtSearch,null);
			txtSearch.ResignFirstResponder ();
			tblEigenschappen.ScrollRectToVisible (new CGRect(0,0,1,1), true);
			isShowingSelected = false;
			_appController.FireUpdateEvent ();
		}

		//handles options menu item
		void toggleShowSelected() {				
			if (isShowingSelected) {
				(tblEigenschappen.Source as EigenschappenTableViewSource).Eigenschappen = _appController.FindEigenschapOpNaam (txtSearch.Text);
				tblEigenschappen.ReloadSections (new NSIndexSet (0), UITableViewRowAnimation.Automatic);
				tblEigenschappen.ScrollRectToVisible (new CGRect(0,0,1,1), false);
				isShowingSelected = !isShowingSelected;
			} else if ((_appController.Eigenschappen.FindAll (x=>x.Selected)).Count != 0) {
				(tblEigenschappen.Source as EigenschappenTableViewSource).Eigenschappen = _appController.Eigenschappen.FindAll (x => x.Selected);
				tblEigenschappen.ReloadSections (new NSIndexSet (0), UITableViewRowAnimation.Automatic);
				isShowingSelected = !isShowingSelected;
				tblEigenschappen.ScrollRectToVisible (new CGRect(0,0,1,1), false);
			}
		}

		//updates number of selected eigenschappen on bottom bar
		void updateCounter() {
			int count = _appController.Eigenschappen.FindAll (x => x.Selected).Count;
			lblNumberSelected.Text = count + " geselecteerd";
			bottomBarHeight.Constant = count > 0 ? 50 : 0;
		}

		//adds loading dialog and calculates totems
		void btnVindTouchUpInside (object sender, EventArgs e) {
			nfloat centerX = View.Frame.Width / 2;
			nfloat centerY = View.Frame.Height / 2;

			//spinner, label and surrounding view
			var activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			activitySpinner.Frame = new CGRect (centerX - (activitySpinner.Frame.Width / 2), centerY - activitySpinner.Frame.Height, 40f, 40f);
			var view = new UIView(new CGRect(centerX, centerY, 150f, 150f));
			var label = new UILabel(new CGRect(centerX - (130f/2), centerY + 20, 130f, 22f));

			//UI settings
			label.Text = "Totems zoeken...";
			label.TextColor = UIColor.White;
			view.Layer.CornerRadius = 15f;
			view.BackgroundColor = UIColor.Black;
			view.Alpha = 0.75f;
			view.Center = View.Center;
			activitySpinner.AutoresizingMask = UIViewAutoresizing.All;

			//add to currnet view
			View.AddSubview (view);
			View.AddSubview (label);
			View.AddSubview (activitySpinner);

			//bring to front
			View.BringSubviewToFront (view);
			View.BringSubviewToFront (label);
			View.BringSubviewToFront (activitySpinner);

			//start spinner
			activitySpinner.StartAnimating ();

			//calculate list on different thread and stop/hide loading dialog afterwards
			new Thread(new ThreadStart(() => InvokeOnMainThread (() => {
				_appController.CalculateResultlist (_appController.Eigenschappen);
				activitySpinner.StopAnimating ();
				view.Hidden = true;
				label.Hidden = true;
			}))).Start();
		}
	}
}