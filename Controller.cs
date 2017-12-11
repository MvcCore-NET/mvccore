namespace MvcCore {
	public class Controller {
		protected Request request;
		protected Response response;
		public Controller(Request request, Response response) {
			this.request = request;
			this.response = response;
		}
	}
}
