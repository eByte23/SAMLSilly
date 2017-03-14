@ECHO OFF

echo Build Soultion....
start /B /WAIT dotnet build SAMLSilly.sln

IF %ERRORLEVEL% EQU 1 exit

if %1==/pack (
    echo Packaging
    start /B /WAIT dotnet pack SAMLSilly.sln -c Debug --version-suffix alpha%2
)