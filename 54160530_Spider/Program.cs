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
using JiebaNet.Segmenter;
using JiebaNet.Analyser;

namespace _54160530_Spider
{
    public class CrawlerController : Controller
    {
        static string total_content = "";
        static int total_amount_news = 0;
        static int total_department = 0;
        //static Dictionary<string, string> department_date = new Dictionary<string, string>();
        static ArrayList department_date = new ArrayList();
        public static void Main(string[]ar)
        {
            string website_prefix = "https://www.jlu.edu.cn/index/tzgg";
            CrawlerController c = new CrawlerController();
            c.Index(website_prefix+".htm");
            for (int i=39;i>0;i--)
            {
                string website = website_prefix + "/"+i.ToString()+".htm";
                //Console.WriteLine(website);
                c.Index(website);
                
            }
            Console.WriteLine("The total amount of news: " + total_amount_news);
            Console.WriteLine("The total amount of department detected: " + total_department);
            foreach(KeyValuePair<string,string> dep_date in department_date )
            {
                Console.WriteLine(dep_date.Key + dep_date.Value);
            }
            c.saveAsCSV();
            c.jieba_analysis();
        }
        // GET: Crawler
        public void Index(string website)
        {
            //抓取整本小说
            CrawlerController cra = new CrawlerController();// 顶点抓取小说网站小说
            //string html = cra.HttpGet("http://www.23us.so/files/article/html/13/13655/index.html", "");
            //string html = cra.HttpGet("http://www.baidu.com", "");
            //string html = cra.HttpGet("https://www.jlu.edu.cn/index/tzgg.htm", "");
            string html = cra.HttpGet(website, "");
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
            Regex tmpreg = new Regex(@"<a href=""[\s\S]+?/info/\d{4,}/\d{5,}.htm"">", RegexOptions.Compiled);
            MatchCollection sMC = tmpreg.Matches(html);
            //<li id="lineu11_0">< a href = "../info/1095/45890.htm" > 长春吉大附中实验学校(高中)公开招聘教师的启事 </ a >< span > 2019 - 06 - 24 </ span ></ li >


            //提取URL
            ArrayList URLS = new ArrayList();
            for (int i = 0; i < sMC.Count;i++)
            {

                //Console.WriteLine(sMC[i].Groups[0].Value);
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
                //Console.WriteLine(i);
                processURL(i);
            }


            //保存网页内容
            
            //FileStream fs = new FileStream(@"\samole\sample.txt", FileMode.Create, FileAccess.Write);
            //StreamWriter sr = new StreamWriter(fs);
            //sr.WriteLine(html);// 开始写入值
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
        public void Novel(string content, string name, string path,string depstr,string datestr)
        {
            string Log = content + "\r\n";
            //将文本存入totalcontent中，方便之后jieba分析
            total_content += content;
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
                total_amount_news++;//因为每一个html页面包含了多于所显示的链接，会导致重复生成文件
                if (!depstr.Equals(@"") && !datestr.Equals(@"")) 
                {
                    KeyValuePair<string, string> dep_date=new KeyValuePair<string, string>(depstr,datestr);
                    department_date.Add(dep_date);
                }
            }
            else
            {
                /*
                FileStream fs = new FileStream(path + name + ".txt" + "", FileMode.Append, FileAccess.Write);
                //FileStream fs = new FileStream("sample.txt", FileMode.Create, FileAccess.Write);

                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(Log);// 开始写入值
                sr.Close();
                fs.Close();
                */
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
            Regex tag_title = new Regex(@"<title>[\s\S]*</title>");
            var mat_mulu = tag_title.Match(html);
            string title = mat_mulu.Groups[0].ToString();
            title=title.Replace("<title>", "");
            title=title.Replace("</title>", "");
            //Console.WriteLine(title);

            //匹配日期
            Regex date = new Regex(@"\d\d\d\d-\d\d-\d\d");
            var mat_date = date.Match(html);
            string datestr = mat_date.Groups[0].ToString();
            //Console.WriteLine(datestr);

            //匹配正文
            Regex text = new Regex(@"<div id=""vsb_content[\s\S]*?"">[\S\s]*?</p></div>");
            var mat_text = text.Match(html);
            string textstr = mat_text.Groups[0].ToString();
            textstr = textstr.Replace("&nbsp;", "");
            //Console.WriteLine(textstr);


            //将</p>标签转换为\n进行换行
            //Regex row = new Regex(@"</p>");
            //textstr = row.Replace(textstr, "\n");

            //将<p></p>之间的每一行存储到arraylist中
            ArrayList rowList = new ArrayList();
            Regex row = new Regex(@"<p[\s\S]*?>[\s\S]*?</p>");
            MatchCollection rMC = row.Matches(textstr);

            //剔除正文中所有的标签
            Regex allTag = new Regex(@"<[\s\S]*?>");

            for (int i=0;i<rMC.Count;i++)
            {
                string one_row = rMC[i].Groups[0].Value;
                //剔除正文中的<br>标签
                if(one_row.Contains("<br>"))
                {
                    string[] strArray = Regex.Split(one_row, "<br>");
                   
                    for(int j=0;j<strArray.Count<string>();j++)
                    {
                        strArray[j] = allTag.Replace(strArray[j], "");
                        if(!strArray[j].Equals(""))
                        {
                            rowList.Add(strArray[j]);
                        }
                        
                    }
                }
                else
                {
                    one_row = allTag.Replace(one_row, "");
                    //当该行不为空时，向arraylist中添加改行
                    if (!one_row.Equals(""))
                    {
                        rowList.Add(one_row);
                    }
                }

               
            }
            /*
            //匹配部门？
            Regex department = new Regex("<p style=\"text-align: right;\">\\S+?</p>");
            var mat_dep = department.Match(html);
            string depstr = mat_dep.Groups[0].ToString();
            Console.WriteLine(depstr);
            if (!depstr.Equals("")) total_department++;
            */
            //匹配部门,筛选掉错误的部门名字
            string depstr = "";
            if(rowList.Count>=2)
            {
                depstr = rowList[rowList.Count - 2].ToString();

            }
            string department = "";
            //Console.WriteLine(depstr);
            if (depstr.Length <= 21&&depstr.Length>0)
            {
                if(!depstr.Contains("年")&&!depstr.Contains("电话")&&!depstr.Contains("附件")&&!depstr.Contains("联系人") && !depstr.Contains("《")&&!depstr.Contains("0") && !depstr.Contains("1") && !depstr.Contains("2") && !depstr.Contains("3") && !depstr.Contains("4") && !depstr.Contains("5") && !depstr.Contains("6") && !depstr.Contains("7") && !depstr.Contains("8") && !depstr.Contains("9") && !depstr.Contains("：") && !depstr.Contains(";"))
                {
                    department = depstr;
                    total_department++;
                    //Console.WriteLine(depstr);
                }
                
            }


            //Console.WriteLine(textstr);
            //存储到目录之中
            string content="";
            foreach(string str in rowList)
            {
                //Console.WriteLine(str);
                content += str + "\r\n";
            }
            string path = Directory.GetCurrentDirectory();
            path += @"\log\"+datestr+@"\";
            //Console.WriteLine(path);
            Novel(content, title, path,department,datestr);
        }
        //将每个部门每次发表新闻的时间以CSV的形式存储下
        public void saveAsCSV()
        {
            //string filename =Directory.GetCurrentDirectory()+ @"\data.csv";
            FileStream fs1 = new FileStream(Directory.GetCurrentDirectory() + @"\data.csv", FileMode.Create, FileAccess.Write);// 创建写入文件 
            //StreamWriter fileWriter = new StreamWriter(filename, true, Encoding.ASCII);
            StreamWriter fileWriter = new StreamWriter(fs1);
            fileWriter.Write("Department, Time \r\n");
            for(int i=0;i<department_date.Count;i++)
            {
                KeyValuePair<string, string> keyValuePair =(KeyValuePair<string,string>) department_date[i];
                fileWriter.Write(keyValuePair.Key.ToString()+","+keyValuePair.Value.ToString()+"\r\n");
            }
            fileWriter.Flush();
            fileWriter.Close();
        }
        public void jieba_analysis()
        {
            Dictionary<string, int> frequency = new Dictionary<string, int>();
            //var s = "在数学和计算机科学之中，算法（algorithm）为任何良定义的具体计算步骤的一个序列，常用于计算、数据处理和自动推理。精确而言，算法是一个表示为有限长列表的有效方法。算法应包含清晰定义的指令用于计算函数。";
            var seg = new JiebaSegmenter();
            var freqs = seg.Cut(total_content);
            foreach(string str in freqs)
            {
                if(!frequency.ContainsKey(str))
                {
                    frequency.Add(str, 1);
                    
                }
                else
                {
                    frequency[str]++;
                }
                
                //Console.WriteLine(str + " ");
            }
            DictonarySort(frequency);
            
     
        }
        //输出分词结果
        private void DictonarySort(Dictionary<string, int> dic)
        {
            var dicSort = from objDic in dic orderby objDic.Value descending select objDic;
            foreach (KeyValuePair<string, int> kvp in dicSort)     
            Console.WriteLine(kvp.Key + "：" + kvp.Value );

            FileStream fs1 = new FileStream(Directory.GetCurrentDirectory() + @"\frequency.csv", FileMode.Create, FileAccess.Write);// 创建写入文件 
            //StreamWriter fileWriter = new StreamWriter(filename, true, Encoding.ASCII);
            StreamWriter fileWriter = new StreamWriter(fs1);
            fileWriter.Write("word, frequency \r\n");
            foreach(KeyValuePair<string, int> kvp in dicSort)
            {
                fileWriter.Write(kvp.Key.ToString() + "," + kvp.Value.ToString() + "\r\n");
            }
            fileWriter.Flush();
            fileWriter.Close();
        }
    }
}


