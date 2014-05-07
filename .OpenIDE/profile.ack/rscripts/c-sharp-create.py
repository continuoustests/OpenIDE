#!/usr/bin/env python
import os, sys, subprocess

def print_react_patterns():
    print("'.cs' 'command' 'c-sharp-create'")
    print("'.csproj' 'command' 'c-sharp-create'")
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

def find_closest_project_root_position(directory):
    if directory == None:
        return None
    if os.path.isdir(directory) == False:
        return find_closest_project_root_position(os.path.dirname(directory))
    for file in os.listdir(directory):
        if file.endswith(".csproj"):
            return directory
    return find_closest_project_root_position(os.path.dirname(directory))

def get_suggested_path(main_command):
    filename = get_caret()
    if main_command == "create":
        closest_project = find_closest_project_root_position(filename)
        if closest_project == None:
            return ""
        return os.path.dirname(os.path.dirname(closest_project))[len(os.getcwd())+1:]+os.sep
    return os.path.dirname(filename)[len(os.getcwd())+1:]+os.sep

def handle_event(event, global_profile, local_profile, args):
    if event == "'.cs' 'command' 'c-sharp-create'":
        output_create = to_single_line(["oi", "get-commands", "C#", "create"]).strip().split(" ")
        output_new = to_single_line(["oi", "get-commands", "C#", "new"]).strip().split(" ")
        option_string_create = (''.join(map(lambda x: "Create "+x.title()+',', output_create))).strip(",")
        option_string_new = (''.join(map(lambda x: "New "+x.title()+',', output_new))).strip(",")
        option_string = option_string_create+","+option_string_new
        print("command|editor user-select c-sharp-create \""+option_string+"\"")
    elif event.startswith("'user-selected' 'c-sharp-create' '"):
        selection = event[34:-1]
        if selection == "user-cancelled":
            return 
        identifier = selection.replace(" ", "|").lower()
        path = get_suggested_path(identifier.split("|")[0])
        print("command|editor user-input c-sharp-create-"+identifier+" \""+path+"\"")
    elif event.startswith("'user-inputted' 'c-sharp-create-"):
        token = "'user-inputted' 'c-sharp-create-"
        selection = event[len(token):event.index("'", len(token))]
        chunks = selection.split("|")
        main_command = chunks[0].lower() 
        command = chunks[1].lower() 
        inputted = event[len(token)+len(selection)+3:-1]
        if inputted == "user-cancelled":
            return 
        for line in run_process(["oi", "C#", main_command, command, inputted]):
            pass

if __name__ == "__main__":
    args = sys.argv
    if len(args) > 1 and args[1] == 'reactive-script-reacts-to':
        print_react_patterns()
    else:
        handle_event(args[1], args[2], args[3], args[4:])
