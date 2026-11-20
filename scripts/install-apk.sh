#!/bin/bash
set -eu

ADB="$HOME/Library/Android/sdk/platform-tools/adb"

if [ "$#" -ne 1 ]; then
  echo "Usage: ./scripts/install-apk.sh /path/to/application.apk"
  exit 1
fi

APK="$1"

if [ ! -f "$APK" ]; then
  echo "ERROR: APK file not found: $APK"
  exit 1
fi

if [ ! -x "$ADB" ]; then
  echo "ERROR: ADB was not found at: $ADB"
  exit 1
fi

DEVICES=$("$ADB" devices | awk 'NR > 1 && $2 == "device" {print $1}')
COUNT=$(printf '%s\n' "$DEVICES" | awk 'NF {n++} END {print n+0}')

if [ "$COUNT" -eq 0 ]; then
  echo "ERROR: No Android device detected."
  exit 1
fi

if [ "$COUNT" -gt 1 ]; then
  echo "ERROR: More than one Android device is connected."
  echo "Connect only the intended device and try again."
  exit 1
fi

DEVICE="$DEVICES"

echo "Device: $DEVICE"
echo "APK: $APK"
echo "Installing or updating Altrafloat..."

"$ADB" -s "$DEVICE" install -r "$APK"

echo "Installation command completed."
