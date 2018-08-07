using Android.App;
using Android.Views;
using Android.Widget;

using System.Collections.Generic;

using TotemAppCore;

namespace TotemAndroid {
	public class TotemAdapter: BaseAdapter<Totem>, ISectionIndexer {
	    private Activity _activity;
	    private List<Totem> totemList;
	    private int[] freqs;
	    private int selected;
	    private bool showDelete;

	    private Dictionary<string, int> alphaIndex;
	    private string [] sections;
	    private Java.Lang.Object[] sectionsObjects;

		public TotemAdapter (Activity activity, List<Totem> list) {	
			_activity = activity;
			totemList = list;
			showDelete = false;

			var items = list.ToArray ();

			alphaIndex = new Dictionary<string, int>();
			for (int i = 0; i < items.Length; i++) {
				var key = items[i].Title[0].ToString();
				if (!alphaIndex.ContainsKey(key)) 
					alphaIndex.Add(key, i);
			}
			sections = new string[alphaIndex.Keys.Count];
			alphaIndex.Keys.CopyTo(sections, 0);
			sectionsObjects = new Java.Lang.Object[sections.Length];
			for (int i = 0; i < sections.Length; i++) {
				sectionsObjects[i] = new Java.Lang.String(sections[i]);
			}
		}

		public TotemAdapter (Activity activity, List<Totem> list, int[] freqs, int selected): this(activity, list) {	
			this.freqs = freqs;
			this.selected = selected;
		}

		public void ShowDelete() {
			showDelete = true;
		}

		public void HideDelete() {
			showDelete = false;
		}

		public void UpdateData(List<Totem> list) {
			totemList = list;
		}

		public override Totem this[int index] {
			get {
				return totemList [index];
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
				viewHolder.totem = convertView.FindViewById<TextView> (Resource.Id.totem);
				viewHolder.checkbox = convertView.FindViewById<CheckBox> (Resource.Id.deleteItem);
				viewHolder.freq = convertView.FindViewById<TextView> (Resource.Id.freq);

				convertView.Tag = viewHolder;
			} else {
				viewHolder = (ViewHolder)convertView.Tag;
			}

			viewHolder.checkbox.Visibility = showDelete ? ViewStates.Visible : ViewStates.Gone;

			viewHolder.checkbox.Tag = position;

			viewHolder.totem.Text = totemList [position].Title;
			viewHolder.checkbox.Checked = totemList [(int)viewHolder.checkbox.Tag].Selected;
			if (freqs != null)
				viewHolder.freq.Text = freqs [position].ToString ();

			viewHolder.checkbox.Click += (o, e) => {
				totemList [(int)viewHolder.checkbox.Tag].Selected = viewHolder.checkbox.Checked;
			};

			return convertView;
		}

		//ViewHolder for better performance
	    private class ViewHolder : Java.Lang.Object {
			public TextView totem;
			public CheckBox checkbox;
			public TextView freq;
		}

		public override int Count {
			get {
				return totemList.Count;
			}
		}

		public Totem GetItemAtPosition(int position) {
			return totemList[position];
		}

		public int GetPositionForSection (int section) {
			return alphaIndex[sections[section]];
		}

		public int GetSectionForPosition(int position) {
			return 0;
		}

		public Java.Lang.Object[] GetSections () {
			return sectionsObjects;
		}
	}
}