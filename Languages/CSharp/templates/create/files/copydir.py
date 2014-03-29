import os 
import sys
import uuid

def copyFile(projectName, replacements, source, destination):
    if os.path.basename(source) == "project.file":
        destination = os.path.join(os.path.dirname(destination), projectName + ".csproj")
    f1 = open(source, 'r')
    f2 = open(destination, 'w')
    for line in f1:
        newLine = line
        for replacement in replacements:
            newLine = newLine.replace(replacement, replacements[replacement])
        newLine = newLine.replace("{NEW_GUID}", "{" + str(uuid.uuid1()) + "}")
        newLine = newLine.replace("{NEW_NOBRACES_GUID}", str(uuid.uuid1()))
        f2.write(newLine)
    f1.close()
    f2.close()

def recurseDir(projectName, replacements, source, destination):
    if os.path.isdir(destination) == False:
        os.makedirs(destination)
    for file in os.listdir(source):
        if os.path.isdir(os.path.join(source, file)):
            if file == "bin":
                continue
            recurseDir(projectName, replacements, os.path.join(source, file), os.path.join(destination, file))
        else:
            copyFile(projectName, replacements, os.path.join(source, file), os.path.join(destination, file))

def copy(projecttype, filepath):
    if filepath.lower().endswith(".csproj") == False:
        filepath = filepath + ".csproj"
    directory = os.path.dirname(filepath)
    projectName = os.path.splitext(os.path.basename(filepath))[0]
    projDir = os.path.join(os.path.dirname(__file__),projecttype)
    projectGUID = "{" + str(uuid.uuid1()) + "}"
    replacements = { '{PROJECT_NAME}': projectName, '{PROJECT_GUID}': projectGUID }
    recurseDir(projectName, replacements, projDir, directory)
    