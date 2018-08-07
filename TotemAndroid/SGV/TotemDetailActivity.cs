using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using System;

using TotemAppCore;

namespace TotemAndroid
{
    [Activity (Label = "Beschrijving", WindowSoftInputMode=SoftInput.StateAlwaysHidden)]			
	public class TotemDetailActivity : BaseActivity, GestureDetector.IOnGestureListener
    {
        private TextView number;
        private TextView title_synonyms;
        private TextView body;

        private Toast mToast;

        private TextView title;
        private ImageButton back;
        private ImageButton action;
        private ImageButton search;

        private bool hidden = false;

		//used for swiping
		public GestureDetector GestureDetector;
        private const int SwipeMinDistance = 120;
        private const int SwipeThresholdVelocity = 200;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.TotemDetail);

            //if else that prevents crash when app is killed
            if (_appController.CurrentTotem == null)
            {
                var i = new Intent(this, typeof(MainActivity));
                StartActivity(i);
                Finish();
            }
            else
            {
                //Action bar
                InitializeActionBar(SupportActionBar);
                title = ActionBarTitle;
                back = ActionBarBack;
                search = ActionBarSearch;

                //single toast for entire activity
                mToast = Toast.MakeText(this, "", ToastLength.Short);

                number = FindViewById<TextView>(Resource.Id.number);
                title_synonyms = FindViewById<TextView>(Resource.Id.title_synonyms);
                body = FindViewById<TextView>(Resource.Id.body);

                GestureDetector = new GestureDetector(this);

                title.Text = "Beschrijving";

                if (!_appController.ShowAdd)
                {
                    action = ActionBarDelete;
                    action.Click += (sender, e) => RemoveFromProfile(_appController.CurrentProfiel.Name);
                }
                else
                {
                    action = ActionBarAdd;
                    action.Click += (sender, e) => ProfilePopup();
                }

                action.Visibility = ViewStates.Visible;

                search.Visibility = ViewStates.Visible;
                search.SetImageResource(Resource.Drawable.ic_visibility_off_white_24dp);
                search.Click += (sender, e) => ToggleHidden();

                _appController.NavigationController.GotoProfileListEvent += StartProfielenActivity;

                SetInfo();
            }
		}

		//redirect touch event
		public override bool OnTouchEvent(MotionEvent e)
		{
			GestureDetector.OnTouchEvent(e);

			return false;
		}

		//detect left or right swipe and update info accordingly
		public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
		    //next
			if (e1.GetX () - e2.GetX () > SwipeMinDistance && Math.Abs (velocityX) > SwipeThresholdVelocity)
			{
				var next = _appController.NextTotem;
				if (next != null)
				{
					_appController.CurrentTotem = next;
					SetInfo ();
				}

				return true;
			//previous
			}

		    if (e2.GetX () - e1.GetX () > SwipeMinDistance && Math.Abs (velocityX) > SwipeThresholdVelocity) {
		        var prev = _appController.PrevTotem;
		        if (prev != null)
		        {
		            _appController.CurrentTotem = prev;
		            SetInfo ();
		        }

		        return true;
		    }

		    return false;
		}

		//GestureListener method
		//NOT USED
		public void OnLongPress(MotionEvent e) {}

		//GestureListener method
		//NOT USED
		public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			return false;
		}

		//GestureListener method
		//NOT USED
		public void OnShowPress(MotionEvent e) {}

		//GestureListener method
		//NOT USED
		public bool OnSingleTapUp(MotionEvent e)
		{
			return false;
		}

		//GestureListener method
		//NOT USED
		public bool OnDown(MotionEvent e)
		{
			return false;
		}

		//ensures swipe works on ScrollView
		public override bool DispatchTouchEvent(MotionEvent ev)
		{
			base.DispatchTouchEvent(ev);
			return GestureDetector.OnTouchEvent(ev);
		}

		protected override void OnResume()
		{
			base.OnResume ();
            SetInfo();
		}

        private void ToggleHidden()
        {
            hidden = !hidden;
            if (hidden)
            {
                search.SetImageResource(Resource.Drawable.ic_visibility_white_24dp);
                number.Visibility = ViewStates.Gone;
            }
            else
            {
                search.SetImageResource(Resource.Drawable.ic_visibility_off_white_24dp);
                number.Visibility = ViewStates.Visible;
            }

            SetInfo();
        }

        private void RemoveFromProfile(string profileName)
        {
			var alert = new AlertDialog.Builder(this);
			alert.SetMessage(_appController.CurrentTotem.Title + " verwijderen uit profiel " + profileName + "?");
			alert.SetPositiveButton("Ja", (senderAlert, args) => {
				_appController.DeleteTotemFromProfile(_appController.CurrentTotem.Nid, profileName);
				mToast.SetText(_appController.CurrentTotem.Title + " verwijderd");
				mToast.Show();
				OnBackPressed();
			});

			alert.SetNegativeButton ("Nee", (senderAlert, args) => {});

			Dialog dialog = alert.Create();
			RunOnUiThread (dialog.Show);
		}

        private void StartProfielenActivity()
        {
			var i = new Intent(this, typeof(ProfielenActivity));
			i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			StartActivity(i);
		}

		private void ProfilePopup()
		{
			var menu = new PopupMenu (this, action);
			menu.Inflate (Resource.Menu.Popup);
			var count = 0;
			foreach(var profiel in _appController.DistinctProfielen)
			{
				menu.Menu.Add(0,count,count,profiel.Name);
				count++;
			}

			menu.Menu.Add(0,count,count, "Nieuw profiel");

			menu.MenuItemClick += (s1, arg1) => {
				if(arg1.Item.ItemId == count)
				{
					var alert = new AlertDialog.Builder (this);
					alert.SetTitle ("Nieuw profiel");
				    var input = new EditText(this)
				    {
				        InputType = InputTypes.TextFlagCapWords,
				        Hint = "Naam"
				    };

				    KeyboardHelper.ShowKeyboard(this, input);
					alert.SetView (input);
					alert.SetPositiveButton ("Ok", (s, args) => {
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
							_appController.AddTotemToProfiel(_appController.CurrentTotem.Nid, value);
							mToast.SetText((hidden ? "Totem" : _appController.GetTotemOnId(_appController.CurrentTotem.Nid).Title) + " toegevoegd aan profiel " + value.Replace("'", ""));
							mToast.Show();
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

				    RunOnUiThread (d1.Show);
				}
				else
				{
					_appController.AddTotemToProfiel(_appController.CurrentTotem.Nid, arg1.Item.TitleFormatted.ToString());
					mToast.SetText((hidden ? "Totem" : _appController.GetTotemOnId(_appController.CurrentTotem.Nid).Title) + " toegevoegd aan profiel " + arg1.Item.TitleFormatted);
					mToast.Show();
				}
			};

			menu.Show ();
		}
			
		private int ConvertDpToPixels(float dp)
		{
			var scale = Resources.DisplayMetrics.Density;
			var result =  (int)(dp * scale + 0.5f);

			return result;
		}

		//displays totem info
		private void SetInfo()
		{
            body.Text = _appController.CurrentTotem.Body;
            if (hidden)
            {
                title_synonyms.Text = "...";
                var totemBody = _appController.CurrentTotem.Body.Replace(_appController.CurrentTotem.Title, "...");
                totemBody = totemBody.Replace(_appController.CurrentTotem.Title.ToLower(), "...");
                totemBody = totemBody.Replace(_appController.CurrentTotem.Title.Normalize(), "...");
                body.Text = totemBody;
            }
            else
            {
                number.Text = _appController.CurrentTotem.Number + ". ";

                var verveine = Typeface.CreateFromAsset(Assets, "fonts/Verveine W01 Regular.ttf");

                //code to get formatting right
                //title and synonyms are in the same TextView
                //font, size,... are given using spans
                if (_appController.CurrentTotem.Synonyms != null)
                {
                    var titlestring = _appController.CurrentTotem.Title;
                    var synonymsstring = " - " + _appController.CurrentTotem.Synonyms + " ";

                    var din = Typeface.CreateFromAsset(Assets, "fonts/DINPro-Light.ttf");

                    ISpannable sp = new SpannableString(titlestring + synonymsstring);
                    sp.SetSpan(new CustomTypefaceSpan("sans-serif", verveine, 0), 0, titlestring.Length, SpanTypes.ExclusiveExclusive);
                    sp.SetSpan(new CustomTypefaceSpan("sans-serif", din, TypefaceStyle.Italic, ConvertDpToPixels(17)), titlestring.Length, titlestring.Length + synonymsstring.Length, SpanTypes.ExclusiveExclusive);

                    title_synonyms.TextFormatted = sp;
                }
                else
                {
                    title_synonyms.Text = _appController.CurrentTotem.Title;
                    title_synonyms.SetTypeface(verveine, 0);
                }
            }
		}
	}
}