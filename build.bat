@ECHO OFF


IF "%1"=="/build-server" (
    call :build_server
)

IF "%1"=="/resgen" (
    call :do_resgen
)

IF "%1"=="/resgen-only" (
    call :do_resgen
    exit /B 0
)

echo Build Soultion....
start /B /WAIT dotnet restore SAMLSilly.sln
start /B /WAIT dotnet build SAMLSilly.sln

IF %ERRORLEVEL% EQU 1 exit

IF "%1"=="/pack" (
    echo Packaging
    start /B /WAIT dotnet pack SAMLSilly.sln -c Debug --version-suffix alpha%2
)


:do_resgen
    echo Generating Strongly Typed Resources
    call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
    IF %ERRORLEVEL% EQU 1 exit
    start /B /WAIT resgen .\src\SAMLSilly\ErrorMessages.resx /publicClass /str:cs,SAMLSilly,,.\src\SAMLSilly\ErrorMessages.Designer.cs
    del .\src\SAMLSilly\ErrorMessages.resources
    IF %ERRORLEVEL% EQU 1 exit
    start /B /WAIT resgen .\src\SAMLSilly\TraceMessages.resx /publicClass /str:cs,SAMLSilly,,.\src\SAMLSilly\TraceMessages.Designer.cs
    del .\src\SAMLSilly\TraceMessages.resources
    IF %ERRORLEVEL% EQU 1 exit
    start /B /WAIT resgen .\src\SAMLSilly\Resources.resx /publicClass /str:cs,SAMLSilly,,.\src\SAMLSilly\Resources.Designer.cs
    del .\src\SAMLSilly\Resources.resources
    IF %ERRORLEVEL% EQU 1 exit

:build_server
    call :do_resgen
    dotnet restore .\SAMLSilly.sln
    dotnet build .\SAMLSilly.sln