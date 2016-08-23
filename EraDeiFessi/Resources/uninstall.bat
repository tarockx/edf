:BEGINNING
cls
echo off
echo EDF Uninstallation script
echo EDF installation path is: %1
echo EDF config files path is: %2

if [%1]==[] goto :USAGE
if [%2]==[] goto :USAGE
GOTO EDFCHECK

:EDFCHECK
FOR /F %%x IN ('tasklist /NH /FI "IMAGENAME eq EraDeiFessi.exe"') DO IF %%x == EraDeiFessi.exe goto WAITFOREXIT
if [%1]==[UNINSTALL] GOTO UNINSTALL
if [%1]==[CLEAN] GOTO CLEAN
GOTO USAGE

:WAITFOREXIT
	echo In attesa della chiusura di EDF...
	PING -n 2 127.0.0.1 > nul
	GOTO EDFCHECK

:UNINSTALL
echo Uninstalling EDF...
echo Removing EDF files...
rmdir /s /q "%~f2"
echo Removing saved settings...
rmdir /s /q "%~f3"

cls
echo ---------------------------------------------------------
echo Disinstallazione completata!
echo EraDeiFessi e' stato rimosso dal computer
echo NOTA:
echo   Eventuali scorciatoie devono essere rimosse manualmente
echo ---------------------------------------------------------
goto END

:CLEAN
echo Removing saved settings...
rmdir /s /q "%~f2"

cls
echo ---------------------------------------------------------
echo Impostazioni rimosse!
echo EraDeiFessi e' stato reimpostato ai settaggi predefiniti
echo ---------------------------------------------------------
goto END


:USAGE
echo Disinstallazione fallita: parametri mancanti o non corretti
goto END

:END
pause
start /b "" cmd /c del "%~f0"&exit /b
exit