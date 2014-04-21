#!/usr/bin/env python
import sys, time, tempfile

def print_react_patterns():
    # Write one event pr line that this script will react to
    print("'tamper-at-caret' 'C#-add-using'")
    print("'tamper-at-caret' 'C#-reorder-usings'")
    print("'user-selected' 'C#-add-using' *")

def run_command(command):
    print(command)
    sys.stdout.flush()
    output = []
    while True:
        line = sys.stdin.readline()
        if line == "end-of-conversation\n":
            break
        if len(line.strip()) > 0:
            output.append(line.strip("\n"))
    return output

def filter_and_format_signatures(signature_output):
    signatures = []
    for line in signature_output:
        if line.startswith("C#|signature|"):
            chunks = line.split("|")
            signatures.append({"ns": chunks[2], "type": chunks[3]})
    return signatures

def collect_usings(caret):
    usings = []
    line_nr = 1
    first_using = -1
    last_using = -1
    for line in caret[1:]:
        stripped = line.strip()
        if stripped.startswith("namespace "):
            break
        if stripped.startswith("using "):
            using = stripped[6:len(stripped)-1]
            sortname = using
            if sortname.startswith("System.") or sortname == "System":
                sortname = " " + sortname
            usings.append({"sortname": sortname, "name": using})
            if first_using == -1:
                first_using = line_nr
            last_using = line_nr
        line_nr+=1
    return usings, first_using, last_using

def contians(list, matcher):
    for item in list:
        if matcher(item):
            return True
    return False

def add_using(caret, ns):
    usings, start_line, end_line = collect_usings(caret)
    if contians(usings, lambda itm: itm['name'] == ns) == True:
        return
    usings.append({"sortname": ns, "name": ns})
    usings_ordered = sorted(usings, key=lambda u: u['sortname'])
    file = tempfile.NamedTemporaryFile(delete = False)
    try:
        for using in usings_ordered:
            file.write("using " + using['name'] + ";\n")
        file.close()
        current_file = caret[0].split("|")[0]
        print("command|editor replace \""+file.name+"\" \""+current_file+"|"+str(start_line)+"|1\" \""+str(end_line)+"|"+str(len(caret[end_line])+1)+"\"")
    except Exception as e:
        print(e)

def reorder_usings(caret):
    usings, start_line, end_line = collect_usings(caret)
    usings_ordered = sorted(usings, key=lambda u: u['sortname'])
    file = tempfile.NamedTemporaryFile(delete = False)
    try:
        for using in usings_ordered:
            file.write("using " + using['name'] + ";\n")
        file.close()
        current_file = caret[0].split("|")[0]
        print("command|editor replace \""+file.name+"\" \""+current_file+"|"+str(start_line)+"|1\" \""+str(end_line)+"|"+str(len(caret[end_line])+1)+"\"")
    except Exception as e:
        print(e)
    
def handle_event(event, global_profile, local_profile, args):
    if (event.startswith("'user-selected' ")):
        type = event[32:len(event)-1]
        signature_output = run_command("request|codemodel get-signatures signature=" + type)
        signatures = filter_and_format_signatures(signature_output)
        if len(signatures) != 1:
            return
        caret = run_command("request|editor get-caret")
        add_using(caret, signatures[0]['ns'])
        return
    if event == "'tamper-at-caret' 'C#-add-using'":
        caret = run_command("request|editor get-caret")
        word = run_command("request|codemodel C# query get-word \"" + caret[0] + "\"")
        if len(word) == 0:
            return
        signature_output = run_command("request|codemodel get-signatures name=" + word[0])
        signatures = filter_and_format_signatures(signature_output)
        if len(signatures) == 0:
            return
        if len(signatures) == 1:
            add_using(caret, signatures[0]['ns'])
            return
        options = ""
        for signature in signatures:
            if options != "":
                options += ","
            options += signature['type']
        print("command|editor user-select C#-add-using \"" + options + "\"")

    if event == "'tamper-at-caret' 'C#-reorder-usings'":
        caret = run_command("request|editor get-caret")
        reorder_usings(caret)

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
