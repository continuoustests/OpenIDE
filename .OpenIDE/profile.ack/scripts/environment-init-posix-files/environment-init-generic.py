#!/usr/bin/env python
import os,sys,subprocess

def run_process(exe,working_dir=""):
    if working_dir == "":
        working_dir = os.getcwd()
    p = subprocess.Popen(exe, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd=working_dir)
    lines = []
    while(True):
        retcode = p.poll() # returns None while subprocess is running
        line = p.stdout.readline().decode().strip('\n')
        lines.append(line)
        if(retcode is not None):
            break
    return lines

def to_single_line(exe,working_dir=""):
    lines = run_process(exe, working_dir)
    if len(lines) == 0:
        return None
    return lines[0]

def run_cmd(command):
    print(command)

def run_command():
    if os.name != "nt":
        run_cmd("oi conf interpreter.exe=mono -g")

    print("Choose editor (sublime,vim):")
    sys.stdout.flush()
    editor = sys.stdin.readline().strip("\n")

    if editor == "sublime":
        if os.name != "nt":
            has_subl = to_single_line(["which", "subl"])
            if has_subl != "subl not found":
                run_cmd("oi conf editor.sublime.executable=subl -g")

    if editor == "vim":
        if os.name != "nt":
            has_subl = to_single_line(["which", "gvim"])
            if has_subl != "gvim not found":
                run_cmd("oi conf editor.vim.executable=gvim -g")

    if os.name == "mac":
        run_cmd("oi conf oi.fallbackmode=disabled -g")

if __name__ == "__main__":
    run_command()
