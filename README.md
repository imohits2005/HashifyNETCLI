Welcome to HashifyNET Command Line Interface
--------------------------------------------

HashifyNET CLI is a professional command-line utility for computing various cryptographic and non-cryptographic hash algorithms supported by [HashifyNET](https://github.com/Deskasoft/HashifyNET).

This CLI relies completely on the [HashifyNET](https://github.com/Deskasoft/HashifyNET) project to demonstrate its capabilities and provide a standardized method for computing multiple hash algorithms.

HashifyNET CLI uses runtime C# scripts for I/O operations. This makes it as flexible as writing C# code without having to write a complete C# program. You are able to process any input and output with anything you desire without limitations.

We support command-line parameters, a file for specifying command-line parameters (preferably for long command lines and to avoid getting stuck by operating system limitations), and JSON config files to customize your own settings for the hash algorithms you want to use.

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

## Quick computation of file HashifyNET.dll using MD5, CRC32 and CRC64
```
HashifyCLI -i "ReadAllBytes('HashifyNET.dll')" -if "Input" -a "MD5 CRC:1 CRC:2" -cp "CRC:1=CRC32 CRC:2=CRC64"
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
```JSON
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
