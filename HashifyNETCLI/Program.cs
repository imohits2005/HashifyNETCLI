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
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HashifyNETCLI
{
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
			Logger.Log("Usage: HashifyCLI [options]");
            Logger.Inform("Options:");
            Logger.Inform("  -cl, --command-line             The path to a file containing the full command line.");
            Logger.Inform("  -h, --help                      Show this help message and exit.");
			Logger.Inform("  -l, --list                      Lists available hash algorithms and exit.");
			Logger.Inform("  -lp, --list-profiles            Lists available config profiles of a specific hash algorithm and exit: blake3 (case-insensitive).");
			Logger.Inform("  -i, --input                     Specify input script: \"'Foo Bar'\"");
			Logger.Inform("  -if, --input-finalizer          Finalizes the input: StringToArray(Input)");
			Logger.Inform("  -a, --algorithms                Specify hashing algorithms: Blake2b, blake3 (case-insensitive).");
            Logger.Inform("  -o, --output                    Specify output script: Print(Algorithm .. ': ' .. Result)");
            Logger.Inform("  -of, --output-finalizer         Finalizes the output: Join(\", \", Coerce(5):AsByteArray())");
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
			Logger.LogDirect(" --algorithms ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" Compute ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" --output-finalizer ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect(">>", ConsoleColor.DarkMagenta, null);
			Logger.LogDirect(" --output ", ConsoleColor.DarkBlue, null);
			Logger.LogDirect("\\\\ cyclist end]", ConsoleColor.DarkRed, null);
			Logger.LogDirect("\n", ConsoleColor.DarkRed, null);
		}

        static string GetHashFunctionName(FunctionVar fvar, bool noVar = false)
        {
            if (!fvar.Function.IsInterface)
                return "N/A";

            if (fvar.Name == null || noVar)
                return fvar.Function.Name.Remove(0, 1);

			return fvar.Function.Name.Remove(0, 1) + ":" + fvar.Name;
        }

        static string GetHashFunctionOriginalName(string name)
        {
            return "I" + name;
        }

        static IReadOnlyList<FunctionVar> GetHashFunction(string query)
        {
            List<FunctionVar> types = new List<FunctionVar>();
            Type[] cryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Cryptographic);
            Type[] noncryptographicHashes = HashFactory.GetHashAlgorithms(HashFunctionType.Noncryptographic);

            if (query == "*")
            {
                IEnumerable<Type> h = cryptographicHashes.Union(noncryptographicHashes);
                foreach (Type t in h)
                {
                    types.Add(new FunctionVar(null, t));
                }

                return types;
            }

            string[] n = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (n == null || n.Length < 1)
            {
                return types;
            }

            foreach (string s in n)
            {
                string actualName;
                string? suffix = null;
                if (s.Contains(":"))
                {
                    string[] p = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length != 2)
                    {
                        // Error, just clear and return an empty result.
                        types.Clear();
                        return types;
                    }

                    actualName = GetHashFunctionOriginalName(p[0].Trim());
                    suffix = p[1].Trim();
				}
                else
                {
                    actualName = GetHashFunctionOriginalName(s);
                }

				foreach (Type t1 in cryptographicHashes)
                {
                    if (t1.Name.Equals(actualName, StringComparison.OrdinalIgnoreCase))
                    {
                        types.Add(new FunctionVar(suffix, t1));
                        break;
                    }
                }

                foreach (Type t2 in noncryptographicHashes)
                {
                    if (t2.Name.Equals(actualName, StringComparison.OrdinalIgnoreCase))
                    {
						types.Add(new FunctionVar(suffix, t2));
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
                Logger.Inform($"  - {GetHashFunctionName(new FunctionVar(null, type))} (Cryptographic)");
			}

            foreach (Type type in noncryptographicHashes)
            {
                Logger.Inform($"  - {GetHashFunctionName(new FunctionVar(null, type))} (Non-Cryptographic)");
			}
		}

        static void PrintProfiles(Type algorithm)
        {
            string algoName = GetHashFunctionName(new FunctionVar(null, algorithm));

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

        static IHashConfigProfile? GetConfigProfile(FunctionVar fvar, string profileName)
        {
			IHashConfigProfile[] profiles = HashFactory.GetConfigProfiles(fvar.Function);
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

        static IHashConfigProfile? GetConfigProfile(FunctionVar fvar, Dictionary<string, string> query)
        {
            string algoName = GetHashFunctionName(fvar);
            string algoName_novar = GetHashFunctionName(fvar, true);

			string? profile = null;
            foreach (KeyValuePair<string, string> pair in query)
            {
                if (algoName.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) || algoName_novar.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
					profile = pair.Value;
                    break;
				}
            }

            if (profile == null)
            {
                return null;
            }

            IHashConfigProfile[] profiles = HashFactory.GetConfigProfiles(fvar.Function);
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

		static IReadOnlyList<JsonConfig>? ParseJSONConfig(string jsonPath)
        {
            try
            {
                List<JsonConfigBuilder> retval = new List<JsonConfigBuilder>();

				JsonConfigBuilder GetOrAdd(Type type)
                {
                    if (retval.Exists(t => t.Type == type))
                    {
                        return retval.Find(t => t.Type == type)!;
                    }
                    else
                    {
                        var builder = new JsonConfigBuilder(type);
                        retval.Add(builder);
                        return builder;
					}
				}

                JsonDocumentOptions options = new JsonDocumentOptions()
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                };

				JsonDocument doc = JsonDocument.Parse(File.ReadAllText(jsonPath), options);
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

                    IReadOnlyList<FunctionVar> hashFunctionTypes = GetHashFunction(n);
                    if (hashFunctionTypes.Count < 1)
                    {
                        Logger.Warning($"Could not find a hash algorithm type with the given JSON name '{n}'.");
						continue;
					}

					FunctionVar hashFunctionType = hashFunctionTypes[0];
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

                        if (!GetOrAdd(hashFunctionType.Function).TryAddProfile(hashFunctionType.Name, configBase))
                        {
                            Logger.Warning($"Config profile '{profile.Name}' for '{n}' already exists, skipping.");
                            continue;
						}
					}
                    else if (bodyFirstProp.Value.Name.Equals("config", StringComparison.OrdinalIgnoreCase))
                    {
						if (bodyFirstProp.Value.Value.ValueKind != JsonValueKind.Object)
						{
							Logger.Warning($"JSON property '{n}' value for 'config' must be an object.");
							continue;
						}

                        if (!HashFactory.TryCreateDefaultConcreteConfig(hashFunctionType.Function, out IHashConfigBase config))
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

						if (!GetOrAdd(hashFunctionType.Function).TryAddProfile(hashFunctionType.Name, config))
                        {
                            Logger.Warning($"Config for '{n}' already exists, skipping.");
                            continue;
						}
					}
				}

                return retval.ConvertAll(t => t.Build());
			}
            catch (JsonException ex)
            {
                Logger.Error($"Failed to parse config JSON with error: {ex}");
                return null;
            }
		}

        public static int Main(string[] args)
        {
            using ScriptEngine scriptEngine = new ScriptEngine();

            string? informationalVersion = Assembly.GetEntryAssembly()?
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;

            if (string.IsNullOrEmpty(informationalVersion))
            {
                informationalVersion = "N/A";
            }

            Logger.Log("HashifyNET Command Line Interface v{0}", informationalVersion);
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
                    IReadOnlyList<FunctionVar> algos = GetHashFunction(lprofiles);
                    if (algos == null || algos.Count < 1)
                    {
                        Logger.Error($"No algorithm found with '{lprofiles}' to retrieve profiles for.");
                        PrintUsage();
                        return 1;
                    }

                    Type f = algos[0].Function;
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
            string inputFinalizer = Stringize(cl.GetValueString("-if", "StringToArray(Input)"));
            string algorithm = Stringize(cl.GetValueString("-a", null!));
            string outputScript = Stringize(cl.GetValueString("-o", "Print(Algorithm .. ': ' .. Result)"));
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
            IReadOnlyList<JsonConfig>? _jsonConfigs = null;
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
                input = scriptEngine.InputScript(inputScript);
            }
            catch (FailException ex)
            {
                Logger.Error(ex.Message);
                return 2;
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
                finalizedInput = scriptEngine.FinalizeInputScript(inputFinalizer, input);
            }
            catch (FailException ex)
            {
                Logger.Error(ex.Message);
                return 2;
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

            IReadOnlyList<FunctionVar> types = GetHashFunction(algorithm);
            if (types == null || types.Count < 1)
            {
                Logger.Error("Invalid or unsupported algorithm specified.");
                PrintAlgorithms();
                return 1;
            }

            foreach (FunctionVar fvar in types)
            {
                try
                {
                    IHashConfigProfile? profile = null;
                    if (configProfileQueries != null)
                    {
                        profile = GetConfigProfile(fvar, configProfileQueries);
                    }

                    IHashFunctionBase function;

                    try
                    {
                        if (profile != null)
                        {
                            IHashConfigBase configProfile = profile.Create();
                            if (configProfile == null)
                            {
                                Logger.Error($"Could not get the config profile instance for algorithm '{GetHashFunctionName(fvar)}'.");
                                PrintAlgorithms();
                                return 1;
                            }

                            function = HashFactory.Create(fvar.Function, configProfile);
                        }
                        else
                        {
                            JsonConfigProfile? jsonConfigProfile = null;
                            if (_jsonConfigs != null && _jsonConfigs.Count > 0)
                            {
                                jsonConfigProfile = _jsonConfigs.Where(t => t.Type == fvar.Function).FirstOrDefault().Profiles?.Where(t => t.AsVar() == fvar).FirstOrDefault();

                                // Try again without the var name, in case there is a globalized config for this function type.
                                if (jsonConfigProfile.HasValue && !jsonConfigProfile.Value.IsValid)
                                {
                                    FunctionVar fvar2 = new FunctionVar(null, fvar.Function);
                                    jsonConfigProfile = _jsonConfigs.Where(t => t.Type == fvar.Function).FirstOrDefault().Profiles?.Where(t => t.AsVar() == fvar2).FirstOrDefault();
                                }

                                if (jsonConfigProfile.HasValue && jsonConfigProfile.Value.Config == null && jsonConfigProfile.Value.Owner == null && jsonConfigProfile.Value.Name == null)
                                {
                                    jsonConfigProfile = null;
                                }
                            }

                            if (jsonConfigProfile.HasValue)
                            {
                                function = HashFactory.Create(fvar.Function, jsonConfigProfile.Value.Config);
                            }
                            else
                            {
                                function = HashFactory.Create(fvar.Function);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Could not create hash function instance: {0}", ex);
                        return 1;
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

                    if (result == null)
                    {
                        Logger.Error($"Hash computation for '{GetHashFunctionName(fvar)}' returned a null result.");
                        return 1;
					}

					object finalizedOutput;
                    try
                    {
                        finalizedOutput = scriptEngine.FinalizeOutputScript(outputFinalizer, result);
                    }
                    catch (FailException ex)
                    {
                        Logger.Error(ex.Message);
                        return 2;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Output finalizer script failed to execute: {0}", ex);
                        return 1;
                    }

                    if (finalizedOutput == null)
                    {
                        Logger.Error("Output finalizer script returned null.");
                        return 1;
					}

                    try
                    {
						scriptEngine.OutputScript(outputScript, finalizedOutput, GetHashFunctionName(fvar));
                    }
                    catch (FailException ex)
                    {
                        Logger.Error(ex.Message);
                        return 2;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Output script failed to execute: {0}", ex);
                        return 1;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("An unexpected error occurred: {0}", ex);
                    return 1;
                }
            }

            return 0;
        }
    }
}
