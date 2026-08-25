Class HowToPlayWindow

    Public Sub New(image As BitmapImage, frame As BitmapImage)
        InitializeComponent()
        HowToPlayImage.Source = image
        FrameImage.Source = frame
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub

End Class
