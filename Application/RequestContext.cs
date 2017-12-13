using System.Runtime.Remoting.Messaging;
using MvcCore.Applications;

namespace MvcCore {
	public partial class Application {
		
		public static RequestContext GetRequestContext() {
			return CallContext.GetData("context") as RequestContext;
		}
		public static void SetRequestContext(RequestContext context) {
			CallContext.SetData("context", context);
		}
		public static bool RemoveRequestContext() {
			CallContext.FreeNamedDataSlot("context");
			return true;
		}
		//public static bool HasRequestContext() {
		//	return CallContext.GetData("context") is RequestContext;
		//}
		/*
		private static Dictionary<string, RequestContext> _contexts = new Dictionary<string, RequestContext>();
		private static ReaderWriterLockSlim _contextsLock = new ReaderWriterLockSlim();
		public static RequestContext GetRequestContext() {
			RequestContext result = null;string key = Application.GetProcessAndThreadKey();
			Application._contextsLock.EnterReadLock();
			if (Application._contexts.ContainsKey(key)) {
				result = Application._contexts[key];
			}
			Application._contextsLock.ExitReadLock();
			return result;
		}
		public static void SetRequestContext(RequestContext context) {
			string key = Application.GetProcessAndThreadKey();
			Application._contextsLock.EnterWriteLock();
			if (Application._contexts.ContainsKey(key)) {
				Application._contexts[key] = context;
			} else {
				Application._contexts.Add(key, context);
			}
			Application._contextsLock.ExitWriteLock();
		}
		public static bool RemoveRequestContext() {
			bool result = false;
			string key = Application.GetProcessAndThreadKey();
			Application._contextsLock.EnterReadLock();
			result = Application._contexts.Remove(key);
			Application._contextsLock.ExitReadLock();

			return result;
		}
		//public static bool HasRequestContext() {
		//	bool result = false;
		//	string key = Application.GetProcessAndThreadKey();
		//	Application._contextsLock.EnterReadLock();
		//	if (Application._contexts.ContainsKey(key)) {
		//		result = true;
		//	}
		//	Application._contextsLock.ExitReadLock();
		//	return result;
		//}
		public static string GetProcessAndThreadKey() {
			return Process.GetCurrentProcess().Id + "_" + Thread.CurrentThread.ManagedThreadId + "_" +
				HttpContext.Current.Timestamp.Ticks;
		}
		*/
	}
}
