' The set of iconic crime bosses a player can be assigned at the start of a war.
Public Module BossRoster

    Private ReadOnly PortraitArt As String() = {
        "BossPortraits\bossA.png",
        "BossPortraits\bossB.png",
        "BossPortraits\BossC.png",
        "BossPortraits\BossD.png"
    }

    Public Function AllBosses() As List(Of (Name As String, BannerImageFile As String))
        Return New List(Of (Name As String, BannerImageFile As String)) From {
            ("Carlo Gambino", "BossNames\Carlo Gambino Boss copy.png"),
            ("Al Capone", "BossNames\Gangster_AlCopone.png"),
            ("Bugsy Siegel", "BossNames\Gangster_Bugsy.png"),
            ("John Gotti", "BossNames\Gangster_JohnGotti.png"),
            ("Lucky Luciano", "BossNames\Gangster_LuckyLuciano.png"),
            ("Paul Castellano", "BossNames\Gangster_PaulCastolano.png"),
            ("Sammy the Bull", "BossNames\Gangster_SammyBull.png"),
            ("Vincent ""Mad Dog"" Coll", "BossNames\Gangster_VincentMadDog.png"),
            ("Vincent Mangano", "BossNames\Gangster_VincentMangano.png")
        }
    End Function

    ' Picks two different bosses (name + banner) and pairs each with a random character portrait.
    Public Function PickTwoDistinct() As ((Name As String, BannerImageFile As String, ArtImageFile As String), (Name As String, BannerImageFile As String, ArtImageFile As String))
        Dim all = AllBosses()
        Dim rng As New Random()

        Dim firstIndex As Integer = rng.Next(all.Count)
        Dim secondIndex As Integer = rng.Next(all.Count)
        While secondIndex = firstIndex
            secondIndex = rng.Next(all.Count)
        End While

        Dim first = all(firstIndex)
        Dim second = all(secondIndex)

        Dim firstArt As String = PortraitArt(rng.Next(PortraitArt.Length))
        Dim secondArt As String = PortraitArt(rng.Next(PortraitArt.Length))

        Return ((first.Name, first.BannerImageFile, firstArt), (second.Name, second.BannerImageFile, secondArt))
    End Function

End Module
