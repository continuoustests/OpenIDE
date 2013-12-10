#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
WORKING_DIR="$1"
DEFAULT_LANGUAGE="$2"
ENABLED_LANGUAGES="$3"

cd "$WORKING_DIR"
CONFIG_DIR="$(oi conf read rootpoint)"
SLN="$(oi conf read autotest.solution)"
cd "$ROOT"

WATCH_PATH="$WORKING_DIR"
if [[ "$SLN" != "" ]]; then
	WATCH_PATH=$CONFIG_DIR/$SLN
fi

#oi process start $ROOT/bin/AutoTest.Net/AutoTest.WinForms.exe "$WATCH_PATH"
#oi process start $ROOT/bin/ContinuousTests/AutoTest.GtkSharp.exe "$WATCH_PATH"
oi process start $ROOT/bin/ContinuousTests/ContinuousTests.exe "$WATCH_PATH"
