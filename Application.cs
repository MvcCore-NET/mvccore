using MvcCore.Applications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace MvcCore {
	public partial class Application : IHttpHandler, IHttpModule, IRequiresSessionState {
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
			if (File.Exists(context.Request.PhysicalPath)) {
				Application.SetRequestContext(new Applications.RequestContext {});
				this.Terminate();
			} else { 
				Application.SetRequestContext(new Applications.RequestContext {
					Request = Request.GetInstance(context),
					Response = Response.GetInstance(context)
				});
			}
		}
		protected virtual void OnPostResolveRequestCache(object sender, EventArgs e) {
			RequestContext context = Application.GetRequestContext();
			if (
				!context.Terminated && 
				!this.processCustomHandlers<PreRouteHandler>(Application.GetInstance().preRouteHandlers)
			) {
				this.Terminate();
				return;
			}
			if (!context.Terminated && !this.routeRequest()) {
				this.Terminate();
			}
		}
		protected virtual void OnPreRequestHandlerExecute(object sender, EventArgs e) {
			if (
				!Application.GetRequestContext().Terminated && 
				!this.processCustomHandlers<PreDispatchHandler>(Application.GetInstance().preDispatchHandlers)
			) {
				this.Terminate();
			}
		}
		public virtual void ProcessRequest(HttpContext context) {
			if (
				!Application.GetRequestContext().Terminated && 
				!this.DispatchMvcRequest(this.GetCurrentRoute())
			) {
				this.Terminate();
			}
		}
		protected virtual void OnPostRequestHandlerExecute(object sender, EventArgs e) {
			if (
				!Application.GetRequestContext().Terminated && 
				!this.processCustomHandlers<PostDispatchHandler>(Application.GetInstance().postDispatchHandlers)
			) {
				this.Terminate();
			}
		}
		protected virtual void OnPreSendRequestHeaders(object sender, EventArgs e) {
			Response response = this.GetResponse();
			if (response is Response) response.SendHeaders();
		}
		protected virtual void OnPreSendRequestContent(object sender, EventArgs e) {
			Response response = this.GetResponse();
			if (response is Response) response.SendBody();
		}
		protected virtual void OnError(object sender, EventArgs e) {
			var lastError = (sender as HttpApplication).Server.GetLastError();
			if (!(lastError is System.Exception)) return;
			if (Desharp.Debug.Enabled()) {
				Desharp.Debug.Dump(lastError);
			} else {
				Desharp.Debug.Log(lastError);
			}
		}
	}
}
