﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports System.Collections.ObjectModel
Imports System.Text
Imports FluentAssertions
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.MyServices
Imports Xunit

Namespace Microsoft.VisualBasic.Forms.Tests

    Public Class FileIoProxyTests

        Private ReadOnly _csvSampleData As String =
            "Index,Customer Id,First Name,Last Name,Company,City,Country
1,DD37Cf93aecA6Dc,Sheryl,Baxter,Rasmussen Group,East Leonard,Chile"

        Private ReadOnly _fileSystem As FileSystemProxy = New Devices.ServerComputer().FileSystem

        Private ReadOnly _fixedSampleData As String =
                    "IndexFirstLastCompanyCityCountry
4321;1234;321;654321;123;1234567"

        Private Sub CleanupDirectories(testDirectory As String, Optional destinationDirectory As String = Nothing, Optional onDirectoryNotEmpty As DeleteDirectoryOption = DeleteDirectoryOption.DeleteAllContents)
            If String.IsNullOrEmpty(testDirectory) Then
                Throw New ArgumentException($"'{NameOf(testDirectory)}' cannot be null or empty.", NameOf(testDirectory))
            End If
            If testDirectory = IO.Path.GetTempPath Then
                Throw New ArgumentException($"'{NameOf(testDirectory)}' cannot be {IO.Path.GetTempPath}.", NameOf(testDirectory))
            End If
            If Not testDirectory.StartsWith(IO.Path.GetTempPath, StringComparison.InvariantCultureIgnoreCase) Then
                Throw New ArgumentException($"'{NameOf(testDirectory)}' must start with {IO.Path.GetTempPath}.", NameOf(testDirectory))
            End If

            _fileSystem.DeleteDirectory(testDirectory, onDirectoryNotEmpty)

            If destinationDirectory IsNot Nothing Then
                If destinationDirectory = IO.Path.GetTempPath Then
                    Throw New ArgumentException($"'{NameOf(destinationDirectory)}' cannot be {IO.Path.GetTempPath}.", NameOf(destinationDirectory))
                End If
                If Not destinationDirectory.StartsWith(IO.Path.GetTempPath, StringComparison.InvariantCultureIgnoreCase) Then
                    Throw New ArgumentException($"'{NameOf(destinationDirectory)}' must start with {IO.Path.GetTempPath}.", NameOf(destinationDirectory))
                End If
                _fileSystem.DeleteDirectory(destinationDirectory, onDirectoryNotEmpty)
            End If
        End Sub

        <WinFormsTheory>
        <ClassData(GetType(PathTestData))>
        Public Sub CleanupDirectoriesTests(testDirectory As String)
            Dim testCode As Action = Sub() CleanupDirectories(testDirectory, onDirectoryNotEmpty:=DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
            testCode.Should.Throw(Of ArgumentException)()
        End Sub

        <WinFormsTheory>
        <ClassData(GetType(PathTestData))>
        Public Sub CleanupDirectoriesWithDestinationDirectoryTests(destinationDirectory As String)
            Dim testDirectory As String = CreateTempDirectory()
            Dim testCode As Action = Sub() CleanupDirectories(testDirectory, destinationDirectory, onDirectoryNotEmpty:=DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
            If destinationDirectory IsNot Nothing Then
                testCode.Should.Throw(Of ArgumentException)()
            End If
        End Sub

        <WinFormsFact>
        Public Sub CleanupDirectoryPathTestDataIteratorTests()
            Dim testClass As New PathTestData
            testClass.IEnumerable_GetEnumerator.Should.NotBeNull()
        End Sub

        <WinFormsFact>
        Public Sub CopyDirectoryWithShowUICancelOptionsProxyTest()
            Dim testDirectory As String = CreateTempDirectory(lineNumber:=1)
            Dim file1 As String = CreateTempFile(tmpFilePath:=testDirectory, optionalFilename:=NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim destinationDirectory As String = CreateTempDirectory(lineNumber:=2)
            _fileSystem.CopyDirectory(
                sourceDirectoryName:=testDirectory,
                destinationDirectory,
                showUI:=UIOption.OnlyErrorDialogs,
                onUserCancel:=UICancelOption.DoNothing)
            IO.Directory.Exists(testDirectory).Should.BeTrue()
            IO.Directory.Exists(destinationDirectory).Should.BeTrue()

            Dim count As Integer = IO.Directory.EnumerateFiles(destinationDirectory).Count
            IO.Directory.EnumerateFiles(testDirectory).Count.Should.Be(count)

            CleanupDirectories(testDirectory, destinationDirectory)
        End Sub

        <WinFormsFact>
        Public Sub CopyDirectoryWithShowUIProxyTest()
            Dim testDirectory As String = CreateTempDirectory(lineNumber:=1)
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim destinationDirectory As String = CreateTempDirectory(lineNumber:=2)
            _fileSystem.CopyDirectory(testDirectory, destinationDirectory, UIOption.OnlyErrorDialogs)
            IO.Directory.Exists(testDirectory).Should.BeTrue()
            IO.Directory.Exists(destinationDirectory).Should.BeTrue()
            IO.Directory.EnumerateFiles(destinationDirectory).Count.Should.Be(IO.Directory.EnumerateFiles(testDirectory).Count)

            CleanupDirectories(testDirectory, destinationDirectory)
        End Sub

        <WinFormsFact>
        Public Sub CopyFileProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2))
            _fileSystem.CopyFile(file1, file2)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub CopyFileWithOverwriteProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2), size:=1)
            _fileSystem.CopyFile(file1, file2, overwrite:=True)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub CopyFileWithShowUIProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2))
            _fileSystem.CopyFile(file1, file2, showUI:=UIOption.OnlyErrorDialogs)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub CopyFileWithShowUIWithCancelOptionProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2))
            _fileSystem.CopyFile(file1, file2, UIOption.OnlyErrorDialogs, UICancelOption.DoNothing)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub DeleteDirectoryRecycleWithUICancelOptionsProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1), size:=1)
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)
            IO.File.Exists(file1).Should.BeTrue()

            _fileSystem.DeleteDirectory(testDirectory, showUI:=UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently, UICancelOption.DoNothing)
            IO.Directory.Exists(testDirectory).Should.BeFalse()
        End Sub

        <WinFormsFact>
        Public Sub DeleteDirectoryWithUIProxyRecycleTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1), size:=1)
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)
            IO.File.Exists(file1).Should.BeTrue()

            _fileSystem.DeleteDirectory(
                directory:=testDirectory,
                showUI:=UIOption.OnlyErrorDialogs,
                recycle:=RecycleOption.DeletePermanently)
            IO.Directory.Exists(testDirectory).Should.BeFalse()
        End Sub

        <WinFormsFact>
        Public Sub DeleteFileProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1), size:=1)
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)
            IO.File.Exists(file1).Should.BeTrue()

            _fileSystem.DeleteFile(file1)
            IO.File.Exists(file1).Should.BeFalse()

            _fileSystem.DeleteDirectory(
                directory:=testDirectory,
                onDirectoryNotEmpty:=DeleteDirectoryOption.DeleteAllContents)
        End Sub

        <WinFormsFact>
        Public Sub DeleteFileWithRecycleOptionProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1), size:=1)
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)
            IO.File.Exists(file1).Should.BeTrue()

            _fileSystem.DeleteFile(file1, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently)
            IO.File.Exists(file1).Should.BeFalse()

            _fileSystem.DeleteDirectory(
                directory:=testDirectory,
                onDirectoryNotEmpty:=DeleteDirectoryOption.DeleteAllContents)
        End Sub

        <WinFormsFact>
        Public Sub DeleteFileWithUIProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1), size:=1)
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)
            IO.File.Exists(file1).Should.BeTrue()

            _fileSystem.DeleteFile(file1, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently, UICancelOption.DoNothing)
            IO.File.Exists(file1).Should.BeFalse()

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub DriveProxyTest()
            _fileSystem.Drives.Count.Should.Be(FileIO.FileSystem.Drives.Count)
        End Sub

        <WinFormsFact>
        Public Sub FileNormalizePathEmptyStringTest_Fail()
            Dim testCode As Action = Sub() FileSystemUtils.NormalizePath("")
            testCode.Should.Throw(Of ArgumentException).WithMessage(
                expectedWildcardPattern:="The path is empty. (Parameter 'path')")
        End Sub

        <WinFormsFact>
        Public Sub FileNormalizePathNullTest_Fail()
            Dim testCode As Action = Sub() FileSystemUtils.NormalizePath(Nothing)
            testCode.Should.Throw(Of ArgumentNullException).WithMessage(
                expectedWildcardPattern:="Value cannot be null. (Parameter 'path')")
        End Sub

        <WinFormsFact>
        Public Sub FileNormalizePathTest_Success()
            FileSystemUtils.NormalizePath(IO.Path.GetTempPath).Should.Be(IO.Path.GetTempPath.TrimEnd(IO.Path.DirectorySeparatorChar))
        End Sub

        <WinFormsFact>
        Public Sub FindInFilesProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory, NameOf(fileA))
            _fileSystem.WriteAllText(fileA, "A", append:=False)
            Dim fileB As String = CreateTempFile(testDirectory, NameOf(fileB), size:=1)
            Dim fileC As String = CreateTempFile(testDirectory, NameOf(fileC))
            _fileSystem.WriteAllText(fileC, "C", append:=False)
            Dim filenames As ReadOnlyCollection(Of String) = _fileSystem.FindInFiles(testDirectory, containsText:="A", ignoreCase:=True, SearchOption.SearchTopLevelOnly)
            filenames.Count.Should.Be(1)
            _fileSystem.CombinePath(testDirectory, NameOf(fileA)).Should.Be(filenames(0))
            filenames = _fileSystem.FindInFiles(testDirectory, containsText:="A", ignoreCase:=True, SearchOption.SearchTopLevelOnly, "*C")
            filenames.Count.Should.Be(0)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub MoveDirectoryWithShowUICancelOptionsProxyTest()
            Dim testDirectory As String = CreateTempDirectory(lineNumber:=1)
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim destinationDirectory As String = CreateTempDirectory(lineNumber:=2)
            _fileSystem.MoveDirectory(testDirectory, destinationDirectory, UIOption.OnlyErrorDialogs, UICancelOption.DoNothing)
            IO.Directory.Exists(testDirectory).Should.BeFalse()
            IO.Directory.Exists(destinationDirectory).Should.BeTrue()
            IO.Directory.EnumerateFiles(destinationDirectory).Count.Should.Be(1)

            CleanupDirectories(destinationDirectory)
        End Sub

        <WinFormsFact>
        Public Sub MoveDirectoryWithShowUIProxyTest()
            Dim testDirectory As String = CreateTempDirectory(lineNumber:=1)
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim destinationDirectory As String = CreateTempDirectory(lineNumber:=2)
            _fileSystem.MoveDirectory(testDirectory, destinationDirectory, UIOption.OnlyErrorDialogs)
            IO.Directory.Exists(testDirectory).Should.BeFalse()
            IO.Directory.Exists(destinationDirectory).Should.BeTrue()
            IO.Directory.EnumerateFiles(destinationDirectory).Count.Should.Be(1)

            CleanupDirectories(destinationDirectory)
        End Sub

        <WinFormsFact>
        Public Sub MoveFileWithShowUIProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2))
            _fileSystem.MoveFile(file1, file2, showUI:=UIOption.OnlyErrorDialogs)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            IO.File.Exists(file1).Should.BeFalse()
            IO.File.Exists(file2).Should.BeTrue()
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub MoveFileWithShowUIWithCancelOptionProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim file1 As String = CreateTempFile(testDirectory, NameOf(file1))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(file1, byteArray, append:=False)

            Dim file2 As String = CreateTempFile(testDirectory, NameOf(file2))
            _fileSystem.MoveFile(file1, file2, showUI:=UIOption.OnlyErrorDialogs, UICancelOption.DoNothing)
            Dim bytes As Byte() = _fileSystem.ReadAllBytes(file2)
            IO.File.Exists(file1).Should.BeFalse()
            IO.File.Exists(file2).Should.BeTrue()
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub OpenEncodedTextFileWriterProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory)
            _fileSystem.WriteAllText(fileA, "A", append:=False)
            Using fileWriter As IO.StreamWriter = _fileSystem.OpenTextFileWriter(fileA, append:=False, Encoding.ASCII)
                fileWriter.WriteLine("A")
            End Using
            Using fileReader As IO.StreamReader = _fileSystem.OpenTextFileReader(fileA, Encoding.ASCII)
                Dim text As String = fileReader.ReadLine()
                text.Should.Be("A")
            End Using

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsTheory>
        <InlineData(Nothing)>
        <InlineData(",")>
        Public Sub OpenTextFieldParserProxyTest(delimiter As String)
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileCSV As String = CreateTempFile(testDirectory)
            _fileSystem.WriteAllText(fileCSV, _csvSampleData, append:=False)
            Dim reader As TextFieldParser
            If delimiter Is Nothing Then
                reader = _fileSystem.OpenTextFieldParser(fileCSV)
                reader.TextFieldType = FieldType.Delimited
                reader.Delimiters = {","}
            Else
                reader = _fileSystem.OpenTextFieldParser(fileCSV, delimiter)
            End If
            Dim currentRow As String()
            Dim totalRows As Integer = 0
            While Not reader.EndOfData
                totalRows += 1
                currentRow = reader.ReadFields()
                Dim currentField As String
                Dim totalFields As Integer = 0
                For Each currentField In currentRow
                    totalFields += 1
                Next
                totalFields.Should.Be(7)
            End While
            totalRows.Should.Be(2)
            reader.Close()

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub OpenTextFileWriterProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory)
            _fileSystem.WriteAllText(fileA, "A", append:=False)
            Using fileWriter As IO.StreamWriter = _fileSystem.OpenTextFileWriter(fileA, append:=False)
                fileWriter.WriteLine("A")
            End Using
            Using fileReader As IO.StreamReader = _fileSystem.OpenTextFileReader(fileA, Encoding.ASCII)
                Dim text As String = fileReader.ReadLine()
                text.Should.Be("A")
            End Using

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub OpenTextFixedFieldParserProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileCSV As String = CreateTempFile(testDirectory)
            _fileSystem.WriteAllText(fileCSV, _fixedSampleData, append:=False)
            Dim reader As TextFieldParser = _fileSystem.OpenTextFieldParser(fileCSV, 5, 5, 4, 7, 4, 7)
            Dim currentRow As String()
            Dim totalRows As Integer = 0
            Dim splitData As String() = _fixedSampleData.Split(vbCrLf)(1).Split(";")
            While Not reader.EndOfData
                currentRow = reader.ReadFields()
                Dim currentField As String
                Dim totalFields As Integer = 0
                For Each currentField In currentRow
                    If totalRows = 1 Then
                        splitData(totalFields).Should.Be(currentField.TrimEnd(";"c))
                    End If
                    totalFields += 1
                Next
                totalFields.Should.Be(6)
                totalRows += 1
            End While
            totalRows.Should.Be(2)
            reader.Close()

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub OpenTextStreamParserProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory)
            _fileSystem.WriteAllText(fileA, "A", append:=False)
            Using fileReader As IO.StreamReader = _fileSystem.OpenTextFileReader(fileA)
                Dim text As String = fileReader.ReadLine()
                text.Should.Be("A")
            End Using

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub ReadAllBytesProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory, NameOf(fileA))
            Dim byteArray As Byte() = {4}
            _fileSystem.WriteAllBytes(fileA, byteArray, append:=False)

            Dim bytes As Byte() = _fileSystem.ReadAllBytes(fileA)
            bytes.Length.Should.Be(1)
            bytes(0).Should.Be(4)

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub ReadAllTextProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory, NameOf(fileA))
            _fileSystem.WriteAllText(fileA, "A", append:=False)
            Dim text As String = _fileSystem.ReadAllText(fileA)
            text.Should.Be("A")

            CleanupDirectories(testDirectory)
        End Sub

        <WinFormsFact>
        Public Sub ReadAllTextWithEncodingProxyTest()
            Dim testDirectory As String = CreateTempDirectory()
            Dim fileA As String = CreateTempFile(testDirectory, NameOf(fileA))
            _fileSystem.WriteAllText(fileA, "A", append:=False, Encoding.UTF8)
            Dim text As String = _fileSystem.ReadAllText(fileA, Encoding.UTF8)
            text.Should.Be("A")

            CleanupDirectories(testDirectory)
        End Sub

        Protected Friend Class PathTestData
            Implements IEnumerable(Of Object())

            Public Iterator Function GetEnumerator() As IEnumerator(Of Object()) Implements IEnumerable(Of Object()).GetEnumerator
                Yield {Nothing}
                Yield {String.Empty}
                Yield {" "}
                Yield {IO.Path.GetPathRoot(IO.Path.GetTempPath)}
                Yield {IO.Path.GetTempPath}
            End Function

            Public Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function

        End Class
    End Class
End Namespace