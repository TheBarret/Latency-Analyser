Imports Machine.Devices
Imports Machine.Abstracts
Imports System.Drawing.Drawing2D

Namespace Output
    Public Class Spectrogram
        Inherits IOutput
        Public Property Bins As Integer
        Public Property Segments As Integer
        Public Property Buffer As Double(,)
        Private m_value As Double = 0

        Sub New(wfunc As WFunction)
            Me.WFunction = wfunc
            Me.Segments = 16
            Me.Bins = 16
            Me.Buffer = New Double(Me.Segments - 1, Me.Bins - 1) {}
        End Sub

        Public Overrides Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)
            'Validate conditions
            If (Me.Data.Count Mod device.Hub.Samplerate = 0) Then
                For i As Integer = 0 To Me.Bins \ 2 - 1
                    For j As Integer = 0 To Me.Segments - 1
                        Me.Buffer(j, i) = device.Real((i * Me.Segments) + j).Magnitude
                    Next
                Next
                'Commit graph
                Using cells As New Pen(Color.Black, 1)
                    Dim scaleX As Single = CSng(bounds.Width / Me.Segments)
                    Dim scaleY As Single = CSng(bounds.Height / (Me.Bins / 2))
                    For j As Integer = 0 To Me.Bins \ 2 - 1
                        For i As Integer = 0 To Me.Segments - 1
                            Me.m_value = Me.Buffer(i, j)
                            Using filler As New SolidBrush(Transformer.GetHeatmap(Me.m_value, Me.Theme.Heatmap))
                                Dim rect As New RectangleF(CSng(i * scaleX), CSng(j * scaleY), scaleX, scaleY)
                                g.FillRectangle(filler, rect)
                                g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height)
                                'Effects.Gloss(g, rect, Me.Theme, 80, 30, LinearGradientMode.BackwardDiagonal)
                            End Using
                        Next
                        If (j > 0) Then
                            Dim frequency As Double = Transformer.GetFrequency(j, device.Hub.Samplerate, Me.Bins)
                            Effects.DrawLabel(g, 1, j * scaleY, device.Font, Color.Blue, String.Format("{0:F0}Hz", frequency))
                        End If
                    Next
                End Using
            End If
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Static resolution As Integer = (Me.Bins \ 2) * Me.Segments
                Return String.Format("Spectrogram({0}/{1:E2})", resolution, Me.m_value)
            End Get
        End Property
    End Class
End Namespace

