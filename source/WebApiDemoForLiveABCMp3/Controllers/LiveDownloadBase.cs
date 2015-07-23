using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace WebApiDemoForLiveABCMp3.Controllers
{
    /// <summary>
    /// LiveABCMp3List
    /// </summary>
    public class LiveDownloadBase : ApiController
    {
        public string DownloadUrlTemplate { get; set; }

        public string DownloadYearMonth { get; set; }

        public string AuthUrl
        {
            get
            {
                return string.Format("{0}", ConfigurationManager.AppSettings["LiveABC.AuthUrl"]);
            }
        }

        public string UserID
        {
            get
            {
                return string.Format("{0}", ConfigurationManager.AppSettings["LiveABC.UserID"]);
            }
        }

        public string Password
        {
            get
            {
                return string.Format("{0}", ConfigurationManager.AppSettings["LiveABC.Password"]);
            }
        }

        public int PageCount { get; set; }

        public string FileType { get; set; }
        public HttpWebRequest webRequest { get; set; }



        CookieContainer cookies = new CookieContainer();
        /// <summary>
        /// 由設定檔取得urltemplate
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetDownloadUrlTemplate(string key)
        {
            string url = string.Format("{0}", ConfigurationManager.AppSettings[key]);
            url = url.Replace("%26", "&");
            return url;
        }

        private void Login()
        {
            webRequest = WebRequest.Create(AuthUrl) as HttpWebRequest;
            //登入Liveabc網址                
            string postString = string.Format("lgck={0}&email={1}&pwd={2}", "yes", UserID, Password);
            const string contentType = "application/x-www-form-urlencoded";
            System.Net.ServicePointManager.Expect100Continue = false;

            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.CookieContainer = cookies;
            webRequest.ContentLength = postString.Length;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding("BIG5"));
            string responseData = responseReader.ReadToEnd();

            responseReader.Close();
            webRequest.GetResponse().Close();

        }

        public List<string> GetPageList(string downloadYearMonth, string downloadUrlTemplate, int pageCount)
        {
            string yyyymmdd = string.Format("{0}/{1}/01", downloadYearMonth.Substring(0, 4), downloadYearMonth.Substring(4, 2));
            DateTime yMd = DateTime.Parse(yyyymmdd);
            string yyyy = yMd.Year.ToString();
            string M = yMd.Month.ToString();
            string urlMonth = downloadUrlTemplate;
            const string patternYear = @"{{yyyy}}";
            var regexYear = new Regex(patternYear);
            urlMonth = regexYear.Replace(urlMonth, yyyy);

            const string patternMonth = @"{{M}}";
            var regexMonth = new Regex(patternMonth);
            urlMonth = regexMonth.Replace(urlMonth, M);
            string urlTemplate = urlMonth;
            const string pattenPage = @"{{page}}";
            List<string> pageList = new List<string>();
            if (pageCount == 0)
            {
                pageList.Add(urlTemplate);

            }
            else
            {
                for (var i = 1; i <= pageCount; i++)
                {
                    var regexPage = new Regex(pattenPage);
                    string urlPage = regexPage.Replace(urlTemplate, i.ToString());
                    if (!pageList.Contains(urlPage))
                        pageList.Add(urlPage);
                }
            }
            return pageList;
        }


        public IEnumerable<string> GetFileUrlList(string downloadYearMonth, string downloadUrlTemplate, int pageCount, string filetype = "zip")
        {

            //login LiveABC
            Login();

            List<string> resultList = null;
            if (DownloadYearMonth.Length == 6)
            {
                List<string> pageList = GetPageList(downloadYearMonth, downloadUrlTemplate, pageCount);
                List<String> mp3List = new List<String>();

                foreach (var pageUrl in pageList)
                {
                    StreamReader readerPage = GetWebPageToStream(pageUrl);
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(readerPage);
                    mp3List = ExtractAllFileType(doc, filetype);
                    if (resultList == null)
                    {
                        resultList = mp3List;
                    }
                    else
                    {
                        mp3List.ForEach(x => { if (!resultList.Contains(x)) resultList.Add(x); });
                    }
                }
                return resultList.ToList();
            }
            else
            {
                return resultList.ToList();
            }

        }

        StreamReader GetWebPageToStream(string url)
        {
            //           cookies = webRequest.CookieContainer;
            webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.CookieContainer = cookies;
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.1) Gecko/2008070208 Firefox/3.0.1";
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //webRequest.Referer = " http://www.liveabc.com/site/Online_Store/magazine_download/for_all.asp";
            StreamReader readerPage = new StreamReader(webRequest.GetResponse().GetResponseStream(), Encoding.GetEncoding("BIG5"));
            //string responsePageFromServer = readerPage.ReadToEnd();
            return readerPage;
        }


        private List<string> ExtractAllFileType(HtmlDocument htmlSnippet, string filetype = "zip")
        {
            List<string> hrefTags = new List<string>();

            HtmlNodeCollection htmlNodes = htmlSnippet.DocumentNode.SelectNodes("//a[@href]");
            if (htmlNodes != null)
            {
                foreach (HtmlNode link in htmlNodes)
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value.IndexOf(filetype) > 0)
                    {
                        hrefTags.Add(att.Value);
                    }
                }
            }
            return hrefTags;
        }

    }
}
