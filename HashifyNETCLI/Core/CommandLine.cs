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
	public class CommandLine
	{
		public int Count => _args.Count;

		private List<string> _args;
		public CommandLine(string[] args)
		{
			if (args != null)
				_args = [.. args];
			else
				_args = new List<string>();
		}

		public void AppendCommandLine(CommandLine cl)
		{
			_ = cl ?? throw new ArgumentNullException(nameof(cl));

			if (cl._args == null || cl._args.Count < 1)
				return;

			_args.AddRange(cl._args);
		}

		public void AppendCommandLine(string file)
		{
			string commandLine = File.ReadAllText(file).Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

			var currentArg = new StringBuilder();
			bool inQuote = false;

			foreach (char c in commandLine)
			{
				if (c == '\"')
				{
					inQuote = !inQuote;
					continue;
				}

				if (c == ' ' && !inQuote)
				{
					if (currentArg.Length > 0)
					{
						_args.Add(currentArg.ToString());
						currentArg.Clear();
					}
				}
				else
				{
					currentArg.Append(c);
				}
			}

			if (currentArg.Length > 0)
			{
				_args.Add(currentArg.ToString());
			}
		}

		public bool MapCommand(string command, string map, bool hasValue)
		{
			if (!HasFlag(command))
				return false;

			_args.Add(map);

			if (hasValue)
				_args.Add(GetValueString(command));

			return true;
		}

		public bool HasFlag(string flag)
		{
			return _args.Contains(flag);
		}

		public string GetValueString(string key, string defaultValue = "")
		{
			for (int i = 0; i < _args.Count; i++)
			{
				if (_args[i] == key && i + 1 < _args.Count)
				{
					return _args[i + 1];
				}
			}
			return defaultValue;
		}

		public string GetKeyValueString(string keyValue, string defaultValue = "")
		{
			foreach (var arg in _args)
			{
				if (arg.StartsWith(keyValue + "="))
				{
					return arg.Substring(keyValue.Length + 1);
				}
			}
			return defaultValue;
		}

		public IReadOnlyList<string> GetAllValuesString(string key)
		{
			List<string> values = new List<string>();
			for (int i = 0; i < _args.Count; i++)
			{
				if (_args[i] == key && i + 1 < _args.Count)
				{
					values.Add(_args[i + 1]);
				}
			}
			return values;
		}

		public IReadOnlyList<string> GetAllKeyValuesString(string keyValue)
		{
			List<string> values = new List<string>();
			foreach (var arg in _args)
			{
				if (arg.StartsWith(keyValue + "="))
				{
					values.Add(arg.Substring(keyValue.Length + 1));
				}
			}
			return values;
		}

		public int GetValueInt(string key, int defaultValue = 0)
		{
			string valueStr = GetValueString(key);
			if (int.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int value))
			{
				return value;
			}
			return defaultValue;
		}

		public int GetKeyValueInt(string keyValue, int defaultValue = 0)
		{
			string valueStr = GetKeyValueString(keyValue);
			if (int.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int value))
			{
				return value;
			}
			return defaultValue;
		}

		public IReadOnlyList<int> GetAllValuesInt(string key)
		{
			List<int> values = new List<int>();
			foreach (var valueStr in GetAllValuesString(key))
			{
				if (int.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public IReadOnlyList<int> GetAllKeyValuesInt(string keyValue)
		{
			List<int> values = new List<int>();
			foreach (var valueStr in GetAllKeyValuesString(keyValue))
			{
				if (int.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public double GetValueDouble(string key, double defaultValue = 0.0)
		{
			string valueStr = GetValueString(key);
			if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
			{
				return value;
			}
			return defaultValue;
		}

		public double GetKeyValueDouble(string keyValue, double defaultValue = 0.0)
		{
			string valueStr = GetKeyValueString(keyValue);
			if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
			{
				return value;
			}
			return defaultValue;
		}

		public IReadOnlyList<double> GetAllValuesDouble(string key)
		{
			List<double> values = new List<double>();
			foreach (var valueStr in GetAllValuesString(key))
			{
				if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public IReadOnlyList<double> GetAllKeyValuesDouble(string keyValue)
		{
			List<double> values = new List<double>();
			foreach (var valueStr in GetAllKeyValuesString(keyValue))
			{
				if (double.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public bool GetValueBool(string key, bool defaultValue = false)
		{
			string valueStr = GetValueString(key);
			if (bool.TryParse(valueStr, out bool value))
			{
				return value;
			}
			return defaultValue;
		}

		public bool GetKeyValueBool(string keyValue, bool defaultValue = false)
		{
			string valueStr = GetKeyValueString(keyValue);
			if (bool.TryParse(valueStr, out bool value))
			{
				return value;
			}
			return defaultValue;
		}

		public IReadOnlyList<bool> GetAllValuesBool(string key)
		{
			List<bool> values = new List<bool>();
			foreach (var valueStr in GetAllValuesString(key))
			{
				if (bool.TryParse(valueStr, out bool value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public IReadOnlyList<bool> GetAllKeyValuesBool(string keyValue)
		{
			List<bool> values = new List<bool>();
			foreach (var valueStr in GetAllKeyValuesString(keyValue))
			{
				if (bool.TryParse(valueStr, out bool value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public float GetValueFloat(string key, float defaultValue = 0.0f)
		{
			string valueStr = GetValueString(key);
			if (float.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
			{
				return value;
			}
			return defaultValue;
		}

		public float GetKeyValueFloat(string keyValue, float defaultValue = 0.0f)
		{
			string valueStr = GetKeyValueString(keyValue);
			if (float.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
			{
				return value;
			}
			return defaultValue;
		}

		public IReadOnlyList<float> GetAllValuesFloat(string key)
		{
			List<float> values = new List<float>();
			foreach (var valueStr in GetAllValuesString(key))
			{
				if (float.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public IReadOnlyList<float> GetAllKeyValuesFloat(string keyValue)
		{
			List<float> values = new List<float>();
			foreach (var valueStr in GetAllKeyValuesString(keyValue))
			{
				if (float.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		public override string ToString()
		{
			return string.Join(" ", _args);
		}
	}
}
