
Namespace Abstracts
    Public MustInherit Class IOutput
        Public Property Theme As Theme
        Public Property Data As List(Of Double)
        Public Property WFunction As WFunction

        Sub New()
            Me.Theme = New Theme
            Me.Data = New List(Of Double)
        End Sub

        Public Sub Load(data As List(Of Double))
            Me.Data = data
        End Sub

        Public MustOverride Sub Render(device As IDevice, g As Graphics, bounds As Rectangle)
        Public MustOverride ReadOnly Property Name As String

    End Class
    End Namespace