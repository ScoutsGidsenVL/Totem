using Foundation;

using System;

using TotemAppCore;

using UIKit;

namespace TotemAppIos {
	public partial class ProfielenTableViewCell : UITableViewCell {
		public static readonly NSString Key = new NSString ("ProfielenTableViewCell");
		public static readonly UINib Nib;
		public Profiel Profiel { get; set; }

		BemCheckBox _checkBox;

		static ProfielenTableViewCell () {
			Nib = UINib.FromName ("ProfielenTableViewCell", NSBundle.MainBundle);
		}

		public ProfielenTableViewCell (IntPtr handle) : base (handle) {}

		public static ProfielenTableViewCell Create () {
			return (ProfielenTableViewCell)Nib.Instantiate (null, null) [0];
		}
			
		public void setData(bool check) {
			lblProfile.Text = Profiel.Name;
			if (check) {
				_checkBox = new BemCheckBox (new CoreGraphics.CGRect (0, 0, 20, 20), new MyBemCheckBoxDelegate(this));
				vwCheckbox.Add (_checkBox);
			}
		}

		public void toggleCheckbox() {
			_checkBox.SetOn (!_checkBox.On,true);
			Profiel.Selected = _checkBox.On;
		}

		class MyBemCheckBoxDelegate : BemCheckBoxDelegate {
			ProfielenTableViewCell cell;

			public MyBemCheckBoxDelegate(ProfielenTableViewCell cell) {
				this.cell = cell;
			}

			public override void DidTapCheckBox(bool checkBoxIsOn) {
				cell.Profiel.Selected = checkBoxIsOn;
			}
		}
	}
}