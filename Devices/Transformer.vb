Imports System.Drawing.Drawing2D
Imports Machine.Abstracts

Namespace Devices
    Public Class Transformer
        Inherits IDevice
        Public Const Delay As Integer = 1

        Sub New(hub As Hub, output As IOutput)
            MyBase.New(hub, output)
        End Sub

        Public Overrides Sub Update(data As List(Of Double))
            Me.Output.Load(data)
        End Sub

        Public Overrides Sub Render(bm As Bitmap, g As Graphics, bounds As Rectangle)
            Dim sw As New Stopwatch
            sw.Start()
            Me.Output.Render(Me, g, bounds)
            g.DrawRectangle(Pens.Black, bounds)
            sw.Stop()
            Dim label As String = String.Format("{0} » {1}({2}ms)", Me.Hub.Provider.Name, Me.Output.GetType.Name, sw.ElapsedMilliseconds)
            Dim width As Single = g.MeasureString(label, Me.Font).Width + 5
            Effects.DrawLabel(g, bounds.Width - width, 0, Me.Font, Color.Black, label)
        End Sub



        ''' <summary>
        ''' Convert bin number to frequency in Hertz
        ''' </summary>
        Public Shared Function GetFrequency(bin As Integer, samplerate As Integer, bins As Integer) As Double
            Return bin * (samplerate / 2) / bins
        End Function

        ''' <summary>
        ''' Returns the corresponding heat color from a min/max range
        ''' </summary>
        Public Shared Function GetHeatmap(value As Double, ParamArray map() As Color) As Color
            ' Normalize value to be between 0 and 1
            value = Math.Min(1.0, Math.Max(0.0, value))

            ' Calculate the section size and which section the value falls into
            Dim sectionSize As Double = 1.0 / (map.Length - 1)
            Dim sectionIndex As Integer = CInt(Math.Min(Math.Floor(value / sectionSize), map.Length - 2))

            ' Get the two colors that define the section
            Dim color1 As Color = map(sectionIndex)
            Dim color2 As Color = map(sectionIndex + 1)

            ' Calculate the ratio within the section
            Dim ratio As Double = (value - sectionSize * sectionIndex) / sectionSize

            ' Linearly interpolate between the colors
            Dim red As Integer = CInt((1 - ratio) * color1.R + ratio * color2.R)
            Dim green As Integer = CInt((1 - ratio) * color1.G + ratio * color2.G)
            Dim blue As Integer = CInt((1 - ratio) * color1.B + ratio * color2.B)

            Return Color.FromArgb(red, green, blue)
        End Function

        ''' <summary>
        ''' Detects peaks automatically, limited by amount
        ''' </summary>
        Public Shared Function DetectPeaks(data As Complex(), Optional count As Integer = -1) As List(Of Highlight)
            Dim peaks As New List(Of Highlight)
            Dim dataReals As IEnumerable(Of Double) = data.Select(Function(c) c.Magnitude).ToArray()

            ' Perform a moving average to smooth the data
            Dim smoothedData As Double() = Transformer.MovingAverage(dataReals)

            ' Loop over the smoothed data, comparing each value to its neighbors
            Dim peakStartIndex As Integer = -1
            For i As Integer = 1 To smoothedData.Length - 2
                ' If the current value is greater than both its neighbors and a new peak starts
                If smoothedData(i - 1) < smoothedData(i) And smoothedData(i) > smoothedData(i + 1) Then
                    peakStartIndex = i
                End If
                ' If a peak was previously started, and the current value starts to decrease
                ' or it's the last data point, the peak has ended, calculate the width and add it as a peak
                If peakStartIndex <> -1 AndAlso ((smoothedData(i) <= smoothedData(i + 1)) OrElse i = smoothedData.Length - 2) Then
                    Dim peakWidth As Integer = i - peakStartIndex + 1
                    Dim peakMagnitude As Double = dataReals.Skip(peakStartIndex).Take(peakWidth).Max()
                    peaks.Add(New Highlight(peakStartIndex, peakMagnitude, peakWidth))
                    peakStartIndex = -1
                End If
            Next

            ' Sort the peaks in descending order of magnitude
            peaks.Sort(Function(a, b) b.Magnitude.CompareTo(a.Magnitude))

            ' Take limited amount if set
            If (count <> -1 AndAlso peaks.Count > count) Then
                peaks = peaks.Take(count).ToList
            End If
            ' Take maximum if exceeding
            If (peaks.Count > Byte.MaxValue) Then
                peaks = peaks.Take(Byte.MaxValue).ToList
            End If

            Return peaks
        End Function

        ''' <summary>
        ''' Calculates the moving average of the array
        ''' </summary>
        Public Shared Function MovingAverage(data As IEnumerable(Of Double), Optional length As Integer = 1) As Double()
            Dim result As New List(Of Double)
            Dim queue As New Queue(Of Double)(length)
            For Each point In data
                If queue.Count >= length Then
                    queue.Dequeue()
                End If
                queue.Enqueue(point)
                result.Add(queue.Average())
            Next
            Return result.ToArray()
        End Function

        ''' <summary>
        ''' Double to Complex Convertor
        ''' </summary>
        Public Shared Function Wrap(data As List(Of Double), Optional filter As WFunction = WFunction.None) As Complex()
            Dim buffer As Complex() = New Complex(data.Count - 1) {}
            For i As Integer = 0 To data.Count - 1
                buffer(i) = New Complex(data(i), 0)
            Next
            Select Case filter
                Case WFunction.Hanning : Transformer.Hann(buffer)
                Case WFunction.Blackman : Transformer.Blackman(buffer)
                Case WFunction.Bartlett : Transformer.Bartlett(buffer)
            End Select
            Return buffer
        End Function

        ''' <summary>
        ''' Fast Fourier Transformer
        ''' </summary>
        Public Shared Function Transform(frame As Complex()) As Complex()
            Dim N As Integer = frame.Length
            If N = 1 Then Return frame
            Dim even(N \ 2 - 1) As Complex
            Dim odd(N \ 2 - 1) As Complex
            For i As Integer = 0 To N - 1 Step 2
                even(i \ 2) = frame(i)
                odd(i \ 2) = frame(i + 1)
            Next
            Dim evenR As Complex() = Transformer.Transform(even)
            Dim oddR As Complex() = Transformer.Transform(odd)
            Dim combined(N - 1) As Complex
            For k As Integer = 0 To N \ 2 - 1
                Dim factor As Complex = New Complex(Math.Cos(-2 * Math.PI * k / N), Math.Sin(-2 * Math.PI * k / N))
                combined(k) = evenR(k) + factor * oddR(k)
                combined(k + N \ 2) = evenR(k) - factor * oddR(k)
            Next
            Return combined
        End Function

        ''' <summary>
        ''' Postprocessor: Bartlett (Triangular) Window
        ''' </summary>
        Public Shared Sub Bartlett(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (1 - Math.Abs((i - (buffer.Count - 1) / 2.0) / (buffer.Count / 2.0)))
            Next
        End Sub

        ''' <summary>
        '''  Postprocessor: Hanning (or Hann) Window
        ''' </summary>
        Public Shared Sub Hann(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * 0.5 * (1 - Math.Cos(2 * Math.PI * i / (buffer.Count - 1)))
            Next
        End Sub
        ''' <summary>
        ''' Postprocessor: Blackman Window
        ''' </summary>
        Public Shared Sub Blackman(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (0.42 - 0.5 * Math.Cos(2 * Math.PI * i / (buffer.Count - 1)) + 0.08 * Math.Cos(4 * Math.PI * i / (buffer.Count - 1)))
            Next
        End Sub

        ''' <summary>
        ''' Draws a grid on bitmap object defined by; dx and dy divisions and the style
        ''' </summary>
        Public Shared Sub Grid(g As Graphics, bounds As Rectangle, dx As Integer, dy As Integer, tint As Color, transparency As Integer, border As Integer)
            Using gpen As New Pen(Color.FromArgb(transparency, tint), border) With {.DashStyle = DashStyle.Dash}
                For i As Integer = 1 To dx
                    Dim x As Single = i * bounds.Width \ dx
                    g.DrawLine(gpen, x, 0, x, bounds.Height)
                Next
                For i As Integer = 1 To dy
                    Dim y As Single = i * bounds.Height \ dy
                    g.DrawLine(gpen, 0, y, bounds.Width, y)
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Draws a nice label box with title in corner
        ''' </summary>
        Public Shared Sub DrawTitle(g As Graphics, font As Font, title As String)
            g.DrawString(title, font, Brushes.Black, 1, 0)
        End Sub

        ''' <summary>
        ''' Returns a human-readable format of a size unit
        ''' </summary>
        Public Shared Function ToReadableFormat(byteCount As Integer) As String
            Static table() As String = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}
            If byteCount = 0 Then Return "0 bytes"
            Dim log As Double = Math.Floor(Math.Log(byteCount, 1024))
            Dim index As Integer = Convert.ToInt32(Math.Min(table.Length - 1, log))
            Dim num As Double = Math.Round(byteCount / Math.Pow(1024, log))
            Return String.Format("{0} {1}", num, table(index))
        End Function


        ''' <summary>
        ''' Returns the division according to the buffer size
        ''' </summary>
        Public Shared ReadOnly Property Divisions(buffer As Complex()) As Integer
            Get
                Return Transformer.Divisions(buffer.Length)
            End Get
        End Property

        ''' <summary>
        ''' Returns the division according to the buffer size
        ''' </summary>
        Public Shared ReadOnly Property Divisions(length As Integer) As Integer
            Get
                Select Case length \ 2
                    Case 64 : Return 8
                    Case 128 : Return 16
                    Case 256 : Return 32
                    Case 512 : Return 64
                    Case 1024 : Return 128
                    Case 2048 : Return 256
                    Case Else : Return -1
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Default heatmap colors (Matlab Jet)
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Jetcolor() As Color()
            Get
                Static colors As Color() = New Color() {
                        Color.FromArgb(143, 0, 0),     ' Dark Red
                        Color.FromArgb(255, 0, 0),     ' Red
                        Color.FromArgb(255, 127, 0),   ' Orange
                        Color.FromArgb(255, 255, 0),   ' Yellow
                        Color.FromArgb(0, 255, 0),     ' Green
                        Color.FromArgb(0, 255, 255),   ' Cyan (Light Blue)
                        Color.FromArgb(0, 0, 255),     ' Blue
                        Color.FromArgb(0, 0, 0)      ' Black
                }
                Return colors
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "Transformer"
        End Function
    End Class
End Namespace