using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	class Program
	{
		static bool match(string regexp, string text)//どこかにマッチするか
		{
			if (regexp[0] == '^')//head
				return matchhere(regexp.Substring(1, regexp.Length - 1), text);
			else
			{
				if (matchhere(regexp, text)) return true;
				while (!string.IsNullOrEmpty(text.Substring(1, text.Length - 1)))
				{
					text = text.Substring(1, text.Length - 1);
					if (matchhere(regexp, text)) return true;
				}
				return false;
			}
		}
		static bool matchhere(string regexp, string text)//先頭にマッチするか
		{
			if (string.IsNullOrEmpty(regexp))
				return true;
			if (regexp.Length > 1 && regexp[1] == '*')
				return matchstar(regexp[0], regexp.Substring(2, regexp.Length - 2), text);
			if (regexp[0] == '$' && regexp[1] == '\0')
				return string.IsNullOrEmpty(text);
			if (!string.IsNullOrEmpty(text) && (regexp[0] == '.' || regexp[0] == text[0]))
				return matchhere(regexp.Substring(1, regexp.Length - 1), text.Substring(1, text.Length - 1));
			return false;
		}
		static bool matchstar(char c, string regexp, string text)
		{
			if (matchhere(regexp, text)) return true;
			while (!string.IsNullOrEmpty(text.Substring(1, text.Length - 1)) && (text[1] == c || c == '.'))
			{
				text = text.Substring(1, text.Length - 1);
				if (matchhere(regexp, text)) return true;
			}
			return false;
		}
		static void Main(string[] args)
		{
			Console.WriteLine(match(".*", ""));
		}
	}
}