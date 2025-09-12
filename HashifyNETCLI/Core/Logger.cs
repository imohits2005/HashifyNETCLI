// *
// *****************************************************************************
// *
// * Copyright (c) 2025 Deskasoft International
// *
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// *
// * The above copyright notice and this permission notice shall be included in all
// * copies or substantial portions of the Software.
// *
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// * SOFTWARE.
// *
// *
// * Please refer to LICENSE file.
// *
// ******************************************************************************
// *

namespace HashifyNETCLI
{
	public class Logger
	{
		private static void Print(int type, string msg = null!, ConsoleColor? foreColor = null, ConsoleColor? backColor = null, bool direct = false, params object[] args)
		{
			if (type == -5)
			{
				Console.WriteLine();
				return;
			}

			string typeStr = type switch
			{
				-1 => "LOG",
				0 => "DEBUG",
				1 => "WARN",
				2 => "ERROR",
				3 => "SCRIPT",
				4 => "INFORMATIVE",
				_ => "UNKNOWN"
			};

			if (foreColor.HasValue)
			{
				Console.ForegroundColor = foreColor.Value;
			}
			else
			{
				switch (type)
				{
					case 0:
						Console.ForegroundColor = ConsoleColor.DarkMagenta;
						break;
					case 1:
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						break;
					case 2:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case 3:
						Console.ForegroundColor = ConsoleColor.White;
						break;
					case 4:
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						break;
					default:
						Console.ForegroundColor = ConsoleColor.Gray;
						break;
				}
			}

			if (backColor.HasValue)
			{
				Console.BackgroundColor = backColor.Value;
			}

			if (direct)
			{
				Console.Write(msg, args);
			}	
			else
			{
				Console.WriteLine($"[{DateTimeOffset.UtcNow:MM/dd/yy-HH:mm:ss}] [{typeStr}]: {msg}", args);
			}

			Console.ResetColor();
		}

		public static void Script(string message, params object[] args)
		{
			Print(3, message, null, null, false, args);
		}

		public static void ScriptDirect(string message, params object[] args)
		{
			Print(3, message, null, null, true, args);
		}

		public static void Debug(string message, params object[] args)
		{
			Print(0, message, null, null, false, args);
		}

		public static void Log()
		{
			Print(-5);
		}

		public static void Log(string message, params object[] args)
		{
			Print(-1, message, null, null, false, args);
		}

		public static void LogDirect(string message, ConsoleColor? foreColor, ConsoleColor? backColor, bool hasTimestamp = false, params object[] args)
		{
			string msg = message;
			if (hasTimestamp)
			{
				msg = $"[{DateTimeOffset.UtcNow:MM/dd/yy-HH:mm:ss}]: " + msg;
			}

			Print(-1, msg, foreColor, backColor, true, args);
		}

		public static void Inform(string message, params object[] args)
		{
			Print(4, message, null, null, false, args);
		}

		public static void Warning(string message, params object[] args)
		{
			Print(1, message, null, null, false, args);
		}

		public static void Error(string message, params object[] args)
		{
			Print(2, message, null, null, false, args);
		}
	}
}
