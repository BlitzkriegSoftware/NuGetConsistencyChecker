# NuGet Consistency Checker (core) 

This recursively searches a tree of files looking for `*.csproj` files and then produces a report of the version of each NuGet library that are in use and by what project.

## Build a Single File

```dos
C:\code\blitz\NuGetConsistencyChecker\dotnetcore\ngcc2\singlefilebuild.cmd
```

Which will produce an EXE and a PDB of this utility as a single file. Very handy XCOPY deployable.

## Usage 

Option 1: Change directories to where you want to start search in

```DOS
ngcc2
```

> Optionally, you can add `-v` for verbose to trace the search the utility does.

Option 2: Specify folder and report name

```DOS
ngcc2 -f {folder to start in} -o {report-file-name}
```

## Also get JSON of all data 

```DOS
ngcc2 -f {folder to start in} -o {report-file-name} -d
```

## Also get Compliance Report 

```DOS
ngcc2 -f {folder to start in} -o {report-file-name} -c
```

## Exit Codes 

Zero (0) is success, not zero = failure