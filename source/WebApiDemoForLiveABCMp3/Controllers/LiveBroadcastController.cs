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
    /// LiveABC 線上廣播MP3 下載網址解析
    /// </summary>
    public class LiveBroadcastController : LiveDownloadBase
    {


        [Route("api/LiveBroadcast/{yyyymm}")]
        public IEnumerable<string> Get(string yyyymm)
        {
            DownloadUrlTemplate = GetDownloadUrlTemplate("LiveABC.Broadcast.Url");
            DownloadYearMonth = yyyymm;
            PageCount = 5;//沒有分頁
            FileType = "zip";
            IEnumerable<string> fileList = GetFileUrlList(DownloadYearMonth, DownloadUrlTemplate, PageCount, FileType);
            return fileList;
        }
    }
}
