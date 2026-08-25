' A Building: played to a Boss's Rackets board, passively generating Cash every turn.
Public Class RacketCard
    Inherits GameCard

    Public Property Income As Integer

    ' Which of the Boss's territory plots this building occupies (-1 = not yet built).
    Public Property SlotIndex As Integer = -1

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class
