Public Class GameEngine

    Public Property Player1 As CrimeBoss
    Public Property Player2 As CrimeBoss
    Public Property ActivePlayerIndex As Integer = 0
    Public Property TurnNumber As Integer = 1

    Public Sub New()
        Dim bosses = BossRoster.PickTwoDistinct()

        Player1 = New CrimeBoss(bosses.Item1.Name) With {.PortraitImageFile = bosses.Item1.BannerImageFile, .BossArtImageFile = bosses.Item1.ArtImageFile}
        Player2 = New CrimeBoss(bosses.Item2.Name) With {.PortraitImageFile = bosses.Item2.BannerImageFile, .BossArtImageFile = bosses.Item2.ArtImageFile}

        For i As Integer = 1 To 3
            Player1.DrawJobCard()
            Player2.DrawJobCard()
        Next

        BeginTurn()
    End Sub

    Public ReadOnly Property ActivePlayer As CrimeBoss
        Get
            Return If(ActivePlayerIndex = 0, Player1, Player2)
        End Get
    End Property

    Public ReadOnly Property WaitingPlayer As CrimeBoss
        Get
            Return If(ActivePlayerIndex = 0, Player2, Player1)
        End Get
    End Property

    Public ReadOnly Property IsGameOver As Boolean
        Get
            Return Player1.Life <= 0 OrElse Player2.Life <= 0
        End Get
    End Property

    Public ReadOnly Property Winner As CrimeBoss
        Get
            If Not IsGameOver Then
                Return Nothing
            End If
            Return If(Player1.Life <= 0, Player2, Player1)
        End Get
    End Property

    Private Sub BeginTurn()
        Dim boss As CrimeBoss = ActivePlayer

        boss.Cash += 1 + boss.RacketIncome
        boss.ActionPoints = CrimeBoss.StartingActionPoints
        boss.DrawJobCard()
    End Sub

    Public Function EndTurn() As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        ActivePlayerIndex = If(ActivePlayerIndex = 0, 1, 0)
        TurnNumber += 1
        BeginTurn()

        Return New ActionResult(True, $"Turn passed to {ActivePlayer.Name}.")
    End Function

    Public Function RecruitCrew(template As CrewCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If template.Cost > boss.Cash Then
            Return New ActionResult(False, $"Not enough cash to recruit {template.Name}.")
        End If
        If boss.Crew.Count >= CrimeBoss.MaxCrewSlots Then
            Return New ActionResult(False, "The crew's already full.")
        End If

        Dim recruit As New CrewCard With {
            .Name = template.Name, .Cost = template.Cost, .Description = template.Description, .ImageFile = template.ImageFile,
            .Power = template.Power, .Toughness = template.Toughness
        }

        boss.Cash -= template.Cost
        boss.ActionPoints -= 1
        boss.Crew.Add(recruit)

        Return New ActionResult(True, $"{boss.Name} recruits {recruit.Name} ({recruit.Power}/{recruit.Toughness}).")
    End Function

    Public Function BuyWeapon(template As WeaponCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If template.Cost > boss.Cash Then
            Return New ActionResult(False, $"Not enough cash for a {template.Name}.")
        End If
        If boss.Inventory.Count >= CrimeBoss.MaxInventorySlots Then
            Return New ActionResult(False, "No room left in the inventory.")
        End If

        Dim weapon As New WeaponCard With {
            .Name = template.Name, .Cost = template.Cost, .Description = template.Description, .ImageFile = template.ImageFile,
            .PowerBoost = template.PowerBoost
        }

        boss.Cash -= template.Cost
        boss.ActionPoints -= 1
        boss.Inventory.Add(weapon)

        Return New ActionResult(True, $"{boss.Name} picks up a {weapon.Name}.")
    End Function

    Public Function EquipWeapon(weapon As WeaponCard, target As CrewCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If Not boss.Inventory.Contains(weapon) Then
            Return New ActionResult(False, "That's not in your inventory.")
        End If
        If Not boss.Crew.Contains(target) Then
            Return New ActionResult(False, "That crew isn't yours to arm.")
        End If

        boss.ActionPoints -= 1
        boss.Inventory.Remove(weapon)
        target.Power += weapon.PowerBoost

        Return New ActionResult(True, $"{boss.Name} arms {target.Name} with a {weapon.Name} (+{weapon.PowerBoost} power).")
    End Function

    Public Function BuildRacket(template As RacketCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If template.Cost > boss.Cash Then
            Return New ActionResult(False, $"Not enough cash to build a {template.Name}.")
        End If
        If boss.Rackets.Count >= CrimeBoss.MaxRacketSlots Then
            Return New ActionResult(False, "No open turf left to build on.")
        End If

        Dim openSlot As Integer = -1
        For candidate As Integer = 0 To CrimeBoss.MaxRacketSlots - 1
            Dim slotTaken As Boolean = False
            For Each existing As RacketCard In boss.Rackets
                If existing.SlotIndex = candidate Then
                    slotTaken = True
                    Exit For
                End If
            Next
            If Not slotTaken Then
                openSlot = candidate
                Exit For
            End If
        Next

        Dim racket As New RacketCard With {
            .Name = template.Name, .Cost = template.Cost, .Description = template.Description, .ImageFile = template.ImageFile,
            .Income = template.Income, .SlotIndex = openSlot
        }

        boss.Cash -= template.Cost
        boss.ActionPoints -= 1
        boss.Rackets.Add(racket)

        Return New ActionResult(True, $"{boss.Name} opens {racket.Name} on the block — pulling in ${racket.Income} more a turn.")
    End Function

    Public Function PlayJobCard(card As OperationCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer
        Dim rival As CrimeBoss = WaitingPlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If Not boss.JobHand.Contains(card) Then
            Return New ActionResult(False, "That job isn't in hand.")
        End If
        If card.Cost > boss.Cash Then
            Return New ActionResult(False, $"Not enough cash to run {card.Name}.")
        End If

        boss.Cash -= card.Cost
        boss.ActionPoints -= 1
        boss.JobHand.Remove(card)
        boss.JobDiscard.Add(card)

        Dim message As String

        Select Case card.Effect
            Case OperationEffect.GainCash
                boss.Cash += card.Amount
                message = $"{boss.Name} runs '{card.Name}' and pockets ${card.Amount} extra."

            Case OperationEffect.DealDamage
                rival.Life -= card.Amount
                message = $"{boss.Name} hits {rival.Name} with '{card.Name}' for {card.Amount} damage."

            Case OperationEffect.DrawCards
                For i As Integer = 1 To card.Amount
                    boss.DrawJobCard()
                Next
                message = $"{boss.Name} calls in a favor: '{card.Name}' draws {card.Amount} more job(s)."

            Case OperationEffect.ForceDiscard
                If rival.JobHand.Count > 0 Then
                    Dim rng As New Random()
                    Dim index As Integer = rng.Next(rival.JobHand.Count)
                    Dim discarded As OperationCard = rival.JobHand(index)
                    rival.JobHand.RemoveAt(index)
                    rival.JobDiscard.Add(discarded)
                    message = $"{boss.Name} runs '{card.Name}' — {rival.Name} loses {discarded.Name}."
                Else
                    message = $"{boss.Name} runs '{card.Name}', but {rival.Name} has nothing to lose."
                End If

            Case Else
                message = $"{boss.Name} runs '{card.Name}'."
        End Select

        Return New ActionResult(True, message)
    End Function

    Public Function AttackBoss(attacker As CrewCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer
        Dim rival As CrimeBoss = WaitingPlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If Not boss.Crew.Contains(attacker) Then
            Return New ActionResult(False, "That crew isn't yours to command.")
        End If

        boss.ActionPoints -= 1
        rival.Life -= attacker.Power

        Return New ActionResult(True, $"{attacker.Name} hits {rival.Name} directly for {attacker.Power} damage.")
    End Function

    Public Function AttackCrew(attacker As CrewCard, defender As CrewCard) As ActionResult
        If IsGameOver Then
            Return New ActionResult(False, "The war is already over.")
        End If

        Dim boss As CrimeBoss = ActivePlayer
        Dim rival As CrimeBoss = WaitingPlayer

        If boss.ActionPoints <= 0 Then
            Return New ActionResult(False, "No moves left this turn.")
        End If
        If Not boss.Crew.Contains(attacker) Then
            Return New ActionResult(False, "That crew isn't yours to command.")
        End If
        If Not rival.Crew.Contains(defender) Then
            Return New ActionResult(False, "That target isn't on the rival's crew.")
        End If

        boss.ActionPoints -= 1
        defender.Toughness -= attacker.Power
        attacker.Toughness -= defender.Power

        Dim message As String = $"{attacker.Name} clashes with {defender.Name}."

        If Not defender.IsAlive Then
            rival.Crew.Remove(defender)
            message &= $" {defender.Name} gets whacked."
        End If
        If Not attacker.IsAlive Then
            boss.Crew.Remove(attacker)
            message &= $" {attacker.Name} goes down too."
        End If

        Return New ActionResult(True, message)
    End Function

End Class
