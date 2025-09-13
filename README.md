Welcome to HashifyNET Command Line Interface
--------------------------------------------

HashifyNET CLI is a robust and versatile platform-independent command-line utility. It provides a high-performance interface for computing a comprehensive array of cryptographic and non-cryptographic hash algorithms, all of which are made available through the underlying [HashifyNET](https://github.com/Deskasoft/HashifyNET) library.

The CLI is fundamentally powered by the [HashifyNET](https://github.com/Deskasoft/HashifyNET) project, inheriting its extensive suite of validated hash algorithm implementations. This foundational dependency ensures that all computations are both accurate and efficient. The tool serves a dual purpose: it acts as a practical demonstration of the library's power and provides the community with a standardized, reliable utility for hash computation tasks.

A key architectural feature of HashifyNET CLI is its use of [Lua](https://github.com/NLua/NLua) for all input/output (I/O) operations. This design choice empowers users with unparalleled flexibility, far exceeding that of typical command-line tools. It grants a level of control comparable to native C# programming without the overhead of compiling a full application. Users can implement complex logic to preprocess input data from various sources, interact with external systems, or dynamically format the output into structured formats such as JSON, CSV, or XML, tailored to their specific needs.

To accommodate a wide range of operational scenarios, from simple ad-hoc tasks to complex, automated workflows, the utility provides a hierarchical configuration system:

* **Direct Command-Line Parameters:** For straightforward, immediate operations and easy integration into shell scripts.
* **Parameter Files:** Ideal for managing exceptionally long or complex command-line arguments. This method enhances readability, ensures reproducibility, and circumvents potential command-length and syntax limitations imposed by the operating system.
* **JSON Configuration Files:** For advanced customization, users can define persistent settings, configure algorithm-specific parameters (e.g., keys, salts), and create multiple profiles for different use cases, ensuring a consistent and efficient workflow.

The combination of extensive algorithm support and advanced scripting makes HashifyNET CLI suitable for a multitude of applications, including file integrity verification, data deduplication, digital forensics, password security analysis, and integration into larger data processing pipelines. In summary, it stands as a powerful and extensible framework for anyone requiring precise and adaptable hashing capabilities.

Usage
-----
### Quick computation using CRC32
```
HashifyCLI -i "'Hello World'" -a "CRC" -cp "CRC=CRC32"
```

### Quick computation using CRC32 and CRC64
```
HashifyCLI -i "'Hello World'" -a "CRC:1 CRC:2" -cp "CRC:1=CRC32 CRC:2=CRC64"
```

### Quick computation of file HashifyCLI.exe (or HashifyCLI for Unix) using MD5, CRC32 and CRC64
```
HashifyCLI -i "ReadAllBytes('HashifyCLI.exe')" -if "Input" -a "MD5 CRC:1 CRC:2" -cp "CRC:1=CRC32 CRC:2=CRC64"
```

Syntax and Execution
--------------------
The query used for algorithms and config profiles are the same, each execution is separated with spaces.
A query of `CRC CRC` will be computing 2 CRC hashes, usually the same output.

To use the same hash algorithm more than once with different config profiles, you must use the name suffixes separated by a colon, just like this:
```
CRC:myCrc1 CRC:myCrc2
```

And when you are going to assign profiles to them, you do it in a very similar way, as follows:
```
CRC:myCrc1=CRC32 CRC:myCrc2=CRC64
```

The command line results in something like this:
```
HashifyCLI -i "'Hello World'" -a "CRC:myCrc1 CRC:myCrc2" -cp "CRC:myCrc1=CRC32 CRC:myCrc2=CRC64"
```

This will generate 2 CRC hashes, one 32 bit, and the second one is 64 bit, for the same input.

The execution order for the entire process is as follows:
```
-input >> --input-finalizer >> [cyclist begin // --algorithm >> Compute >> --output-finalizer >> --output \\ cyclist end]
```

Usage Scenario Examples
-----------------------

#### Random CRC32 hash for every call:
```
HashifyCLI -i "tostring(DateTimeOffset.UtcNow.Ticks)" -a "CRC" -cp "CRC=CRC32"
```

#### Validate Hash - Ensure the computed hash equals to pre-computed hash
```
HashifyCLI -i "ReadAllBytes('HashifyNET.dll')" -if "Input" -a "MD5" -of "AsHexString() ~= '18c5770ef035f90924b988f2a947362a' and Fail('Hash Mismatch').ToString() or 'Hash Matches!'"
```

> [!NOTE]
> Call to Fail interrupts the entire execution and causes the CLI to return status code 2.

#### Compute Argon2id
```
HashifyCLI -i "'Hello World'" -a "Argon2id" -cp "Argon2id=OWASP" -of "Decode()"
```

#### Compute Argon2id and Print Directly (no timestamp)
```
HashifyCLI -i "'Hello World'" -a "Argon2id" -cp "Argon2id=OWASP" -of "Decode()" -o "PrintDirect(Result)"
```

JSON Support
------------
The CLI supports JSON config files to be passed with the `-cf` or `--config-file` parameter. This JSON file must contain config profiles or preferences for any supported hash algorithm that you'd use.

A valid JSON config is like this:
```JSON
{
	"CRC": {
		"profile": "CRC32"
	}
}
```

A custom CRC would look like this in JSON:
```JSON
{
	"CRC": {
		"config": {
			"HashSizeInBits": 32,
			"Polynomial": 79764919,
			"InitialValue": 4294967295,
			"ReflectIn": true,
			"ReflectOut": true,
			"XOrOut": 4294967295
		}
	}
}
```

Here's another JSON containing multiple configs:
```JSON
{
	"CRC": {
		"config": {
			"HashSizeInBits": 32,
			"Polynomial": 79764919,
			"InitialValue": 4294967295,
			"ReflectIn": true,
			"ReflectOut": true,
			"XOrOut": 4294967295
		}
	},
	"Argon2id": {
		"profile": "OWASP"
	},
	"FNV1a": {
		"profile": "Default"
	}
}
```

JSON containing multiple configs for the same algorithm:
```JSONC
{
	// Default CRC config that will be used for callees without a suffix.
	"CRC": {
		"profile": "CRC32"
	},

	// CRC config for myCrc1 suffix.
	"CRC:myCrc1": {
		"profile": "CRC64"
	},
}
```

> [!NOTE]
> JSON input is ignored when `-cp` or `--config-profiles` exists in the command line parameters.
> Make sure to use only one of them.
