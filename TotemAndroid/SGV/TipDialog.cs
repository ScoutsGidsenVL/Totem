using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using TotemAppCore;

namespace TotemAndroid
{
    public class TipDialog : DialogFragment
    {
        private TextView tip;

        private Context context;

        private AppController _appController = AppController.Instance;

		public TipDialog(Context context)
		{
			this.context = context;
		}

		public static TipDialog NewInstance(Context context)
		{
			var dialogFragment = new TipDialog(context);

			return dialogFragment;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			var builder = new AlertDialog.Builder(Activity); 

			var inflater = Activity.LayoutInflater;
			var dialogView = inflater.Inflate(Resource.Layout.TipPopUp, null);

			if (dialogView != null)
			{
				tip = dialogView.FindViewById<TextView> (Resource.Id.tip);
				tip.Text = _appController.GetRandomTip();

				builder.SetView(dialogView);
				builder.SetNegativeButton("Ok", HandleNegativeButtonClick);
			}
				 
			var dialog = builder.Create();

			return dialog;
		}

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
			var dialog = (AlertDialog) sender;
			dialog.Dismiss ();
		}
	}
}