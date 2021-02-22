# Job Search Backend
Take-home challenge for Pathrise.

To get started, download the entire contents of the release.zip from [here.](https://github.com/PranavSakthivel/pathrise-backend/releases)


# Instructions

 1. Extract the contents of the release zip to a folder.
 2. Copy files to the program directory, named `job-opportunities.csv` and `jobBoards.json` for the job offers csv and job boards list respectively.
 3. Copy the firebase config json file sent in the email as that is needed for database authentication.
 4. Open a command prompt in the folder.
 5. run `pathrise-backend.exe`
 6. If you want to write to database (which takes around 2 hours to write 20,000 entries to firebase), run `pathrise-backend.exe write` instead.

## Frontend

 - https://pathrise.azurewebsites.net

## Code workings

Backend: 
 - Private maps are used to store the job count per board as well as the job boards themselves. Maps have O(1) read access so they are very efficient to look up job board by key (the job website).
 - Firebase is used as the database of choice. The data we are storing is simple and minimal. The querying done is also fairly simple, and Firebase offers a generous free quota and good integration with Angular (used for front-end), as both are Google owned.
 - Paths for files are stored as variables for easy extensibility, if those needed to be made as parameters or grabbed from another source instead of hardcoding them in.
 - The Job Source is determined by parsing the URL and splitting the root domain (ex. www.google.com) with a `.` as the delimiter. This allows us to get just the last 2 parts (google.com) and match them with the job boards list.
 - The data is then added to firestore, the resolution CSV, and the count of the job per job board is incremented by 1 in the map.

Frontend:

 - Angular is used as the frontend of choice. It is feature rich and I have been working with it a little at the time, so I thought I would use the opportunity to learn it better and master it through this project.
 - The Job Boards and Jobs are pulled from Firestore and displayed as Material Cards.

## Third party libraries
Backend:
 - CsvHelper - Very helpful to read and write CSV files in C# in an organized manner.
 - Newtonsoft.JSON - helpful library to read in the job_boards.json and extract needed info
 - Google Cloud Firestore - Necessary to add files to Cloud Firestore database

Frontend:
 - Angular Material components for design
 - Angular Firebase (AngularFire) to access data from Cloud Firestore

## Limitations

 Due to time constraints and free database quota constraints, I was unable to include some planned features and fixes.

Frontend:
 - Only 200 jobs are pulled in per board - this is because otherwise the daily read quota on Firebase skyrockets after a couple page refreshes, as AngularFire offers no easy way to bulk read or read selectively from Firestore.

Backend:
 - Only 10000 jobs can be added to the firestore via the backend program - Firestore is a little glitchy with its quotas and cut off my program after 10k writes, and I was unable to test more as it took hours each time I had to test and the daily write limit to Firestore was 20000. 

## Considerations/Improvements

Given more time and (increased) database quota, this is what I would add:

Backend:
 - **Location detection** - Crawl through the provided job URL looking for locations, and save that to the database as well to enable location-based querying in the frontend.
 - **JUnit testing** - unable to set this up due to time constraints
 - Determine if URL is valid - I have partially done this in my program but it is not fleshed out. There is a segment that checks for URL validity. C# has additional libraries to test this, I wanted to implement a quick check to add that info into the database too. There is a possibility to detect a 404 redirect as well if the job link worked, but the job didn't exist.
 - Customizable parameters - being able to provide own database connection, CSV and JSON files with different names
 - Cleaner flow of code - some things can be taken out of methods or put into helper methods
 - Possible bulk write - reduce number of individual writes with the help of a bulk write method or database that supports it
 
 Frontend:
 - Better design - the lists and views definitely have room for improvement, they are enough to be functional for now
 - Proper pagination based queries - query the database every time the user moves to the next page of entries, reducing number of reads/reading on a need basis
 - Searching by location functionality as explained above

# Visualization of Job Data
Company Website | 1157
Unknown | 4544
Google | 160
Glassdoor | 295
AngelList | 120
LinkedIn | 6568
Dribble | 0
Indeed | 891
J's Easy Apply | 0
Triplebyte | 13
Hired | 0
Wayup | 0
YCombinator Jobs | 0
Work At A Startup | 4
Jopwell | 0
Tech Ladies | 2
Intern Supply | 0
Underdog | 0
Stella | 0
ZipRecruiter | 64
SimplyHired | 27
Gamasutra | 0
Huntr Jobs | 0
Lever | 2658
Greenhouse | 3039
Monster | 4
Github | 0
Stackoverflow | 2
Employbl | 0
Who Is Hiring? | 0
Jobvite | 382
SmartRecruiters | 69
Government Jobs | 1

