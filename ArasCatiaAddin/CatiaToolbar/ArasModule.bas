Attribute VB_Name = "ArasModule"
' Aras Innovator Integration for CATIA V5
' Import this module into CATIA VBA Editor

Const ARASCLI_PATH As String = "D:\Worklab\SAP\Behran\PLM\Codes\v2\aras-innovator-cli\ArasCLI\bin\Debug\ArasCLI.exe"

Public Sub ArasLogin()
    Dim fso As Object, shell As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "Aras Login"
        Exit Sub
    End If

    Set shell = CreateObject("WScript.Shell")
    shell.Run """" & ARASCLI_PATH & """ --catia-login", 1, False
    Set shell = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasCheckIn()
    Dim fso As Object, shell As Object, doc As Document, docPath As String
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "Check In"
        Exit Sub
    End If

    If CATIA.Documents.Count = 0 Then
        MsgBox "No document is open in CATIA.", vbExclamation, "Check In"
        Exit Sub
    End If

    Set doc = CATIA.ActiveDocument

    If doc.Saved = False Then
        Dim result As VbMsgBoxResult
        result = MsgBox("Document has unsaved changes. Save before check-in?", vbYesNoCancel + vbQuestion, "Check In")
        If result = vbCancel Then Exit Sub
        If result = vbYes Then doc.Save
    End If

    docPath = doc.FullName
    If docPath = "" Then
        MsgBox "Please save the document first.", vbExclamation, "Check In"
        Exit Sub
    End If

    Set shell = CreateObject("WScript.Shell")
    shell.Run """" & ARASCLI_PATH & """ """ & docPath & """", 1, False
    Set shell = Nothing
    Set doc = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasCheckOut()
    Dim fso As Object, shell As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "Check Out"
        Exit Sub
    End If

    Set shell = CreateObject("WScript.Shell")
    shell.Run """" & ARASCLI_PATH & """ --catia-checkout", 1, False
    Set shell = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasGetLatest()
    Dim fso As Object, shell As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "Get Latest"
        Exit Sub
    End If

    Set shell = CreateObject("WScript.Shell")
    shell.Run """" & ARASCLI_PATH & """", 1, False
    Set shell = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasSearch()
    Dim fso As Object, shell As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "Search"
        Exit Sub
    End If

    Set shell = CreateObject("WScript.Shell")
    shell.Run """" & ARASCLI_PATH & """", 1, False
    Set shell = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasBOMSync()
    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If Not fso.FileExists(ARASCLI_PATH) Then
        MsgBox "ArasCLI not found at:" & vbCrLf & ARASCLI_PATH, vbExclamation, "BOM Sync"
        Exit Sub
    End If

    If CATIA.Documents.Count = 0 Then
        MsgBox "No document is open in CATIA.", vbExclamation, "BOM Sync"
        Exit Sub
    End If

    Dim doc As Document
    Set doc = CATIA.ActiveDocument

    If TypeName(doc) <> "ProductDocument" Then
        MsgBox "BOM Sync requires a CATProduct (assembly)." & vbCrLf & vbCrLf & _
               "Current document type: " & TypeName(doc), vbExclamation, "BOM Sync"
        Exit Sub
    End If

    MsgBox "BOM Sync for: " & doc.Product.PartNumber & vbCrLf & vbCrLf & _
           "Feature coming soon.", vbInformation, "BOM Sync"

    Set doc = Nothing
    Set fso = Nothing
End Sub

Public Sub ArasSettings()
    Dim fso As Object, msg As String
    Set fso = CreateObject("Scripting.FileSystemObject")

    msg = "Aras Integration Settings" & vbCrLf & vbCrLf
    msg = msg & "ArasCLI Path:" & vbCrLf
    msg = msg & ARASCLI_PATH & vbCrLf & vbCrLf

    If fso.FileExists(ARASCLI_PATH) Then
        msg = msg & "Status: ArasCLI Found"
    Else
        msg = msg & "Status: ArasCLI NOT Found!"
    End If

    MsgBox msg, vbInformation, "Aras Settings"
    Set fso = Nothing
End Sub
