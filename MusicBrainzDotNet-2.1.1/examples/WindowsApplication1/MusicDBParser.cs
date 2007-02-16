using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using CarlosAg.ExcelXmlWriter;
using musicbrainz;

namespace MusicDataminer
{
    class MusicDBParser
    {
        private Form1 iForm;

        public struct Country
        {
            public string acronym;
            public string name;
            public int gdpPerCapita;
        };

        public struct MusicBrainzAlbum
        {
            public Country country;
            public int date;
        };

        public struct Album
        {
            public string title;

            // probably store it as pointer :) ??
            public string artist;
            public string style;

            public List<MusicBrainzAlbum> releases;
        };

        // private attributes
        private List<Album> albums = new List<Album>();
        private Hashtable countries = new Hashtable();

        //Constructor
        public MusicDBParser( Form1 aForm ) 
        {
            iForm = aForm;
            this.ParseCountries("../../../countries_acronyms.txt");
        }

        public bool GetMusicBrainzReleases(string artist, string albumName, MusicBrainz o,
            List<MusicBrainzAlbum> releasesList, out string retrievedName)
        {
            retrievedName = albumName;
            bool foundRelevantRelease = false;


            
            //Console.WriteLine("Searching for occurrences for: " + artist + " / " + albumName);
            iForm.PrintLine("Searching for occurrences for: " + artist + " / " + albumName);
            bool ret = o.Query(MusicBrainz.MBQ_FileInfoLookup, new String[] { "", artist, albumName, "", "", "" });

            // Select the first album
            o.Select(MusicBrainz.MBS_SelectLookupResult, 1);

            string type;
            o.GetResultData(MusicBrainz.MBE_LookupGetType, out type);
            string fragment;
            o.GetFragmentFromURL(type, out fragment);

            // iterate through all the results
            o.Select(MusicBrainz.MBS_Rewind);

            if (!o.Select(MusicBrainz.MBS_SelectLookupResult, 1))
            {
                return foundRelevantRelease;
            }

            // NOTE: must be done before the next Select
            int relevance = o.GetResultInt(MusicBrainz.MBE_LookupGetRelevance);

            // if not sure about it, quit
            if (relevance < 80)
            {
                return foundRelevantRelease;
            }

            // select the album
            o.Select(MusicBrainz.MBS_SelectLookupResultAlbum);

            o.GetResultData(MusicBrainz.MBE_AlbumGetAlbumName, out retrievedName);

            int nReleases = o.GetResultInt(MusicBrainz.MBE_AlbumGetNumReleaseDates);

            if (nReleases != 0)
            {
                albumName = retrievedName;

                for (int i = 1; i <= nReleases; i++)
                {
                    if (o.Select(MusicBrainz.MBS_SelectReleaseDate, i))
                    {
                        foundRelevantRelease = true;

                        string country;
                        o.GetResultData(MusicBrainz.MBE_ReleaseGetCountry, out country);

                        string date;
                        o.GetResultData(MusicBrainz.MBE_ReleaseGetDate, out date);

                        // add it to the list
                        MusicBrainzAlbum release = new MusicBrainzAlbum();

                        if (countries.ContainsKey(country.ToUpper()))
                        {
                            release.country = (Country)countries[country];
                            release.date = int.Parse(date.Substring(0, 4));
                            releasesList.Add(release);
                        }

                    }

                    o.Select(MusicBrainz.MBS_Back);
                }
            }

            return foundRelevantRelease;
        }

        private static void WriteSomeStuff()
        {
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets.Add("Sample");
            WorksheetRow row = sheet.Table.Rows.Add();

            // Header
            row.Cells.Add("Artist");
            row.Cells.Add("Album/Release");
            row.Cells.Add("Similar Artist");
            row.Cells.Add("Date");
            row.Cells.Add("Country");

            // Rows
            row = sheet.Table.Rows.Add();
            row.Cells.Add("Pink Floyd");
            row.Cells.Add("The Division Bell");
            row.Cells.Add("Someone :)");
            row.Cells.Add("2000-10-05");
            row.Cells.Add("SE");

            book.Save(@"../../../test.xls");

            //string fileName = "test.txt";  // a sample file name

            //// Delete the file if it exists.
            //if (File.Exists(fileName))
            //{
            //    File.Delete(fileName);
            //}

            //// Create the file.
            //FileStream fs = File.Create(fileName, 1024);

            //// Add some information to the file.
            //byte[] info = new System.Text.UTF8Encoding(true).GetBytes("This is some text in the file.");
            //fs.Write(info, 0, info.Length);

            //// Open the file and read it back.
            //StreamReader sr = File.OpenText(fileName);
            //string s = "";
            //while ((s = sr.ReadLine()) != null) 
            //{
            //    System.Console.WriteLine(s);
            //}
        }

        public void ParseCountries(string filename)
        {
            // Open the file and read it back.
            StreamReader sr = File.OpenText(filename);
            string text = "";
            while ((text = sr.ReadLine()) != null)
            {
                char[] delimiterChars = { '\t' };

                string[] words = text.Split(delimiterChars);

                if (words.Length > 2)
                {
                    Country country;
                    country.acronym = words[0].ToUpper();
                    country.name = words[1];
                    country.gdpPerCapita = int.Parse(words[2]);

                    if (!countries.ContainsKey(country.acronym))
                    {
                        countries.Add(country.acronym, country);
                    }
                }
            }
        }

        ParseMusicStyleDelegate iParseMusicStyleDelegate;

        private delegate void ParseMusicStyleDelegate(string style, MusicBrainz queryObject);
        private void ParseMusicStyleCallback(IAsyncResult ar)
        {
            Console.WriteLine("DONE");
        }

        public void AsyncParseByStyle(string style, MusicBrainz queryObject) 
        {
            iParseMusicStyleDelegate = new ParseMusicStyleDelegate( ParseMusicStyle );
            AsyncCallback callback = new AsyncCallback( ParseMusicStyleCallback );
            iParseMusicStyleDelegate.BeginInvoke( style, queryObject, callback, 123456789 );
        }

        public void ParseMusicStyle(string style, MusicBrainz queryObject)
        {
            // Open the file and read it back.
            StreamReader sr = File.OpenText("../../../" + style + ".txt");
            string text = "";
            
            int lineNumber = 0;
            while ((text = sr.ReadLine()) != null && iForm.queryOnGoing )
            {
                char[] delimiterChars = { '\t' };

                string[] tokens = text.Split(delimiterChars);

                Album album = new Album();

                // Lowercase all the tokens
                album.artist = tokens[0].ToLower();
                album.title = tokens[1].ToLower();

                if (tokens.Length < 3 || tokens[2].Trim().Length == 0)
                {
                    album.style = style;
                }
                else
                {
                    album.style = tokens[2].ToLower();
                }

                // search for info in the MusicBrainz DB
                List<MusicBrainzAlbum> releases = new List<MusicBrainzAlbum>();
                string retrievedName;
                bool foundSomething = GetMusicBrainzReleases(album.artist, album.title, queryObject,
                    releases, out retrievedName);

                if (foundSomething)
                {
                    album.title = retrievedName;
                    album.releases = releases;
                    albums.Add(album);

                    //Console.WriteLine("Added Album: " + album.title + " Artist: " + album.artist + ": ");
                    iForm.PrintLine("Added Album: " + album.title + " Artist: " + album.artist + ": ");
                    foreach (MusicBrainzAlbum release in album.releases)
                    {
                        //Console.WriteLine("\tCountry: " + release.country.name + "  Date: " + release.date);
                        iForm.PrintLine("\tCountry: " + release.country.name + "  Date: " + release.date);
                    }
                }
                lineNumber++;
            }

            //query was interrupted
            if (!iForm.queryOnGoing)
            {
                iForm.PrintLine("Stopped at line: " + lineNumber);
            }
            //end ok
            else 
            {
                iForm.PrintLine("Queried lines: " + lineNumber);
            }
        }

    }
}