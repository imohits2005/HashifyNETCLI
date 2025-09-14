// *
// *****************************************************************************
// *
// * Copyright (c) 2025 Deskasoft International
// *
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the ""Software""), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// *
// * The above copyright notice and this permission notice shall be included in all
// * copies or substantial portions of the Software.
// *
// * THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
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

using HashifyNet;
using NLua;
using NLua.Exceptions;
using System.Reflection;
using System.Text;

namespace HashifyNETCLI
{
	public sealed class ScriptEngine : IDisposable
	{
		private bool disposedValue;

		private Lua _luaState;
		public ScriptEngine()
		{
			_luaState = new Lua();
			_luaState.State.Encoding = Encoding.UTF8;

			_luaState.LoadCLRPackage();
			_luaState.DoString("import ('HashifyNet', 'HashifyNet')");
			_luaState.DoString("import ('HashifyNETCLI', 'HashifyNETCLI')");
			_luaState.DoString("import ('System', 'System')");
			_luaState.DoString("import ('System.Core', 'System.Linq')");

			PushLuaHelpers(_luaState, typeof(ScriptHelpers));
			PushLuaHelpers(_luaState, typeof(System.IO.Directory));
			PushLuaHelpers(_luaState, typeof(System.IO.File));
			PushLuaHelpers(_luaState, typeof(System.String));
		}

		private static void PushLuaHelpers(Lua state, Type staticType)
		{
			MethodInfo[] mis = staticType.GetMethods(BindingFlags.Public | BindingFlags.Static);
			if (mis.Length < 1)
			{
				Logger.Warning($"No public static method available in '{staticType.FullName}' to bind into Lua.");
				return;
			}

			foreach (MethodInfo mi in mis)
			{
				state.RegisterFunction(mi.Name, null, mi);
			}
		}

		private static void CleanAndRegister(Lua state, string name, object target, MethodInfo method)
		{
			if (state[name] != null)
			{
				if (state[name] is IDisposable disposable)
				{
					disposable.Dispose();
				}

				state[name] = null;
			}

			state.RegisterFunction(name, target, method);
		}

		private static void PushLuaVariables(Lua state, object instance)
		{
			Type type = instance.GetType();

			PropertyInfo[] pis = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo pi in pis)
			{
				if (!pi.CanRead) continue;
				state[pi.Name] = pi.GetValue(instance);
			}

			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
							  .Where(m => !m.IsSpecialName);

			var methodGroups = methods.GroupBy(m => m.Name);

			foreach (var group in methodGroups)
			{
				if (group.Count() == 1)
				{
					var singleMethod = group.Single();
					CleanAndRegister(state, group.Key, instance, singleMethod);
				}
				else
				{
					MethodInfo primaryMethod;

					var noParamMethod = group.FirstOrDefault(m => m.GetParameters().Length == 0);

					if (noParamMethod != null)
					{
						primaryMethod = noParamMethod;
					}
					else
					{
						primaryMethod = group.OrderBy(m => m.GetParameters().Length)
											 .ThenBy(m => m.ToString())
											 .First();
					}

					CleanAndRegister(state, group.Key, instance, primaryMethod);

					foreach (var overloadedMethod in group.Where(m => m != primaryMethod))
					{
						string suffix = string.Join("_", overloadedMethod.GetParameters().Select(p => p.ParameterType.Name));
						CleanAndRegister(state, $"{group.Key}_{suffix}", instance, overloadedMethod);
					}
				}
			}
		}

		private static string PrepareScript(string script)
		{
			return "return " + script;
		}

		private object[] ExecuteScript(string script)
		{
			try
			{
				return _luaState.DoString(script);
			}
			catch (LuaScriptException ex)
			{
				if (ex.IsNetException && ex.InnerException != null)
				{
					throw ex.InnerException;
				}

				throw;
			}
		}

		public object InputScript(string script)
		{
			script = PrepareScript(script);

			object[] res = ExecuteScript(script);
			if (res == null || res.Length < 1)
			{
				Logger.Error("Input script did not return any value.");
				return null!;
			}
			return res[0]!;
		}

		public object FinalizeInputScript(string finalizer, object input)
		{
			finalizer = PrepareScript(finalizer);

			_luaState["Input"] = input;
			object[] res = ExecuteScript(finalizer);

			if (res == null || res.Length < 1)
			{
				Logger.Error("Input finalizer script did not return any value.");
				return null!;
			}

			return res[0]!;
		}

		public void OutputScript(string script, object result, string algorithm)
		{
			_luaState["Result"] = result;
			_luaState["Algorithm"] = algorithm;
			ExecuteScript(script);
		}

		public object FinalizeOutputScript(string finalizer, IHashValue value)
		{
			finalizer = PrepareScript(finalizer);

			PushLuaVariables(_luaState, value);
			object[] res = ExecuteScript(finalizer);

			if (res == null || res.Length < 1)
			{
				Logger.Error("Output finalizer script did not return any value.");
				return null!;
			}

			return res[0]!;
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				_luaState.Dispose();
				_luaState = null!;

				disposedValue = true;
			}
		}

		~ScriptEngine()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
