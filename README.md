# Gili — English, global edition

Gili is a native Windows Dynamic Island-style desktop companion. This fork removes the original Indonesian-specific and religious features and provides an English-first experience with global weather support.

> This fork is a community-maintained refactor of the upstream project. It retains the original MIT licence and attribution; see [LICENSE](LICENSE).

## What changed in this fork

- All user-facing application text, menus, dialogs, settings, notifications, demo content and documentation are in English.
- Removed prayer schedules, adzan/imsak reminders, religious calendar calculations, Ramadan behaviour and the religious theme.
- Removed BMKG and Indonesian administrative-region lookup dependencies.
- Replaced BMKG forecasts with [Open-Meteo](https://open-meteo.com/) using latitude and longitude.
- Kept generic IP-based location detection and moved it into its own `Location.cs` service.
- Replaced the former prayer page with a general weather page.

## Features retained

- Dynamic Island-style compact and expanded desktop interface.
- Media controls and synchronized lyrics through LRCLIB.
- Audio spectrum visualizer.
- Pomodoro focus timer and break cycles.
- Weather conditions and rain alerts.
- Meeting agenda, reminders and iCal calendar integration.
- Windows notifications and privacy activity indicators.
- Clipboard history and temporary file shelf.
- Network, device, battery and system activity indicators.
- Auto-hide, notch/floating shapes, themes and Liquid Glass rendering.

## Weather and location

Weather forecasts are retrieved from Open-Meteo using the configured latitude and longitude. The app requests hourly temperature, humidity and WMO weather codes. Rain alerts are generated from forecast slots.

On first launch, Gili can estimate a city-level location from the public IP using ipwho.is, with ip-api.com as a fallback. This is approximate and may not identify the correct suburb. You can edit the `lat`, `lon` and `city` values in:

`%APPDATA%\Gili\settings.ini`

The tray menu includes a weather-location action and a shortcut to edit the settings file. Location detection contacts the IP geolocation provider; weather requests are sent to Open-Meteo. Review those providers' privacy policies if this matters to your deployment.

## Build requirements

- Windows 10 or Windows 11, 64-bit.
- .NET Framework 4.8 runtime and the Windows Framework C# compiler.
- Windows SDK WinRT metadata files referenced by `build.cmd`.

Build from a Windows command prompt:

```bat
build.cmd
```

The script compiles all `src\*.cs` files into `Gili.exe` in the repository root.

## Settings and data

Application settings and logs are stored under:

`%APPDATA%\Gili\`

This includes `settings.ini`, `gili.log`, cached lyrics and meeting data. Weather location settings use `lat`, `lon` and `city`; no Indonesian administrative code is required.

## Project status

The refactor is in progress. The working branch is `refactor/global-english-cleanup`. The fork's `main` branch should only be updated after the source audit and Windows build validation are complete.

## Credits

- Upstream project: [maulanarizky16/gili](https://github.com/maulanarizky16/gili)
- Weather data: [Open-Meteo](https://open-meteo.com/)
- IP geolocation: [ipwho.is](https://ipwho.is/) and [ip-api.com](https://ip-api.com/)
- Synchronized lyrics: [LRCLIB](https://lrclib.net)

See [LICENSE](LICENSE) for licence terms and upstream attribution.
