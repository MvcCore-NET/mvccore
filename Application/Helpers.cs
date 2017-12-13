using MvcCore.Applications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace MvcCore {
	public partial class Application {
		public string GetDefaultControllerIfHasAction(string actionName) {
			string defaultControllerName = this.CompleteControllerName(
				Application.instance.defaultControllerName
			);
			Type defaultCtrlType = Tool.GetTypeGlobaly(defaultControllerName);
			if (defaultCtrlType is Type) {
				MethodInfo mi = defaultCtrlType.GetMethod(
					actionName + "Action", BindingFlags.Instance | BindingFlags.Public
				);
				if (mi is MethodInfo) return defaultControllerName;
			}
			return "";
		}
		public string CompleteControllerName(string controllerNamePascalCase) {
			string firstChar = controllerNamePascalCase.Substring(0, 1);
			if (firstChar == "/") return controllerNamePascalCase;
			return String.Join(".", new string[] {
				Application.instance.controllersDir,
				controllerNamePascalCase
			});
		}
		public bool IsErrorDispatched() {
			string defaultCtrlName = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerName);
			string errorActionName = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerErrorActionName);
			Dictionary<string, object> _params = MvcCore.Application.instance.GetRequest().Params;
			return _params["controller"].ToString() == defaultCtrlName && _params["action"].ToString() == errorActionName;
		}
		public bool IsNotFoundDispatched() {
			string defaultCtrlName = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerName);
			string errorActionName = MvcCore.Tool.GetDashedFromPascalCase(Application.instance.defaultControllerNotFoundActionName);
			Dictionary<string, object> _params = MvcCore.Application.instance.GetRequest().Params;
			return _params["controller"].ToString() == defaultCtrlName && _params["action"].ToString() == errorActionName;
		}
	}
}
