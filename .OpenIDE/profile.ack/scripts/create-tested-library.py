#!/usr/bin/env python
import os
import sys
import time
import threading 
import subprocess

commandCompleted = False

def print_definitions():
    # Definition format usually represented as a single line:

    # Script description|
    # command1|"Command1 description"
    #   param|"Param description" end
    # end
    # command2|"Command2 description"
    #   param|"Param description" end
    # end
    print('Creates a library project with a related test library|')
    print('[[C#]]|"" ')
    print('  [[create]]|"" ')
    print('    tested-library|"Create library and test library related to it" ')
    print('      NEW_PROJECT|"Path to project file to create" ')
    print('        NUNIT_FRAMEWORK|"Path to nunit.framework.dll" end ')
    print('      end ')
    print('    end ')
    print('  end ')
    print('end ')

def write(message):
    sys.stdout.write(message + '\n')
    sys.stdout.flush()

def runProcess(exe,workingDir=""):    
    if workingDir == "":
        workingDir = os.getcwd()
    p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=workingDir)
    while(True):
        retcode = p.poll() # returns None while subprocess is running
        line = p.stdout.readline().decode(encoding='windows-1252').strip('\n').strip('\r')
        if line != "":
            yield line
        if(retcode is not None):
            break

def toSingleLine(exe, workingDir=""):
    lines = ""
    for line in runProcess(exe, workingDir):
        lines = lines + line
    return lines

def readThread():
    line = sys.stdin.readline()
    while line:
        if line == "end-of-command\n":
            commandCompleted = True
            break

def runCommand(command):
    commandCompleted = False;
    waitForEndOfCommand = threading.Thread(target = readThread)
    waitForEndOfCommand.start()
    write(command)
    waitForEndOfCommand.join()

def run_command(run_location, global_profile, local_profile, args):
    # Script parameters
    #   Param 1: Script run location
    #   Param 2: global profile name
    #   Param 3: local profile name
    #   Param 4-: Any passed argument
    #
    # When calling oi use the --profile=PROFILE_NAME and 
    # --global-profile=PROFILE_NAME argument to ensure calling scripts
    # with the right profile.
    #
    # To post back oi commands print command prefixed by command| to standard output
    # To post a comment print to std output prefixed by comment|
    # To post an error print to std output prefixed by error|
    # Place script core here
    path = args[0]
    if path.endswith('.csproj') == False:
        path += '.csproj'
    projectname = os.path.splitext(os.path.basename(args[0]))[0]
    testpath = os.path.join(os.path.dirname(args[0]) + ".Tests", projectname + ".Tests.csproj")

    runCommand('command|create library "' + args[0] + '"')
    runCommand('command|create library "' + testpath + '"')
    runCommand('command|reference "' + path + '" "' + testpath + '"')
    runCommand('command|reference "' + args[1] + '" "' + testpath + '"')

if __name__ == "__main__":
    args = sys.argv
    if len(args) > 1 and args[2] == 'get-command-definitions':
        print_definitions()
    else:
        run_command(args[1], args[2], args[3], args[4:])
