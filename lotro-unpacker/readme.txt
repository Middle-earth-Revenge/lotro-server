DAT_UNPACKER is a software tool for extracting files from Turbine .dat game archives (LOTRO and DDO games currently).
It's a small Windows console application, requires .NET 4.0 Framework to be installed on your computer.

About .dat file format:
Each .dat file is a container with a lot of files inside it. Some of it are compressed, some not.
Unpacker extracts files from the container, also decompresses compressed files.
It also will try to analyze extracting file and detect it's extension for common file formats (music, pictures, etc.).

About file names:
Turbine doesn't keep filenames or filehashes inside containers - they use files by their unique file_id.
File_id (in hex view) contains 8 characters - first 3 are catalogs info and last 5 are real file_id.
Example: file 42065766 - it will be unpacked in 4XXXXXXX\42XXXXXX\420XXXXX\42065766.bin

About file dictionary:
Each file in a container has a version field. Unpacker creates a file dictionary for every container on it's first launch.
After first unpacking, you will see dictionary.bin inside your data\<container_name> folder. It contains binary data (ids and versions).
If you want to follow Turbine changes - do not remove unpacked files. After patching your game run unpacker again.
It will check file versions and unpack only added or modified files. Also it will save information about added and modified files in log report.

About unpacking time:
Unpacking is a long process and duration depends on multiple factors such as number of files inside container, size of each file, speed of your
hdd input/output operations and cpu effeciency. System memory is not included into those factors: unpacker will not exceed 100 mb even for very
huge container.

About copyrights:
Unpacker is absolutely legal software. It uses Turbine C++ written DLL datexport.dll and works as a wrapper for it.
All rights to the content of the game goes to Turbine. Real aim for this project is to unpack some music/graphics/texts from the containers.

Contacts with author:
email: dancingonarockhacker@gmail.com

Installation:
Just unpack DAT_UNPACKER.exe and datexport.dll to your game folder.

Usage: DAT_UNPACKER <.dat file> <flags> <file_id>
.dat file is your game archive with .dat extension
flags - optional parameter for some customization
file_id - optional parameter for extracting only one file (by it's id) from archive. It requires 512 flag (see below).

Flags:
  01 - extract wav files
  02 - extract ogg files
  04 - extract jpeg files
  08 - extract dds files
  16 - extract hks files
  32, 64, 128 are reserved for future common formats
 256 - extract raw Turbine files (extracting with .bin extension by default)
 512 - extract required file (if file_id argument is not empty and such files presents inside container, it will be extracted).
1024 - no extracting any files, just generating filelist for container.
2048 - extracting file as raw ignoring extension detection code.

With best regards, Dancing_on_a_rock_hacker.