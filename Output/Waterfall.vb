Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports Machine.Abstracts
Imports Machine.Devices

Namespace Output
    Public Class Waterfall
        Inherits IOutput
        Public Property History As List(Of List(Of Double))
        Sub New(wfunc As WFunction)
            Me.WFunction = wfunc
            Me.History = New List(Of List(Of Double))
        End Sub

        Public Overrides Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)

            ' Add the latest spectrum data to the history
            Dim buffer As New List(Of Double)
            buffer.AddRange(device.Real.Select(Function(X) X.Magnitude))

            ' Set first frame
            Me.History.Insert(0, buffer)

            ' Remove the oldest spectrum data if the history is too long
            If Me.History.Count > bounds.Height Then
                Me.History.RemoveAt(Me.History.Count - 1)
            End If

            ' Create a new bitmap and lock it for direct manipulation
            Using bitmap As New Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb)
                Dim bData As BitmapData = bitmap.LockBits(New Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat)
                ' Draw each spectrum as a row in the waterfall plot
                For y As Integer = 0 To Math.Min(Me.History.Count, bounds.Height) - 1
                    Dim spectrum As List(Of Double) = Me.History(y)
                    For x As Integer = 0 To (bounds.Width - 1)
                        ' Map the amplitude to a color
                        Dim amplitude As Double = spectrum(x * spectrum.Count \ bounds.Width)
                        Dim color As Color = Transformer.GetHeatmap(amplitude, Me.Theme.Heatmap)
                        ' Set the pixel in the bitmap
                        Dim index As Integer = (y * bData.Stride) + (x * 4)
                        Marshal.WriteByte(bData.Scan0, index, color.B)
                        Marshal.WriteByte(bData.Scan0, index + 1, color.G)
                        Marshal.WriteByte(bData.Scan0, index + 2, color.R)
                        Marshal.WriteByte(bData.Scan0, index + 3, color.A)
                    Next
                Next
                ' Unlock the bitmap and draw it onto the graphics object
                bitmap.UnlockBits(bData)
                g.DrawImage(bitmap, bounds)
            End Using

            ' Draw frequency and time labels
            Dim interval As Integer = bounds.Width \ 8
            Using brush As New SolidBrush(Color.White)
                ' Draw frequency labels along the x axis
                For x As Integer = 0 To bounds.Width - interval Step interval
                    Dim frequency As Double = device.Hub.Samplerate * x / bounds.Width
                    Dim label As String = String.Format("{0:F0}Hz", frequency)
                    Effects.DrawLabel(g, x, bounds.Height - 20, device.Font, Color.Black, label)
                Next
                ' Draw time labels along the y axis
                For y As Integer = 0 To bounds.Height - interval Step interval
                    Dim label As String = String.Format("{0:F1}s", y / device.Hub.Samplerate)
                    Effects.DrawLabel(g, 0, y, device.Font, Color.Black, label)
                Next
            End Using
        End Sub

        Public Overrides ReadOnly Property Name As String
            Get
                Return String.Format("Waterfall({0})", Me.History.Count)
            End Get
        End Property
    End Class
End Namespace

