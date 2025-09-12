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

using HashifyNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HashifyNETCLI
{
	public class FinalizeInputScriptGlobals
	{
		public object? Input;
	}

	public class OutputScriptGlobals
	{
        public string? Algorithm;
		public object? Result;
	}

	internal class Program
    {
		static string Stringize(string str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (c == '\'' && (i > 0 ? str[i - 1] != '\\' : true))
                {
                    sb.Append('"');
                    continue;
                }

                sb.Append(c);
			}

            return sb.ToString();
		}

        static bool IsValidString(string str)
        {
            return !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str);
        }

        static void PrintUsage()
        {
			Logger.Log("Usage: HashifyNETConsole [options]");
            Logger.Inform("Options:");
            Logger.Inform("  -cl, --command-line             The path to a file containing the full command line.");
            Logger.Inform("  -h, --help                      Show this help message and exit.");
			Logger.Inform("  -l, --list                      Lists available hash algorithms and exit.");
			Logger.Inform("  -lp, --list-profiles            Lists available config profiles of a specific hash algorithm and exit: blake3 (case-insensitive).");
			Logger.Inform("  -i, --input                     Specify input script: \"'Foo Bar'\"");
			Logger.Inform("  -if, --input-finalizer          Finalizes the input: StringToArray(Input as string)");
			Logger.Inform("  -a, --algorithms                Specify hashing algorithms: Blake2b, blake3 (case-insensitive).");
            Logger.Inform("  -o, --output                    Specify output script: Print(Result)");
            Logger.Inform("  -of, --output-finalizer         Finalizes the output: Join(\", \", Coerce(5).AsByteArray())");
            Logger.Inform("  -cp, --config-profiles          Specify the config profiles (one for every algorithm specified by --algorithms) to use: CRC=CRC32 Argon2id=OWASP (case-insensitive).");
            Logger.Inform("  -cf, --config-file              Specify the config JSON to use: configs.json.");

			Logger.Log();
            Logger.Log();

			Logger.LogDirect("Execution:\n", ConsoleColor.White, null, true);
			Logger.LogDirect("   ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect("\0-input ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" --input-finalizer ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" [cyclist begin //", ConsoleColor.DarkRed, null);
			Logger.LogDirect(" --algorithm ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" Compute ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" --output-finalizer ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" --output ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect("\\\\ cyclist end]", ConsoleColor.DarkRed, null);
			Logger.LogDirect("\n", ConsoleColor.DarkRed, null);
		}

        static string GetHashFunctionName(Type type)
        {
            if (!type.IsInterface)
                return "N/A";

			return type.Name.Remove(0, 1);
        }

        static string GetHashFunctionOriginalName(string name)
        {
            return "I" + name;
        }

        static IReadOnlyList<Type> GetHashFunction(string name)
        {
            List<Type> types = new List<Type>();
            string interfaceName = GetHashFunctionOriginalName(name);
            Type[] cryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Cryptographic);
            Type[] noncryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Noncryptographic);

            if (name == "*")
            {
                types.AddRange(cryptographicHashes);
                types.AddRange(noncryptographicHashes);
                return types;
			}

            string[] n = name.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (n == null || n.Length < 1)
            {
                return types;
            }

            foreach (string s in n)
            {
                foreach (Type t1 in cryptographicHashes)
                {
                    if (t1.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase))
                    {
                        types.Add(t1);
                        break;
                    }
                }

                foreach (Type t2 in noncryptographicHashes)
                {
                    if (t2.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase))
                    {
                        types.Add(t2);
                        break;
                    }
                }
            }

            return types;
        }

        static void PrintAlgorithms()
        {
            Logger.Inform("Available Hash Algorithms:");
            Type[] cryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Cryptographic);
            Type[] noncryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Noncryptographic);

            foreach (Type type in cryptographicHashes)
            {
                Logger.Inform($"  - {GetHashFunctionName(type)} (Cryptographic)");
			}

            foreach (Type type in noncryptographicHashes)
            {
                Logger.Inform($"  - {GetHashFunctionName(type)} (Non-Cryptographic)");
			}
		}

        static void PrintProfiles(Type algorithm)
        {
            string algoName = GetHashFunctionName(algorithm);

			IHashConfigProfile[] profiles = HashFactory.GetConfigProfiles(algorithm);
            if (profiles.Length < 1)
            {
                Logger.Warning($"No config profile available for type '{algoName}'.");
                return;
            }

            Logger.Inform($"Available Config Profiles of '{algoName}':");
			foreach (IHashConfigProfile profile in profiles)
            {
                if (profile.Description != null)
                    Logger.Inform($"  - {profile.Name}: {profile.Description}");
                else
					Logger.Inform($"  - {profile.Name}");
			}
        }

        static Dictionary<string, string>? ParseConfigProfileQuery(string query)
        {
            string[] p = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 1)
                return null;

            Dictionary<string, string> retval = new Dictionary<string, string>();

            foreach (string s in p)
            {
                string[] v = s.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (v.Length != 2)
                {
                    return null;
                }

                string v1 = v[0].Trim();
                string v2 = v[1].Trim();

                if (retval.ContainsKey(v1))
                {
                    return null;
                }

                retval.Add(v1, v2);
            }

            return retval;
        }

        static IHashConfigProfile? GetConfigProfile(Type algorithm, string profileName)
        {
			IHashConfigProfile[] profiles = HashFactory.GetConfigProfiles(algorithm);
			if (profiles.Length < 1)
			{
				return null;
			}

			foreach (IHashConfigProfile prof in profiles)
			{
				if (profileName.Equals(prof.Name, StringComparison.OrdinalIgnoreCase))
				{
					return prof;
				}
			}

            return null;
		}

        static IHashConfigProfile? GetConfigProfile(Type algorithm, Dictionary<string, string> query)
        {
            string algoName = GetHashFunctionName(algorithm);

            string? profile = null;
            foreach (KeyValuePair<string, string> pair in query)
            {
                if (algoName.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
					profile = pair.Value;
                    break;
				}
            }

            if (profile == null)
            {
                return null;
            }

            IHashConfigProfile[] profiles = HashFactory.GetConfigProfiles(algorithm);
            if (profiles.Length < 1)
            {
                return null;
            }

            foreach (IHashConfigProfile prof in profiles)
            {
                if (profile.Equals(prof.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return prof;
                }
            }

            return null;
		}

		static object? GetJSONValue(JsonElement element, Type instPropType)
        {
            object? retval = null;
			JsonValueKind vk = element.ValueKind;
			if ((vk == JsonValueKind.True || vk == JsonValueKind.False) && instPropType == typeof(bool))
			{
                retval = element.GetBoolean();
			}
			else if (vk == JsonValueKind.String)
			{
                if (instPropType == typeof(string))
                {
                    retval = element.GetString();
                }
                else if (instPropType == typeof(BigInteger))
                {
                    if (BigInteger.TryParse(element.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out BigInteger result))
                    {
                        retval = result;
                    }
                }
			}
			else if (vk == JsonValueKind.Number)
			{
                if (instPropType == typeof(byte))
                {
                    if (element.TryGetByte(out byte result))
                    {
                        retval = result;
                    }
                }
				else if (instPropType == typeof(sbyte))
				{
					if (element.TryGetSByte(out sbyte result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(ushort))
				{
					if (element.TryGetUInt16(out ushort result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(short))
				{
					if (element.TryGetInt16(out short result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(uint))
				{
					if (element.TryGetUInt32(out uint result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(int))
				{
					if (element.TryGetInt32(out int result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(ulong))
				{
					if (element.TryGetUInt64(out ulong result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(long))
				{
					if (element.TryGetInt64(out long result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(float))
				{
					if (element.TryGetSingle(out float result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(double))
				{
					if (element.TryGetDouble(out double result))
					{
						retval = result;
					}
				}
				else if (instPropType == typeof(decimal))
				{
					if (element.TryGetDecimal(out decimal result))
					{
						retval = result;
					}
				}
			}
            else if (vk == JsonValueKind.Array && instPropType.IsArray && instPropType.GetArrayRank() == 1)
            {
                Type underlyingType = instPropType.GetElementType()!;
                var enumerator = element.EnumerateArray();
                List<object> values = new List<object>();
                foreach (var arrayElement in enumerator)
                {
                    object? value = GetJSONValue(arrayElement, underlyingType);
                    if (value == null)
                    {
                        Logger.Error($"Failed to build a C# array from JSON array. GetJSONValue returned a null object for type '{underlyingType.FullName}'.");
                        // Clear up the previous values if any, we do not want partially correct arrays.
                        values.Clear();
                        break;
                    }

                    values.Add(value);
				}

                if (values.Count > 1)
                {
                    Array array;

                    // Array.CreateInstance is AOT unfriendly, so we initialize each type's array manually.
                    // Note that we currently do not support AOT due to runtime C# scripting, but in the future this is planned.
                    if (underlyingType == typeof(sbyte))
                    {
                        array = new sbyte[values.Count];
                    }
                    else if (underlyingType == typeof(byte))
                    {
                        array = new byte[values.Count];
                    }
					else if (underlyingType == typeof(ushort))
					{
						array = new ushort[values.Count];
					}
					else if (underlyingType == typeof(short))
					{
						array = new short[values.Count];
					}
					else if (underlyingType == typeof(uint))
					{
						array = new uint[values.Count];
					}
					else if (underlyingType == typeof(int))
					{
						array = new int[values.Count];
					}
					else if (underlyingType == typeof(ulong))
					{
						array = new ulong[values.Count];
					}
					else if (underlyingType == typeof(long))
					{
						array = new long[values.Count];
					}
					else if (underlyingType == typeof(float))
					{
						array = new float[values.Count];
					}
					else if (underlyingType == typeof(double))
					{
						array = new double[values.Count];
					}
					else if (underlyingType == typeof(decimal))
					{
						array = new decimal[values.Count];
					}
					else if (underlyingType == typeof(string))
					{
						array = new string[values.Count];
					}
					else if (underlyingType == typeof(BigInteger))
					{
						array = new BigInteger[values.Count];
					}
                    else
                    {
                        array = null!;
                    }

                    if (array != null)
                    {
                        try
                        {
                            Array.ConstrainedCopy(values.ToArray(), 0, array, 0, values.Count);

                            retval = array;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Array.ConstrainedCopy failed with: {ex}");
                        }
                    }
                    else
                    {
                        Logger.Error($"Unsupported underyling array type '{underlyingType.FullName}'.");
                    }
                }
            }

            return retval;
		}

		static IReadOnlyList<(Type AlgorithmType, IHashConfigBase Config)>? ParseJSONConfig(string jsonPath)
        {
            try
            {
                List<(Type AlgorithmType, IHashConfigBase Config)> retval = new List<(Type AlgorithmType, IHashConfigBase Config)>();

				JsonDocument doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
                JsonElement root = doc.RootElement;
                var objectEnumerator = root.EnumerateObject();
                foreach (var property in objectEnumerator)
                {
					if (property.Value.ValueKind != JsonValueKind.Object)
					{
						Logger.Warning($"Body of config '{property.Name}' must be an object.");
                        continue;
					}

					string n = property.Name;
                    if (!IsValidString(n))
                    {
                        Logger.Warning($"Expected a valid string in the JSON key. Got \"\".");
                        continue;
					}

                    IReadOnlyList<Type> hashFunctionTypes = GetHashFunction(n);
                    if (hashFunctionTypes.Count < 1)
                    {
                        Logger.Warning($"Could not find a hash algorithm type with the given JSON name '{n}'.");
						continue;
					}

                    Type hashFunctionType = hashFunctionTypes[0];
                    var body = property.Value.EnumerateObject();
                    JsonProperty? bodyFirstProp = null;
                    foreach (var bodyProp in body)
                    {
                        bodyFirstProp = bodyProp;
                        break;
                    }

                    if (!bodyFirstProp.HasValue)
                    {
                        continue;
                    }

                    if (bodyFirstProp.Value.Name.Equals("profile", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bodyFirstProp.Value.Value.ValueKind != JsonValueKind.String)
                        {
                            Logger.Warning($"JSON property '{n}' value for 'profile' must be a string.");
							continue;
						}

                        string? v = bodyFirstProp.Value.Value.GetString();
                        if (!IsValidString(v!))
                        {
							Logger.Warning($"JSON property '{n}' value for 'profile' must have a valid string.");
							continue;
						}

						IHashConfigProfile? profile = GetConfigProfile(hashFunctionType, v!);
                        if (profile == null)
                        {
                            Logger.Warning($"Failed to get a valid config profile for '{n}'.");
							continue;
						}

                        IHashConfigBase configBase = profile.Create();
                        if (configBase == null)
                        {
                            Logger.Warning($"Failed to create a valid config for '{n}' from config profile '{profile.Name}'.");
							continue;
						}

                        retval.Add((hashFunctionType, configBase));
                    }
                    else if (bodyFirstProp.Value.Name.Equals("config", StringComparison.OrdinalIgnoreCase))
                    {
						if (bodyFirstProp.Value.Value.ValueKind != JsonValueKind.Object)
						{
							Logger.Warning($"JSON property '{n}' value for 'config' must be an object.");
							continue;
						}

                        if (!HashFactory.TryCreateDefaultConcreteConfig(hashFunctionType, out IHashConfigBase config))
                        {
                            Logger.Warning($"Unable to create an instance of default concrete hash config for '{n}'.");
							continue;
						}

						Type type = config.GetType();
						PropertyInfo[] pis = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

						if (pis.Length < 1)
                        {
							Logger.Warning($"Config does not have any modifiable property, consider calling the algorithm directly.");
                            continue;
						}

                        IEnumerable<PropertyInfo> configProperties = pis.Where(t => t.CanWrite);
                        var enumerator = bodyFirstProp.Value.Value.EnumerateObject();
                        bool modified = false;
                        foreach (var prop in enumerator)
                        {
                            if (!modified)
                            {
                                modified = true;
							}

                            bool fail = false;
                            foreach (var propinf in configProperties)
                            {
                                if (prop.Name.Equals(propinf.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    object? value = GetJSONValue(prop.Value, propinf.PropertyType);
                                    if (value == null)
                                    {
                                        Logger.Error("Constructing the config from JSON failed due to an unexpected null value from GetJSONValue. Please refer to previous log messages for further details.");
                                        fail = true;
                                        break;
                                    }

                                    propinf.SetValue(config, value);
                                }
                            }

                            if (fail)
                            {
                                modified = false;
                                break;
                            }
                        }

                        if (!modified)
                        {
                            continue;
                        }

                        retval.Add((hashFunctionType, config));
					}
				}

                return retval;
			}
            catch (JsonException ex)
            {
                Logger.Error($"Failed to parse config JSON with error: {ex}");
                return null;
            }
		}

		static object InputScript(string script)
		{
			var options = ScriptOptions.Default
				.WithImports("System", "System.Linq", "System.IO", "HashifyNet")
				.WithReferences(typeof(IHashValue).Assembly)
				;
            
            string[] statics =
			[
				"using static System.IO.Directory;",
                "using static System.IO.File;",
                "using static System.String;",
            ];

            foreach (string @static in statics)
            {
                if (!script.Contains(@static))
                {
                    script = @static + Environment.NewLine + script;
                }
            }

			return CSharpScript.EvaluateAsync(script, options).GetAwaiter().GetResult();
		}

		static object FinalizeInputScript(string finalizer, object input)
		{
			var options = ScriptOptions.Default
				.WithImports("System", "System.Linq", "HashifyNet")
				.WithReferences(typeof(IHashValue).Assembly, typeof(ScriptHelpers).Assembly)
				;

			const string staticString = "using static System.String;";
            const string helperString = "using static HashifyNETCLI.ScriptHelpers;";
			if (!finalizer.Contains(staticString))
			{
				finalizer = staticString + Environment.NewLine + finalizer;
			}

            if (!finalizer.Contains(helperString))
            {
                finalizer = helperString + Environment.NewLine + finalizer;
			}

			return CSharpScript.EvaluateAsync(finalizer, options, new FinalizeInputScriptGlobals() { Input = input}).GetAwaiter().GetResult();
		}

        static void OutputScript(string script, object result, string algorithm)
        {
            var options = ScriptOptions.Default
                .WithImports("System", "System.Linq", "System.IO", "HashifyNet")
			    .WithReferences(typeof(IHashValue).Assembly, typeof(ScriptHelpers).Assembly)
				;

            string[] statics =
            [
                "using static System.IO.Directory;",
                "using static System.IO.File;",
                "using static System.String;",
                "using static HashifyNETConsole.ScriptHelpers;",
            ];

            foreach (string @static in statics)
            {
                if (!script.Contains(@static))
                {
                    script = @static + Environment.NewLine + script;
                }
            }

            CSharpScript.EvaluateAsync(script, options, new OutputScriptGlobals() { Result = result, Algorithm = algorithm }).GetAwaiter().GetResult();
        }

		static object FinalizeOutputScript(string finalizer, IHashValue value)
        {
            var options = ScriptOptions.Default
                .WithImports("System", "System.Linq", "HashifyNet")
				.WithReferences(typeof(IHashValue).Assembly)
				;

            const string staticString = "using static System.String;";
            if (!finalizer.Contains(staticString))
            {
                finalizer = staticString + Environment.NewLine + finalizer;
            }

            return CSharpScript.EvaluateAsync(finalizer, options, value, null).GetAwaiter().GetResult();
		}

        public static int Main(string[] args)
        {
                string? informationalVersion = Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;

            if (string.IsNullOrEmpty(informationalVersion))
            {
                informationalVersion = "N/A";
            }

			Logger.Log("HashifyNET Console Application v{0}", informationalVersion);
			Logger.Log("Copyright (c) 2025, Deskasoft International. All rights reserved.");
            Logger.Log();

			CommandLine cl = new CommandLine(args);
            if (cl.Count < 1)
            {
                PrintUsage();
                return 0;
            }

            CommandLine proc = cl;
            label_restart:

			proc.MapCommand("--command-line", "-cl", true);
			proc.MapCommand("--help", "-h", false);
			proc.MapCommand("--list", "-l", false);
			proc.MapCommand("--list-profiles", "-lp", false);
			proc.MapCommand("--input", "-i", true);
			proc.MapCommand("--input-finalizer", "-if", true);
			proc.MapCommand("--algorithms", "-a", true);
			proc.MapCommand("--output", "-o", true);
			proc.MapCommand("--output-finalizer", "-of", true);
			proc.MapCommand("--config-profile", "-cp", true);
			proc.MapCommand("--config-file", "-cf", true);

			if (proc.HasFlag("-cl"))
            {
                string clPath = proc.GetValueString("-cl", null!);
                if (!File.Exists(clPath))
                {
                    Logger.Error("Command line file does not exist.");
                    return 1;
                }
                else
                {
                    CommandLine? orig = null;
                    if (proc != cl)
                        orig = proc;

					proc = new CommandLine(null!);
					proc.AppendCommandLine(clPath);

                    if (orig != null)
                        proc.AppendCommandLine(orig);

					goto label_restart;
				}
            }

            if (cl != proc)
                cl.AppendCommandLine(proc);

            if (cl.HasFlag("-h"))
            {
                PrintUsage();
                return 0;
            }

            if (cl.HasFlag("-l"))
            {
                PrintAlgorithms();
                return 0;
            }

            if (cl.HasFlag("-cp") && cl.HasFlag("-cf"))
            {
                Logger.Warning("Detected ambiguous command line parameters of '--config-profiles' and '--config-file'. '--config-profiles' will be chosen over --config-file by default but consider using only one of them.");
            }

            if (cl.HasFlag("-lp"))
            {
                string lprofiles = cl.GetValueString("-lp", null!);
                if (lprofiles != null)
                {
                    IReadOnlyList<Type> algos = GetHashFunction(lprofiles);
                    if (algos == null || algos.Count < 1)
                    {
                        Logger.Error($"No algorithm found with '{lprofiles}' to retrieve profiles for.");
                        PrintUsage();
                        return 1;
                    }

                    Type f = algos[0];
                    PrintProfiles(f);
                    return 0;
                }
                else
                {
                    Logger.Warning("--list-profiles requires a hash name to list the config profiles of.");
                    PrintUsage();
                    return 1;
                }
            }

            string inputScript = Stringize(cl.GetValueString("-i", null!));
            string inputFinalizer = Stringize(cl.GetValueString("-if", "StringToArray(Input as string)"));
            string algorithm = Stringize(cl.GetValueString("-a", null!));
            string outputScript = Stringize(cl.GetValueString("-o", "Print(Result)"));
            string outputFinalizer = Stringize(cl.GetValueString("-of", "Join(\", \", AsByteArray())"));

            string configProfileQuery = cl.GetValueString("-cp", null!);
            Dictionary<string, string>? configProfileQueries = null;
            if (configProfileQuery != null)
            {
                if (!IsValidString(configProfileQuery))
                {
					Logger.Error("If not null, config profile query must contain a valid non-empty and non-whitespace string.");
					PrintUsage();
					return 1;
				}

				configProfileQueries = ParseConfigProfileQuery(configProfileQuery);

                if (configProfileQueries == null)
                {
					Logger.Error("Could not parse config profile query.");
					PrintUsage();
					return 1;
				}
            }

            string configJson = cl.GetValueString("-cf", null!);
            IReadOnlyList<(Type AlgorithmType, IHashConfigBase Config)>? _jsonConfigs = null;
            if (configJson != null)
            {
                if (!IsValidString(configJson) || !File.Exists(configJson))
                {
					Logger.Error("If not null, config JSON file must have a valid non-empty, non-whitespace file path pointing to an existing file.");
					PrintUsage();
					return 1;
				}

				_jsonConfigs = ParseJSONConfig(configJson);
                if (_jsonConfigs == null)
                {
					// Assuming ParseJSONConfig already logged the error.
					PrintUsage();
					return 1;
                }
			}

            if (!IsValidString(inputScript))
            {
                Logger.Error("Must have a valid input script.");
                PrintUsage();
                return 1;
            }

            if (!IsValidString(inputFinalizer))
            {
                Logger.Error("Must have a valid input finalizer.");
                PrintUsage();
                return 1;
            }

            if (!IsValidString(algorithm))
            {
                Logger.Error("Must have a valid algorithm.");
                PrintUsage();
                return 1;
            }

            if (!IsValidString(outputScript))
            {
                Logger.Error("Must have a valid output script.");
                PrintUsage();
                return 1;
            }

            if (!IsValidString(outputFinalizer))
            {
                Logger.Error("Must have a valid output finalizer.");
                PrintUsage();
                return 1;
            }

			object input;
			try
			{
				input = InputScript(inputScript);
			}
			catch (Exception ex)
			{
				Logger.Error("Input script failed to execute: {0}", ex);
				return 1;
			}

			if (input == null)
			{
				Logger.Error("Input script returned null.");
				return 1;
			}

			object finalizedInput;
			try
			{
				finalizedInput = FinalizeInputScript(inputFinalizer, input);
			}
			catch (Exception ex)
			{
				Logger.Error("Input finalizer script failed to execute: {0}", ex);
				return 1;
			}

			if (!(finalizedInput is byte[] inputArray))
			{
				Logger.Error("Input finalizer script must return a byte array.");
				return 1;
			}

			IReadOnlyList<Type> types = GetHashFunction(algorithm);
            if (types == null || types.Count < 1)
            {
                Logger.Error("Invalid or unsupported algorithm specified.");
                PrintAlgorithms();
                return 1;
            }

            foreach (Type type in types)
            {
                if (type == null)
                {
                    Logger.Error("Invalid or unsupported algorithm specified.");
                    PrintAlgorithms();
                    return 1;
                }

                IHashConfigProfile? profile = null;
                if (configProfileQueries != null)
                {
					profile = GetConfigProfile(type, configProfileQueries);
                }

                IHashFunctionBase function;
                if (profile != null)
                {
                    IHashConfigBase configProfile = profile.Create();
                    if (configProfile == null)
                    {
                        Logger.Error($"Could not get the config profile instance for algorithm '{GetHashFunctionName(type)}'.");
						PrintAlgorithms();
						return 1;
					}

                    function = HashFactory.Create(type, configProfile);
                }
                else
                {
                    (Type AlgorithmType, IHashConfigBase Config)? jsonConfig = null;
					if (_jsonConfigs != null && _jsonConfigs.Count > 0)
                    {
						jsonConfig = _jsonConfigs.Where(t => t.AlgorithmType == type).FirstOrDefault();
                    }

                    if (jsonConfig.HasValue)
                    {
						function = HashFactory.Create(type, jsonConfig.Value.Config);
					}
                    else
                    {
                        function = HashFactory.Create(type);
                    }
				}

                if (function == null)
                {
                    Logger.Error("Failed to create hash function instance.");
                    return 1;
                }

                IHashValue result;
                try
                {
                    result = function.ComputeHash(inputArray);
                }
                catch (Exception ex)
                {
                    Logger.Error("Hash computation failed: {0}", ex);
                    return 1;
                }

                object finalizedOutput = FinalizeOutputScript(outputFinalizer, result);
                try
                {
                    OutputScript(outputScript, finalizedOutput, GetHashFunctionName(type));
                }
                catch (Exception ex)
                {
                    Logger.Error("Output script failed to execute: {0}", ex);
                    return 1;
                }
            }

            return 0;
        }
    }
}
