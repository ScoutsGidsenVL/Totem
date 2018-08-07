using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using ServiceStack.Text;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TotemAndroid
{
	[Activity (Label = "Checklist")]			
	public class TotemisatieChecklistActivity : BaseActivity
	{
	    private Dictionary<string, List<string>> dictGroup;
	    private ExpandableListView expand;
	    private ExpendListAdapter expandAdapater;

	    private ISharedPreferences sharedPrefs;

	    private ImageButton delete;

        protected override void OnCreate (Bundle savedInstanceState) {
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Checklist);

            //Action bar
			InitializeActionBar (SupportActionBar);
            delete = ActionBarDelete;

            ActionBarTitle.Text = "Totemisatie checklist";

            sharedPrefs = GetSharedPreferences("checklist", FileCreationMode.Private);
            var ser = sharedPrefs.GetString("states", "empty");
            List<List<bool>> states;
            states = !ser.Equals("empty") ? JsonSerializer.DeserializeFromString<List<List<bool>>>(ser) : null;

            delete.SetImageResource(Resource.Drawable.ic_reset_white_24dp);
            delete.Click += ResetChecklist;
            delete.Visibility = ViewStates.Visible;

            expand = FindViewById<ExpandableListView>(Resource.Id.expand);

            InitializeExpandableListView ();
            expandAdapater = new ExpendListAdapter(this, dictGroup, states);
            expand.SetAdapter(expandAdapater);
        }

        protected override void OnPause()
        {
            base.OnPause();

            //save eigenschappenlist state in sharedprefs
            var editor = sharedPrefs.Edit();
            var ser = JsonSerializer.SerializeToString(expandAdapater.checkedStates);
            editor.PutString("states", ser);
            editor.Commit();
        }

	    private void ResetChecklist(object sender, EventArgs e)
	    {
            var alert = new AlertDialog.Builder(this);
            alert.SetMessage("Checklist resetten?");
            alert.SetPositiveButton("Ja", (senderAlert, args) => {
                expandAdapater.ResetChecklist();
                expandAdapater.NotifyDataSetChanged();
            });

            alert.SetNegativeButton("Nee", (senderAlert, args) => { });

            Dialog dialog = alert.Create();
            RunOnUiThread(dialog.Show);
        }

        //adds header, footer and data to the ExpandableListView
        private void InitializeExpandableListView() {
			dictGroup = new Dictionary<string, List<string>>();
			FillDictGroup ();

			var view = View.Inflate (this, Resource.Layout.ExpandHeadFoot, null);
			view.FindViewById<TextView> (Resource.Id.intro).Text = Resources.GetString(Resource.String.checklist_head);
			expand.AddHeaderView (view, null, false);
			view = View.Inflate (this, Resource.Layout.ExpandHeadFoot, null);
			view.FindViewById<TextView> (Resource.Id.intro).Text = Resources.GetString(Resource.String.checklist_foot);;
			expand.AddFooterView (view, null, false);

			expand.DividerHeight = 0;
		}

		//stores the data from arrays.xml in dictgroup per section
		private void FillDictGroup()
		{
			var voorbereiding = Resources.GetStringArray (Resource.Array.voorbereiding).ToList();
			var totemopdrachten = Resources.GetStringArray (Resource.Array.totemopdrachten).ToList();
			var geven = Resources.GetStringArray (Resource.Array.geven).ToList();
			var bezinning = Resources.GetStringArray (Resource.Array.bezinning).ToList();

			dictGroup.Add ("Voorbereiding", voorbereiding);
			dictGroup.Add ("Totemopdrachten", totemopdrachten);
			dictGroup.Add ("Geven van een totem", geven);
			dictGroup.Add ("Bezinning", bezinning);
		}
	}
}