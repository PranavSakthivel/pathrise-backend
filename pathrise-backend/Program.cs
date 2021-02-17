using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Globalization;
using System.IO;
using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pathrise_backend
{
    class Program
    {
        private static Dictionary<string, string> BoardMap = new Dictionary<string, string>();
        static async Task Main(string[] args)
        {
            // Variables for paths
            string OpportunitiesFilePath, jobBoardsPath, ProjectName;
            ProjectName = "pathrise-6a289";
            OpportunitiesFilePath = "job_opportunities.csv"; //CHANGE!
            jobBoardsPath = "jobBoards.json";

            //Connect to Firestore
            FirestoreDb db = ConnectToDb(ProjectName);

            // Open opportunities.csv
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(OpportunitiesFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open file: " + OpportunitiesFilePath);
                Console.WriteLine(e); //debug
                Environment.Exit(-1);
            }


            CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await BuildBoardList(jobBoardsPath, db);
            await ParseCSV(csv, db);
        }

        static StreamReader GetFileReader(string filePath)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
            } catch (Exception)
            {
                Console.WriteLine("Unable to open file: " + filePath);
                Environment.Exit(-1);
            }
            return reader;
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
        static async Task ParseCSV(CsvReader csv, FirestoreDb db)
        {
            
            csv.Read();
            //csv.ReadHeader(); //needed?
            while (csv.Read())
            {
                Offer record = csv.GetRecord<Offer>();
                Dictionary<string, object> offer = new Dictionary<string, object>
                {
                    { "JobTitle", record.JobTitle},
                    { "Company", record.Company },
                    { "JobUrl", record.JobUrl },
                    { "JobSource", FindJobSource(record.JobUrl, record.Company) } //TODO: UPDATE
                };
                // await db.Collection("offers").Document(record.Id.ToString()).SetAsync(offer);
                Console.WriteLine("Added Offer: " + record.Id + ", " + record.Company + ", " + record.JobUrl + 
                    ", " + FindJobSource(record.JobUrl, record.Company)); // debug
            }
        }

        static async Task BuildBoardList(string jobBoardsPath, FirestoreDb db)
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

            string json = reader.ReadToEnd();
            JObject boards = JObject.Parse(json);

            foreach(JObject b in boards["job_boards"])
            {
                // Add urls to local data structure to get source for job offers
                try
                {
                    BoardMap.Add(b["root_domain"].ToString(), b["name"].ToString());
                }
                catch (ArgumentException)
                {
                    BoardMap[b["root_domain"].ToString()] = b["name"].ToString(); //check logic
                }
                

                // Add to database 
                Dictionary<string, object> board = new Dictionary<string, object>
                {
                    { "name", b["name"].ToString()},
                    { "rating",  b["rating"].ToString()},
                    { "root_domain",  b["root_domain"].ToString()},
                    { "logo_file",  b["logo_file"].ToString()}, 
                    { "description",  b["description"].ToString()}
                };

                // await db.Collection("boards").Document(b["name"].ToString()).SetAsync(board);
                Console.WriteLine("Added job board: " + b["name"] + ", " + b["root_domain"]); //debug
            }
        }

        static string FindJobSource(string Url, string CompanyName)
        {
            // No url given
            if (Url == "" || Url == null) return "Unknown";

            string host;
            Uri myUri;
            
            // Attempt to parse as actual URL
            try
            {
                myUri = new Uri(Url);
                host = myUri.Host;
            } catch (UriFormatException)
            {
                // Case: invalid URL
                return "Unknown";
            }

            // Good place to check if the URL is valid
            // 

            // Split URL by "."
            string[] hostArray = myUri.Host.Split('.');
            int n = hostArray.Length;

            // Find a better way to check if actual URL
            if (n < 2) return "Unknown";

            // Console.WriteLine(hostArray[n - 2].ToLower() + "." + hostArray[n - 1].ToLower()); // debug

            // Case: Job Board site
            string JobBoard;
            if (BoardMap.TryGetValue(hostArray[n - 2].ToLower() + "." + hostArray[n - 1].ToLower(), out JobBoard))
            {
                // Console.WriteLine(JobBoard); // debug
                return JobBoard;
            }

            // Case: Company website
            if (hostArray[n - 2].ToLower() == CompanyName.ToLower()) return "Company Website";


            // Case: none of these
            return "Unknown";
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

