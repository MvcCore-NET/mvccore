using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace MvcCore {
	public class Request {
		/// <summary>
		/// Non-secured HTTP protocol (http:).
		/// </summary>
		public const string PROTOCOL_HTTP = "http:";
		/// <summary>
		/// Secured HTTP(s) protocol (https:).
		/// </summary>
		public const string PROTOCOL_HTTPS = "https:";
		/// <summary>
		/// Retrieves the information or entity that is identified by the URI of the request.
		/// </summary>
		public const string METHOD_GET = "GET";
		/// <summary>
		/// Posts a new entity as an addition to a URI.
		/// </summary>
		public const string METHOD_POST = "POST";
		/// <summary>
		/// Replaces an entity that is identified by a URI.
		/// </summary>
		public const string METHOD_PUT = "PUT";
		/// <summary>
		/// Requests that a specified URI be deleted.
		/// </summary>
		public const string METHOD_DELETE = "DELETE";
		/// <summary>
		/// Retrieves the message headers for the information or entity that is identified by the URI of the request.
		/// </summary>
		public const string METHOD_HEAD = "HEAD";
		/// <summary>
		/// Represents a request for information about the communication options available on the request/response chain identified by the Request-URI.
		/// </summary>
		public const string METHOD_OPTIONS = "OPTIONS";
		/// <summary>
		/// Requests that a set of changes described in the request entity be applied to the resource identified by the Request- URI.
		/// </summary>
		public const string METHOD_PATCH = "PATCH";
		/// <summary>
		/// The TRACE method performs a message loop-back test along the path to the target resource.
		/// </summary>
		public const string METHOD_TRACE = "TRACE";
		/// <summary>
		/// The CONNECT method establishes a tunnel to the server identified by the target resource.
		/// </summary>
		public const string METHOD_CONNECT= "CONNECT";

		/// <summary>
		/// Language international code, lowercase, not used by default.
		/// To use this variable - install MvcCore.Router extension MvcCore.Ext.Router.Lang.
		/// </summary>
		/// <example>"en"</example>
		public string Lang = "";
		/// <summary>
		/// Country/locale code, uppercase, not used by default.
		/// To use this variable - install MvcCore.Router extension MvcCoreExt.Router.Locale.
		/// </summary>
		/// <example>"US"</example>
		public string Locale = "";
		/// <summary>
		/// Http protocol: "http:" | "https:" ...
		/// </summary>
		/// <example>"http:"</example>
		public string Protocol = "";
		/// <summary>
		/// Application server name - domain without any port.
		/// </summary>
		/// <example>"localhost" | "domain.com" | "www.domain.com" ...</example>
		public string ServerName = "";
		/// <summary>
		/// Application host with port if there is any different than 80.
		/// </summary>
		/// <example>"localhost:88" | "domain.com:88" | "www.domain.com:88" ...</example>
		public string Host = "";
		/// <summary>
		/// Http port.
		/// </summary>
		/// <example>"80" | "88" ...</example>
		public string Port = "";
		/// <summary>
		/// Requested path in from application root, never with query string.
		/// </summary>
		/// <example>"/products/page/2"</example>
		public string Path = "";
		/// <summary>
		/// Uri query string without question mark.
		/// </summary>
		/// <example>"param-1=value-1&param-2=value-2&param-3[]=value-3-a&param-3[]=value-3-b"</example>
		public string Query = "";
		/// <summary>
		/// Uri fragment after hash including hash.
		/// </summary>
		/// <example>"#any-sublink-path"</example>
		public string Fragment = "";
		/// <summary>
		/// Application root path in hard drive without trailing slash.
		/// </summary>
		/// <example>"C:/www/my/development/direcotry/www"</example>
		public string AppRoot = "";
		/// <summary>
		/// Base app directory path after domain, if application is placed in domain subdirectory. 
		/// </summary>
		/// <example>
		/// full path: "https://localhost:88/my/development/direcotry/www/requested/path/after/domain?with=possible&query=string"
		/// base path: "/my/development/direcotry/www"
		/// </example>
		public string BasePath	= "";
		/// <summary>
		/// Request path after domain with possible query string.
		/// </summary>
		/// <example>"/requested/path/after/app/root?with=possible&query=string"</example>
		public string RequestPath	= "";
		/// <summary>
		/// Url to requested domain and possible port.
		/// </summary>
		/// <example>"https://domain.com" | "http://domain:88" if any port..</example>
		public string DomainUrl	= "";
		/// <summary>
		/// Base url to application root with possible port.
		/// </summary>
		/// <example>"http://domain:88/my/development/direcotry/www"</example>
		public string BaseUrl = "";
		/// <summary>
		/// Request url including scheme, domain, port, path, without any query string.
		/// </summary>
		/// <example>"http://localhost:88/my/development/direcotry/www/requested/path/after/domain"</example>
		public string RequestUrl = "";
		/// <summary>
		/// Request url including scheme, domain, port, path and with query string.
		/// </summary>
		/// <example>"http://localhost:88/my/development/direcotry/www/requested/path/after/domain?with=possible&query=string"</example>
		public string FullUrl = "";
		/// <summary>
		/// Http method (uppercase) - GET, POST, PUT, HEAD...
		/// </summary>
		/// <example>"GET"</example>
		public string Method = "";
		/// <summary>
		/// Referer url if any.
		/// </summary>
		/// <example>"http://foreing.domain.com/path/where/is/link/to/?my=app"</example>
		public string Referer = "";
		/// <summary>
		/// Raw request params array, with keys defined in route or by query string, 
		/// always with controller and action keys completed by router.
		/// To get safe param value - use: request.GetParam("user", "a-zA-Z0-9");
		/// </summary>
		/// <example>
		/// request.Params = new Dictionary&lt;string, object&gt; () { 
		///		{ "controller",	"default" }, 
		///		{ "action",		"default" }, 
		///		{ "user",		"' OR 1=1;-- " } // with raw danger value!
		///	}
		/// </example>
		public Dictionary<string, object> Params = new Dictionary<string, object>();
		/// <summary>
		/// Media site key - "full" | "tablet" | "mobile".
		/// To use this variable - install MvcCore.Router extension MvcCore.Ext.Router.Media
		/// </summary>
		/// <example>"full" | "tablet" | "mobile"</example>
		public string MediaSiteKey = "";
		/// <summary>
		/// Web server global variables.
		/// </summary>
		public Dictionary<string, string> Server = new Dictionary<string, string>();
		/// <summary>
		/// Raw request headers.
		/// </summary>
		public Dictionary<string, string> Headers = new Dictionary<string, string>();
		/// <summary>
		/// Request input stream.
		/// </summary>
		public Stream Body;
		/// <summary>
		/// Sended files collection.
		/// </summary>
		public HttpFileCollection Files;

		protected HttpRequest contextRequest;

		public static Request GetInstance(HttpContext context) {
			return Activator.CreateInstance(
				MvcCore.Application.GetInstance().GetRequestClass(),
				new object[] { context }
			) as Request;
		}

		public Request(HttpContext context) {

			this.contextRequest = context.Request;

			this.initServer();

			this.initAppRoot();
			this.initMethod();
			this.initBasePath();
			this.initProtocol();
			this.initParsedUrlSegments();
			this.initHttpParams();
			this.initPath();
			this.initReferer();
			this.initUrlCompositions();
		}

		private void initServer() {
			NameValueCollection server = this.contextRequest.ServerVariables;
			string[] serverKeys = server.AllKeys;
			string serverKey;
			string serverValue;
			for (int i = 0, l = serverKeys.Length; i < l; i += 1) {
				serverKey = serverKeys[i];
				serverValue = server[serverKey];
				this.Server.Add(serverKey, serverValue);
			}
		}

		protected virtual void initAppRoot() {
			this.AppRoot = this.contextRequest.PhysicalApplicationPath.Replace("\\", "/").TrimEnd('/');
		}

		protected virtual void initMethod() {
			this.Method = this.contextRequest.HttpMethod;
		}

		protected virtual void initBasePath() {
			this.BasePath = this.contextRequest.ApplicationPath.TrimEnd('/');
		}

		protected virtual void initProtocol() {
			this.Protocol = this.contextRequest.Url.Scheme + ":";
		}

		protected virtual void initParsedUrlSegments() {
			System.Uri url = this.contextRequest.Url;
			this.Port = url.Port.ToString();
			this.Query = url.Query.TrimStart('?');
			this.Fragment = this.contextRequest.Url.Fragment.TrimStart('#');
			if (this.Fragment.Length > 0) this.Fragment = "#" + this.Fragment;
			this.ServerName = url.Host;
			if (
				(this.Protocol == Request.PROTOCOL_HTTP && url.Port != 80) ||
				(this.Protocol == Request.PROTOCOL_HTTPS && url.Port != 443)
			) {
				this.Host = url.Host + ":" + url.Port.ToString();
			} else {
				this.Host = url.Host;
			}
		}

		protected virtual void initHttpParams() {
			Desharp.Debug.Dump("asdf");
		}

		protected virtual void initPath() {
			this.Path = this.contextRequest.AppRelativeCurrentExecutionFilePath.TrimStart('~');
		}

		protected virtual void initReferer() {
			if (this.contextRequest.UrlReferrer is System.Uri) {
				this.Referer = this.contextRequest.UrlReferrer.AbsoluteUri;
			}
		}

		protected virtual void initUrlCompositions() {
			this.RequestPath = this.Path + ((this.Query.Length > 0) ? "?" + this.Query : "") + this.Fragment;
			this.DomainUrl = this.Protocol + "//" + this.Host;
			this.BaseUrl = this.DomainUrl + this.BasePath;
			this.RequestUrl = this.BaseUrl + this.Path;
			this.FullUrl = this.RequestUrl + ((this.Query.Length > 0) ? "?" + this.Query : "");
		}
	}
}
