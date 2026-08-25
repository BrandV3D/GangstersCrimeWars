Public Class Gangster_Crime_Wars

    Private ReadOnly _assetsRoot As String = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets")

    Private ReadOnly _crewMarket As List(Of CrewCard) = CardLibrary.MenTemplates()
    Private ReadOnly _weaponMarket As List(Of WeaponCard) = CardLibrary.GunTemplates()
    Private ReadOnly _buildingMarket As List(Of RacketCard) = CardLibrary.BuildingTemplates()

    Private _engine As GameEngine
    Private _selectedAttacker As CrewCard = Nothing
    Private _selectedWeapon As WeaponCard = Nothing
    Private _musicOn As Boolean = True

    Public Sub New()
        InitializeComponent()

        TableBackgroundImage.Source = LoadImage("TableBackground.jpg")
        BannerImage.Source = LoadImage("Banner.png")
        MapBannerImage.Source = LoadImage("Map.jpg")
        RecruitHeaderImage.Source = LoadImage("Headers\Recruit.png")
        BootleggingHeaderImage.Source = LoadImage("Headers\Bootlegging.png")
        CityBuildingsHeaderImage.Source = LoadImage("Headers\CityBuildings.png")
        NewspaperHeaderImage.Source = LoadImage("Headers\Newspaper.png")
        NewspaperImage.Source = LoadImage("Newspaper.png")
        OwnCashIcon.Source = LoadImage("Icons\Cash.png")
        RivalCashIcon.Source = LoadImage("Icons\Cash.png")
        OwnLifeBarBrush.ImageSource = LoadImage("Icons\LifeBar.png")
        RivalLifeBarBrush.ImageSource = LoadImage("Icons\LifeBar.png")
        OwnDeckBackImage.Source = LoadImage("DeckBack.png")
        RivalDeckBackImage.Source = LoadImage("DeckBack.png")
        Me.Icon = LoadImage("Logo.png")

        SetUpMusic()
        StartNewGame()
    End Sub

    Private Function LoadImage(relativePath As String) As BitmapImage
        If String.IsNullOrEmpty(relativePath) Then
            Return Nothing
        End If

        Dim fullPath As String = System.IO.Path.Combine(_assetsRoot, relativePath)
        If Not System.IO.File.Exists(fullPath) Then
            Return Nothing
        End If

        Dim image As New BitmapImage()
        image.BeginInit()
        image.CacheOption = BitmapCacheOption.OnLoad
        image.UriSource = New Uri(fullPath)
        image.EndInit()
        image.Freeze()
        Return image
    End Function

    Private Sub SetUpMusic()
        Dim musicPath As String = System.IO.Path.Combine(_assetsRoot, "Music", "GameMusic.wav")
        If Not System.IO.File.Exists(musicPath) Then
            MuteButton.IsEnabled = False
            Return
        End If

        AddHandler MusicPlayer.MediaEnded, Sub(sender As Object, e As RoutedEventArgs)
                                                MusicPlayer.Position = TimeSpan.Zero
                                                MusicPlayer.Play()
                                            End Sub

        MusicPlayer.Source = New Uri(musicPath)
        MusicPlayer.Volume = 1.0
        MusicPlayer.Play()
    End Sub

    Private Sub StartNewGame()
        _engine = New GameEngine()
        _selectedAttacker = Nothing
        _selectedWeapon = Nothing
        LogListBox.Items.Clear()
        AddLog($"{_engine.ActivePlayer.Name} opens the war. Turn 1.")
        RefreshUI()
    End Sub

    Private Sub AddLog(message As String)
        LogListBox.Items.Insert(0, message)
    End Sub

    Private Sub RefreshUI()
        Dim boss As CrimeBoss = _engine.ActivePlayer
        Dim rival As CrimeBoss = _engine.WaitingPlayer

        OwnArtImage.Source = LoadImage(boss.BossArtImageFile)
        RivalArtImage.Source = LoadImage(rival.BossArtImageFile)
        OwnPortraitImage.Source = LoadImage(boss.PortraitImageFile)
        RivalPortraitImage.Source = LoadImage(rival.PortraitImageFile)

        OwnCashText.Text = $"${boss.Cash} (+{1 + boss.RacketIncome}/turn)"
        OwnLifeText.Text = $"{Math.Max(0, boss.Life)}/20"
        OwnLifeBarFill.Width = 150.0 * Math.Max(0, Math.Min(20, boss.Life)) / 20.0
        OwnDeckCountText.Text = $"{boss.JobDeck.Count} jobs left"
        BuildApPips(OwnApPanel, boss.ActionPoints)

        RivalCashText.Text = $"${rival.Cash} (+{1 + rival.RacketIncome}/turn)"
        RivalLifeText.Text = $"{Math.Max(0, rival.Life)}/20"
        RivalLifeBarFill.Width = 150.0 * Math.Max(0, Math.Min(20, rival.Life)) / 20.0
        RivalDeckCountText.Text = $"{rival.JobDeck.Count} jobs left"
        BuildApPips(RivalApPanel, rival.ActionPoints)

        TurnText.Text = $"Turn {_engine.TurnNumber} — {boss.Name}'s move"

        BuildCrewPanel(OwnBoardPanel, boss.Crew, True)
        BuildCrewPanel(RivalBoardPanel, rival.Crew, False)
        BuildTurfPanel(OwnTurfPanel, boss.Rackets)
        BuildTurfPanel(RivalTurfPanel, rival.Rackets)
        BuildMarketPanel(CrewMarketPanel, _crewMarket)
        BuildMarketPanel(WeaponMarketPanel, _weaponMarket)
        BuildMarketPanel(BuildingMarketPanel, _buildingMarket)
        BuildInventoryPanel(boss)
        BuildJobHandPanel(boss)

        AttackBossButton.IsEnabled = (_selectedAttacker IsNot Nothing) AndAlso boss.ActionPoints > 0 AndAlso Not _engine.IsGameOver
        EndTurnButton.IsEnabled = Not _engine.IsGameOver

        If _engine.IsGameOver Then
            TurnText.Text = $"{_engine.Winner.Name} RUNS THIS TOWN NOW"
        End If
    End Sub

    Private Sub BuildApPips(panel As StackPanel, currentAp As Integer)
        panel.Children.Clear()
        Dim readyBitmap As BitmapImage = LoadImage("Icons\ApReady.png")
        Dim spentBitmap As BitmapImage = LoadImage("Icons\ApSpent.png")

        For i As Integer = 1 To CrimeBoss.StartingActionPoints
            panel.Children.Add(New Image With {
                .Source = If(i <= currentAp, readyBitmap, spentBitmap),
                .Width = 16,
                .Height = 16,
                .Margin = New Thickness(2, 0, 2, 0)
            })
        Next
    End Sub

    Private Sub BuildJobHandPanel(boss As CrimeBoss)
        JobHandPanel.Children.Clear()

        For Each card As OperationCard In boss.JobHand
            Dim playable As Boolean = card.Cost <= boss.Cash AndAlso boss.ActionPoints > 0 AndAlso Not _engine.IsGameOver

            Dim tile As Border = BuildCardTile(card, playable)
            AddHandler tile.MouseLeftButtonUp, Sub(sender As Object, e As MouseButtonEventArgs)
                                                    If playable Then
                                                        JobCard_Click(card)
                                                    End If
                                                End Sub
            JobHandPanel.Children.Add(tile)
        Next
    End Sub

    Private Sub BuildMarketPanel(panel As WrapPanel, items As IEnumerable(Of GameCard))
        panel.Children.Clear()
        Dim boss As CrimeBoss = _engine.ActivePlayer

        For Each template As GameCard In items
            Dim enabled As Boolean = template.Cost <= boss.Cash AndAlso boss.ActionPoints > 0 AndAlso Not _engine.IsGameOver

            If TypeOf template Is CrewCard AndAlso boss.Crew.Count >= CrimeBoss.MaxCrewSlots Then
                enabled = False
            End If
            If TypeOf template Is WeaponCard AndAlso boss.Inventory.Count >= CrimeBoss.MaxInventorySlots Then
                enabled = False
            End If
            If TypeOf template Is RacketCard AndAlso boss.Rackets.Count >= CrimeBoss.MaxRacketSlots Then
                enabled = False
            End If

            Dim tile As Border = BuildCardTile(template, enabled, True)
            AddHandler tile.MouseLeftButtonUp, Sub(sender As Object, e As MouseButtonEventArgs)
                                                    If enabled Then
                                                        MarketItem_Click(template)
                                                    End If
                                                End Sub
            panel.Children.Add(tile)
        Next
    End Sub

    Private Sub BuildInventoryPanel(boss As CrimeBoss)
        InventoryPanel.Children.Clear()

        For i As Integer = 0 To CrimeBoss.MaxInventorySlots - 1
            If i < boss.Inventory.Count Then
                Dim weapon As WeaponCard = boss.Inventory(i)
                Dim tile As Border = BuildCardTile(weapon, Not _engine.IsGameOver, True)

                If weapon Is _selectedWeapon Then
                    tile.BorderBrush = Brushes.Gold
                    tile.BorderThickness = New Thickness(3)
                    tile.Opacity = 1.0
                End If

                AddHandler tile.MouseLeftButtonUp, Sub(sender As Object, e As MouseButtonEventArgs)
                                                        InventorySlot_Click(weapon)
                                                    End Sub
                InventoryPanel.Children.Add(tile)
            Else
                InventoryPanel.Children.Add(BuildEmptySlotTile())
            End If
        Next
    End Sub

    Private Function BuildEmptySlotTile() As Border
        Dim border As New Border With {
            .Width = 100,
            .Height = 130,
            .Margin = New Thickness(4),
            .BorderBrush = New SolidColorBrush(Color.FromRgb(&H44, &H40, &H38)),
            .BorderThickness = New Thickness(1)
        }

        Dim bitmap As BitmapImage = LoadImage("Icons\EmptySlot.png")
        If bitmap IsNot Nothing Then
            border.Child = New Image With {.Source = bitmap, .Stretch = Stretch.Uniform, .Margin = New Thickness(10)}
        End If

        Return border
    End Function

    Private Sub BuildTurfPanel(panel As WrapPanel, rackets As List(Of RacketCard))
        panel.Children.Clear()

        For slotIndex As Integer = 0 To CrimeBoss.MaxRacketSlots - 1
            Dim racket As RacketCard = Nothing
            For Each candidate As RacketCard In rackets
                If candidate.SlotIndex = slotIndex Then
                    racket = candidate
                    Exit For
                End If
            Next
            If racket IsNot Nothing Then
                panel.Children.Add(BuildCardTile(racket, True, True))
            Else
                panel.Children.Add(BuildEmptyTurfTile())
            End If
        Next
    End Sub

    Private Function BuildEmptyTurfTile() As Border
        Return New Border With {
            .Width = 100,
            .Height = 130,
            .Margin = New Thickness(4),
            .Background = New SolidColorBrush(Color.FromArgb(&H40, &H00, &H00, &H00)),
            .BorderBrush = New SolidColorBrush(Color.FromRgb(&H33, &H33, &H33)),
            .BorderThickness = New Thickness(1),
            .Child = New TextBlock With {
                .Text = "Open Turf",
                .Foreground = Brushes.Gray,
                .FontSize = 10,
                .TextAlignment = TextAlignment.Center,
                .TextWrapping = TextWrapping.Wrap,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .Margin = New Thickness(6)
            }
        }
    End Function

    Private Sub BuildCrewPanel(panel As WrapPanel, crew As List(Of CrewCard), isOwn As Boolean)
        panel.Children.Clear()

        For Each member As CrewCard In crew
            Dim isSelected As Boolean = isOwn AndAlso member Is _selectedAttacker
            Dim tile As Border = BuildCrewTile(member, isOwn, isSelected)

            AddHandler tile.MouseLeftButtonUp, Sub(sender As Object, e As MouseButtonEventArgs)
                                                    CrewTile_Click(member, isOwn)
                                                End Sub
            panel.Children.Add(tile)
        Next
    End Sub

    Private Function BuildCardTile(card As GameCard, enabled As Boolean, Optional compact As Boolean = False) As Border
        Dim tileWidth As Double = If(compact, 100, 140)
        Dim tileHeight As Double = If(compact, 130, 185)
        Dim imageHeight As Double = If(compact, 55, 100)
        Dim nameFontSize As Double = If(compact, 10, 12)
        Dim statFontSize As Double = If(compact, 9, 10)

        Dim border As New Border With {
            .Width = tileWidth,
            .Height = tileHeight,
            .Margin = New Thickness(4),
            .Background = New SolidColorBrush(Color.FromRgb(&H17, &H15, &H1A)),
            .BorderBrush = New SolidColorBrush(Color.FromRgb(&HC9, &HA2, &H27)),
            .BorderThickness = New Thickness(If(enabled, 2, 1)),
            .Cursor = If(enabled, Cursors.Hand, Cursors.Arrow),
            .Opacity = If(enabled, 1.0, 0.5),
            .ToolTip = card.Description
        }

        Dim outer As New StackPanel()

        Dim bitmap As BitmapImage = LoadImage(card.ImageFile)
        If bitmap IsNot Nothing Then
            outer.Children.Add(New Image With {
                .Source = bitmap,
                .Height = imageHeight,
                .Stretch = Stretch.UniformToFill,
                .ClipToBounds = True
            })
        End If

        Dim textPanel As New StackPanel With {.Margin = New Thickness(6, 4, 6, 4)}

        textPanel.Children.Add(New TextBlock With {
            .Text = card.Name,
            .FontFamily = New FontFamily("Georgia"),
            .FontWeight = FontWeights.Bold,
            .Foreground = Brushes.Wheat,
            .TextWrapping = TextWrapping.Wrap,
            .FontSize = nameFontSize
        })

        textPanel.Children.Add(New TextBlock With {
            .Text = $"${card.Cost}",
            .Foreground = New SolidColorBrush(Color.FromRgb(&HC9, &HA2, &H27)),
            .FontWeight = FontWeights.Bold,
            .FontSize = nameFontSize,
            .Margin = New Thickness(0, 2, 0, 2)
        })

        If TypeOf card Is CrewCard Then
            Dim crew As CrewCard = DirectCast(card, CrewCard)
            textPanel.Children.Add(New TextBlock With {.Text = $"PWR {crew.Power} / TGH {crew.Toughness}", .Foreground = Brushes.LightGray, .FontSize = statFontSize})
        ElseIf TypeOf card Is WeaponCard Then
            Dim weapon As WeaponCard = DirectCast(card, WeaponCard)
            textPanel.Children.Add(New TextBlock With {.Text = $"+{weapon.PowerBoost} Power (equip)", .Foreground = Brushes.LightGray, .FontSize = statFontSize, .TextWrapping = TextWrapping.Wrap})
        ElseIf TypeOf card Is RacketCard Then
            Dim racket As RacketCard = DirectCast(card, RacketCard)
            textPanel.Children.Add(New TextBlock With {.Text = $"+${racket.Income}/turn income", .Foreground = Brushes.LightGray, .FontSize = statFontSize, .TextWrapping = TextWrapping.Wrap})
        Else
            textPanel.Children.Add(New TextBlock With {.Text = card.Description, .Foreground = Brushes.LightGray, .FontSize = statFontSize - 1, .TextWrapping = TextWrapping.Wrap})
        End If

        outer.Children.Add(textPanel)
        border.Child = outer
        Return border
    End Function

    Private Function BuildCrewTile(crew As CrewCard, isOwn As Boolean, isSelected As Boolean) As Border
        Dim canClick As Boolean = Not _engine.IsGameOver AndAlso (isOwn OrElse _selectedAttacker IsNot Nothing)

        Dim tile As Border = BuildCardTile(crew, canClick)

        If isSelected Then
            tile.BorderBrush = Brushes.Gold
            tile.BorderThickness = New Thickness(3)
            tile.Opacity = 1.0
        End If

        Return tile
    End Function

    Private Sub MarketItem_Click(template As GameCard)
        Dim result As ActionResult

        If TypeOf template Is CrewCard Then
            result = _engine.RecruitCrew(DirectCast(template, CrewCard))
        ElseIf TypeOf template Is WeaponCard Then
            result = _engine.BuyWeapon(DirectCast(template, WeaponCard))
        Else
            result = _engine.BuildRacket(DirectCast(template, RacketCard))
        End If

        AddLog(result.Message)
        RefreshUI()
    End Sub

    Private Sub InventorySlot_Click(weapon As WeaponCard)
        If _engine.IsGameOver Then
            Return
        End If

        _selectedWeapon = If(weapon Is _selectedWeapon, Nothing, weapon)
        RefreshUI()
    End Sub

    Private Sub CrewTile_Click(crew As CrewCard, isOwn As Boolean)
        If _engine.IsGameOver Then
            Return
        End If

        If isOwn Then
            If _selectedWeapon IsNot Nothing Then
                Dim result As ActionResult = _engine.EquipWeapon(_selectedWeapon, crew)
                AddLog(result.Message)
                _selectedWeapon = Nothing
                RefreshUI()
                Return
            End If

            _selectedAttacker = If(crew Is _selectedAttacker, Nothing, crew)
        Else
            If _selectedAttacker IsNot Nothing Then
                Dim result As ActionResult = _engine.AttackCrew(_selectedAttacker, crew)
                AddLog(result.Message)
                _selectedAttacker = Nothing
            End If
        End If

        RefreshUI()
    End Sub

    Private Sub JobCard_Click(card As OperationCard)
        Dim result As ActionResult = _engine.PlayJobCard(card)
        AddLog(result.Message)
        RefreshUI()
    End Sub

    Private Sub AttackBossButton_Click(sender As Object, e As RoutedEventArgs)
        If _selectedAttacker Is Nothing Then
            Return
        End If

        Dim result As ActionResult = _engine.AttackBoss(_selectedAttacker)
        AddLog(result.Message)
        _selectedAttacker = Nothing
        RefreshUI()
    End Sub

    Private Sub EndTurnButton_Click(sender As Object, e As RoutedEventArgs)
        _selectedAttacker = Nothing
        _selectedWeapon = Nothing
        Dim result As ActionResult = _engine.EndTurn()
        AddLog(result.Message)
        RefreshUI()
    End Sub

    Private Sub NewGameButton_Click(sender As Object, e As RoutedEventArgs)
        StartNewGame()
    End Sub

    Private Sub HowToPlayButton_Click(sender As Object, e As RoutedEventArgs)
        Dim image As BitmapImage = LoadImage("HowToPlay.png")
        If image Is Nothing Then
            AddLog("How-to-play sheet not found.")
            Return
        End If

        Dim frame As BitmapImage = LoadImage("DialogBox.png")
        Dim window As New HowToPlayWindow(image, frame) With {.Owner = Me}
        window.ShowDialog()
    End Sub

    Private Sub MuteButton_Click(sender As Object, e As RoutedEventArgs)
        _musicOn = Not _musicOn
        MusicPlayer.Volume = If(_musicOn, 1.0, 0.0)
        MuteButton.Content = If(_musicOn, "🔊", "🔇")
    End Sub

End Class
