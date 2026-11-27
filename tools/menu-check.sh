#!/usr/bin/env bash
# Walk the menu tree (MENU-TREE.md) with adb key events and check the "MENU page=.. sel=.." log lines. No screenshots.
# Usage: tools/menu-check.sh [apk] [device]   (device default: the first adb device; run `adb connect` first for wireless). Exit 1 on the first mismatch.
set -u
APK=${1:-}; S=${2:-$(adb devices | awk 'NR==2{print $1}')}; PKG=se.vibing.serenity
A="adb -s $S"
[ -n "$APK" ] && { $A install -r "$APK" | tail -1; }
$A shell input keyevent KEYCODE_WAKEUP; $A shell wm dismiss-keyguard
$A shell am force-stop $PKG; $A shell pm clear $PKG >/dev/null; $A logcat -c  # clean prefs: known defaults
$A shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1; sleep 10
fail=0
# step "<keys>" "<expected last MENU state>"   keys: names without KEYCODE_ prefix, e.g. DPAD_DOWN BUTTON_A
step() {
  for k in $1; do $A shell input keyevent KEYCODE_$k; sleep 0.6; done
  sleep 0.4
  got=$($A logcat -d -s Unity | grep -o 'MENU page=.*' | tail -1 | sed 's/[[:space:]]*$//')
  if [ "$got" = "MENU $2" ]; then echo "ok    [$1] -> $2"; else echo "FAIL  [$1] expected '$2' got '$got'"; fail=1; fi
}
# Pad mode: if no pad is connected the first press only wakes pad mode (sel=Play); with a pad connected it already moves (sel=Exit).
$A shell input keyevent KEYCODE_DPAD_DOWN; sleep 1
[ "$($A logcat -d -s Unity | grep -o 'MENU page=.*' | tail -1)" = "MENU page=start sel=Exit" ] && { $A shell input keyevent KEYCODE_DPAD_DOWN; sleep 1; }
step "" "page=start sel=Play"
step "DPAD_DOWN"                 "page=start sel=Exit"
step "DPAD_UP"                   "page=start sel=Play"
step "BUTTON_B"                  "page=start sel=Play"          # B on the start screen does nothing (state unchanged)
step "BUTTON_START"              "page=main sel=Close"
step "DPAD_DOWN"                 "page=main sel=Settings"
step "BUTTON_A"                  "page=settings sel=Frame rate: 60"
step "BUTTON_B"                  "page=main sel=Close"
step "DPAD_DOWN DPAD_DOWN"       "page=main sel=Controls"
step "BUTTON_A"                  "page=controls sel=Tilt steering: OFF"
step "BUTTON_B"                  "page=main sel=Close"
step "DPAD_DOWN DPAD_DOWN DPAD_DOWN" "page=main sel=How to play"
step "BUTTON_A"                  "page=help sel=Close"
step "BUTTON_B"                  "page=main sel=Close"
step "BUTTON_START"              "page=start sel=Play"
step "BUTTON_A"                  "page=game sel=-"
step "BUTTON_START"              "page=main sel=Resume"
step "BUTTON_START"              "page=game sel=-"
$A shell am force-stop $PKG
[ $fail = 0 ] && echo "menu-check: all ok" || { echo "menu-check: FAILED"; exit 1; }
