using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace MvcCore {
	public partial class Application {

		protected List<PreRouteHandler> preRouteHandlers = new List<PreRouteHandler>();
		protected List<PreDispatchHandler> preDispatchHandlers = new List<PreDispatchHandler>();
		protected List<PostDispatchHandler> postDispatchHandlers = new List<PostDispatchHandler>();

		protected Type configClass = typeof(Config);
		protected Type sessionClass = typeof(Session);
		protected Type requestClass = typeof(Request);
		protected Type responseClass = typeof(Response);
		protected Type routerClass = typeof(Router);
		protected Type viewClass = typeof(View);
		protected Type debugClass = typeof(Debug);

		protected string controllersDir = "Controllers";
		protected string viewsDir = "Views";

		protected string defaultControllerName = "Index";
		protected string defaultControllerDefaultActionName = "Index";
		protected string defaultControllerErrorActionName = "Error";
		protected string defaultControllerNotFoundActionName = "NotFound";

		public virtual Application AddPreRouteHandler(PreRouteHandler handler) {
			this.preRouteHandlers.Add(handler);
			return this;
		}
		public virtual Application AddPreDispatchHandler(PreDispatchHandler handler) {
			this.preDispatchHandlers.Add(handler);
			return this;
		}
		public virtual Application AddPostDispatchHandler(PostDispatchHandler handler) {
			this.postDispatchHandlers.Add(handler);
			return this;
		}

		public virtual Application SetRequestClass(Type requestClass) {
			if (!(typeof(Request).IsAssignableFrom(requestClass))) {
				throw new Exception($"Class {requestClass.FullName} is not inherited from MvcCore.Request.");
			}
			this.requestClass = requestClass;
			return this;
		}
		public virtual Application SetRequestClass<RequestClass>() {
			Type requestClass = typeof(RequestClass);
			if (!(typeof(Request).IsAssignableFrom(requestClass))) {
				throw new Exception($"Class {requestClass.FullName} is not inherited from MvcCore.Request.");
			}
			this.requestClass = typeof(RequestClass);
			return this;
		}
		public virtual Type GetRequestClass() {
			return this.requestClass;
		}

		public virtual Application SetResponseClass(Type responseClass) {
			if (!(typeof(Response).IsAssignableFrom(responseClass))) {
				throw new Exception($"Class {responseClass.FullName} is not inherited from MvcCore.Response.");
			}
			this.responseClass = responseClass;
			return this;
		}
		public virtual Application SetResponseClass<ResponseClass>() {
			Type responseClass = typeof(ResponseClass);
			if (!(typeof(Response).IsAssignableFrom(responseClass))) {
				throw new Exception($"Class {responseClass.FullName} is not inherited from MvcCore.Response.");
			}
			this.responseClass = responseClass;
			return this;
		}
		public virtual Type GetResponseClass() {
			return this.responseClass;
		}

		public virtual Application SetRouterClass(Type routerClass) {
			if (!(typeof(Router).IsAssignableFrom(routerClass))) {
				throw new Exception($"Class {routerClass.FullName} is not inherited from MvcCore.Router.");
			}
			this.routerClass = routerClass;
			return this;
		}
		public virtual Application SetRouterClass<RouterClass>() {
			Type routerClass = typeof(RouterClass);
			if (!(typeof(Router).IsAssignableFrom(routerClass))) {
				throw new Exception($"Class {routerClass.FullName} is not inherited from MvcCore.Router.");
			}
			this.routerClass = routerClass;
			return this;
		}
		public virtual Type GetRouterClass() {
			return this.routerClass;
		}

		public virtual Application SetConfigClass(Type configClass) {
			if (!(typeof(Config).IsAssignableFrom(configClass))) {
				throw new Exception($"Class {configClass.FullName} is not inherited from MvcCore.Config.");
			}
			this.configClass = configClass;
			return this;
		}
		public virtual Application SetConfigClass<ConfigClass>() {
			Type configClass = typeof(ConfigClass);
			if (!(typeof(Config).IsAssignableFrom(configClass))) {
				throw new Exception($"Class {configClass.FullName} is not inherited from MvcCore.Config.");
			}
			this.configClass = configClass;
			return this;
		}
		public virtual Type GetConfigClass() {
			return this.configClass;
		}

		public virtual Application SetSessionClass(Type sessionClass) {
			if (!(typeof(Session).IsAssignableFrom(sessionClass))) {
				throw new Exception($"Class {sessionClass.FullName} is not inherited from MvcCore.Session.");
			}
			this.sessionClass = sessionClass;
			return this;
		}
		public virtual Application SetSessionClass<SessionClass>() {
			Type sessionClass = typeof(SessionClass);
			if (!(typeof(Session).IsAssignableFrom(sessionClass))) {
				throw new Exception($"Class {sessionClass.FullName} is not inherited from MvcCore.Session.");
			}
			this.sessionClass = sessionClass;
			return this;
		}
		public virtual Type GetSessionClass() {
			return this.sessionClass;
		}

		public virtual Application SetViewClass(Type viewClass) {
			if (!(typeof(View).IsAssignableFrom(viewClass))) {
				throw new Exception($"Class {viewClass.FullName} is not inherited from MvcCore.View.");
			}
			this.viewClass = viewClass;
			return this;
		}
		public virtual Application SetViewClass<ViewClass>() {
			Type viewClass = typeof(ViewClass);
			if (!(typeof(View).IsAssignableFrom(viewClass))) {
				throw new Exception($"Class {viewClass.FullName} is not inherited from MvcCore.View.");
			}
			this.viewClass = viewClass;
			return this;
		}
		public virtual Type GetViewClass() {
			return this.viewClass;
		}

		public virtual Application SetDebugClass(Type debugClass) {
			this.debugClass = debugClass;
			return this;
		}
		public virtual Application SetDebugClass<DebugClass>() {
			this.debugClass = typeof(DebugClass);
			return this;
		}
		public virtual Type GetDebugClass() {
			return this.debugClass;
		}

		public virtual Router GetRouter (params Route[] routes) {
			return Router.GetInstance(routes);
		}
		public virtual Route GetCurrentRoute () {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return context.CurrentRoute;
			}
			return null;
		}
		public virtual Controller GetController() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return context.Controller;
			}
			return null;
		}
		public virtual Request GetRequest() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return context.Request;
			}
			return null;
		}
		public virtual Response GetResponse() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return context.Response;
			}
			return null;
		}

		public virtual string GetControllersDir() {
			return this.controllersDir;
		}
		public virtual Application SetControllersDir(string controllersDir) {
			this.controllersDir = controllersDir;
			return this;
		}
		public virtual string GetViewsDir() {
			return this.viewsDir;
		}
		public virtual Application SetViewsDir(string viewsDir) {
			this.viewsDir = viewsDir;
			return this;
		}
		public virtual string[] GetDefaultControllerAndActionNames () {
			return new string[] {
				this.defaultControllerName,
				this.defaultControllerDefaultActionName
			};
		}
		public virtual Application SetDefaultControllerName(string defaultControllerName) {
			this.defaultControllerName = defaultControllerName;
			return this;
		}
		public virtual Application SetDefaultControllerDefaultActionName(string defaultActionName) {
			this.defaultControllerDefaultActionName = defaultActionName;
			return this;
		}
		public virtual Application SetDefaultControllerErrorActionName(string defaultControllerErrorActionName) {
			this.defaultControllerErrorActionName = defaultControllerErrorActionName;
			return this;
		}
		public virtual Application SetDefaultControllerNotFoundActionName(string defaultControllerNotFoundActionName) {
			this.defaultControllerNotFoundActionName = defaultControllerNotFoundActionName;
			return this;
		}
	}
}
