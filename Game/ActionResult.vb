Public Class ActionResult

    Public Property Success As Boolean
    Public Property Message As String

    Public Sub New(success As Boolean, message As String)
        Me.Success = success
        Me.Message = message
    End Sub

End Class
