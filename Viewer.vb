Imports Machine.Devices
Imports Machine.Output
Imports Machine.Providers

Public Class Viewer

    Public Property Hub As Hub
    Private Sub Viewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Hub = New Hub(Me.Vp.ClientRectangle, 256, New Latency("kpn.nl"))
        Me.Hub.Devices.Add(New Transformer(Me.Hub, New Fourier(WFunction.Polynomial)))
        Me.Hub.Devices.Add(New Transformer(Me.Hub, New Waterfall(WFunction.Polynomial)))
        Me.Hub.Devices.Add(New Transformer(Me.Hub, New Spectrogram(WFunction.Polynomial)))

        Me.Clock.Interval = 250
        Me.Clock.Start()
        Me.Text = String.Format("Analyzing...")
    End Sub

    Private Sub Clock_Tick(sender As Object, e As EventArgs) Handles Clock.Tick
        If (Not Me.Hub.Busy) Then
            Task.Run(AddressOf Me.Worker)
        End If
    End Sub

    Private Sub Worker()
        Me.Hub.Update()
        Me.CopyImage(Me.Hub.Render)
    End Sub

    Private Sub CopyImage(bm As Image)
        If (Me.InvokeRequired) Then
            Try
                Me.Invoke(Sub() Me.CopyImage(bm))
            Catch ex As ObjectDisposedException
                'catch IsDisposed exception
            End Try
        Else
            Me.Vp.BackgroundImage = bm
        End If
    End Sub

End Class
