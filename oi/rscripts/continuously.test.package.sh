#!/bin/bash 

if [ "$1" = "reactive-script-reacts-to" ]; then
    # Write one event pr line that this script will react to
    echo "'codemodel' 'raw-filesystem-change-*.oi-pkg-tests.*'"
    exit
fi

# Write scirpt code here.
#   Param 1: event
#   Param 2: global profile name
#   Param 3: local profile name
#
# When calling other commands use the --profile=PROFILE_NAME and 
# --global-profile=PROFILE_NAME argument to ensure calling scripts
# with the right profile.
iconOK="`dirname \"$0\"`/continuously.test.package-files/graphics/circleWIN.png"
iconFAIL="`dirname \"$0\"`/continuously.test.package-files/graphics/circleFAIL.png"

file=$(echo "$1"|cut -d' ' -f 3)
#filename=`basename "$file"`

#end=$(echo $filename | grep -b -o ".oi-pkg-test." | awk 'BEGIN {FS=":"}{print $1}')
#name=${filename:0:end}

result=`oi packagetest "$file" --only-errors -o -e|sed 's/ *$//g'`
failed=false
if [[ "$result" == *FAILED* ]]; then
    failed=true
fi
if [[ "$result" == *\?\?\?\?\?\?* ]]; then
    failed=true
fi

if [[ $failed == false ]] ; then
    notify-send --icon="$iconOK" "Tests passed"
else
    notify-send --icon="$iconFAIL" "Tests FAILED" "$result"
fi