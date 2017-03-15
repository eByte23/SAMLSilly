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
dotnet restore .\SAMLSilly.sln
dotnet build .\SAMLSilly.sln
IF %ERRORLEVEL% EQU 1 exit