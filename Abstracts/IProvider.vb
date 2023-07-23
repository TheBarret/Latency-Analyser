Namespace Abstracts

    Public MustInherit Class IProvider
        Public MustOverride Sub Reset()
        Public MustOverride Function GetValue(hub As Hub) As Double
        Public MustOverride ReadOnly Property Name As String

    End Class

End Namespace