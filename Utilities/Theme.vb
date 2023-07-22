Public Class Theme
    Public Property ButtomTop As Color
    Public Property ButtonBottom As Color
    Public Property ButtonHighlight As Color
    Public Property ButtonShadow As Color
    Public Property Heatmap As Color()
    Sub New()
        ' Gloss Effect
        Me.ButtomTop = Color.FromArgb(255, 200, 200, 200)       ' Light gray
        Me.ButtonBottom = Color.FromArgb(255, 100, 100, 100)    ' Dark gray
        Me.ButtonHighlight = Color.FromArgb(120, Color.White)   ' White highlight with 120/255 transparency
        Me.ButtonShadow = Color.FromArgb(100, Color.Black)      ' Black shadow with 100/255 transparency
        ' Heatmap
        Me.Heatmap = New Color() {
                        Color.Purple,
                        Color.DarkRed,
                        Color.Red,
                        Color.OrangeRed,
                        Color.Orange,
                        Color.Yellow,
                        Color.YellowGreen,
                        Color.Green,
                        Color.DarkGreen,
                        Color.Black
                    }
    End Sub
End Class
