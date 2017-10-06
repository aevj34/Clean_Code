Public Class WebBrowser

    Public Property Name As BrowserName
    Public Property MajorVersion As Integer

    Public Sub New(name As String, majorVersion As Integer)
        Me.Name = TranslateStringToBrowserName(name)
        Me.MajorVersion = majorVersion
    End Sub

    Private Function TranslateStringToBrowserName(name As String) As BrowserName
        If name.Contains("IE") Then
            Return BrowserName.InternetExplorer
        End If
        'TODO: Add more logic for properly sniffing for other browsers.
        Return BrowserName.Unknown
    End Function

    Public Enum BrowserName
        Unknown
        InternetExplorer
        Firefox
        Chrome
        Opera
        Safari
        Dolphin
        Konqueror
        Linx
    End Enum

End Class