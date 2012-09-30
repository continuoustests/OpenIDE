#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
WORKING_DIR="$1"
DEFAULT_LANGUAGE="$2"
ENABLED_LANGUAGES="$3"
SLN="$(oi configure read autotest.solution)"

WATCH_PATH="$WORKING_DIR"
if [[ "$SLN" != "" ]]; then
	WATCH_PATH=$WORKING_DIR/$SLN
fi

#oi process-start $ROOT/bin/AutoTest.Net/AutoTest.WinForms.exe "$WATCH_PATH"
oi process-start $ROOT/bin/ContinuousTests/ContinuousTests.exe "$WATCH_PATH"
