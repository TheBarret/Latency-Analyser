'############################################################################################
'Network Latency And FFT-Derived Delay are distinct types Of delay measurements.

'FFT-Derived Delay (Phase Delay And Group Delay):
'These are derived from the Fourier Transform Of a signal,
'revealing how the phase Of the frequency components changes from one sample To another.
'A rapid phase shift For a particular frequency component could imply delay. 

'Phase Delay: 
'The delay that a specific frequency component Of the signal experiences.

'Group Delay: 
'The derivative Of the phase response With respect To frequency.
'It measures the rate Of change Of the phase delay And Is often considered As a time delay.

'Remark:
'A high FFT-derived delay does Not necessarily imply a high network latency.
'A peak In the FFT-derived delay can highlight a frequency component being significantly present and experiencing delay In the signal.
'However, it's important to note that FFT-derived delay and network latency measure different aspects and are not directly comparable.
'############################################################################################

Public Class Highlight
    Public Property Index As Integer
    Public Property Magnitude As Double
    Public Property Width As Double

    Sub New(index As Integer, magn As Double, width As Double)
        Me.Index = index
        Me.Magnitude = magn
        Me.Width = width
    End Sub

    ''' <summary>
    ''' Returns milliseconds translation from of frequency peak 
    ''' </summary>
    Public Function ToMilliseconds(transformed As Complex()) As Double
        Dim phases As IEnumerable(Of Double) = transformed.Select(Function(c) c.Phase).ToArray()
        Dim unwrapped As Double() = Highlight.Unwrap(phases)
        Dim groupDelays = New Double(unwrapped.Length - 2) {}
        For i = 1 To unwrapped.Length - 2
            groupDelays(i) = (unwrapped(i + 1) - unwrapped(i - 1)) / 2
        Next
        Return groupDelays(Me.Index) / (2 * Math.PI) * 1000 'convert to milliseconds
    End Function

    ''' <summary>
    ''' Phase Mitigation Unwrapper
    ''' Phase unwrapping tries to mitigate the phase discontinuities (Jumps from -π to π or from π to -π), 
    ''' by adding or subtracting 2π where a discontinuity is detected. 
    ''' This produces a phase response that is strictly increasing or decreasing, making the derivative (the group delay) more meaningful.
    ''' </summary>
    Public Shared Function Unwrap(phase As IEnumerable(Of Double)) As Double()
        Dim unwrapped As Double() = New Double(phase.Count - 1) {}
        unwrapped(0) = phase(0)
        For i As Integer = 1 To phase.Count - 1
            Dim d = phase(i) - phase(i - 1)
            d -= 2 * Math.PI * Math.Round(d / (2 * Math.PI))
            unwrapped(i) = unwrapped(i - 1) + d
        Next
        Return unwrapped
    End Function

End Class
