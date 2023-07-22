Public Class Complex
    Implements IComparable
    Public Property Real As Double
    Public Property Imaginary As Double

    Sub New(real As Double, imaginary As Double)
        Me.Real = real
        Me.Imaginary = imaginary
    End Sub

    Public Function Magnitude() As Double
        Return Math.Sqrt(Me.Real * Me.Real + Me.Imaginary * Me.Imaginary)
    End Function

    Public Shared Operator +(v As Complex, v2 As Complex) As Complex
        Return New Complex(v.Real + v2.Real, v.Imaginary + v2.Imaginary)
    End Operator

    Public Shared Operator -(v As Complex, v2 As Complex) As Complex
        Return New Complex(v.Real - v2.Real, v.Imaginary - v2.Imaginary)
    End Operator

    ' z = a + bi and w = c + di = z * w = (a*c - b*d) + (a*d + b*c)i
    Public Shared Operator *(v As Complex, v2 As Complex) As Complex
        Return New Complex(v.Real * v2.Real - v.Imaginary * v2.Imaginary, v.Real * v2.Imaginary + v.Imaginary * v2.Real)
    End Operator

    ' z by w = z / w = [(a*c + b*d) / (c^2 + d^2)] + [(b*c - a*d) / (c^2 + d^2)]i
    Public Shared Operator /(v As Complex, v2 As Complex) As Complex
        Dim denom As Double = v2.Real * v2.Real + v2.Imaginary * v2.Imaginary
        Return New Complex((v.Real * v2.Real + v.Imaginary * v2.Imaginary) / denom, (v.Imaginary * v2.Real - v.Real * v2.Imaginary) / denom)
    End Operator

    Public ReadOnly Property Phase() As Double
        Get
            Return Math.Atan2(Me.Imaginary, Me.Real)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{0:F2}/{1:F2} [{2:F2}]", Me.Real, Me.Imaginary, Me.Phase)
    End Function

    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        If Not (TypeOf obj Is Complex) Then Return 0
        Dim other As Complex = CType(obj, Complex)
        If Me.Real < other.Real Or (Me.Real = other.Real And Me.Imaginary < other.Imaginary) Then
            Return -1
        ElseIf Me.Real > other.Real Or (Me.Real = other.Real And Me.Imaginary > other.Imaginary) Then
            Return 1
        Else
            Return 0
        End If
    End Function
End Class
