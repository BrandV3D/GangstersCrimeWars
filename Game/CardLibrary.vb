' Fixed catalogs (the "shops") plus the small Job deck that still gets drawn as a hand of cards.
Public Module CardLibrary

    Public Function MenTemplates() As List(Of CrewCard)
        Return New List(Of CrewCard) From {
            New CrewCard With {.Name = "Street Tough", .Cost = 1, .Power = 1, .Toughness = 2, .ImageFile = "Cards\Men\men_Street_Tough.png", .Description = "Cheap muscle to start the crew."},
            New CrewCard With {.Name = "Robber", .Cost = 2, .Power = 2, .Toughness = 2, .ImageFile = "Cards\Men\men_Robber.png", .Description = "Quick hands, quicker feet."},
            New CrewCard With {.Name = "Jeweler", .Cost = 3, .Power = 2, .Toughness = 3, .ImageFile = "Cards\Men\men_Jeweler.png", .Description = "Knows the value of everything you steal."},
            New CrewCard With {.Name = "Lieutenant", .Cost = 4, .Power = 3, .Toughness = 4, .ImageFile = "Cards\Men\men_Lieutenant.png", .Description = "Runs the crew when the Boss can't."},
            New CrewCard With {.Name = "Hitman", .Cost = 5, .Power = 5, .Toughness = 3, .ImageFile = "Cards\Men\men_Hitman.png", .Description = "Hired for one job. Always finishes it."},
            New CrewCard With {.Name = "Under Boss", .Cost = 6, .Power = 5, .Toughness = 5, .ImageFile = "Cards\Men\men_Under_Boss.png", .Description = "Second only to the Boss."}
        }
    End Function

    Public Function GunTemplates() As List(Of WeaponCard)
        Return New List(Of WeaponCard) From {
            New WeaponCard With {.Name = "Colt", .Cost = 1, .PowerBoost = 1, .ImageFile = "Cards\Guns\guns_COLT.png", .Description = "A classic sidearm."},
            New WeaponCard With {.Name = "Remington", .Cost = 2, .PowerBoost = 2, .ImageFile = "Cards\Guns\guns_REMINGTON.png", .Description = "Reliable and hard-hitting."},
            New WeaponCard With {.Name = "Shotgun", .Cost = 2, .PowerBoost = 2, .ImageFile = "Cards\Guns\guns_SHOTGUN.png", .Description = "Close range, close call."},
            New WeaponCard With {.Name = "Molotov Cocktail", .Cost = 3, .PowerBoost = 3, .ImageFile = "Cards\Guns\guns_MOLOTOV_COCKTAIL.png", .Description = "Homemade and unforgiving."},
            New WeaponCard With {.Name = "Machine Gun", .Cost = 4, .PowerBoost = 3, .ImageFile = "Cards\Guns\guns_MACHINE_GUN.png", .Description = "Sprays the whole block."},
            New WeaponCard With {.Name = "Tommy Gun", .Cost = 5, .PowerBoost = 4, .ImageFile = "Cards\Guns\guns_TOMMY_GUN.png", .Description = "The icon of the era."}
        }
    End Function

    Public Function BuildingTemplates() As List(Of RacketCard)
        Return New List(Of RacketCard) From {
            New RacketCard With {.Name = "Safe House", .Cost = 2, .Income = 1, .ImageFile = "Cards\Buildings\building_SAFE_HOUSE.png", .Description = "A quiet place to lay low."},
            New RacketCard With {.Name = "Fish Market", .Cost = 3, .Income = 1, .ImageFile = "Cards\Buildings\building_FISH_MARKET.png", .Description = "Moves more than fish."},
            New RacketCard With {.Name = "Pizzeria", .Cost = 3, .Income = 2, .ImageFile = "Cards\Buildings\building_PIZZAERIA.png", .Description = "A good front for a better racket."},
            New RacketCard With {.Name = "Nightclub", .Cost = 4, .Income = 2, .ImageFile = "Cards\Buildings\building_NIGHTCLUB.png", .Description = "Booze, jazz, and skimmed profits."},
            New RacketCard With {.Name = "Loan Office", .Cost = 4, .Income = 3, .ImageFile = "Cards\Buildings\building_LOAN OFFICE.png", .Description = "Interest rates nobody dares question."},
            New RacketCard With {.Name = "Casino", .Cost = 5, .Income = 3, .ImageFile = "Cards\Buildings\building_CASINO.png", .Description = "The house always wins."},
            New RacketCard With {.Name = "Money Laundry", .Cost = 6, .Income = 4, .ImageFile = "Cards\Buildings\building_MONEY_LAUNDRY.png", .Description = "Dirty money in, clean money out."},
            New RacketCard With {.Name = "Hotel", .Cost = 7, .Income = 5, .ImageFile = "Cards\Buildings\building_HOTEL.png", .Description = "The crown jewel of the empire."}
        }
    End Function

    Public Function OperationTemplates() As List(Of OperationCard)
        Return New List(Of OperationCard) From {
            New OperationCard With {.Name = "Bribe the Beat Cop", .Cost = 1, .Effect = OperationEffect.GainCash, .Amount = 3, .Description = "Grease a palm, free up some cash."},
            New OperationCard With {.Name = "Drive-By Shooting", .Cost = 2, .Effect = OperationEffect.DealDamage, .Amount = 3, .Description = "Deal 3 damage straight to the rival Boss."},
            New OperationCard With {.Name = "Tip-Off", .Cost = 2, .Effect = OperationEffect.DrawCards, .Amount = 2, .Description = "A friend on the inside gets you 2 more jobs."},
            New OperationCard With {.Name = "Rat Out", .Cost = 3, .Effect = OperationEffect.ForceDiscard, .Amount = 1, .Description = "Force the rival Boss to lose a job."}
        }
    End Function

    ' Each player's personal Job deck: three copies of every job template, shuffled.
    Public Function BuildJobDeck() As List(Of OperationCard)
        Dim deck As New List(Of OperationCard)

        For copyNumber As Integer = 1 To 3
            deck.AddRange(OperationTemplates())
        Next

        Return deck
    End Function

    Public Sub ShuffleJobs(deck As List(Of OperationCard))
        Dim rng As New Random()
        For i As Integer = deck.Count - 1 To 1 Step -1
            Dim j As Integer = rng.Next(i + 1)
            Dim temp As OperationCard = deck(i)
            deck(i) = deck(j)
            deck(j) = temp
        Next
    End Sub

End Module
