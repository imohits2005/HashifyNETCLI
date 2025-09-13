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

using System.Text;

namespace HashifyNETCLI
{
	public static class ScriptHelpers
	{
		public static bool Print(string message, params object[] args)
		{
			Logger.Script(message, args);
			return true;
		}

		public static bool Print(object message, params object[] args)
		{
			Logger.Script(message is string ? (string)message : message?.ToString() ?? "N/A", args);
			return true;
		}

		public static bool PrintDirect(string message, params object[] args)
		{
			if (!message.EndsWith("\n"))
				message = message += "\n";

			Logger.ScriptDirect(message, args);
			return true;
		}

		public static bool PrintDirect(object message, params object[] args)
		{
			string m = message is string ? (string)message : message?.ToString() ?? "N/A";

			if (!m.EndsWith("\n"))
				m += "\n";

			Logger.ScriptDirect(m, args);
			return true;
		}

		public static bool Fail(string message, params object[] args)
		{
			throw new FailException(string.Format(message, args));
		}

		public static byte[] StringToArray(string input)
		{
			return Encoding.UTF8.GetBytes(input);
		}

		public static byte[] StringToArrayWithEncoding(string input, Encoding encoding)
		{
			return encoding.GetBytes(input);
		}

		public static byte[] HexToArray(string hex)
		{
			if (hex.Length % 2 != 0)
				throw new ArgumentException("Hex string must have an even length.");

			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}

			return bytes;
		}

		public static byte[] ToByteArray(NLua.LuaTable table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));

			if (table.Values.Count < 1)
				return Array.Empty<byte>();

			byte[] result = new byte[table.Values.Count];
			int ndx = 0;
			foreach (var value in table.Values)
			{
				if (value is not long && value is not int && value is not byte)
					throw new ArgumentException("Lua table contains non-integer values.");

				byte b = Convert.ToByte(value);
				result[ndx] = b;
				ndx++;
			}

			return result;
		}
	}
}
