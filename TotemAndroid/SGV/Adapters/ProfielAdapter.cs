using Android.App;
using Android.Views;
using Android.Widget;

using System.Collections.Generic;

using TotemAppCore;

namespace TotemAndroid {
	public class ProfielAdapter: BaseAdapter<Profiel> {
	    private Activity _activity;
	    private List<Profiel> profielList;
	    private bool showDelete;

		public ProfielAdapter (Activity activity, List<Profiel> list) {	
			this._activity = activity;
			this.profielList = list;
			this.showDelete = false;
		}

		public void UpdateData(List<Profiel> list) {
			this.profielList = list;
		}

		public void ShowDelete() {
			this.showDelete = true;
		}

		public void HideDelete() {
			this.showDelete = false;
		}

		public override Profiel this[int index] {
			get {
				return profielList [index];
			}
		}

		public override long GetItemId (int position) {
			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent) {
			ViewHolder viewHolder;

			if (convertView == null) {
				convertView = _activity.LayoutInflater.Inflate (Resource.Layout.TotemListItem, parent, false);

				viewHolder = new ViewHolder ();
				viewHolder.profiel = convertView.FindViewById<TextView> (Resource.Id.totem);
				viewHolder.checkbox = convertView.FindViewById<CheckBox> (Resource.Id.deleteItem);

				convertView.Tag = viewHolder;
			} else {
				viewHolder = (ViewHolder)convertView.Tag;
			}

			viewHolder.checkbox.Visibility = showDelete ? ViewStates.Visible : ViewStates.Gone;

			viewHolder.checkbox.Tag = position;

			viewHolder.profiel.Text = profielList [position].Name;
			viewHolder.checkbox.Checked = profielList [(int)viewHolder.checkbox.Tag].Selected;

			viewHolder.checkbox.Click += (o, e) => {
				profielList [(int)viewHolder.checkbox.Tag].Selected = viewHolder.checkbox.Checked ? true : false;
			};

			return convertView;
		}

		//ViewHolder for better performance
	    private class ViewHolder : Java.Lang.Object {
			public TextView profiel;
			public CheckBox checkbox;
		}

		public override int Count {
			get {
				return profielList.Count;
			}
		}

		public Profiel GetItemAtPosition(int position) {
			return profielList[position];
		}
	}
}