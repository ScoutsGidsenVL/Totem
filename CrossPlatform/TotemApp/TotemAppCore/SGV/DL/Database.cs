﻿using System;
using System.Collections.Generic;
using System.IO;
using Foundation;


#if __ANDROID__
using Android.Content;
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

		string currentDBName = "totems.sgv";

			string DatabasePath //path for checking if database exists.
			{
				get { 
					var sqliteFilename = currentDBName;

					#if __IOS__
					int SystemVersion = Convert.ToInt16(UIKit.UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
					string libraryPath;
					if(SystemVersion >= 8){
						var documentsPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
						libraryPath = Path.Combine(documentsPath, "..", "Library/Testaankoop");
					}else{
						string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
						libraryPath = Path.Combine (documentsPath, "..", "Library/Testaankoop"); // Library folder
					}
					var path = Path.Combine(libraryPath, sqliteFilename);
					#else
					#if __ANDROID__
					string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
					var path = Path.Combine(documentsPath, sqliteFilename);
					//var path = sqliteFilename;
					#else
					// WinPhone
					var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);;
					#endif
					#endif
					return path;
				}
			}

			/// <Summary>
			/// Reads the write stream.
			/// </Summary>
			/// <param name="readStream">Read stream.</param>
			/// <param name="writeStream">Write stream.</param>
			void ReadWriteStream(Stream readStream, Stream writeStream)
			{
				int Length = 256;
				Byte[] buffer = new Byte[Length];
				int bytesRead = readStream.Read(buffer, 0, Length);
				// write the required bytes
				while (bytesRead > 0)
				{
					writeStream.Write(buffer, 0, bytesRead);
					bytesRead = readStream.Read(buffer, 0, Length);
				}
				readStream.Close();
				writeStream.Close();
			}

			/// <Summary>
			/// Initializes a new instance of the <see cref="Bazookas.Testaankoop.DL.TestaankoopDB"/> Database. 
			/// if the database doesn't exist, it will create the database and all the tables.
			/// </Summary>
			/// <param name='path'>
			/// Path.
			/// </param>
			public Database()
			{
				var dbPath = DatabasePath;
				if (!File.Exists (dbPath)) 
				{
					#if __ANDROID__
					var s = Application.Context.Assets.Open (originalDBLocation);
					FileStream writeStream = new FileStream (dbPath, FileMode.OpenOrCreate, FileAccess.Write);
					ReadWriteStream (s, writeStream);
					writeStream.Close ();

					#elif __IOS__
					int SystemVersion = Convert.ToInt16(UIKit.UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
					string libraryPath;
					string imagePath;
					if(SystemVersion >= 8){
						var documents = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0].Path;
						var lib = Path.Combine(documents, "..", "Library");
						libraryPath = Path.Combine (lib, "Testaankoop");
						imagePath = Path.Combine (libraryPath, "Images");
					}else{
						string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
						libraryPath = Path.Combine (documentsPath, "../Library/Testaankoop");
						imagePath = Path.Combine (documentsPath, "../Library/Testaankoop/Images");
					}
					if (!Directory.Exists (libraryPath)) {
						Directory.CreateDirectory (libraryPath);
					}
					if (!Directory.Exists (imagePath)) {
						Directory.CreateDirectory (imagePath);
					}
					// does not exist yet, copy local database into new database

					var appDir = NSBundle.MainBundle.ResourcePath;
					var originalLocation = Path.Combine (appDir, originalDBLocation);
					File.Copy (originalLocation, dbPath);
					#endif
				}

				database = new SQLiteConnection (dbPath);
				// create the tables
				//database.CreateTable<TodoItem>();
			}
			

		/* ------------------------------ INITIALIZE DB ------------------------------ */

		//extract eigenschappen from DB and put them in a list
		public List<Eigenschap> GetEigenschappen() {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select * from eigenschap_nieuw order by name";
				var eigenschappen = cmd.ExecuteQuery<Eigenschap> ();
				return eigenschappen;
			}
		}

		//extract totems from DB and put them in a list
		public List<Totem> GetTotems() {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select * from totem_nieuw order by title";
				var totems = cmd.ExecuteQuery<Totem> ();
				return totems;
			}
		}


		/* ------------------------------ PROFIELEN ------------------------------ */


		//get list of profile-objects
		public List<Profiel> GetProfielen() {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				cmd.CommandText = "select distinct name from profiel";
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




		/* ------------------------------ TOTEMS EN EIGENSCHAPPEN ------------------------------ */

		//returns List of Totem_eigenschapp related to eigenschap id
		public List<Totem_eigenschap> GetTotemsVanEigenschapsID(string id) {
			List<Totem_eigenschap> totemsVanEigenschap;
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanId = id.Replace("'", "");
				cmd.CommandText = "select nid from totem_eigenschap_nieuw where tid = " + cleanId;
				totemsVanEigenschap = cmd.ExecuteQuery<Totem_eigenschap> ();
			}
			return totemsVanEigenschap;
		}




		/* ------------------------------ UTILS ------------------------------ */


		//returns Userpref-object based on parameter
		public Userpref GetPreference(string preference) {
			lock (database) {
				List<Userpref> list;
				var cmd = new SQLiteCommand (database);
				var cleanPreference = preference.Replace("'", "");
				cmd.CommandText = "select value from userprefs where preference='" + cleanPreference + "'";
				list = cmd.ExecuteQuery<Userpref> ();
				return list [0];
			}
		}

		//updates the preference with new value
		public void ChangePreference(string preference, string value) {
			lock (database) {
				var cmd = new SQLiteCommand (database);
				var cleanPreference = preference.Replace("'", "");
				var cleanValue = value.Replace("'", "");
				cmd.CommandText = "update userprefs set value='" + cleanValue + "' where preference='" + cleanPreference + "'";
				cmd.ExecuteQuery<Userpref> ();
			}
		}

		//returns random tip out of the database
		public string GetRandomTip() {
			List<Tip> list;
			lock (database) {
				var cmd = new SQLite.SQLiteCommand (database);
				cmd.CommandText = "select * from tip";
				list = cmd.ExecuteQuery<Tip> ();
			}
			var rnd = new Random ();
			return list [rnd.Next (list.Count)].tip;
		}
	}
}