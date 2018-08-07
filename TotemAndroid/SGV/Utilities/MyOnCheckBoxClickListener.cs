using Android.Content;

using System;

namespace TotemAndroid {
	public class MyOnCheckBoxClickListener {
		
		//WeakReference to context to avoid memory leak
	    private readonly WeakReference<Context> mContext;

		public MyOnCheckBoxClickListener(Context context) {
			mContext = new WeakReference<Context>(context);
		}

		//hides keyboard when checkbox is clicked
		public void OnCheckboxClicked() {
			Context context;
			mContext.TryGetTarget(out context);
			KeyboardHelper.HideKeyboard (context);
		}
	}
}