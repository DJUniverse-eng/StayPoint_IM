
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffGuestSearch
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly Muted As Color = Color.FromArgb(100, 116, 139)

    Private txtSearch As TextBox
    Private dgvGuests As DataGridView

    Private txtFirstName As TextBox
    Private txtLastName As TextBox
    Private txtEmail As TextBox
    Private txtPhone As TextBox
    Private txtAddress As TextBox
    Private cboIdType As ComboBox
    Private txtIdNumber As TextBox

    Private lblMessage As Label
    Private lblBookingHistory As Label
    Private btnSave As Button
    Private btnNew As Button

    Private selectedGuestId As Integer = 0
    Private isLoading As Boolean = False

    Public Sub New()
        BuildInterface()
        LoadGuests("")
    End Sub

    '========================================================
    ' INTERFACE
    '========================================================

    Private Sub BuildInterface()
        Controls.Clear()

        Dock = DockStyle.Fill
        BackColor = BgColor
        Font = New Font("Segoe UI", 9.0F)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(16),
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = BgColor
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 54))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 55))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 45))

        Dim heading As New Label With {
            .Text = "Guest Search & Registration",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        root.Controls.Add(heading, 0, 0)

        ' Search bar
        Dim searchPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(8)
        }

        txtSearch = New TextBox With {
            .Location = New Point(10, 10),
            .Size = New Size(330, 30),
            .Font = New Font("Segoe UI", 10),
            .Anchor = AnchorStyles.Left Or AnchorStyles.Top
        }

        txtSearch.Text = "Name, phone, email, or ID number"

        Dim btnSearch As Button = CreateButton("Search", Teal)
        btnSearch.SetBounds(350, 7, 90, 36)

        Dim btnRefresh As Button = CreateButton("Refresh", Navy)
        btnRefresh.SetBounds(448, 7, 90, 36)

        AddHandler btnSearch.Click,
            Sub(sender, e)
                LoadGuests(txtSearch.Text.Trim())
            End Sub

        AddHandler txtSearch.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    e.SuppressKeyPress = True
                    LoadGuests(txtSearch.Text.Trim())
                End If
            End Sub

        AddHandler btnRefresh.Click,
            Sub(sender, e)
                txtSearch.Clear()
                LoadGuests("")
                ClearGuestForm()
            End Sub

        searchPanel.Controls.Add(txtSearch)
        searchPanel.Controls.Add(btnSearch)
        searchPanel.Controls.Add(btnRefresh)

        root.Controls.Add(searchPanel, 0, 1)

        ' Guest results
        Dim resultsPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(8)
        }

        Dim resultsTitle As New Label With {
            .Text = "Registered Guests",
            .Dock = DockStyle.Top,
            .Height = 30,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Navy
        }

        dgvGuests = CreateGrid()
        dgvGuests.Name = "dgvGuests"
        dgvGuests.Dock = DockStyle.Fill

        AddHandler dgvGuests.SelectionChanged,
            AddressOf GuestSelectionChanged

        resultsPanel.Controls.Add(dgvGuests)
        resultsPanel.Controls.Add(resultsTitle)

        root.Controls.Add(resultsPanel, 0, 2)

        ' Guest details and registration form
        Dim detailsPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12)
        }

        Dim detailsTitle As New Label With {
            .Text = "Guest Details",
            .Dock = DockStyle.Top,
            .Height = 30,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Navy
        }

        Dim formLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 4,
            .Padding = New Padding(0, 5, 0, 0)
        }

        formLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95))
        formLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        formLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95))
        formLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        For i As Integer = 0 To 2
            formLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25))
        Next

        formLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25))

        txtFirstName = CreateTextBox()
        txtLastName = CreateTextBox()
        txtEmail = CreateTextBox()
        txtPhone = CreateTextBox()
        txtAddress = CreateTextBox()
        txtIdNumber = CreateTextBox()

        cboIdType = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cboIdType.Items.AddRange(
            New Object() {
                "National ID",
                "Passport",
                "Driver's License",
                "UMID",
                "PhilHealth ID",
                "Postal ID",
                "Other"
            })

        cboIdType.SelectedIndex = 0

        AddFormField(formLayout, 0, 0, "First Name", txtFirstName)
        AddFormField(formLayout, 0, 2, "Last Name", txtLastName)
        AddFormField(formLayout, 1, 0, "Email", txtEmail)
        AddFormField(formLayout, 1, 2, "Phone", txtPhone)
        AddFormField(formLayout, 2, 0, "ID Type", cboIdType)
        AddFormField(formLayout, 2, 2, "ID Number", txtIdNumber)
        AddFormField(formLayout, 3, 0, "Address", txtAddress)

        formLayout.SetColumnSpan(txtAddress, 3)

        Dim actionPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 42,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False,
            .Padding = New Padding(0, 4, 0, 0)
        }

        btnSave = CreateButton("Register Guest", Teal)
        btnSave.Size = New Size(145, 32)

        btnNew = CreateButton("New Guest", Navy)
        btnNew.Size = New Size(110, 32)

        AddHandler btnSave.Click, AddressOf SaveGuest_Click

        AddHandler btnNew.Click,
            Sub(sender, e)
                ClearGuestForm()
            End Sub

        actionPanel.Controls.Add(btnSave)
        actionPanel.Controls.Add(btnNew)

        lblBookingHistory = New Label With {
            .Text = "Booking history: Select a guest to view history.",
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = Muted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        lblMessage = New Label With {
            .Text = "",
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = Muted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        detailsPanel.Controls.Add(formLayout)
        detailsPanel.Controls.Add(lblBookingHistory)
        detailsPanel.Controls.Add(lblMessage)
        detailsPanel.Controls.Add(actionPanel)
        detailsPanel.Controls.Add(detailsTitle)

        root.Controls.Add(detailsPanel, 0, 3)

        Controls.Add(root)
    End Sub

    Private Function CreateGrid() As DataGridView
        Dim grid As New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .AutoGenerateColumns = True
        }

        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Navy
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)
        grid.ColumnHeadersHeight = 32
        grid.RowTemplate.Height = 29

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(246, 249, 252)

        Return grid
    End Function

    Private Function CreateButton(caption As String, color As Color) As Button
        Dim btn As New Button With {
            .Text = caption,
            .BackColor = color,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }

        btn.FlatAppearance.BorderSize = 0
        Return btn
    End Function

    Private Function CreateTextBox() As TextBox
        Return New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 9)
        }
    End Function

    Private Sub AddFormField(
        layout As TableLayoutPanel,
        rowIndex As Integer,
        columnIndex As Integer,
        caption As String,
        control As Control)

        Dim label As New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        layout.Controls.Add(label, columnIndex, rowIndex)
        layout.Controls.Add(control, columnIndex + 1, rowIndex)
    End Sub

    '========================================================
    ' LOAD AND SEARCH GUESTS
    '========================================================

    Private Sub LoadGuests(searchText As String)
        isLoading = True

        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT guest_id, " &
                    "CONCAT(first_name, ' ', last_name) AS `Guest Name`, " &
                    "phone AS `Phone`, email AS `Email`, " &
                    "id_type AS `ID Type`, id_number AS `ID Number`, " &
                    "created_at AS `Registered On` " &
                    "FROM guests " &
                    "WHERE (@search = '' " &
                    "OR first_name LIKE @pattern " &
                    "OR last_name LIKE @pattern " &
                    "OR CONCAT(first_name, ' ', last_name) LIKE @pattern " &
                    "OR phone LIKE @pattern " &
                    "OR email LIKE @pattern " &
                    "OR id_number LIKE @pattern) " &
                    "ORDER BY guest_id DESC;"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@search", searchText)
                    cmd.Parameters.AddWithValue(
                        "@pattern", "%" & searchText & "%")

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            dgvGuests.DataSource = dt

            If dgvGuests.Columns.Contains("guest_id") Then
                dgvGuests.Columns("guest_id").Visible = False
            End If

            lblMessage.Text =
                If(dt.Rows.Count = 0,
                   "No guests found.",
                   dt.Rows.Count.ToString() & " guest(s) found.")

        Catch ex As Exception
            ShowError("Unable to load guests.", ex)
        Finally
            isLoading = False
        End Try
    End Sub

    Private Sub GuestSelectionChanged(sender As Object, e As EventArgs)
        If isLoading OrElse dgvGuests.CurrentRow Is Nothing Then Return

        Try
            Dim row As DataGridViewRow = dgvGuests.CurrentRow

            If row.Cells("guest_id").Value Is Nothing OrElse
               row.Cells("guest_id").Value Is DBNull.Value Then Return

            selectedGuestId = Convert.ToInt32(row.Cells("guest_id").Value)

            LoadGuestDetails(selectedGuestId)
            LoadBookingHistory(selectedGuestId)

        Catch ex As Exception
            ShowError("Unable to select guest.", ex)
        End Try
    End Sub

    Private Sub LoadGuestDetails(guestId As Integer)
        Using conn As MySqlConnection = DbConnection.GetConnection()
            conn.Open()

            Const sql As String =
                "SELECT first_name, last_name, email, phone, address, " &
                "id_type, id_number FROM guests WHERE guest_id = @id;"

            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@id", guestId)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        txtFirstName.Text = ReadText(reader, "first_name")
                        txtLastName.Text = ReadText(reader, "last_name")
                        txtEmail.Text = ReadText(reader, "email")
                        txtPhone.Text = ReadText(reader, "phone")
                        txtAddress.Text = ReadText(reader, "address")
                        txtIdNumber.Text = ReadText(reader, "id_number")

                        Dim idType As String = ReadText(reader, "id_type")
                        Dim index As Integer = cboIdType.Items.IndexOf(idType)

                        If index >= 0 Then
                            cboIdType.SelectedIndex = index
                        Else
                            cboIdType.SelectedItem = "Other"
                        End If

                        btnSave.Text = "Update Guest"
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadBookingHistory(guestId As Integer)
        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT COUNT(*) AS total, " &
                    "SUM(CASE WHEN booking_status = 'CHECKED_IN' " &
                    "THEN 1 ELSE 0 END) AS checked_in " &
                    "FROM bookings WHERE guest_id = @guest;"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@guest", guestId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim total As Integer =
                                If(reader.IsDBNull(0), 0, reader.GetInt32(0))

                            Dim checkedIn As Integer =
                                If(reader.IsDBNull(1), 0, Convert.ToInt32(reader.GetValue(1)))

                            lblBookingHistory.Text =
                                "Booking history: " & total.ToString() &
                                " total booking(s); " &
                                checkedIn.ToString() & " currently checked in."
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            lblBookingHistory.Text = "Booking history unavailable."
        End Try
    End Sub

    '========================================================
    ' REGISTER OR UPDATE GUEST
    '========================================================

    Private Sub SaveGuest_Click(sender As Object, e As EventArgs)
        Dim firstName As String = txtFirstName.Text.Trim()
        Dim lastName As String = txtLastName.Text.Trim()
        Dim email As String = txtEmail.Text.Trim()
        Dim phone As String = txtPhone.Text.Trim()
        Dim address As String = txtAddress.Text.Trim()
        Dim idNumber As String = txtIdNumber.Text.Trim()
        Dim idType As String =
            If(cboIdType.SelectedItem Is Nothing,
               "",
               cboIdType.SelectedItem.ToString())

        If firstName = "" OrElse lastName = "" Then
            MessageBox.Show(
                "First name and last name are required.",
                "Missing Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If idNumber <> "" AndAlso idType = "" Then
            MessageBox.Show("Select an ID type.")
            Return
        End If

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                ' Prevent another guest from using the same non-empty ID.
                If idNumber <> "" Then
                    Const duplicateSql As String =
                        "SELECT COUNT(*) FROM guests " &
                        "WHERE id_number = @idNumber " &
                        "AND (@guestId = 0 OR guest_id <> @guestId);"

                    Using cmd As New MySqlCommand(duplicateSql, conn)
                        cmd.Parameters.AddWithValue("@idNumber", idNumber)
                        cmd.Parameters.AddWithValue("@guestId", selectedGuestId)

                        If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                            MessageBox.Show(
                                "That ID number is already registered to another guest.",
                                "Duplicate ID",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                            Return
                        End If
                    End Using
                End If

                If selectedGuestId = 0 Then
                    Const insertSql As String =
                        "INSERT INTO guests " &
                        "(first_name, last_name, email, phone, address, id_type, id_number) " &
                        "VALUES (@first, @last, @email, @phone, @address, @idType, @idNumber);"

                    Using cmd As New MySqlCommand(insertSql, conn)
                        AddGuestParameters(
                            cmd, firstName, lastName, email,
                            phone, address, idType, idNumber)

                        cmd.ExecuteNonQuery()
                        selectedGuestId = Convert.ToInt32(cmd.LastInsertedId)
                    End Using

                    MessageBox.Show(
                        "Guest registered successfully.",
                        "StayPoint HMS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                Else
                    Const updateSql As String =
                        "UPDATE guests SET " &
                        "first_name = @first, last_name = @last, " &
                        "email = @email, phone = @phone, address = @address, " &
                        "id_type = @idType, id_number = @idNumber " &
                        "WHERE guest_id = @guestId;"

                    Using cmd As New MySqlCommand(updateSql, conn)
                        AddGuestParameters(
                            cmd, firstName, lastName, email,
                            phone, address, idType, idNumber)

                        cmd.Parameters.AddWithValue("@guestId", selectedGuestId)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show(
                        "Guest details updated successfully.",
                        "StayPoint HMS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
                End If
            End Using

            Dim savedId As Integer = selectedGuestId

            LoadGuests(txtSearch.Text.Trim())
            SelectGuestRow(savedId)

        Catch ex As Exception
            ShowError("Unable to save guest.", ex)
        End Try
    End Sub

    Private Sub AddGuestParameters(
        cmd As MySqlCommand,
        firstName As String,
        lastName As String,
        email As String,
        phone As String,
        address As String,
        idType As String,
        idNumber As String)

        cmd.Parameters.AddWithValue("@first", firstName)
        cmd.Parameters.AddWithValue("@last", lastName)

        cmd.Parameters.AddWithValue(
            "@email",
            If(email = "", CType(DBNull.Value, Object), email))

        cmd.Parameters.AddWithValue(
            "@phone",
            If(phone = "", CType(DBNull.Value, Object), phone))

        cmd.Parameters.AddWithValue(
            "@address",
            If(address = "", CType(DBNull.Value, Object), address))

        cmd.Parameters.AddWithValue(
            "@idType",
            If(idNumber = "", CType(DBNull.Value, Object), idType))

        cmd.Parameters.AddWithValue(
            "@idNumber",
            If(idNumber = "", CType(DBNull.Value, Object), idNumber))
    End Sub

    Private Sub SelectGuestRow(guestId As Integer)
        For Each row As DataGridViewRow In dgvGuests.Rows
            If Convert.ToInt32(row.Cells("guest_id").Value) = guestId Then
                row.Selected = True
                dgvGuests.CurrentCell = row.Cells("Guest Name")
                Exit For
            End If
        Next
    End Sub

    Private Sub ClearGuestForm()
        selectedGuestId = 0

        txtFirstName.Clear()
        txtLastName.Clear()
        txtEmail.Clear()
        txtPhone.Clear()
        txtAddress.Clear()
        txtIdNumber.Clear()

        If cboIdType.Items.Count > 0 Then
            cboIdType.SelectedIndex = 0
        End If

        btnSave.Text = "Register Guest"
        lblBookingHistory.Text =
            "Booking history: Select a guest to view history."
        lblMessage.Text = ""

        dgvGuests.ClearSelection()
    End Sub

    '========================================================
    ' HELPERS
    '========================================================

    Private Function ReadText(
        reader As MySqlDataReader,
        columnName As String) As String

        Dim value As Object = reader(columnName)

        If value Is Nothing OrElse value Is DBNull.Value Then
            Return ""
        End If

        Return Convert.ToString(value)
    End Function

    Private Sub ShowError(message As String, ex As Exception)
        MessageBox.Show(
            message & Environment.NewLine & ex.Message,
            "StayPoint HMS",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

End Class
