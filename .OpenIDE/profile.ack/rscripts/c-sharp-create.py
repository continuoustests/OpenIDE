#!/usr/bin/env python
import os, sys, subprocess

def print_react_patterns():
    print("'c-sharp-create'")
    print("'user-selected' 'c-sharp-create' '*")
    print("'user-inputted' 'c-sharp-create-*")

def run_process(exe,working_dir=""):    
    if working_dir == "":
        working_dir = os.getcwd()
    p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=working_dir)
    while(True):
        retcode = p.poll() # returns None while subprocess is running
        line = p.stdout.readline().decode(encoding='windows-1252').strip('\n').strip('\r')
        if line != "":
            yield line
        if(retcode is not None):
            break

def to_single_line(exe, working_dir=""):
    lines = ""
    for line in run_process(exe, working_dir):
        lines = lines + line
    return lines

def get_caret():
    output = [];
    sys.stdout.write("request|editor get-caret\n")
    sys.stdout.flush()
    while True:
        line = sys.stdin.readline().strip("\n")
        if line == "end-of-conversation":
            break;
        output.append(line)
    caret = output[0].split("|")
    return caret[0]

def handle_event(event, global_profile, local_profile, args):
    if event == "'c-sharp-create'":
        output = to_single_line(["oi", "get-commands", "C#", "new"]).strip().split(" ")
        option_string = (''.join(map(lambda x: "New "+x.title()+',', output))).strip(",")
        print("command|editor user-select c-sharp-create \""+option_string+"\"")
    elif event.startswith("'user-selected' 'c-sharp-create' '"):
        selection = event[34:-1]
        if selection == "user-cancelled":
            return
        command = selection.split(" ")[1].lower() 
        path = os.path.dirname(get_caret())[len(os.getcwd())+1:]+os.sep
        print("command|editor user-input c-sharp-create-"+command+" \""+path+"\"")
    elif event.startswith("'user-inputted' 'c-sharp-create-"):
        command = event[32:event.index("'", 32)]
        inputted = event[35+len(command):-1]
        if inputted == "user-cancelled":
            return 
        for line in run_process(["oi", "C#", "new", command, inputted]):
            pass

if __name__ == "__main__":
    args = sys.argv
    if len(args) > 1 and args[1] == 'reactive-script-reacts-to':
        print_react_patterns()
    else:
        handle_event(args[1], args[2], args[3], args[4:])
