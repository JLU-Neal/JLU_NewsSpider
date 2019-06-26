using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace _54160530_Spider
{
    public class CrawlerController : Controller
    {
        public static void Main(string[]ar)
        {
            CrawlerController c = new CrawlerController();
            c.Index();
        }
        // GET: Crawler
        public void Index()
        {
            //抓取整本小说
            CrawlerController cra = new CrawlerController();// 顶点抓取小说网站小说
            //string html = cra.HttpGet("http://www.23us.so/files/article/html/13/13655/index.html", "");
            //string html = cra.HttpGet("http://www.baidu.com", "");
            string html = cra.HttpGet("https://www.jlu.edu.cn/index/tzgg.htm", "");
            //Console.WriteLine(html);
            
            // 获取小说名字
            //Match ma_name = Regex.Match(html, @"<meta name=""keywords"".+content=""(.+)""/>");
            //string name = ma_name.Groups[1].Value.ToString().Split(',')[0];
            /*
            // 获取章节目录
            Regex reg_mulu = new Regex(@"<table cellspacing=""1"" cellpadding=""0"" bgcolor=""#E4E4E4"" at"">(.|\n)*?</table>");
            var mat_mulu = reg_mulu.Match(html);
            string mulu = mat_mulu.Groups[0].ToString();
            */
            //截取
            int startIndex = html.IndexOf(@"<ul class=""list fl"">");
            int endIndex = html.IndexOf("</ul>", startIndex);
            html = html.Substring(startIndex,endIndex-startIndex);
            

            // 匹配a标签里面的url
            html.Replace("\n", "");
            Regex tmpreg = new Regex(@"<a href=""../info/\d{4,}/\d{5,}.htm"">", RegexOptions.Compiled);
            MatchCollection sMC = tmpreg.Matches(html);
            //<li id="lineu11_0">< a href = "../info/1095/45890.htm" > 长春吉大附中实验学校(高中)公开招聘教师的启事 </ a >< span > 2019 - 06 - 24 </ span ></ li >


            //提取URL
            ArrayList URLS = new ArrayList();
            for (int i = 0; i < sMC.Count;i++)
            {

                Console.WriteLine(sMC[i].Groups[0].Value);
                int startInd = sMC[i].Groups[0].Value.IndexOf("/info/");
                int len = sMC[i].Groups[0].Value.Length-startInd-2;
                String subURL = sMC[i].Groups[0].Value.Substring(startInd, len);
                String prefix = "https://www.jlu.edu.cn";
                String completedURL = prefix + subURL;
                URLS.Add(completedURL);
                
         
            }
            //依次对所有URL执行操作
            foreach(String i in URLS)
            {
                Console.WriteLine(i);
                processURL(i);
            }


            //保存网页内容
            
            FileStream fs = new FileStream(@"\samole\sample.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs);
            sr.WriteLine(html);// 开始写入值
            /*
            if (sMC.Count != 0)
            {
                //循环目录url，获取正文内容
                for (int i = 0; i < sMC.Count; i++)
                {
                    //sMC[i].Groups[1].Value
                    //0是<a href="http://www.23us.so/files/article/html/13/13655/5638725.html">第一章 泰山之巅</a> 
                    //1是http://www.23us.so/files/article/html/13/13655/5638725.html
                    //2是第一章 泰山之巅

                    // 获取章节标题
                    string title = sMC[i].Groups[2].Value;

                    // 获取文章内容
                    //string html_z = cra.HttpGet(sMC[i].Groups[1].Value, "");

                    // 获取小说名字,章节中也可以查找名字
                    //Match ma_name = Regex.Match(html, @"<meta name=""keywords"".+content=""(.+)"" />");
                    //string name = ma_name.Groups[1].Value.ToString().Split(',')[0];

                    // 获取标题,通过分析h1标签也可以得到章节标题
                    //string title = html_z.Replace("<h1>", "*").Replace("</h1>", "*").Split('*')[1];

                    // 获取正文
                    Regex reg = new Regex(@"<dd contents"">(.|\n)*?</dd>");
                    MatchCollection mc = reg.Matches(html_z);
                    var mat = reg.Match(html_z);
                    string content = mat.Groups[0].ToString().Replace("<dd id=\"contents\">", "").Replace("</dd>", "").Replace("&nbsp;", "").Replace("<br />", "\r\n");
                    // txt文本输出
                    string path = Directory.GetCurrentDirectory()+"\\";
                    Novel(title + "\r\n" + content, name, path);
                }
            }
            */
        }

        /// <summary>
        /// 创建文本
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="name">名字</param>
        /// <param name="path">路径</param>
        public void Novel(string content, string name, string path)
        {
            string Log = content + "\r\n";
            // 创建文件夹，如果不存在就创建file文件夹
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            // 判断文件是否存在，不存在则创建
            if (!System.IO.File.Exists(path + name + ".txt"))
            {
                FileStream fs1 = new FileStream(path + name + ".txt", FileMode.Create, FileAccess.Write);// 创建写入文件 
                //FileStream fs1 = new FileStream("sample.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(Log);// 开始写入值
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(path + name + ".txt" + "", FileMode.Append, FileAccess.Write);
                //FileStream fs = new FileStream("sample.txt", FileMode.Create, FileAccess.Write);

                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(Log);// 开始写入值
                sr.Close();
                fs.Close();
            }
        }

        public string HttpPost(string Url, string postDataStr)
        {
            CookieContainer cookie = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            HttpWebResponse response;
            request.ContentType = "text/html;charset=utf-8";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)request.GetResponse();
            }

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        public void processURL(String url)
        {
            String html=HttpGet(url, "");
            //匹配标题
            Regex tag_title = new Regex(@"<title>\S*</title>");
            var mat_mulu = tag_title.Match(html);
            string title = mat_mulu.Groups[0].ToString();
            title=title.Replace("<title>", "");
            title=title.Replace("</title>", "");
            Console.WriteLine(title);

            //匹配日期
            Regex date = new Regex(@"\d\d\d\d-\d\d-\d\d");
            var mat_date = date.Match(html);
            string datestr = mat_date.Groups[0].ToString();
            Console.WriteLine(datestr);

            //匹配正文
            Regex text = new Regex(@"<div id=""vsb_content_\d"">[\S\s]*?</p></div>");
            var mat_text = text.Match(html);
            string textstr = mat_text.Groups[0].ToString();
            //Console.WriteLine(textstr);

            //匹配部门？
            Regex department = new Regex("<p style=\"text-align: right;\">\\S+?</p>");
            var mat_dep = department.Match(html);
            string depstr = mat_dep.Groups[0].ToString();
            Console.WriteLine(depstr);

            //将</p>标签转换为\n进行换行
            Regex row = new Regex(@"</p>");
            textstr = row.Replace(textstr, "\n");
            //剔除正文中所有的标签
            Regex allTag = new Regex(@"<[\s\S]*?>");
            textstr = allTag.Replace(textstr, "");
            //Console.WriteLine(textstr);
            //存储到目录之中
            string path = Directory.GetCurrentDirectory();
            path += @"\"+datestr+@"\";
            Console.WriteLine(path);
            Novel(textstr, title, path);
        }
    }
}


