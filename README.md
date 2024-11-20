## Project SearchPlugIns

The PlugIns are for the FindInFiles-Project, see [here](https://github.com/lukas-adrian/FindInFiles.git)

Use the PlugIn-Project TextFilesKMP as an example if you want to create own PlugIns.

To create some plugin just reference PlugInBase and inherit from ISearchInFolderPlugIn.

      public async Task<List<FileSearchEventArgs>> SearchInFolder(
         String path,
         String extension,
         String searchTerm,
         Boolean subDirs,
         Int32 minFileSizeMB,
         Int32 maxFileSizeMB,
         IProgress<Int32> progress,
         CancellationToken cancellationToken)

* path, is the folder path where you will search for the files
* extension, extension of the files. Just one extension because in the application is a loop for multiple extensions
* searchTerm, some text
* subDirs, ture if search also in subdirs
* minFileSizeMB, it can be 0 if every file will be allowed. Idea was to limit the results
* maxFileSizeMB, it can be 0 if every file will be allowed. If that value is 0 minFileSizeMB will be also 0
* progress, is for the waiting bar
* cancellationToken, for cancelling

### Extensions

I am using Extensions like txt, xm*, etc.\
I don't use extension like *.txt, just txt because I don't see any use of it. If there is an use to search for files like *test.txt or something like that I will think about it. But I never experienced it

This is how the PlugIn is reading the files:\
`Directory.EnumerateFiles(path, $"*.{extension}", SearchOption.AllDirectories)`

## ToDo

* PlugIns including PreViewWindow like TextFiles + TextPreViewWindow, PDF + PDF PreViewWindow
* searching in multiple folder
* add optional page number and not only row number for PDFs
* add more PlugIns like PDF, Office Documents, etc

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for more details.