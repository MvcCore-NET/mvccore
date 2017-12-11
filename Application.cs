using System;
using System.Web;
using System.Web.SessionState;

namespace MvcCore {
	public partial class Application: IHttpModule, IHttpHandler {
		protected static Application instance = null;

		public bool IsReusable => true;

		public Application() {
			if (!(Application.instance is Application)) {
				Application.instance = this;
			}
		}
		public static Application GetInstance () {
			return Application.instance;
		}
		public virtual void Init(HttpApplication app) {
			this.InitSession(app);
			// https://msdn.microsoft.com/en-us/library/bb470252.aspx
			app.Error += this.OnError;
			app.BeginRequest += this.OnBeginRequest;
			//app.AuthenticateRequest += this.OnAuthenticateRequest;
			app.AuthorizeRequest += this.OnAuthorizeRequest;
			//app.ResolveRequestCache += this.OnResolveRequestCache;
			//app.AcquireRequestState += this.OnAcquireRequestState;
			//app.PreRequestHandlerExecute += this.OnPreRequestHandlerExecute;
			//app.PostRequestHandlerExecute += this.OnPostRequestHandlerExecute;
			//app.ReleaseRequestState += this.OnReleaseRequestState;
			//app.UpdateRequestCache += this.OnUpdateRequestCache;
			//app.EndRequest += this.OnEndRequest;
			app.PreSendRequestHeaders += this.OnPreSendRequestHeaders;
			app.PreSendRequestContent += this.OnPreSendRequestContent;
		}

		public void ProcessRequest(HttpContext context) {
			Desharp.Debug.Dump("ProcessRequest");
			Desharp.Debug.Log("ProcessRequest");
		}

		protected virtual void OnError(object sender, EventArgs e) {
			var lastError = (sender as HttpApplication).Server.GetLastError();
			if (!(lastError is Exception)) return;
			if (Desharp.Debug.Enabled()) {
				Desharp.Debug.Dump(lastError);
			} else {
				Desharp.Debug.Log(lastError);
			}
		}

		protected virtual void OnBeginRequest(object sender, EventArgs e) {
			// if (Application.HasRequestContext()) return; // better to use in web.config: <handlers><remove name="ExtensionlessUrlHandler-ISAPI-4.0_32bit" /></handlers>
			System.Web.HttpContext context = HttpContext.Current;
			Application.SetRequestContext(new Applications.RequestContext {
				Request = Request.GetInstance(context),
				Response = Response.GetInstance(context)
			});
		}
		//protected virtual void OnAuthenticateRequest(object sender, EventArgs e) {}
		protected virtual void OnAuthorizeRequest(object sender, EventArgs e) {
			this.GetResponse()
				.SetCode(200)
				.SetHeader("Content-Type", "text/html")
				.AppendBody("OnAuthorizeRequest")
				.PrependBody("Helo world - ");
		}
		//protected virtual void OnResolveRequestCache(object sender, EventArgs e) {}
		//protected virtual void OnAcquireRequestState(object sender, EventArgs e) {}
		//protected virtual void OnPreRequestHandlerExecute(object sender, EventArgs e) {}
		//protected virtual void OnPostRequestHandlerExecute(object sender, EventArgs e) {}
		//protected virtual void OnReleaseRequestState(object sender, EventArgs e) {}
		//protected virtual void OnUpdateRequestCache(object sender, EventArgs e) {}
		//protected virtual void OnEndRequest(object sender, EventArgs e) {}
		protected virtual void OnPreSendRequestHeaders(object sender, EventArgs e) {
			this.GetResponse().SendHeaders();
		}
		protected virtual void OnPreSendRequestContent(object sender, EventArgs e) {
			this.GetResponse().SendBody();
		}
	}
}
