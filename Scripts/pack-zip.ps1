Remove-Item ..\QuickLook.Plugin.AsepriteViewer.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\bin\Release\ -Exclude *.pdb,*.xml
Compress-Archive $files ..\QuickLook.Plugin.AsepriteViewer.zip
Move-Item ..\QuickLook.Plugin.AsepriteViewer.zip ..\QuickLook.Plugin.AsepriteViewer.qlplugin