Imports Machine.Abstracts
Imports System.Drawing.Drawing2D

Public Class Hub
    Public Property Max As Double
    Public Property Counter As Integer
    Public Property Lock As Object
    Public Property Value As Double
    Public Property Padding As Integer
    Public Property Timing As DateTime
    Public Property Busy As Boolean
    Public Property Bounds As Rectangle
    Public Property Samplerate As Integer
    Public Property Provider As IProvider
    Public Property Data As List(Of Double)
    Public Property Devices As List(Of IDevice)

    Sub New(bounds As Rectangle, samplerate As Integer, iprov As IProvider)
        Me.Lock = New Object
        Me.Padding = 1
        Me.Busy = False
        Me.Provider = iprov
        Me.Bounds = bounds
        Me.Samplerate = samplerate
        Me.Timing = DateTime.Now
        Me.Devices = New List(Of IDevice)
        Me.Data = New List(Of Double)(Me.Samplerate)
        Me.Reset()
    End Sub

    Public Sub Reset()
        SyncLock Me.Lock
            Me.Value = 0
            Me.Counter = 0
            Me.Max = 0
            Me.Data.Clear()
            For i As Integer = 0 To Me.Samplerate - 1
                Me.Data.Add(0R)
            Next
        End SyncLock
    End Sub

    Public Sub Update()
        If (Me.Devices.Any) Then
            Dim offset As TimeSpan = DateTime.Now - Me.Timing
            SyncLock Me.Lock
                If (offset.Duration.Seconds >= 1) Then
                    Me.Value = Me.Provider.GetValue(Me)
                    If (Me.Value > 0) Then
                        Me.Data.Add(Me.Normalize(Me.Value))
                        Me.Timing = DateTime.Now
                        Me.Counter += 1
                        If (Me.Data.Count > Me.Samplerate) Then
                            Me.Data.RemoveAt(0)
                        End If
                    End If
                End If
                For i As Integer = 0 To Me.Devices.Count - 1
                    Me.Devices(i).Update(Me.Data)
                Next
            End SyncLock
        End If
    End Sub

    ''' <summary>
    ''' Main render frame
    ''' </summary>
    Public Function Render() As Image
        Me.Busy = True
        Using src As New Bitmap(Me.Bounds.Width, Me.Bounds.Height)
            Using g As Graphics = Graphics.FromImage(src)
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.Clear(Color.White)
                Dim width As Integer = (src.Width - 2 * Me.Padding) - 1
                Dim height As Integer = ((src.Height - ((Me.Devices.Count + 1) * Me.Padding)) \ Me.Devices.Count) - 1
                For i As Integer = 0 To Me.Devices.Count - 1
                    Using frame As New Bitmap(width, height)
                        Using g2 As Graphics = Graphics.FromImage(frame)
                            g2.Clear(Color.Black)
                            g2.SmoothingMode = SmoothingMode.AntiAlias
                            Dim srcrect As New Rectangle(0, 0, frame.Width - Me.Padding, frame.Height - Me.Padding)
                            Me.Devices(i).Render(frame, g2, srcrect)
                        End Using
                        g.DrawImage(CType(frame.Clone, Image), New Rectangle(Me.Padding, Me.Padding + (i * (height + Me.Padding)), width, height))
                    End Using
                Next
            End Using
            Me.Busy = False
            Return CType(src.Clone, Image)
        End Using
    End Function

    ''' <summary>
    ''' Very basic normalizer
    ''' </summary>
    Public Function Normalize(value As Double) As Single
        If (value > Me.Max) Then Me.Max = value
        Return CSng(value / Me.Max)
    End Function

    ''' <summary>
    ''' Returns a random value between a min/max range
    ''' </summary>
    Public Shared Function Range(min As Single, max As Single, Optional seed As Integer = 0) As Single
        Return CSng((max - min) * Hub.Randomizer(seed).NextDouble + min)
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

End Class
