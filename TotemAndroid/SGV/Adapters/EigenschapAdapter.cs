using Android.App;
using Android.Views;
using Android.Widget;

using System.Collections.Generic;

using TotemAppCore;

namespace TotemAndroid {
    public class EigenschapAdapter: BaseAdapter<Eigenschap> {
        private Activity _activity;
        private List<Eigenschap> eigenschapList;

        private AppController _appController = AppController.Instance;

        private MyOnCheckBoxClickListener mListener;

        private int mScreenWidth;

		public EigenschapAdapter (Activity activity, List<Eigenschap> list, MyOnCheckBoxClickListener listener, int screenWidth) {	
			_activity = activity;
			eigenschapList = list;
			mListener = listener;
			_appController.FireUpdateEvent();
            mScreenWidth = screenWidth;
		}

		public void UpdateData(List<Eigenschap> list) {
			eigenschapList = list;
			_appController.FireUpdateEvent();
		}

		public override Eigenschap this[int index] {
			get {
				return eigenschapList [index];
			}
		}

		public override long GetItemId (int position) {
			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent) {
			ViewHolder viewHolder;

			if (convertView == null) {
				convertView = _activity.LayoutInflater.Inflate (Resource.Layout.EigenschapListItem, parent, false);

				viewHolder = new ViewHolder ();
				viewHolder.eigenschap = convertView.FindViewById<TextView> (Resource.Id.eigenschap);
				viewHolder.checkbox = convertView.FindViewById<CheckBox> (Resource.Id.checkbox);

				convertView.Tag = viewHolder;
			} else {
				viewHolder = (ViewHolder)convertView.Tag;
			}

			//IMPORTANT: keeps track of which checkboxes are checked
			//and which aren't during scrolling
			viewHolder.checkbox.Tag = position;

			viewHolder.eigenschap.Text = eigenschapList [position].Name; 
			viewHolder.checkbox.Checked = eigenschapList [(int)viewHolder.checkbox.Tag].Selected;

            //smaller font size for smaller screens
            //otherwise UI issue
            if (mScreenWidth <= 480)
                viewHolder.eigenschap.TextSize = 15;

            //notifies CheckBoxListener and stores selection
            if (!viewHolder.checkbox.HasOnClickListeners) {
				viewHolder.checkbox.Click += (o, e) => {
					mListener.OnCheckboxClicked ();
					if (viewHolder.checkbox.Checked)
						eigenschapList [(int)viewHolder.checkbox.Tag].Selected = true;
					else
						eigenschapList [(int)viewHolder.checkbox.Tag].Selected = false;
				
					_appController.FireUpdateEvent ();
				};
			}
				
			if (!convertView.HasOnClickListeners) {
				convertView.Click += (o, e) => {
					mListener.OnCheckboxClicked ();
					viewHolder.checkbox.Checked = !(viewHolder.checkbox.Checked);
					if (viewHolder.checkbox.Checked)
						eigenschapList [(int)viewHolder.checkbox.Tag].Selected = true;
					else
						eigenschapList [(int)viewHolder.checkbox.Tag].Selected = false;

					_appController.FireUpdateEvent();
				};
			}
				
			return convertView;
		}

		//ViewHolder for better performance
        private class ViewHolder : Java.Lang.Object {
			public TextView eigenschap;
			public CheckBox checkbox;
		}

		public override int Count {
			get {
				return eigenschapList.Count;
			}
		}

		public Eigenschap GetItemAtPosition(int position) {
			return eigenschapList[position];
		}
	}
}