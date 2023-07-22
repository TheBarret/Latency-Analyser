Imports System.Net.NetworkInformation

Namespace Utilities
    Public Class TRecord
        Public Property Hostname As String
        Public Property Timing As Double
        Public Property Check As IPStatus

        Sub New(hostname As String, timing As Double, check As IPStatus)
            Me.Hostname = hostname
            Me.Timing = timing
            Me.Check = check
        End Sub

        Public ReadOnly Property Success As Boolean
            Get
                Return Me.Check = IPStatus.Success
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0} [{1}]", Me.Hostname, Me.Check)
        End Function
    End Class
End Namespace