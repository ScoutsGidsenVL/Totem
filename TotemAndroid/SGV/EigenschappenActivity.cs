using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using ServiceStack.Text;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "Eigenschappen", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateAlwaysHidden)]			
	public class EigenschappenActivity : BaseActivity
    {
        private EigenschapAdapter eigenschapAdapter;
        private ListView allEigenschappenListView;
        private List<Eigenschap> eigenschappenList;

        private Toast mToastShort;
        private Toast mToastLong;

        private RelativeLayout bottomBar;

        private EditText query;
        private TextView title;
        private ImageButton back;
        private ImageButton search;

        private IMenu menu;

        private ISharedPreferences sharedPrefs;

        private MyOnCheckBoxClickListener mListener;

        private bool fullList = true;
        private bool IsProfileNull;
        private Profiel currProfiel;

        private ProgressDialog progress;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.AllEigenschappen);

            //Action bar
            InitializeActionBar(SupportActionBar);
            title = ActionBarTitle;
            query = ActionBarQuery;
            search = ActionBarSearch;
            back = ActionBarBack;

            mToastShort = Toast.MakeText(this, "", ToastLength.Short);
            mToastLong = Toast.MakeText(this, "", ToastLength.Long);

            currProfiel = _appController.CurrentProfiel;
            IsProfileNull = (currProfiel == null);
            eigenschappenList = _appController.Eigenschappen;

            //listener to pass to EigenschapAdapter containing context
            mListener = new MyOnCheckBoxClickListener(this);

            //pass screen width to adapter
            var disp = WindowManager.DefaultDisplay;
            Point size = new Point();
            disp.GetSize(size);

            eigenschapAdapter = new EigenschapAdapter(this, _appController.Eigenschappen, mListener, size.X);
            allEigenschappenListView = FindViewById<ListView>(Resource.Id.all_eigenschappen_list);
            allEigenschappenListView.Adapter = eigenschapAdapter;

            title.Text = IsProfileNull ? "Eigenschappen" : "Selectie";
            query.Hint = "Zoek eigenschap";

            //hide keyboard when scrolling through list
            allEigenschappenListView.SetOnTouchListener(new MyOnTouchListener(this, query));

            //initialize progress dialog used when calculating totemlist
            progress = new ProgressDialog(this);
            progress.SetMessage("Totems zoeken...");
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);

            LiveSearch();

            sharedPrefs = GetSharedPreferences("data", FileCreationMode.Private);

            var vind = FindViewById<LinearLayout>(Resource.Id.vind);
            vind.Click += VindTotem;

            bottomBar = FindViewById<RelativeLayout>(Resource.Id.bottomBar);

            search.Visibility = ViewStates.Visible;
            search.Click += (sender, e) => ToggleSearch();

            //hide keyboard when enter is pressed
            query.EditorAction += (sender, e) => {
                if (e.ActionId == ImeAction.Search)
                    KeyboardHelper.HideKeyboard(this);
                else
                    e.Handled = false;
            };          
        }

        protected override void OnResume()
        {
			base.OnResume ();

			_appController.UpdateCounter += UpdateCounter;
			_appController.ShowSelected += ShowSelectedOnly;
			_appController.NavigationController.GotoTotemResultEvent+= StartResultTotemsActivity;

            IsProfileNull = currProfiel == null;

            string ser;
            ser = IsProfileNull ? sharedPrefs.GetString("eigenschappen", null) : _appController.GetSerFromProfile(currProfiel.Name);

            if (ser != null) {
                _appController.Eigenschappen = JsonSerializer.DeserializeFromString<List<Eigenschap>>(ser);
                eigenschapAdapter.UpdateData(_appController.Eigenschappen);
            }

            eigenschapAdapter.NotifyDataSetChanged ();
            HideSearch();

            //this needs a delay for some reason
            Task.Factory.StartNew(() => Thread.Sleep(50)).ContinueWith(t => {
				allEigenschappenListView.SetSelection (0);
			}, TaskScheduler.FromCurrentSynchronizationContext());

            _appController.FireUpdateEvent ();
		}

		protected override void OnPause()
		{
			base.OnPause ();

			_appController.UpdateCounter -= UpdateCounter;
			_appController.ShowSelected -= ShowSelectedOnly;
			_appController.NavigationController.GotoTotemResultEvent-= StartResultTotemsActivity;
            var ser = JsonSerializer.SerializeToString(_appController.Eigenschappen);

            if (IsProfileNull) {
                //save eigenschappenlist state in sharedprefs
                var editor = sharedPrefs.Edit();
                editor.PutString("eigenschappen", ser);
                editor.Commit();
            } else {
                _appController.AddOrUpdateEigenschappenSer(currProfiel.Name, ser);
            }
		}

        private void UpdateCounter()
		{
			var count = _appController.Eigenschappen.FindAll (x => x.Selected).Count;
			var tvNumberSelected = FindViewById<TextView> (Resource.Id.selected);
			tvNumberSelected.Text = count + " geselecteerd";
			bottomBar.Visibility = count > 0 ? ViewStates.Visible : ViewStates.Gone;
		}

		//toggles the search bar
        private void ToggleSearch()
		{
			if (query.Visibility == ViewStates.Visible)
			{
				HideSearch();
			} else
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
            search.SetImageResource(Resource.Drawable.ic_search_white_24dp);
            KeyboardHelper.HideKeyboard (this, query);
            eigenschapAdapter.UpdateData (_appController.Eigenschappen);
			eigenschapAdapter.NotifyDataSetChanged ();
			query.Text = "";
			fullList = true;
		    if (menu != null)
		    {
		        UpdateOptionsMenu();
		    }
		}

		//update list after every keystroke
        private void LiveSearch() {
			query.AfterTextChanged += (sender, args) => {
				Search();
			    if (query.Text.Equals(""))
			    {
			        fullList = true;
			    }
			};
		}

		//shows only totems that match the query
        private void Search()
		{
			fullList = false;
			eigenschappenList = _appController.FindEigenschapOpNaam(query.Text);
			eigenschapAdapter.UpdateData (eigenschappenList);
			eigenschapAdapter.NotifyDataSetChanged ();
		    if (query.Length() > 0)
		    {
		        allEigenschappenListView.SetSelection(0);
		    }
		}

		//adds loading dialog and calculates totemlist
        private void VindTotem(object sender, EventArgs e)
		{
            //show progress dialog on UI thread
            RunOnUiThread(progress.Show);

            //calculate list on different thread and dismiss loading dialog afterwards
            new Thread(new ThreadStart(delegate {
                _appController.CalculateResultlist(_appController.Eigenschappen);
                RunOnUiThread(() => progress.Dismiss());
            })).Start();
        }

        private void StartResultTotemsActivity()
		{
			var totemsActivity = new Intent (this, typeof(ResultTotemsActivity));
			StartActivity (totemsActivity);
		}

		//create options menu
		public override bool OnCreateOptionsMenu(IMenu m)
		{
			menu = m;
			MenuInflater.Inflate(Resource.Menu.EigenschapSelectieMenu, menu);
			var item = menu.FindItem (Resource.Id.full);
			item.SetVisible (false);
            if(!IsProfileNull)
            {
                var save = menu.FindItem(Resource.Id.saveSelection);
                save.SetVisible(false);
            }

			return base.OnCreateOptionsMenu(menu);
		}

		//options menu: add profile, view selection of view full list
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			    //reset selection
			    case Resource.Id.reset:
                    query.Text = "";
				    fullList = true;
			        foreach (var eigenschap in eigenschappenList)
			        {
			            eigenschap.Selected = false;
			        }

			        eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				    eigenschapAdapter.NotifyDataSetChanged ();
                    UpdateOptionsMenu ();
                
                    //this needs a delay for some reason
                    Task.Factory.StartNew(() => Thread.Sleep(50)).ContinueWith(t => {
                        allEigenschappenListView.SetSelection(0);
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                    return true;
			
			    //show selected only
			    case Resource.Id.select:
				    ShowSelectedOnly ();
				    return true;

			    //show full list
			    case Resource.Id.full:
				    query.Text = "";
				    fullList = true;
				    UpdateOptionsMenu ();
				    eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				    eigenschapAdapter.NotifyDataSetChanged ();
				    return true;

			    //show full list
			    case Resource.Id.tinderView:
				    var totemsActivity = new Intent (this, typeof(TinderEigenschappenActivity));
				    StartActivity (totemsActivity);
				    return true;

                //show full list
                case Resource.Id.saveSelection:
                    ProfilePopup();
                    return true;
            }

			return base.OnOptionsItemSelected(item);
		}

        private void ProfilePopup()
        {
            var menu = new PopupMenu(this, search);
            menu.Inflate(Resource.Menu.Popup);
            var count = 0;
            foreach (var profiel in _appController.DistinctProfielen) {
                menu.Menu.Add(0, count, count, profiel.Name);
                count++;
            }

            menu.Menu.Add(0, count, count, "Nieuw profiel");

            menu.MenuItemClick += (s1, arg1) => {
                if (arg1.Item.ItemId == count) {
                    var alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Nieuw profiel");
                    var input = new EditText(this)
                    {
                        InputType = InputTypes.TextFlagCapWords,
                        Hint = "Naam"
                    };
                    KeyboardHelper.ShowKeyboard(this, input);
                    alert.SetView(input);
                    alert.SetPositiveButton("Ok", (s, args) => {
                        string value = input.Text;
                        if (value.Replace("'", "").Replace(" ", "").Equals(""))
                        {
                            mToastShort.SetText("Ongeldige naam");
                            mToastShort.Show();
                        }
                        else if (_appController.GetProfielNamen().Contains(value))
                        {
                            input.Text = "";
                            mToastShort.SetText("Profiel " + value + " bestaat al");
                            mToastShort.Show();
                        }
                        else
                        {
                            _appController.AddProfile(value);
                            _appController.AddOrUpdateEigenschappenSer(value, JsonSerializer.SerializeToString(_appController.Eigenschappen));
                            mToastShort.SetText("Selectie opgeslagen voor profiel " + value);
                            mToastShort.Show();
                        }
                    });

                    var d1 = alert.Create();

                    //add profile when enter is clicked
                    input.EditorAction += (s2, e) => {
                        if (e.ActionId == ImeAction.Done)
                        {
                            d1.GetButton(-1).PerformClick();
                        }
                        else
                        {
                            e.Handled = false;
                        }
                    };

                    RunOnUiThread(d1.Show);

                } else {
                    _appController.AddOrUpdateEigenschappenSer(arg1.Item.TitleFormatted.ToString(), JsonSerializer.SerializeToString(_appController.Eigenschappen));
                    mToastShort.SetText("Selectie opgeslagen voor profiel " + arg1.Item.TitleFormatted);
                    mToastShort.Show();
                }
            };

            menu.Show();
        }

        private void ShowSelectedOnly()
        {
			var list = GetSelectedEigenschappen();
			if (list.Count == 0)
			{
				mToastShort.SetText("Geen eigenschappen geselecteerd");
				mToastShort.Show();
			}
			else
			{
				fullList = false;
				UpdateOptionsMenu();
				eigenschapAdapter.UpdateData(list);
				eigenschapAdapter.NotifyDataSetChanged();
				bottomBar.Visibility = ViewStates.Visible;
			}
		}

		//changes the options menu items according to list
		//delay of 0.5 seconds to take animation into account
        private void UpdateOptionsMenu()
        {
			var select = menu.FindItem(Resource.Id.select);
			var full = menu.FindItem(Resource.Id.full);

			Task.Factory.StartNew(() => Thread.Sleep(500)).ContinueWith(t => {
				if (fullList)
				{
					select.SetVisible(true);
					full.SetVisible(false);
				}
				else
				{
					select.SetVisible(false);
					full.SetVisible(true);
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		//returns list of eigenschappen that have been checked
        private List<Eigenschap> GetSelectedEigenschappen()
        {
			var result = new List<Eigenschap> ();
            foreach (var eigenschap in _appController.Eigenschappen)
            {
                if (eigenschap.Selected)
                {
                    result.Add(eigenschap);
                }
            }

            return result;
		}

		//return to full list and empty search field when 'back' is pressed
		//this happens only when a search query is currently entered
		public override void OnBackPressed()
		{
			if (query.Visibility == ViewStates.Visible)
			{
				HideSearch ();
			}
			else if (!fullList)
			{
				query.Text = "";
				fullList = true;
				UpdateOptionsMenu ();
				eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				eigenschapAdapter.NotifyDataSetChanged ();
			}
			else
			{
				base.OnBackPressed ();
			}
		}
	}
}