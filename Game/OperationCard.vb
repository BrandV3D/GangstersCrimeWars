Public Enum OperationEffect
    GainCash
    DealDamage
    DrawCards
    ForceDiscard
End Enum

' A one-shot job played from hand for an immediate effect.
Public Class OperationCard
    Inherits GameCard

    Public Property Effect As OperationEffect
    Public Property Amount As Integer

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class
