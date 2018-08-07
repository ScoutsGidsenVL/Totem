using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using System.Collections.Generic;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "Totems", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]			
	public class TotemsActivity : BaseActivity
    {
        private TotemAdapter totemAdapter;
        private ListView allTotemListView;
        private List<Totem> totemList;

        private EditText query;
        private TextView title;
        private ImageButton back;
        private ImageButton search;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AllTotems);

			//Action bar
			InitializeActionBar (SupportActionBar);
			title = ActionBarTitle;
			query = ActionBarQuery;
			search = ActionBarSearch;
			back = ActionBarBack;

			totemList = _appController.Totems;

			totemAdapter = new TotemAdapter (this, totemList);
			allTotemListView = FindViewById<ListView> (Resource.Id.all_totem_list);
			allTotemListView.Adapter = totemAdapter;
			allTotemListView.FastScrollEnabled = true;

			title.Text = "Totems";
			query.Hint = "Zoek totem";

			//hide keyboard when scrolling through list
			allTotemListView.SetOnTouchListener(new MyOnTouchListener(this, query));

			LiveSearch ();

			_appController.detailMode = AppController.DetailMode.NORMAL;

			allTotemListView.ItemClick += ShowDetail;

			search.Visibility = ViewStates.Visible;
			search.Click += (sender, e) => ToggleSearch ();

			//hide keybaord when enter is pressed
			query.EditorAction += (sender, e) => {
			    if (e.ActionId == ImeAction.Search)
			    {
			        KeyboardHelper.HideKeyboard(this);
			    }
			    else
			    {
			        e.Handled = false;
			    }
			};
		}

		protected override void OnResume()
		{
			base.OnResume ();

			_appController.NavigationController.GotoTotemDetailEvent+= StartDetailActivity;
		}

		protected override void OnPause()
		{
			base.OnPause ();

			_appController.NavigationController.GotoTotemDetailEvent-= StartDetailActivity;
		}

		//toggles the search bar
        private void ToggleSearch()
        {
			if (query.Visibility == ViewStates.Visible)
			{
				HideSearch();
				search.SetImageResource (Resource.Drawable.ic_search_white_24dp);
			}
			else
			{
				back.Visibility = ViewStates.Gone;
				title.Visibility = ViewStates.Gone;
				query.Visibility = ViewStates.Visible;
				KeyboardHelper.ShowKeyboard (this, query);
				query.Text = "";
				query.RequestFocus ();
				search.SetImageResource (Resource.Drawable.ic_close_white_24dp);
			}
		}

		//hides the search bar
        private void HideSearch()
        {
			back.Visibility = ViewStates.Visible;
			title.Visibility = ViewStates.Visible;
			query.Visibility = ViewStates.Gone;
			KeyboardHelper.HideKeyboard(this, query);
			totemAdapter.UpdateData(_appController.Totems); 
			totemAdapter.NotifyDataSetChanged();
		}

		//update list after every keystroke
        private void LiveSearch()
        {
			query.AfterTextChanged += (sender, args) => Search();
		}

		//shows only totems that are searched
        private void Search()
        {
			totemList = _appController.FindTotemOpNaamOfSyn(query.Text);
			totemAdapter.UpdateData (totemList); 
			totemAdapter.NotifyDataSetChanged ();
            if (query.Length() > 0)
            {
                allTotemListView.SetSelection(0);
            }
        }

		//get DetailActivity of the totem that is clicked
		//ID is passed as parameter
        private void ShowDetail(object sender, AdapterView.ItemClickEventArgs e)
        {
			var pos = e.Position;
			var item = totemAdapter.GetItemAtPosition(pos);
			KeyboardHelper.HideKeyboard(this);

			_appController.TotemSelected(item.Nid);
		}

        private void StartDetailActivity()
        {
			var detailActivity = new Intent(this, typeof(TotemDetailActivity));
			StartActivity (detailActivity); 
		}
			
		//return to full list and empty search field when 'back' is pressed
		//this happens only when a search query is currently entered
		public override void OnBackPressed()
		{
		    if (query.Visibility == ViewStates.Visible)
		    {
		        HideSearch();
		    }
		    else
		    {
		        base.OnBackPressed();
		    }
		}
	}
}