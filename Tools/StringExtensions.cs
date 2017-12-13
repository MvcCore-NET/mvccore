using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcCore.Tools {
	//Extension methods must be defined in a static class
	public static class StringExtensions {
		public static string MD5 (this string s) {
			System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
			byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++) {
				sBuilder.Append(data[i].ToString("x2"));
			}
			return sBuilder.ToString();
		}
		// js equivalent to "@".charCodeAt(0); -> 64
		public static string[] ToUnicodeIndexes (this string s) {
			List<string> r = new List<string>();
			char[] chars = s.ToCharArray();
			for (int i = 0, l = chars.Length; i < l; i += 1) {
				r.Add(
					Convert.ToUInt16(chars[i]).ToString()
				);
			}
			return r.ToArray();
		}
		// js equivalent to String.FromCharCode(64); -> "@"
		public static string FromUnicodeIndexes (string indexes) {
			string r = "";
			string[] arr = indexes.Split(',');
			for (int i = 0, l = arr.Length; i < l; i += 1) {
				r += Convert.ToChar(Convert.ToInt32(arr[i])).ToString();
			}
			return r;
		}
	}
}