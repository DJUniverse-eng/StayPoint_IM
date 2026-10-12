
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffCheckInOut
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)

    Private txtSearch As TextBox
    Private dgvBookings As DataGridView
    Private lblDetails As Label
    Private lblMessage As Label
    Private btnCheckIn As Button
    Private btnCheckOut As Button

    Private selectedBookingId As Integer
    Private selectedBookingStatus As String = ""

    Public Sub New()
        InitializeComponent()
        BuildInterface()
        LoadBookings("")
    End Sub

    '========================================================
    ' USER INTERFACE
    '========================================================

    Private Sub BuildInterface()
        Controls.Clear()
        Dock = DockStyle.Fill
        BackColor = BgColor
        Font = New Font("Segoe UI", 9.0F)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 4,
            .Padding = New Padding(24),
            .BackColor = BgColor
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 62))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 142))

        Dim lblTitle As New Label With {
            .Text = "Check-In / Check-Out",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        root.Controls.Add(lblTitle, 0, 0)

        '---------------- SEARCH ----------------

        Dim searchPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(10)
        }

        txtSearch = New TextBox With {
            .Name = "txtSearchBooking",
            .Location = New Point(10, 13),
            .Size = New Size(320, 30),
            .Font = New Font("Segoe UI", 10.0F),
            .Anchor = AnchorStyles.Left Or AnchorStyles.Top
        }

        Dim btnSearch As Button = CreateButton("Search", Teal)
        btnSearch.Location = New Point(340, 10)
        btnSearch.Size = New Size(95, 36)

        Dim btnRefresh As Button = CreateButton("Refresh", Navy)
        btnRefresh.Location = New Point(445, 10)
        btnRefresh.Size = New Size(95, 36)

        AddHandler btnSearch.Click,
            Sub(sender, e)
                LoadBookings(txtSearch.Text.Trim())
            End Sub

        AddHandler txtSearch.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    e.SuppressKeyPress = True
                    LoadBookings(txtSearch.Text.Trim())
                End If
            End Sub

        AddHandler btnRefresh.Click,
            Sub(sender, e)
                txtSearch.Clear()
                LoadBookings("")
            End Sub

        searchPanel.Controls.Add(txtSearch)
        searchPanel.Controls.Add(btnSearch)
        searchPanel.Controls.Add(btnRefresh)
        root.Controls.Add(searchPanel, 0, 1)

        '---------------- BOOKINGS GRID ----------------

        dgvBookings = New DataGridView With {
            .Name = "dgvStaffStayBookings",
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

        dgvBookings.EnableHeadersVisualStyles = False
        dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvBookings.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9.0F, FontStyle.Bold)
        dgvBookings.ColumnHeadersHeight = 38
        dgvBookings.RowTemplate.Height = 34
        dgvBookings.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(246, 249, 252)

        AddHandler dgvBookings.SelectionChanged,
            AddressOf BookingSelectionChanged

        root.Controls.Add(dgvBookings, 0, 2)

        '---------------- SELECTED BOOKING / ACTIONS ----------------

        Dim bottom As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Padding = New Padding(0, 10, 0, 0)
        }

        bottom.RowStyles.Add(New RowStyle(SizeType.Percent, 56))
        bottom.RowStyles.Add(New RowStyle(SizeType.Percent, 44))

        lblDetails = New Label With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .ForeColor = Navy,
            .Padding = New Padding(12),
            .Text = "Select a booking to review its details."
        }

        Dim actionPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = BgColor
        }

        lblMessage = New Label With {
            .Dock = DockStyle.Left,
            .Width = 360,
            .ForeColor = Color.DimGray,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        btnCheckIn = CreateButton("Check In", Teal)
        btnCheckIn.Size = New Size(130, 36)
        btnCheckIn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnCheckIn.Enabled = False

        btnCheckOut = CreateButton("Check Out", Navy)
        btnCheckOut.Size = New Size(130, 36)
        btnCheckOut.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnCheckOut.Enabled = False

        AddHandler btnCheckIn.Click, AddressOf CheckIn_Click
        AddHandler btnCheckOut.Click, AddressOf CheckOut_Click

        AddHandler actionPanel.Resize,
            Sub(sender, e)
                btnCheckOut.Location = New Point(
                    actionPanel.ClientSize.Width - btnCheckOut.Width - 4, 10)

                btnCheckIn.Location = New Point(
                    btnCheckOut.Left - btnCheckIn.Width - 8, 10)
            End Sub

        actionPanel.Controls.Add(lblMessage)
        actionPanel.Controls.Add(btnCheckIn)
        actionPanel.Controls.Add(btnCheckOut)

        bottom.Controls.Add(lblDetails, 0, 0)
        bottom.Controls.Add(actionPanel, 0, 1)
        root.Controls.Add(bottom, 0, 3)

        Controls.Add(root)
    End Sub

    Private Function CreateButton(
        caption As String,
        backColor As Color
    ) As Button

        Dim btn As New Button With {
            .Text = caption,
            .BackColor = backColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }

        btn.FlatAppearance.BorderSize = 0
        Return btn
    End Function

    '========================================================
    ' LOAD AND SEARCH BOOKINGS
    '========================================================

    Private Sub LoadBookings(searchText As String)
        selectedBookingId = 0
        selectedBookingStatus = ""

        btnCheckIn.Enabled = False
        btnCheckOut.Enabled = False
        lblDetails.Text = "Select a booking to review its details."
        lblMessage.Text = ""

        Try
            Dim dt As New DataTable()

            Using connection As MySqlConnection =
                DbConnection.GetConnection()

                connection.Open()

                Const sql As String =
                    "SELECT b.booking_id, " &
                    "b.booking_reference AS `Booking Reference`, " &
                    "CONCAT(g.first_name, ' ', g.last_name) AS `Guest`, " &
                    "b.check_in_date AS `Scheduled Check-In`, " &
                    "b.check_out_date AS `Scheduled Check-Out`, " &
                    "b.booking_status AS `Booking Status`, " &
                    "COALESCE(GROUP_CONCAT(DISTINCT r.room_number " &
                    "ORDER BY r.room_number SEPARATOR ', '), " &
                    "'Not assigned') AS `Assigned Rooms` " &
                    "FROM bookings b " &
                    "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                    "LEFT JOIN booking_rooms br ON br.booking_id = b.booking_id " &
                    "LEFT JOIN rooms r ON r.room_id = br.room_id " &
                    "WHERE (@search = '' " &
                    "OR b.booking_reference LIKE @pattern " &
                    "OR g.first_name LIKE @pattern " &
                    "OR g.last_name LIKE @pattern " &
                    "OR CONCAT(g.first_name, ' ', g.last_name) LIKE @pattern) " &
                    "GROUP BY b.booking_id, b.booking_reference, " &
                    "g.first_name, g.last_name, b.check_in_date, " &
                    "b.check_out_date, b.booking_status " &
                    "ORDER BY b.check_in_date DESC;"

                Using command As New MySqlCommand(sql, connection)
                    command.Parameters.AddWithValue("@search", searchText)
                    command.Parameters.AddWithValue(
                        "@pattern", "%" & searchText & "%")

                    Using adapter As New MySqlDataAdapter(command)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            dgvBookings.DataSource = dt

            If dgvBookings.Columns.Contains("booking_id") Then
                dgvBookings.Columns("booking_id").Visible = False
            End If

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No matching bookings found."
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load bookings." & Environment.NewLine &
                ex.Message,
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BookingSelectionChanged(
        sender As Object,
        e As EventArgs
    )
        If dgvBookings.CurrentRow Is Nothing OrElse
           dgvBookings.CurrentRow.IsNewRow Then
            Return
        End If

        Try
            selectedBookingId = Convert.ToInt32(
                dgvBookings.CurrentRow.Cells("booking_id").Value)

            selectedBookingStatus = Convert.ToString(
                dgvBookings.CurrentRow.Cells("Booking Status").Value)

            Dim bookingRef As String =
                Convert.ToString(
                    dgvBookings.CurrentRow.Cells("Booking Reference").Value)

            Dim guest As String =
                Convert.ToString(
                    dgvBookings.CurrentRow.Cells("Guest").Value)

            Dim rooms As String =
                Convert.ToString(
                    dgvBookings.CurrentRow.Cells("Assigned Rooms").Value)

            Dim checkIn As DateTime =
                Convert.ToDateTime(
                    dgvBookings.CurrentRow.Cells("Scheduled Check-In").Value)

            Dim checkOut As DateTime =
                Convert.ToDateTime(
                    dgvBookings.CurrentRow.Cells("Scheduled Check-Out").Value)

            lblDetails.Text =
                "Guest: " & guest &
                "    Reference: " & bookingRef & Environment.NewLine &
                "Room(s): " & rooms & Environment.NewLine &
                "Scheduled: " &
                checkIn.ToString("MMM dd, yyyy hh:mm tt") & " - " &
                checkOut.ToString("MMM dd, yyyy hh:mm tt") &
                "    Status: " & selectedBookingStatus

            btnCheckIn.Enabled =
                selectedBookingStatus = "CONFIRMED"

            btnCheckOut.Enabled =
                selectedBookingStatus = "CHECKED_IN"

        Catch
            selectedBookingId = 0
            selectedBookingStatus = ""
            btnCheckIn.Enabled = False
            btnCheckOut.Enabled = False
        End Try
    End Sub

    '========================================================
    ' CHECK IN
    '========================================================

    Private Sub CheckIn_Click(sender As Object, e As EventArgs)
        If selectedBookingId <= 0 Then Return

        If FrmLogin.CurrentUserId <= 0 Then
            MessageBox.Show(
                "Your login session is unavailable. Please log in again.",
                "Session Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show(
            "Confirm check-in for the selected booking?",
            "Confirm Check-In",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        btnCheckIn.Enabled = False
        btnCheckOut.Enabled = False

        Try
            Using connection As MySqlConnection =
                DbConnection.GetConnection()

                connection.Open()

                Using transaction As MySqlTransaction =
                    connection.BeginTransaction()

                    Try
                        Dim bookingStatus As String = ""
                        Dim checkInDate As DateTime
                        Dim checkOutDate As DateTime

                        Using command As New MySqlCommand(
                            "SELECT booking_status, check_in_date, check_out_date " &
                            "FROM bookings WHERE booking_id = @id FOR UPDATE;",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            Using reader As MySqlDataReader =
                                command.ExecuteReader()

                                If Not reader.Read() Then
                                    Throw New Exception("Booking not found.")
                                End If

                                bookingStatus =
                                    reader("booking_status").ToString()

                                checkInDate =
                                    Convert.ToDateTime(reader("check_in_date"))

                                checkOutDate =
                                    Convert.ToDateTime(reader("check_out_date"))
                            End Using
                        End Using

                        If bookingStatus <> "CONFIRMED" Then
                            Throw New Exception(
                                "Only confirmed bookings can be checked in.")
                        End If

                        If checkOutDate <= checkInDate Then
                            Throw New Exception(
                                "The booking has invalid stay dates. " &
                                "Contact the administrator.")
                        End If

                        If DateTime.Now < checkInDate Then
                            Throw New Exception(
                                "The scheduled check-in time has not arrived.")
                        End If

                        If DateTime.Now >= checkOutDate Then
                            Throw New Exception(
                                "The scheduled checkout time has passed. " &
                                "Contact the administrator.")
                        End If

                        ' Lock the assigned rooms.
                        Dim roomIds As New List(Of Integer)
                        Dim roomNumbers As New List(Of String)

                        Using command As New MySqlCommand(
                            "SELECT r.room_id, r.room_number, r.status " &
                            "FROM booking_rooms br " &
                            "INNER JOIN rooms r ON r.room_id = br.room_id " &
                            "WHERE br.booking_id = @id " &
                            "ORDER BY r.room_id FOR UPDATE;",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            Using reader As MySqlDataReader =
                                command.ExecuteReader()

                                While reader.Read()
                                    roomIds.Add(
                                        Convert.ToInt32(reader("room_id")))

                                    roomNumbers.Add(
                                        reader("room_number").ToString())

                                    If reader("status").ToString() <>
                                       "AVAILABLE" Then
                                        Throw New Exception(
                                            "Room " &
                                            reader("room_number").ToString() &
                                            " is not available.")
                                    End If
                                End While
                            End Using
                        End Using

                        If roomIds.Count = 0 Then
                            Throw New Exception(
                                "No room is assigned to this booking. " &
                                "Use Room Assignment first.")
                        End If

                        ' Prevent a room from being occupied by overlapping
                        ' active reservations.
                        Const conflictSql As String =
                            "SELECT COUNT(*) " &
                            "FROM booking_rooms other_br " &
                            "INNER JOIN bookings other_b " &
                            "ON other_b.booking_id = other_br.booking_id " &
                            "WHERE other_br.room_id = @room " &
                            "AND other_b.booking_id <> @booking " &
                            "AND other_b.booking_status IN " &
                            "('PENDING','CONFIRMED','CHECKED_IN') " &
                            "AND other_b.check_in_date < @checkout " &
                            "AND other_b.check_out_date > @checkin;"

                        For Each roomId As Integer In roomIds
                            Using command As New MySqlCommand(
                                conflictSql, connection, transaction)

                                command.Parameters.AddWithValue("@room", roomId)
                                command.Parameters.AddWithValue(
                                    "@booking", selectedBookingId)
                                command.Parameters.AddWithValue(
                                    "@checkout", checkOutDate)
                                command.Parameters.AddWithValue(
                                    "@checkin", checkInDate)

                                If Convert.ToInt32(
                                    command.ExecuteScalar()) > 0 Then
                                    Throw New Exception(
                                        "A room has an overlapping active " &
                                        "reservation. Resolve the conflict first.")
                                End If
                            End Using
                        Next

                        ' Change booking status.
                        Using command As New MySqlCommand(
                            "UPDATE bookings SET booking_status = 'CHECKED_IN' " &
                            "WHERE booking_id = @id " &
                            "AND booking_status = 'CONFIRMED';",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            If command.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "Booking status changed. Refresh and retry.")
                            End If
                        End Using

                        ' Mark assigned rooms occupied.
                        Using command As New MySqlCommand(
                            "UPDATE rooms r " &
                            "INNER JOIN booking_rooms br ON br.room_id = r.room_id " &
                            "SET r.status = 'OCCUPIED' " &
                            "WHERE br.booking_id = @id " &
                            "AND r.status = 'AVAILABLE';",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            If command.ExecuteNonQuery() <> roomIds.Count Then
                                Throw New Exception(
                                    "Room status changed during check-in. " &
                                    "The operation has been cancelled.")
                            End If
                        End Using

                        ' Create an open folio if one does not exist.
                        Using command As New MySqlCommand(
                            "INSERT INTO guest_folios (booking_id, folio_status) " &
                            "SELECT @id, 'OPEN' " &
                            "WHERE NOT EXISTS (" &
                            "SELECT 1 FROM guest_folios WHERE booking_id = @id);",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)
                            command.ExecuteNonQuery()
                        End Using

                        WriteAuditLog(
                            connection,
                            transaction,
                            "CHECK_IN",
                            "bookings",
                            selectedBookingId,
                            "Booking checked in. Rooms: " &
                            String.Join(", ", roomNumbers))

                        transaction.Commit()
                    Catch
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show(
                "Check-in completed successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            LoadBookings(txtSearch.Text.Trim())

        Catch ex As Exception
            MessageBox.Show(
                ex.Message,
                "Check-In Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        Finally
            UpdateActionButtons()
        End Try
    End Sub

    '========================================================
    ' CHECK OUT
    '========================================================

    Private Sub CheckOut_Click(sender As Object, e As EventArgs)
        If selectedBookingId <= 0 Then Return

        If FrmLogin.CurrentUserId <= 0 Then
            MessageBox.Show(
                "Your login session is unavailable. Please log in again.",
                "Session Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show(
            "Confirm checkout? The guest folio must have no outstanding balance.",
            "Confirm Check-Out",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        btnCheckIn.Enabled = False
        btnCheckOut.Enabled = False

        Try
            Using connection As MySqlConnection =
                DbConnection.GetConnection()

                connection.Open()

                Using transaction As MySqlTransaction =
                    connection.BeginTransaction()

                    Try
                        Dim bookingStatus As String = ""

                        Using command As New MySqlCommand(
                            "SELECT booking_status FROM bookings " &
                            "WHERE booking_id = @id FOR UPDATE;",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            Dim result As Object = command.ExecuteScalar()

                            If result Is Nothing OrElse
                               result Is DBNull.Value Then
                                Throw New Exception("Booking not found.")
                            End If

                            bookingStatus = result.ToString()
                        End Using

                        If bookingStatus <> "CHECKED_IN" Then
                            Throw New Exception(
                                "Only checked-in bookings can be checked out.")
                        End If

                        Dim folioId As Integer = 0
                        Dim folioStatus As String = ""

                        Using command As New MySqlCommand(
                            "SELECT folio_id, folio_status " &
                            "FROM guest_folios WHERE booking_id = @id " &
                            "FOR UPDATE;",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            Using reader As MySqlDataReader =
                                command.ExecuteReader()

                                If reader.Read() Then
                                    folioId = Convert.ToInt32(
                                        reader("folio_id"))

                                    folioStatus =
                                        reader("folio_status").ToString()
                                End If
                            End Using
                        End Using

                        If folioId <= 0 Then
                            Throw New Exception(
                                "No folio exists for this booking. " &
                                "Contact the administrator.")
                        End If

                        If folioStatus <> "OPEN" Then
                            Throw New Exception(
                                "The guest folio is not open.")
                        End If

                        ' Calculate charges minus completed payments,
                        ' plus refunds already issued.
                        Const balanceSql As String =
                            "SELECT " &
                            "COALESCE((SELECT SUM(amount) " &
                            "FROM folio_charges WHERE folio_id = @folio), 0) " &
                            "- COALESCE((SELECT SUM(amount) FROM payments " &
                            "WHERE folio_id = @folio " &
                            "AND payment_status = 'COMPLETED'), 0) " &
                            "+ COALESCE((SELECT SUM(r.refund_amount) " &
                            "FROM refunds r INNER JOIN payments p " &
                            "ON p.payment_id = r.payment_id " &
                            "WHERE p.folio_id = @folio), 0);"

                        Dim balance As Decimal

                        Using command As New MySqlCommand(
                            balanceSql, connection, transaction)

                            command.Parameters.AddWithValue(
                                "@folio", folioId)

                            balance = Convert.ToDecimal(
                                command.ExecuteScalar())
                        End Using

                        If balance > 0.01D Then
                            Throw New Exception(
                                "Outstanding balance: " &
                                balance.ToString("N2") &
                                ". Record payment before checkout.")
                        End If

                        ' Change booking status.
                        Using command As New MySqlCommand(
                            "UPDATE bookings SET booking_status = 'CHECKED_OUT' " &
                            "WHERE booking_id = @id " &
                            "AND booking_status = 'CHECKED_IN';",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            If command.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "Booking status changed. Refresh and retry.")
                            End If
                        End Using

                        ' Release rooms to the cleaning workflow.
                        Using command As New MySqlCommand(
                            "UPDATE rooms r " &
                            "INNER JOIN booking_rooms br ON br.room_id = r.room_id " &
                            "SET r.status = 'CLEANING' " &
                            "WHERE br.booking_id = @id " &
                            "AND r.status = 'OCCUPIED';",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@id", selectedBookingId)

                            If command.ExecuteNonQuery() = 0 Then
                                Throw New Exception(
                                    "No occupied assigned rooms were found. " &
                                    "Checkout was cancelled.")
                            End If
                        End Using

                        ' Close the folio.
                        Using command As New MySqlCommand(
                            "UPDATE guest_folios " &
                            "SET folio_status = 'CLOSED', closed_at = NOW() " &
                            "WHERE folio_id = @folio " &
                            "AND folio_status = 'OPEN';",
                            connection, transaction)

                            command.Parameters.AddWithValue(
                                "@folio", folioId)

                            If command.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "The folio could not be closed.")
                            End If
                        End Using

                        WriteAuditLog(
                            connection,
                            transaction,
                            "CHECK_OUT",
                            "bookings",
                            selectedBookingId,
                            "Booking checked out. Final folio balance: " &
                            balance.ToString("0.00"))

                        transaction.Commit()
                    Catch
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            MessageBox.Show(
                "Checkout completed. The assigned rooms are now marked for cleaning.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            LoadBookings(txtSearch.Text.Trim())

        Catch ex As Exception
            MessageBox.Show(
                ex.Message,
                "Check-Out Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        Finally
            UpdateActionButtons()
        End Try
    End Sub

    '========================================================
    ' AUDIT LOG
    '========================================================

    Private Sub WriteAuditLog(
        connection As MySqlConnection,
        transaction As MySqlTransaction,
        action As String,
        tableName As String,
        recordId As Integer,
        details As String
    )
        Const sql As String =
            "INSERT INTO audit_logs " &
            "(user_id, action, table_name, record_id, details) " &
            "VALUES (@user, @action, @table, @record, @details);"

        Using command As New MySqlCommand(
            sql, connection, transaction)

            command.Parameters.AddWithValue(
                "@user", FrmLogin.CurrentUserId)
            command.Parameters.AddWithValue("@action", action)
            command.Parameters.AddWithValue("@table", tableName)
            command.Parameters.AddWithValue(
                "@record", recordId.ToString())
            command.Parameters.AddWithValue("@details", details)

            command.ExecuteNonQuery()
        End Using
    End Sub

    '========================================================
    ' REFRESH BUTTON STATES
    '========================================================

    Private Sub UpdateActionButtons()
        btnCheckIn.Enabled =
            selectedBookingId > 0 AndAlso
            selectedBookingStatus = "CONFIRMED"

        btnCheckOut.Enabled =
            selectedBookingId > 0 AndAlso
            selectedBookingStatus = "CHECKED_IN"
    End Sub

End Class
