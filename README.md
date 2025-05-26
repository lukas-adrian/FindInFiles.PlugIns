## Project SearchPlugIns

The PlugIns are for the FindInFiles-Project, see [here](https://github.com/lukas-adrian/FindInFiles.git)

Use the PlugIn-Project TextFilesKMP as an example if you want to create own PlugIns.

To create some plugin just reference PlugInBase and inherit from ISearchInFolderPlugIn.

      public Task<List<FileSearchEventArgs>> SearchInFolder(
         List<String> lstAllFiles,
         String searchTerm,
         bool matchCase,
         bool wholeWord,
         IProgress<Int32> progress,
         CancellationToken cancellationToken);

* path, is the folder path where you will search for the files
* extension, extension of the files. Just one extension because in the application is a loop for multiple extensions
* searchTerm, some search term
* matchCase, match case only
* wholeWord, search for the whole word only
* progress, is for the waiting bar
* cancellationToken, for cancelling

## Additional projects
See in the ProjectFolder

## ToDo

* ~~PlugIns including PreviewWindow like TextFiles + TextPreViewWindow, PDF + PDF PreViewWindow~~
* searching in multiple folder
* ~~add optional page number and not only row number for PDFs~~
* ~~add more PlugIns like PDF, Office Documents, etc~~

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for more details.
