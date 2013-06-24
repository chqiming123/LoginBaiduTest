using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace LoginBAIDU
{
	class Program
	{
		CookieCollection cookies = new CookieCollection();
		CookieContainer cc = new CookieContainer();

		string username = "asdkf1956@163.com";
		string password = "test1956";

		string Token = "";
		string CodeString = "";


		private string Accept = "*/*";
		private string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";
		private string Referer = "";

		private string ContentType = "application/x-www-form-urlencoded";

		private void Run()
		{
			string html = "";

			//访问百度首页，获取 BAIDUID Cookie
			GetPageByGet("http://www.baidu.com", Encoding.UTF8);

			//获取 Token, CodeString
			GetTokenAndCodeString();

			//登录百度
			Login(username, password);

			PrintCookies();
			
		}

		/// <summary>
		/// 获取 Token & CodeString
		/// </summary>
		private void GetTokenAndCodeString()
		{ 
			Console.WriteLine("---------------------------------Get Token--------------------------------------");
			
			string url = string.Format("https://passport.baidu.com/v2/api/?getapi&tpl=ik&apiver=v3&tt={0}&class=login", Utility.GetCurrentTimeStamp());
			string html = GetPageByGet(url, Encoding.UTF8);
			Console.WriteLine(url);
						
			ResultData result = JsonConvert.DeserializeObject<ResultData>(html);
			if (result.ErrInfo.no == "0") {
				Token = result.Data.Token;
				CodeString = result.Data.CodeString;
			}

			Console.WriteLine("Token:{0}", Token);
			Console.WriteLine("CodeString:{0}", CodeString);
			
			Console.WriteLine("--------------------------------------------------------------------------------");
		}

		private void Login(string username, string password)
		{
			string loginUrl = "https://passport.baidu.com/v2/api/?login";

			Dictionary<string, string> postData = new Dictionary<string, string>();
			postData.Add("staticpage", "http://zhidao.baidu.com/static/html/v3Jump_bf2a8d6e.html");
			postData.Add("charset", "GBK");
			postData.Add("token", Token);
			postData.Add("tpl", "ik");
			postData.Add("apiver", "v3");
			postData.Add("tt", Utility.GetCurrentTimeStamp().ToString());
			postData.Add("codestring", "");
			postData.Add("isPhone", "false");
			postData.Add("safeflg", "0");
			postData.Add("u", "http://www.baidu.com/");
			postData.Add("username", username);
			postData.Add("password", password);
			postData.Add("verifycode", "");
			postData.Add("mem_pass", "on");
			postData.Add("ppui_logintime", "22429");
			postData.Add("callback", "parent.bd__pcbs__7doo5q");

			string html = GetPageByPost(loginUrl, postData, Encoding.UTF8);
			Console.WriteLine(html);
		}

		/// <summary>
		/// 打印 Cookies
		/// </summary>
		private void PrintCookies()
		{
			Console.WriteLine("---------------------------------Cookies----------------------------------------");
			
			foreach (Cookie cookie in cookies) {
				Console.WriteLine("{0}: {1} Domain: {2}", cookie.Name, cookie.Value, cookie.Domain);
			}

			Console.WriteLine("--------------------------------------------------------------------------------");
		}

		private void GetMyAsk()
		{
			string getAskUrl = "http://zhidao.baidu.com/uhome/ask";
			string html = GetPageByGet(getAskUrl, Encoding.GetEncoding("GBK"));
			Console.WriteLine(html);
		}

		/// <summary>
		/// 以 Post 方式提交网页数据,获得服务器返回的数据
		/// </summary>
		/// <param name="url"> Url </param>
		/// <param name="postData">Post 数据</param>
		/// <param name="encoder">网页编码</param>
		/// <returns>服务器返回的数据</returns>
		private string GetPageByPost(string url, Dictionary<string, string> postData, Encoding encoder)
		{
			string html = "";

			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
			webReq.CookieContainer = cc;
			webReq.Method = "POST";

			webReq.Accept = this.Accept;
			webReq.UserAgent = this.UserAgent;
			webReq.Referer = this.Referer;

			Stream reqStream = null;
			if (postData != null && postData.Count > 0) {
				StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, string> kv in postData) {
					sb.Append(HttpUtility.UrlEncode(kv.Key));
					sb.Append("=");
					sb.Append(HttpUtility.UrlEncode(kv.Value));
					sb.Append("&");
				}

				byte[] data = Encoding.UTF8.GetBytes(sb.ToString().TrimEnd('&'));

				webReq.ContentType = ContentType;
				webReq.ContentLength = data.Length;
				reqStream = webReq.GetRequestStream();
				reqStream.Write(data, 0, data.Length);
				if (reqStream != null) {
					reqStream.Close();
				}
			}

			HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
			cookies.Add(webResp.Cookies);
			Stream stream = webResp.GetResponseStream();
			StreamReader sr = new StreamReader(stream, encoder);
			html = sr.ReadToEnd();

			sr.Close();
			stream.Close();

			return html;
		}

		private string GetPageByGet(string url, Encoding encoder)
		{
			string html = "";

			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
			webReq.CookieContainer = cc;
			webReq.Method = "GET";

			HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
			cookies.Add(webResp.Cookies);
			Stream stream = webResp.GetResponseStream();
			StreamReader sr = new StreamReader(stream, encoder);
			html = sr.ReadToEnd();

			sr.Close();
			stream.Close();

			return html;
		}

		static void Main(string[] args)
		{
			new Program().Run();
			Console.ReadKey();
		}
	}
}
