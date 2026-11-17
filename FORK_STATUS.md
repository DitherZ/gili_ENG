# Fork migration status

Branch: `refactor/global-english-cleanup`

## Scope

This fork removes Indonesian-specific and religious functionality from the upstream Gili application while retaining general desktop utility features.

### Removed

- Prayer-time calculations and schedules.
- Adzan and imsak reminders, notifications and chimes.
- Hijri calendar and Ramadan-specific behaviour.
- Religious theme and prayer-specific interface states.
- BMKG weather API integration.
- Indonesian administrative-region lookup and BMKG `adm4` configuration.

### Replaced or retained

- Generic IP-based location detection is retained in `src/Location.cs`.
- Weather is served by Open-Meteo using latitude and longitude.
- The former prayer page is now a general weather page.
- Media, lyrics, Pomodoro, meetings, notifications, clipboard, file shelf, device activity, system monitoring and visual themes remain in scope.
- Existing Indonesian theme values are normalized by the theme layer where possible for settings-file compatibility.

## Validation checklist

- [x] Fork and write access verified.
- [x] Dedicated refactor branch created.
- [x] Prayer calculation module removed.
- [x] Generic location detection extracted to `Location.cs`.
- [x] BMKG service replaced with Open-Meteo.
- [x] Weather view and location flow updated.
- [x] Prayer and Ramadan UI/state hooks removed from the main island and settings.
- [x] English README and build output added.
- [ ] Complete English string audit across all source files.
- [ ] Verify no stale prayer/BMKG symbols or settings remain.
- [ ] Windows build with the repository's `build.cmd`.
- [ ] Runtime smoke test on Windows 10/11.
- [ ] Merge validated branch into `main`.

## Build and runtime notes

The project targets native Windows APIs and .NET Framework 4.8. A successful build must be confirmed on Windows with the required Windows SDK WinRT metadata. The Open-Meteo integration and IP geolocation flow also require network access.

Do not treat this branch as release-ready until the outstanding validation items are complete.
