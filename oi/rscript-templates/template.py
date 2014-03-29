#!/usr/bin/env python
import sys

def print_react_patterns():
    # Write one event pr line that this script will react to
    print("goto*.py|*")
    
def handle_event(event, global_profile, local_profile, args):
    # Write scirpt code here.
    print("Hello world!")

if __name__ == "__main__":
    #   Param 1: event
    #   Param 2: global profile name
    #   Param 3: local profile name
    #
    # When calling other commands use the --profile=PROFILE_NAME and 
    # --global-profile=PROFILE_NAME argument to ensure calling scripts
    # with the right profile.
    args = sys.argv
    if len(args) > 1 and args[1] == 'reactive-script-reacts-to':
        print_react_patterns()
    else:
        handle_event(args[1], args[2], args[3], args[4:])
