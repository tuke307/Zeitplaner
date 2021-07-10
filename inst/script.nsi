!define APPNAME "Zeitplaner"
OutFile "ZeitplanerInstaller.exe"


!include "MUI2.nsh"


Name "${APPNAME}Installer.exe"
OutFile "${APPNAME}Installer.exe"


InstallDir "$PROGRAMFILES\${APPNAME}"


!insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  ;uninstaller
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES


!insertmacro MUI_LANGUAGE "German"


Section "Installationsdateien"

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; clear INSTDIR
  RMDir /r "$INSTDIR"
  
  ; Put file there
  File C:\Users\praktikant\Desktop\Deploy\*
  
  ; uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM "SOFTWARE\${APPNAME}" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
SectionEnd


Section "DesktopShorcut"

 CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe" ""
  
SectionEnd


Section "Uninstall"

  ;installation directory
  Delete "$INSTDIR\${APPNAME}.exe"
  Delete "$INSTDIR\uninstall.exe"
  ;RMDir /r "$INSTDIR"
 
  ; shortcuts
  Delete "$DESKTOP\${APPNAME}.lnk"
  Delete "$SMPROGRAMS\${APPNAME}\*.*"
  RMDir "$SMPROGRAMS\${APPNAME}"
  
  ; Registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
  DeleteRegKey HKLM "SOFTWARE\${APPNAME}"

  ; Appdata
  RMDir /r "$APPDATA\${APPNAME}"
  
SectionEnd
