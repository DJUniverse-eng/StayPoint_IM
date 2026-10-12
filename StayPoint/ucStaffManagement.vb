
Imports System.Data
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffManagement

    '========================================================
    ' COLORS
    '========================================================
    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly Teal As Color =
        Color.FromArgb(22, 160, 145)

    Private ReadOnly PageBackground As Color =
        Color.FromArgb(245, 247, 250)

    Private ReadOnly TextColor As Color =
        Color.FromArgb(45, 55, 72)

    Private ReadOnly MutedColor As Color =
        Color.FromArgb(110, 120, 135)

    Private ReadOnly BorderColor As Color =
        Color.FromArgb(225, 230, 237)

    Private grid As DataGridView
    Private txtSearch As TextBox
    Private cboRoleFilter As ComboBox
    Private lblTotal As Label
    Private lblActive As Label
    Private lblInactive As Label

    '========================================================
    ' INITIALIZE
    '========================================================
    Public Sub New()
        InitializeComponent()
        BuildInterface()
        LoadUsers()
    End Sub

    '========================================================
    ' BUILD INTERFACE
    '========================================================
    Private Sub BuildInterface()

        Me.Controls.Clear()
        Me.BackColor = PageBackground
        Me.Dock = DockStyle.Fill
        Me.Padding = New Padding(22)
        Me.AutoScroll = True

        Dim main As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = PageBackground,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }

        main.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100))

        For i As Integer = 0 To 3
            main.RowStyles.Add(
                New RowStyle(SizeType.AutoSize))
        Next

        '---------------- HEADER ----------------
        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 72,
            .BackColor = PageBackground,
            .Margin = New Padding(0, 0, 0, 10)
        }

        Dim title As New Label With {
            .Text = "Staff Management",
            .Font = New Font("Segoe UI", 21, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 0)
        }

        Dim subtitle As New Label With {
            .Text = "Manage user accounts, access roles, and account status.",
            .Font = New Font("Segoe UI", 9.5F),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(2, 42)
        }

        header.Controls.Add(title)
        header.Controls.Add(subtitle)

        main.Controls.Add(header, 0, 0)

        '---------------- SUMMARY CARDS ----------------
        Dim cards As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 94,
            .ColumnCount = 3,
            .RowCount = 1,
            .BackColor = PageBackground,
            .Margin = New Padding(0, 0, 0, 12)
        }

        For i As Integer = 0 To 2
            cards.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 33.3333F))
        Next

        lblTotal = New Label()
        lblActive = New Label()
        lblInactive = New Label()

        cards.Controls.Add(
            CreateSummaryCard("Total Users", lblTotal, Navy), 0, 0)

        cards.Controls.Add(
            CreateSummaryCard("Active Users", lblActive, Teal), 1, 0)

        cards.Controls.Add(
            CreateSummaryCard(
                "Inactive Users",
                lblInactive,
                Color.FromArgb(190, 105, 55)), 2, 0)

        main.Controls.Add(cards, 0, 1)

        '---------------- TOOLBAR ----------------
        Dim toolbar As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .BackColor = Color.White,
            .Padding = New Padding(10),
            .Margin = New Padding(0, 0, 0, 8)
        }

        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 10),
            .Width = 260,
            .Height = 32,
            .Margin = New Padding(0, 4, 8, 0)
        }

        cboRoleFilter = New ComboBox With {
            .Font = New Font("Segoe UI", 9.5F),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 130,
            .Height = 32,
            .Margin = New Padding(0, 4, 12, 0)
        }

        cboRoleFilter.Items.AddRange(
            New Object() {"All Roles", "ADMIN", "STAFF"})

        cboRoleFilter.SelectedIndex = 0

        Dim btnAdd As Button =
            CreateButton("+ Add User", Teal, Color.White)

        Dim btnEdit As Button =
            CreateButton("Edit User", Navy, Color.White)

        Dim btnStatus As Button =
            CreateButton(
                "Change Status",
                Color.FromArgb(90, 105, 125),
                Color.White)

        Dim btnRefresh As Button =
            CreateButton(
                "Refresh",
                Color.FromArgb(235, 239, 244),
                Navy)

        btnAdd.Width = 120
        btnEdit.Width = 110
        btnStatus.Width = 130
        btnRefresh.Width = 100

        btnAdd.Height = 38
        btnEdit.Height = 38
        btnStatus.Height = 38
        btnRefresh.Height = 38

        btnAdd.Margin = New Padding(0, 0, 8, 0)
        btnEdit.Margin = New Padding(0, 0, 8, 0)
        btnStatus.Margin = New Padding(0, 0, 8, 0)
        btnRefresh.Margin = New Padding(0)

        AddHandler btnAdd.Click, AddressOf AddUser
        AddHandler btnEdit.Click, AddressOf EditUser
        AddHandler btnStatus.Click, AddressOf ChangeUserStatus
        AddHandler btnRefresh.Click, AddressOf RefreshUsers

        toolbar.Controls.Add(txtSearch)
        toolbar.Controls.Add(cboRoleFilter)
        toolbar.Controls.Add(btnAdd)
        toolbar.Controls.Add(btnEdit)
        toolbar.Controls.Add(btnStatus)
        toolbar.Controls.Add(btnRefresh)

        AddHandler txtSearch.TextChanged, AddressOf ApplyFilters

        AddHandler cboRoleFilter.SelectedIndexChanged,
            AddressOf ApplyFilters

        main.Controls.Add(toolbar, 0, 2)

        '---------------- USER TABLE ----------------
        grid = New DataGridView With {
            .Dock = DockStyle.Top,
            .Height = 320,
            .Margin = New Padding(0),
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            .AutoGenerateColumns = False,
            .RowHeadersVisible = False,
            .AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,
            .EnableHeadersVisualStyles = False,
            .GridColor = BorderColor,
            .Font = New Font("Segoe UI", 9.5F),
            .ColumnHeadersHeight = 42
        }

        grid.ColumnHeadersDefaultCellStyle.BackColor = Navy
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9.5F, FontStyle.Bold)

        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White

        grid.DefaultCellStyle.BackColor = Color.White
        grid.DefaultCellStyle.ForeColor = TextColor
        grid.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(220, 242, 239)
        grid.DefaultCellStyle.SelectionForeColor = Navy
        grid.DefaultCellStyle.Padding = New Padding(6, 0, 6, 0)

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        grid.RowTemplate.Height = 38

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "user_id",
            .HeaderText = "User ID",
            .DataPropertyName = "user_id",
            .FillWeight = 55
        })

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "full_name",
            .HeaderText = "Full Name",
            .DataPropertyName = "full_name",
            .FillWeight = 150
        })

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "username",
            .HeaderText = "Username",
            .DataPropertyName = "username",
            .FillWeight = 120
        })

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "role",
            .HeaderText = "Role",
            .DataPropertyName = "role",
            .FillWeight = 75
        })

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "status",
            .HeaderText = "Account Status",
            .DataPropertyName = "status",
            .FillWeight = 95
        })

        grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "created_at",
            .HeaderText = "Date Created",
            .DataPropertyName = "created_at",
            .FillWeight = 125,
            .DefaultCellStyle = New DataGridViewCellStyle With {
                .Format = "MM/dd/yy hh:mm tt"
            }
        })

        AddHandler grid.CellDoubleClick,
            Sub(sender, e)
                If e.RowIndex >= 0 Then
                    EditUser(sender, EventArgs.Empty)
                End If
            End Sub

        main.Controls.Add(grid, 0, 3)

        Me.Controls.Add(main)

    End Sub

    '========================================================
    ' SUMMARY CARD
    '========================================================
    Private Function CreateSummaryCard(
        heading As String,
        valueLabel As Label,
        accent As Color) As Panel

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(0, 0, 12, 0),
            .Padding = New Padding(14)
        }

        AddHandler card.Paint,
            Sub(sender, e)
                Using pen As New Pen(BorderColor)
                    e.Graphics.DrawRectangle(
                        pen, 0, 0, card.Width - 1, card.Height - 1)
                End Using
            End Sub

        Dim headingLabel As New Label With {
            .Text = heading,
            .Font = New Font("Segoe UI", 9.5F),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(14, 12)
        }

        valueLabel.Text = "0"
        valueLabel.Font = New Font("Segoe UI", 21, FontStyle.Bold)
        valueLabel.ForeColor = accent
        valueLabel.AutoSize = True
        valueLabel.Location = New Point(14, 34)

        card.Controls.Add(headingLabel)
        card.Controls.Add(valueLabel)

        Return card

    End Function

    '========================================================
    ' BUTTON STYLE
    '========================================================
    Private Function CreateButton(
        caption As String,
        backColor As Color,
        foreColor As Color) As Button

        Return New Button With {
            .Text = caption,
            .BackColor = backColor,
            .ForeColor = foreColor,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
            .Height = 38,
            .Width = 125,
            .Cursor = Cursors.Hand,
            .Margin = New Padding(0, 0, 8, 0),
            .UseVisualStyleBackColor = False
        }

    End Function

    '========================================================
    ' LOAD USERS
    '========================================================
    Private Sub LoadUsers()

        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Dim sql As String =
                    "SELECT user_id, full_name, username, role, " &
                    "is_active, " &
                    "CASE WHEN is_active = 1 THEN 'Active' " &
                    "ELSE 'Inactive' END AS status, " &
                    "created_at " &
                    "FROM users ORDER BY user_id DESC"

                Using cmd As New MySqlCommand(sql, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using

            End Using

            grid.DataSource = dt

            UpdateSummary(dt)
            ApplyFilters(Nothing, EventArgs.Empty)

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load user accounts." &
                Environment.NewLine & ex.Message,
                "Staff Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try

    End Sub

    '========================================================
    ' UPDATE SUMMARY
    '========================================================
    Private Sub UpdateSummary(dt As DataTable)

        Dim total As Integer = dt.Rows.Count
        Dim active As Integer = 0

        For Each row As DataRow In dt.Rows
            If Not IsDBNull(row("is_active")) AndAlso
               Convert.ToInt32(row("is_active")) = 1 Then

                active += 1
            End If
        Next

        lblTotal.Text = total.ToString()
        lblActive.Text = active.ToString()
        lblInactive.Text = (total - active).ToString()

    End Sub

    '========================================================
    ' SEARCH AND ROLE FILTER
    '========================================================
    Private Sub ApplyFilters(
        sender As Object,
        e As EventArgs)

        If grid Is Nothing OrElse
           grid.DataSource Is Nothing OrElse
           txtSearch Is Nothing OrElse
           cboRoleFilter Is Nothing Then
            Return
        End If

        Dim dt As DataTable =
            TryCast(grid.DataSource, DataTable)

        If dt Is Nothing Then Return

        Dim search As String = txtSearch.Text.Trim()

        ' Escape special characters used by DataView.RowFilter.
        search = search.Replace("[", "[[]")
        search = search.Replace("'", "''")
        search = search.Replace("%", "[%]")
        search = search.Replace("*", "[*]")

        Dim role As String =
            If(cboRoleFilter.SelectedItem Is Nothing,
               "All Roles",
               cboRoleFilter.SelectedItem.ToString())

        Dim filters As New List(Of String)

        If search <> "" Then
            filters.Add(
                "(full_name LIKE '%" & search &
                "%' OR username LIKE '%" & search & "%')")
        End If

        If role <> "All Roles" Then
            filters.Add("role = '" & role.Replace("'", "''") & "'")
        End If

        Try
            dt.DefaultView.RowFilter = String.Join(" AND ", filters)
        Catch ex As EvaluateException
            dt.DefaultView.RowFilter = ""
        End Try

    End Sub

    '========================================================
    ' ADD USER
    '========================================================
    Private Sub AddUser(sender As Object, e As EventArgs)

        Using editor As New UserEditorForm()

            If editor.ShowDialog() <> DialogResult.OK Then Return

            Try
                ' Use the existing PBKDF2 password-hashing module.
                Dim passwordHash As String =
                    PasswordSecurity.CreateHash(editor.Password)

                Using conn As MySqlConnection =
                    DbConnection.GetConnection()

                    If conn.State <> ConnectionState.Open Then
                        conn.Open()
                    End If

                    Dim sql As String =
                        "INSERT INTO users " &
                        "(full_name, username, password_hash, role, is_active) " &
                        "VALUES (@name, @username, @password, @role, @active)"

                    Using cmd As New MySqlCommand(sql, conn)

                        cmd.Parameters.AddWithValue(
                            "@name", editor.FullName)

                        cmd.Parameters.AddWithValue(
                            "@username", editor.Username)

                        cmd.Parameters.AddWithValue(
                            "@password", passwordHash)

                        cmd.Parameters.AddWithValue(
                            "@role", editor.UserRole)

                        cmd.Parameters.AddWithValue(
                            "@active", If(editor.IsActive, 1, 0))

                        cmd.ExecuteNonQuery()

                    End Using
                End Using

                MessageBox.Show(
                    "User account created successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                LoadUsers()

            Catch ex As MySqlException When ex.Number = 1062
                MessageBox.Show(
                    "That username is already in use. Choose another username.",
                    "Duplicate Username",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to create the user account." &
                    Environment.NewLine & ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try

        End Using

    End Sub

    '========================================================
    ' EDIT USER
    '========================================================
    Private Sub EditUser(sender As Object, e As EventArgs)

        If grid.CurrentRow Is Nothing OrElse
           grid.CurrentRow.IsNewRow Then

            MessageBox.Show(
                "Select a user first.",
                "Staff Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Dim row As DataGridViewRow = grid.CurrentRow

        Dim userId As Integer =
            Convert.ToInt32(row.Cells("user_id").Value)

        Dim fullName As String =
            Convert.ToString(row.Cells("full_name").Value)

        Dim username As String =
            Convert.ToString(row.Cells("username").Value)

        Dim userRole As String =
            Convert.ToString(row.Cells("role").Value)

        Dim isActive As Boolean =
            Convert.ToString(row.Cells("status").Value) = "Active"

        Using editor As New UserEditorForm(
            fullName, username, userRole, isActive)

            If editor.ShowDialog() <> DialogResult.OK Then Return

            Try
                ' Generate a compatible hash only if a new password was entered.
                Dim newPasswordHash As String = Nothing

                If editor.Password <> "" Then
                    newPasswordHash =
                        PasswordSecurity.CreateHash(editor.Password)
                End If

                Using conn As MySqlConnection =
                    DbConnection.GetConnection()

                    If conn.State <> ConnectionState.Open Then
                        conn.Open()
                    End If

                    ' Save account details and password together.
                    ' If either update fails, roll back both changes.
                    Using transaction As MySqlTransaction =
                        conn.BeginTransaction()

                        Try
                            Dim sql As String =
                                "UPDATE users SET full_name = @name, " &
                                "username = @username, role = @role, " &
                                "is_active = @active WHERE user_id = @id"

                            Using cmd As New MySqlCommand(
                                sql, conn, transaction)

                                cmd.Parameters.AddWithValue(
                                    "@name", editor.FullName)

                                cmd.Parameters.AddWithValue(
                                    "@username", editor.Username)

                                cmd.Parameters.AddWithValue(
                                    "@role", editor.UserRole)

                                cmd.Parameters.AddWithValue(
                                    "@active", If(editor.IsActive, 1, 0))

                                cmd.Parameters.AddWithValue(
                                    "@id", userId)

                                cmd.ExecuteNonQuery()

                            End Using

                            If newPasswordHash IsNot Nothing Then

                                Dim passwordSql As String =
                                    "UPDATE users SET password_hash = @password " &
                                    "WHERE user_id = @id"

                                Using passwordCmd As New MySqlCommand(
                                    passwordSql, conn, transaction)

                                    passwordCmd.Parameters.AddWithValue(
                                        "@password", newPasswordHash)

                                    passwordCmd.Parameters.AddWithValue(
                                        "@id", userId)

                                    passwordCmd.ExecuteNonQuery()

                                End Using

                            End If

                            transaction.Commit()

                        Catch
                            Try
                                transaction.Rollback()
                            Catch
                                ' Preserve the original exception.
                            End Try

                            Throw
                        End Try

                    End Using
                End Using

                MessageBox.Show(
                    "User account updated successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                LoadUsers()

            Catch ex As MySqlException When ex.Number = 1062
                MessageBox.Show(
                    "That username is already in use.",
                    "Duplicate Username",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to update the user account." &
                    Environment.NewLine & ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try

        End Using

    End Sub

    '========================================================
    ' CHANGE ACCOUNT STATUS
    '========================================================
    Private Sub ChangeUserStatus(
        sender As Object,
        e As EventArgs)

        If grid.CurrentRow Is Nothing OrElse
           grid.CurrentRow.IsNewRow Then

            MessageBox.Show(
                "Select a user first.",
                "Staff Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Dim row As DataGridViewRow = grid.CurrentRow

        Dim userId As Integer =
            Convert.ToInt32(row.Cells("user_id").Value)

        Dim fullName As String =
            Convert.ToString(row.Cells("full_name").Value)

        Dim currentActive As Boolean =
            Convert.ToString(row.Cells("status").Value) = "Active"

        Dim newActive As Boolean = Not currentActive

        Dim actionText As String =
            If(newActive, "activate", "deactivate")

        If MessageBox.Show(
            "Are you sure you want to " & actionText &
            " the account for " & fullName & "?",
            "Confirm Account Status",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then

            Return
        End If

        Try
            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                ' Prevent deactivating the last active administrator.
                If Not newActive AndAlso
                   Convert.ToString(row.Cells("role").Value) = "ADMIN" Then

                    Dim sqlCount As String =
                        "SELECT COUNT(*) FROM users " &
                        "WHERE role = 'ADMIN' AND is_active = 1"

                    Using countCmd As New MySqlCommand(sqlCount, conn)

                        Dim activeAdmins As Integer =
                            Convert.ToInt32(countCmd.ExecuteScalar())

                        If activeAdmins <= 1 Then
                            MessageBox.Show(
                                "The last active administrator cannot be deactivated.",
                                "Action Not Allowed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                            Return
                        End If

                    End Using
                End If

                Dim sql As String =
                    "UPDATE users SET is_active = @active " &
                    "WHERE user_id = @id"

                Using cmd As New MySqlCommand(sql, conn)

                    cmd.Parameters.AddWithValue(
                        "@active", If(newActive, 1, 0))

                    cmd.Parameters.AddWithValue("@id", userId)

                    cmd.ExecuteNonQuery()

                End Using
            End Using

            LoadUsers()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to change account status." &
                Environment.NewLine & ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try

    End Sub

    '========================================================
    ' REFRESH
    '========================================================
    Private Sub RefreshUsers(sender As Object, e As EventArgs)
        LoadUsers()
    End Sub

End Class


'============================================================
' USER ADD / EDIT DIALOG
'============================================================
Public Class UserEditorForm
    Inherits Form

    Private txtFullName As TextBox
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private cboRole As ComboBox
    Private chkActive As CheckBox

    Public ReadOnly Property FullName As String
        Get
            Return txtFullName.Text.Trim()
        End Get
    End Property

    Public ReadOnly Property Username As String
        Get
            Return txtUsername.Text.Trim()
        End Get
    End Property

    Public ReadOnly Property Password As String
        Get
            Return txtPassword.Text
        End Get
    End Property

    Public ReadOnly Property UserRole As String
        Get
            If cboRole.SelectedItem Is Nothing Then
                Return "STAFF"
            End If

            Return cboRole.SelectedItem.ToString()
        End Get
    End Property

    Public ReadOnly Property IsActive As Boolean
        Get
            Return chkActive.Checked
        End Get
    End Property

    Public Sub New(
        Optional fullName As String = "",
        Optional username As String = "",
        Optional role As String = "STAFF",
        Optional active As Boolean = True)

        Me.Text = If(username = "",
                     "Add User Account",
                     "Edit User Account")

        Me.ClientSize = New Size(410, 425)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.White
        Me.Font = New Font("Segoe UI", 10)

        Dim heading As New Label With {
            .Text = Me.Text,
            .Font = New Font("Segoe UI", 17, FontStyle.Bold),
            .ForeColor = Color.FromArgb(23, 43, 77),
            .AutoSize = True,
            .Location = New Point(24, 16)
        }

        Me.Controls.Add(heading)

        txtFullName = AddTextField("Full Name", 60, fullName)
        txtUsername = AddTextField("Username", 125, username)
        txtPassword = AddTextField("Password", 190, "")

        txtPassword.UseSystemPasswordChar = True

        Dim lblPasswordHint As New Label With {
            .Text = If(username = "",
                "Enter a password for the new account.",
                "Leave blank to keep the existing password."),
            .ForeColor = Color.Gray,
            .Font = New Font("Segoe UI", 8.5F),
            .AutoSize = True,
            .Location = New Point(25, 243)
        }

        Me.Controls.Add(lblPasswordHint)

        Dim roleLabel As New Label With {
            .Text = "Role",
            .AutoSize = True,
            .Location = New Point(25, 270)
        }

        cboRole = New ComboBox With {
            .Location = New Point(25, 292),
            .Width = 360,
            .Height = 30,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }

        cboRole.Items.AddRange(
            New Object() {"STAFF", "ADMIN"})

        cboRole.SelectedItem = role

        If cboRole.SelectedIndex < 0 Then
            cboRole.SelectedIndex = 0
        End If

        chkActive = New CheckBox With {
            .Text = "Account is active",
            .Checked = active,
            .AutoSize = True,
            .Location = New Point(25, 330)
        }

        Me.Controls.Add(roleLabel)
        Me.Controls.Add(cboRole)
        Me.Controls.Add(chkActive)

        Dim btnSave As New Button With {
            .Text = "Save",
            .Location = New Point(180, 370),
            .Size = New Size(95, 35),
            .BackColor = Color.FromArgb(22, 160, 145),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .UseVisualStyleBackColor = False
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New Point(285, 370),
            .Size = New Size(100, 35),
            .DialogResult = DialogResult.Cancel
        }

        AddHandler btnSave.Click, AddressOf SaveUser

        Me.Controls.Add(btnSave)
        Me.Controls.Add(btnCancel)

        Me.AcceptButton = btnSave
        Me.CancelButton = btnCancel

    End Sub

    Private Function AddTextField(
        caption As String,
        top As Integer,
        initialValue As String) As TextBox

        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = True,
            .Location = New Point(25, top)
        }

        Dim txt As New TextBox With {
            .Location = New Point(25, top + 22),
            .Width = 360,
            .Text = initialValue
        }

        Me.Controls.Add(lbl)
        Me.Controls.Add(txt)

        Return txt

    End Function

    Private Sub SaveUser(sender As Object, e As EventArgs)

        If FullName = "" OrElse Username = "" Then
            MessageBox.Show(
                "Enter both the full name and username.",
                "Missing Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Username.Length > 50 OrElse FullName.Length > 120 Then
            MessageBox.Show(
                "The username cannot exceed 50 characters, " &
                "and the full name cannot exceed 120 characters.",
                "Invalid Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If txtPassword.Text <> "" AndAlso
           txtPassword.Text.Length < 8 Then

            MessageBox.Show(
                "Passwords must contain at least 8 characters.",
                "Invalid Password",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Me.Text = "Add User Account" AndAlso Password = "" Then
            MessageBox.Show(
                "Enter a password for the new account.",
                "Missing Password",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Me.DialogResult = DialogResult.OK
        Me.Close()

    End Sub

End Class
