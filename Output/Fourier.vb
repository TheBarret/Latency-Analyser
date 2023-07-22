Imports Machine.Devices
Imports Machine.Abstracts
Imports System.Drawing.Drawing2D
Imports System.Net

Namespace Output
    Public Class Fourier
        Inherits IOutput
        Public Property Absolute As Double
        Public Property Line As Color
        Public Property Shift As PointF
        Public Property MaxScale As Double
        Public Property MinScale As Double
        Public Property MaxFreq As Double
        Public Property MinFreq As Double
        Public Property Real As Complex()
        Public Property Peeks As Integer
        Public Property Peaks As List(Of Highlight)
        Public Property WFunction As WFunction

        Sub New(wfunc As WFunction, Optional peaks As Integer = 4)
            Me.Absolute = 0.1F
            Me.WFunction = wfunc
            Me.Peeks = peaks
            Me.MaxScale = 1.0F
            Me.MinScale = -1.0F
            Me.Line = Color.White
            Me.Shift = New PointF(0, 25)
            Me.Peaks = New List(Of Highlight)
        End Sub

        Public Overrides Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)
            'Validate conditions
            If (Me.Data.Any AndAlso Me.Data.Count Mod device.Hub.Samplerate = 0) Then

                ' Gather information
                Me.MinFreq = 0
                Me.MaxFreq = (device.Hub.Samplerate / 2)
                Me.Real = Transformer.Transform(Transformer.Wrap(Me.Data, Me.WFunction))
                Me.MinScale = Me.Real.Min(Function(c) c.Magnitude)
                Me.MaxScale = Me.Real.Max(Function(c) c.Magnitude)

                ' Minimum Y check
                Dim offset As Double = Math.Abs(Me.MaxScale - Me.MinScale)
                If offset < Me.Absolute Then
                    ' Display the % to user to indicate how far we need to be
                    Dim perc As Double = Math.Round((offset / Me.Absolute) * 100)
                    Dim label As String = String.Format("Adjusting...{0}%", perc)
                    Dim wscale As Double = g.MeasureString(label, device.Font).Width
                    Effects.DrawLabel(g, CSng((bounds.Width \ 2) - wscale), bounds.Height \ 2, device.Font, Color.Black, label)
                Else
                    ' Zero check
                    If Me.MaxScale = Me.MinScale Then Me.MaxScale += 1.0
                    ' Gather division
                    Dim division As Integer = Transformer.Divisions(Me.Real)

                    ' Gather peak information
                    Me.Peaks = Transformer.DetectPeaks(Me.Real, Me.Peeks)

                    ' Scale the data to fit
                    Dim scaleY As Double = bounds.Height / (Me.MaxScale - Me.MinScale)
                    Dim scaleX As Double = bounds.Width / ((Me.Real.Length / 2) - 1)

                    'Commit graph
                    Using pen As New Pen(Me.Line, 1)
                        Using grid As New Pen(Color.FromArgb(45, pen.Color), 1) With {.DashStyle = DashStyle.Dot}
                            Me.DrawGrid(g, bounds, grid, division)
                            Me.DrawFFT(device, g, bounds, pen, division, scaleX, scaleY)
                            Me.DrawPeaks(device, g, bounds, scaleX, scaleY, 25)
                        End Using
                    End Using
                End If
            End If
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
            For i As Integer = 0 To (Me.Real.Length \ 2) - 2
                Dim x1 As Single = CSng(i * sx)
                Dim y1 As Single = CSng((bounds.Height - Me.Shift.Y) - (Me.Real(i).Magnitude - Me.MinScale) * sy)
                Dim x2 As Single = CSng((i + 1) * sx)
                Dim y2 As Single = CSng((bounds.Height - Me.Shift.Y) - (Me.Real((i + 1)).Magnitude - Me.MinScale) * sy)
                g.DrawLine(pen, x1, y1, x2, y2)
                If (i Mod division = 0) Then
                    Dim frequency As Double = (i * device.Hub.Samplerate) / Me.Real.Length
                    Effects.DrawLabel(g, x1, CSng(bounds.Height - 10), device.Font, Color.Black, String.Format("{0:F0}Hz", frequency))
                    g.DrawLine(Pens.White, x1, bounds.Height - 10, x1, bounds.Height - 10)
                End If
            Next
        End Sub

        Private Sub DrawPeaks(device As IDevice, g As Graphics, bounds As Rectangle, sx As Double, sy As Double, padding As Single, Optional edge As Integer = 10)
            If (Me.Peaks.Any) Then
                Dim lastPeakY As Single = 0
                For Each peak As Highlight In Me.Peaks.OrderBy(Function(p) p.Magnitude)
                    Dim peakX As Single = CSng(peak.Index * sx)
                    Dim peakY As Single = bounds.Height - CSng(bounds.Height - (peak.Magnitude - Me.MinScale) * sy)
                    If ((bounds.Width - peakX) < edge Or peakX < edge) Then Continue For
                    Dim frequency As Double = (peak.Index * device.Hub.Samplerate) / Me.Real.Length
                    Dim label As String = String.Format("{0:F0}Hz", frequency)
                    g.DrawLine(Pens.White, peakX, peakY, peakX, bounds.Height)
                    g.FillEllipse(Brushes.White, peakX - 3, peakY - 3, 6, 6)
                    g.DrawEllipse(Pens.White, peakX - 3, peakY - 3, 6, 6)
                    If peakY < lastPeakY + padding Then
                        peakY = lastPeakY + padding
                    End If
                    Effects.DrawLabel(g, peakX, peakY + (bounds.Height \ 4), device.Font, Color.Black, String.Format("Grp delay: {0:F2}ms", peak.ToMilliseconds(Me.Real)))
                    Effects.DrawLabel(g, peakX, peakY + (bounds.Height \ 5), device.Font, Color.Black, String.Format("Frequency: {0:F2}Hz", frequency))
                    lastPeakY = peakY
                Next
            End If
        End Sub
    End Class

End Namespace