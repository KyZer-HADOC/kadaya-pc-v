# KADAYA

2D ninja / warrior action game (MonoGame, C#). You are The Grim Reaper.

## Run
1. Install the .NET 8 SDK: https://dotnet.microsoft.com/download
2. In this folder: `dotnet run`

## Build an .exe
`dotnet publish -c Release -r win-x64 --self-contained -o publish`

## Controls
A/D move | W/Space jump (x2) | Shift shadow step | J scythe combo | K soul kunai
Hold L Soul Rasengan | E Shadow Clone Jutsu | Q Death's Eclipse | F11 fullscreen | Esc quit

## setup.exe (GitHub Actions)
Push to `main` -> Actions tab -> "Build KADAYA setup.exe" -> download the `KADAYA-setup` artifact.
Push a tag like `v1.0` to also publish setup.exe on the Releases page.
