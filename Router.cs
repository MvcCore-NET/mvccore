using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace MvcCore {
	public class Router {
		protected static Router instance = null;
		protected List<Route> routes = new List<Route>();
		protected Dictionary<string, Route> urlRoutes = new Dictionary<string, Route>();
		//protected Route currentRoute = null; // nonsense - this is moved into MvcCore.Applications.RequestContext object
		protected bool routeToDefaultIfNotMatch = false;
		public static Router GetInstance(params Route[] routes) {
			if (!(Router.instance is Router)) {
				Router.instance = Activator.CreateInstance(
					Application.GetInstance().GetRouterClass(),
					routes
				) as Router;
			}
			return Router.instance;
		}
		public Router(params Route[] routes) {
			if (routes.Length > 0) this.SetRoutes(routes);
		}
		public Router SetRoutes(Route[] routes) {
			this.routes.Clear();
			this.AddRoutes(routes);
			return this;
		}
		public List<Route> GetRoutes() {
			return this.routes;
		}
		public Router AddRoutes(Route[] routes, bool prepend = false) {
			List<Route> routesList = routes.ToList();
			if (prepend) routesList.Reverse();
			foreach (Route route in routes) {
				this.AddRoute(route, prepend);
			}
			return this;
		}
		public Router AddRoute(Route route, bool prepend = false) {
			if (prepend) {
				this.routes.Insert(0, route);
			} else {
				this.routes.Add(route);
			}
			this.urlRoutes[route.Name] = route;
			this.urlRoutes[route.Controller + ":" + route.Action] = route;
			return this;
		}
		public bool GetRouteToDefaultIfNotMatch() {
			return this.routeToDefaultIfNotMatch;
		}
		public Router SetRouteToDefaultIfNotMatch(bool enable = true) {
			this.routeToDefaultIfNotMatch = enable;
			return this;
		}
		
		public virtual Route Route (MvcCore.Request request) {
			Route currentRoute = null;
			string chars = @"a-zA-Z0-9\-_/";
			string controllerName = request.GetParam("controller", chars).ToString();
			string actionName = request.GetParam("action", chars).ToString();
			if (controllerName.Length > 0 && actionName.Length > 0) {
				currentRoute = this.routeByControllerAndActionQueryString(request, controllerName, actionName);
			} else {
				currentRoute = this.routeByRewriteRoutes(request);
			}
			string[] defaultCtrlAction = MvcCore.Application.GetInstance().GetDefaultControllerAndActionNames();
			Dictionary<string, string> defaultCtrlActionDct = new Dictionary<string, string>() {
				{ "controller", defaultCtrlAction[0] },
				{ "action", defaultCtrlAction[1] }
			};
			foreach (var item in defaultCtrlActionDct) {
				if (!request.Params.ContainsKey(item.Key) || (request.Params.ContainsKey(item.Key) && request.Params[item.Key].ToString().Length == 0)) {
					request.Params[item.Key] = MvcCore.Tool.GetDashedFromPascalCase(item.Value);
				}
			}
			if (!(currentRoute is Route) && (request.Path == "/" || this.routeToDefaultIfNotMatch)) {
				currentRoute = MvcCore.Route.GetInstance(
					name		: $"{defaultCtrlAction[0]}:{defaultCtrlAction[1]}",
					controller	: defaultCtrlAction[0],
					action		: defaultCtrlAction[1]
				);
			}
			if (currentRoute is Route) {
				if (currentRoute.Controller.Length == 0) {
					currentRoute.Controller = MvcCore.Tool.GetPascalCaseFromDashed(
						request.Params["controller"].ToString()
					);
				}
				if (currentRoute.Action.Length == 0) {
					currentRoute.Action = MvcCore.Tool.GetPascalCaseFromDashed(
						request.Params["action"].ToString()
					);
				}
			}
			return currentRoute;
		}
		protected Route routeByControllerAndActionQueryString(Request request, string controllerName, string actionName) {
			Route currentRoute;
			string[] ctrlDefaults = Router.completeControllerActionParam(controllerName);
			string controllerDashed = ctrlDefaults[0];
			string controllerPascalCase = ctrlDefaults[1];
			string[] actionDefaults = Router.completeControllerActionParam(actionName);
			string actionDashed = actionDefaults[0];
			string actionPascalCase = actionDefaults[1];
			currentRoute = MvcCore.Route.GetInstance(
				name		: $"{controllerPascalCase}:{actionPascalCase}",
				controller	: controllerPascalCase,
				action		: actionPascalCase
			);
			request.Params["controller"] = controllerDashed;
			request.Params["action"] = actionDashed;
			return currentRoute;
		}
		protected Route routeByRewriteRoutes(Request request) {
			Route currentRoute = null;
			string requestPath = request.Path;
			Match patternMatches;
			foreach (Route route in this.routes) {
				patternMatches = route.MatchRegex.Match(requestPath);
				if (patternMatches.Success) {
					currentRoute = route;
					string controllerName = String.IsNullOrEmpty(route.Controller) ? "" : route.Controller;
					Dictionary<string, object> routeParams = new Dictionary<string, object>() {
						{ "controller", MvcCore.Tool.GetDashedFromPascalCase(controllerName.Replace('_', '/').Replace('\\', '/')) },
						{ "action", MvcCore.Tool.GetDashedFromPascalCase(String.IsNullOrEmpty(route.Action) ? "" : route.Action) }
					};
					MatchCollection matches = Regex.Matches(route.Reverse, "{%([a-zA-Z0-9]*)}");
					if (matches.Count > 0) {
						foreach (var match in matches) {
							// TODO
						}
					}
				}

			}
			return currentRoute;
		}
		protected static string[] completeControllerActionParam(string dashed = "") {
			dashed = dashed.Length > 0 ? dashed.ToLower() : "default";
			return new string[] { dashed, MvcCore.Tool.GetPascalCaseFromDashed(dashed) };
		}


		public string Url(Applications.RequestContext context, string controllerActionOrRouteName = "Index:Index", object urlParams = null) {
			string result = "";
			Dictionary<string, object> requestParams = context.Request.Params;
			Dictionary<string, object> urlParamsDct = new Dictionary<string, object>();
			if (controllerActionOrRouteName.IndexOf(":") > -1) {
				string[] ctrlActionPc = controllerActionOrRouteName.Split(':');
				string ctrlPc = ctrlActionPc[0];
				string actionPc = ctrlActionPc[1];
				if (ctrlPc.Length == 0) ctrlPc = MvcCore.Tool.GetPascalCaseFromDashed(requestParams["controller"].ToString());
				if (actionPc.Length == 0) actionPc = MvcCore.Tool.GetPascalCaseFromDashed(requestParams["action"].ToString());
				controllerActionOrRouteName = $"{ctrlPc}:{actionPc}";
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(urlParams);
				foreach (PropertyDescriptor prop in props) {
					urlParamsDct.Add(prop.Name, prop.GetValue(urlParams));
				}
			} else if (controllerActionOrRouteName == "self") {
				controllerActionOrRouteName = context.CurrentRoute is Route ? context.CurrentRoute.Name : ":";
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(urlParams);
				foreach (PropertyDescriptor prop in props) {
					urlParamsDct.Add(prop.Name, prop.GetValue(urlParams));
				}
				foreach (var item in requestParams) {
					if (!urlParamsDct.ContainsKey(item.Key)) urlParamsDct.Add(item.Key, item.Value);
				}
				urlParamsDct.Remove("controller");
				urlParamsDct.Remove("action");
			}
			bool absolute = false;
			if (urlParamsDct.Count > 0 && urlParamsDct.ContainsKey("absolute")) {
				absolute = Convert.ToBoolean(urlParamsDct["absolute"]);
				urlParamsDct.Remove("absolute");
			}
			if (this.urlRoutes.ContainsKey(controllerActionOrRouteName)) {
				result = this.urlByRoute(context, controllerActionOrRouteName, urlParamsDct);
			} else {
				result = this.urlByQueryString(context, controllerActionOrRouteName, urlParamsDct);
			}
			if (absolute) result = context.Request.DomainUrl + result;
			return result;
		}
		protected string urlByRoute(Applications.RequestContext context, string controllerActionOrRouteName, Dictionary<string, object> urlParamsDct) {
			Route route = this.urlRoutes[controllerActionOrRouteName];
			string result = context.Request.BasePath + route.Reverse.TrimEnd(new char[] { '?', '&'});
			Dictionary<string, object> allParams = new Dictionary<string, object>(urlParamsDct);
			foreach (var item in route.Params) {
				if (!allParams.ContainsKey(item.Key)) allParams.Add(item.Key, item.Value);
			}
			foreach (var item in allParams) {
				string paramKeyReplacement = "{%" + item.Key + "}";
				if (result.IndexOf(paramKeyReplacement) == -1) {
					string glue = result.IndexOf('?') == -1 ? "?" : "&";
					result += this.httpBuildQuery(new Dictionary<string, object>() {
						{ item.Key, item.Value }
					});
				} else {
					result = result.Replace(paramKeyReplacement, item.Value.ToString());
				}
			}
			return result;
		}
		protected string urlByQueryString(Applications.RequestContext context, string controllerActionOrRouteName, Dictionary<string, object> urlParamsDct) {
			string[] ctrlActionPascal = controllerActionOrRouteName.Split(':');
			string contollerPascalCase = ctrlActionPascal[0];
			string actionPascalCase = ctrlActionPascal[1];
			string controllerDashed = MvcCore.Tool.GetDashedFromPascalCase(contollerPascalCase);
			string actionDashed = MvcCore.Tool.GetDashedFromPascalCase(actionPascalCase);
			string result = context.Request.BasePath + $"?controller={controllerDashed}&action={actionDashed}";
			result += this.httpBuildQuery(urlParamsDct);
			return result;
		}
		protected string httpBuildQuery (Dictionary<string, object> urlParamsDct) {
			string result = "";
			if (urlParamsDct.Count > 0) {
				foreach (var item in urlParamsDct) {
					if (item.Value is object[]) {
						result += "&" + item.Key + "[]=" + item.Value;
					} else {
						result += "&" + item.Key + "=" + item.Value;
					}
				}
			}
			return result;
		}
	}
}
