using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcCore;

namespace Controllers {
	public class Index: MvcCore.Controller {
		public void IndexAction () {
			this.view.Title = "Homepage!";
		}
	}
}