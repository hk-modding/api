rem "If you want to use something other than 7zip add a file called 'localzip.bat' to this folder and this will call that instead"
rem "Parameter 1 is the zip name, Parameter 2 is the folder to zip, leave them as %1 %2"
if exist "localzip.bat" (
    call localzip.bat %1 %2
)else (
    "C:\Program Files\7-Zip\7z.exe" a -tzip %1 %2
)