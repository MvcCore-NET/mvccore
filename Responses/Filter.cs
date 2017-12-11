using System;
using System.Web;
using System.IO;

/*
HttpResponse response = HttpContext.Current.Response;
Responses.Filter filter = new Responses.Filter(response.Filter);
// add any response string replace filter method
filter.TransformString += CustomMethodToTransformString;
filter.TransformStream += CustomMethodToTransformStream;
response.Filter = filter;
*/

namespace MvcCore.Responses {
	public class Filter: Stream {

		public event Func<byte[], byte[]> TransformWrite;
		public event Func<string, string> TransformWriteString;

		public event Func<string, string> TransformString;
		public event Func<MemoryStream, MemoryStream> TransformStream;

		public event Action<MemoryStream> CaptureStream;
		public event Action<string> CaptureString;

		public override bool CanRead {
			get { return true; }
		}
		public override bool CanSeek {
			get { return true; }
		}
		public override bool CanWrite {
			get { return true; }
		}
		public override long Length {
			get { return 0; }
		}
		public bool IsCaptured {
			get {
				if (
					this.CaptureStream != null || this.CaptureString != null ||
					this.TransformStream != null || this.TransformString != null
				) {
					return true;
				}
				return false;
			}
		}
		public bool IsOutputDelayed {
			get {
				if (this.TransformStream != null || this.TransformString != null) {
					return true;
				}
				return false;
			}
		}
		public override long Position {
			get { return this._position; }
			set { this._position = value; }
		}
		protected long _position;

		protected Stream stream;
		protected MemoryStream cacheStream = new MemoryStream(5000);
		protected int cachePointer = 0;

		public Filter(Stream responseStream) {
			this.stream = responseStream;
		}

		public virtual void OnCaptureStream(MemoryStream ms) {
			if (CaptureStream != null) this.CaptureStream(ms);
		}
		public void OnCaptureStringInternal(MemoryStream ms) {
			if (this.CaptureString != null) {
				string content = HttpContext.Current.Response.ContentEncoding.GetString(ms.ToArray());
				this.OnCaptureString(content);
			}
		}
		public virtual void OnCaptureString(string output) {
			if (this.CaptureString != null) {
				this.CaptureString(output);
			}
		}
		public virtual byte[] OnTransformWrite(byte[] buffer) {
			if (this.TransformWrite != null) {
				return this.TransformWrite(buffer);
			}
			return buffer;
		}
		public byte[] OnTransformWriteStringInternal(byte[] buffer) {
			System.Text.Encoding encoding = HttpContext.Current.Response.ContentEncoding;
			string output = this.OnTransformWriteString(encoding.GetString(buffer));
			return encoding.GetBytes(output);
		}
		public string OnTransformWriteString(string value) {
			if (this.TransformWriteString != null)
				return this.TransformWriteString(value);
			return value;
		}
		public virtual MemoryStream OnTransformCompleteStream(MemoryStream ms) {
			if (this.TransformStream != null) {
				return this.TransformStream(ms);
			}
			return ms;
		}
		public string OnTransformCompleteString(string responseText) {
			if (TransformString != null) {
				this.TransformString(responseText);
			}
			return responseText;
		}
		public MemoryStream OnTransformCompleteStringInternal(MemoryStream ms) {
			if (this.TransformString == null) return ms;
			string content = HttpContext.Current.Response.ContentEncoding.GetString(ms.ToArray());
			content = TransformString(content);
			byte[] buffer = HttpContext.Current.Response.ContentEncoding.GetBytes(content);
			ms = new MemoryStream();
			ms.Write(buffer, 0, buffer.Length);
			return ms;
		}

		public override long Seek(long offset, System.IO.SeekOrigin direction) {
			return this.stream.Seek(offset, direction);
		}
		public override void SetLength(long length) {
			this.stream.SetLength(length);
		}
		public override void Close() {
			this.stream.Close();
		}
		public override void Flush() {
			if (this.IsCaptured && this.cacheStream.Length > 0) {
				this.cacheStream = this.OnTransformCompleteStream(cacheStream);
				this.cacheStream = this.OnTransformCompleteStringInternal(cacheStream);
				this.OnCaptureStream(this.cacheStream);
				this.OnCaptureStringInternal(this.cacheStream);
				if (this.IsOutputDelayed) { 
					this.stream.Write(
						this.cacheStream.ToArray(), 0, (int)this.cacheStream.Length
					);
				}
				this.cacheStream.SetLength(0);
			}
			this.stream.Flush();
		}
		public override int Read(byte[] buffer, int offset, int count) {
			return this.stream.Read(buffer, offset, count);
		}
		public override void Write(byte[] buffer, int offset, int count) {
			if (this.IsCaptured) {
				this.cacheStream.Write(buffer, 0, count);
				this.cachePointer += count;
			}
			if (this.TransformWrite != null) { 
				buffer = this.OnTransformWrite(buffer);
			}
			if (this.TransformWriteString != null) {
				buffer = this.OnTransformWriteStringInternal(buffer);
			}
			if (!this.IsOutputDelayed) {
				this.stream.Write(buffer, offset, buffer.Length);
			}
		}
	}
}