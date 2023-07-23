Imports System.Drawing.Drawing2D
Imports Machine.Abstracts

Namespace Devices
    Public Class Transformer
        Inherits IDevice
        Public Const Delay As Integer = 1
        Public Shared Property Beta As Double = 0.5R
        Public Shared Property Sigma As Double = 0.4R

        Sub New(hub As Hub, output As IOutput)
            MyBase.New(hub, output)
        End Sub

        ''' <summary>
        ''' Updates the transformer with new data and loads it into the output driver.
        ''' </summary>
        ''' <param name="data">The new data to be processed.</param>
        Public Overrides Sub Update(data As List(Of Double))
            Me.Real = Transformer.Transform(Transformer.Wrap(data, Me.Output.WFunction))
            Me.Output.Load(data)
        End Sub

        ''' <summary>
        ''' Renders the visual representation of the data and additional information.
        ''' </summary>
        ''' <param name="bm">The bitmap to render on.</param>
        ''' <param name="g">The graphics object to use for rendering.</param>
        ''' <param name="bounds">The bounds of the rendering area.</param>
        Public Overrides Sub Render(bm As Bitmap, g As Graphics, bounds As Rectangle)
            Dim sw As New Stopwatch
            sw.Start()
            Me.Output.Render(Me, g, bounds)
            g.DrawRectangle(Pens.Black, bounds)
            sw.Stop()
            Dim label As String = String.Format("{0} » WFunc({1}) » {2}({3}ms)", Me.Hub.Provider.Name, Me.Output.WFunction, Me.Output.Name, sw.ElapsedMilliseconds)
            Dim width As Single = g.MeasureString(label, Me.Font).Width + 5
            Effects.DrawLabel(g, bounds.Width - width, 0, Me.Font, Color.Black, label)
        End Sub

        ''' <summary>
        ''' Converts a bin number to its corresponding frequency in Hertz.
        ''' </summary>
        ''' <param name="bin">The bin number.</param>
        ''' <param name="samplerate">The sample rate of the data.</param>
        ''' <param name="bins">The total number of bins.</param>
        ''' <returns>The frequency in Hertz.</returns>
        Public Shared Function GetFrequency(bin As Integer, samplerate As Integer, bins As Integer) As Double
            Return bin * (samplerate / 2) / bins
        End Function

        ''' <summary>
        ''' Converts a value to a color on a heatmap.
        ''' </summary>
        ''' <param name="value">The value to convert, expected to be between 0 and 1.</param>
        ''' <param name="map">The color map to use for the conversion.</param>
        ''' <returns>The color corresponding to the value on the heatmap.</returns>
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
        ''' Detects peaks in the data using a simple local maximum method.
        ''' </summary>
        ''' <param name="data">The data to detect peaks in.</param>
        ''' <param name="count">The maximum number of peaks to detect.</param>
        ''' <returns>A list of detected peaks.</returns>
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
        ''' Calculates the moving average of the data.
        ''' </summary>
        ''' <param name="data">The data to calculate the moving average of.</param>
        ''' <param name="length">The length of the moving average window.</param>
        ''' <returns>The moving average of the data.</returns>
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
        ''' Wraps the data into a complex number array and applies a window function.
        ''' </summary>
        ''' <param name="data">The data to wrap.</param>
        ''' <param name="filter">The window function to apply.</param>
        ''' <returns>The wrapped data.</returns>
        Public Shared Function Wrap(data As List(Of Double), Optional filter As WFunction = WFunction.None) As Complex()
            Dim buffer As Complex() = New Complex(data.Count - 1) {}
            For i As Integer = 0 To data.Count - 1
                buffer(i) = New Complex(data(i), 0)
            Next
            Select Case filter
                Case WFunction.BSpline : Transformer.BSpline(buffer, 2)
                Case WFunction.Parzen : Transformer.Parzen(buffer, 1)
                Case WFunction.Polynomial : Transformer.Polynomial(buffer)
                Case WFunction.FlatTop : Transformer.FlatTop(buffer)
                Case WFunction.NuttallBm : Transformer.BlackmanNuttall(buffer)
                Case WFunction.PlanckTaper : Transformer.PlanckTaper(buffer, 0.01)
                Case WFunction.Tukey : Transformer.Tukey(buffer, 0.1)
                Case WFunction.Hanning : Transformer.Hann(buffer)
                Case WFunction.Blackman : Transformer.Blackman(buffer)
                Case WFunction.Bartlett : Transformer.Bartlett(buffer)
                Case WFunction.Gaussian : Transformer.Gaussian(buffer)
                Case WFunction.Kaiser : Transformer.Kaiser(buffer)
            End Select
            Return buffer
        End Function

        ''' <summary>
        ''' Performs a Fast Fourier Transform on the data.
        ''' </summary>
        ''' <param name="frame">The data to transform.</param>
        ''' <returns>The Fourier transformed data.</returns>
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
                Dim factor As New Complex(Math.Cos(-2 * Math.PI * k / N), Math.Sin(-2 * Math.PI * k / N))
                combined(k) = evenR(k) + factor * oddR(k)
                combined(k + N \ 2) = evenR(k) - factor * oddR(k)
            Next
            Return combined
        End Function

        ' L:  1,2
        Public Shared Sub BSpline(buffer As Complex(), L As Integer)
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (1 - Math.Abs((i - buffer.Count / 2.0) / (L / 2.0)))
            Next
        End Sub

        ' L: 1
        Public Shared Sub Parzen(buffer As Complex(), L As Integer)
            For i As Integer = 0 To buffer.Count - 1
                Dim n As Double = i - buffer.Count / 2.0
                If Math.Abs(n) <= L / 4.0 Then
                    buffer(i).Real = buffer(i).Real * (1 - 6 * (n / (L / 2.0)) ^ 2 * (1 - Math.Abs(n) / (L / 2.0)))
                Else
                    buffer(i).Real = buffer(i).Real * (2 * (1 - Math.Abs(n) / (L / 2.0)) ^ 3)
                End If
            Next
        End Sub

        Public Shared Sub Polynomial(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (1 - ((i - buffer.Count / 2.0) / (buffer.Count / 2.0)) ^ 2)
            Next
        End Sub

        Public Shared Sub BlackmanNuttall(buffer As Complex())
            Static a0 As Double = 0.3635819
            Static a1 As Double = 0.4891775
            Static a2 As Double = 0.1365995
            Static a3 As Double = 0.0106411
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (a0 - a1 * Math.Cos(2 * Math.PI * i / buffer.Count) + a2 * Math.Cos(4 * Math.PI * i / buffer.Count) - a3 * Math.Cos(6 * Math.PI * i / buffer.Count))
            Next
        End Sub

        Public Shared Sub FlatTop(buffer As Complex())
            Static a0 As Double = 0.21557895
            Static a1 As Double = 0.41663158
            Static a2 As Double = 0.277263158
            Static a3 As Double = 0.083578947
            Static a4 As Double = 0.006947368
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = buffer(i).Real * (a0 - a1 * Math.Cos(2 * Math.PI * i / buffer.Count) + a2 * Math.Cos(4 * Math.PI * i / buffer.Count) - a3 * Math.Cos(6 * Math.PI * i / buffer.Count) + a4 * Math.Cos(8 * Math.PI * i / buffer.Count))
            Next
        End Sub

        Public Shared Sub Tukey(buffer As Complex(), alpha As Double)
            For i As Integer = 0 To buffer.Count - 1
                If i < alpha * buffer.Count / 2 Then
                    buffer(i).Real = buffer(i).Real * 0.5 * (1 - Math.Cos(2 * Math.PI * i / (alpha * buffer.Count)))
                ElseIf i <= buffer.Count / 2 Then
                    buffer(i).Real = buffer(i).Real * 1
                Else
                    buffer(buffer.Count - i).Real = buffer(i).Real
                End If
            Next
        End Sub

        Public Shared Sub PlanckTaper(buffer As Complex(), epsilon As Double)
            For i As Integer = 0 To buffer.Count - 1
                If i < epsilon * buffer.Count Then
                    buffer(i).Real = buffer(i).Real * (1 + Math.Exp(epsilon * buffer.Count / i - epsilon * buffer.Count / (epsilon * buffer.Count - i))) ^ -1
                ElseIf i <= buffer.Count / 2 Then
                    buffer(i).Real = buffer(i).Real * 1
                Else
                    buffer(buffer.Count - i).Real = buffer(i).Real
                End If
            Next
        End Sub


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
        ''' Postprocessor: Gaussian Window, Utilities.Sigma is a parameter you can adjust, default is: 0.4
        ''' </summary>
        Public Shared Sub Gaussian(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = CSng(buffer(i).Real * Math.Exp(-0.5 * Math.Pow((i - (buffer.Count - 1) / 2.0) / (Transformer.Sigma * (buffer.Count - 1) / 2.0), 2)))
            Next
        End Sub

        ''' <summary>
        ''' Postprocessor: Kaiser Window, Utilities.Beta is a parameter you can adjust, default is: 0.5
        ''' </summary>
        Public Shared Sub Kaiser(buffer As Complex())
            For i As Integer = 0 To buffer.Count - 1
                buffer(i).Real = CSng(buffer(i).Real * (Transformer.I0(Transformer.Beta * Math.Sqrt(1 - Math.Pow(2.0 * i / (buffer.Count - 1) - 1, 2))) / Transformer.I0(Transformer.Beta)))
            Next
        End Sub

        ''' <summary>
        ''' Simplified Modified Bessel function: I0(x) = (x^(2n))/((2^n * n!)^2) for n going from 0 to infinity.
        ''' Defined by an infinite series: I0(x) = 1 + (x^2)/4 + (x^4)/(64*2!) + (x^6)/(2304*3!) + (x^8)/(147456*4!)
        ''' </summary>
        Public Shared Function I0(x As Double) As Double
            Dim sum As Double = 1.0
            Dim term As Double = 1.0
            Dim squaredX As Double = x * x
            Dim denominator As Double = 1.0
            For i As Integer = 1 To 25
                denominator *= i * i
                term *= squaredX / (4 * i * i)
                sum += term
            Next
            Return sum
        End Function

        ''' <summary>
        ''' Draws a grid on a bitmap object.
        ''' </summary>
        ''' <param name="g">The graphics object to use for drawing.</param>
        ''' <param name="bounds">The bounds of the drawing area.</param>
        ''' <param name="dx">The number of divisions in the x direction.</param>
        ''' <param name="dy">The number of divisions in the y direction.</param>
        ''' <param name="tint">The color of the grid lines.</param>
        ''' <param name="transparency">The transparency of the grid lines.</param>
        ''' <param name="border">The width of the grid lines.</param>
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
        ''' Draws a title on a bitmap object.
        ''' </summary>
        ''' <param name="g">The graphics object to use for drawing.</param>
        ''' <param name="font">The font to use for the title.</param>
        ''' <param name="title">The title to draw.</param>
        Public Shared Sub DrawTitle(g As Graphics, font As Font, title As String)
            g.DrawString(title, font, Brushes.Black, 1, 0)
        End Sub

        ''' <summary>
        ''' Converts a byte count to a human-readable format.
        ''' </summary>
        ''' <param name="byteCount">The byte count to convert.</param>
        ''' <returns>The human-readable format of the byte count.</returns>
        Public Shared Function ToReadableFormat(byteCount As Integer) As String
            Static table() As String = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}
            If byteCount = 0 Then Return "0 bytes"
            Dim log As Double = Math.Floor(Math.Log(byteCount, 1024))
            Dim index As Integer = Convert.ToInt32(Math.Min(table.Length - 1, log))
            Dim num As Double = Math.Round(byteCount / Math.Pow(1024, log))
            Return String.Format("{0} {1}", num, table(index))
        End Function

        ''' <summary>
        ''' Very basic normalizer
        ''' </summary>
        Public Shared Function Normalize(ByRef max As Double, value As Double) As Single
            If (value > max) Then max = value
            Return CSng(value / max)
        End Function

        ''' <summary>
        ''' Returns a random value between a min/max range
        ''' </summary>
        Public Shared Function Range(min As Single, max As Single, Optional seed As Integer = 0) As Single
            Return CSng((max - min) * Transformer.Randomizer(seed).NextDouble + min)
        End Function

        ''' <summary>
        ''' Returns the randomizer instance
        ''' </summary>
        Public Shared ReadOnly Property Randomizer(Optional seed As Integer = 0) As Random
            Get
                Static r As New Random(DateTime.Now.Millisecond + seed)
                Return r
            End Get
        End Property

        ''' <summary>
        ''' Gets the adjusted division according to the buffer size.
        ''' </summary>
        Public Shared ReadOnly Property Divisions(buffer As Complex()) As Integer
            Get
                Return Transformer.Divisions(buffer.Length)
            End Get
        End Property

        ''' <summary>
        ''' Gets the adjusted division according to the buffer size.
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

        Public Overrides Function ToString() As String
            Return String.Format("Transformer({0})", Me.Output.Name)
        End Function
    End Class
End Namespace