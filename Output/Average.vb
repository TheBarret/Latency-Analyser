Imports Machine.Abstracts

Namespace Output
    Public Class Average
        Inherits IOutput
        Public Property Period As Integer
        Public Property MovingAverage As List(Of Double)
        Private m_avg As Double = 1

        Sub New(wfunc As WFunction)
            Me.WFunction = wfunc
            Me.Period = 1
            Me.MovingAverage = New List(Of Double)
        End Sub

        Public Overrides Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)

            ' Adjust period window after division by 10
            Me.Period = device.Hub.Samplerate \ 10

            ' Calculate moving average
            If Me.Data.Count >= Me.Period Then
                Dim sum As Double = 0
                For i As Integer = 0 To Me.Period - 1
                    sum += Me.Data(Me.Data.Count - 1 - i)
                Next

                Me.MovingAverage.Add(sum / Me.Period)
                Me.m_avg = Me.MovingAverage.Last
                If (Me.MovingAverage.Count > 32) Then
                    Me.MovingAverage.RemoveAt(0)
                End If
            End If

            ' Draw moving average
            If Me.MovingAverage.Count > 1 Then
                Dim maxY As Double = Me.MovingAverage.Max()
                Dim minY As Double = Me.MovingAverage.Min()
                Dim rangeY As Double = maxY - minY
                Dim scaleX As Double = bounds.Width / (Me.MovingAverage.Count - 1)
                Dim scaleY As Double = If(rangeY <> 0, bounds.Height / rangeY, 1)

                ' Draw grid lines
                Using pen As New Pen(Color.Gray, 0.5F)
                    For i As Integer = 0 To bounds.Height Step 50
                        g.DrawLine(pen, 0, i, bounds.Width, i)
                    Next
                End Using

                ' Draw moving average line with color gradient
                For i As Integer = 1 To Me.MovingAverage.Count - 1
                    Dim x1 As Single = CSng((i - 1) * scaleX)
                    Dim y1 As Single = CSng(bounds.Height - (Me.MovingAverage(i - 1) - minY) * scaleY)
                    Dim x2 As Single = CSng(i * scaleX)
                    Dim y2 As Single = CSng(bounds.Height - (Me.MovingAverage(i) - minY) * scaleY)

                    ' Create a color gradient from blue (low) to red (high)
                    Static half As Integer = bounds.Height \ 2
                    Dim color As Color = Color.FromArgb(255, CInt(255 * (y2 / bounds.Height)), 0, CInt(255 * (1 - y2 / bounds.Height)))
                    Using pen As New Pen(color, 1)
                        g.DrawLine(pen, x1, y1, x2, y2)
                        If (i Mod 4 = 0) Then
                            Dim label As String = String.Format("{0:F2}", Me.m_avg)
                            g.DrawString(label, device.Font, Brushes.White, x2, half)
                        End If
                    End Using
                Next
            End If
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return String.Format("Moving Average({0}/{1:E2})", Me.Period, Me.m_avg)
            End Get
        End Property
    End Class
End Namespace

