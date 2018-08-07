using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using System;

namespace TotemAndroid {
	public class MyOnTouchListener : Java.Lang.Object, View.IOnTouchListener {

		//WeakReference to context to avoid memory leak
	    private readonly WeakReference<Context> mContext;
	    private readonly EditText edittext;

		public MyOnTouchListener (Context context, EditText edittext) {
			mContext = new WeakReference<Context>(context);
			this.edittext = edittext;
		}

		//hides keyboard when ListView scrolled
		public bool OnTouch(View v, MotionEvent e) {
			Context context = null;
			mContext.TryGetTarget(out context);
			var imm = (InputMethodManager)context.GetSystemService (Context.InputMethodService);
			imm.HideSoftInputFromWindow (edittext.WindowToken, 0);
			return false;
		}
	}
}