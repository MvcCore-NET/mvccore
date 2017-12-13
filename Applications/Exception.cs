using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcCore.Applications {
	public class Exception : System.Exception {
		public int Code = 0;
		public Exception(string errorMessage) : base(errorMessage) {
		}
		public Exception(string errorMessage, int code) : base(errorMessage) {
			this.Code = code;
		}
		public Exception(string errorMessage, int code, Exception innerException) : base(errorMessage, innerException) {
			this.Code = code;
		}
		public Exception(string errorMessage, Exception innerException, int code) : base(errorMessage, innerException) {
			this.Code = code;
		}
	}
}
