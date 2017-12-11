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

		protected Controller controller = null;
		protected Request request = null;
		protected Response response = null;
		protected Router router = null;

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
			this.requestClass = requestClass;
			return this;
		}
		public virtual Application SetRequestClass<RequestClass>() {
			this.requestClass = typeof(RequestClass);
			return this;
		}
		public virtual Type GetRequestClass() {
			return this.requestClass;
		}

		public virtual Application SetResponseClass(Type responseClass) {
			this.responseClass = responseClass;
			return this;
		}
		public virtual Application SetResponseClass<ResponseClass>() {
			this.responseClass = typeof(ResponseClass);
			return this;
		}
		public virtual Type GetResponseClass() {
			return this.responseClass;
		}

		public virtual Application SetRouterClass(Type routerClass) {
			this.routerClass = routerClass;
			return this;
		}
		public virtual Application SetRouterClass<RouterClass>() {
			this.routerClass = typeof(RouterClass);
			return this;
		}
		public virtual Type GetRouterClass() {
			return this.routerClass;
		}

		public virtual Application SetConfigClass(Type configClass) {
			this.configClass = configClass;
			return this;
		}
		public virtual Application SetConfigClass<ConfigClass>() {
			this.configClass = typeof(ConfigClass);
			return this;
		}
		public virtual Type GetConfigClass() {
			return this.configClass;
		}

		public virtual Application SetSessionClass(Type sessionClass) {
			this.sessionClass = sessionClass;
			return this;
		}
		public virtual Application SetSessionClass<SessionClass>() {
			this.sessionClass = typeof(SessionClass);
			return this;
		}
		public virtual Type GetSessionClass() {
			return this.sessionClass;
		}

		public virtual Application SetViewClass(Type viewClass) {
			this.viewClass = viewClass;
			return this;
		}
		public virtual Application SetViewClass<ViewClass>() {
			this.viewClass = typeof(ViewClass);
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

		public virtual Router GetRouter () {
			return this.router;
		}
		public virtual Controller GetController() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return Application.GetRequestContext().Controller;
			}
			return null;
		}
		public virtual Request GetRequest() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return Application.GetRequestContext().Request;
			}
			return null;
		}
		public virtual Response GetResponse() {
			Applications.RequestContext context = Application.GetRequestContext();
			if (context is Applications.RequestContext) {
				return Application.GetRequestContext().Response;
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
