# ALTRAFLOAT

Native Android application built with C# and .NET MAUI. The solution targets `net10.0-android` only.

The current app is a single home screen. A view model exposes status text and a command. No backend, account system, or external service is connected.

## Build

This repository is set up for GitHub Actions. The workflow in `.github/workflows/android.yml` installs the .NET 10 SDK from `global.json`, installs the MAUI Android workload, restores `ALTRAFLOAT.sln`, and compiles a Release build.

A local .NET SDK is optional. Do not commit signing keys. Release signing can be added later with GitHub Actions secrets and is not configured here.

Application id: `com.domainmakem.altrafloat`.
