Imports System.Net.NetworkInformation
Imports Machine.Abstracts
Imports Machine.Utilities

Namespace Providers
    Public Class Latency
        Inherits IProvider

        Public Property Hostname As String
        Private m_value As Double = 0R
        Private m_status As IPStatus = IPStatus.Unknown
        Private m_hops As List(Of TRecord) = New List(Of TRecord)

        Sub New(hostname As String)
            Me.Hostname = hostname
        End Sub

        Public Overrides Sub Reset()
            Me.m_hops.Clear()
        End Sub

        Public Overrides Function GetValue(hub As Hub) As Double
            If (Me.m_hops.Count = 0) Then
                Me.m_hops = Me.Trace(Me.Hostname, 30)
            End If
            Return Me.Transmit
        End Function

        Public Overrides ReadOnly Property Name As String
            Get
                Return String.Format("Latency({0} {1}ms {2} hops {3})", Me.Hostname, Me.m_value, Me.m_hops.Count, Me.m_status)
            End Get
        End Property

        Private Function Transmit() As Double
            Try
                Using icmp As New Ping
                    Dim reply As PingReply = icmp.Send(Me.Hostname, 150, New Byte(0) {0})
                    Me.m_status = reply.Status
                    If (reply.Status = IPStatus.Success) Then
                        Me.m_value = reply.RoundtripTime * 1.0R
                        Return m_value
                    End If
                    Return 0
                End Using
            Catch
                Return 0
            End Try
        End Function

        Private Function Trace(hostname As String, maximum As Integer) As List(Of TRecord)
            Dim results As New List(Of TRecord)
            ' zero-check
            If maximum < 1 Then maximum = 1
            Using ping As New Ping
                Dim options As New PingOptions
                For ttl As Integer = 1 To maximum
                    options.Ttl = ttl
                    Dim reply As PingReply = ping.Send(hostname, 150, New Byte(0) {0}, options)
                    If (reply.Address IsNot Nothing) Then
                        Dim entry As String = reply.Address.ToString
                        If (results.Any) Then
                            If (results.Last.Hostname.Equals(entry)) Then
                                Exit For
                            End If
                        End If
                        results.Add(New TRecord(entry, reply.RoundtripTime, reply.Status))
                    Else
                        results.Add(New TRecord(String.Format("0.0.0.0", reply.Status), -1, IPStatus.Unknown))
                    End If
                Next
            End Using
            Return results
        End Function

    End Class
End Namespace

