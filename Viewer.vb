Imports Machine.Devices
Imports Machine.Output
Imports Machine.Providers

Public Class Viewer

    Public Property Hub As Hub

    Private Sub Viewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Hub = New Hub(Me.Vp.ClientRectangle, 512, New Latency("kpn.nl"))

        Me.Hub.Devices.Add(New Transformer(Me.Hub, New Fourier(WFunction.Bartlett)))
        Me.Hub.Devices.Add(New Transformer(Me.Hub, New Spectrogram(WFunction.Bartlett)))

        Me.Clock.Interval = 250
        Me.Clock.Start()
        Me.Text = String.Format("Analyzing...")
    End Sub

    Private Sub Clock_Tick(sender As Object, e As EventArgs) Handles Clock.Tick
        If (Not Me.Hub.Busy) Then
            Task.Run(Sub()
                         Me.Hub.Update()
                         Me.CopyImage(Me.Hub.Render)
                     End Sub)
        End If
    End Sub

    Private Sub CopyImage(bm As Image)
        Try
            If (Not Me.IsDisposed And Me.InvokeRequired) Then
                Me.Invoke(Sub() Me.CopyImage(bm))
            Else
                Me.Vp.BackgroundImage = bm
            End If
        Catch
            'Fix: 'IsDisposed?' not working
        End Try
    End Sub

End Class
