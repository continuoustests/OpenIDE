#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
WORKING_DIR="$1"
DEFAULT_LANGUAGE="$2"
ENABLED_LANGUAGES="$3"

#oi process-start $ROOT/bin/AutoTest.Net/AutoTest.WinForms.exe "$WORKING_DIR"
oi process-start $ROOT/bin/ContinuousTests/ContinuousTests.exe "$WORKING_DIR"
