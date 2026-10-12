
Imports MySql.Data.MySqlClient

Public Module DbConnection

    Private ReadOnly connectionString As String =
        "Server=localhost;" &
        "Port=3306;" &
        "Database=staypoint_hms;" &
        "Uid=root;" &
        "Pwd=;"

    Public Function GetConnection() As MySqlConnection
        Return New MySqlConnection(connectionString)
    End Function

    Public Function TestConnection() As Boolean
        Try
            Using connection As MySqlConnection = GetConnection()
                connection.Open()
                Return connection.State = ConnectionState.Open
            End Using
        Catch ex As MySqlException
            Return False
        End Try
    End Function

End Module


