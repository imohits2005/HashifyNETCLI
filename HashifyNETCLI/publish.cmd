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
@pause