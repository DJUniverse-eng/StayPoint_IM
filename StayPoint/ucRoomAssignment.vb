
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucRoomAssignment
    Inherits UserControl

    '========================================================
    ' COLORS
    '========================================================

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly BorderColor As Color = Color.FromArgb(226, 232, 240)
    Private ReadOnly MutedColor As Color = Color.FromArgb(107, 114, 128)
    Private ReadOnly TextColor As Color = Color.FromArgb(51, 65, 85)
    Private ReadOnly DangerColor As Color = Color.FromArgb(185, 28, 28)

    '========================================================
    ' CONTROLS
    '========================================================

    Private dgvBookings As DataGridView
    Private dgvRooms As DataGridView

    Private txtSearch As TextBox
    Private cboBookingStatus As ComboBox
    Private cboRoomType As ComboBox
    Private txtNotes As TextBox

    Private lblBookingCount As Label
    Private lblRoomCount As Label
    Private lblMessage As Label

    Private lblGuest As Label
    Private lblBookingReference As Label
    Private lblBookingStatus As Label
    Private lblArrival As Label
    Private lblDeparture As Label
    Private lblGuests As Label

    Private lblSelectedRoom As Label
    Private lblSelectedRoomType As Label
    Private lblSelectedRate As Label
    Private lblAvailability As Label

    Private btnRefresh As Button
    Private btnAssign As Button
    Private btnCancel As Button

    Private bookingTable As DataTable
    Private roomTable As DataTable

    Private selectedBookingId As Integer = 0
    Private selectedRoomId As Integer = 0

    Private selectedCheckIn As DateTime
    Private selectedCheckOut As DateTime

    Private selectedBookingReference As String = ""
    Private selectedGuestName As String = ""

    Private selectedRoomNumber As String = ""
    Private selectedRoomTypeName As String = ""
    Private selectedRoomRate As Decimal = 0D

    Private isLoading As Boolean = False

    '========================================================
    ' CONSTRUCTOR
    '========================================================

    Public Sub New()

        InitializeComponent()
        BuildInterface()

        AddHandler Me.Load, AddressOf RoomAssignment_Load

    End Sub

    Private Sub RoomAssignment_Load(
        sender As Object,
        e As EventArgs
    )

        RefreshAll()

    End Sub

    '========================================================
    ' MAIN INTERFACE
    '========================================================

    Private Sub BuildInterface()

        Me.SuspendLayout()

        Me.Controls.Clear()
        Me.Dock = DockStyle.Fill
        Me.BackColor = BgColor
        Me.Padding = New Padding(16)
        Me.AutoScroll = True

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = BgColor,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }

        root.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 78)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 32)
        )

        '----------------------------------------------------
        ' HEADER
        '----------------------------------------------------

        Dim header As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = BgColor,
            .Margin = New Padding(0)
        }

        Dim lblTitle As New Label With {
            .Text = "Room Assignment",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 0)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Assign available rooms to eligible bookings and walk-in guests.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(3, 42)
        }

        btnRefresh = CreateButton(
            "Refresh",
            Navy,
            Color.White,
            105,
            36
        )

        btnRefresh.Anchor =
            AnchorStyles.Top Or AnchorStyles.Right

        AddHandler btnRefresh.Click,
            AddressOf RefreshButton_Click

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblSubtitle)
        header.Controls.Add(btnRefresh)

        AddHandler header.Resize,
            Sub(sender As Object, e As EventArgs)

                btnRefresh.Location = New Point(
                    header.ClientSize.Width - btnRefresh.Width,
                    8
                )

            End Sub

        '----------------------------------------------------
        ' THREE-COLUMN WORKSPACE
        '----------------------------------------------------

        Dim workspace As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 3,
            .RowCount = 1,
            .BackColor = BgColor,
            .Margin = New Padding(0)
        }

        ' Use minimum widths to keep all three panels usable.
        workspace.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 40.0F)
        )

        workspace.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 29.0F)
        )

        workspace.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 31.0F)
        )

        workspace.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        Dim bookingPanel As Panel = BuildBookingPanel()
        Dim roomsPanel As Panel = BuildRoomsPanel()
        Dim assignmentPanel As Panel = BuildAssignmentPanel()

        bookingPanel.Margin = New Padding(0, 0, 8, 0)
        roomsPanel.Margin = New Padding(0, 0, 8, 0)
        assignmentPanel.Margin = New Padding(0)

        workspace.Controls.Add(bookingPanel, 0, 0)
        workspace.Controls.Add(roomsPanel, 1, 0)
        workspace.Controls.Add(assignmentPanel, 2, 0)

        '----------------------------------------------------
        ' MESSAGE BAR
        '----------------------------------------------------

        lblMessage = New Label With {
            .Dock = DockStyle.Fill,
            .Text = "Ready.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .TextAlign = ContentAlignment.MiddleLeft,
            .AutoEllipsis = True,
            .Margin = New Padding(0, 5, 0, 0)
        }

        root.Controls.Add(header, 0, 0)
        root.Controls.Add(workspace, 0, 1)
        root.Controls.Add(lblMessage, 0, 2)

        Me.Controls.Add(root)

        Me.ResumeLayout(True)

    End Sub

    '========================================================
    ' BOOKING PANEL
    '========================================================

    Private Function BuildBookingPanel() As Panel

        Dim panel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12),
            .MinimumSize = New Size(280, 300)
        }

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 5,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        layout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 34)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 94)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 42)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 25)
        )

        Dim title As Label = CreateSectionTitle("Booking List")

        ' Search and status filters.
        Dim filterPanel As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        filterPanel.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        filterPanel.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 44)
        )

        filterPanel.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 45)
        )

        txtSearch = New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 9),
            .BorderStyle = BorderStyle.FixedSingle,
            .Text = "Search booking number or guest name...",
            .Margin = New Padding(0, 4, 0, 4)
        }

        cboBookingStatus = New ComboBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 9),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Margin = New Padding(0, 3, 0, 5)
        }

        cboBookingStatus.Items.AddRange(
            New Object() {
                "All Eligible",
                "PENDING",
                "CONFIRMED"
            }
        )

        cboBookingStatus.SelectedIndex = 0

        AddHandler txtSearch.TextChanged,
            AddressOf BookingFilters_Changed

        AddHandler cboBookingStatus.SelectedIndexChanged,
            AddressOf BookingFilters_Changed

        filterPanel.Controls.Add(txtSearch, 0, 0)
        filterPanel.Controls.Add(cboBookingStatus, 0, 1)

        dgvBookings = CreateGrid()

        dgvBookings.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvBookings.ColumnHeadersHeight = 44
        dgvBookings.RowTemplate.Height = 42

        lblBookingCount = CreateFooterLabel("0 bookings")

        AddHandler dgvBookings.SelectionChanged,
            AddressOf BookingSelection_Changed

        layout.Controls.Add(title, 0, 0)
        layout.Controls.Add(filterPanel, 0, 1)
        layout.Controls.Add(New Label With {
            .Text = "Select a booking to view assignable rooms.",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 8),
            .ForeColor = MutedColor,
            .TextAlign = ContentAlignment.MiddleLeft
        }, 0, 2)

        layout.Controls.Add(dgvBookings, 0, 3)
        layout.Controls.Add(lblBookingCount, 0, 4)

        panel.Controls.Add(layout)

        Return panel

    End Function

    '========================================================
    ' AVAILABLE ROOMS PANEL
    '========================================================

    Private Function BuildRoomsPanel() As Panel

        Dim panel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12),
            .MinimumSize = New Size(230, 300)
        }

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        layout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 34)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 56)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 25)
        )

        layout.Controls.Add(
            CreateSectionTitle("Available Rooms"),
            0,
            0
        )

        Dim typePanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White
        }

        Dim lblType As New Label With {
            .Text = "Room Type",
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .Location = New Point(0, 0)
        }

        cboRoomType = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(0, 24),
            .Size = New Size(220, 28),
            .Anchor = AnchorStyles.Left Or
                      AnchorStyles.Right Or
                      AnchorStyles.Top
        }

        cboRoomType.Items.Add("All Types")
        cboRoomType.SelectedIndex = 0

        AddHandler cboRoomType.SelectedIndexChanged,
            AddressOf RoomTypeFilter_Changed

        AddHandler typePanel.Resize,
            Sub(sender As Object, e As EventArgs)
                cboRoomType.Width =
                    Math.Max(100, typePanel.ClientSize.Width)
            End Sub

        typePanel.Controls.Add(lblType)
        typePanel.Controls.Add(cboRoomType)

        dgvRooms = CreateGrid()

        dgvRooms.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill

        dgvRooms.ColumnHeadersHeight = 42
        dgvRooms.RowTemplate.Height = 42

        AddHandler dgvRooms.SelectionChanged,
            AddressOf RoomSelection_Changed

        lblRoomCount = CreateFooterLabel("0 rooms")

        layout.Controls.Add(typePanel, 0, 1)
        layout.Controls.Add(dgvRooms, 0, 2)
        layout.Controls.Add(lblRoomCount, 0, 3)

        panel.Controls.Add(layout)

        Return panel

    End Function

    '========================================================
    ' ASSIGNMENT DETAILS PANEL
    '========================================================

    Private Function BuildAssignmentPanel() As Panel

        Dim panel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(14),
            .MinimumSize = New Size(245, 300)
        }

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        root.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 38)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 48)
        )

        root.Controls.Add(
            CreateSectionTitle("Assign Room"),
            0,
            0
        )

        ' Scrollable information area.
        Dim details As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .AutoScroll = True,
            .Margin = New Padding(0)
        }

        Dim y As Integer = 0

        AddDetailHeading(details, "Selected Booking", y)
        y += 30

        lblGuest = AddDetailLabel(
            details, "Guest: Select a booking", y
        )
        y += 34

        lblBookingReference = AddDetailLabel(
            details, "Booking No.: —", y
        )
        y += 34

        lblBookingStatus = AddDetailLabel(
            details, "Status: —", y
        )
        y += 34

        lblArrival = AddDetailLabel(
            details, "Arrival: —", y
        )
        y += 34

        lblDeparture = AddDetailLabel(
            details, "Departure: —", y
        )
        y += 34

        lblGuests = AddDetailLabel(
            details, "Guests: —", y
        )
        y += 42

        AddDetailHeading(details, "Selected Room", y)
        y += 32

        lblSelectedRoom = AddDetailLabel(
            details, "Room No.: Select a room", y
        )
        y += 34

        lblSelectedRoomType = AddDetailLabel(
            details, "Room Type: —", y
        )
        y += 34

        lblSelectedRate = AddDetailLabel(
            details, "Rate / Night: —", y
        )
        y += 42

        lblAvailability = New Label With {
            .Text = "Select a booking and an available room.",
            .Location = New Point(0, y),
            .Size = New Size(270, 62),
            .Font = New Font("Segoe UI", 9),
            .ForeColor = Teal,
            .BackColor = Color.FromArgb(240, 253, 250),
            .Padding = New Padding(8),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        details.Controls.Add(lblAvailability)
        y += 75

        Dim lblNotes As New Label With {
            .Text = "Assignment Notes (Optional)",
            .Location = New Point(0, y),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy
        }

        details.Controls.Add(lblNotes)
        y += 24

        txtNotes = New TextBox With {
            .Location = New Point(0, y),
            .Size = New Size(270, 80),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .Font = New Font("Segoe UI", 9),
            .MaxLength = 1000
        }

        details.Controls.Add(txtNotes)

        y += 90

        details.AutoScrollMinSize = New Size(0, y + 10)

        AddHandler details.Resize,
            Sub(sender As Object, e As EventArgs)

                Dim availableWidth As Integer =
                    Math.Max(150, details.ClientSize.Width - 25)

                lblAvailability.Width = availableWidth
                txtNotes.Width = availableWidth

            End Sub

        ' Buttons remain visible outside the scrolling details.
        Dim buttonPanel As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = Color.White,
            .Margin = New Padding(0, 5, 0, 0)
        }

        buttonPanel.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 42.0F)
        )

        buttonPanel.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 58.0F)
        )

        btnCancel = CreateButton(
            "Clear",
            Color.White,
            Navy,
            90,
            36
        )

        btnCancel.Dock = DockStyle.Fill
        btnCancel.FlatAppearance.BorderSize = 1
        btnCancel.FlatAppearance.BorderColor = BorderColor

        btnAssign = CreateButton(
            "Assign Room",
            Teal,
            Color.White,
            120,
            36
        )

        btnAssign.Dock = DockStyle.Fill
        btnAssign.Enabled = False

        AddHandler btnCancel.Click,
            AddressOf ClearSelection_Click

        AddHandler btnAssign.Click,
            AddressOf AssignRoom_Click

        buttonPanel.Controls.Add(btnCancel, 0, 0)
        buttonPanel.Controls.Add(btnAssign, 1, 0)

        root.Controls.Add(details, 0, 1)
        root.Controls.Add(buttonPanel, 0, 2)

        panel.Controls.Add(root)

        Return panel

    End Function

    '========================================================
    ' REUSABLE UI HELPERS
    '========================================================

    Private Function CreateGrid() As DataGridView

        Dim grid As New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .GridColor = BorderColor,
            .RowHeadersVisible = False,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .AllowUserToResizeColumns = True,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .EnableHeadersVisualStyles = False,
            .AutoGenerateColumns = True,
            .ScrollBars = ScrollBars.Both
        }

        grid.ColumnHeadersDefaultCellStyle.BackColor = Navy
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        grid.DefaultCellStyle.Font = New Font("Segoe UI", 9)
        grid.DefaultCellStyle.ForeColor = TextColor
        grid.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)

        grid.DefaultCellStyle.SelectionForeColor = Navy

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        Return grid

    End Function

    Private Function CreateSectionTitle(
        caption As String
    ) As Label

        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }

    End Function

    Private Function CreateFooterLabel(
        caption As String
    ) As Label

        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 8),
            .ForeColor = MutedColor,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }

    End Function

    Private Function CreateButton(
        caption As String,
        backColor As Color,
        foreColor As Color,
        width As Integer,
        height As Integer
    ) As Button

        Dim button As New Button With {
            .Text = caption,
            .Size = New Size(width, height),
            .BackColor = backColor,
            .ForeColor = foreColor,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }

        button.FlatAppearance.BorderSize = 0

        Return button

    End Function

    Private Sub AddDetailHeading(
        parent As Control,
        caption As String,
        y As Integer
    )

        parent.Controls.Add(
            New Label With {
                .Text = caption,
                .Location = New Point(0, y),
                .AutoSize = True,
                .Font = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = Navy
            }
        )

    End Sub

    Private Function AddDetailLabel(
        parent As Control,
        caption As String,
        y As Integer
    ) As Label

        Dim lbl As New Label With {
            .Text = caption,
            .Location = New Point(0, y),
            .Size = New Size(270, 28),
            .Font = New Font("Segoe UI", 9),
            .ForeColor = TextColor,
            .AutoEllipsis = True
        }

        parent.Controls.Add(lbl)

        Return lbl

    End Function

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub RefreshButton_Click(
        sender As Object,
        e As EventArgs
    )

        RefreshAll()

    End Sub


    Private Sub RefreshAll()

        If isLoading Then Return

        isLoading = True

        Try
            selectedBookingId = 0
            selectedRoomId = 0

            ClearBookingDetails()
            ClearRoomDetails()

            LoadBookings()
            LoadRoomTypes()
            LoadAvailableRooms()

            SetMessage(
            "Booking and available-room information refreshed.",
            MutedColor
        )

        Catch ex As Exception

            SetMessage(
            "Unable to refresh room assignment data: " &
            ex.Message,
            DangerColor
        )

            MessageBox.Show(
            "Unable to refresh room assignment data." &
            Environment.NewLine & Environment.NewLine &
            ex.Message,
            "StayPoint HMS",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        )

        Finally
            isLoading = False
        End Try

    End Sub


    '========================================================
    ' LOAD ROOM TYPES
    '========================================================


    Private Sub LoadRoomTypes()

        Dim previousSelection As String =
        If(cboRoomType.SelectedItem Is Nothing,
           "All Types",
           cboRoomType.SelectedItem.ToString())

        RemoveHandler cboRoomType.SelectedIndexChanged,
        AddressOf RoomTypeFilter_Changed

        Try
            cboRoomType.Items.Clear()
            cboRoomType.Items.Add("All Types")

            Using conn As MySqlConnection = DbConnection.GetConnection()
                If conn.State <> ConnectionState.Open Then conn.Open()

                Const sql As String =
                "SELECT DISTINCT type_name " &
                "FROM room_types " &
                "WHERE is_active = 1 " &
                "ORDER BY type_name"

                Using cmd As New MySqlCommand(sql, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            If Not reader.IsDBNull(0) Then
                                cboRoomType.Items.Add(reader.GetString(0))
                            End If
                        End While
                    End Using
                End Using
            End Using

            cboRoomType.SelectedIndex = 0

            For i As Integer = 0 To cboRoomType.Items.Count - 1
                If String.Equals(
                Convert.ToString(cboRoomType.Items(i)),
                previousSelection,
                StringComparison.OrdinalIgnoreCase
            ) Then
                    cboRoomType.SelectedIndex = i
                    Exit For
                End If
            Next

        Finally
            AddHandler cboRoomType.SelectedIndexChanged,
            AddressOf RoomTypeFilter_Changed
        End Try

        ' Reload available rooms after restoring the filter.
        LoadAvailableRooms()

    End Sub


    '========================================================
    ' LOAD ELIGIBLE BOOKINGS
    '========================================================

    Private Sub LoadBookings()

        Dim selectedIdBeforeReload As Integer = selectedBookingId

        bookingTable = New DataTable()

        Const sql As String =
            "SELECT " &
            " b.booking_id, " &
            " b.booking_reference, " &
            " b.guest_id, " &
            " CONCAT(g.first_name, ' ', g.last_name) AS guest_name, " &
            " b.check_in_date, " &
            " b.check_out_date, " &
            " b.adults, " &
            " b.children, " &
            " b.booking_status " &
            "FROM bookings b " &
            "INNER JOIN guests g ON g.guest_id = b.guest_id " &
            "WHERE UPPER(b.booking_status) IN ('PENDING', 'CONFIRMED') " &
            "AND b.check_out_date > NOW() " &
            "AND NOT EXISTS ( " &
            "   SELECT 1 " &
            "   FROM booking_rooms br " &
            "   WHERE br.booking_id = b.booking_id " &
            ") " &
            "ORDER BY b.check_in_date, b.booking_id"

        Using conn As MySqlConnection = DbConnection.GetConnection()

            If conn.State <> ConnectionState.Open Then
                conn.Open()
            End If

            Using adapter As New MySqlDataAdapter(sql, conn)
                adapter.Fill(bookingTable)
            End Using

        End Using

        dgvBookings.DataSource = bookingTable

        ConfigureBookingColumns()
        ApplyBookingFilters()

        If selectedIdBeforeReload > 0 Then

            For Each gridRow As DataGridViewRow In dgvBookings.Rows

                If Convert.ToInt32(
                    gridRow.Cells("booking_id").Value
                ) = selectedIdBeforeReload Then

                    gridRow.Selected = True
                    dgvBookings.CurrentCell =
                        gridRow.Cells("booking_reference")

                    Exit For

                End If

            Next

        End If

        lblBookingCount.Text =
            dgvBookings.Rows.Count.ToString() & " eligible bookings"

    End Sub

    Private Sub ConfigureBookingColumns()

        If dgvBookings.Columns.Count = 0 Then Return

        dgvBookings.Columns("booking_id").Visible = False
        dgvBookings.Columns("guest_id").Visible = False

        dgvBookings.Columns("booking_reference").HeaderText =
            "Booking No."

        dgvBookings.Columns("guest_name").HeaderText =
            "Guest Name"

        dgvBookings.Columns("check_in_date").HeaderText =
            "Arrival"

        dgvBookings.Columns("check_out_date").HeaderText =
            "Departure"

        dgvBookings.Columns("booking_status").HeaderText =
            "Status"

        dgvBookings.Columns("adults").HeaderText = "Adults"
        dgvBookings.Columns("children").HeaderText = "Children"

        dgvBookings.Columns("booking_reference").FillWeight = 115
        dgvBookings.Columns("guest_name").FillWeight = 135
        dgvBookings.Columns("check_in_date").FillWeight = 105
        dgvBookings.Columns("check_out_date").FillWeight = 105
        dgvBookings.Columns("adults").FillWeight = 55
        dgvBookings.Columns("children").FillWeight = 65
        dgvBookings.Columns("booking_status").FillWeight = 90

        dgvBookings.Columns("check_in_date").DefaultCellStyle.Format =
            "MMM dd, yyyy HH:mm"

        dgvBookings.Columns("check_out_date").DefaultCellStyle.Format =
            "MMM dd, yyyy HH:mm"

        For Each col As DataGridViewColumn In dgvBookings.Columns
            col.MinimumWidth = 55
            col.SortMode = DataGridViewColumnSortMode.Automatic
        Next

    End Sub

    '========================================================
    ' FILTER BOOKINGS
    '========================================================

    Private Sub BookingFilters_Changed(
        sender As Object,
        e As EventArgs
    )

        If isLoading Then Return

        ApplyBookingFilters()

    End Sub

    Private Sub ApplyBookingFilters()

        If bookingTable Is Nothing Then Return

        Dim search As String = txtSearch.Text.Trim()

        Dim statusFilter As String =
            If(cboBookingStatus.SelectedItem Is Nothing,
               "All Eligible",
               cboBookingStatus.SelectedItem.ToString())

        Dim view As DataView = bookingTable.DefaultView

        Dim filters As New System.Collections.Generic.List(Of String)

        If statusFilter <> "All Eligible" Then

            filters.Add(
                "booking_status = '" &
                statusFilter.Replace("'", "''") & "'"
            )

        End If

        If Not String.IsNullOrWhiteSpace(search) Then

            Dim safeSearch As String = search.Replace("'", "''")

            filters.Add(
                "(booking_reference LIKE '%" & safeSearch & "%' " &
                "OR guest_name LIKE '%" & safeSearch & "%')"
            )

        End If

        view.RowFilter = String.Join(" AND ", filters)

        Dim previousSelection As Integer = selectedBookingId

        dgvBookings.ClearSelection()

        If previousSelection > 0 Then

            For Each row As DataGridViewRow In dgvBookings.Rows

                If Convert.ToInt32(
                    row.Cells("booking_id").Value
                ) = previousSelection Then

                    row.Selected = True

                    If row.Cells("booking_reference").Visible Then
                        dgvBookings.CurrentCell =
                            row.Cells("booking_reference")
                    End If

                    Exit For

                End If

            Next

        End If

        lblBookingCount.Text =
            dgvBookings.Rows.Count.ToString() & " eligible bookings"

    End Sub

    '========================================================
    ' BOOKING SELECTION
    '========================================================

    Private Sub BookingSelection_Changed(
        sender As Object,
        e As EventArgs
    )

        If isLoading Then Return
        If dgvBookings.CurrentRow Is Nothing Then Return
        If dgvBookings.SelectedRows.Count = 0 Then Return

        Try

            Dim row As DataGridViewRow =
                dgvBookings.SelectedRows(0)

            selectedBookingId =
                Convert.ToInt32(row.Cells("booking_id").Value)

            selectedBookingReference =
                Convert.ToString(
                    row.Cells("booking_reference").Value
                )

            selectedGuestName =
                Convert.ToString(row.Cells("guest_name").Value)

            selectedCheckIn =
                Convert.ToDateTime(
                    row.Cells("check_in_date").Value
                )

            selectedCheckOut =
                Convert.ToDateTime(
                    row.Cells("check_out_date").Value
                )

            lblGuest.Text = "Guest: " & selectedGuestName

            lblBookingReference.Text =
                "Booking No.: " & selectedBookingReference

            lblBookingStatus.Text =
                "Status: " &
                Convert.ToString(row.Cells("booking_status").Value)

            lblArrival.Text =
                "Arrival: " &
                selectedCheckIn.ToString("MMM dd, yyyy hh:mm tt")

            lblDeparture.Text =
                "Departure: " &
                selectedCheckOut.ToString("MMM dd, yyyy hh:mm tt")

            lblGuests.Text =
                "Guests: " &
                Convert.ToString(row.Cells("adults").Value) &
                " Adults, " &
                Convert.ToString(row.Cells("children").Value) &
                " Children"

            selectedRoomId = 0
            ClearRoomDetails()

            LoadAvailableRooms()

            SetMessage(
                "Select a room available for this booking's dates.",
                MutedColor
            )

        Catch ex As Exception

            SetMessage(
                "Unable to display the selected booking: " & ex.Message,
                DangerColor
            )

        End Try

    End Sub

    '========================================================
    ' LOAD AVAILABLE ROOMS
    '========================================================


    Private Sub LoadAvailableRooms()

        If dgvRooms Is Nothing Then Return

        Dim hasBooking As Boolean = selectedBookingId > 0

        If hasBooking AndAlso selectedCheckOut <= selectedCheckIn Then
            ClearRoomGrid()
            SetMessage(
            "The selected booking has an invalid stay period.",
            DangerColor
        )
            Return
        End If

        Dim guestCount As Integer = 1

        If hasBooking AndAlso
       dgvBookings IsNot Nothing AndAlso
       dgvBookings.SelectedRows.Count > 0 Then

            Dim bookingRow As DataGridViewRow =
            dgvBookings.SelectedRows(0)

            guestCount =
            Convert.ToInt32(bookingRow.Cells("adults").Value) +
            Convert.ToInt32(bookingRow.Cells("children").Value)

            If guestCount < 1 Then guestCount = 1
        End If

        roomTable = New DataTable()

        Dim sql As String =
        "SELECT " &
        "r.room_id, " &
        "r.room_number, " &
        "rt.type_name, " &
        "rt.default_rate, " &
        "rt.capacity, " &
        "r.status " &
        "FROM rooms r " &
        "INNER JOIN room_types rt " &
        "ON rt.room_type_id = r.room_type_id " &
        "WHERE UPPER(TRIM(r.status)) = 'AVAILABLE' " &
        "AND rt.is_active = 1 "

        If hasBooking Then
            sql &=
            "AND rt.capacity >= @guestCount " &
            "AND NOT EXISTS ( " &
            "   SELECT 1 " &
            "   FROM booking_rooms br " &
            "   INNER JOIN bookings b " &
            "       ON b.booking_id = br.booking_id " &
            "   WHERE br.room_id = r.room_id " &
            "   AND UPPER(b.booking_status) IN " &
            "       ('PENDING', 'CONFIRMED', 'CHECKED_IN') " &
            "   AND b.check_in_date < @checkOut " &
            "   AND b.check_out_date > @checkIn " &
            ") "
        End If

        sql &= "ORDER BY r.room_number"

        Try

            Using conn As MySqlConnection =
            DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Using cmd As New MySqlCommand(sql, conn)

                    If hasBooking Then

                        cmd.Parameters.Add(
                        "@guestCount",
                        MySqlDbType.Int32
                    ).Value = guestCount

                        cmd.Parameters.Add(
                        "@checkIn",
                        MySqlDbType.DateTime
                    ).Value = selectedCheckIn

                        cmd.Parameters.Add(
                        "@checkOut",
                        MySqlDbType.DateTime
                    ).Value = selectedCheckOut

                    End If

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(roomTable)
                    End Using

                End Using
            End Using

            dgvRooms.DataSource = roomTable

            ConfigureRoomColumns()
            ApplyRoomTypeFilter()

            If lblRoomCount IsNot Nothing Then
                lblRoomCount.Text =
                dgvRooms.Rows.Count.ToString() &
                " available rooms"
            End If

            If roomTable.Rows.Count = 0 Then

                If hasBooking Then
                    lblAvailability.Text =
                    "No available rooms match this booking's dates and guest capacity."
                Else
                    lblAvailability.Text =
                    "No rooms are currently marked AVAILABLE in the active room types."
                End If

                lblAvailability.ForeColor = DangerColor

            ElseIf hasBooking Then

                lblAvailability.Text =
                "Rooms match the selected booking. Availability will be validated again before saving."

                lblAvailability.ForeColor = Teal

            Else

                lblAvailability.Text =
                "Available rooms are displayed. Select a booking to check stay-date conflicts."

                lblAvailability.ForeColor = Teal

            End If

        Catch ex As Exception

            ClearRoomGrid()

            SetMessage(
            "Unable to load available rooms: " & ex.Message,
            DangerColor
        )

        End Try

    End Sub


    Private Sub ConfigureRoomColumns()

        If dgvRooms.Columns.Count = 0 Then Return

        dgvRooms.Columns("room_id").Visible = False
        dgvRooms.Columns("capacity").Visible = False
        dgvRooms.Columns("status").Visible = False

        dgvRooms.Columns("room_number").HeaderText = "Room No."
        dgvRooms.Columns("type_name").HeaderText = "Type"
        dgvRooms.Columns("default_rate").HeaderText = "Rate / Night"

        dgvRooms.Columns("room_number").FillWeight = 75
        dgvRooms.Columns("type_name").FillWeight = 95
        dgvRooms.Columns("default_rate").FillWeight = 110

        dgvRooms.Columns("default_rate").DefaultCellStyle.Format =
            "₱ #,##0.00"

        dgvRooms.Columns("default_rate").DefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleRight

        For Each col As DataGridViewColumn In dgvRooms.Columns
            col.MinimumWidth = 65
        Next

    End Sub

    '========================================================
    ' FILTER AVAILABLE ROOMS
    '========================================================


    Private Sub RoomTypeFilter_Changed(
    sender As Object,
    e As EventArgs
)

        If isLoading Then Return

        ApplyRoomTypeFilter()

    End Sub


    Private Sub ApplyRoomTypeFilter()

        If roomTable Is Nothing Then Return

        Dim selectedType As String =
            If(cboRoomType.SelectedItem Is Nothing,
               "All Types",
               cboRoomType.SelectedItem.ToString())

        Dim view As DataView = roomTable.DefaultView

        If selectedType = "All Types" Then

            view.RowFilter = ""

        Else

            view.RowFilter =
                "type_name = '" &
                selectedType.Replace("'", "''") & "'"

        End If

        dgvRooms.ClearSelection()
        selectedRoomId = 0

        ClearRoomDetails()

        lblRoomCount.Text =
            dgvRooms.Rows.Count.ToString() & " available rooms"

    End Sub

    '========================================================
    ' ROOM SELECTION
    '========================================================

    Private Sub RoomSelection_Changed(
        sender As Object,
        e As EventArgs
    )

        If isLoading Then Return
        If dgvRooms.SelectedRows.Count = 0 Then Return

        Try

            Dim row As DataGridViewRow =
                dgvRooms.SelectedRows(0)

            selectedRoomId =
                Convert.ToInt32(row.Cells("room_id").Value)

            selectedRoomNumber =
                Convert.ToString(row.Cells("room_number").Value)

            selectedRoomTypeName =
                Convert.ToString(row.Cells("type_name").Value)

            selectedRoomRate =
                Convert.ToDecimal(row.Cells("default_rate").Value)

            lblSelectedRoom.Text =
                "Room No.: " & selectedRoomNumber

            lblSelectedRoomType.Text =
                "Room Type: " & selectedRoomTypeName

            lblSelectedRate.Text =
                "Rate / Night: " &
                selectedRoomRate.ToString(
                    "₱ #,##0.00",
                    CultureInfo.GetCultureInfo("en-PH")
                )

            If selectedBookingId > 0 Then

                lblAvailability.Text =
                    "Selected room. Availability will be validated again before saving."

                lblAvailability.ForeColor = Teal

            End If

            UpdateAssignButton()

        Catch ex As Exception

            SetMessage(
                "Unable to display the selected room: " & ex.Message,
                DangerColor
            )

        End Try

    End Sub

    '========================================================
    ' ASSIGN BUTTON STATE
    '========================================================

    Private Sub UpdateAssignButton()

        btnAssign.Enabled =
            selectedBookingId > 0 AndAlso selectedRoomId > 0

    End Sub

    '========================================================
    ' SAVE ROOM ASSIGNMENT
    '========================================================

    Private Sub AssignRoom_Click(
        sender As Object,
        e As EventArgs
    )

        If selectedBookingId <= 0 Then

            MessageBox.Show(
                "Select an eligible booking first.",
                "Booking Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Return

        End If

        If selectedRoomId <= 0 Then

            MessageBox.Show(
                "Select an available room first.",
                "Room Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Return

        End If

        If selectedCheckOut <= selectedCheckIn Then

            MessageBox.Show(
                "The booking's check-out must be later than check-in.",
                "Invalid Stay Period",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Return

        End If

        Dim confirmation As DialogResult =
            MessageBox.Show(
                "Assign room " & selectedRoomNumber &
                " to " & selectedGuestName & "?" &
                Environment.NewLine & Environment.NewLine &
                "Booking: " & selectedBookingReference &
                Environment.NewLine &
                "Rate per night: " &
                selectedRoomRate.ToString(
                    "₱ #,##0.00",
                    CultureInfo.GetCultureInfo("en-PH")
                ) &
                Environment.NewLine & Environment.NewLine &
                "This action does not check the guest in or record payment.",
                "Confirm Room Assignment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

        If confirmation <> DialogResult.Yes Then Return

        btnAssign.Enabled = False

        Try

            Using conn As MySqlConnection = DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Using tx As MySqlTransaction = conn.BeginTransaction()

                    Try

                        '----------------------------------------
                        ' 1. Lock and validate booking.
                        '----------------------------------------

                        Dim bookingStatus As String = ""
                        Dim checkIn As DateTime
                        Dim checkOut As DateTime
                        Dim adults As Integer
                        Dim children As Integer

                        Const bookingSql As String =
                            "SELECT booking_status, check_in_date, " &
                            "check_out_date, adults, children " &
                            "FROM bookings " &
                            "WHERE booking_id = @bookingId " &
                            "FOR UPDATE"

                        Using cmd As New MySqlCommand(
                            bookingSql, conn, tx
                        )

                            cmd.Parameters.Add(
                                "@bookingId",
                                MySqlDbType.Int32
                            ).Value = selectedBookingId

                            Using reader As MySqlDataReader =
                                cmd.ExecuteReader()

                                If Not reader.Read() Then
                                    Throw New Exception(
                                        "The booking no longer exists. Refresh the list."
                                    )
                                End If

                                bookingStatus =
                                    Convert.ToString(
                                        reader("booking_status")
                                    )

                                checkIn =
                                    Convert.ToDateTime(
                                        reader("check_in_date")
                                    )

                                checkOut =
                                    Convert.ToDateTime(
                                        reader("check_out_date")
                                    )

                                adults =
                                    Convert.ToInt32(reader("adults"))

                                children =
                                    Convert.ToInt32(reader("children"))

                            End Using

                        End Using

                        If Not String.Equals(
                            bookingStatus,
                            "PENDING",
                            StringComparison.OrdinalIgnoreCase
                        ) AndAlso Not String.Equals(
                            bookingStatus,
                            "CONFIRMED",
                            StringComparison.OrdinalIgnoreCase
                        ) Then

                            Throw New Exception(
                                "This booking is no longer eligible for room assignment."
                            )

                        End If

                        If checkOut <= checkIn Then

                            Throw New Exception(
                                "The booking has an invalid check-in/check-out period."
                            )

                        End If

                        If checkOut <= DateTime.Now Then

                            Throw New Exception(
                                "This booking's stay has already ended."
                            )

                        End If

                        '----------------------------------------
                        ' 2. Lock and validate room and rate.
                        '----------------------------------------

                        Dim roomNumber As String = ""
                        Dim roomStatus As String = ""
                        Dim roomTypeName As String = ""
                        Dim currentRate As Decimal = 0D
                        Dim capacity As Integer = 0
                        Dim roomIsActive As Boolean = False

                        Const roomSql As String =
                            "SELECT r.room_number, r.status, " &
                            "rt.type_name, rt.default_rate, " &
                            "rt.capacity, rt.is_active " &
                            "FROM rooms r " &
                            "INNER JOIN room_types rt " &
                            "ON rt.room_type_id = r.room_type_id " &
                            "WHERE r.room_id = @roomId " &
                            "FOR UPDATE"

                        Using cmd As New MySqlCommand(
                            roomSql, conn, tx
                        )

                            cmd.Parameters.Add(
                                "@roomId",
                                MySqlDbType.Int32
                            ).Value = selectedRoomId

                            Using reader As MySqlDataReader =
                                cmd.ExecuteReader()

                                If Not reader.Read() Then

                                    Throw New Exception(
                                        "The selected room no longer exists."
                                    )

                                End If

                                roomNumber =
                                    Convert.ToString(
                                        reader("room_number")
                                    )

                                roomStatus =
                                    Convert.ToString(
                                        reader("status")
                                    )

                                roomTypeName =
                                    Convert.ToString(
                                        reader("type_name")
                                    )

                                currentRate =
                                    Convert.ToDecimal(
                                        reader("default_rate")
                                    )

                                capacity =
                                    Convert.ToInt32(
                                        reader("capacity")
                                    )

                                roomIsActive =
                                    Convert.ToBoolean(
                                        reader("is_active")
                                    )

                            End Using

                        End Using

                        If Not String.Equals(
                            roomStatus,
                            "AVAILABLE",
                            StringComparison.OrdinalIgnoreCase
                        ) Then

                            Throw New Exception(
                                "Room " & roomNumber &
                                " is no longer marked AVAILABLE."
                            )

                        End If

                        If Not roomIsActive Then

                            Throw New Exception(
                                "The selected room type is inactive in Admin settings."
                            )

                        End If

                        If capacity < Math.Max(1, adults + children) Then

                            Throw New Exception(
                                "The selected room does not have enough guest capacity."
                            )

                        End If

                        If currentRate < 0D Then

                            Throw New Exception(
                                "The room's configured rate is invalid."
                            )

                        End If

                        '----------------------------------------
                        ' 3. Check existing overlapping assignments.
                        '----------------------------------------

                        Const overlapSql As String =
                            "SELECT COUNT(*) " &
                            "FROM booking_rooms br " &
                            "INNER JOIN bookings b " &
                            "ON b.booking_id = br.booking_id " &
                            "WHERE br.room_id = @roomId " &
                            "AND UPPER(b.booking_status) IN " &
                            "('PENDING', 'CONFIRMED', 'CHECKED_IN') " &
                            "AND b.check_in_date < @checkOut " &
                            "AND b.check_out_date > @checkIn"

                        Using cmd As New MySqlCommand(
                            overlapSql, conn, tx
                        )

                            cmd.Parameters.Add(
                                "@roomId",
                                MySqlDbType.Int32
                            ).Value = selectedRoomId

                            cmd.Parameters.Add(
                                "@checkIn",
                                MySqlDbType.DateTime
                            ).Value = checkIn

                            cmd.Parameters.Add(
                                "@checkOut",
                                MySqlDbType.DateTime
                            ).Value = checkOut

                            Dim overlapCount As Integer =
                                Convert.ToInt32(cmd.ExecuteScalar())

                            If overlapCount > 0 Then

                                Throw New Exception(
                                    "This room already has an overlapping booking. " &
                                    "Choose another available room."
                                )

                            End If

                        End Using

                        '----------------------------------------
                        ' 4. Confirm booking has no existing room.
                        '----------------------------------------

                        Const existingAssignmentSql As String =
                            "SELECT COUNT(*) " &
                            "FROM booking_rooms " &
                            "WHERE booking_id = @bookingId"

                        Using cmd As New MySqlCommand(
                            existingAssignmentSql, conn, tx
                        )

                            cmd.Parameters.Add(
                                "@bookingId",
                                MySqlDbType.Int32
                            ).Value = selectedBookingId

                            Dim existingCount As Integer =
                                Convert.ToInt32(cmd.ExecuteScalar())

                            If existingCount > 0 Then

                                Throw New Exception(
                                    "This booking already has a room assignment. " &
                                    "Refresh the list before continuing."
                                )

                            End If

                        End Using

                        '----------------------------------------
                        ' 5. Calculate stay duration.
                        '----------------------------------------

                        Dim totalHours As Double =
                            (checkOut - checkIn).TotalHours

                        Dim numberOfNights As Integer =
                            Math.Max(
                                1,
                                CInt(Math.Ceiling(totalHours / 24.0R))
                            )

                        '----------------------------------------
                        ' 6. Insert booking_rooms record.
                        '----------------------------------------

                        Const insertSql As String =
                            "INSERT INTO booking_rooms " &
                            "(booking_id, room_id, nightly_rate, number_of_nights) " &
                            "VALUES " &
                            "(@bookingId, @roomId, @rate, @nights)"

                        Using cmd As New MySqlCommand(
                            insertSql, conn, tx
                        )

                            cmd.Parameters.Add(
                                "@bookingId",
                                MySqlDbType.Int32
                            ).Value = selectedBookingId

                            cmd.Parameters.Add(
                                "@roomId",
                                MySqlDbType.Int32
                            ).Value = selectedRoomId

                            cmd.Parameters.Add(
                                "@rate",
                                MySqlDbType.Decimal
                            ).Value = currentRate

                            cmd.Parameters.Add(
                                "@nights",
                                MySqlDbType.Int32
                            ).Value = numberOfNights

                            cmd.ExecuteNonQuery()

                        End Using

                        ' Room status is not changed to OCCUPIED here.
                        ' The separate check-in module handles occupancy.

                        tx.Commit()

                    Catch

                        Try
                            tx.Rollback()
                        Catch
                            ' Preserve the original exception.
                        End Try

                        Throw

                    End Try

                End Using

            End Using

            MessageBox.Show(
                "Room assignment saved successfully." &
                Environment.NewLine & Environment.NewLine &
                "Guest: " & selectedGuestName &
                Environment.NewLine &
                "Booking: " & selectedBookingReference &
                Environment.NewLine &
                "Room: " & selectedRoomNumber &
                Environment.NewLine & Environment.NewLine &
                "Proceed to Check-In/Check-Out when the guest is ready to check in.",
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            txtNotes.Clear()

            RefreshAll()

        Catch ex As Exception

            MessageBox.Show(
                "Room assignment was not saved." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "Assignment Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            SetMessage(
                "Assignment failed. Refresh and verify the booking and room.",
                DangerColor
            )

        Finally

            UpdateAssignButton()

        End Try

    End Sub

    '========================================================
    ' CLEAR SELECTION
    '========================================================

    Private Sub ClearSelection_Click(
        sender As Object,
        e As EventArgs
    )

        selectedBookingId = 0
        selectedRoomId = 0

        dgvBookings.ClearSelection()
        dgvRooms.ClearSelection()

        ClearBookingDetails()
        ClearRoomDetails()
        ClearRoomGrid()

        txtNotes.Clear()

        UpdateAssignButton()

        SetMessage(
            "Select a booking to begin room assignment.",
            MutedColor
        )

    End Sub

    Private Sub ClearBookingDetails()

        If lblGuest Is Nothing Then Return

        lblGuest.Text = "Guest: Select a booking"
        lblBookingReference.Text = "Booking No.: —"
        lblBookingStatus.Text = "Status: —"
        lblArrival.Text = "Arrival: —"
        lblDeparture.Text = "Departure: —"
        lblGuests.Text = "Guests: —"

    End Sub

    Private Sub ClearRoomDetails()

        If lblSelectedRoom Is Nothing Then Return

        lblSelectedRoom.Text = "Room No.: Select a room"
        lblSelectedRoomType.Text = "Room Type: —"
        lblSelectedRate.Text = "Rate / Night: —"

        If selectedBookingId > 0 Then

            lblAvailability.Text =
                "Select an available room for the selected booking."

        Else

            lblAvailability.Text =
                "Select a booking and an available room."

        End If

        lblAvailability.ForeColor = Teal

        UpdateAssignButton()

    End Sub

    Private Sub ClearRoomGrid()

        If dgvRooms IsNot Nothing Then
            dgvRooms.DataSource = Nothing
        End If

        roomTable = Nothing

        If lblRoomCount IsNot Nothing Then
            lblRoomCount.Text = "0 available rooms"
        End If

    End Sub

    '========================================================
    ' STATUS MESSAGE
    '========================================================

    Private Sub SetMessage(
        message As String,
        messageColor As Color
    )

        If lblMessage Is Nothing Then Return

        lblMessage.Text = message
        lblMessage.ForeColor = messageColor

    End Sub

End Class
