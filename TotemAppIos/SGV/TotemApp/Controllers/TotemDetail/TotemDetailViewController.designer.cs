// WARNING
//
// This file has been generated automatically by Xamarin Studio Community to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace TotemAppIos
{
	[Register ("TotemDetailViewController")]
	partial class TotemDetailViewController
	{
		[Outlet]
		UIKit.UIButton btnAction { get; set; }

		[Outlet]
		UIKit.UIButton btnHidden { get; set; }

		[Outlet]
		UIKit.UIButton btnReturn { get; set; }

		[Outlet]
		UIKit.UIImageView imgAction { get; set; }

		[Outlet]
		UIKit.UIImageView imgHidden { get; set; }

		[Outlet]
		UIKit.UIImageView imgLine { get; set; }

		[Outlet]
		UIKit.UIImageView imgReturn { get; set; }

		[Outlet]
		UIKit.UILabel lblBody { get; set; }

		[Outlet]
		UIKit.UILabel lblHead { get; set; }

		[Outlet]
		UIKit.UILabel lblNumber { get; set; }

		[Outlet]
		UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint paddingHeight { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnAction != null) {
				btnAction.Dispose ();
				btnAction = null;
			}

			if (btnHidden != null) {
				btnHidden.Dispose ();
				btnHidden = null;
			}

			if (btnReturn != null) {
				btnReturn.Dispose ();
				btnReturn = null;
			}

			if (imgAction != null) {
				imgAction.Dispose ();
				imgAction = null;
			}

			if (imgHidden != null) {
				imgHidden.Dispose ();
				imgHidden = null;
			}

			if (imgLine != null) {
				imgLine.Dispose ();
				imgLine = null;
			}

			if (imgReturn != null) {
				imgReturn.Dispose ();
				imgReturn = null;
			}

			if (lblBody != null) {
				lblBody.Dispose ();
				lblBody = null;
			}

			if (lblHead != null) {
				lblHead.Dispose ();
				lblHead = null;
			}

			if (lblNumber != null) {
				lblNumber.Dispose ();
				lblNumber = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (paddingHeight != null) {
				paddingHeight.Dispose ();
				paddingHeight = null;
			}
		}
	}
}
