using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Globalization;
using System.IO;
using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace pathrise_backend
{
    class Program
    {
        
        static void Main(string[] args)
        {
            // Variables for paths
            string opportunitiesFilePath, jobBoardsPath, project;
            project = "pathrise-6a289";
            opportunitiesFilePath = "job_opportunities_limited.csv"; //CHANGE!
            jobBoardsPath = "jobBoards.json";

            //Connect to Firestore
            FirestoreDb db = ConnectToDb(project);

            // Open opportunities.csv
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(opportunitiesFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open file: " + opportunitiesFilePath);
                Console.WriteLine(e); //debug
                Environment.Exit(-1);
            }


            CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            BuildBoardList(jobBoardsPath, db);
            ParseCSV(csv, db);
        }

        static FirestoreDb ConnectToDb(string project)
        {
            FirestoreDb db = null;
            string path = AppDomain.CurrentDomain.BaseDirectory + @"pathrise-6a289-firebase-adminsdk-2p2b1-e75644c732.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            try
            {
                db = FirestoreDb.Create(project);
                Console.WriteLine("Created Cloud Firestore client with project ID: {0}", project);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to database with ID: {0}", project);
                Console.WriteLine(e); //debug
                Environment.Exit(-1);
            }
            return db;
        }
        static async void ParseCSV(CsvReader csv, FirestoreDb db)
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                Offer record = csv.GetRecord<Offer>();
                Dictionary<string, object> offer = new Dictionary<string, object>
                {
                    { "First", "Alan" },
                    { "Middle", "Mathison" },
                    { "Last", "Turing" },
                    { "Born", 1912 }
                };
                await db.Collection("offers").Document(record.Id.ToString()).SetAsync(offer);
                //Console.WriteLine(record.Id + record.jobTitle + record.company + record.jobUrl);
            }
        }

        static void BuildBoardList(string jobBoardsPath, FirestoreDb db)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(jobBoardsPath);
            }
            catch
            {
                Console.WriteLine("Unable to open file: " + jobBoardsPath);
                Environment.Exit(-1);
            }

            CollectionReference colRef = db.Collection("boards");
            JsonTextReader jsonTextReader = new JsonTextReader(reader);
            JObject o2 = (JObject)JToken.ReadFrom(jsonTextReader);
            Console.WriteLine(o2);
        }
    }
}

    public class Offer
    {
        [Index(0)]
        public int Id { get; set; }
        [Index(1)]
        public string JobTitle { get; set; }
        [Index(2)]
        public string Company { get; set; }
        [Index(3)]
        public string JobUrl { get; set; }
    }


    public class Rootobject
    {
        public Job_Boards[] job_boards { get; set; }
    }

    public class Job_Boards
    {
        public string name { get; set; }
        public string rating { get; set; }
        public string root_domain { get; set; }
        public string logo_file { get; set; }
        public string description { get; set; }
    }




/*
    [FirestoreData]
    public class Offer
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string JobTitle { get; set; }

        [FirestoreProperty]
        public string Company { get; set; }

        [FirestoreProperty]
        public string JobUrl { get; set; }
    }
*/