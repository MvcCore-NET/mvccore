using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcCore {
	public delegate void PreRouteHandler(Request request, Response response);
	public delegate void PreDispatchHandler(Request request, Response response);
	public delegate void PostDispatchHandler(Request request, Response response);
}
