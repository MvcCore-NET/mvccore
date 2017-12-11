using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace MvcCore {
	public class Response {

		public const int OK = 200;
		public const int MOVED_PERMANENTLY = 301;
		public const int SEE_OTHER = 303;
		public const int NOT_FOUND = 404;
		public const int INTERNAL_SERVER_ERROR = 500;

		public static Dictionary<int, string> CodeMessages = new Dictionary<int, string> {
			{ Response.OK                   , "OK" },
			{ Response.MOVED_PERMANENTLY    , "Moved Permanently" },
			{ Response.SEE_OTHER            , "See Other" },
			{ Response.NOT_FOUND            , "Not Found" },
			{ Response.INTERNAL_SERVER_ERROR, "Internal Server Error" },
		};

		public bool HeadersSent {
			get { return this.headersSent; }
		}
		public bool Redirected {
			get { return this.contextResponse.IsRequestBeingRedirected; }
		}
		public bool HtmlOutput {
			get {
				string value = this.contextResponse.ContentType;
				if (value.Length > 0 && (
					value.IndexOf("text/html") > -1 || value.IndexOf("application/xhtml+xml") > -1
				)) {
					return true;
				}
				return false;
			}
		}

		public int Code {
			get { return this.code; }
		}
		protected int code = Response.OK;

		public Dictionary<string, string> Headers = new Dictionary<string, string>();
		public StreamWriter Body;
		public MemoryStream BodyStream;

		protected System.Web.HttpResponse contextResponse;
		protected bool headersSent = false;
		protected bool bodySent = false;

		public static Response GetInstance(HttpContext context) {
			return Activator.CreateInstance(
				MvcCore.Application.GetInstance().GetResponseClass(),
				new object[] { context }
			) as Response;
		}
		public Response (HttpContext context) {
			this.contextResponse = context.Response;
			this.code = this.contextResponse.StatusCode;
			this.UpdateHeaders();
			this.SetBody("");
		}
		public virtual Response SetCode(int code) {
			this.code = code;
			this.contextResponse.StatusCode = code;
			return this;
		}
		public virtual Response SetHeader(string name, string value) {
			this.contextResponse.Headers[name] = value;
			this.Headers[name] = value;
			if (name.ToLower() == "content-type") {
				int pos = value.IndexOf(";");
				if (pos > -1) {
					string encoding = value.Substring(pos + 1).Trim();
					value = value.Substring(0, pos).Trim();
					pos = encoding.IndexOf("=");
					if (pos > -1) {
						this.contextResponse.ContentEncoding = Encoding.GetEncoding(
							encoding.Substring(pos + 1).Trim()
						);
					}
				}
				this.contextResponse.ContentType = value;
			}
			return this;
		}
		public virtual Response SetBody(string body) {
			this.BodyStream = new MemoryStream();
			this.Body = new StreamWriter(this.BodyStream);
			this.Body.Write(body);
			return this;
		}
		public virtual Response AppendBody(string body) {
			this.Body.Write(body);
			return this;
		}
		public virtual Response PrependBody(string body) {
			this.Body.Flush();
			this.BodyStream.Position = 0;
			string previousBody = (new StreamReader(this.BodyStream)).ReadToEnd();
			this.BodyStream = new MemoryStream();
			this.Body = new StreamWriter(this.BodyStream);
			this.Body.Write(body + previousBody);
			return this;
		}
		public virtual Response UpdateHeaders () {
			this.Headers = new Dictionary<string, string>();
			NameValueCollection contextHeaders = this.contextResponse.Headers;
			string[] allKeys = contextHeaders.AllKeys;
			string headerName;
			for (int i = 0, l = allKeys.Length; i < l; i += 1) {
				headerName = allKeys[i];
				this.Headers.Add(headerName, contextHeaders[headerName]);
			}
			return this;
		}
		public virtual Response SendHeaders() {
			if (this.headersSent) return this;
			this.contextResponse.StatusCode = this.Code;
			if (Response.CodeMessages.ContainsKey(this.code)) {
				this.contextResponse.StatusDescription = Response.CodeMessages[this.code];
			}
			foreach (var item in this.Headers) {
				this.contextResponse.Headers[item.Key] = item.Value;
			}
			this.addTimeAndMemoryHeader();
			this.headersSent = true;
			return this;
		}
		public virtual Response SendBody () {
			if (this.bodySent) return this;
			this.contextResponse.OutputStream.Flush();
			this.Body.Flush();
			this.BodyStream.WriteTo(this.contextResponse.OutputStream);
			this.bodySent = true;
			return this;
		}
		protected virtual void addTimeAndMemoryHeader() {
			string format = "0.###";
			CultureInfo formatInfo = new CultureInfo("en-US");
			HttpContext context = HttpContext.Current;
			string requestTime = (DateTime.Now - context.Timestamp).Milliseconds
				.ToString(format, formatInfo) + " ms";
			string gcTotalMemory = (GC.GetTotalMemory(true) / 1048576)
				.ToString(format, formatInfo) + " MB";
			string headerName = "X-MvcCore-Cpu-Ram";
			string headerValue = requestTime + ", " + gcTotalMemory;
			this.contextResponse.Headers[headerName] = headerValue;
			this.Headers[headerName] = headerValue;
		}
	}
}
