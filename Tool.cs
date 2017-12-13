using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace MvcCore {
	public class Tool {
		protected static DateTime beginOfTime = new DateTime(1970, 1, 1, 0, 0, 0);

		protected static Regex dashedOrUnderscoredFromPascal = new Regex(@"([a-z])([A-Z])");

		protected static Regex pascalFromDashed = new Regex(@"\-([^\-])");
		protected static Regex underscoredFromPascal = new Regex(@"_([^_])");

		public static string GetDashedFromPascalCase(string pascalCase = "") {
			pascalCase = pascalCase.Substring(0, 1).ToLower() + pascalCase.Substring(1);
			return Tool.dashedOrUnderscoredFromPascal.Replace(pascalCase, "$1-$2").ToLower();
		}
		public static string GetPascalCaseFromDashed(string dashed = "") {
			return Tool.getXCaseFromDashed(Tool.pascalFromDashed, dashed);
		}
		public static string GetUnderscoredFromPascalCase(string pascalCase = "") {
			pascalCase = pascalCase.Substring(0, 1).ToLower() + pascalCase.Substring(1);
			return Tool.dashedOrUnderscoredFromPascal.Replace(pascalCase, "$1_$2").ToLower();
		}
		public static string GetPascalCaseFromUnderscored(string dashed = "") {
			return Tool.getXCaseFromDashed(Tool.underscoredFromPascal, dashed);
		}
		protected static string getXCaseFromDashed(Regex regex, string dashed) {
			string[] dashedArr = dashed.Split('/');
			string dashedArrItem;
			for (int i = 0, l = dashedArr.Length; i < l; i += 1) {
				dashedArrItem = regex.Replace(
					dashedArr[i], new MatchEvaluator(match => {
						return match.Value.Substring(1, 1).ToString().ToUpper();
					})
				);
				dashedArr[i] = dashedArrItem.Substring(0, 1).ToUpper() + dashedArrItem.Substring(1);
			}
			return String.Join("/", dashedArr);
		}
		public static string EncodeJson(object data) {
			StringBuilder result = new StringBuilder();
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			serializer.Serialize(data, result);
			return result.ToString();
		}
		public static void EncodeJson(object data, StringBuilder output) {
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			serializer.Serialize(data, output);
		}
		public static object DecodeJson(string jsonStr, Type resultType) {
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Deserialize(jsonStr, resultType);
		}
		public static object DecodeJson<ResultType>(string jsonStr) {
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Deserialize<ResultType>(jsonStr);
		}
		/// <summary>
		/// Return Type object by sstring in forms: "Full.Class.Name" or "AssemblyName:Full.Class.Name"
		/// </summary>
		/// <param name="fullClassName" type="String">"Full.Class.Name" or "AssemblyName:Full.Class.Name"</param>
		/// <returns type="Type">Desired type</returns>
		public static Type GetTypeGlobaly(string fullClassName) {
			Type type = Type.GetType(fullClassName);
			if (type != null) return type;
			if (fullClassName.IndexOf(":") > -1) {
				string[] fullNameAndAssembly = fullClassName.Split(':');
				type = Tool.GetTypeGlobaly(fullNameAndAssembly[0], fullNameAndAssembly[1]);
			}
			if (type == null) {
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (System.Reflection.Assembly assembly in assemblies) {
					type = assembly.GetType(fullClassName);
					if (type != null) {
						break;
					}
				}
			}
			return type;
		}
		/// <summary>
		/// Return Type object by two strings in form: "AssemblyName", "Full.Class.Name"
		/// </summary>
		/// <param name="assemblyName" type="String">"AssemblyName" for AssemblyName.dll</param>
		/// <param name="fullClassName" type="String">Full class name including namespace</param>
		/// <returns type="Type">Desired type</returns>
		public static Type GetTypeGlobaly(string assemblyName, string fullClassName) {
			Type type = null;
			// load all assemblies in folder
			try {
				IEnumerable<Assembly> assemblies =
					from file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
					where Path.GetExtension(file).ToLower() == ".dll"
					select Assembly.LoadFrom(file);
				foreach (System.Reflection.Assembly assembly in assemblies) {
					if (assembly.GetName().Name == assemblyName) {
						type = assembly.GetType(fullClassName);
						break;
					}
				}
			} catch (Exception ex) {
			}
			return type;
		}
		public static void DispatchEvent(object source, string eventName, EventArgs eventArgs) {
			EventInfo eventObject = source.GetType().GetEvent(eventName);
			if (eventObject != null) {
				IEnumerable<FieldInfo> fis = source.GetType().GetFields(
					BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
				);
				foreach (FieldInfo fi in fis) {
					if (fi.Name == eventName + "Event") {
						System.Delegate del = fi.GetValue(source) as System.Delegate;
						List<System.Delegate> invocationList = del.GetInvocationList().ToList();
						foreach (System.Delegate invItem in invocationList) {
							invItem.DynamicInvoke(source, eventArgs);
						}
					}
				}
			}
		}
		public static Dictionary<string, object> ParseQueryString(string queryString = "") {
			Dictionary<string, object> result = new Dictionary<string, object>();
			if (String.IsNullOrEmpty(queryString)) return result;
			queryString = queryString.TrimStart('?');
			string[] exploded = queryString.Split('&');
			string item;
			for (int i = 0, l = exploded.Length; i < l; i += 1) {
				item = exploded[i];
				Tool.ParseQueryStringItem(ref item, ref result);
			}
			return result;
		}
		internal static void ParseQueryStringItem(ref string item, ref Dictionary<string, object> result) {
			string paramName = "";
			object paramValue = "";
			string[] paramValueArr;
			List<string> paramPrevValue;
			Tool.parseQueryStringItem(ref item, ref paramName, ref paramValue);
			if (paramValue is string[]) {
				paramValueArr = paramValue as string[];
				if (result.ContainsKey(paramName)) {
					paramPrevValue = (result[paramName] as string[]).ToList();
					paramPrevValue.Add(paramValueArr[0]);
					result[paramName] = paramPrevValue.ToArray();
				} else {
					result.Add(paramName, paramValue);
				}
			} else if (paramValue is string) {
				result.Add(paramName, paramValue);
			}
		}
		protected static void parseQueryStringItem (ref string item, ref string name, ref object value) {
			// if param name is item[] - return name "item" and value type string[]
			// if param name is item[][] - return name "item" and value type as string[] inside string[]
			// if param name is item vratim klic "item" and value as string
			int pos = item.IndexOf("=");
			if (pos == -1) {
				name = item;
				value = "";
			} else {
				name = item.Substring(0, pos);
				value = item.Substring(pos + 1);
				while (true) {
					pos = name.IndexOf("[]");
					if (pos == name.Length - 2 && pos > 0) { 
						name = name.Substring(0, name.Length - 2);
						value = new object[] { value };
					} else {
						break;
					}
				}
				if (value is object[]) {
					value = value as string[];
				}
			}
		}
		public static long GetFileLastWriteTimestamp(string fileFullPath = "") {
			if (!File.Exists(fileFullPath)) return 0;
			FileInfo fi = new FileInfo(fileFullPath);
			return (long)fi.LastWriteTime.Subtract(Tool.beginOfTime).TotalSeconds;
		}
	}
}