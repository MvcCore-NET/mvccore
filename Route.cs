using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcCore {
	public class Route {
		/// <summary>
		/// Route name, your custom keyword/term 
		/// or pascal case combination of 'Any.Namespace.Controller:Action'.
		/// </summary>
		public string Name = "";
		/// <summary>
		/// Controller name in pascal case.
		/// </summary>
		public string Controller = "";
		/// <summary>
		/// Action name in pascal case.
		/// </summary>
		public string Action = "";
		/// <summary>
		/// Route match pattern in classic form:
		/// "^/url\-begin/([^/]*)/([^/]*)/(.*)".
		/// </summary>
		public string Pattern = "";
		/// <summary>
		/// Route reverse address form from Regex pattern 
		/// in form: "/url-begin/{%first}/{%second}/{%third}".
		/// </summary>
		public string Reverse = "";
		/// <summary>
		/// Route params with default values in form:
		/// { first = 1, second = 2, third = 3 }.
		/// </summary>
		public Dictionary<string, object> Params = new Dictionary<string, object>();
		private object routeParams;

		internal Regex MatchRegex = null;

		public static Route GetInstance(object config) {
			return new Route(config);
		}
		public static Route GetInstance(
			string name = null, 
			string controller = null,
			string action = null,
			string pattern = null,
			string reverse = null,
			object @params = null
		) {
			return new MvcCore.Route(name, controller, action, pattern, reverse, @params);
		}

		public Route(object values) {
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(values);
			object value;
			string localName;
			Type currentType = this.GetType();
			foreach (PropertyDescriptor prop in props) {
				value = prop.GetValue(values);
				localName = prop.Name.Substring(0, 1).ToUpper() + prop.Name.Substring(1);
				if (prop.Name == "params" && value != null) {
					this.setUpParams(value);
				} else if (value != null && value.ToString() != "") {
					currentType.GetField(
						localName, BindingFlags.Instance | BindingFlags.Public
					).SetValue(this, value);
					if (prop.Name == "pattern") {
						this.MatchRegex = new Regex(value.ToString());
					}
				}
			}
		}

		public Route(
			string name = null,
			string controller = null,
			string action = null,
			string pattern = null,
			string reverse = null,
			object @params = null
		) {
			if (!String.IsNullOrEmpty(name)) this.Name = name;
			if (!String.IsNullOrEmpty(controller)) this.Controller = controller;
			if (!String.IsNullOrEmpty(action)) this.Action = action;
			if (!String.IsNullOrEmpty(pattern)) {
				this.Pattern = pattern;
				this.MatchRegex = new Regex(pattern);
			}
			if (!String.IsNullOrEmpty(reverse)) this.Reverse = reverse;
			if (@params != null) this.setUpParams(@params);
		}
		protected virtual Route setUpParams(object @params) {
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(@params);
			foreach (PropertyDescriptor prop in props) {
				this.Params.Add(prop.Name, prop.GetValue(@params));
			}
			return this;
		}
		public Route SetName(string name) {
			this.Name = name;
			return this;
		}
		public Route SetController(string controller) {
			this.Controller = controller;
			return this;
		}
		public Route SetAction(string action) {
			this.Action = action;
			return this;
		}
		public Route SetPattern(string pattern) {
			this.Pattern = pattern;
			this.MatchRegex = new Regex(pattern);
			return this;
		}
		public Route SetReverse(string reverse) {
			this.Reverse = reverse;
			return this;
		}
		public Route SetParams(object @params) {
			return this.setUpParams(@params);
		}
	}
}
