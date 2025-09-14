@echo off
dotnet publish -c Release -p:PublishProfile=Profile_Contained_win-x86
dotnet publish -c Release -p:PublishProfile=Profile_Contained_win-x64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_win-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_osx-x64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_osx-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_linux-x64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_linux-arm
dotnet publish -c Release -p:PublishProfile=Profile_Contained_linux-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Contained_linux-musl-x64

dotnet publish -c Release -p:PublishProfile=Profile_Dependent_win-x86
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_win-x64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_win-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_osx-x64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_osx-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_linux-x64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_linux-arm
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_linux-arm64
dotnet publish -c Release -p:PublishProfile=Profile_Dependent_linux-musl-x64

SETLOCAL

SET "PROJECT_FILE=HashifyNETCLI.csproj"
FOR /F "usebackq" %%i IN (`powershell -Command "((Select-Xml -Path '%PROJECT_FILE%' -XPath '//Version').Node.InnerText).Replace('.', '-')"`) DO (
    SET "PROJECT_VERSION=%%i"
)

cd "bin\Release\net8.0\publish\contained\"

echo Archiving...

IF DEFINED PROJECT_VERSION (
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-x86-contained.zip" "win-x86"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-x64-contained.zip" "win-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-arm64-contained.zip" "win-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-osx-x64-contained.zip" "osx-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-osx-arm64-contained.zip" "osx-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-x64-contained.zip" "linux-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-arm-contained.zip" "linux-arm"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-arm64-contained.zip" "linux-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-musl-x64-contained.zip" "linux-musl-x64"
) ELSE (
tar -a -c -f "hashifycli-win-x86-contained.zip" "win-x86"
tar -a -c -f "hashifycli-win-x64-contained.zip" "win-x64"
tar -a -c -f "hashifycli-win-arm64-contained.zip" "win-arm64"
tar -a -c -f "hashifycli-osx-x64-contained.zip" "osx-x64"
tar -a -c -f "hashifycli-osx-arm64-contained.zip" "osx-arm64"
tar -a -c -f "hashifycli-linux-x64-contained.zip" "linux-x64"
tar -a -c -f "hashifycli-linux-arm-contained.zip" "linux-arm"
tar -a -c -f "hashifycli-linux-arm64-contained.zip" "linux-arm64"
tar -a -c -f "hashifycli-linux-musl-x64-contained.zip" "linux-musl-x64"
)

cd "..\dependent\"

IF DEFINED PROJECT_VERSION (
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-x86-dependent.zip" "win-x86"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-x64-dependent.zip" "win-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-win-arm64-dependent.zip" "win-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-osx-x64-dependent.zip" "osx-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-osx-arm64-dependent.zip" "osx-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-x64-dependent.zip" "linux-x64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-arm-dependent.zip" "linux-arm"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-arm64-dependent.zip" "linux-arm64"
tar -a -c -f "hashifycli-%PROJECT_VERSION%-linux-musl-x64-dependent.zip" "linux-musl-x64"
) ELSE (
tar -a -c -f "hashifycli-win-x86-dependent.zip" "win-x86"
tar -a -c -f "hashifycli-win-x64-dependent.zip" "win-x64"
tar -a -c -f "hashifycli-win-arm64-dependent.zip" "win-arm64"
tar -a -c -f "hashifycli-osx-x64-dependent.zip" "osx-x64"
tar -a -c -f "hashifycli-osx-arm64-dependent.zip" "osx-arm64"
tar -a -c -f "hashifycli-linux-x64-dependent.zip" "linux-x64"
tar -a -c -f "hashifycli-linux-arm-dependent.zip" "linux-arm"
tar -a -c -f "hashifycli-linux-arm64-dependent.zip" "linux-arm64"
tar -a -c -f "hashifycli-linux-musl-x64-dependent.zip" "linux-musl-x64"
)

ENDLOCAL

echo Completed!

@pause
