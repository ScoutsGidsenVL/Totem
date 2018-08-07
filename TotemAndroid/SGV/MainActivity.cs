using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace TotemAndroid
{
    [Activity (Label = "Totemapp", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppThemeNoAction")]
	public class MainActivity : BaseActivity
    {
		//Database db;
        private Button totems;
        private Button eigenschappen;
        private Button profielen;
        private Button checklist;

        private Toast mToast;
        private View toastView;

        protected override void OnCreate(Bundle bundle)
        {
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

			totems = FindViewById<Button>(Resource.Id.totems);
			eigenschappen = FindViewById<Button>(Resource.Id.eigenschappen);
			profielen = FindViewById<Button>(Resource.Id.profielen);
			checklist = FindViewById<Button>(Resource.Id.checklist);

			totems.Click += (sender, eventArgs) => _appController.TotemMenuItemClicked();
			eigenschappen.Click += (sender, eventArgs) => _appController.EigenschappenMenuItemClicked();
			profielen.Click += (sender, eventArgs) => _appController.ProfileMenuItemClicked();
			checklist.Click += (sender, eventArgs) => _appController.ChecklistMenuItemClicked();

            var title = FindViewById<TextView>(Resource.Id.totemapp_title);
            title.LongClick += ShowEasterEgg;

            var tip = FindViewById<ImageButton>(Resource.Id.tst);
            tip.Click += (sender, e) => ShowTipDialog();

            var mInflater = LayoutInflater.From(this);
            toastView = mInflater.Inflate(Resource.Layout.InfoToast, null);

            //smaller font size for smaller screens
            //otherwise UI issue
            var disp = WindowManager.DefaultDisplay;
            var size = new Point();
            disp.GetSize(size);

            if (size.X <= 480)
            {
                title.TextSize = 60;
            }
        }

        protected override void OnResume()
        {
			base.OnResume ();

            _appController.NavigationController.GotoTotemListEvent += GotoTotemListHandler;
			_appController.NavigationController.GotoEigenschapListEvent += GotoEigenschappenListHandler;
			_appController.NavigationController.GotoProfileListEvent += GotoProfileListHandler;
			_appController.NavigationController.GotoChecklistEvent += GotoChecklistHandler;
		}

		protected override void OnPause()
		{
			base.OnPause();
            toastView.Visibility = ViewStates.Gone;

            _appController.NavigationController.GotoTotemListEvent -= GotoTotemListHandler;
			_appController.NavigationController.GotoEigenschapListEvent -= GotoEigenschappenListHandler;
			_appController.NavigationController.GotoProfileListEvent -= GotoProfileListHandler;
			_appController.NavigationController.GotoChecklistEvent -= GotoChecklistHandler;
		}

        private void ShowEasterEgg(object sender, View.LongClickEventArgs e)
        {
            mToast = new Toast(this)
            {
                Duration = ToastLength.Short
            };
            mToast.SetGravity(GravityFlags.Center | GravityFlags.Bottom, 0, ConvertDpToPixels(10));

            toastView.Visibility = ViewStates.Visible;
            mToast.View = toastView;

            //smaller font size for smaller screens
            //otherwise UI issue
            var disp = WindowManager.DefaultDisplay;
            var size = new Point();
            disp.GetSize(size);

            if (size.X <= 480) {
                var t = mToast.View.FindViewById<TextView>(Resource.Id.info);
                t.TextSize = 10;
            }

            mToast.Show();
        }

        private int ConvertDpToPixels(float dp)
        {
            var scale = Resources.DisplayMetrics.Density;
            var result = (int)(dp * scale + 0.5f);

            return result;
        }

        private void GoToActivity(string activity)
        {
			Intent intent = null;
			switch (activity)
			{
			    case "totems":
				    intent = new Intent(this, typeof(TotemsActivity));
				    break;
			    case "eigenschappen":
				    intent = new Intent(this, typeof(EigenschappenActivity));
				    break;
			    case "profielen":
				    intent = new Intent(this, typeof(ProfielenActivity));
				    break;
			    case "checklist":
				    intent = new Intent(this, typeof(TotemisatieChecklistActivity));
				    break;
			}

			StartActivity(intent);
		}

		public void ShowTipDialog()
		{
			var dialog = TipDialog.NewInstance(this);
			RunOnUiThread(() => dialog.Show (FragmentManager, "dialog"));
		}

		public override void OnBackPressed()
		{
			var startMain = new Intent(Intent.ActionMain);
			startMain.AddCategory(Intent.CategoryHome);
			startMain.SetFlags(ActivityFlags.NewTask);
			StartActivity(startMain);
		}

        private void GotoTotemListHandler()
        {
			GoToActivity ("totems");
		}

        private void GotoEigenschappenListHandler()
        {
			GoToActivity("eigenschappen");
		}

        private void GotoProfileListHandler()
        {
			GoToActivity("profielen");
		}

        private void GotoChecklistHandler()
        {
			GoToActivity("checklist");
		}
	}
}