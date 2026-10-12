
Imports System
Imports System.Security.Cryptography

Public Module PasswordSecurity

    Private Const Iterations As Integer = 150000
    Private Const SaltSize As Integer = 16
    Private Const HashSize As Integer = 32

    Private Const HashPrefix As String = "PBKDF2-SHA1"

    '========================================================
    ' CREATE PASSWORD HASH
    '========================================================
    Public Function CreateHash(password As String) As String

        If password Is Nothing Then
            Throw New ArgumentNullException("password")
        End If

        Dim salt(SaltSize - 1) As Byte

        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Using

        Using pbkdf2 As New Rfc2898DeriveBytes(
            password,
            salt,
            Iterations)

            Dim hash As Byte() = pbkdf2.GetBytes(HashSize)

            Return HashPrefix & "$" &
                Iterations.ToString() & "$" &
                Convert.ToBase64String(salt) & "$" &
                Convert.ToBase64String(hash)

        End Using

    End Function

    '========================================================
    ' VERIFY PASSWORD
    '========================================================
    Public Function VerifyPassword(
        password As String,
        storedHash As String) As Boolean

        If String.IsNullOrEmpty(password) OrElse
           String.IsNullOrWhiteSpace(storedHash) Then
            Return False
        End If

        Try
            Dim parts As String() = storedHash.Split("$"c)

            If parts.Length <> 4 OrElse
               parts(0) <> HashPrefix Then
                Return False
            End If

            Dim iterations As Integer

            If Not Integer.TryParse(parts(1), iterations) OrElse
               iterations < 100000 OrElse
               iterations > 1000000 Then
                Return False
            End If

            Dim salt As Byte() =
                Convert.FromBase64String(parts(2))

            Dim expected As Byte() =
                Convert.FromBase64String(parts(3))

            If salt.Length <> SaltSize OrElse
               expected.Length <> HashSize Then
                Return False
            End If

            Using pbkdf2 As New Rfc2898DeriveBytes(
                password,
                salt,
                iterations)

                Dim actual As Byte() =
                    pbkdf2.GetBytes(expected.Length)

                ' Compare hashes without stopping at the first
                ' differing byte.
                Dim difference As Integer = 0

                For i As Integer = 0 To expected.Length - 1
                    difference = difference Or
                        (actual(i) Xor expected(i))
                Next

                Return difference = 0

            End Using

        Catch ex As FormatException
            Return False

        Catch ex As ArgumentException
            Return False

        Catch ex As CryptographicException
            Return False

        End Try

    End Function

End Module
