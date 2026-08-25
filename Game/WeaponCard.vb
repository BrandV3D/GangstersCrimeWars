' A Gun: played from hand to permanently arm one of your own living Crew.
Public Class WeaponCard
    Inherits GameCard

    Public Property PowerBoost As Integer

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class
