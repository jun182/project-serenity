#!/usr/bin/env bash
# Walk the menu tree (specs/system/menus.md) with adb key events and check the "MENU page=.. sel=.." log lines. No screenshots.
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
# Developer mode (session-only, hidden): Konami code on the start screen, and 7 taps on the build number in Settings.
dev() { $A logcat -d -s Unity | grep -o 'MENU dev=.*' | tail -1; }
$A shell am force-stop $PKG; $A logcat -c; $A shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1; sleep 10
$A shell input keyevent KEYCODE_DPAD_DOWN; sleep 1; $A shell input keyevent KEYCODE_DPAD_UP; sleep 1
step "BUTTON_START DPAD_DOWN BUTTON_A DPAD_DOWN DPAD_DOWN DPAD_DOWN DPAD_DOWN DPAD_DOWN" "page=settings sel=Build b$(grep -o 'Number = [0-9]*' Assets/Scripts/BuildInfo.cs | grep -o '[0-9]*')"
[ -z "$(dev)" ] && echo "ok    dev mode off by default" || { echo "FAIL  dev mode on before unlock"; fail=1; }
for i in 1 2 3 4 5 6; do $A shell input keyevent KEYCODE_BUTTON_A; sleep 0.5; done
[ -z "$(dev)" ] && echo "ok    6 taps: still locked" || { echo "FAIL  unlocked after 6 taps"; fail=1; }
$A shell input keyevent KEYCODE_BUTTON_A; sleep 0.7
[ "$(dev)" = "MENU dev=ON" ] && echo "ok    7 taps: developer mode ON" || { echo "FAIL  7 taps did not unlock"; fail=1; }
for k in DPAD_DOWN BUTTON_A; do $A shell input keyevent KEYCODE_$k; sleep 0.6; done
s1=$($A logcat -d -s Unity | grep -o 'MENU page=.*' | tail -1 | sed 's/[[:space:]]*$//')   # pm clear is denied on some phones, so the starting state may be ON or OFF
$A shell input keyevent KEYCODE_BUTTON_A; sleep 0.8
s2=$($A logcat -d -s Unity | grep -o 'MENU page=.*' | tail -1 | sed 's/[[:space:]]*$//')
s1=${s1#MENU }; s2=${s2#MENU }
case "$s1" in "page=dev sel=Debug panel: "*) [ "$s1" != "$s2" ] && echo "ok    dev page: Debug panel toggles ($s1 -> $s2)" || { echo "FAIL  Debug panel did not toggle ($s1)"; fail=1; };; *) echo "FAIL  not on dev page: $s1"; fail=1;; esac
step "DPAD_DOWN DPAD_DOWN DPAD_DOWN BUTTON_A" "page=settings sel=Frame rate: 60"   # Back from the dev page
# restart: must be locked again, then Konami (up up down down left right left right B A) must unlock without starting the game
$A shell am force-stop $PKG; $A logcat -c; $A shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1; sleep 10
$A shell input keyevent KEYCODE_DPAD_DOWN; sleep 1; $A shell input keyevent KEYCODE_DPAD_UP; sleep 1
[ -z "$(dev)" ] && echo "ok    restart: locked again" || { echo "FAIL  dev mode survived restart"; fail=1; }
step "BUTTON_B DPAD_UP DPAD_UP DPAD_DOWN DPAD_DOWN DPAD_LEFT DPAD_RIGHT DPAD_LEFT DPAD_RIGHT BUTTON_B BUTTON_A" "page=start sel=Exit"   # the d-pad presses also move the highlight (Exit after 2 downs)
[ "$(dev)" = "MENU dev=ON" ] && echo "ok    Konami: developer mode ON, game not started" || { echo "FAIL  Konami did not unlock (or started the game)"; fail=1; }
$A shell am force-stop $PKG
[ $fail = 0 ] && echo "menu-check: all ok" || { echo "menu-check: FAILED"; exit 1; }
