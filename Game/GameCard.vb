Public MustInherit Class GameCard

    Public Property Name As String
    Public Property Cost As Integer
    Public Property Description As String

    ' Relative path under the Assets folder, e.g. "Cards\Men\men_Hitman.png". Empty for cards with no dedicated art.
    Public Property ImageFile As String

End Class
