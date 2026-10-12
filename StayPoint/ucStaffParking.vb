
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffParking
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly Muted As Color = Color.FromArgb(100, 116, 139)

    Private cboBooking As ComboBox
    Private cboSpace As ComboBox
    Private cboVehicleType As ComboBox
    Private cboBilling As ComboBox
    Private txtPlate As TextBox

    Private dgvAssignments As DataGridView
    Private lblHourlyRate As Label
    Private lblDailyRate As Label
    Private lblMessage As Label

    Private btnAssign As Button
    Private btnCheckout As Button
    Private btnRefresh As Button

    Private selectedAssignmentId As Integer
    Private isLoading As Boolean

    Public Sub New()
        BuildInterface()
        LoadBookings()
        LoadAvailableSpaces()
        LoadAssignments()
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
            .Padding = New Padding(14),
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = BgColor
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 225))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 25))

        Dim heading As New Label With {
            .Text = "Parking Assignment",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        root.Controls.Add(heading, 0, 0)

        '---------------- ASSIGNMENT FORM ----------------

        Dim formPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12),
            .Margin = New Padding(0, 0, 0, 8)
        }

        Dim formTitle As New Label With {
            .Text = "Assign Parking Space",
            .Dock = DockStyle.Top,
            .Height = 28,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Navy
        }

        Dim form As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 4,
            .Padding = New Padding(0, 6, 0, 0)
        }

        form.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 105))
        form.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        form.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 105))
        form.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        For i As Integer = 0 To 3
            form.RowStyles.Add(New RowStyle(SizeType.Percent, 25))
        Next

        cboBooking = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cboSpace = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        txtPlate = New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10)
        }

        cboVehicleType = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cboVehicleType.Items.AddRange(
            New Object() {"Car", "Motorcycle", "SUV", "Van", "Pickup", "Other"})

        cboVehicleType.SelectedIndex = 0

        cboBilling = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cboBilling.Items.AddRange(New Object() {"HOURLY", "DAILY"})
        cboBilling.SelectedIndex = 0

        lblHourlyRate = New Label With {
            .Text = "Hourly rate: ₱0.00",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Teal,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }

        lblDailyRate = New Label With {
            .Text = "Daily rate: ₱0.00",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Teal,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }

        AddFormField(form, 0, 0, "Guest Booking", cboBooking)
        AddFormField(form, 0, 2, "Parking Space", cboSpace)
        AddFormField(form, 1, 0, "Plate Number", txtPlate)
        AddFormField(form, 1, 2, "Vehicle Type", cboVehicleType)
        AddFormField(form, 2, 0, "Billing Type", cboBilling)
        AddFormField(form, 2, 2, "Hourly Rate", lblHourlyRate)
        AddFormField(form, 3, 0, "Rate Information", lblDailyRate)

        Dim buttonPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(0, 3, 0, 0)
        }

        btnAssign = CreateButton("Assign Parking", Teal)
        btnAssign.Size = New Size(145, 34)

        btnCheckout = CreateButton("Vehicle Checkout", Navy)
        btnCheckout.Size = New Size(145, 34)
        btnCheckout.Enabled = False

        btnRefresh = CreateButton("Refresh", Navy)
        btnRefresh.Size = New Size(90, 34)

        AddHandler btnAssign.Click, AddressOf AssignParking_Click
        AddHandler btnCheckout.Click, AddressOf CheckoutVehicle_Click
        AddHandler btnRefresh.Click, AddressOf Refresh_Click

        buttonPanel.Controls.Add(btnAssign)
        buttonPanel.Controls.Add(btnCheckout)
        buttonPanel.Controls.Add(btnRefresh)

        form.Controls.Add(buttonPanel, 1, 3)
        form.SetColumnSpan(buttonPanel, 3)

        AddHandler cboSpace.SelectedIndexChanged,
            AddressOf SpaceSelectionChanged

        AddHandler cboBilling.SelectedIndexChanged,
            AddressOf BillingSelectionChanged

        formPanel.Controls.Add(form)
        formPanel.Controls.Add(formTitle)

        root.Controls.Add(formPanel, 0, 1)

        '---------------- ASSIGNMENTS GRID ----------------

        Dim gridPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(8)
        }

        Dim gridTitle As New Label With {
            .Text = "Parking Assignment Records",
            .Dock = DockStyle.Top,
            .Height = 30,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .ForeColor = Navy
        }

        dgvAssignments = CreateGrid()
        dgvAssignments.Name = "dgvStaffParkingAssignments"

        AddHandler dgvAssignments.SelectionChanged,
            AddressOf AssignmentSelectionChanged

        gridPanel.Controls.Add(dgvAssignments)
        gridPanel.Controls.Add(gridTitle)

        root.Controls.Add(gridPanel, 0, 2)

        lblMessage = New Label With {
            .Text = "",
            .Dock = DockStyle.Fill,
            .ForeColor = Muted,
            .TextAlign = ContentAlignment.MiddleLeft,
            .AutoEllipsis = True
        }

        root.Controls.Add(lblMessage, 0, 3)

        Controls.Add(root)
        root.BringToFront()
    End Sub

    Private Sub AddFormField(
        layout As TableLayoutPanel,
        row As Integer,
        labelColumn As Integer,
        caption As String,
        control As Control
    )
        layout.Controls.Add(MakeLabel(caption), labelColumn, row)
        layout.Controls.Add(control, labelColumn + 1, row)
    End Sub

    Private Function MakeLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Navy
        }
    End Function

    Private Function CreateButton(
        caption As String,
        color As Color
    ) As Button

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
        grid.ColumnHeadersHeight = 34
        grid.RowTemplate.Height = 30
        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(246, 249, 252)

        Return grid
    End Function

    '========================================================
    ' LOAD ELIGIBLE BOOKINGS
    '========================================================

    Private Sub LoadBookings()
        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT b.booking_id, " &
                    "CONCAT(b.booking_reference, ' - ', " &
                    "g.first_name, ' ', g.last_name) AS booking_display " &
                    "FROM bookings b " &
                    "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                    "WHERE b.booking_status IN ('CONFIRMED','CHECKED_IN') " &
                    "ORDER BY b.check_in_date DESC, b.booking_id DESC;"

                Using cmd As New MySqlCommand(sql, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            cboBooking.DataSource = Nothing
            cboBooking.DisplayMember = "booking_display"
            cboBooking.ValueMember = "booking_id"
            cboBooking.DataSource = dt

            If dt.Rows.Count = 0 Then
                lblMessage.Text =
                    "No confirmed or checked-in bookings are available."
            End If

        Catch ex As Exception
            ShowError("Unable to load eligible bookings.", ex)
        End Try
    End Sub

    '========================================================
    ' LOAD AVAILABLE PARKING SPACES
    '========================================================

    Private Sub LoadAvailableSpaces()
        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT parking_space_id, space_code, " &
                    "rate_per_hour, rate_per_day, status " &
                    "FROM parking_spaces " &
                    "WHERE status = 'AVAILABLE' " &
                    "ORDER BY space_code;"

                Using cmd As New MySqlCommand(sql, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            cboSpace.DataSource = Nothing
            cboSpace.DisplayMember = "space_code"
            cboSpace.ValueMember = "parking_space_id"
            cboSpace.DataSource = dt

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No available parking spaces."
            End If

            UpdateRates()

        Catch ex As Exception
            ShowError("Unable to load available parking spaces.", ex)
        End Try
    End Sub

    Private Sub SpaceSelectionChanged(sender As Object, e As EventArgs)
        UpdateRates()
    End Sub

    Private Sub BillingSelectionChanged(sender As Object, e As EventArgs)
        UpdateRates()
    End Sub

    Private Sub UpdateRates()
        If cboSpace Is Nothing OrElse
           cboSpace.SelectedItem Is Nothing Then

            lblHourlyRate.Text = "Hourly rate: ₱0.00"
            lblDailyRate.Text = "Daily rate: ₱0.00"
            Return
        End If

        Try
            Dim row As DataRowView =
                TryCast(cboSpace.SelectedItem, DataRowView)

            If row Is Nothing Then Return

            Dim hourly As Decimal =
                Convert.ToDecimal(row("rate_per_hour"))

            Dim daily As Decimal =
                Convert.ToDecimal(row("rate_per_day"))

            lblHourlyRate.Text = "Hourly rate: " & Peso(hourly)
            lblDailyRate.Text = "Daily rate: " & Peso(daily)

        Catch
            lblHourlyRate.Text = "Hourly rate: unavailable"
            lblDailyRate.Text = "Daily rate: unavailable"
        End Try
    End Sub

    '========================================================
    ' ASSIGN PARKING
    '========================================================

    Private Sub AssignParking_Click(sender As Object, e As EventArgs)
        If cboBooking.SelectedValue Is Nothing OrElse
           cboSpace.SelectedValue Is Nothing Then

            MessageBox.Show(
                "Select a booking and an available parking space.")
            Return
        End If

        If String.IsNullOrWhiteSpace(txtPlate.Text) Then
            MessageBox.Show("Enter the vehicle plate number.")
            txtPlate.Focus()
            Return
        End If

        If cboVehicleType.SelectedItem Is Nothing OrElse
           cboBilling.SelectedItem Is Nothing Then

            MessageBox.Show("Select the vehicle type and billing type.")
            Return
        End If

        Dim bookingId As Integer =
            Convert.ToInt32(cboBooking.SelectedValue)

        Dim spaceId As Integer =
            Convert.ToInt32(cboSpace.SelectedValue)

        Dim plate As String = txtPlate.Text.Trim().ToUpperInvariant()
        Dim vehicleType As String =
            cboVehicleType.SelectedItem.ToString()

        Dim billingType As String =
            cboBilling.SelectedItem.ToString()

        Dim assignedRate As Decimal = 0D

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        ' Verify the booking is still eligible.
                        Using cmd As New MySqlCommand(
                            "SELECT booking_status FROM bookings " &
                            "WHERE booking_id = @booking FOR UPDATE;",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@booking", bookingId)

                            Dim result As Object = cmd.ExecuteScalar()

                            If result Is Nothing OrElse result Is DBNull.Value Then
                                Throw New Exception("Booking not found.")
                            End If

                            Dim bookingStatus As String = result.ToString()

                            If bookingStatus <> "CONFIRMED" AndAlso
                               bookingStatus <> "CHECKED_IN" Then

                                Throw New Exception(
                                    "Only confirmed or checked-in bookings " &
                                    "can receive parking assignments.")
                            End If
                        End Using

                        ' Lock and verify the selected space and its Admin rate.
                        Dim spaceFound As Boolean = False

                        Using cmd As New MySqlCommand(
                            "SELECT rate_per_hour, rate_per_day " &
                            "FROM parking_spaces " &
                            "WHERE parking_space_id = @space " &
                            "AND status = 'AVAILABLE' FOR UPDATE;",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@space", spaceId)

                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                If reader.Read() Then
                                    spaceFound = True

                                    If billingType = "HOURLY" Then
                                        assignedRate =
                                            Convert.ToDecimal(reader("rate_per_hour"))
                                    Else
                                        assignedRate =
                                            Convert.ToDecimal(reader("rate_per_day"))
                                    End If
                                End If
                            End Using
                        End Using

                        If Not spaceFound Then
                            Throw New Exception(
                                "This parking space is no longer available. Refresh and select another space.")
                        End If

                        If assignedRate <= 0D Then
                            Throw New Exception(
                                "The selected space has no valid Admin-configured rate for " &
                                billingType.ToLowerInvariant() & " billing.")
                        End If

                        ' Prevent the same plate from having another active assignment.
                        Using cmd As New MySqlCommand(
                            "SELECT COUNT(*) FROM parking_assignments " &
                            "WHERE UPPER(vehicle_plate) = @plate " &
                            "AND assignment_status = 'ACTIVE';",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@plate", plate)

                            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                                Throw New Exception(
                                    "This vehicle already has an active parking assignment.")
                            End If
                        End Using

                        ' Create assignment.
                        Const insertSql As String =
                            "INSERT INTO parking_assignments " &
                            "(booking_id, parking_space_id, vehicle_plate, " &
                            "vehicle_type, start_time, billing_type, " &
                            "agreed_rate, assignment_status, created_by) " &
                            "VALUES (@booking, @space, @plate, @type, " &
                            "NOW(), @billing, @rate, 'ACTIVE', @user);"

                        Dim assignmentId As Integer

                        Using cmd As New MySqlCommand(insertSql, conn, tx)
                            cmd.Parameters.AddWithValue("@booking", bookingId)
                            cmd.Parameters.AddWithValue("@space", spaceId)
                            cmd.Parameters.AddWithValue("@plate", plate)
                            cmd.Parameters.AddWithValue("@type", vehicleType)
                            cmd.Parameters.AddWithValue("@billing", billingType)
                            cmd.Parameters.AddWithValue("@rate", assignedRate)
                            cmd.Parameters.AddWithValue(
                                "@user", FrmLogin.CurrentUserId)

                            cmd.ExecuteNonQuery()
                            assignmentId = Convert.ToInt32(cmd.LastInsertedId)
                        End Using

                        ' Reserve the space.
                        Using cmd As New MySqlCommand(
                            "UPDATE parking_spaces SET status = 'OCCUPIED' " &
                            "WHERE parking_space_id = @space " &
                            "AND status = 'AVAILABLE';",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@space", spaceId)

                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "Unable to reserve the selected parking space.")
                            End If
                        End Using

                        WriteAuditLog(
                            conn,
                            tx,
                            "ASSIGN_PARKING",
                            assignmentId,
                            "Booking #" & bookingId &
                            "; space ID " & spaceId &
                            "; plate " & plate &
                            "; billing " & billingType &
                            "; rate " & assignedRate.ToString("0.00"))

                        tx.Commit()

                    Catch
                        Try
                            tx.Rollback()
                        Catch
                        End Try

                        Throw
                    End Try
                End Using
            End Using

            txtPlate.Clear()

            LoadBookings()
            LoadAvailableSpaces()
            LoadAssignments()

            lblMessage.Text = "Parking assignment created successfully."

            MessageBox.Show(
                "Parking assigned successfully." &
                Environment.NewLine &
                "Vehicle: " & plate &
                Environment.NewLine &
                "Billing: " & billingType &
                Environment.NewLine &
                "Rate: " & Peso(assignedRate),
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            ShowError("Unable to assign parking.", ex)
        End Try
    End Sub

    '========================================================
    ' LOAD ASSIGNMENT RECORDS
    '========================================================

    Private Sub LoadAssignments()
        isLoading = True
        selectedAssignmentId = 0
        btnCheckout.Enabled = False

        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT pa.parking_assignment_id, " &
                    "b.booking_reference AS `Booking Reference`, " &
                    "CONCAT(g.first_name, ' ', g.last_name) AS `Guest`, " &
                    "ps.space_code AS `Parking Space`, " &
                    "pa.vehicle_plate AS `Plate Number`, " &
                    "pa.vehicle_type AS `Vehicle Type`, " &
                    "pa.start_time AS `Parked At`, " &
                    "pa.end_time AS `Checked Out`, " &
                    "pa.billing_type AS `Billing Type`, " &
                    "pa.agreed_rate AS `Agreed Rate`, " &
                    "pa.assignment_status AS `Status`, " &
                    "pa.charge_id " &
                    "FROM parking_assignments pa " &
                    "INNER JOIN bookings b ON b.booking_id = pa.booking_id " &
                    "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                    "INNER JOIN parking_spaces ps " &
                    "ON ps.parking_space_id = pa.parking_space_id " &
                    "ORDER BY CASE WHEN pa.assignment_status = 'ACTIVE' " &
                    "THEN 0 ELSE 1 END, pa.start_time DESC;"

                Using cmd As New MySqlCommand(sql, conn)
                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            dgvAssignments.DataSource = dt

            If dgvAssignments.Columns.Contains("parking_assignment_id") Then
                dgvAssignments.Columns("parking_assignment_id").Visible = False
            End If

            If dgvAssignments.Columns.Contains("charge_id") Then
                dgvAssignments.Columns("charge_id").Visible = False
            End If

            If dgvAssignments.Columns.Contains("Parked At") Then
                dgvAssignments.Columns("Parked At").DefaultCellStyle.Format =
                    "MMM dd, yyyy hh:mm tt"
            End If

            If dgvAssignments.Columns.Contains("Checked Out") Then
                dgvAssignments.Columns("Checked Out").DefaultCellStyle.Format =
                    "MMM dd, yyyy hh:mm tt"
            End If

            If dgvAssignments.Columns.Contains("Agreed Rate") Then
                dgvAssignments.Columns("Agreed Rate").DefaultCellStyle.Format =
                    """₱ ""#,##0.00"
            End If

            lblMessage.Text = "Parking records loaded."

        Catch ex As Exception
            ShowError("Unable to load parking assignments.", ex)
        Finally
            isLoading = False
        End Try
    End Sub

    Private Sub AssignmentSelectionChanged(sender As Object, e As EventArgs)
        If isLoading OrElse dgvAssignments.CurrentRow Is Nothing Then
            selectedAssignmentId = 0
            btnCheckout.Enabled = False
            Return
        End If

        Try
            Dim row As DataGridViewRow = dgvAssignments.CurrentRow

            selectedAssignmentId =
                Convert.ToInt32(row.Cells("parking_assignment_id").Value)

            Dim status As String =
                Convert.ToString(row.Cells("Status").Value)

            btnCheckout.Enabled = (status = "ACTIVE")

        Catch
            selectedAssignmentId = 0
            btnCheckout.Enabled = False
        End Try
    End Sub

    '========================================================
    ' VEHICLE CHECKOUT AND FOLIO CHARGE
    '========================================================

    Private Sub CheckoutVehicle_Click(sender As Object, e As EventArgs)
        If selectedAssignmentId <= 0 Then
            MessageBox.Show("Select an active parking assignment.")
            Return
        End If

        Dim bookingId As Integer = 0
        Dim spaceId As Integer = 0
        Dim startTime As DateTime
        Dim rate As Decimal = 0D
        Dim billingType As String = ""
        Dim plate As String = ""
        Dim assignmentId As Integer = selectedAssignmentId

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT booking_id, parking_space_id, start_time, " &
                    "agreed_rate, billing_type, vehicle_plate " &
                    "FROM parking_assignments " &
                    "WHERE parking_assignment_id = @id " &
                    "AND assignment_status = 'ACTIVE';"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", assignmentId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If Not reader.Read() Then
                            Throw New Exception(
                                "The assignment is no longer active.")
                        End If

                        bookingId = Convert.ToInt32(reader("booking_id"))
                        spaceId = Convert.ToInt32(reader("parking_space_id"))
                        startTime = Convert.ToDateTime(reader("start_time"))
                        rate = Convert.ToDecimal(reader("agreed_rate"))
                        billingType = Convert.ToString(reader("billing_type"))
                        plate = Convert.ToString(reader("vehicle_plate"))
                    End Using
                End Using
            End Using

            Dim endTime As DateTime = DateTime.Now
            Dim units As Decimal

            If billingType = "DAILY" Then
                units = CDec(Math.Max(
                    1,
                    Math.Ceiling((endTime - startTime).TotalDays)))
            Else
                units = CDec(Math.Max(
                    1,
                    Math.Ceiling((endTime - startTime).TotalMinutes / 60.0)))
            End If

            Dim totalFee As Decimal = Math.Round(
                units * rate, 2, MidpointRounding.AwayFromZero)

            Dim unitName As String =
                If(billingType = "DAILY", "day(s)", "hour(s)")

            Dim message As String =
                "Vehicle: " & plate & Environment.NewLine &
                "Parking duration: " & units.ToString("0") &
                " " & unitName & Environment.NewLine &
                "Rate: " & Peso(rate) & " per " &
                If(billingType = "DAILY", "day", "hour") &
                Environment.NewLine &
                "Total parking fee: " & Peso(totalFee) &
                Environment.NewLine & Environment.NewLine &
                "The parking fee will be added to the guest's open folio. " &
                "Any outstanding amount can be collected in cash through " &
                "Payments & Guest Folio." &
                Environment.NewLine & Environment.NewLine &
                "Continue with vehicle checkout?"

            If MessageBox.Show(
                message,
                "Confirm Parking Checkout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then

                Return
            End If

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        ' Lock the active assignment and confirm its data.
                        Using cmd As New MySqlCommand(
                            "SELECT booking_id, parking_space_id " &
                            "FROM parking_assignments " &
                            "WHERE parking_assignment_id = @id " &
                            "AND assignment_status = 'ACTIVE' FOR UPDATE;",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@id", assignmentId)

                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New Exception(
                                        "This parking assignment has already been completed.")
                                End If

                                If Convert.ToInt32(reader("booking_id")) <> bookingId OrElse
                                   Convert.ToInt32(reader("parking_space_id")) <> spaceId Then

                                    Throw New Exception(
                                        "The assignment changed. Refresh and try again.")
                                End If
                            End Using
                        End Using

                        ' Lock the parking space.
                        Using cmd As New MySqlCommand(
                            "SELECT status FROM parking_spaces " &
                            "WHERE parking_space_id = @space FOR UPDATE;",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@space", spaceId)

                            Dim spaceStatus As Object = cmd.ExecuteScalar()

                            If spaceStatus Is Nothing OrElse
                               spaceStatus Is DBNull.Value OrElse
                               Convert.ToString(spaceStatus) <> "OCCUPIED" Then

                                Throw New Exception(
                                    "The parking space is not marked OCCUPIED. " &
                                    "Check the parking inventory before checkout.")
                            End If
                        End Using

                        ' Get or create the booking's open folio.
                        Dim folioId As Integer = GetOrCreateOpenFolio(
                            conn, tx, bookingId)

                        ' Prevent duplicate posting by completing the assignment
                        ' and linking its folio charge within this transaction.
                        Dim chargeId As Integer

                        Const chargeSql As String =
                            "INSERT INTO folio_charges " &
                            "(folio_id, charge_type, description, quantity, " &
                            "unit_price, amount, charge_date, created_by) " &
                            "VALUES (@folio, 'PARKING', @description, " &
                            "@quantity, @rate, @amount, NOW(), @user);"

                        Using cmd As New MySqlCommand(chargeSql, conn, tx)
                            cmd.Parameters.AddWithValue("@folio", folioId)
                            cmd.Parameters.AddWithValue(
                                "@description",
                                "Parking " & billingType.ToLowerInvariant() &
                                " fee - " & plate &
                                " (Assignment #" & assignmentId & ")")
                            cmd.Parameters.AddWithValue("@quantity", units)
                            cmd.Parameters.AddWithValue("@rate", rate)
                            cmd.Parameters.AddWithValue("@amount", totalFee)
                            cmd.Parameters.AddWithValue(
                                "@user", FrmLogin.CurrentUserId)

                            cmd.ExecuteNonQuery()
                            chargeId = Convert.ToInt32(cmd.LastInsertedId)
                        End Using

                        Using cmd As New MySqlCommand(
                            "UPDATE parking_assignments " &
                            "SET end_time = @endTime, " &
                            "assignment_status = 'COMPLETED', " &
                            "charge_id = @charge " &
                            "WHERE parking_assignment_id = @id " &
                            "AND assignment_status = 'ACTIVE';",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@endTime", endTime)
                            cmd.Parameters.AddWithValue("@charge", chargeId)
                            cmd.Parameters.AddWithValue("@id", assignmentId)

                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "Unable to complete the parking assignment.")
                            End If
                        End Using

                        Using cmd As New MySqlCommand(
                            "UPDATE parking_spaces SET status = 'AVAILABLE' " &
                            "WHERE parking_space_id = @space " &
                            "AND status = 'OCCUPIED';",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@space", spaceId)

                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New Exception(
                                    "Unable to release the parking space.")
                            End If
                        End Using

                        WriteAuditLog(
                            conn,
                            tx,
                            "PARKING_CHECKOUT",
                            assignmentId,
                            "Vehicle " & plate &
                            "; booking #" & bookingId &
                            "; folio #" & folioId &
                            "; charge #" & chargeId &
                            "; fee " & totalFee.ToString("0.00"))

                        tx.Commit()

                    Catch
                        Try
                            tx.Rollback()
                        Catch
                        End Try

                        Throw
                    End Try
                End Using
            End Using

            LoadAvailableSpaces()
            LoadAssignments()

            MessageBox.Show(
                "Vehicle checkout completed." &
                Environment.NewLine &
                "Parking fee: " & Peso(totalFee) &
                Environment.NewLine &
                "Folio charge added successfully." &
                Environment.NewLine &
                "Collect any outstanding amount in cash through Payments & Guest Folio.",
                "Parking Checkout",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Catch ex As Exception
            ShowError("Unable to complete vehicle checkout.", ex)
        End Try
    End Sub

    '========================================================
    ' GET OR CREATE OPEN FOLIO
    '========================================================

    Private Function GetOrCreateOpenFolio(
        conn As MySqlConnection,
        tx As MySqlTransaction,
        bookingId As Integer
    ) As Integer

        Using cmd As New MySqlCommand(
            "SELECT folio_id, folio_status FROM guest_folios " &
            "WHERE booking_id = @booking FOR UPDATE;",
            conn, tx)

            cmd.Parameters.AddWithValue("@booking", bookingId)

            Using reader As MySqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    Dim folioId As Integer =
                        Convert.ToInt32(reader("folio_id"))

                    Dim status As String =
                        Convert.ToString(reader("folio_status"))

                    If status <> "OPEN" Then
                        Throw New Exception(
                            "The guest's folio is closed. Parking checkout " &
                            "cannot add a charge until the folio issue is resolved.")
                    End If

                    Return folioId
                End If
            End Using
        End Using

        ' Only create a folio for an eligible booking.
        Using cmd As New MySqlCommand(
            "SELECT booking_status FROM bookings " &
            "WHERE booking_id = @booking FOR UPDATE;",
            conn, tx)

            cmd.Parameters.AddWithValue("@booking", bookingId)

            Dim result As Object = cmd.ExecuteScalar()

            If result Is Nothing OrElse result Is DBNull.Value Then
                Throw New Exception("Booking not found.")
            End If

            Dim bookingStatus As String = result.ToString()

            If bookingStatus <> "CONFIRMED" AndAlso
               bookingStatus <> "CHECKED_IN" Then

                Throw New Exception(
                    "An open folio cannot be created for this booking status.")
            End If
        End Using

        Using cmd As New MySqlCommand(
            "INSERT INTO guest_folios " &
            "(booking_id, folio_status, opened_at) " &
            "VALUES (@booking, 'OPEN', NOW());",
            conn, tx)

            cmd.Parameters.AddWithValue("@booking", bookingId)
            cmd.ExecuteNonQuery()

            Return Convert.ToInt32(cmd.LastInsertedId)
        End Using
    End Function

    '========================================================
    ' AUDIT LOG
    '========================================================

    Private Sub WriteAuditLog(
        conn As MySqlConnection,
        tx As MySqlTransaction,
        action As String,
        recordId As Integer,
        details As String
    )
        Const sql As String =
            "INSERT INTO audit_logs " &
            "(user_id, action, table_name, record_id, details) " &
            "VALUES (@user, @action, 'parking_assignments', @record, @details);"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@user", FrmLogin.CurrentUserId)
            cmd.Parameters.AddWithValue("@action", action)
            cmd.Parameters.AddWithValue("@record", recordId)
            cmd.Parameters.AddWithValue("@details", details)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub Refresh_Click(sender As Object, e As EventArgs)
        LoadBookings()
        LoadAvailableSpaces()
        LoadAssignments()
        lblMessage.Text = "Parking information refreshed."
    End Sub

    '========================================================
    ' HELPERS
    '========================================================

    Private Function Peso(amount As Decimal) As String
        Return "₱" & amount.ToString(
            "N2", CultureInfo.GetCultureInfo("en-PH"))
    End Function

    Private Sub ShowError(message As String, ex As Exception)
        lblMessage.Text = message

        MessageBox.Show(
            message & Environment.NewLine & ex.Message,
            "StayPoint HMS - Parking",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

End Class
