REM @ECHO OFF

REM *********************
REM  Limpia el escritorio
REM *********************

del \windows\Desktop\TelnetCE.lnk
del \windows\Desktop\"Install Samples.lnk"
del \windows\Desktop\"Media Player.lnk"
del \windows\Desktop\"Microsoft WordPad.lnk"
del \windows\Desktop\"Remote Desktop Connection.lnk"
del \windows\Desktop\"Internet Explorer.lnk
del \windows\Desktop\"My Documents.lnk"

REM ****************************
REM Limpia "Inicio - Programas"
REM ****************************

del \Windows\Programs\"Airbeam Client.lnk"
del \Windows\Programs\"BT Information.lnk"
del \Windows\Programs\"BTScannerCtlPanel.lnk"
del \Windows\Programs\"Command Prompt.lnk"
del \Windows\Programs\"Internet Explorer.lnk"
del \Windows\Programs\"Media Player.lnk"
del \Windows\Programs\"Microsoft WordPad.lnk"
del \Windows\Programs\"MSP Agent.lnk"
del \Windows\Programs\"Rapid Deployment.lnk"
del \Windows\Programs\"Remote Desktop Connection.lnk"
del \Windows\Programs\"TelnetCE.lnk"
del \Windows\Programs\Samples.C*
del \Windows\Programs\"Windows Explorer.lnk"

Del \Windows\Programs\Communication\"Terminal.lnk"
RD \Windows\Programs\Communication
Del \Windows\Programs\Fusion\Wire*.*
RD \Windows\Programs\Fusion
del \Windows\Programs\CtlPanel*.*

REM ************************
REM Crea del VNC
REM ************************
ECHO
if not exist "\Application\VNC" mkdir \Application\VNC

REM ************************
REM Crea del Scanner
REM ************************
ECHO
if not exist "\Application\Samples.C" mkdir \Application\Samples.C

REM ************************
REM Crea del StartUp
REM ************************
ECHO
if not exist "\Application\StartUp" mkdir \Application\StartUp


