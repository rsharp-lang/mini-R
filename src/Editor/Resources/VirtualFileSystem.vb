﻿Imports System.IO
Imports System.Resources
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip

Public Module VirtualFileSystem

    Const baseName = "vscode.vscode"
    Const resourceName = "vscode"

    Public Function GetHttpResourcesProvider() As IFileSystemEnvironment
        Dim resourceManager As New ResourceManager(baseName, GetType(VirtualFileSystem).Assembly)
        Dim buffer As Byte() = resourceManager.GetObject(resourceName)
        Dim zip As New MemoryStream(buffer)
        Dim fs As New ZipStream(zip, is_readonly:=True)

        Return fs
    End Function

End Module
