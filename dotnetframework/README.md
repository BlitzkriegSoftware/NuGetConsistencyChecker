# NuGetConsistencyChecker #

This recursively searches a tree of files looking for `packages.config` files and then produces a report of the version of each NuGet library that are in use and by what project.

## Usage 

Option 1: Change directories to where you want to start search in

```DOS
NuGetConsistencyChecker.exe
```

Option 2: Specify folder and report name

```DOS
NuGetConsistencyChecker.exe -f {folder to start in} -o {report-file-name}
```

## Also get JSON of all data 

```DOS
NuGetConsistencyChecker.exe -f {folder to start in} -o {report-file-name} -d
```

## Also get Compliance Report 

```DOS
NuGetConsistencyChecker.exe -f {folder to start in} -o {report-file-name} -c
```

## Exit Codes 

Zero (0) is success, not zero = failure
