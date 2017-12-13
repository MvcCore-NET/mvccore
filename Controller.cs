using System;
using System.Web;

namespace MvcCore {
	public class Controller {

		protected Request request;
		protected Response response;
		protected string controller = "";
		protected string action = "";
		protected bool ajax = false;
		protected dynamic view = null;
		protected string layout = "layout";
		protected bool viewEnabled = true;

		protected static string staticPath = "/static";
		protected static string tmpPath = "/Var/Tmp";

		public virtual void Init () {
			this.controller = request.Params["controller"].ToString();
			this.action = request.Params["action"].ToString();
			if (request.Headers.ContainsKey("X-Requested-With")) {
				this.ajax = true;
				this.DisableView();
			}
		}
		public virtual void PreDispatch () {
			if (this.viewEnabled) {
				this.view = Activator.CreateInstance(
					MvcCore.Application.GetInstance().GetViewClass(),
					new object[] { this }
				) as MvcCore.View;
			}
		}
		public object GetParam (string name = "", string regExpAllowedChars = @"a-zA-Z0-9_/\-\.\@") {
			return this.request.GetParam(name, regExpAllowedChars);
		}
		public Controller DisableView () {
			this.viewEnabled = false;
			return this;
		}
		public Request GetRequest() {
			return this.request;
		}
		public Controller SetRequest(MvcCore.Request request) {
			this.request = request;
			return this;
		}
		public Controller SetResponse(MvcCore.Response response) {
			this.response = response;
			return this;
		}
		public View GetView() {
			return this.view;
		}
		public Controller SetView(MvcCore.View view) {
			this.view = view;
			return this;
		}
		public string GetLayout() {
			return this.layout;
		}
		public Controller SetLayout(string layout = "") {
			this.layout = layout;
			return this;
		}
		public virtual void Render (string controllerName = "", string actionName = "") {
			if (this.viewEnabled) {
				if (controllerName.Length == 0)	controllerName	= this.request.Params["controller"].ToString();
				if (actionName.Length == 0)		actionName		= this.request.Params["action"].ToString();
				// complete paths
				string controllerPath = controllerName.Replace('_', '/').Replace(@"\\", "/");
				string viewScriptPath = String.Join("/", new string[] {
					controllerPath, actionName
				});
				// render content string
				string actionResult = this.view.RenderScript(viewScriptPath);
				// create parent layout view, set up and render to outputResult
				
				View layout = Activator.CreateInstance(
					MvcCore.Application.GetInstance().GetViewClass(),
					new object[] { this }
				) as View;
				layout.SetUp(this.view);
				layout["Content"] = new System.Web.HtmlString(actionResult);
				string outputResult = layout.RenderLayoutAndContent(this.layout, actionResult);
				layout = null;
				this.view = null;
				// send response and exit
				this.HtmlResponse(outputResult);
				this.DisableView(); // disable to not render it again
			}
		}
		public Controller HtmlResponse(string output = "") {
			string contentTypeHeaderValue = MvcCore.View.Doctype.IndexOf(MvcCore.View.DOCTYPE_XHTML) > -1 
				? "application/xhtml+xml" 
				: "text/html";
			this.response
				.SetHeader("Content-Type", contentTypeHeaderValue + "; charset=utf-8")
				.SetBody(output);
			return this;
		}
		public Controller JsonResponse(object data = null) {
			string output = MvcCore.Tool.EncodeJson(data);
			this.response
				.SetHeader("Content-Type", "text/javascript; charset=utf-8")
				.SetHeader("Content-Length", output.Length.ToString())
				.SetBody(output);
			return this;
		}
		public string Url(string controllerActionOrRouteName = "Index:Index", object routeParams = null) {
			return MvcCore.Router.GetInstance().Url(
				Application.GetRequestContext(), controllerActionOrRouteName, routeParams
			);
		}
		public Controller RenderError(string exceptionMessage = "") {
			if (MvcCore.Application.GetInstance().IsErrorDispatched()) return this;
			throw new Applications.Exception(
				exceptionMessage.Length > 0 
					? exceptionMessage :
					"Server error: \n'" + this.request.FullUrl + "'",
				500
			);
		}
		public Controller RenderNotFound() {
			if (MvcCore.Application.GetInstance().IsNotFoundDispatched()) return this;
			throw new Applications.Exception(
				"Page not found: \n'" + this.request.FullUrl + "'", 404
			);
		}
		public Controller Terminate() {
			MvcCore.Application.GetInstance().Terminate();
			return this;
		}
		public static void Redirect(string location = "", int code = MvcCore.Response.SEE_OTHER) {
			MvcCore.Application app = MvcCore.Application.GetInstance();
			app.GetResponse()
				.SetCode(code)
				.SetHeader("Location", location);
			app.Terminate();
		}
	}
}
