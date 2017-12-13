using MvcCore.Applications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace MvcCore {
	public partial class Application {

		protected virtual bool processCustomHandlers<HandlerType>(List<HandlerType> handlers) {
			bool result = true;
			RequestContext context = Application.GetRequestContext();
			foreach (HandlerType handler in handlers) {
				try {
					(handler as Delegate).DynamicInvoke(context.Request, context.Response);
				} catch (System.Exception e) {
					this.DispatchException(context, e);
					result = false;
					break;
				}
			}
			return result;
		}
		public virtual bool DispatchException(RequestContext context, System.Exception e) {
			int code = 0;
			if (e is Applications.Exception) code = (e as Applications.Exception).Code;
			if (code == 404) {
				Desharp.Debug.Log(e, Desharp.Level.ERROR);
				return this.RenderNotFound(context, e.Message);
			} else if (MvcCore.Config.IsDevelopment) {
				Desharp.Debug.Dump(e);
				return false;
			} else {
				Desharp.Debug.Log(e);
				return this.RenderError(context, e);
			}
		}
		protected bool routeRequest () {
			Applications.RequestContext context = Application.GetRequestContext();
			if (!(context is Applications.RequestContext)) return true;
			try {
				context.CurrentRoute = Router.GetInstance().Route(context.Request);
				return true;
			} catch (System.Exception e) {
				return this.DispatchException(context, e);
			}
		}
		public bool DispatchMvcRequest (Route currentRoute) {
			RequestContext context = Application.GetRequestContext();
			if (currentRoute == null) {
				try {
					throw new Applications.Exception("No route for request.", 404);
				} catch (Applications.Exception e) {
					return this.DispatchException(context, e);
				}
			};
			string controllerNamePascalCase = currentRoute.Controller;
			string actionNamePascalCase = currentRoute.Action;
			string actionName = actionNamePascalCase + "Action";
			string coreControllerName = "MvcCore.Controller";
			Dictionary<string, object> requestParams = context.Request.Params;
			string viewScriptFullPath = MvcCore.View.GetViewScriptFullPath(
				MvcCore.View.ScriptsDir,
				requestParams["controller"] + "/" + requestParams["action"]
			);
			string controllerName;
			Type controllerType = null;
			if (controllerNamePascalCase == "Controller") {
				controllerName = coreControllerName;
			} else {
				// App.Controllers.{controllerNamePascalCase}
				controllerName = this.CompleteControllerName(controllerNamePascalCase);
				controllerType = Tool.GetTypeGlobaly(controllerName);
				if (!(controllerType is Type)) {
					// if controller doesn't exists - check if at least view exists
					if (File.Exists(viewScriptFullPath)) {
						// if view exists - change controller name to core controller, if not let it go to exception
						controllerName = coreControllerName;
					}
				}
			}
			if (!(controllerType is Type)) controllerType = Tool.GetTypeGlobaly(controllerName);
			return this.DispatchControllerAction(
				context,
				controllerType,
				controllerName,
				actionName, 
				viewScriptFullPath, 
				(System.Exception e) => {
					return this.DispatchException(context, e);
				}
			);
		}
		public bool DispatchControllerAction(
			RequestContext context, 
			Type controllerType,
			string controllerClassFullName, 
			string actionName, 
			string viewScriptFullPath, 
			Func<System.Exception, bool> exceptionCallback
		) {
			context.Controller = null;
			try {
				context.Controller = Activator.CreateInstance(
					Tool.GetTypeGlobaly(controllerClassFullName)
				) as MvcCore.Controller;
				context.Controller.SetRequest(context.Request).SetResponse(context.Response);
			} catch (System.Exception e) {
				try {
					throw new Applications.Exception(e.Message, 404);
				} catch (Applications.Exception ae) { 
					return this.DispatchException(context, ae);
				}
			}
			MethodInfo ctrlAction = controllerType.GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public);
			bool ctrlHasAction = ctrlAction is MethodInfo;
			if (!ctrlHasAction && controllerClassFullName != "MvcCore.Controller") {
				if (!File.Exists(viewScriptFullPath)) {
					try {
						throw new Applications.Exception(
							$"Controller '{controllerClassFullName}' has not method '{actionName}' "
							+$"or view doesn't exists in path: '{viewScriptFullPath}'.", 404
						);
					} catch (Applications.Exception ae) {
						return this.DispatchException(context, ae);
					}
				}
			}
			string controllerNameDashed = context.Request.Params["controller"].ToString();
			string actionNameDashed = context.Request.Params["action"].ToString();
			try {
				// MvcCore.Debug.Timer("dispatch");
				context.Controller.Init();
				// MvcCore.Debug.Timer("dispatch");
				context.Controller.PreDispatch();
				// MvcCore.Debug.Timer("dispatch");
				if (ctrlHasAction) ctrlAction.Invoke(context.Controller, null);
				// MvcCore.Debug.Timer("dispatch");
				context.Controller.Render(controllerNameDashed, actionNameDashed);
				// MvcCore.Debug.Timer("dispatch");
			} catch (System.Exception e) {
				return exceptionCallback.Invoke(e);
			}
			return true;
		}
		public string Url (string controllerActionOrRouteName = "Index:Index", object urlParams = null) {
			return MvcCore.Router.GetInstance().Url(Application.GetRequestContext(), controllerActionOrRouteName, urlParams);
		}
		public virtual void Terminate () {
			Application.GetRequestContext().Terminated = true;
		}
		public virtual bool RenderError(RequestContext context, System.Exception e) {
			string defaultCtrlFullName = this.GetDefaultControllerIfHasAction(
				Application.instance.defaultControllerErrorActionName
			);
			string exceptionMessage = e.Message;
			if (defaultCtrlFullName.Length > 0) {
				context.Request.Params["Code"] = 500;
				context.Request.Params["message"] = exceptionMessage;
				context.Request.Params["controller"] = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerName);
				context.Request.Params["action"] = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerErrorActionName);
				return this.DispatchControllerAction(
					context,
					Tool.GetTypeGlobaly(defaultCtrlFullName),
					defaultCtrlFullName,
					Application.instance.defaultControllerErrorActionName + "Action",
					"",
					ex => {
						Desharp.Debug.Log(e);
						return this.RenderError500PlainText(context, exceptionMessage + Environment.NewLine + Environment.NewLine + ex.Message);
					}
				);
			} else {
				return this.RenderError500PlainText(context, exceptionMessage);
			}
		}
		public virtual bool RenderNotFound(RequestContext context, string exceptionMessage = "") {
			if (exceptionMessage.Length == 0) exceptionMessage = "Page not found.";
			string defaultCtrlFullName = this.GetDefaultControllerIfHasAction(
				Application.instance.defaultControllerNotFoundActionName
			);
			if (defaultCtrlFullName.Length > 0) {
				context.Request.Params["Code"] = 404;
				context.Request.Params["message"] = exceptionMessage;
				context.Request.Params["controller"] = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerName);
				context.Request.Params["action"] = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerNotFoundActionName);
				return this.DispatchControllerAction(
					context,
					Tool.GetTypeGlobaly(defaultCtrlFullName),
					defaultCtrlFullName,
					Application.instance.defaultControllerNotFoundActionName + "Action",
					"",
					ex => {
						Desharp.Debug.Log(ex);
						return this.RenderError404PlainText(context);
					}
				);
			} else {
				return this.RenderError404PlainText(context, exceptionMessage);
			}
		}
		public virtual bool RenderError500PlainText(RequestContext context, string text = "") {
			if (text.Length == 0) text = "Internal Server Error.";
			context.Response = MvcCore.Response.GetInstance(HttpContext.Current)
				.SetCode(MvcCore.Response.INTERNAL_SERVER_ERROR)
				.SetHeader("Content-Type", "text/plain")
				.SetBody("Error 500:" + Environment.NewLine + Environment.NewLine + text);
			return true;
		}
		public virtual bool RenderError404PlainText(RequestContext context, string text = "") {
			if (text.Length == 0) text = "Page not found.";
			context.Response = MvcCore.Response.GetInstance(HttpContext.Current)
				.SetCode(MvcCore.Response.NOT_FOUND)
				.SetHeader("Content-Type", "text/plain")
				.SetBody("Error 404:" + Environment.NewLine + Environment.NewLine + text);
			return true;
		}
	}
}
