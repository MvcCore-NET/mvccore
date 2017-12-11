using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcCore {
	public class Router {
		protected static Router instance = null;
		protected List<Route> routes = new List<Route>();
		protected Dictionary<string, Route> urlRoutes = new Dictionary<string, Route>();
		protected Route currentRoute = null;
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
		public void AddRoutes(Route[] routes, bool prepend = false) {
			//if (prepend) $routes = array_reverse($routes);
			//string routeName;
			//Route route;
			//for (int i = 0, l = routes.Length; i < l; i += 1) {
			//	string routeName
			//}
			//for ($routes as $routeName => & $route) {
			//$routeType = gettype($route);
			//$numericKey = is_numeric($routeName);
			//	if ($route instanceof \MvcCore\Route) {
			//		if (!$numericKey) {
			//		$route->Name = $routeName;
			//		}
			//	} else if ($routeType == 'array') {
			//		if (!$numericKey) {
			//		$route['name'] = $routeName;
			//		}
			//	} else if ($routeType == 'string') {
			//	// route name is always Controller:Action
			//	$route = array(
			//		'name'		=> $routeName,
			//		'pattern'	=> $route
			//	);
			//	}
			//$this->AddRoute($route, $prepend);
			//}
			//return $this;
		}
	}
}
