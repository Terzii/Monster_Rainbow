@echo off
cd /d "%~dp0"
if not exist MonsterRainbow.exe (
  echo Once Derle.bat dosyasini calistirin.
  pause
  exit /b 1
)
powershell -NoProfile -ExecutionPolicy Bypass -Command "$s=(New-Object -ComObject WScript.Shell).CreateShortcut([Environment]::GetFolderPath('Startup')+'\MonsterRainbow.lnk');$s.TargetPath='%~dp0MonsterRainbow.exe';$s.WorkingDirectory='%~dp0';$s.Save()"
echo Windows baslangicina eklendi.
pause
