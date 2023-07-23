Imports Machine.Devices
Imports Machine.Abstracts
Imports System.Drawing.Drawing2D

Namespace Output
    Public Class Fourier
        Inherits IOutput
        Public Property Absolute As Double
        Public Property Shift As Double
        Public Property MaxScale As Double
        Public Property MinScale As Double
        Public Property MaxFreq As Double
        Public Property MinFreq As Double
        Public Property Peeks As Integer
        Public Property Peaks As List(Of Highlight)

        Sub New(wfunc As WFunction, Optional peaks As Integer = 16)
            Me.Shift = 5.0F
            Me.Absolute = 0.1F
            Me.WFunction = wfunc
            Me.Peeks = peaks
            Me.MaxScale = 1.0F
            Me.MinScale = -1.0F
            Me.Peaks = New List(Of Highlight)
        End Sub

        Public Overrides Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)
            'Validate conditions
            If (Me.Data.Any AndAlso Me.Data.Count Mod device.Hub.Samplerate = 0) Then

                ' Gather information
                Me.MinFreq = 0
                Me.MaxFreq = (device.Hub.Samplerate / 2)
                'Me.Real = Transformer.Transform(Transformer.Wrap(Me.Data, Me.WFunction))
                Me.MinScale = device.Real.Min(Function(c) c.Magnitude)
                Me.MaxScale = device.Real.Max(Function(c) c.Magnitude)

                ' Minimum Y check
                Dim offset As Double = Math.Abs(Me.MaxScale - Me.MinScale)
                If offset < Me.Absolute Then
                    Me.ProgressBar(g, bounds, bounds.Width \ 10, bounds.Height \ 10, Math.Round((offset / Me.Absolute) * 100))
                Else
                    ' Zero check
                    If Me.MaxScale = Me.MinScale Then Me.MaxScale += 1.0
                    ' Gather division
                    Dim division As Integer = Transformer.Divisions(device.Real)

                    ' Gather peak information
                    Me.Peaks = Transformer.DetectPeaks(device.Real, Me.Peeks)

                    ' Scale the data to fit
                    Dim scaleY As Double = bounds.Height / (Me.MaxScale - Me.MinScale)
                    Dim scaleX As Double = bounds.Width / ((device.Real.Length / 2) - 1)

                    'Commit graph
                    Using pen As New Pen(Color.White, 1)
                        Using grid As New Pen(Color.FromArgb(45, pen.Color), 1) With {.DashStyle = DashStyle.Dash}
                            Me.DrawGrid(g, bounds, grid, division)
                            Me.DrawFFT(device, g, bounds, pen, division, scaleX, scaleY)
                            Me.DrawPeaks(device, g, bounds, pen, scaleX, scaleY, 14, 100)
                        End Using
                    End Using
                End If
            End If
        End Sub

        Private Sub ProgressBar(g As Graphics, bounds As Rectangle, width As Single, height As Single, percentage As Double)
            If (percentage > 100.0F) Then Return
            Static padding As Integer = 6
            ' the progress bar base size
            Dim srcrect As New RectangleF((bounds.Width - width) / 2, (bounds.Height - height) / 2, width, height)

            ' Draw the background of the progress bar
            Using background As New SolidBrush(Color.DarkGray)
                g.FillRectangle(background, srcrect)
            End Using

            ' Draw the progress indicator
            Dim pwidth As Single = width * CSng(percentage / 100.0)
            Dim pbox As New RectangleF(srcrect.Left + (padding \ 2), srcrect.Top + (padding \ 2), pwidth - padding, srcrect.Height - padding)
            Using pbrush As New SolidBrush(Color.Blue)
                g.FillRectangle(pbrush, pbox)
                g.DrawRectangle(Pens.Blue, pbox.X, pbox.Y, pbox.Width, pbox.Height)
                Effects.Gloss(g, pbox, Me.Theme, 45, 120, LinearGradientMode.ForwardDiagonal)
            End Using
            ' Draw the border of the progress bar
            Using borderPen As New Pen(Color.Black, 1)
                g.DrawRectangle(borderPen, Rectangle.Round(srcrect))
            End Using
        End Sub

        Private Sub DrawGrid(g As Graphics, bounds As Rectangle, pen As Pen, division As Integer)
            Dim half As Integer = division \ 2
            For i As Integer = 1 To half
                Dim x As Single = i * bounds.Width \ half
                g.DrawLine(pen, x, 0, x, bounds.Height)
            Next
            For i As Integer = 1 To half
                Dim y As Single = i * bounds.Height \ half
                g.DrawLine(pen, 0, y, bounds.Width, y)
            Next
        End Sub

        Private Sub DrawFFT(device As IDevice, g As Graphics, bounds As Rectangle, pen As Pen, division As Integer, sx As Double, sy As Double)
            For i As Integer = 0 To (device.Real.Length \ 2) - 2
                Dim x1 As Single = CSng(i * sx)
                Dim y1 As Single = CSng((bounds.Height - Me.Shift) - (device.Real(i).Magnitude - Me.MinScale) * sy)
                Dim x2 As Single = CSng((i + 1) * sx)
                Dim y2 As Single = CSng((bounds.Height - Me.Shift) - (device.Real((i + 1)).Magnitude - Me.MinScale) * sy)
                g.DrawLine(pen, x1, y1, x2, y2)
                If (i Mod division = 0) Then
                    Dim frequency As Double = (i * device.Hub.Samplerate) / device.Real.Length
                    Effects.DrawLabel(g, x1, CSng(bounds.Height - 10), device.Font, Color.Blue, String.Format("{0:F0}Hz", frequency))
                    g.DrawLine(pen, x1, bounds.Height - 10, x1, bounds.Height - 10)
                End If
            Next
        End Sub

        Private Sub DrawPeaks(device As IDevice, g As Graphics, bounds As Rectangle, pen As Pen, sx As Double, sy As Double, padding As Single, Optional edge As Integer = 10)
            If (Me.Peaks.Any) Then
                Dim lastPeakY As Single = 0
                For Each peak As Highlight In Me.Peaks.OrderBy(Function(p) p.Magnitude)
                    Dim peakX As Single = CSng(peak.Index * sx)
                    Dim peakY As Single = 11 + CSng((peak.Magnitude - Me.MinScale) * sy)
                    If ((bounds.Width - peakX) < edge Or peakX < edge) Then Continue For
                    Dim whalf As Single = CSng(peak.Width / 2)
                    Dim frequency As Double = (peak.Index * device.Hub.Samplerate) / device.Real.Length
                    Dim label As String = String.Format("{0:F0}Hz", frequency)
                    If peakY < lastPeakY + padding Then
                        peakY = lastPeakY + padding
                    End If
                    g.DrawLine(pen, peakX, peakY, peakX, bounds.Height)
                    g.FillEllipse(New SolidBrush(Color.Blue), peakX - 3, peakY - 3, 6, 6)
                    g.DrawEllipse(pen, peakX - 3, peakY - 3, 6, 6)
                    Dim tag As String = String.Format("{0:F2}Hz|{1:F2}ms", frequency, peak.ToMilliseconds(device.Real))
                    Effects.DrawLabel(g, peakX, peakY, device.Font, Color.Black, tag)
                    lastPeakY = peakY
                Next
            End If
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return String.Format("Spectrum({0:F0}Hz~{1:F0}Hz,{2:F2}Hz~{3:F2}Hz)", Me.MinFreq, Me.MaxFreq, Me.MinScale, Me.MaxScale)
            End Get
        End Property
    End Class

End Namespace