Public Class Session

    Public Property Title As String
    Public Property Description As String
    Public Property Approved As Boolean

    Public Sub New(title As String, description As String)
        Me.Title = title
        Me.Description = description
    End Sub

End Class
