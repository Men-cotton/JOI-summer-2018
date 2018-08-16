using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
	class Inst
	{
		public string opcode;//演算子
		public char c;//比較（Char演算のときのみ使用）
		public int x;//分岐先1（Jump,Split演算で使用）
		public int y;//分岐先2（Split演算のときのみ使用）
	}
	class Executer
	{
		public static List<Inst> commandlist = new List<Inst>();
		static string regexp;
		public static int Recursive(ref int pc, ref int sp)
		{
			switch (commandlist[pc].opcode)
			{
				case "Char":
					if (!(commandlist[pc].c == regexp[sp] || commandlist[pc].c == '.'))//先頭同士がマッチするか確認
						return 0;
					pc++; sp++;
					return Recursive(ref pc, ref sp);//一個ずつ進める
				case "Match":
					return 1;
				case "Jump":
					return Recursive(ref commandlist[pc].x, ref sp);//xの命令を実行
				case "Split":
					if (Recursive(ref commandlist[pc].x, ref sp) == 1)//xの命令を実行
						return 1;
					return Recursive(ref commandlist[pc].y, ref sp);//yの命令を実行
			}
			return -1;//エラー
		}
	}
	class Convert //このclass内では、関数は命令が開始するカウンタを返す
	{
		static int Make_Char(string s)//Charコマンドを作る
		{
			int ret = Executer.commandlist.Count;
			Inst inst = new Inst() { opcode = "Chara", c = s[0], };
			Executer.commandlist.Add(inst);
			return ret;
		}
		static int Make_Jump()//Jumpコマンドを作る
		{
			int ret = Executer.commandlist.Count;
			Inst inst = new Inst() { opcode = "Jump", };
			Executer.commandlist.Add(inst);
			return ret;
		}
		static int Make_Sprit()//Spritコマンドを作る
		{
			int ret = Executer.commandlist.Count;
			Inst inst = new Inst() { opcode = "Sprit", };
			Executer.commandlist.Add(inst);
			return ret;
		}
		static int Expression(string s)//繰り返し軍団をばらす関数
		{
			int ret = Executer.commandlist.Count;
			switch (s[s.Length - 1])
			{
				case ('?'):
				{
					int sprit = Make_Sprit();
					Executer.commandlist[sprit].x = Expression(s.Substring(0, s.Length - 1));
					Executer.commandlist[sprit].y = Executer.commandlist.Count;
					break;
				}
				case ('*'):
				{
					int sprit = Make_Sprit();
					Executer.commandlist[sprit].x = Expression(s.Substring(0, s.Length - 1));
					int jump = Make_Jump();
					Executer.commandlist[jump].x = sprit;
					Executer.commandlist[sprit].y = Executer.commandlist.Count;
					break;
				}
				case ('+'):
				{
					int start = Expression(s.Substring(0, s.Length - 1));
					int sprit = Make_Sprit();
					Executer.commandlist[sprit].x = start; Executer.commandlist[sprit].y = Executer.commandlist.Count;
					break;
				}
				default:
				{
					Make_Char(s);
					break;
				}
			}
			return ret;
		}
		static int Term(string s)//連接をばらす関数
		{
			int ret = Executer.commandlist.Count;
			int start_string = 0;
			for (int i = 1; i < s.Length; i++)
			{
				if (char.IsLetter(s[i]))//文字が出てきたら直前で区切り、繰り返しをばらす過程へ
				{
					Expression(s.Substring(start_string, i - start_string));
					start_string = i;
				}
			}
			Expression(s.Substring(start_string, s.Length - start_string));
			return ret;
		}
		public static int Sentence(string s)//選択をばらす関数
		{
			bool found = false;
			for (int i = 0; i < s.Length && !found; i++)
			{
				if (s[i] == '|')
				{
					found = true;
					int sprit = Make_Sprit();
					Executer.commandlist[sprit].x = Term(s.Substring(0, i));//調べた個所以前には選択はないので、連接をばらす過程へ
					int jump = Executer.commandlist.Count; Make_Jump();
					Executer.commandlist[sprit].y = Sentence(s.Substring(i + 1, s.Length - (i + 1)));
					Executer.commandlist[jump].x = Executer.commandlist.Count;
					return sprit;
				}
			}
			return Term(s);
		}
	}
	class Program
	{
		static void Main(string[] args)
		{
			Convert.Sentence("");
		}
	}
}