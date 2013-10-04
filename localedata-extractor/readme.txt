LOCALDATAEXTRACTOR is a software tool for extracting localization texts from internal Turbine games local data files (LOTRO and DDO games currently).
It's a small Windows console application, requires .NET 4.0 Framework to be installed on your computer.

About localization data:
Localization data contains in game archive client_local_<language_name>.dat. You can unpack this file using DATEXTRACTOR tool.
After unpacking all localization texts can be found in data/client_local_<language_name>\2XXXXXXX\25XXXXXX\250XXXXX folder.
Put LOCALDATAEXTACTOR into this folder and run. When the process will be finished, you'll find new file inside this folder -
LocalData.txt - it's simple text file with a number of localization texts written in the following format:
<unique text number> - <text>

About unique text numbers:
Unique text numbers is a combination of text's filename and it's index inside this file.

About copyrights:
Extractor is absolutely legal software.
All rights to the content of the game goes to Turbine. Real aim for this project is to read localization texts from the game archives.

Contacts with author:
email: dancingonarockhacker@gmail.com

Installation:
Just place LOCALDATAEXTACTOR.exe to data/client_local_<language_name>\2XXXXXXX\25XXXXXX\250XXXXX folder

With best regards, Dancing_on_a_rock_hacker.