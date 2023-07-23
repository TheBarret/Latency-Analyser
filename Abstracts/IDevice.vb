Namespace Abstracts
    Public MustInherit Class IDevice

        Public Property Hub As Hub
        Public Property Font As Font
        Public Property Output As IOutput
        Public Property Real As Complex()

        Sub New(hub As Hub, output As IOutput)
            Me.Hub = hub
            Me.Output = output
            Me.Font = New Font("Consolas", 8)
        End Sub

        Public Overridable Sub Reset()

        End Sub
        Public Overridable Sub Update(data As List(Of Double))

        End Sub
        Public Overridable Sub Render(bm As Bitmap, g As Graphics, bounds As Rectangle)

        End Sub
    End Class
End Namespace