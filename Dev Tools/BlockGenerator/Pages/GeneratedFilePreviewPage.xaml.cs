using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Rock;

namespace BlockGenerator.Pages
{
    /// <summary>
    /// Interaction logic for GeneratedFilePreview.xaml
    /// </summary>
    public partial class GeneratedFilePreviewPage : Page
    {
        private readonly List<ExportFile> _exportFiles;

        public GeneratedFilePreviewPage( IList<GeneratedFile> files )
        {
            InitializeComponent();

            _exportFiles = files.Select( f => new ExportFile( f ) ).ToList();

            _exportFiles.ForEach( UpdateExportFile );

            _exportFiles = _exportFiles.OrderByDescending( f => f.IsWriteNeeded )
                .ThenBy( f => f.File.FileName )
                .ToList();

            FileListBox.ItemsSource = _exportFiles;
        }

        private static void UpdateExportFile( ExportFile file )
        {
            var solutionPath = GetSolutionPath();

            // Check if the file needs to be written.
            if ( solutionPath.IsNotNullOrWhiteSpace() )
            {
                var filePath = Path.Combine( solutionPath, file.File.SolutionRelativePath );

                if ( File.Exists( filePath ) )
                {
                    var fileContents = File.ReadAllText( filePath );

                    file.IsWriteNeeded = fileContents != file.File.Content;
                }
                else
                {
                    file.IsWriteNeeded = true;
                }
            }
            else
            {
                file.IsWriteNeeded = true;
            }

            file.IsUpToDate = !file.IsWriteNeeded;
            file.IsExporting = !file.IsUpToDate;
        }

        private static string GetSolutionPath()
        {
            var directoryInfo = new DirectoryInfo( Directory.GetCurrentDirectory() );

            while ( directoryInfo != null )
            {
                if ( File.Exists( Path.Combine( directoryInfo.FullName, "Rock.sln" ) ) )
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }

        private static void EnsureDirectoryExists( string path )
        {
            var directories = new Stack<DirectoryInfo>();

            var directory = new DirectoryInfo( path );

            while ( directory != null && !directory.Exists )
            {
                directories.Push( directory );
                directory = directory.Parent;
            }

            while ( directories.Count > 0 )
            {
                directory = directories.Pop();
                directory.Create();
            }
        }

        private void Save_Click( object sender, RoutedEventArgs e )
        {
            var solutionPath = GetSolutionPath();

            if ( solutionPath.IsNullOrWhiteSpace() )
            {
                MessageBox.Show( Window.GetWindow( this ), "Unable to determine solution path." );
                return;
            }

            var files = _exportFiles
                .Where( f => f.IsExporting )
                .Select( f => f.File )
                .ToList();

            var failedFiles = new List<GeneratedFile>();

            foreach ( var file in files )
            {
                try
                {
                    var filePath = Path.Combine( solutionPath, file.SolutionRelativePath );

                    EnsureDirectoryExists( Path.GetDirectoryName( filePath ) );

                    File.WriteAllText( filePath, file.Content );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"Error processing file '{file.SolutionRelativePath}': {ex.Message}" );
                    failedFiles.Add( file );
                }
            }

            if ( failedFiles.Any() )
            {
                var errorMessage = $"The following files had errors:\n{string.Join( "\n", failedFiles.Select( f => f.SolutionRelativePath ) )}";
                MessageBox.Show( Window.GetWindow( this ), errorMessage, "Some files failed to process." );
            }
            else
            {
                MessageBox.Show( "Selected files have been created or updated." );
            }
        }

        private void FileListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( FileListBox.SelectedItem is ExportFile exportFile )
            {
                FilePreviewContent.Text = exportFile.File.Content;
                FilePreviewContent.ScrollToHome();
                FilePreviewPath.Text = $"Path: {exportFile.File.SolutionRelativePath}";
            }
            else
            {
                FilePreviewContent.Text = string.Empty;
                FilePreviewPath.Text = string.Empty;
            }
        }

        private class ExportFile : IComparable
        {
            public bool IsExporting { get; set; }

            public bool IsWriteNeeded { get; set; }

            public bool IsUpToDate { get; set; }

            public GeneratedFile File { get; set; }

            public ExportFile( GeneratedFile file )
            {
                IsExporting = true;
                File = file;
            }

            public int CompareTo( object obj )
            {
                return File.FileName.CompareTo( obj );
            }
        }
    }
}
