using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using AbotX.Parallel;
using AbotX.Poco;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml;
using HtmlAgilityPack.Samples;
using System.Text;
using System.Xml.Serialization;

namespace Abot.CryptoCrawler
{

    class Program 
    {
        private static string numberPattern = " ({0})";
        private static string urls = "";
       
        static void Main(string[] args)
        {
            //Lognet Method
            log4net.Config.XmlConfigurator.Configure();
            PrintDisclaimer();
        
            
            StartCrawlSitesAsync();

    
            Console.WriteLine("The end");

            PrintDisclaimer();

            Console.ReadLine();

        }
        //Crawl Instance
        private async static void StartCrawlSitesAsync()
        {
            
            string[] lines;

            string crawledUrl = null;
            string a =null;
            var siteToCrawlProvider = new SiteToCrawlProvider();
            var list = new List<string>();
            var fileStream = new FileStream("Sites.txt", FileMode.Open, FileAccess.Read);//open Sites.txt to read websites to be crawled
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    siteToCrawlProvider.AddSitesToCrawl(new List<SiteToCrawl>
                {
                    new SiteToCrawl {Uri = new Uri(line)
                }
                });// add sites to a list

                    Console.WriteLine(line);//display the read sites
                   
                    a = line;
                }


                    //Create the crawl engine instance
                    var crawlEngine = new ParallelCrawlerEngine(new ParallelImplementationContainer
                    {
                        SiteToCrawlProvider = siteToCrawlProvider
                    });

                    //Register for site level events
                    crawlEngine.AllCrawlsCompleted += (sender, eventArgs) =>
                    {

                        Console.WriteLine("Completed crawling all sites");
                    };
                    crawlEngine.SiteCrawlCompleted += (sender, eventArgs) =>
                    {
                        Console.WriteLine("Completed crawling site {0}", eventArgs.CrawledSite.SiteToCrawl.Uri);
                        
                        WebClient webClient = new WebClient();
                        Console.WriteLine(line);
                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        // Create Folder
                        // Specify a name for your top-level folder.
                        string folderName = @"c:\files\";

                        
                        // You can write out the path name directly instead of using the Combine
                        // method. Combine just makes the process easier.
                        string pathString = @"c:\files\CryptoCrawler\";
                        System.IO.Directory.CreateDirectory(pathString);
                        // You can extend the depth of your path if you want to.
                        //pathString = System.IO.Path.Combine(pathString, "SubSubFolder");

                        // Create the subfolder. You can verify in File Explorer that you have this
                        // structure in the C: drive.
                        //    Local Disk (C:)
                        //        Top-Level Folder
                        //            SubFolder
                        //System.IO.Directory.CreateDirectory(pathString);

                        // Create a file name for the file you want to create. 
                        //string fileName = System.IO.Path.GetRandomFileName();

                        // This example uses a random string for the name, but you also can specify
                        // a particular name.
                        string fileName = "NewFile.htm";

                        // Use Combine again to add the file name to the path.
                        //string path = NextAvailableFilename(pathString);

                        // Verify the path that you have constructed.
                        Console.WriteLine("Path to my file: {0}\n", pathString);

                        // Check that the file doesn't already exist. If it doesn't exist, create
                        // the file and write integers 0 - 99 to it.
                        // DANGER: System.IO.File.Create will overwrite the file if it already exists.
                        // This could happen even with random file names, although it is unlikely.
                       
                       
                         webClient.DownloadFileAsync((eventArgs.CrawledSite.SiteToCrawl.Uri), NextAvailableFilename(pathString +
                             fileName));
                        
                        List<UrlInfo> eCrawlResult = new List<UrlInfo>();
                        DateTime date = DateTime.Now; // will give the date for today
                        string dateWithFormat = date.ToLongDateString();
                        crawledUrl = eventArgs.CrawledSite.SiteToCrawl.Uri.ToString();
                        eCrawlResult.AddRange(new UrlInfo[] {
                        new UrlInfo((eventArgs.CrawledSite.SiteToCrawl.Uri).ToString(), (eventArgs.CrawledSite.SiteToCrawl.Id).ToString(), DateTime.Now.ToString() )});
                        string Path = (@"c:\files\");
                        //Stream stream = File.OpenWrite(Environment.CurrentDirectory + "\\mytext.txt");
                        XmlSerializer serial = new XmlSerializer(typeof(List<UrlInfo>));// serialize addresses to xml file
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(@"C:\files\url.xml", true);
                        urls = eventArgs.CrawledSite.SiteToCrawl.Uri.ToString();


                        serial.Serialize(writer, eCrawlResult);


                        writer.Close();


                        // Keep the console window open in debug mode.
                        //System.Console.WriteLine("Press any key to exit.");
                        //System.Console.ReadKey();
                        //webClient.DownloadFileAsync(eventArgs.CrawledSite.SiteToCrawl.Uri, @"c:\files\myfile.htm");

                    };
                    crawlEngine.CrawlerInstanceCreated += (sender, eventArgs) =>
                    {
                        //Register for crawler level events. These are Abot's events!!!
                        eventArgs.Crawler.PageCrawlCompleted += (abotSender, abotEventArgs) =>
                            {
                                Console.WriteLine("You have the crawled page here in abotEventArgs.CrawledPage...");
                               

                            };

                    };

                    await crawlEngine.StartAsync();
                
                    Test(crawledUrl);
                    Console.WriteLine("Press enter key to stop");
                    Console.Read();

                    //crawlEngine.Stop();
                    Console.WriteLine(line);
                
            }

        }
        
        public static string NextAvailableFilename(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern);
        }
        /// <summary>
        /// Get Next File
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string GetNextFilename(string pattern)
        {
            string tmp = string.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }

        
        /// <summary>
        /// Html to Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Test(string url)
        {

            string pathString = @"c:\files\CryptoCrawler\";
            string [] fileEntries = Directory.GetFiles(pathString);
           
                    ProcessDirectory(pathString);
            
        }
        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);
              
            string latestfile = "";
            DateTime lastupdated = DateTime.MinValue;
            var files = new DirectoryInfo(targetDirectory).GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                    //Console.WriteLine("Hey I am not null"+latestfile);

                }
            }
            ProcessFile((targetDirectory+latestfile));
            Console.WriteLine(targetDirectory+latestfile);
            
        }
        public static void ProcessTextDirectory(string targetDirectory)
        {
            // Process the latest list of files found in the directory.

           string latestfile = "";
            var files = new DirectoryInfo(targetDirectory).GetFiles("*.*");
            Console.WriteLine(targetDirectory);
            DateTime lastupdated = DateTime.MinValue;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > lastupdated)
                {
                    lastupdated = file.LastWriteTime;
                    latestfile = file.Name;
                    
                }
               
            }
            ProcessTextFile(targetDirectory + latestfile);


        }

        // Convert downloaded HTML pages to Text file

        public static void ProcessFile(string path)
        {
            HtmlToText htt = new HtmlToText();
            Console.WriteLine("Processed file '{0}'.", path);
            string s = htt.Convert(path);
            string pathString = @"c:\files\CryptoCrawler\Text\";
            string fileName = "NewFile.txt";
            StreamWriter sw = new StreamWriter(NextAvailableFilename(pathString +
                             fileName));
            sw.Write(s);
            sw.Flush();
            sw.Close();
            //Get Converted Text files
            ProcessTextDirectory(pathString);

        }
        //Scrape BTC Addresses from text files
        public static void ProcessTextFile(string path)
        {
            
        Regex g = new Regex("[13][a-km-zA-HJ-NP-Z0-9]{26,33}");
        var regex = new Regex("[13][a-km-zA-HJ-NP-Z0-9]{26,33}");
        string search = "[13][a-km-zA-HJ-NP-Z0-9]{26,33}";
            using (StreamReader r = new StreamReader(path))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    // X.
                    // Try to match each line against the Regex.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                    Match m = g.Match(line);
                    MatchCollection addresses = Regex.Matches(line, search);// use matchcollection to match addresses
                    List<Addresses> eCrawlResult = new List<Addresses>();

                    if (addresses.Count > 0) //Count No.of addresses
                    {
                        for (int a = 0; a < addresses.Count; a++)
                        {

                            Console.WriteLine(addresses[a] + " " + "is Address No." + " " + a);// Display parsed addresses
                            DateTime date = DateTime.Now; // will give the date for today
                            string dateWithFormat = date.ToLongDateString();
                            string v = m.Groups[1].Value;
                            eCrawlResult.AddRange(new Addresses[] {
                                new Addresses(addresses[a].ToString(), v, urls, DateTime.Now.ToString())});
                            string path1 = (@"c:\files\");
                            //Stream stream = File.OpenWrite(Environment.CurrentDirectory + "\\myaddress.txt");
                            XmlSerializer serial = new XmlSerializer(typeof(List<Addresses>));// serialize addresses to xml file
                            System.IO.StreamWriter writer = new System.IO.StreamWriter(@"C:\files\address.xml", true);
                            serial.Serialize(writer, eCrawlResult);
                            writer.Close();
                        }
                        ///////////////To be continued///////////////////////////////////eCrawlResult.Add(new Addresses() {Address =v });
                        //Console.WriteLine("\t" + v);
                    }
                }
            }
        }
        private static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = e.ProgressPercentage;
        }

        private static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Complete");
        }
   
        private static void PrintDisclaimer()
        {
            Console.WriteLine("Crawling");
        }


    }
}
