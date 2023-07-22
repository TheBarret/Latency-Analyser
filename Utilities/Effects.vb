Imports System.Drawing.Drawing2D

Public Class Effects

    ''' <summary>
    ''' Draws caption label
    ''' </summary>
    Public Shared Sub DrawLabel(g As Graphics, x As Single, y As Single, font As Font, tint As Color, text As String, Optional gloss As Boolean = True)
        Dim offset As Single = g.MeasureString(text, font).Width
        Dim srcrect As New RectangleF(x + 5, y, offset, 11)
        g.FillRectangle(Brushes.White, srcrect)
        g.DrawString(text, font, New SolidBrush(tint), srcrect.X, srcrect.Y)
        g.DrawRectangle(Pens.Black, srcrect.X, srcrect.Y, srcrect.Width, srcrect.Height)
        If (gloss) Then Effects.Gloss(g, srcrect, New Theme, 100, 50, LinearGradientMode.ForwardDiagonal)
    End Sub

    Public Shared Sub DrawHexagon(g As Graphics, cx As Single, cy As Single, radius As Single, brush As SolidBrush)
        ' Calculate the points for the vertices of the hexagon
        Dim points As New List(Of Point)
        For i As Integer = 0 To 5
            Dim angle As Double = 2.0 * Math.PI / 6 * i
            Dim x As Integer = CInt(cx + radius * Math.Cos(angle))
            Dim y As Integer = CInt(cy + radius * Math.Sin(angle))
            points.Add(New Point(x, y))
        Next
        ' Draw the hexagon using a GraphicsPath

        Using path As New GraphicsPath()
            path.AddPolygon(points.ToArray())
            g.FillPath(brush, path)
            g.DrawPath(Pens.Black, path)
        End Using
    End Sub

    ''' <summary>
    ''' Draws an area with given gradient color and shades
    ''' </summary>
    Public Shared Sub GradientEffect(g As Graphics, bounds As RectangleF,
                                     tThreshold As Integer, tColor As Color,
                                     gThreshold As Integer, gColor As Color,
                                     mode As LinearGradientMode)
        ' Create the gradient brush for the top part of the button
        Using b As New LinearGradientBrush(bounds, Color.FromArgb(tThreshold, tColor),
                                                   Color.FromArgb(gThreshold, gColor), mode)

            g.FillRectangle(b, bounds)
        End Using
    End Sub

    ''' <summary>
    ''' Draws a glass like gloss effect over area
    ''' </summary>
    Public Shared Sub Gloss(g As Graphics, bounds As RectangleF, theme As Theme, tThreshold As Integer, gThreshold As Integer, mode As LinearGradientMode)
        Effects.Gloss(g, bounds, tThreshold, theme.ButtomTop, gThreshold, theme.ButtonBottom, theme.ButtonHighlight, theme.ButtonShadow, mode)
    End Sub

    ''' <summary>
    ''' Draws a glass like gloss effect over area
    ''' </summary>
    Public Shared Sub Gloss(g As Graphics, bounds As RectangleF,
                                    tThreshold As Integer, tColor As Color,
                                    gThreshold As Integer, gColor As Color,
                                    highlight As Color, shadow As Color,
                                    mode As LinearGradientMode)
        ' Create the gradient brush for the top part of the button
        Using topBrush As New LinearGradientBrush(bounds, Color.FromArgb(tThreshold, tColor),
                                                  Color.FromArgb(gThreshold, gColor), mode)
            ' Fill the top part of the button with the gradient
            g.FillRectangle(topBrush, bounds)
        End Using

        ' Calculate the highlight and shadow rectangles
        Dim highlightBounds As New RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height / 2)
        Dim shadowBounds As New RectangleF(bounds.Left, bounds.Top + bounds.Height / 2, bounds.Width, bounds.Height / 2)

        ' Create the brushes for the highlight and shadow
        Using highlightBrush As New LinearGradientBrush(highlightBounds, highlight, Color.Transparent, mode)
            Using shadowBrush As New LinearGradientBrush(shadowBounds, Color.Transparent, shadow, mode)
                ' Fill the highlight and shadow areas
                g.FillRectangle(highlightBrush, highlightBounds)
                g.FillRectangle(shadowBrush, shadowBounds)
            End Using
        End Using
    End Sub

    Public Shared Function Hue(tint As Color, shift As Single) As Color
        Dim hsv As Single() = {tint.GetHue(), tint.GetSaturation(), tint.GetBrightness()}
        hsv(0) = (hsv(0) + shift * 360.0F) Mod 360.0F
        Return ColorFromHSV(hsv(0), hsv(1), hsv(2))
    End Function

    Public Shared Function ColorFromHSV(h As Single, s As Single, v As Single) As Color
        If s = 0.0F Then Return Color.FromArgb(CInt(v * 255), CInt(v * 255), CInt(v * 255))
        Dim hi As Integer = CInt(Math.Floor(h / 60.0F)) Mod 6
        Dim f As Single = CSng((h / 60.0F) - Math.Floor(h / 60.0F))

        v *= 255

        Dim p As Integer = CInt(v * (1.0F - s))
        Dim q As Integer = CInt(v * (1.0F - (f * s)))
        Dim t As Integer = CInt(v * (1.0F - ((1.0F - f) * s)))

        Select Case hi
            Case 0 : Return Color.FromArgb(CInt(v), t, p)
            Case 1 : Return Color.FromArgb(q, CInt(v), p)
            Case 2 : Return Color.FromArgb(p, CInt(v), t)
            Case 3 : Return Color.FromArgb(p, q, CInt(v))
            Case 4 : Return Color.FromArgb(t, p, CInt(v))
        End Select
        Return Color.FromArgb(CInt(v), p, q)
    End Function
End Class
