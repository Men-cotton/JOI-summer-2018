using System;
using System.Collections.Generic;
using System.Linq;

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
		public static string text;
		public static int Recursive(int pc, int sp)
		{
			switch (commandlist[pc].opcode)
			{
				case "Char":
					if (sp >= text.Length)
						return 0;//対応する文字列がないのでアウト
					switch (commandlist[pc].c)
					{
						case ('!')://[a-z]
							return char.IsLower(text[sp]) ? Recursive(pc + 1, sp + 1) : 0;
						case ('#')://[A-Z]
							return char.IsUpper(text[sp]) ? Recursive(pc + 1, sp + 1) : 0;
						case ('.'):
							return true ? Recursive(pc + 1, sp + 1) : 0;
						default:
							return commandlist[pc].c == text[sp] ? Recursive(pc + 1, sp + 1) : 0;
					}
				case "Match":
					return sp >= text.Length ? 1 : 0;//文字列を使い切っていたらマッチ成功
				case "Jump":
					return Recursive(commandlist[pc].x, sp);//xの命令を実行
				case "Split":
					if (Recursive(commandlist[pc].x, sp) == 1)//xの命令を実行
						return 1;
					return Recursive(commandlist[pc].y, sp);//yの命令を実行
			}
			return -1;//エラー
		}
	}
	class Convert //このclass内では、関数は命令が開始するカウンタを返す
	{
		public static char[] special_letters = new char[] { '.', '!', '#' };
		static int Make_Char(string s)//Charコマンドを作る
		{
			int ret = Executer.commandlist.Count;
			Inst inst = new Inst() { opcode = "Char", c = s[0], };
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
		static int Make_Split()//Splitコマンドを作る
		{
			int ret = Executer.commandlist.Count;
			Inst inst = new Inst() { opcode = "Split", };
			Executer.commandlist.Add(inst);
			return ret;
		}
		static int Expression(string s)//繰り返し軍団をばらす関数
		{
			int ret = Executer.commandlist.Count;
			if (string.IsNullOrEmpty(s)) return ret - 1;
			switch (s[s.Length - 1])
			{
				case ('?'):
				{
					int split = Make_Split();
					Executer.commandlist[split].x = Expression(s.Substring(0, s.Length - 1));
					Executer.commandlist[split].y = Executer.commandlist.Count;
					break;
				}
				case ('*'):
				{
					int split = Make_Split();
					Executer.commandlist[split].x = Expression(s.Substring(0, s.Length - 1));
					int jump = Make_Jump();
					Executer.commandlist[jump].x = split;
					Executer.commandlist[split].y = Executer.commandlist.Count;
					break;
				}
				case ('+'):
				{
					int start = Expression(s.Substring(0, s.Length - 1));
					int split = Make_Split();
					Executer.commandlist[split].x = start; Executer.commandlist[split].y = Executer.commandlist.Count;
					break;
				}
				case (')')://この時点で繰り返しと()以外はないはずなので、)が最後に来ているということはs全体が()
				{
					Sentence(s.Substring(1, s.Length - 2));
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
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '(')
				{
					Expression(s.Substring(start_string, i - start_string));
					start_string = i;
					int count = 0;
					do//()内を一文字として考える
					{
						switch (s[i])
						{
							case ('('): { count++; break; }
							case (')'): { count--; break; }
							default: break;
						}
						i++;
					} while (count > 0);
					if (i >= s.Length)//最後までカッコで探索した場合
						break;
				}
				// )aというパターンを考慮し、else ifでは無くす
				if (char.IsLetter(s[i]) || special_letters.Contains(s[i]))//文字が出てきたら直前で区切り、繰り返しをばらす過程へ
				{
					Expression(s.Substring(start_string, i - start_string));
					start_string = i;
				}
			}
			Expression(s.Substring(start_string, s.Length - start_string));
			return ret;
		}
		static int Sentence(string s)//選択をばらす関数
		{
			bool found = false;
			int bracketscount = 0;//()を別々の文字列にしないように注意
			for (int i = 0; i < s.Length && !found; i++)
			{
				if (s[i] == '(') bracketscount++;
				else if (s[i] == ')') bracketscount--;
				else if (s[i] == '|' && bracketscount == 0)
				{
					found = true;
					int split = Make_Split();
					Executer.commandlist[split].x = Term(s.Substring(0, i));//調べた個所以前には選択はないので、連接をばらす過程へ
					int jump = Executer.commandlist.Count; Make_Jump();
					Executer.commandlist[split].y = Sentence(s.Substring(i + 1, s.Length - (i + 1)));
					Executer.commandlist[jump].x = Executer.commandlist.Count;
					return split;
				}
			}
			return Term(s);
		}
		public static void Input_regexp(string s)//開始・終了地点をばらす関数
		{
			if (s[0] == '^') s = s.Substring(1, s.Length - 1);
			if (s[s.Length - 1] == '$') s = s.Substring(0, s.Length - 1);
			Sentence(s);
			Inst finish = new Inst() { opcode = "Match", };
			Executer.commandlist.Add(finish);
		}
	}
	class Program
	{
		static void Show_Command()
		{
			int count = 1;
			foreach (var command in Executer.commandlist)
			{
				Console.Write(count.ToString("D3") + ":");
				if (command.opcode != "") Console.Write(" " + command.opcode);
				if (command.c != 0) Console.Write(" " + command.c);
				if (command.x != 0) Console.Write(" " + (command.x + 1));
				if (command.y != 0) Console.Write(" " + (command.y + 1));
				Console.WriteLine();
				count++;
			}
		}
		static void Main(string[] args)
		{
			Convert.Input_regexp("^(bc*d|ef*g|h*i(j*|k)*)$");//!=[a-z] #=[A-Z]
			Show_Command();
			Executer.text = Console.ReadLine();
			Console.WriteLine(Executer.Recursive(0, 0) == 1 ? "YES" : "NO");
		}
	}
}
