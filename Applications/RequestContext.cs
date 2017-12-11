using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;

namespace MvcCore.Applications {
	public class RequestContext {
		public Controller Controller = null;
		public Request Request = null;
		public Response Response = null;
	}
}
