using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abot.CryptoCrawler
{
    public class BrowserSession
    {
        private bool _isPost;
        private HtmlDocument _htmlDoc;

        /// <summary>
        /// System.Net.CookieCollection. Provides a collection container for instances of Cookie class 
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Provide a key-value-pair collection of form elements 
        /// </summary>
        public FormElementCollection FormElements { get; set; }

        /// <summary>
        /// Makes a HTTP GET request to the given URL
        /// </summary>
        public string Get(string url)
        {
            _isPost = false;
            CreateWebRequestObject().Load(url);
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        public string getAddress(string body)
{
            String charset = null;
            var webGet = new HtmlWeb();

            //HtmlNode newNode = body.DocumentNode.SelectSingleNode("^[13][a-km-zA-HJ-NP-Z1-9]{25,34}");
            //find expression from : Get Bitcoin Address
            Console.WriteLine(body);
            string pattern = "^[13][a-km-zA-HJ-NP-Z0-9]{26,33}$";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(body);

            if (!rgx.IsMatch(body))
            {
                //Console.WriteLine("No Address");
            } else
            {
                Console.WriteLine(body);
                //charset = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? null : match.Groups[2].Value;
                //Console.WriteLine(charset);
            }

            Console.WriteLine(matches.Count);
               if (matches.Count > 0)
                {
                    Console.WriteLine(body);
                    charset = body;
                    
                    Console.WriteLine("{0} ({1} matches):", body, matches.Count);
                    foreach (Match a in matches)
                       Console.WriteLine("   " + a.Value);
                    Console.WriteLine("Carol");
                }
            
            Match match = Regex.Match(body, "^[13][a-km-zA-HJ-NP-Z0-9]{26,33}$", RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                Console.WriteLine(body);
                charset = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? null : match.Groups[2].Value;
                Console.WriteLine(charset);
            }
            
            return body;
}
        public MemoryStream GetRawData(WebResponse webResponse)
        {
            MemoryStream rawData = new MemoryStream();

            try
            {
                using (Stream rs = webResponse.GetResponseStream())
                {
                    byte[] buffer = new byte[1024];
                    int read = rs.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        rawData.Write(buffer, 0, read);
                        read = rs.Read(buffer, 0, buffer.Length);

                    }
                }
            }
            catch (Exception e)
            {
                //_logger.WarnFormat("Error occurred while downloading content of url {0}", webResponse.ResponseUri.AbsoluteUri);
                //_logger.Warn(e);
            }

            return rawData;

        }

        /// <summary>
        /// Makes a HTTP POST request to the given URL
        /// </summary>
        public string Post(string url)
        {
            _isPost = true;
            CreateWebRequestObject().Load(url, "POST");
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Creates the HtmlWeb object and initializes all event handlers. 
        /// </summary>
        private HtmlWeb CreateWebRequestObject()
        {
            HtmlWeb web = new HtmlWeb();
            web.UseCookies = true;
            web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);
            web.PostResponse = new HtmlWeb.PostResponseHandler(OnAfterResponse);
            web.PreHandleDocument = new HtmlWeb.PreHandleDocumentHandler(OnPreHandleDocument);
            return web;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
        /// </summary>
        protected bool OnPreRequest(HttpWebRequest request)
        {
            AddCookiesTo(request);               // Add cookies that were saved from previous requests
            if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request
            return true;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PostResponseHandler. Occurs after a HTTP response is received
        /// </summary>
        protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
        {
            SaveCookiesFrom(response); // Save cookies for subsequent requests
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreHandleDocumentHandler. Occurs before a HTML document is handled
        /// </summary>
        protected void OnPreHandleDocument(HtmlDocument document)
        {
            SaveHtmlDocument(document);
        }

        /// <summary>
        /// Assembles the Post data and attaches to the request object
        /// </summary>
        private void AddPostDataTo(HttpWebRequest request)
        {
            string payload = FormElements.AssemblePostPayload();
            byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
            request.ContentLength = buff.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream reqStream = request.GetRequestStream();
            reqStream.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Add cookies to the request object
        /// </summary>
        private void AddCookiesTo(HttpWebRequest request)
        {
            if (Cookies != null && Cookies.Count > 0)
            {
                request.CookieContainer.Add(Cookies);
            }
        }

        /// <summary>
        /// Saves cookies from the response object to the local CookieCollection object
        /// </summary>
        private void SaveCookiesFrom(HttpWebResponse response)
        {
            if (response.Cookies.Count > 0)
            {
                if (Cookies == null) Cookies = new CookieCollection();
                Cookies.Add(response.Cookies);
            }
        }

        /// <summary>
        /// Saves the form elements collection by parsing the HTML document
        /// </summary>
        private void SaveHtmlDocument(HtmlDocument document)
        {
            _htmlDoc = document;
            //FormElements = new FormElementCollection(_htmlDoc);
        }

        public class FormElementCollection : Dictionary<string, string>
        {
            /// <summary>
            /// Constructor. Parses the HtmlDocument to get all form input elements. 
            /// </summary>
           /* public FormElementCollection(HtmlDocument htmlDoc)
            {
                var inputs = htmlDoc.DocumentNode.Descendants("input");
                foreach (var element in inputs)
                {
                    string name = element.GetAttributeValue("name", "undefined");
                    string value = element.GetAttributeValue("value", "");
                    if (!name.Equals("undefined")) Add(name, value);
                }
            }*/

            /// <summary>
            /// Assembles all form elements and values to POST. Also html encodes the values.  
            /// </summary>
            public string AssemblePostPayload()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var element in this)
                {
                    string value = System.Web.HttpUtility.UrlEncode(element.Value);
                    sb.Append("&" + element.Key + "=" + value);
                }
                return sb.ToString().Substring(1);
            }
        }
    }
}
