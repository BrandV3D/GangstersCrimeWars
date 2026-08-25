' A recruited crew member fielded to a Boss's crew board.
Public Class CrewCard
    Inherits GameCard

    Public Property Power As Integer
    Public Property Toughness As Integer

    ' False the turn a crew member is recruited (no move that same turn).
    Public Property CanAttack As Boolean = False

    Public ReadOnly Property IsAlive As Boolean
        Get
            Return Toughness > 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{Name} ({Power}/{Toughness})"
    End Function

End Class
