# Project 1: BATCH RENAME
## Member
**Họ và tên:** Phan Huy Trường\
**MSSV:** 20120231\
**Email:** 20120231@student.hcmus.edu.vn
## Notice:
> * All dll rules is stored in "Rules" folder which is in the same directory with execution file ".exe"
* Libraries used in this project:
    * Fluent.Ribbon (9.0.4) 
PM> NuGet\Install-Package Fluent.Ribbon
    * MaterialDesignTheme (4.6.1)
PM> NuGet\Install-Package MaterialDesignThemes
    * Newtonsoft.Json (13.0.2)
PM> NuGet\Install-Package Newtonsoft.Json
    * PropertyChanged.Fody (4.1.0)
PM> NuGet\Install-Package PropertyChanged.Fody
## Core Requirements Achieved
1. Dynamically load all renaming rules from external DLL files
2. Select all files and folders you want to rename
3. Create a set of rules for renaming the files. 
    - Each rule can be added from a menu 
    - Each rule's parameters can be edited 
4. Apply the set of rules in numerical order to each file, make them have a new name
5. Save this set of rules into presets for quickly loading later if you need to reuse

## Renaming Rules
1. Change the extension to another extension (no conversion, force renaming extension)
2. Add counter to the end of the file
    - Could specify the start value, steps, number of digits (Could have padding like 01, 02, 03...10....99)
3. Remove all space from the beginning and the ending of the filename
4. Replace certain characters into one character like replacing "-" ad "_" into space " "
    - Could be the other way like replace all space " " into dot "."
5. Adding a prefix to all the files
6. Adding a suffix to all the files
7. Convert all characters to lowercase, remove all spaces
8. Convert filename to PascalCase

## Improvements/Bonus
1. Drag & Drop a file to add to the list
2. Storing parameters for renaming using both txt and json file
3. Preset using both txt and json file can be read
4. Adding recursively: just specify a folder only, the application will automatically scan and add all the files inside
5. Handling file/folder duplication when adding them into items list
6. Last time state: When exiting the application, auto-save and load the 
    1. Last file list
    2. Last chosen preset
7. Autosave & load the current working condition to prevent sudden power loss
    1. The current file list
    2. The current set of renaming rules, together with the parameters
8. Using regular expressions when handling exception character for filename
9. Checking exceptions when editing rules: 
    - Characters that cannot be in the file name: first 32 characters in ASCII table and < > : " / \ | ? *
    - the maximum length of the filename cannot exceed 255 characters
10. Save and load your work into a project using json. The project file is saved with ".proj" extension. Application has 2 options for saving: "save" and "save as"
11. Preset saving has 2 options for saving: "save" and "save as"
12. Let the user see the preview of the result
13. Create a copy of all the files and move them to a selected folder rather than perform the renaming on the original file
14. Handling conflict when rename a file or folder to an already exist filename or foldername. The errors like that will be displayed in errors field next to path field.
15. Have some button handling changing the order of rules and files/folders
16. Parameters's edit dialog is auto generated. Meaning its edit fields base on parameters of chosen rule.
17. New file button to quickly start a new project

## Video demo link: 
https://youtu.be/sYGlfdOT6H0
