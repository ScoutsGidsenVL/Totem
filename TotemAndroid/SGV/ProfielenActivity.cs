using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using System;
using System.Collections.Generic;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "Profielen", WindowSoftInputMode=SoftInput.StateAlwaysHidden, LaunchMode=LaunchMode.SingleTask)]			
	public class ProfielenActivity : BaseActivity
    {
        private ProfielAdapter profielAdapter;
        private ListView profielenListView;
        private List<Profiel> profielen;

        private TextView title;
        private ImageButton back;
        private ImageButton close;
        private ImageButton add;
        private ImageButton delete;
        private TextView noProfiles;
        private Toast mToast;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Profielen);

			//Action bar
			InitializeActionBar (SupportActionBar);
			title = ActionBarTitle;
			close = ActionBarClose;
			back = ActionBarBack;
			add = ActionBarAdd;
			delete = ActionBarDelete;

			//single toast for entire activity
			mToast = Toast.MakeText (this, "", ToastLength.Short);

			profielen = _appController.DistinctProfielen;
			
			profielAdapter = new ProfielAdapter (this, profielen);
			profielenListView = FindViewById<ListView> (Resource.Id.profielen_list);
			profielenListView.Adapter = profielAdapter;

			profielenListView.ItemClick += ShowTotems;
			profielenListView.ItemLongClick += DeleteProfile;

			noProfiles = FindViewById<TextView> (Resource.Id.empty_profiel);

			title.Text = "Profielen";

			add.Visibility = ViewStates.Visible;
			add.Click += (sender, e) => AddProfile ();

			delete.Click += ShowDeleteProfiles;
			close.Click += HideDeleteProfiles;

			if (profielen.Count == 0)
			{
				noProfiles.Visibility = ViewStates.Visible;
				delete.Visibility = ViewStates.Gone;
			}
			else
			{
				delete.Visibility = ViewStates.Visible;
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			_appController.NavigationController.GotoProfileTotemListEvent+= StartTotemProfileActivity;
		}

		protected override void OnPause()
        {
			base.OnPause();

			_appController.NavigationController.GotoProfileTotemListEvent-= StartTotemProfileActivity;
		}

        private void StartTotemProfileActivity()
		{
			var i = new Intent(this, typeof(ProfielTotemsActivity));
			StartActivity(i);
		}

		//updates data of the adapter and shows/hides the "empty"-message when needed
        private void UpdateList()
        {
			profielen = _appController.DistinctProfielen;
			if (profielen.Count == 0)
			{
				noProfiles.Visibility = ViewStates.Visible;
				delete.Visibility = ViewStates.Gone;
			}
			else
			{
				noProfiles.Visibility = ViewStates.Gone;
				delete.Visibility = ViewStates.Visible;
			}

			profielAdapter.UpdateData(profielen);
			profielAdapter.NotifyDataSetChanged();
		}

        private void ShowTotems(object sender, AdapterView.ItemClickEventArgs e)
        {
			var pos = e.Position;
			var item = profielAdapter.GetItemAtPosition(pos);

			_appController.ProfileSelected(item.Name);
		}

        private void DeleteProfile(object sender, AdapterView.ItemLongClickEventArgs e)
        {
			var pos = e.Position;
			var item = profielAdapter.GetItemAtPosition(pos);

			var alert = new AlertDialog.Builder(this);
			alert.SetMessage("Profiel " + item.Name + " verwijderen?");
			alert.SetPositiveButton ("Ja", (senderAlert, args) => {
				_appController.DeleteProfile(item.Name);
				mToast.SetText("Profiel " + item.Name + " verwijderd");
				mToast.Show();
				UpdateList();
			});

			alert.SetNegativeButton("Nee", (senderAlert, args) => {});

			Dialog dialog = alert.Create();
			RunOnUiThread(dialog.Show);
		}

        private void AddProfile()
        {
			var alert = new AlertDialog.Builder (this);
			alert.SetTitle ("Nieuw profiel");
            var input = new EditText(this)
            {
                InputType = Android.Text.InputTypes.TextFlagCapWords,
                Hint = "Naam"
            };
            KeyboardHelper.ShowKeyboard (this, input);
			alert.SetView (input);
			alert.SetPositiveButton ("Ok", (sender, args) => {
				var value = input.Text;
				if (value.Replace("'", "").Replace(" ", "").Equals(""))
				{
					mToast.SetText("Ongeldige naam");
					mToast.Show();				
				}
				else if (_appController.GetProfielNamen().Contains(value))
				{
					input.Text = "";
					mToast.SetText("Profiel " + value + " bestaat al");
					mToast.Show();
				}
				else
				{
					_appController.AddProfile(value);
					UpdateList();
				}
			});

			var d1 = alert.Create();

			//add profile when enter is clicked
			input.EditorAction += (sender, e) => {
			    if (e.ActionId == ImeAction.Done)
			    {
			        d1.GetButton(-1).PerformClick();
			    }
			    else
			    {
			        e.Handled = false;
			    }
			};

			RunOnUiThread (d1.Show);
		}

        private void ShowDeleteProfiles(object sender, EventArgs e)
        {
			profielAdapter.ShowDelete ();
			profielAdapter.NotifyDataSetChanged ();

			back.Visibility = ViewStates.Gone;
			close.Visibility = ViewStates.Visible;
			title.Visibility = ViewStates.Gone;
			add.Visibility = ViewStates.Gone;

			delete.Click -= ShowDeleteProfiles;
			delete.Click += RemoveSelectedProfiles;
		}

        private void HideDeleteProfiles(object sender, EventArgs e)
        {
			profielAdapter.HideDelete ();
			profielAdapter.NotifyDataSetChanged ();

			back.Visibility = ViewStates.Visible;
			close.Visibility = ViewStates.Gone;
			title.Visibility = ViewStates.Visible;
			add.Visibility = ViewStates.Visible;

			delete.Click -= RemoveSelectedProfiles;
			delete.Click += ShowDeleteProfiles;
		}

        private void RemoveSelectedProfiles(object sender, EventArgs e)
        {
			var selected = false;
			foreach (var profiel in profielen)
			{
				if (profiel.Selected)
				{
					selected = true;
					break;
				}
			}

			if (selected)
			{		
				var alert1 = new AlertDialog.Builder (this);
				alert1.SetMessage("Geselecteerde profielen verwijderen?");
				alert1.SetPositiveButton("Ja", (senderAlert, args) => {
				    foreach (var profiel in profielen)
				    {
				        if (profiel.Selected)
				        {
				            _appController.DeleteProfile(profiel.Name);
				        }
				    }

				    UpdateList ();
					HideDeleteProfiles (sender, e);
				});

				alert1.SetNegativeButton("Nee", (senderAlert, args) => {});

				Dialog d2 = alert1.Create();

				RunOnUiThread(d2.Show);
			}
			else
			{
				mToast.SetText("Geen profielen geselecteerd om te verwijderen");
				mToast.Show();
			}
		}
	}
}