using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;

namespace TotemAndroid
{
    [Activity (Label = "Totem bepalen", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
	public class TinderEigenschappenActivity : BaseActivity
    {
        private TextView adjectief;
        private int eigenschapCount = 0;

        private ISharedPreferences sharedPrefs;

        protected override void OnCreate (Bundle bundle)
        {
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Eigenschappen);

			//Action bar
			InitializeActionBar (SupportActionBar);

			ActionBarTitle.Text = "Eigenschappen";

			var jaKnop = FindViewById<Button> (Resource.Id.jaKnop);
			var neeKnop = FindViewById<Button> (Resource.Id.neeKnop);

			adjectief = FindViewById<TextView> (Resource.Id.eigenschap);

            sharedPrefs = GetSharedPreferences("data", FileCreationMode.Private);

            UpdateScreen ();

			jaKnop.Click += (sender, eventArgs) => Push(true);
			neeKnop.Click += (sender, eventArgs) => Push(false);

            //smaller font size for smaller screens
            //otherwise UI issue
            var disp = WindowManager.DefaultDisplay;
            var size = new Point();
            disp.GetSize(size);

            if (size.X < 4000)
            {
                var eig = FindViewById<TextView>(Resource.Id.eigenschap);
                eig.TextSize = 20;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            //save eigenschappenlist state in sharedprefs
            var editor = sharedPrefs.Edit();
            var ser = ServiceStack.Text.JsonSerializer.SerializeToString(_appController.Eigenschappen);
            editor.PutString("eigenschappen", ser);
            editor.Commit();
        }

        //show next eigenschap
        public void UpdateScreen()
        {
			if (eigenschapCount < 324)
			{
				adjectief.Text = _appController.Eigenschappen[eigenschapCount].Name;
			}
			else
			{
				var i = new Intent (this, typeof(EigenschappenActivity));
				i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
				_appController.FireSelectedEvent ();
				StartActivity (i);
			}
		}
			
		public void Push(bool choice)
		{
			_appController.Eigenschappen [eigenschapCount].Selected = choice;
			eigenschapCount++;
			UpdateScreen ();
		}
	}
}