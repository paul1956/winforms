﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports System.IO
Imports System.Windows.Forms
Imports FluentAssertions
Imports Microsoft.VisualBasic.Logging
Imports Xunit

Namespace Microsoft.VisualBasic.Forms.Tests

    Public Class FileLogTraceListenerTests
        Inherits VbFileCleanupTestBase

        Private Shared Function DirectoryHasPermission(DirectoryPath As String) As Boolean
            If String.IsNullOrWhiteSpace(DirectoryPath) Then
                Return False
            End If

            Try
                Dim info As New DirectoryInfo(DirectoryPath)
                If Not info.Exists Then
                    Return False
                End If
                Dim path As String = IO.Path.Combine(DirectoryPath, GetUniqueFileName())
                Dim stream As FileStream = File.Create(path)
                stream.Close()
                File.Delete(path)
            Catch s As Security.SecurityException
                Return False
            End Try
            Return True
        End Function

        <Fact>
        Public Sub ListenerPropertiesTest()
            Dim testDirectory As String = CreateTempDirectory()
            Using listener As New FileLogTraceListener() With {
                .Location = LogFileLocation.Custom,
                .CustomLocation = testDirectory
            }

                listener.BaseFileName.Should.BeEquivalentTo("testHost")

                listener.AutoFlush.Should.BeFalse()
                listener.AutoFlush = True
                listener.AutoFlush.Should.BeTrue()

                listener.IncludeHostName.Should.BeFalse()
                listener.IncludeHostName = True
                listener.IncludeHostName.Should.BeTrue()

                listener.Append.Should.BeTrue()
                listener.Append = False
                listener.Append.Should.BeFalse()

                listener.DiskSpaceExhaustedBehavior.Should.Be(DiskSpaceExhaustedOption.DiscardMessages)
                listener.DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.ThrowException
                listener.DiskSpaceExhaustedBehavior.Should.Be(DiskSpaceExhaustedOption.ThrowException)

                listener.FullLogFileName.Should.BeEquivalentTo(Path.Combine(testDirectory, "testHost.log"))

                listener.LogFileCreationSchedule.Should.Be(LogFileCreationScheduleOption.None)
                listener.LogFileCreationSchedule = LogFileCreationScheduleOption.Daily
                listener.LogFileCreationSchedule.Should.Be(LogFileCreationScheduleOption.Daily)

                listener.MaxFileSize.Should.Be(5_000_000)
                listener.MaxFileSize = 500_000
                listener.MaxFileSize.Should.Be(500_000)

                listener.ReserveDiskSpace.Should.Be(10_000_000)
                listener.ReserveDiskSpace = 1_000_000
                listener.ReserveDiskSpace.Should.Be(1_000_000)

                listener.Delimiter.Should.Be(vbTab)
                listener.Delimiter = ";"
                listener.Delimiter.Should.Be(";")

                listener.Encoding.EncodingName.Should.Be("Unicode (UTF-8)")
                listener.Encoding = Text.Encoding.ASCII
                listener.Encoding.EncodingName.Should.Be("US-ASCII")
            End Using
        End Sub

        <Fact>
        Public Sub LocationPropertyWithCommonApplicationDirectoryLocationTest()
            If DirectoryHasPermission(Application.CommonAppDataPath) Then
                Dim fullLogFileName As String
                Using listener As New FileLogTraceListener() With {
                    .Location = LogFileLocation.CommonApplicationDirectory}

                    fullLogFileName = listener.FullLogFileName
                    fullLogFileName.Should.StartWith(Application.CommonAppDataPath)
                End Using
                File.Delete(fullLogFileName)
            End If
        End Sub

        <Fact>
        Public Sub LocationPropertyWithCustomLocationTest()
            Dim testDirectory As String = CreateTempDirectory()
            Using listener As New FileLogTraceListener() With {
                .Location = LogFileLocation.Custom,
                .CustomLocation = testDirectory}

                listener.Location.Should.Be(LogFileLocation.Custom)
                listener.CustomLocation.Should.Be(testDirectory)
            End Using
        End Sub

        <Fact>
        Public Sub LocationPropertyWithExecutableDirectoryLocationTest()
            If DirectoryHasPermission(Path.GetDirectoryName(Application.ExecutablePath)) Then
                Dim fullLogFileName As String
                Using listener As New FileLogTraceListener() With {
                     .Location = LogFileLocation.ExecutableDirectory}

                    fullLogFileName = listener.FullLogFileName
                    fullLogFileName.Should.StartWith(Path.GetDirectoryName(Application.ExecutablePath))
                End Using
                File.Delete(fullLogFileName)
            End If
        End Sub

        <Fact>
        Public Sub LocationPropertyWithLocalUserApplicationDirectoryLocationTest()
            If DirectoryHasPermission(Application.UserAppDataPath) Then
                Dim fullLogFileName As String
                Using listener As New FileLogTraceListener() With {
                    .Location = LogFileLocation.LocalUserApplicationDirectory}

                    fullLogFileName = listener.FullLogFileName
                    fullLogFileName.Should.StartWith(Application.UserAppDataPath)
                End Using
                File.Delete(fullLogFileName)
            End If
        End Sub

        <Fact>
        Public Sub LocationPropertyWithTempDirectoryLocationTest()
            If DirectoryHasPermission(Application.UserAppDataPath) Then
                Dim fullLogFileName As String

                Using listener As New FileLogTraceListener() With {
                    .Location = LogFileLocation.TempDirectory}

                    fullLogFileName = listener.FullLogFileName
                    fullLogFileName.Should.StartWith(Path.GetTempPath)
                End Using
                File.Delete(fullLogFileName)
            End If
        End Sub

    End Class
End Namespace