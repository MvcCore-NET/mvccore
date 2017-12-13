using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using MvcCore.Tools;

namespace MvcCore {
	public class View: System.Dynamic.DynamicObject {

		/// <summary>
		/// View output document type HTML4
		/// </summary>
		public const string DOCTYPE_HTML4 = "HTML4";
		/// <summary>
		/// View output document type XHTML
		/// </summary>
		public const string DOCTYPE_XHTML = "XHTML";
		/// <summary>
		/// View output document type HTML5
		/// </summary>
		public const string DOCTYPE_HTML5 = "HTML5";
		/// <summary>
		/// View script files extenion in Views application directory
		/// </summary>
		public const string EXTENSION = ".cshtml";

		/// <summary>
		/// Document type (HTML, XHTML or anything you desire)
		/// </summary>
		public static string Doctype = View.DOCTYPE_HTML5;
		/// <summary>
		/// Controller action templates directory placed in '/App/Views' dir. For read & write.
		/// </summary>
		public static string ScriptsDir = "Scripts";
		/// <summary>
		/// views helpers directory placed in '/Views' dir. For read & write.
		/// </summary>
		public static string HelpersDir = "Helpers";
		/// <summary>
		/// Layout templates directory placed in '/Views' dir. For read & write.
		/// </summary>
		public static string LayoutsDir = "Layouts";
		/// <summary>
		/// Helpers classes - base class names. For read & write.
		/// </summary>
		public static List<string> HelpersClassBases = new List<string>() {/*'\MvcCore\Ext\View\Helpers\'*/};

		public static IRazorEngineService renderService = null;

		public object this[string key] {
			get {
				if (this._data.ContainsKey(key))
					return this._data[key];
				return null;
			}
			set {
				this._data[key] = value;
			}
		}
		private Dictionary<string, object> _data = new Dictionary<string, object>();

		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			if (this._data.ContainsKey(binder.Name)) {
				result = this._data[binder.Name];
				return true;
			}
			return base.TryGetMember(binder, out result);
		}
		public override bool TrySetMember(SetMemberBinder binder, object value) {
			this._data[binder.Name] = value;
			return true;
		}
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
			string className;
			MethodInfo methodInfo;
			string method = binder.Name;
			foreach (string helperClassBase in View.HelpersClassBases) {
				className = helperClassBase + method.Substring(0, 1).ToUpper() + method.Substring(1);
				if (View._helpers.ContainsKey(className)) {
					methodInfo = View._helpers[className];
					result = methodInfo.Invoke(null, args);
					return true;
				} else {
					Type helperType = Tool.GetTypeGlobaly(className);
					if (helperType is Type) {
						methodInfo = helperType.GetMethod("Render", BindingFlags.Static | BindingFlags.Public);
						if (methodInfo is MethodInfo) {
							View._helpers[className] = methodInfo;
							result = methodInfo.Invoke(null, args);
							return true;
						}
					}
				}
			}
			return base.TryInvokeMember(binder, args, out result);
		}

		/// <summary>
		/// Executed controller
		/// </summary>
		public MvcCore.Controller Controller;
		/// <summary>
		/// Rendered content
		/// </summary>
		private string _content = "";
		/// <summary>
		/// Currently rendered php/html file path
		/// </summary>
		private List<string> _renderedFullPaths = new List<string>();

		/// <summary>
		/// Originaly declared dynamic properties to protect from __set() magic method
		/// </summary>
		protected static List<string> originalyDeclaredProperties = new List<string>() {
			"Controller",
			"_content",
			"_data",
			"_renderedFullPaths",
		};

		/// <summary>
		/// Helpers instances storrage
		/// </summary>
		private static Dictionary<string, MethodInfo> _helpers = new Dictionary<string, MethodInfo>();

		static View() {
			MvcCore.Application app = MvcCore.Application.GetInstance();
			View.HelpersClassBases = new List<string>() {
				"MvcCore.Ext.View.Helpers.",
				// "Views.Helpers." :
				String.Join(".", new string[] {
					app.GetViewsDir(),
					View.HelpersDir
				}) + "."
			};

			var renderConfig = new TemplateServiceConfiguration();
			renderConfig.Debug = true;
			renderConfig.BaseTemplateType = typeof(BaseTemplate<>);
			View.renderService = RazorEngineService.Create(renderConfig);
		}
		public static void AddHelpersClassBases(params string[] helpersClassBases) {
			foreach (string helpersClassBase in helpersClassBases) {
				View.HelpersClassBases.Add(helpersClassBase.TrimEnd('.') + ".");
			}
		}
		public static string GetViewScriptFullPath(string typePath = "", string corectedRelativePath = "") {
			MvcCore.Application app = MvcCore.Application.GetInstance();
			return String.Join("/", new string[] {
				app.GetRequest().AppRoot,
				app.GetViewsDir(),
				typePath,
				corectedRelativePath + MvcCore.View.EXTENSION
			});
		}
		public View(MvcCore.Controller controller) {
			this.Controller = controller;
		}
		public View SetUp(View view) {
			foreach (var item in view._data) {
				if (View.originalyDeclaredProperties.Contains(item.Key)) continue;
				this._data[item.Key] = item.Value;
			}
			return this;
		}
		public View SetContent(string content) {
			this._content = content;
			return this;
		}
		public string GetContent() {
			return this._content;
		}
		public Controller GetController() {
			return this.Controller;
		}
		public string RenderScript(string relativePath = "") {
			return this.Render(View.ScriptsDir, relativePath);
		}
		public string RenderLayout(string relativePath = "") {
			return this.Render(View.LayoutsDir, relativePath);
		}
		public string RenderLayoutAndContent(string relativePath = "", string content = "") {
			this._content = content;
			return this.Render(View.LayoutsDir, relativePath);
		}
		public string Render(string typePath = "", string relativePath = "") {
			if (typePath.Length == 0) typePath = View.ScriptsDir;
			string result = "";
			relativePath = this._correctRelativePath(
				this.Controller.GetRequest().AppRoot, 
				typePath, 
				relativePath
			);
			string viewScriptFullPath = View.GetViewScriptFullPath(typePath, relativePath);
			if (!File.Exists(viewScriptFullPath)) {
				throw new System.Exception($"Template not found in path: '{viewScriptFullPath}'.");
			}
			this._renderedFullPaths.Add(viewScriptFullPath);
			result = this.include(viewScriptFullPath);
			this._renderedFullPaths.RemoveAt(this._renderedFullPaths.Count - 1);// unset last
			return result;
		}
		public string Url(string controllerActionOrRouteName = "Index:Index", object routeParams = null) {
			return MvcCore.Router.GetInstance().Url(
				Application.GetRequestContext(), controllerActionOrRouteName, routeParams
			);
		}
		private string _correctRelativePath(string appRoot, string typePath, string relativePath) {
			string result = relativePath.Replace('\\', '/');
			if (relativePath.Substring(0, 2) == "./") {
				MvcCore.Application app = MvcCore.Application.GetInstance();
				string typedViewDirFullPath = String.Join("/", new string[] {
					appRoot, app.GetViewsDir(), typePath
				});
				string lastRenderedFullPath = this._renderedFullPaths[this._renderedFullPaths.Count - 1];
				string renderedRelPath = lastRenderedFullPath.Substring(typedViewDirFullPath.Length);
				int renderedRelPathLastSlashPos = renderedRelPath.LastIndexOf('/');
				if (renderedRelPathLastSlashPos > -1) {
					result = renderedRelPath.Substring(0, renderedRelPathLastSlashPos + 1) + relativePath.Substring(2);
					result = result.TrimStart('/');
				}
			}
			return result;
		}
		// TODO: RazorEngine
		protected virtual string include (string viewScriptFullPath) {

			long templateLastWriteTime = Tool.GetFileLastWriteTimestamp(viewScriptFullPath);
			Type viewType = typeof(View);
			string templateCacheKey = (viewScriptFullPath + ":" + templateLastWriteTime.ToString()).MD5();

			if (!View.renderService.IsTemplateCached(templateCacheKey, viewType)) {
				string viewSource = System.IO.File.ReadAllText(viewScriptFullPath);
				renderService.AddTemplate(templateCacheKey, viewSource);
			}

			string renderedText = renderService.RunCompile(
				templateCacheKey, viewType, null, new DynamicViewBag(this._data)
			);

			return renderedText;
		}
	}
	public abstract class BaseTemplate<T> : TemplateBase<T> {
		private View _view = null;
		public MvcCore.View View {
			get {
				return this._view;
			}
		}
		public BaseTemplate() {
		}
	}
}
