@ECHO off
TITLE 9b0N_fbx_hpatchz_imm
ECHO "   ___  _      ___  _   _    __ _           _                 _       _           _____                     "
ECHO "  / _ \| |    / _ \| \ | |  / _| |         | |               | |     | |         |_   _|                    "
ECHO " | (_) | |__ | | | |  \| | | |_| |____  __ | |__  _ __   __ _| |_ ___| |__  ____   | |  _ __ ___  _ __ ___  "
ECHO "  \__, | '_ \| | | | . ` | |  _| '_ \ \/ / | '_ \| '_ \ / _` | __/ __| '_ \|_  /   | | | '_ ` _ \| '_ ` _ \ "
ECHO "    / /| |_) | |_| | |\  | | | | |_) >  <  | | | | |_) | (_| | || (__| | | |/ /   _| |_| | | | | | | | | | |"
ECHO "   /_/ |_.__/ \___/|_| \_| |_| |_.__/_/\_\ |_| |_| .__/ \__,_|\__\___|_| |_/___| |_____|_| |_| |_|_| |_| |_|"
ECHO "                       ______          ______    | |                         ______                         "
ECHO "                      |______|        |______|   |_|                        |______|                        "
ECHO.
ECHO v0.2
ECHO.

SET OLDPATH="..\RINDO.fbx"
SET PATPATH=".\_hdiff\RINDO_RINDO_FT.hdiff"
::Zev
IF EXIST %OLDPATH% (
  GOTO FoundFileOld 
) ELSE (
  ECHO Failed to find files in %OLDPATH%
  GOTO CUSTOM_EOF
)
::Zev
:FoundFileOld
IF EXIST %PATPATH% (
  GOTO FoundFilePat 
) ELSE (
  ECHO Failed to find files in %PATPATH%
  GOTO CUSTOM_EOF
)
::Zev
:FoundFilePat
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "str_date=%YYYY%%MM%%DD%%HH%%Min%%Sec%"

SET plusname_space=output_%str_date%
SET plusname=%plusname_space: =%
::Zev
COPY "%OLDPATH%" "%OLDPATH%.back_%plusname%"
hpatchz.exe -f "%OLDPATH%" "%PATPATH%" "%OLDPATH%"

:CUSTOM_EOF
PAUSE
EXIT