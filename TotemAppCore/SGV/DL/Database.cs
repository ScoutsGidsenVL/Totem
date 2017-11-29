using System;
using System.Collections.Generic;
using System.IO;

#if __ANDROID__
using Android.App;
#endif

#if __IOS__
using Foundation;
#endif

using SQLite;

namespace TotemAppCore {
    public class Database {
		SQLiteConnection database;

		#if __ANDROID__
		string originalDBLocation = "totems.sqlite";
        #elif __IOS__
		string originalDBLocation = "SharedAssets/totems.sqlite";
        #endif

        const int DATABASE_VERSION = 3;
		string currentDBName = "totems" + DATABASE_VERSION + ".sqlite";
        string oldDBName = "totems" + (DATABASE_VERSION - 1) + ".sqlite";

        //path of current version of database
        string DatabasePath { 
			get { 
				var sqliteFilename = currentDBName;

                #if __IOS__
				int SystemVersion = Convert.ToInt16(UIKit.UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
				string documentsPath = SystemVersion >= 8 ? NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path : Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				var path = Path.Combine(documentsPath, sqliteFilename);

                #elif __ANDROID__
                string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				var path = Path.Combine(documentsPath, sqliteFilename);
                #endif

                return path;
			}
		}

        //path of previous version of database
        string OldDatabasePath {
            get {
                var sqliteFilename = oldDBName;

                #if __IOS__
				int SystemVersion = Convert.ToInt16(UIKit.UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
				string documentsPath = SystemVersion >= 8 ? NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path : Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				var path = Path.Combine(documentsPath, sqliteFilename);

                #elif __ANDROID__
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var path = Path.Combine(documentsPath, sqliteFilename);
                #endif

                return path;
            }
        }

        //reads the write stream.
        void ReadWriteStream(Stream readStream, Stream writeStream) {
			int Length = 256;
			var buffer = new byte[Length];
			int bytesRead = readStream.Read(buffer, 0, Length);
			// write the required bytes
			while (bytesRead > 0) {
				writeStream.Write(buffer, 0, bytesRead);
				bytesRead = readStream.Read(buffer, 0, Length);
			}
			readStream.Close();
			writeStream.Close();
		}

		//initializes a new instance of the database
		//if the database doesn't exist, it will create the database
		public Database() {
			var dbPath = DatabasePath;

			if (!File.Exists (dbPath))
                CreateDatabase(dbPath);

            database = new SQLiteConnection(dbPath);			
		}

        void CreateDatabase(string dbPath) {
            #if __ANDROID__
            var s = Application.Context.Assets.Open(originalDBLocation);
            var writeStream = new FileStream(dbPath, FileMode.OpenOrCreate, FileAccess.Write);
            ReadWriteStream(s, writeStream);
            writeStream.Close();

            #elif __IOS__
			var appDir = NSBundle.MainBundle.ResourcePath;
			var originalLocation = Path.Combine (appDir, originalDBLocation);
			File.Copy (originalLocation, dbPath);
            #endif

            //copies profiles and selected eigenschappen from the old database to the new one
            if (File.Exists(OldDatabasePath)) {
                SQLiteConnection oldDb = new SQLiteConnection(OldDatabasePath);
                SQLiteConnection newDB = new SQLiteConnection(dbPath);

                var oldProfielen = oldDb.Query<Profiel>("SELECT * FROM profiel");
                var oldEigenschappenSer = oldDb.Query<Profiel_eigenschappen>("SELECT * FROM profiel_eigenschappen");
                foreach(Profiel p in oldProfielen)
                    newDB.Execute("INSERT INTO profiel (name, nid) values (?, ?)", p.name, p.nid);
                
                foreach (Profiel_eigenschappen pe in oldEigenschappenSer)
                    newDB.Execute("INSERT INTO profiel_eigenschappen (name, eigenschappen_ser) values (?, ?)", pe.name, pe.eigenschappen_ser);
                
                oldDb.Dispose();
                newDB.Dispose();
                File.Delete(OldDatabasePath);
            }
        }

        
        /* ------------------------------ INITIALIZE DB ------------------------------ */


        //extract eigenschappen from DB and put them in a list
        public List<Eigenschap> GetEigenschappen() {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select * from eigenschap order by name";
				var eigenschappen = cmd.ExecuteQuery<Eigenschap> ();
				return eigenschappen;
			}
		}

		//extract totems from DB and put them in a list
		public List<Totem> GetTotems() {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select * from totem order by title";
				var totems = cmd.ExecuteQuery<Totem> ();
				foreach (Totem totem in totems)
				{
                    totem.eigenschappen = GetEigenschappenVanTotemID(totem.nid);
				}
				return totems;
			}
		}


		/* ------------------------------ PROFIELEN ------------------------------ */


		//get list of profile-objects
		public List<Profiel> GetProfielen(bool distinct = false) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				if (distinct)
					cmd.CommandText = "select distinct name from profiel";
				else
					cmd.CommandText = "select * from profiel";
				return cmd.ExecuteQuery<Profiel> ();
			}
		}

		//add totem to profile in db
		public void AddTotemToProfiel(string totemID, string profielName) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanProfielName = profielName.Replace("'", "");
				var cleanTotemID = totemID.Replace("'", "");
				cmd.CommandText = "insert into profiel (name, nid) select '" + cleanProfielName + "'," + cleanTotemID + 
					" WHERE NOT EXISTS ( SELECT * FROM profiel WHERE name='"+ cleanProfielName +"' AND nid=" + cleanTotemID + ");";
				cmd.ExecuteQuery<Profiel> ();
			}
		}

		//add a profile
		public void AddProfile(string name) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanName = name.Replace("'", "");
				cmd.CommandText = "insert into profiel (name) values ('" + cleanName + "')";
				cmd.ExecuteQuery<Profiel> ();
			}
		}

		//delete a profile
		public void DeleteProfile(string name) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanName = name.Replace("'", "");
				cmd.CommandText = "DELETE FROM profiel WHERE name='" + cleanName + "'";
				cmd.ExecuteQuery<Profiel> ();
			}
            DeleteSer(name);
        }

		//delete a totem from a profile
		public void DeleteTotemFromProfile(string totemID, string profielName) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanProfielName = profielName.Replace("'", "");
				var cleanTotemID = totemID.Replace("'", "");
				cmd.CommandText = "DELETE FROM profiel WHERE name='" + cleanProfielName + "' AND nid=" + cleanTotemID;
				cmd.ExecuteQuery<Profiel> ();
			}
		}

        //returns serialized eigenschappenlist of profile
        public string GetSerFromProfile(string profileName) {
            List<Profiel_eigenschappen> res;
            lock (database) {
                var cmd = new SQLiteCommand(database);
                var cleanName = profileName.Replace("'", "");
                cmd.CommandText = "select * from profiel_eigenschappen where name='" + cleanName + "'";
                res = cmd.ExecuteQuery<Profiel_eigenschappen>();
            }
            if (res.Count == 0)
                return null;
            else
                return res[0].eigenschappen_ser;
        }

        //adds or updates serialization-entry of profile
        public void AddOrUpdateEigenschappenSer(string profielName, string ser) {
            if (ProfileSerExists(profielName)) {
                lock (database) {
                    var cmd = new SQLiteCommand(database);
                    var cleanName = profielName.Replace("'", "");
                    cmd.CommandText = "update profiel_eigenschappen set eigenschappen_ser='" + ser + "' where name='" + cleanName + "'";
                    cmd.ExecuteQuery<Profiel_eigenschappen>();
                }
            } else {
                lock (database) {
                    var cmd = new SQLiteCommand(database);
                    var cleanName = profielName.Replace("'", "");
                    cmd.CommandText = "insert into profiel_eigenschappen (name, eigenschappen_ser) values ('" + cleanName + "', '" + ser + "')";
                    cmd.ExecuteQuery<Profiel_eigenschappen>();
                }
            }
        }

        //delete serialization entry of profile
        public void DeleteSer(string profielName) {
            var cmd = new SQLiteCommand(database);
            var cleanProfielName = profielName.Replace("'", "");
            cmd.CommandText = "DELETE FROM profiel_eigenschappen WHERE name='" + cleanProfielName + "'";
            cmd.ExecuteQuery<Profiel_eigenschappen>();
        }

        //checks if database already has serialization-entry
        bool ProfileSerExists(string profielName) {
            List<Profiel_eigenschappen> res;
            lock (database) {
                var cmd = new SQLiteCommand(database);
                var cleanName = profielName.Replace("'", "");
                cmd.CommandText = "select name from profiel_eigenschappen where name='" + cleanName + "'";
                res = cmd.ExecuteQuery<Profiel_eigenschappen>();
            }
            return !(res.Count == 0);
        }


        /* ------------------------------ TOTEMS EN EIGENSCHAPPEN ------------------------------ */

        //returns List of Totem_eigenschapp related to totem id
        public List<Eigenschap> GetEigenschappenVanTotemID(string id)
        {
            List<Eigenschap> EigenschappenVanTotem = new List<Eigenschap>();
            lock (database)
            {
                var cmd = new SQLiteCommand(database);
                var cleanId = id.Replace("'", "");
                cmd.CommandText = "select tid from totem_eigenschap where nid = " + cleanId;
                var totem_eigenschapen = cmd.ExecuteQuery<Totem_eigenschap>();

                foreach (var totem_eigenschap in totem_eigenschapen)
                {
                    cmd.CommandText = "select * from eigenschap where tid = " + totem_eigenschap.tid;
                    EigenschappenVanTotem.Add(cmd.ExecuteQuery<Eigenschap>()[0]);
                }
            }
            return EigenschappenVanTotem;
        }

        //returns List of Totem_eigenschapp related to eigenschap id
        public List<Totem_eigenschap> GetTotemsVanEigenschapsID(string id) {
			List<Totem_eigenschap> totemsVanEigenschap;
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanId = id.Replace("'", "");
				cmd.CommandText = "select nid from totem_eigenschap where tid = " + cleanId;
				totemsVanEigenschap = cmd.ExecuteQuery<Totem_eigenschap> ();
			}
			return totemsVanEigenschap;
		}

		//returns List of Totem_eigenschapp related to eigenschap id
		public Totem GetTotemsOnId(string id) {
			List<Totem> list;
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanId = id.Replace("'", "");
				cmd.CommandText = "select * from totem where nid = " + cleanId;
				list = cmd.ExecuteQuery<Totem> ();
			}
			return list [0];
		}


		/* ------------------------------ MISC ------------------------------ */


		//returns random tip out of the database
		public string GetRandomTip() {
			List<Tip> list;
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select * from tip";
				list = cmd.ExecuteQuery<Tip> ();
			}
			var rnd = new Random ();
			return list [rnd.Next (list.Count)].tip;
		}
	}
}