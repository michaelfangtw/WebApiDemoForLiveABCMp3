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
    /// LiveABC 課文講解MP3 下載網址解析
    /// </summary>
    public class LiveExplainController : LiveDownloadBase
    {


        // GET api/LiveExplain/201505
        [Route("api/LiveExplain/{yyyymm}")]
        public IEnumerable<string> Get(string yyyymm)
        {
            //http://www.liveabc.com/site/Online_Store/magazine_download/live_explain.asp?select1={{yyyy}}/{{M}}%26Page={{page}}
            DownloadUrlTemplate = GetDownloadUrlTemplate("LiveABC.Explain.Url");
            DownloadYearMonth = yyyymm;
            PageCount = 4;
            IEnumerable<string> fileList = GetFileUrlList(DownloadYearMonth, DownloadUrlTemplate, PageCount);
            return fileList;
        }
    }
}
