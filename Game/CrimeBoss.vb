Public Class CrimeBoss

    Public Const MaxCrewSlots As Integer = 5
    Public Const MaxInventorySlots As Integer = 6
    Public Const MaxRacketSlots As Integer = 6
    Public Const MaxJobHandSize As Integer = 3
    Public Const StartingActionPoints As Integer = 3

    Public Property Name As String
    Public Property PortraitImageFile As String ' iconic name banner
    Public Property BossArtImageFile As String ' character portrait art
    Public Property Life As Integer = 20
    Public Property Cash As Integer = 8
    Public Property ActionPoints As Integer = StartingActionPoints

    Public Property Crew As New List(Of CrewCard)
    Public Property Inventory As New List(Of WeaponCard)
    Public Property Rackets As New List(Of RacketCard)

    Public Property JobDeck As New List(Of OperationCard)
    Public Property JobHand As New List(Of OperationCard)
    Public Property JobDiscard As New List(Of OperationCard)

    Public Sub New(name As String)
        Me.Name = name
        JobDeck = CardLibrary.BuildJobDeck()
        CardLibrary.ShuffleJobs(JobDeck)
    End Sub

    Public ReadOnly Property RacketIncome As Integer
        Get
            Dim total As Integer = 0
            For Each racket As RacketCard In Rackets
                total += racket.Income
            Next
            Return total
        End Get
    End Property

    Public Sub DrawJobCard()
        If JobDeck.Count = 0 OrElse JobHand.Count >= MaxJobHandSize Then
            Return
        End If

        Dim top As OperationCard = JobDeck(0)
        JobDeck.RemoveAt(0)
        JobHand.Add(top)
    End Sub

End Class
