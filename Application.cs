using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace MvcCore {
	public partial class Application: IHttpHandler, IHttpModule, IRequiresSessionState {
		public bool IsReusable => true;
		protected static Application instance = null;

		public static Application GetInstance() {
			return Application.instance;
		}
		public void Dispose() { }
		public Application() {
			if (!(Application.instance is Application)) {
				Application.instance = this;
			}
		}
		public virtual void Init(HttpApplication app) {
			// https://msdn.microsoft.com/en-us/library/bb470252.aspx
			app.Error += this.OnError;
			app.BeginRequest += this.OnBeginRequest;
			//app.AuthenticateRequest += this.OnAuthenticateRequest;
			//app.AuthorizeRequest += this.OnAuthorizeRequest;
			//app.ResolveRequestCache += this.OnResolveRequestCache;
			//app.AcquireRequestState += this.OnAcquireRequestState;

			app.PreRequestHandlerExecute += this.OnPreRequestHandlerExecute;
			app.PostResolveRequestCache += this.OnPostResolveRequestCache;
			app.PostRequestHandlerExecute += this.OnPostRequestHandlerExecute;

			//app.ReleaseRequestState += this.OnReleaseRequestState;
			//app.UpdateRequestCache += this.OnUpdateRequestCache;
			//app.EndRequest += this.OnEndRequest;
			app.PreSendRequestHeaders += this.OnPreSendRequestHeaders;
			app.PreSendRequestContent += this.OnPreSendRequestContent;
		}
		protected virtual void OnBeginRequest(object sender, EventArgs e) {
			// if (Application.HasRequestContext()) return; // better to use in web.config: <handlers><remove name="ExtensionlessUrlHandler-ISAPI-4.0_32bit" /></handlers>
			System.Web.HttpContext context = HttpContext.Current;
			Application.SetRequestContext(new Applications.RequestContext {
				Request = Request.GetInstance(context),
				Response = Response.GetInstance(context)
			});
		}
		protected virtual void OnPostResolveRequestCache(object sender, EventArgs e) {
			// preroute handlers
			// routing
		}
		protected virtual void OnPreRequestHandlerExecute(object sender, EventArgs e) {
			// predispatch handlers
		}
		public virtual void ProcessRequest(HttpContext context) {
			Desharp.Debug.Dump("ProcessRequest");
			context.Session["something"] = 5;
			this.GetResponse()
				.SetCode(200)
				.SetHeader("Content-Type", "text/html")
				.AppendBody("ProcessRequest")
				.PrependBody("Helo world - ");
		}
		protected virtual void OnPostRequestHandlerExecute(object sender, EventArgs e) {
			// post dispatch handlers
		}
		protected virtual void OnPreSendRequestHeaders(object sender, EventArgs e) {
			this.GetResponse().SendHeaders();
		}
		protected virtual void OnPreSendRequestContent(object sender, EventArgs e) {
			this.GetResponse().SendBody();
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

		protected virtual bool processCustomHandlers(List<PreRouteHandler> handlers) {
			bool result = TRUE;
			foreach (handlers as handler) {
				if (is_callable($handler)) {
					try {
					$handler($this->request, $this->response);
					} catch (\Exception $e) {
					$this->DispatchException($e);
					$result = FALSE;
						break;
					}
					}
				}
				return result;
			}
		}
}
