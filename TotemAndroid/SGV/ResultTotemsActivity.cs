using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using System.Collections.Generic;
using System.Linq;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "Totems")]			
	public class ResultTotemsActivity : BaseActivity
    {
        private TotemAdapter totemAdapter;
        private ListView totemListView;
        private List<Totem> totemList;

        private TextView title;
        private ImageButton back;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Totems);

			//Action bar
			InitializeActionBar (SupportActionBar);
			title = ActionBarTitle;
			back = ActionBarBack;
	
			int selected = Intent.GetIntExtra ("selected", 0);

			totemList = _appController.TotemEigenschapDict.Keys.ToList();
			var freqs = _appController.TotemEigenschapDict.Values.ToArray ();

			totemAdapter = new TotemAdapter (this, totemList, freqs, selected);
			totemListView = FindViewById<ListView> (Resource.Id.totem_list);
			totemListView.Adapter = totemAdapter;

			totemListView.ItemClick += ShowDetail;

			title.Text = "Totems";
		}

		protected override void OnResume()
		{
			base.OnResume();

			_appController.NavigationController.GotoTotemDetailEvent += StartDetailActivity;
		}

		protected override void OnPause()
		{
			base.OnPause ();

			_appController.NavigationController.GotoTotemDetailEvent -= StartDetailActivity;
		}

		//fill totemList with Totem-objects whose ID is in totemIDs
        private List<Totem> ConvertIdArrayToTotemList(int[] totemIDs)
        {
            return totemIDs.Select(idx => _appController.GetTotemOnId(idx)).ToList();
        }

        private void ShowDetail(object sender, AdapterView.ItemClickEventArgs e)
        {
			var pos = e.Position;
			var item = totemAdapter.GetItemAtPosition(pos);

			_appController.TotemSelected(item.Nid);
		}

        private void StartDetailActivity()
        {
			var detailActivity = new Intent(this, typeof(TotemDetailActivity));
			StartActivity(detailActivity); 
		}

		//goes back to main screen when GoToMain is set to true
		//otherwise acts normal
		public override void OnBackPressed()
		{
			if (Intent.GetBooleanExtra ("GoToMain", false)) {
				var i = new Intent (this, typeof(MainActivity));
				i.SetFlags (ActivityFlags.ClearTop | ActivityFlags.SingleTop);
				StartActivity (i);
			}
			else
			{
				base.OnBackPressed ();
			}
		}
	}
}