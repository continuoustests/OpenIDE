#!/usr/bin/env python
import os
import sys
import shutil
import subprocess
import collections
sys.path.append(os.path.dirname(__file__))
import tests

def getTests():
    return {
        "Can create and load local profile":
            canCreateLocalProfile,
        "Can create and load global profile":
            canCreateGlobalProfile
    }

def canCreateLocalProfile():
    tests.out("command|profile init new-local-profile")
    tests.out("command|profile load new-local-profile")
    tests.out("command|profile list")
    tests.assertOn(tests.hasOutput("Active local profile:  new-local-profile"))

def canCreateGlobalProfile():
    root = "nonexistent"
    for line in runProcess(["oi", "conf", "read", "rootpoint", "-g"]):
        root=line
    profile = os.path.join(root, ".OpenIDE", "active.profile")
    backup = profile + ".backup"
    if os.path.exists(profile) == False:
        profile = None
    if not profile == None:
        shutil.copyfile(profile, backup)

    tests.out("command|profile init new-global-profile -g")
    tests.out("command|profile load new-global-profile -g")
    tests.out("command|profile list")

    if not profile == None:
        os.remove(profile)
        shutil.copyfile(backup, profile)
        os.remove(backup)

    tests.assertOn(tests.hasOutput("Active global profile: new-global-profile"))
    tests.out("command|profile rm new-global-profile -g")

def runProcess(exe):    
    p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    while(True):
        retcode = p.poll() # returns None while subprocess is running
        line = p.stdout.readline().decode(encoding='windows-1252').strip('\n').strip('\r')
        if line != "":
            yield line
        if(retcode is not None):
            break

if __name__ == "__main__":
    tests.main("initialized", getTests)