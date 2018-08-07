using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "BaseActivity")]			
	public abstract class BaseActivity : AppCompatActivity
    {

		protected TextView ActionBarTitle { get; set; }
		protected ImageButton ActionBarBack { get; set; }
		protected ImageButton ActionBarAdd { get; set; }
		protected ImageButton ActionBarSearch { get; set; }
		protected ImageButton ActionBarClose { get; set; }
		protected ImageButton ActionBarDelete { get; set; }
		protected EditText ActionBarQuery { get; set; }
		protected Android.Support.V7.App.ActionBar mActionBar { get; set; }

		protected AppController _appController = AppController.Instance;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);
			RequestedOrientation = ScreenOrientation.SensorPortrait;
		}

		protected void InitializeActionBar(Android.Support.V7.App.ActionBar ab)
		{
			mActionBar = ab;

			var mInflater = LayoutInflater.From (this);
			var mCustomView = mInflater.Inflate (Resource.Layout.ActionBar, null);

			ActionBarTitle = mCustomView.FindViewById<TextView> (Resource.Id.title);
			ActionBarBack = mCustomView.FindViewById<ImageButton> (Resource.Id.backButton);
			ActionBarAdd = mCustomView.FindViewById<ImageButton> (Resource.Id.addButton);
			ActionBarSearch = mCustomView.FindViewById<ImageButton> (Resource.Id.searchButton);
			ActionBarClose = mCustomView.FindViewById<ImageButton> (Resource.Id.closeButton);
			ActionBarDelete = mCustomView.FindViewById<ImageButton> (Resource.Id.deleteButton);
			ActionBarQuery = mCustomView.FindViewById<EditText>(Resource.Id.query);

			ActionBarBack.Click += (sender, e) => OnBackPressed ();

			var layout = new Android.Support.V7.App.ActionBar.LayoutParams (ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

			mActionBar.SetCustomView (mCustomView, layout);
			mActionBar.SetDisplayShowCustomEnabled (true);
		}
	}
}