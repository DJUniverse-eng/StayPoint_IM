
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

'============================================================
' STAFF BOOKINGS
'============================================================

Public Class ucStaffBookings
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly MutedColor As Color = Color.FromArgb(107, 114, 128)
    Private ReadOnly BorderColor As Color = Color.FromArgb(226, 232, 240)

    Private dgvBookings As DataGridView
    Private txtSearch As TextBox
    Private cboStatus As ComboBox
    Private lblMessage As Label
    Private lblRecordCount As Label

    Private lblTotal As Label
    Private lblPending As Label
    Private lblConfirmed As Label
    Private lblCancelled As Label

    Private sourceTable As DataTable
    Private isLoading As Boolean

    Public Sub New()
        BuildInterface()
        AddHandler Me.Load, AddressOf StaffBookings_Load
    End Sub

    Private Sub StaffBookings_Load(sender As Object, e As EventArgs)
        LoadBookings()
    End Sub

    Private Sub BuildInterface()

        Me.Controls.Clear()
        Me.Dock = DockStyle.Fill
        Me.BackColor = BgColor
        Me.Padding = New Padding(20)

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .BackColor = BgColor
        }

        Dim title As New Label With {
            .Text = "Bookings & Walk-ins",
            .Font = New Font("Segoe UI", 21, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 4)
        }

        Dim subtitle As New Label With {
            .Text = "Register walk-in guests and review booking records.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(3, 47)
        }

        Dim btnWalkIn As New Button With {
            .Text = "+ Register Walk-in",
            .Size = New Size(165, 38),
            .BackColor = Teal,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        btnWalkIn.FlatAppearance.BorderSize = 0
        AddHandler btnWalkIn.Click, AddressOf OpenWalkInForm

        header.Controls.AddRange(New Control() {
            title, subtitle, btnWalkIn
        })

        AddHandler header.Resize,
            Sub()
                btnWalkIn.Location = New Point(
                    header.ClientSize.Width - btnWalkIn.Width - 4, 12)
            End Sub

        Dim summary As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 100,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = BgColor,
            .Padding = New Padding(0, 5, 0, 5)
        }

        lblTotal = New Label()
        lblPending = New Label()
        lblConfirmed = New Label()
        lblCancelled = New Label()

        summary.Controls.Add(CreateSummaryCard("Total Bookings", lblTotal, Navy))
        summary.Controls.Add(CreateSummaryCard("Pending", lblPending, Color.DarkOrange))
        summary.Controls.Add(CreateSummaryCard("Confirmed", lblConfirmed, Teal))
        summary.Controls.Add(CreateSummaryCard("Cancelled", lblCancelled, Color.Firebrick))

        Dim content As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(14)
        }

        Dim tableHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 40,
            .BackColor = Color.White
        }

        Dim tableTitle As New Label With {
            .Text = "Reservation Records",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 7)
        }

        lblRecordCount = New Label With {
            .Text = "0 records",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        tableHeader.Controls.Add(tableTitle)
        tableHeader.Controls.Add(lblRecordCount)

        AddHandler tableHeader.Resize,
            Sub()
                lblRecordCount.Left =
                    tableHeader.ClientSize.Width - lblRecordCount.Width - 4
            End Sub

        Dim filters As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 72,
            .BackColor = Color.White
        }

        txtSearch = New TextBox With {
            .Location = New Point(0, 25),
            .Size = New Size(245, 30),
            .Font = New Font("Segoe UI", 10)
        }

        cboStatus = New ComboBox With {
            .Location = New Point(260, 25),
            .Size = New Size(165, 30),
            .Font = New Font("Segoe UI", 9),
            .DropDownStyle = ComboBoxStyle.DropDownList
        }

        cboStatus.Items.AddRange(New Object() {
            "All Statuses", "PENDING", "CONFIRMED",
            "CHECKED_IN", "CHECKED_OUT", "CANCELLED", "NO_SHOW"
        })
        cboStatus.SelectedIndex = 0

        Dim btnRefresh As New Button With {
            .Text = "Refresh",
            .Location = New Point(440, 24),
            .Size = New Size(90, 32),
            .BackColor = Navy,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        btnRefresh.FlatAppearance.BorderSize = 0

        filters.Controls.Add(New Label With {
            .Text = "Search guest, reference, or booking ID",
            .Location = New Point(0, 3),
            .AutoSize = True,
            .ForeColor = Navy
        })
        filters.Controls.Add(txtSearch)
        filters.Controls.Add(New Label With {
            .Text = "Status",
            .Location = New Point(260, 3),
            .AutoSize = True,
            .ForeColor = Navy
        })
        filters.Controls.Add(cboStatus)
        filters.Controls.Add(btnRefresh)

        dgvBookings = New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .GridColor = BorderColor,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .EnableHeadersVisualStyles = False
        }

        dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvBookings.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)
        dgvBookings.ColumnHeadersHeight = 38
        dgvBookings.DefaultCellStyle.Font = New Font("Segoe UI", 9)
        dgvBookings.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)
        dgvBookings.DefaultCellStyle.SelectionForeColor = Navy
        dgvBookings.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        lblMessage = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 32,
            .ForeColor = Color.Firebrick,
            .Font = New Font("Segoe UI", 9),
            .Visible = False
        }

        content.Controls.Add(dgvBookings)
        content.Controls.Add(lblMessage)
        content.Controls.Add(filters)
        content.Controls.Add(tableHeader)

        Me.Controls.Add(content)
        Me.Controls.Add(summary)
        Me.Controls.Add(header)

        AddHandler txtSearch.TextChanged, AddressOf ApplyFilters
        AddHandler cboStatus.SelectedIndexChanged, AddressOf ApplyFilters
        AddHandler btnRefresh.Click, AddressOf RefreshBookings

    End Sub

    Private Function CreateSummaryCard(
        caption As String,
        valueLabel As Label,
        accent As Color
    ) As Panel

        Dim card As New Panel With {
            .Size = New Size(195, 78),
            .BackColor = Color.White,
            .Margin = New Padding(0, 0, 10, 0),
            .Padding = New Padding(10)
        }

        AddHandler card.Paint,
            Sub(sender As Object, e As PaintEventArgs)
                Using p As New Pen(BorderColor)
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1)
                End Using
                Using b As New SolidBrush(accent)
                    e.Graphics.FillRectangle(b, 0, 0, 4, card.Height)
                End Using
            End Sub

        card.Controls.Add(New Label With {
            .Text = caption,
            .Location = New Point(12, 8),
            .AutoSize = True,
            .ForeColor = MutedColor
        })

        valueLabel.Text = "0"
        valueLabel.Location = New Point(12, 30)
        valueLabel.AutoSize = True
        valueLabel.Font = New Font("Segoe UI", 19, FontStyle.Bold)
        valueLabel.ForeColor = accent

        card.Controls.Add(valueLabel)
        Return card

    End Function

    '========================================================
    ' LOAD BOOKINGS USING ACTUAL DATABASE COLUMN NAMES
    '========================================================

    Private Sub LoadBookings()

        If isLoading Then Return
        isLoading = True

        Try
            lblMessage.Visible = False

            Using conn As MySqlConnection = DbConnection.GetConnection()
                If conn.State <> ConnectionState.Open Then conn.Open()

                Dim sql As String =
                    "SELECT b.booking_id, b.booking_reference, " &
                    "g.first_name, g.last_name, b.check_in_date, " &
                    "b.check_out_date, b.booking_status, b.adults, b.children " &
                    "FROM bookings b " &
                    "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                    "ORDER BY b.booking_date DESC, b.booking_id DESC"

                Using adapter As New MySqlDataAdapter(sql, conn)
                    sourceTable = New DataTable()
                    adapter.Fill(sourceTable)
                End Using
            End Using

            UpdateSummary()
            ApplyFilters(Nothing, EventArgs.Empty)

        Catch ex As Exception
            ShowError("Unable to load booking records.", ex)
        Finally
            isLoading = False
        End Try

    End Sub

    Private Sub UpdateSummary()

        If sourceTable Is Nothing Then Return

        lblTotal.Text = sourceTable.Rows.Count.ToString()
        lblPending.Text = CountStatus("PENDING").ToString()
        lblConfirmed.Text = CountStatus("CONFIRMED").ToString()
        lblCancelled.Text = CountStatus("CANCELLED").ToString()

    End Sub

    Private Function CountStatus(status As String) As Integer

        If sourceTable Is Nothing Then Return 0

        Dim result As Integer = 0

        For Each row As DataRow In sourceTable.Rows
            If String.Equals(
                Convert.ToString(row("booking_status")),
                status,
                StringComparison.OrdinalIgnoreCase
            ) Then
                result += 1
            End If
        Next

        Return result

    End Function

    Private Sub ApplyFilters(sender As Object, e As EventArgs)

        If sourceTable Is Nothing OrElse txtSearch Is Nothing Then Return

        Dim view As New DataView(sourceTable)
        Dim search As String = txtSearch.Text.Trim().Replace("'", "''")
        Dim status As String = Convert.ToString(cboStatus.SelectedItem)

        Dim filters As New List(Of String)

        If search <> "" Then
            filters.Add(
                "(Convert(booking_id, 'System.String') LIKE '%" & search & "%' " &
                "OR booking_reference LIKE '%" & search & "%' " &
                "OR first_name LIKE '%" & search & "%' " &
                "OR last_name LIKE '%" & search & "%')"
            )
        End If

        If status <> "" AndAlso status <> "All Statuses" Then
            filters.Add("booking_status = '" & status.Replace("'", "''") & "'")
        End If

        view.RowFilter = String.Join(" AND ", filters)
        dgvBookings.DataSource = view

        If dgvBookings.Columns.Contains("booking_id") Then
            dgvBookings.Columns("booking_id").HeaderText = "Booking ID"
        End If
        If dgvBookings.Columns.Contains("booking_reference") Then
            dgvBookings.Columns("booking_reference").HeaderText = "Reference"
        End If
        If dgvBookings.Columns.Contains("first_name") Then
            dgvBookings.Columns("first_name").HeaderText = "First Name"
        End If
        If dgvBookings.Columns.Contains("last_name") Then
            dgvBookings.Columns("last_name").HeaderText = "Last Name"
        End If
        If dgvBookings.Columns.Contains("check_in_date") Then
            dgvBookings.Columns("check_in_date").HeaderText = "Check-In"
            dgvBookings.Columns("check_in_date").DefaultCellStyle.Format =
                "MMM dd, yyyy hh:mm tt"
        End If
        If dgvBookings.Columns.Contains("check_out_date") Then
            dgvBookings.Columns("check_out_date").HeaderText = "Check-Out"
            dgvBookings.Columns("check_out_date").DefaultCellStyle.Format =
                "MMM dd, yyyy hh:mm tt"
        End If
        If dgvBookings.Columns.Contains("booking_status") Then
            dgvBookings.Columns("booking_status").HeaderText = "Status"
        End If

        lblRecordCount.Text = view.Count.ToString() &
            If(view.Count = 1, " record", " records")

    End Sub

    Private Sub RefreshBookings(sender As Object, e As EventArgs)
        LoadBookings()
    End Sub

    Private Sub OpenWalkInForm(sender As Object, e As EventArgs)

        Using form As New FrmStaffWalkInBooking()
            If form.ShowDialog(Me) = DialogResult.OK Then
                LoadBookings()
            End If
        End Using

    End Sub

    Private Sub ShowError(message As String, ex As Exception)
        lblMessage.Text = message & " " & ex.Message
        lblMessage.Visible = True
    End Sub

End Class


'============================================================
' WALK-IN REGISTRATION
' Creates a CONFIRMED booking and room assignment.
' Actual check-in remains in the separate staff module.
'============================================================

Public Class FrmStaffWalkInBooking
    Inherits Form

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly TextMuted As Color = Color.FromArgb(107, 114, 128)

    Private txtFirstName As TextBox
    Private txtLastName As TextBox
    Private txtPhone As TextBox
    Private txtEmail As TextBox
    Private txtAddress As TextBox
    Private txtIdType As TextBox
    Private txtIdNumber As TextBox
    Private txtRequests As TextBox

    Private dtpCheckIn As DateTimePicker
    Private dtpCheckOut As DateTimePicker

    Private nudAdults As NumericUpDown
    Private nudChildren As NumericUpDown

    Private cboRoom As ComboBox
    Private chkParking As CheckBox
    Private cboParking As ComboBox
    Private txtPlate As TextBox
    Private cboVehicleType As ComboBox

    Private lblRoomRate As Label
    Private lblParkingRate As Label
    Private lblEstimatedTotal As Label

    Private btnSave As Button
    Private btnCancel As Button

    Private receiptText As String = ""
    Private WithEvents receiptDocument As New PrintDocument()

    Private Class RoomChoice
        Public RoomId As Integer
        Public RoomNumber As String
        Public RoomTypeId As Integer
        Public Rate As Decimal
        Public MinimumHours As Integer

        Public Overrides Function ToString() As String
            Return RoomNumber & " - " & RoomTypeName(RoomTypeId) &
                " (₱" & Rate.ToString("N2") & "/night)"
        End Function

        Public Function RoomTypeName(typeId As Integer) As String
            Return "Room Type " & typeId.ToString()
        End Function
    End Class

    Private Class ParkingChoice
        Public SpaceId As Integer
        Public SpaceCode As String
        Public RatePerDay As Decimal

        Public Overrides Function ToString() As String
            Return SpaceCode & " (₱" & RatePerDay.ToString("N2") & "/day)"
        End Function
    End Class

    Public Sub New()
        BuildForm()
        LoadRooms()
        LoadParkingSpaces()
        UpdateEstimate()
    End Sub

    '========================================================
    ' FORM
    '========================================================

    Private Sub BuildForm()

        Me.Text = "Register Walk-in Guest"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MinimizeBox = False
        Me.MaximizeBox = True
        Me.MinimumSize = New Size(760, 760)
        Me.ClientSize = New Size(820, 790)
        Me.BackColor = BgColor
        Me.Font = New Font("Segoe UI", 9)

        Dim main As New Panel With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .Padding = New Padding(24),
            .BackColor = BgColor
        }
        Me.Controls.Add(main)

        Dim title As New Label With {
            .Text = "Register Walk-in Guest",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(24, 18)
        }

        Dim subtitle As New Label With {
            .Text = "Register the guest and assign a room. Check-in is completed separately.",
            .ForeColor = TextMuted,
            .AutoSize = True,
            .Location = New Point(27, 56)
        }

        main.Controls.Add(title)
        main.Controls.Add(subtitle)

        Dim y As Integer = 95

        txtFirstName = AddTextField(main, "First Name *", 28, y, 300)
        txtLastName = AddTextField(main, "Last Name *", 355, y, 300)
        y += 66

        txtPhone = AddTextField(main, "Phone", 28, y, 300)
        txtEmail = AddTextField(main, "Email", 355, y, 300)
        y += 66

        txtAddress = AddTextField(main, "Address", 28, y, 627)
        y += 66

        txtIdType = AddTextField(main, "ID Type", 28, y, 300)
        txtIdNumber = AddTextField(main, "ID Number", 355, y, 300)
        y += 66

        dtpCheckIn = AddTimeField(main, "Check-In Time *", 28, y, 300)
        dtpCheckOut = AddTimeField(main, "Expected Check-Out Time *", 355, y, 300)

        dtpCheckIn.Value = DateTime.Now
        dtpCheckOut.Value = DateTime.Now.AddHours(2)

        AddHandler dtpCheckIn.ValueChanged,
            Sub()
                UpdateEstimate()
            End Sub

        AddHandler dtpCheckOut.ValueChanged,
            Sub()
                UpdateEstimate()
            End Sub

        y += 66

        main.Controls.Add(New Label With {
            .Text = "Adults *",
            .Location = New Point(28, y),
            .AutoSize = True,
            .ForeColor = Navy
        })

        nudAdults = New NumericUpDown With {
            .Location = New Point(28, y + 22),
            .Size = New Size(120, 29),
            .Minimum = 1,
            .Maximum = 20,
            .Value = 1
        }

        main.Controls.Add(nudAdults)

        main.Controls.Add(New Label With {
            .Text = "Children",
            .Location = New Point(180, y),
            .AutoSize = True,
            .ForeColor = Navy
        })

        nudChildren = New NumericUpDown With {
            .Location = New Point(180, y + 22),
            .Size = New Size(120, 29),
            .Minimum = 0,
            .Maximum = 20,
            .Value = 0
        }

        main.Controls.Add(nudChildren)

        y += 64

        main.Controls.Add(New Label With {
            .Text = "Available Room *",
            .Location = New Point(28, y),
            .AutoSize = True,
            .ForeColor = Navy
        })

        cboRoom = New ComboBox With {
            .Location = New Point(28, y + 22),
            .Size = New Size(420, 31),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }
        main.Controls.Add(cboRoom)

        lblRoomRate = New Label With {
            .Location = New Point(465, y + 27),
            .AutoSize = True,
            .ForeColor = Teal
        }
        main.Controls.Add(lblRoomRate)

        AddHandler cboRoom.SelectedIndexChanged,
            Sub()
                UpdateEstimate()
            End Sub

        y += 65

        chkParking = New CheckBox With {
            .Text = "Assign parking (optional)",
            .Location = New Point(28, y),
            .AutoSize = True,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
        }
        main.Controls.Add(chkParking)

        AddHandler chkParking.CheckedChanged,
            Sub()
                cboParking.Enabled = chkParking.Checked
                txtPlate.Enabled = chkParking.Checked
                cboVehicleType.Enabled = chkParking.Checked
                UpdateEstimate()
            End Sub

        y += 30

        main.Controls.Add(New Label With {
            .Text = "Available Parking Space",
            .Location = New Point(28, y),
            .AutoSize = True,
            .ForeColor = Navy
        })

        cboParking = New ComboBox With {
            .Location = New Point(28, y + 22),
            .Size = New Size(300, 30),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Enabled = False
        }
        main.Controls.Add(cboParking)

        lblParkingRate = New Label With {
            .Location = New Point(355, y + 27),
            .AutoSize = True,
            .ForeColor = Teal
        }
        main.Controls.Add(lblParkingRate)

        y += 62

        txtPlate = AddTextField(main, "Vehicle Plate Number *", 28, y, 300)
        txtPlate.Enabled = False

        main.Controls.Add(New Label With {
            .Text = "Vehicle Type",
            .Location = New Point(355, y),
            .AutoSize = True,
            .ForeColor = Navy
        })

        cboVehicleType = New ComboBox With {
            .Location = New Point(355, y + 22),
            .Size = New Size(300, 30),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Enabled = False
        }
        cboVehicleType.Items.AddRange(New Object() {
            "Car", "Motorcycle", "SUV", "Van", "Other"
        })
        cboVehicleType.SelectedIndex = 0
        main.Controls.Add(cboVehicleType)

        y += 65

        txtRequests = AddTextField(main, "Special Requests", 28, y, 627)
        y += 62

        lblEstimatedTotal = New Label With {
            .Location = New Point(28, y),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = Navy
        }
        main.Controls.Add(lblEstimatedTotal)

        y += 45

        btnSave = New Button With {
            .Text = "Register & Print Receipt",
            .Size = New Size(205, 40),
            .Location = New Point(28, y),
            .BackColor = Teal,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand
        }
        btnSave.FlatAppearance.BorderSize = 0
        AddHandler btnSave.Click, AddressOf SaveWalkIn
        main.Controls.Add(btnSave)

        btnCancel = New Button With {
            .Text = "Cancel",
            .Size = New Size(100, 40),
            .Location = New Point(245, y),
            .BackColor = Color.White,
            .ForeColor = Navy,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }
        AddHandler btnCancel.Click,
            Sub()
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End Sub
        main.Controls.Add(btnCancel)

        AddHandler nudAdults.ValueChanged, Sub() UpdateEstimate()
        AddHandler nudChildren.ValueChanged, Sub() UpdateEstimate()
        AddHandler chkParking.CheckedChanged, Sub() UpdateEstimate()
        AddHandler cboParking.SelectedIndexChanged, Sub() UpdateEstimate()

    End Sub

    Private Function AddTextField(
        parent As Control,
        caption As String,
        x As Integer,
        y As Integer,
        width As Integer
    ) As TextBox

        parent.Controls.Add(New Label With {
            .Text = caption,
            .Location = New Point(x, y),
            .AutoSize = True,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        })

        Dim txt As New TextBox With {
            .Location = New Point(x, y + 22),
            .Size = New Size(width, 30),
            .Font = New Font("Segoe UI", 10)
        }
        parent.Controls.Add(txt)

        Return txt

    End Function

    Private Function AddTimeField(
        parent As Control,
        caption As String,
        x As Integer,
        y As Integer,
        width As Integer
    ) As DateTimePicker

        parent.Controls.Add(New Label With {
            .Text = caption,
            .Location = New Point(x, y),
            .AutoSize = True,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        })

        Dim picker As New DateTimePicker With {
            .Location = New Point(x, y + 22),
            .Size = New Size(width, 30),
            .Format = DateTimePickerFormat.Custom,
            .CustomFormat = "hh:mm tt",
            .ShowUpDown = True
        }
        parent.Controls.Add(picker)

        Return picker

    End Function

    '========================================================
    ' ROOM AND PARKING LISTS
    '========================================================

    Private Sub LoadRooms()

        cboRoom.Items.Clear()

        Using conn As MySqlConnection = DbConnection.GetConnection()
            If conn.State <> ConnectionState.Open Then conn.Open()

            Dim sql As String =
                "SELECT r.room_id, r.room_number, r.room_type_id, " &
                "rt.daily_rate, rt.minimum_hours " &
                "FROM rooms r " &
                "INNER JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                "WHERE r.status = 'AVAILABLE' AND rt.is_active = 1 " &
                "AND NOT EXISTS (" &
                " SELECT 1 FROM booking_rooms br " &
                " INNER JOIN bookings b ON b.booking_id = br.booking_id " &
                " WHERE br.room_id = r.room_id " &
                " AND b.booking_status IN ('PENDING','CONFIRMED','CHECKED_IN') " &
                " AND b.check_in_date < @checkout " &
                " AND b.check_out_date > @checkin " &
                ") ORDER BY r.room_number"

            Using cmd As New MySqlCommand(sql, conn)
                Dim ci As DateTime = DateTime.Now
                Dim co As DateTime = ci.AddHours(2)
                cmd.Parameters.AddWithValue("@checkin", ci)
                cmd.Parameters.AddWithValue("@checkout", co)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim item As New RoomChoice With {
                            .RoomId = Convert.ToInt32(reader("room_id")),
                            .RoomNumber = Convert.ToString(reader("room_number")),
                            .RoomTypeId = Convert.ToInt32(reader("room_type_id")),
                            .Rate = Convert.ToDecimal(reader("daily_rate")),
                            .MinimumHours = Convert.ToInt32(reader("minimum_hours"))
                        }
                        cboRoom.Items.Add(item)
                    End While
                End Using
            End Using
        End Using

        If cboRoom.Items.Count > 0 Then
            cboRoom.SelectedIndex = 0
        Else
            lblRoomRate.Text = "No available rooms"
        End If

    End Sub

    Private Sub LoadParkingSpaces()

        cboParking.Items.Clear()

        Using conn As MySqlConnection = DbConnection.GetConnection()
            If conn.State <> ConnectionState.Open Then conn.Open()

            Dim sql As String =
                "SELECT parking_space_id, space_code, rate_per_day " &
                "FROM parking_spaces " &
                "WHERE status = 'AVAILABLE' ORDER BY space_code"

            Using cmd As New MySqlCommand(sql, conn)
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        cboParking.Items.Add(New ParkingChoice With {
                            .SpaceId = Convert.ToInt32(reader("parking_space_id")),
                            .SpaceCode = Convert.ToString(reader("space_code")),
                            .RatePerDay = Convert.ToDecimal(reader("rate_per_day"))
                        })
                    End While
                End Using
            End Using
        End Using

        If cboParking.Items.Count > 0 Then
            cboParking.SelectedIndex = 0
        Else
            lblParkingRate.Text = "No available parking"
        End If

    End Sub

    Private Function GetStayDates() As Tuple(Of DateTime, DateTime)

        Dim checkIn As DateTime = DateTime.Now
        Dim selectedCheckOut As TimeSpan = dtpCheckOut.Value.TimeOfDay

        Dim checkOut As DateTime = checkIn.Date.Add(selectedCheckOut)

        If checkOut <= checkIn Then
            checkOut = checkOut.AddDays(1)
        End If

        Return Tuple.Create(checkIn, checkOut)

    End Function

    Private Function CalculateNights(
        checkIn As DateTime,
        checkOut As DateTime
    ) As Integer

        Dim hours As Double = (checkOut - checkIn).TotalHours
        Return Math.Max(1, CInt(Math.Ceiling(hours / 24.0)))

    End Function

    Private Function GetRoomTotal() As Decimal

        Dim room As RoomChoice = TryCast(cboRoom.SelectedItem, RoomChoice)
        If room Is Nothing Then Return 0D

        Dim dates = GetStayDates()
        Dim nights As Integer = CalculateNights(dates.Item1, dates.Item2)

        Return room.Rate * nights

    End Function

    Private Function GetParkingTotal() As Decimal

        If Not chkParking.Checked Then Return 0D

        Dim parking As ParkingChoice =
            TryCast(cboParking.SelectedItem, ParkingChoice)

        If parking Is Nothing Then Return 0D

        Dim dates = GetStayDates()
        Dim nights As Integer = CalculateNights(dates.Item1, dates.Item2)

        Return parking.RatePerDay * nights

    End Function

    Private Sub UpdateEstimate()

        If lblRoomRate Is Nothing Then Return

        Dim room As RoomChoice = TryCast(cboRoom.SelectedItem, RoomChoice)

        If room IsNot Nothing Then
            lblRoomRate.Text = "Daily rate: ₱" & room.Rate.ToString("N2")
        End If

        Dim parking As ParkingChoice =
            TryCast(cboParking.SelectedItem, ParkingChoice)

        If parking IsNot Nothing Then
            lblParkingRate.Text = "Daily rate: ₱" & parking.RatePerDay.ToString("N2")
        Else
            lblParkingRate.Text = If(chkParking.Checked, "Select parking", "Not selected")
        End If

        lblEstimatedTotal.Text =
            "Estimated Charges: ₱" &
            (GetRoomTotal() + GetParkingTotal()).ToString("N2") &
            "     |     Payment received: ₱0.00"

    End Sub

    '========================================================
    ' SAVE WALK-IN TRANSACTION
    '========================================================

    Private Sub SaveWalkIn(sender As Object, e As EventArgs)

        If String.IsNullOrWhiteSpace(txtFirstName.Text) OrElse
           String.IsNullOrWhiteSpace(txtLastName.Text) Then

            MessageBox.Show(
                "Enter the guest's first and last name.",
                "Required Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return
        End If

        Dim room As RoomChoice = TryCast(cboRoom.SelectedItem, RoomChoice)

        If room Is Nothing Then
            MessageBox.Show(
                "Select an available room before registering the walk-in.",
                "Room Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return
        End If

        If chkParking.Checked Then
            If cboParking.SelectedItem Is Nothing OrElse
               String.IsNullOrWhiteSpace(txtPlate.Text) Then
                MessageBox.Show(
                    "Select a parking space and enter the vehicle plate number.",
                    "Parking Details Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                )
                Return
            End If
        End If

        Dim dates = GetStayDates()
        Dim checkIn As DateTime = dates.Item1
        Dim checkOut As DateTime = dates.Item2
        Dim nights As Integer = CalculateNights(checkIn, checkOut)

        If checkOut <= checkIn Then
            MessageBox.Show("Check-out must be later than check-in.")
            Return
        End If

        Dim roomTotal As Decimal = room.Rate * nights
        Dim parking As ParkingChoice =
            TryCast(cboParking.SelectedItem, ParkingChoice)

        Dim parkingTotal As Decimal = 0D
        If chkParking.Checked AndAlso parking IsNot Nothing Then
            parkingTotal = parking.RatePerDay * nights
        End If

        Dim total As Decimal = roomTotal + parkingTotal
        Dim bookingReference As String =
            "SP-" & Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant()

        Dim savedBookingId As Integer = 0
        Dim savedFolioId As Integer = 0

        Try
            btnSave.Enabled = False

            Using conn As MySqlConnection = DbConnection.GetConnection()
                If conn.State <> ConnectionState.Open Then conn.Open()

                Using tx As MySqlTransaction = conn.BeginTransaction()

                    Try
                        '------------------------------------
                        ' 1. Recheck room availability
                        '------------------------------------
                        Dim roomCheckSql As String =
                            "SELECT r.status FROM rooms r " &
                            "WHERE r.room_id = @roomId FOR UPDATE"

                        Using cmd As New MySqlCommand(roomCheckSql, conn, tx)
                            cmd.Parameters.AddWithValue("@roomId", room.RoomId)

                            Dim roomStatus As Object = cmd.ExecuteScalar()

                            If roomStatus Is Nothing OrElse
                               roomStatus Is DBNull.Value OrElse
                               Not String.Equals(
                                   Convert.ToString(roomStatus),
                                   "AVAILABLE",
                                   StringComparison.OrdinalIgnoreCase
                               ) Then
                                Throw New Exception(
                                    "The selected room is no longer available. Refresh and select another room."
                                )
                            End If
                        End Using

                        ' Check overlapping bookings after locking the room.
                        Dim overlapSql As String =
                            "SELECT COUNT(*) FROM booking_rooms br " &
                            "INNER JOIN bookings b ON b.booking_id = br.booking_id " &
                            "WHERE br.room_id = @roomId " &
                            "AND b.booking_status IN ('PENDING','CONFIRMED','CHECKED_IN') " &
                            "AND b.check_in_date < @checkout " &
                            "AND b.check_out_date > @checkin"

                        Using cmd As New MySqlCommand(overlapSql, conn, tx)
                            cmd.Parameters.AddWithValue("@roomId", room.RoomId)
                            cmd.Parameters.AddWithValue("@checkin", checkIn)
                            cmd.Parameters.AddWithValue("@checkout", checkOut)

                            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                                Throw New Exception(
                                    "The selected room already has an overlapping booking."
                                )
                            End If
                        End Using

                        '------------------------------------
                        ' 2. Insert guest
                        '------------------------------------
                        Dim guestId As Integer

                        Dim guestSql As String =
                            "INSERT INTO guests " &
                            "(first_name, last_name, email, phone, address, id_type, id_number) " &
                            "VALUES (@firstName, @lastName, @email, @phone, @address, @idType, @idNumber)"

                        Using cmd As New MySqlCommand(guestSql, conn, tx)
                            cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim())
                            cmd.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim())
                            cmd.Parameters.AddWithValue("@email", DbValue(txtEmail.Text))
                            cmd.Parameters.AddWithValue("@phone", DbValue(txtPhone.Text))
                            cmd.Parameters.AddWithValue("@address", DbValue(txtAddress.Text))
                            cmd.Parameters.AddWithValue("@idType", DbValue(txtIdType.Text))
                            cmd.Parameters.AddWithValue("@idNumber", DbValue(txtIdNumber.Text))
                            cmd.ExecuteNonQuery()
                        End Using

                        Using cmd As New MySqlCommand(
                            "SELECT LAST_INSERT_ID()", conn, tx)
                            guestId = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using

                        '------------------------------------
                        ' 3. Insert booking as CONFIRMED
                        '    Actual check-in is a separate action.
                        '------------------------------------
                        Dim bookingSql As String =
                            "INSERT INTO bookings " &
                            "(booking_reference, guest_id, booking_date, " &
                            "check_in_date, check_out_date, adults, children, " &
                            "booking_status, special_requests) " &
                            "VALUES (@reference, @guestId, NOW(), " &
                            "@checkIn, @checkOut, @adults, @children, " &
                            "'CONFIRMED', @requests)"

                        Using cmd As New MySqlCommand(bookingSql, conn, tx)
                            cmd.Parameters.AddWithValue("@reference", bookingReference)
                            cmd.Parameters.AddWithValue("@guestId", guestId)
                            cmd.Parameters.AddWithValue("@checkIn", checkIn)
                            cmd.Parameters.AddWithValue("@checkOut", checkOut)
                            cmd.Parameters.AddWithValue("@adults", Convert.ToInt32(nudAdults.Value))
                            cmd.Parameters.AddWithValue("@children", Convert.ToInt32(nudChildren.Value))
                            cmd.Parameters.AddWithValue("@requests", DbValue(txtRequests.Text))
                            cmd.ExecuteNonQuery()
                        End Using

                        Using cmd As New MySqlCommand(
                            "SELECT LAST_INSERT_ID()", conn, tx)
                            savedBookingId = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using

                        '------------------------------------
                        ' 4. Insert room assignment
                        '------------------------------------
                        Dim roomSql As String =
                            "INSERT INTO booking_rooms " &
                            "(booking_id, room_id, nightly_rate, number_of_nights) " &
                            "VALUES (@bookingId, @roomId, @rate, @nights)"

                        Using cmd As New MySqlCommand(roomSql, conn, tx)
                            cmd.Parameters.AddWithValue("@bookingId", savedBookingId)
                            cmd.Parameters.AddWithValue("@roomId", room.RoomId)
                            cmd.Parameters.AddWithValue("@rate", room.Rate)
                            cmd.Parameters.AddWithValue("@nights", nights)
                            cmd.ExecuteNonQuery()
                        End Using

                        '------------------------------------
                        ' 5. Create folio
                        '------------------------------------
                        Using cmd As New MySqlCommand(
                            "INSERT INTO guest_folios (booking_id, folio_status) " &
                            "VALUES (@bookingId, 'OPEN')", conn, tx)

                            cmd.Parameters.AddWithValue("@bookingId", savedBookingId)
                            cmd.ExecuteNonQuery()
                        End Using

                        Using cmd As New MySqlCommand(
                            "SELECT LAST_INSERT_ID()", conn, tx)
                            savedFolioId = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using

                        '------------------------------------
                        ' 6. Add room charge
                        '------------------------------------
                        InsertFolioCharge(
                            conn, tx, savedFolioId,
                            "ROOM",
                            "Room " & room.RoomNumber & " (" & nights & " night(s))",
                            CDec(nights), room.Rate, roomTotal
                        )

                        '------------------------------------
                        ' 7. Optional parking assignment
                        '------------------------------------
                        If chkParking.Checked AndAlso parking IsNot Nothing Then

                            Using cmd As New MySqlCommand(
                                "SELECT status FROM parking_spaces " &
                                "WHERE parking_space_id = @spaceId FOR UPDATE",
                                conn, tx)

                                cmd.Parameters.AddWithValue("@spaceId", parking.SpaceId)

                                Dim statusValue As Object = cmd.ExecuteScalar()

                                If statusValue Is Nothing OrElse
                                   statusValue Is DBNull.Value OrElse
                                   Not String.Equals(
                                       Convert.ToString(statusValue),
                                       "AVAILABLE",
                                       StringComparison.OrdinalIgnoreCase
                                   ) Then
                                    Throw New Exception(
                                        "The selected parking space is no longer available."
                                    )
                                End If
                            End Using

                            Using cmd As New MySqlCommand(
                                "INSERT INTO parking_assignments " &
                                "(booking_id, parking_space_id, vehicle_plate, " &
                                "vehicle_type, start_time, end_time, billing_type, " &
                                "agreed_rate, assignment_status) " &
                                "VALUES (@bookingId, @spaceId, @plate, @vehicleType, " &
                                "@startTime, @endTime, 'DAILY', @rate, 'ACTIVE')",
                                conn, tx)

                                cmd.Parameters.AddWithValue("@bookingId", savedBookingId)
                                cmd.Parameters.AddWithValue("@spaceId", parking.SpaceId)
                                cmd.Parameters.AddWithValue("@plate", txtPlate.Text.Trim())
                                cmd.Parameters.AddWithValue("@vehicleType",
                                    Convert.ToString(cboVehicleType.SelectedItem))
                                cmd.Parameters.AddWithValue("@startTime", checkIn)
                                cmd.Parameters.AddWithValue("@endTime", checkOut)
                                cmd.Parameters.AddWithValue("@rate", parking.RatePerDay)
                                cmd.ExecuteNonQuery()
                            End Using

                            ' Mark the space reserved until staff handles check-in.
                            Using cmd As New MySqlCommand(
                                "UPDATE parking_spaces SET status = 'RESERVED' " &
                                "WHERE parking_space_id = @spaceId AND status = 'AVAILABLE'",
                                conn, tx)

                                cmd.Parameters.AddWithValue("@spaceId", parking.SpaceId)

                                If cmd.ExecuteNonQuery() <> 1 Then
                                    Throw New Exception(
                                        "Parking availability changed. Please refresh and try again."
                                    )
                                End If
                            End Using

                            InsertFolioCharge(
                                conn, tx, savedFolioId,
                                "PARKING",
                                "Parking " & parking.SpaceCode & " (" & nights & " day(s))",
                                CDec(nights), parking.RatePerDay, parkingTotal
                            )
                        End If

                        tx.Commit()

                    Catch
                        Try
                            tx.Rollback()
                        Catch
                            ' Preserve the original database error.
                        End Try
                        Throw
                    End Try

                End Using
            End Using

            '------------------------------------
            ' 8. Prepare registration receipt.
            ' No payment is recorded here.
            '------------------------------------
            receiptText = BuildRegistrationReceipt(
                bookingReference,
                savedBookingId,
                savedFolioId,
                checkIn,
                checkOut,
                room,
                parking,
                nights,
                roomTotal,
                If(chkParking.Checked, parkingTotal, 0D),
                total
            )

            MessageBox.Show(
                "Walk-in registration completed." & Environment.NewLine &
                "Booking reference: " & bookingReference & Environment.NewLine &
                "Status: CONFIRMED" & Environment.NewLine &
                "Payment received: ₱0.00" & Environment.NewLine &
                "Balance due: ₱" & total.ToString("N2") & Environment.NewLine &
                Environment.NewLine &
                "The guest must still be checked in through the separate staff module.",
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            ' Print preview is offered after the database transaction commits.
            Using preview As New PrintPreviewDialog()
                preview.Document = receiptDocument
                preview.Width = 850
                preview.Height = 700
                preview.StartPosition = FormStartPosition.CenterParent
                preview.ShowDialog(Me)
            End Using

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to register the walk-in." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "Registration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        Finally
            btnSave.Enabled = True
        End Try

    End Sub

    Private Sub InsertFolioCharge(
        conn As MySqlConnection,
        tx As MySqlTransaction,
        folioId As Integer,
        chargeType As String,
        description As String,
        quantity As Decimal,
        unitPrice As Decimal,
        amount As Decimal
    )

        Dim sql As String =
            "INSERT INTO folio_charges " &
            "(folio_id, charge_type, description, quantity, unit_price, amount) " &
            "VALUES (@folioId, @type, @description, @quantity, @unitPrice, @amount)"

        Using cmd As New MySqlCommand(sql, conn, tx)
            cmd.Parameters.AddWithValue("@folioId", folioId)
            cmd.Parameters.AddWithValue("@type", chargeType)
            cmd.Parameters.AddWithValue("@description", description)
            cmd.Parameters.AddWithValue("@quantity", quantity)
            cmd.Parameters.AddWithValue("@unitPrice", unitPrice)
            cmd.Parameters.AddWithValue("@amount", amount)
            cmd.ExecuteNonQuery()
        End Using

    End Sub

    Private Function BuildRegistrationReceipt(
        bookingReference As String,
        bookingId As Integer,
        folioId As Integer,
        checkIn As DateTime,
        checkOut As DateTime,
        room As RoomChoice,
        parking As ParkingChoice,
        nights As Integer,
        roomTotal As Decimal,
        parkingTotal As Decimal,
        total As Decimal
    ) As String

        Dim result As New System.Text.StringBuilder()

        result.AppendLine("                 STAYPOINT HOTEL")
        result.AppendLine("          WALK-IN REGISTRATION RECEIPT")
        result.AppendLine(New String("="c, 44))
        result.AppendLine("Receipt date: " & DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"))
        result.AppendLine("Booking reference: " & bookingReference)
        result.AppendLine("Booking ID: " & bookingId.ToString())
        result.AppendLine("Folio ID: " & folioId.ToString())
        result.AppendLine("Status: CONFIRMED - NOT YET CHECKED IN")
        result.AppendLine(New String("-"c, 44))
        result.AppendLine("Guest: " & txtFirstName.Text.Trim() & " " & txtLastName.Text.Trim())
        result.AppendLine("Phone: " & txtPhone.Text.Trim())
        result.AppendLine("Room: " & room.RoomNumber)
        result.AppendLine("Check-in: " & checkIn.ToString("MMM dd, yyyy hh:mm tt"))
        result.AppendLine("Expected check-out: " & checkOut.ToString("MMM dd, yyyy hh:mm tt"))
        result.AppendLine("Duration: " & nights.ToString() & " night(s)")
        result.AppendLine(New String("-"c, 44))
        result.AppendLine("Room charges:                 ₱" & roomTotal.ToString("N2"))

        If chkParking.Checked AndAlso parking IsNot Nothing Then
            result.AppendLine("Parking space: " & parking.SpaceCode)
            result.AppendLine("Vehicle plate: " & txtPlate.Text.Trim())
            result.AppendLine("Parking charges:              ₱" & parkingTotal.ToString("N2"))
        Else
            result.AppendLine("Parking: Not requested")
        End If

        result.AppendLine(New String("-"c, 44))
        result.AppendLine("TOTAL CHARGES:                 ₱" & total.ToString("N2"))
        result.AppendLine("PAYMENT RECEIVED:              ₱0.00")
        result.AppendLine("BALANCE DUE:                   ₱" & total.ToString("N2"))
        result.AppendLine(New String("="c, 44))
        result.AppendLine("This is a registration receipt, not a payment receipt.")
        result.AppendLine("A separate payment receipt is issued when payment is recorded.")
        result.AppendLine()
        result.AppendLine("         Thank you for choosing StayPoint!")

        Return result.ToString()

    End Function

    '========================================================
    ' PRINTING
    '========================================================

    Private Sub ReceiptDocument_PrintPage(
        sender As Object,
        e As PrintPageEventArgs
    ) Handles receiptDocument.PrintPage

        Using font As New Font("Consolas", 9)
            Using brush As New SolidBrush(Color.Black)
                e.Graphics.DrawString(
                    receiptText,
                    font,
                    brush,
                    e.MarginBounds.Left,
                    e.MarginBounds.Top
                )
            End Using
        End Using

        e.HasMorePages = False

    End Sub

    Private Function DbValue(value As String) As Object

        If String.IsNullOrWhiteSpace(value) Then
            Return DBNull.Value
        End If

        Return value.Trim()

    End Function

End Class
