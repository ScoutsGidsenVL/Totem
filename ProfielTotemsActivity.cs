﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;

using Java.Lang;

namespace Totem {
	[Activity (Label = "Totems")]
	public class ProfielTotemsActivity : Activity {
		TotemAdapter totemAdapter;
		ListView allTotemListView;
		List<Totem> totemList;

		TextView title;
		ImageButton back;
		ImageButton delete;
		ImageButton close;

		Database db;
		Toast mToast;

		string profileName;

		protected override void OnCreate (Bundle bundle) {
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AllTotems);

			ActionBar mActionBar = ActionBar;

			LayoutInflater mInflater = LayoutInflater.From (this);
			View mCustomView = mInflater.Inflate (Resource.Layout.ActionBar, null);

			db = DatabaseHelper.GetInstance (this);

			//single toast for entire activity
			mToast = Toast.MakeText (this, "", ToastLength.Short);

			profileName = Intent.GetStringExtra ("profileName");
			totemList = db.GetTotemsFromProfiel (profileName);

			totemAdapter = new TotemAdapter (this, totemList);
			allTotemListView = FindViewById<ListView> (Resource.Id.all_totem_list);
			allTotemListView.Adapter = totemAdapter;

			allTotemListView.ItemClick += ShowDetail;
			allTotemListView.ItemLongClick += DeleteTotem;

			title = mCustomView.FindViewById<TextView> (Resource.Id.title);
			title.Text = "Totems voor " + profileName;

			back = mCustomView.FindViewById<ImageButton> (Resource.Id.backButton);
			back.Click += (object sender, EventArgs e) => OnBackPressed();

			close = mCustomView.FindViewById<ImageButton> (Resource.Id.closeButton);
			close.Click += HideDeleteTotems;

			ImageButton search = mCustomView.FindViewById<ImageButton> (Resource.Id.searchButton);
			search.Visibility = ViewStates.Gone;

			delete = mCustomView.FindViewById<ImageButton> (Resource.Id.deleteButton);
			delete.Visibility = ViewStates.Visible;
			delete.Click += ShowDeleteTotems;

			var layout = new ActionBar.LayoutParams (WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent);

			mActionBar.SetCustomView (mCustomView, layout);
			mActionBar.SetDisplayShowCustomEnabled (true);
		}

		//update data from adapter on restart
		protected override void OnRestart() {
			base.OnRestart ();
			totemList = db.GetTotemsFromProfiel (profileName);
			totemAdapter.UpdateData (totemList);
			totemAdapter.NotifyDataSetChanged ();
		}

		//get DetailActivity of the totem that is clicked
		//ID is passed as parameter
		private void ShowDetail(object sender, AdapterView.ItemClickEventArgs e) {
			int pos = e.Position;
			var item = totemAdapter.GetItemAtPosition(pos);

			var detailActivity = new Intent(this, typeof(TotemDetailActivity));
			detailActivity.PutExtra ("totemID", item.nid);
			detailActivity.PutExtra ("profileName", profileName);
			StartActivity (detailActivity);
		}

		private void DeleteTotem(object sender, AdapterView.ItemLongClickEventArgs e) {
			int pos = e.Position;
			var item = totemAdapter.GetItemAtPosition(pos);

			AlertDialog.Builder alert = new AlertDialog.Builder (this);
			alert.SetMessage (item.title + " verwijderen uit profiel " + profileName + "?");
			alert.SetPositiveButton ("Ja", (senderAlert, args) => {
				db.DeleteTotemFromProfile(item.nid, profileName);
				mToast.SetText(item.title + " verwijderd");
				mToast.Show();
				totemList = db.GetTotemsFromProfiel(profileName);
				if(totemList.Count == 0) {
					base.OnBackPressed();
				} else {
					totemAdapter.UpdateData (totemList);
					totemAdapter.NotifyDataSetChanged ();
				}
			});

			alert.SetNegativeButton ("Nee", (senderAlert, args) => {});

			Dialog dialog = alert.Create();
			RunOnUiThread (() => {
				dialog.Show();
			} );
		}

		//also sets all the checkboxes unchecked
		private void ShowDeleteTotems(object sender, EventArgs e) {
			foreach (Totem t in totemList)
				t.selected = false;

			totemAdapter.ShowDelete ();
			totemAdapter.UpdateData (totemList);
			totemAdapter.NotifyDataSetChanged ();

			back.Visibility = ViewStates.Gone;
			close.Visibility = ViewStates.Visible;
			title.Visibility = ViewStates.Gone;

			delete.Click -= ShowDeleteTotems;
			delete.Click += RemoveSelectedTotems;
		}

		private void HideDeleteTotems(object sender, EventArgs e) {
			totemAdapter.HideDelete ();
			totemAdapter.NotifyDataSetChanged ();

			back.Visibility = ViewStates.Visible;
			close.Visibility = ViewStates.Gone;
			title.Visibility = ViewStates.Visible;

			delete.Click -= RemoveSelectedTotems;
			delete.Click += ShowDeleteTotems;
		}

		private void RemoveSelectedTotems(object sender, EventArgs e) {
			bool selected = false;
			foreach(Totem t in totemList) {
				if (t.selected) {
					selected = true;
					break;
				}
			}
			if (selected) {
				AlertDialog.Builder alert1 = new AlertDialog.Builder (this);
				alert1.SetMessage ("Geselecteerde totems verwijderen?");
				alert1.SetPositiveButton ("Ja", (senderAlert, args) => {
					foreach (Totem t in totemList)
						if (t.selected)
							db.DeleteTotemFromProfile (t.nid, profileName);
				
					totemList = db.GetTotemsFromProfiel (profileName);
					if (totemList.Count == 0) {
						base.OnBackPressed ();
					} else {
						totemAdapter.UpdateData (totemList);
						totemAdapter.NotifyDataSetChanged ();
						HideDeleteTotems (sender, e);
					}
				});

				alert1.SetNegativeButton ("Nee", (senderAlert, args) => {
				});

				Dialog d2 = alert1.Create ();

				RunOnUiThread (() => {
					d2.Show ();
				});
			} else {
				mToast.SetText("Geen totems geselecteerd om te verwijderen");
				mToast.Show();
			}
		}
	}
}