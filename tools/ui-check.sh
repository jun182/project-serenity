#!/usr/bin/env bash
# Walk through the game's UI on a phone over adb and save a screenshot of each screen + one contact sheet.
# Usage: tools/ui-check.sh [apk] [device]   (device default: Nord 2T over Tailscale; coords are for a 2400x1080 landscape screen)
# Taps are blind (no UI inspection), so look at the sheet; if a screen is off, the step list below is the place to fix.
set -u
APK=${1:-}; S=${2:-100.64.3.98:5555}; PKG=se.vibing.serenity
OUT=${OUT:-$(mktemp -d)}; mkdir -p "$OUT"; n=0
adb connect "$S" >/dev/null 2>&1
adb -s "$S" get-state >/dev/null 2>&1 || { echo "device $S not reachable"; exit 1; }
[ -n "$APK" ] && { adb -s "$S" install -r "$APK" | tail -1; }
adb -s "$S" shell svc power stayon true; adb -s "$S" shell input keyevent KEYCODE_WAKEUP; adb -s "$S" shell wm dismiss-keyguard
adb -s "$S" shell am force-stop $PKG; adb -s "$S" shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1; sleep 10
shot() { n=$((n+1)); f=$(printf "%s/%02d-%s.png" "$OUT" $n "$1"); adb -s "$S" exec-out screencap -p > "$f"; }
tap()  { adb -s "$S" shell input tap "$1" "$2"; sleep "${3:-1.5}"; }
shot menu
tap 2142 90;  shot howto;      tap 1200 980            # How to play, Close
tap 2142 992; shot mode2;      tap 2142 992            # Mode toggle (once, then back)
tap 257 992;  shot tilt-off;   tap 257 992             # Tilt toggle (once, then back)
adb -s "$S" shell input tap 1200 756; sleep 0.3; shot ripple; sleep 6; shot game   # touch anywhere starts; ripple shot taken 0.3 s after
tap 100 100;  shot pause                               # II = pause menu
echo "$OUT"; ls "$OUT"
# contact sheet: 3 columns
ffmpeg -loglevel error -y -pattern_type glob -i "$OUT/[0-9]*.png" -vf "scale=800:-1,tile=3x3" -frames:v 1 "$OUT/sheet.png" && echo "$OUT/sheet.png"
