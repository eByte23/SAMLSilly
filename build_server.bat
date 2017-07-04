dotnet restore .\SAMLSilly.sln
dotnet build .\SAMLSilly.sln
IF %ERRORLEVEL% EQU 1 exit