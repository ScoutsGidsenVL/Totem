using ServiceStack.Text;

using System.Collections.Generic;
using System.Linq;

namespace TotemAppCore {
	public class AppController {

		static readonly AppController _instance = new AppController();
		readonly Database database = DatabaseHelper.GetInstance ();

		public delegate void Update();
		public event Update UpdateCounter;

		public delegate void SelectedEigenschappen();
		public event Update ShowSelected;

		List<Totem> _totems;
		List<Eigenschap> _eigenschappen;
        List<Eigenschap> _eigenschappen_empty;
        List<Profiel> _profielen;

		NavigationController _navigationController;

		public AppController () {
			_totems = database.GetTotems ();
			_eigenschappen = database.GetEigenschappen ();

            //default list of eigenschappen which are never selected
            _eigenschappen_empty = database.GetEigenschappen();
            _navigationController = new NavigationController ();
			TotemEigenschapDict = new Dictionary<Totem, int> ();
		}

		//used to determine context of detail page
		public enum DetailMode {
			NORMAL,
			PROFILE,
			RESULT
		}


		/* ------------------------------ PROPERTIES ------------------------------ */


		public static AppController Instance {
			get {
				return _instance;
			}
		}

		public NavigationController NavigationController {
			get {
				return _navigationController;
			}
		}

		public List<Totem> Totems {
			get {
				return _totems;
			}
		}

		public List<Eigenschap> Eigenschappen {
			get {
				return _eigenschappen;
			}

            set {
                _eigenschappen = value;
            }
		}

		public List<Profiel> AllProfielen {
			get {
				_profielen = database.GetProfielen ();
				return _profielen;
			}
		}

		public List<Profiel> DistinctProfielen{
			get {
				_profielen = database.GetProfielen (true);
				return _profielen;
			}
		}

		public Totem CurrentTotem { get; set; }
		public DetailMode detailMode { get; set; }

		//returns next totem depending on context
		public Totem NextTotem {
			get {
				List<Totem> list;
				switch (detailMode)
				{
				    case DetailMode.NORMAL:
				        list = Totems;
				        break;
				    case DetailMode.PROFILE:
				        list = GetTotemsFromProfiel(CurrentProfiel.Name);
				        break;
				    default:
				        list = TotemEigenschapDict.Keys.ToList ();
				        break;
				}
				
				var index = list.FindIndex (x => x.Title.Equals (CurrentTotem.Title));

				return index != (list.Count - 1) ? list [index + 1] : null;
			}
		}

		//returns previous totem depending on context
		public Totem PrevTotem {
			get {
				List<Totem> list;
				switch (detailMode)
				{
				    case DetailMode.NORMAL:
				        list = Totems;
				        break;
				    case DetailMode.PROFILE:
				        list = GetTotemsFromProfiel(CurrentProfiel.Name);
				        break;
				    default:
				        list = TotemEigenschapDict.Keys.ToList ();
				        break;
				}
				
				var index = list.FindIndex (x => x.Title.Equals (CurrentTotem.Title));

				return index != 0 ? list[index - 1] : null;
			}
		}

		public Profiel CurrentProfiel { get; set; }

		public Dictionary<Totem, int> TotemEigenschapDict { get; set; }

		public bool ShowAdd { get; set; }


		/* ------------------------------ DATABASE ------------------------------ */


		//returns totem-object with given id (int)
		public Totem GetTotemOnId(int idx) {
		    foreach (var totem in _totems)
		    {
		        if (totem.Nid.Equals(idx.ToString()))
		        {
		            return totem;
		        }
		    }

		    return null;
		}

		//returns totem-object with given id (string)
		public Totem GetTotemOnId(string idx) {
			return GetTotemOnId (int.Parse (idx));
		}

		//returns totem-object with given name
		public List<Totem> FindTotemOpNaam(string name) {
			var result = new List<Totem> ();
			foreach(var totem in _totems)
				if(totem.Title.ToLower().Contains(name.ToLower()))
					result.Add (totem);

			return result;
		}

        public List<Totem> FindTotemOpNaamOfSyn(string name) {
            var result = new List<Totem>();
            foreach (var totem in _totems) {
                if (totem.Title.ToLower().Contains(name.ToLower()))
                {
                    result.Add(totem);
                }
                else if (totem.Synonyms != null && totem.Synonyms.ToLower().Contains(name.ToLower()))
                {
                    result.Add(totem);
                }
            }

            return result;
        }

        //returns eigenschap-object with given name
        public List<Eigenschap> FindEigenschapOpNaam(string name) {
			var result = new List<Eigenschap> ();
            foreach (var eigenschap in _eigenschappen)
            {
                if (eigenschap.Name.Normalize().Contains(name.Normalize()))
                {
                    result.Add(eigenschap);
                }
            }

            return result;
		}

		//returns a list of totems related to a profile
		public List<Totem> GetTotemsFromProfiel(string name) {
			var list = AllProfielen.FindAll (x => x.Name == name);
			var profiles = list.FindAll (y =>y.Nid!=null);
			var result = new List<Totem> ();
			foreach (var profile in profiles)
			{
				result.Add (GetTotemOnId (profile.Nid));
			}

			return result;
		}

		//add totem to profile in db
		public void AddTotemToProfiel(string totemID, string profielName) {
			database.AddTotemToProfiel (totemID,profielName);
		}

		//add a profile
		public void AddProfile(string name){
			database.AddProfile (name);
            AddOrUpdateEigenschappenSer(name, JsonSerializer.SerializeToString(_eigenschappen_empty));
        }

		//delete a profile
		public void DeleteProfile(string name) {
			database.DeleteProfile (name);
		}

		//delete a totem from a profile
		public void DeleteTotemFromProfile(string totemID, string profielName) {
			database.DeleteTotemFromProfile (totemID,profielName);
		}

		//returns List of Totem_eigenschappen related to eigenschap id
		public List<Totem_eigenschap> GetTotemsVanEigenschapsID(string id) {
			return database.GetTotemsVanEigenschapsID (id);
		}

		//returns random tip out of the database
		public string GetRandomTip() {
			return database.GetRandomTip ();
		}
		//get list of profile names
		public List<string> GetProfielNamen() {
			var namen = new List<string> ();
			foreach (Profiel p in DistinctProfielen)
				namen.Add (p.Name);
			return namen;
		}

        public string GetSerFromProfile(string profileName) {
            return database.GetSerFromProfile(profileName);
        }

        public void AddOrUpdateEigenschappenSer(string profielName, string ser) {
            database.AddOrUpdateEigenschappenSer(profielName, ser);
        }


        /* ------------------------------ CLICK EVENTS ------------------------------ */


        public void TotemMenuItemClicked() {
			_navigationController.GoToTotemList ();
		}

		public void EigenschappenMenuItemClicked() {
            SetCurrentProfile(null);
            _navigationController.GoToEigenschapList ();
		}

		public void ProfileMenuItemClicked() {
			_navigationController.GoToProfileList ();
		}

		public void ChecklistMenuItemClicked() {
			_navigationController.GoToChecklist ();
		}

		public void TinderMenuItemClicked() {
			_navigationController.GoToTinder ();
		}

		//sets current totem and resets current profile
		public void TotemSelected(string totemID) {
			SetCurrentTotem (totemID);
			ShowAdd = true;
			_navigationController.GoToTotemDetail ();
		}

		//sets current profile
		public void ProfileSelected(string profileName) {
			SetCurrentProfile (profileName);
			ShowAdd = false;
			_navigationController.GoToProfileTotemList ();
		}

		//sets current totem and current profile
		public void ProfileTotemSelected(string profileName, string totemID) {
			SetCurrentProfile (profileName);
			SetCurrentTotem (totemID);
			ShowAdd = false;
			detailMode = DetailMode.PROFILE;
			_navigationController.GoToTotemDetail ();
		}

        //sets current totem and current profile
        public void ProfileEigenschappenSelected(string profileName) {
            SetCurrentProfile(profileName);
			ShowAdd = true;
            _navigationController.GoToEigenschapList();
        }

        public void CalculateResultlist(List<Eigenschap> checkboxList) {
			FillAndSortDict (checkboxList);
            detailMode = DetailMode.RESULT;
			ShowAdd = true;
			_navigationController.GoToTotemResult ();
		}


		/* ------------------------------ MISC ------------------------------ */

			
		void SetCurrentTotem(string totemId) {
			var totem = _totems.Find (x => x.Nid.Equals (totemId));
			CurrentTotem = totem;
		}

		void SetCurrentProfile(string profileName) {
			if (profileName == null) {
				CurrentProfiel = null;
			} else {
				var profiel = _profielen.Find (x => x.Name.Equals (profileName));
				CurrentProfiel = profiel;
			}
		}

		//fills dictionary with totems and according frequency based on selected eigenschappen
        //sorts the dicitionary by value
		void FillAndSortDict(List<Eigenschap> checkboxList) {
			TotemEigenschapDict = new Dictionary<Totem, int> ();
			foreach (Eigenschap eig in checkboxList) {
				if(eig.Selected) {
					List<Totem_eigenschap> toAdd = GetTotemsVanEigenschapsID (eig.EigenschapId);
					foreach(Totem_eigenschap totem in toAdd) {
						CollectionHelper.AddOrUpdateDictionaryEntry (TotemEigenschapDict, database.GetTotemsOnId(totem.nid));
					}
				}
			}
			TotemEigenschapDict = CollectionHelper.GetSortedDict (TotemEigenschapDict);
		}

		public void FireUpdateEvent()
		{
		    UpdateCounter?.Invoke ();
		}

		public void FireSelectedEvent()
		{
		    ShowSelected?.Invoke ();
		}
	}
}