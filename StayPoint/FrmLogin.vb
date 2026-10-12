
Imports System
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class FrmLogin

    '========================================================
    ' AUTHENTICATED USER SESSION
    '========================================================

    Public Shared CurrentUserId As Integer = 0

    '========================================================
    ' FORM LOAD
    '========================================================

    Private Sub FrmLogin_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        txtPassword.UseSystemPasswordChar = True

        Me.AcceptButton = btnLogin
        Me.CancelButton = btnExit
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Normal

    End Sub

    '========================================================
    ' SHOW / HIDE PASSWORD
    '========================================================

    Private Sub chkShowPassword_CheckedChanged(
        sender As Object,
        e As EventArgs
    ) Handles chkShowPassword.CheckedChanged

        txtPassword.UseSystemPasswordChar =
            Not chkShowPassword.Checked

    End Sub

    '========================================================
    ' DASHBOARD CLOSED
    '========================================================

    Private Sub Dashboard_FormClosed(
        sender As Object,
        e As FormClosedEventArgs
    )

        If Me.IsDisposed Then Return

        CurrentUserId = 0

        Me.Show()
        txtPassword.Clear()
        txtUsername.Focus()

    End Sub

    '========================================================
    ' LOGIN / AUTHENTICATION
    '========================================================

    Private Sub btnLogin_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnLogin.Click

        Dim username As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Text

        If String.IsNullOrWhiteSpace(username) OrElse
           String.IsNullOrEmpty(password) Then

            MessageBox.Show(
                "Please enter both username and password.",
                "Required Fields",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Return
        End If

        Try
            Dim userId As Integer = 0
            Dim storedHash As String = Nothing
            Dim role As String = Nothing
            Dim isActive As Boolean = False

            '------------------------------------------------
            ' RETRIEVE USER ACCOUNT
            '------------------------------------------------

            Using connection As MySqlConnection =
                DbConnection.GetConnection()

                connection.Open()

                Const sql As String =
                    "SELECT user_id, password_hash, role, is_active " &
                    "FROM users " &
                    "WHERE username = @username " &
                    "LIMIT 1;"

                Using command As New MySqlCommand(sql, connection)

                    command.Parameters.AddWithValue(
                        "@username", username)

                    Using reader As MySqlDataReader =
                        command.ExecuteReader()

                        If reader.Read() Then

                            If Not reader.IsDBNull(
                                reader.GetOrdinal("user_id")) Then

                                userId = Convert.ToInt32(
                                    reader("user_id"))

                            End If

                            If Not reader.IsDBNull(
                                reader.GetOrdinal("password_hash")) Then

                                storedHash =
                                    reader("password_hash").ToString()

                            End If

                            If Not reader.IsDBNull(
                                reader.GetOrdinal("role")) Then

                                role = reader("role").ToString()

                            End If

                            If Not reader.IsDBNull(
                                reader.GetOrdinal("is_active")) Then

                                isActive =
                                    Convert.ToBoolean(
                                        reader("is_active"))

                            End If

                        End If

                    End Using
                End Using
            End Using

            '------------------------------------------------
            ' VERIFY PASSWORD
            '------------------------------------------------

            If userId <= 0 OrElse
               String.IsNullOrEmpty(storedHash) OrElse
               Not PasswordSecurity.VerifyPassword(
                   password, storedHash) Then

                MessageBox.Show(
                    "Invalid username or password.",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

                txtPassword.Clear()
                txtPassword.Focus()
                Return

            End If

            '------------------------------------------------
            ' CHECK ACCOUNT STATUS
            '------------------------------------------------

            If Not isActive Then

                MessageBox.Show(
                    "This account is inactive. Contact your administrator.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )

                txtPassword.Clear()
                txtPassword.Focus()
                Return

            End If

            '------------------------------------------------
            ' SELECT DASHBOARD BY ROLE
            '------------------------------------------------

            Dim dashboard As Form = Nothing

            Select Case If(role, String.Empty).
                Trim().ToUpperInvariant()

                Case "ADMIN"

                    dashboard = New FrmAdminDashboard()

                Case "STAFF"

                    dashboard = New FrmStaffDashboard()

                Case Else

                    MessageBox.Show(
                        "This account has an invalid role.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    )

                    Return

            End Select

            '------------------------------------------------
            ' SET AUTHENTICATED USER ID
            '------------------------------------------------

            CurrentUserId = userId

            '------------------------------------------------
            ' REGISTER DASHBOARD CLOSE EVENT
            '------------------------------------------------

            AddHandler dashboard.FormClosed,
                AddressOf Dashboard_FormClosed

            '------------------------------------------------
            ' OPEN DASHBOARD
            '------------------------------------------------

            Try
                Me.Hide()
                dashboard.Show()

            Catch ex As Exception

                RemoveHandler dashboard.FormClosed,
                    AddressOf Dashboard_FormClosed

                CurrentUserId = 0

                If Not Me.IsDisposed Then
                    Me.Show()
                End If

                dashboard.Dispose()

                MessageBox.Show(
                    "Unable to open the dashboard." &
                    Environment.NewLine &
                    ex.Message,
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

            End Try

        Catch ex As MySqlException

            MessageBox.Show(
                "Unable to access the database. " &
                "Check XAMPP and your connection settings." &
                Environment.NewLine &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        Catch ex As Exception

            MessageBox.Show(
                "An unexpected error occurred during login." &
                Environment.NewLine &
                ex.Message,
                "System Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        End Try

    End Sub

    '========================================================
    ' EXIT APPLICATION
    '========================================================

    Private Sub btnExit_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnExit.Click

        Dim result As DialogResult =
            MessageBox.Show(
                "Are you sure you want to exit StayPoint HMS?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

        If result = DialogResult.Yes Then
            Application.Exit()
        End If

    End Sub

    '========================================================
    ' TEST DATABASE CONNECTION
    '========================================================

    Private Sub btnTestConnection_Click(
        sender As Object,
        e As EventArgs
    ) Handles btnTestConnection.Click

        If DbConnection.TestConnection() Then

            MessageBox.Show(
                "Database connection successful.",
                "Connection Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

        Else

            MessageBox.Show(
                "Could not connect to the database. " &
                "Check XAMPP, the port, and database credentials.",
                "Connection Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

        End If

    End Sub

End Class
