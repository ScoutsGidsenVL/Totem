﻿using System;

using UIKit;
using System.Collections.Generic;
using System.Xml;
using Foundation;

namespace TotemAppIos {
	public partial class ChecklistViewController : UIViewController {
		Dictionary<string, List<string>> tableData;

		public ChecklistViewController () : base ("ChecklistViewController", null) {}

		public override void ViewDidLoad () {
			base.ViewDidLoad ();
			setData ();
			NavigationController.NavigationBarHidden = true;
			NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
		}

		public override void ViewDidAppear (bool animated) {
			base.ViewDidAppear (animated);
			btnReturn.TouchUpInside+= btnReturnTouchUpInside;
		}

		public override void ViewWillDisappear (bool animated) {
			base.ViewWillDisappear (animated);
			btnReturn.TouchUpInside-= btnReturnTouchUpInside;
		}

		public override UIStatusBarStyle PreferredStatusBarStyle ()	{
			return UIStatusBarStyle.LightContent;
		}

		public override void DidReceiveMemoryWarning () {
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		private void setData() {
			lblTitle.Text = "Totemisatie checklist";

			imgReturn.Image = UIImage.FromBundle ("SharedAssets/arrow_back_white");

			ExtrectDataFromXML ();
		}

		void ExtrectDataFromXML() {
			tableData = new Dictionary<string, List<string>>();

			XmlDocument doc = new XmlDocument ();
			doc.Load(NSBundle.MainBundle.PathForResource ("SharedAssets/checklist","xml"));
			var childNodes = doc.FirstChild.ChildNodes;
			foreach (var item in childNodes) {
				var itemName=(item as XmlElement).GetAttribute ("name");
				tableData.Add (itemName,new List<string>());
				var children = (item as XmlElement).ChildNodes;
				foreach (var child in children) {

					tableData [itemName].Add ((child as XmlElement).InnerText);
				}
			}
		}

		void btnReturnTouchUpInside (object sender, EventArgs e) {
			NavigationController.PopViewController (true);
		}
	}
}