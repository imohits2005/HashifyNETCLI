using System.Text;

namespace HashifyNETCLI
{
	public static class ScriptHelpers
	{
		public static void Print(string message, params object[] args)
		{
			Logger.Script(message, args);
		}

		public static void Print(object message, params object[] args)
		{
			Logger.Script(message is string ? (string)message : message?.ToString() ?? "N/A", args);
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
	}
}
