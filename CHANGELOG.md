Change log
==========

## v1.0.0 Alpha 6 - January 13, 2019
 * An attempt was made to prevent simultaneous writing to the output file

## v1.0.0 Alpha 5 - November 29, 2017
 * Fixed a error “Detected package downgrade: NETStandard.Library from 1.6.1 to 1.6.0”

## v1.0.0 Alpha 4 - November 29, 2017
 * Fixed a error, that occurred during processing paths in Unix-like operating systems
 * Added a ability to specify the namespace and set the internal access modifier for the resource classes
 * In ResxToCs.MSBuild module the `ResourceDirectory` property of task was renamed to the `InputDirectory` property

## v1.0.0 Alpha 3 - November 21, 2017
 * Improved a performance

## v1.0.0 Alpha 2 - November 6, 2017
 * From the dotnet-resx2cs module was extracted a basic logic and moved to the ResxToCs.Core module
 * Created the ResxToCs.MSBuild module, that contains MSBuild task for conversion the `.resx` files into the `.Designer.cs` files

## v1.0.0 Alpha 1 - October 23, 2017
 * Initial version uploaded