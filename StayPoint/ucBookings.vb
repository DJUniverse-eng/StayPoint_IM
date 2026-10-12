Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucBookings

    '========================================================
    ' COLORS - MATCH STAYPOINT DASHBOARD
    '========================================================

    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly BgColor As Color =
        Color.FromArgb(240, 244, 248)

    Private ReadOnly MutedColor As Color =
        Color.FromArgb(107, 114, 128)

    Private ReadOnly BorderColor As Color =
        Color.FromArgb(226, 232, 240)

    Private ReadOnly CardColor As Color =
        Color.White

    '========================================================
    ' CONTROLS
    '========================================================

    Private pnlHeader As Panel
    Private pnlSummary As FlowLayoutPanel
    Private pnlFilters As Panel
    Private pnlTable As Panel

    Private lblPageTitle As Label
    Private lblSubtitle As Label
    Private lblTableTitle As Label
    Private lblRecordCount As Label
    Private lblMessage As Label

    Private txtSearch As TextBox
    Private cboStatus As ComboBox
    Private btnRefresh As Button

    Private dgvBookings As DataGridView

    Private lblTotal As Label
    Private lblPending As Label
    Private lblConfirmed As Label
    Private lblCancelled As Label

    Private sourceTable As DataTable
    Private displayTable As DataTable

    Private isLoading As Boolean = False

    Private pnlTableHeader As Panel

    '========================================================
    ' INITIALIZE
    '========================================================

    Public Sub New()

        InitializeComponent()

        BuildInterface()

    End Sub

    Private Sub ucBookings_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        LoadBookings()

    End Sub

    '========================================================
    ' BUILD INTERFACE
    '========================================================

    Private Sub BuildInterface()

        Me.SuspendLayout()

        Me.Controls.Clear()

        Me.Dock = DockStyle.Fill
        Me.BackColor = BgColor
        Me.AutoScroll = True
        Me.MinimumSize = New Size(700, 500)

        '------------------------------
        ' PAGE HEADER
        '------------------------------

        pnlHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 90,
            .BackColor = BgColor,
            .Padding = New Padding(24, 14, 24, 8)
        }

        lblPageTitle = New Label With {
            .Text = "Bookings",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(24, 12)
        }

        lblSubtitle = New Label With {
            .Text = "Manage reservations and booking statuses.",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(27, 53)
        }

        pnlHeader.Controls.Add(lblPageTitle)
        pnlHeader.Controls.Add(lblSubtitle)

        '------------------------------
        ' SUMMARY CARDS
        '------------------------------

        pnlSummary = New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 112,
            .Padding = New Padding(20, 4, 12, 8),
            .BackColor = BgColor,
            .WrapContents = False,
            .AutoScroll = True
        }

        lblTotal = New Label()
        lblPending = New Label()
        lblConfirmed = New Label()
        lblCancelled = New Label()

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Total Bookings",
                lblTotal,
                Navy
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Pending",
                lblPending,
                Color.FromArgb(217, 119, 6)
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Confirmed",
                lblConfirmed,
                Teal
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Cancelled",
                lblCancelled,
                Color.FromArgb(220, 38, 38)
            )
        )

        '------------------------------
        ' BOOKINGS TABLE CONTAINER
        '------------------------------

        pnlTable = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = CardColor,
            .Padding = New Padding(16),
            .Margin = New Padding(20)
        }
        '------------------------------
        ' TABLE TITLE
        '------------------------------

        Dim pnlTableHeader As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 48,
            .BackColor = CardColor
        }

        lblTableTitle = New Label With {
            .Text = "Reservation Records",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(2, 10)
        }

        lblRecordCount = New Label With {
            .Text = "0 records",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        AddHandler pnlTableHeader.Resize,
            Sub(sender, e)
                lblRecordCount.Location =
                    New Point(
                        pnlTableHeader.ClientSize.Width -
                        lblRecordCount.Width - 4,
                        15
                    )
            End Sub

        pnlTableHeader.Controls.Add(lblTableTitle)
        pnlTableHeader.Controls.Add(lblRecordCount)

        '------------------------------
        ' SEARCH AND FILTERS
        '------------------------------

        pnlFilters = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .BackColor = CardColor
        }

        Dim lblSearch As New Label With {
            .Text = "Search by Booking ID, Guest, or Room",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(2, 2)
        }

        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(2, 27),
            .Size = New Size(270, 32),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }

        Dim lblStatus As New Label With {
            .Text = "Booking Status",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(288, 2)
        }

        cboStatus = New ComboBox With {
            .Font = New Font("Segoe UI", 10),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(288, 27),
            .Size = New Size(155, 32),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }


        cboStatus.Items.AddRange(
    New Object() {
        "All Statuses",
        "PENDING",
        "CONFIRMED",
        "CHECKED_IN",
        "CHECKED_OUT",
        "CANCELLED",
        "NO_SHOW"
    }
)

        cboStatus.SelectedIndex = 0


        btnRefresh = New Button With {
            .Text = "Refresh",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .BackColor = Navy,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Size = New Size(95, 32),
            .Location = New Point(455, 27),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left
        }

        btnRefresh.FlatAppearance.BorderSize = 0

        AddHandler txtSearch.TextChanged,
            AddressOf ApplyFilters

        AddHandler cboStatus.SelectedIndexChanged,
            AddressOf ApplyFilters

        AddHandler btnRefresh.Click,
            AddressOf RefreshBookings

        pnlFilters.Controls.Add(lblSearch)
        pnlFilters.Controls.Add(txtSearch)
        pnlFilters.Controls.Add(lblStatus)
        pnlFilters.Controls.Add(cboStatus)
        pnlFilters.Controls.Add(btnRefresh)

        '------------------------------
        ' DATABASE MESSAGE
        '------------------------------

        lblMessage = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 28,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Visible = False
        }

        '------------------------------
        ' DATA GRID
        '------------------------------

        dgvBookings = New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .GridColor = BorderColor,
            .AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,
            .AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None,
            .RowHeadersVisible = False,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            .ScrollBars = ScrollBars.Both,
            .EnableHeadersVisualStyles = False,
            .RowTemplate = New DataGridViewRow With {
                .Height = 38
            }
        }

        dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvBookings.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvBookings.ColumnHeadersHeight = 42
        dgvBookings.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        dgvBookings.DefaultCellStyle.Font =
            New Font("Segoe UI", 9)

        dgvBookings.DefaultCellStyle.ForeColor =
            Color.FromArgb(51, 65, 85)

        dgvBookings.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)

        dgvBookings.DefaultCellStyle.SelectionForeColor = Navy

        dgvBookings.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        dgvBookings.RowTemplate.Height = 38

        '------------------------------
        ' ADD CONTROLS
        '------------------------------

        pnlTable.Controls.Add(dgvBookings)
        pnlTable.Controls.Add(lblMessage)
        pnlTable.Controls.Add(pnlFilters)
        pnlTable.Controls.Add(pnlTableHeader)

        ' Docked controls are brought to the front in reverse
        ' docking order. Add the main containers in this order.

        Me.Controls.Add(pnlTable)
        Me.Controls.Add(pnlSummary)
        Me.Controls.Add(pnlHeader)

        Me.ResumeLayout(True)

    End Sub

    '========================================================
    ' SUMMARY CARD
    '========================================================

    Private Function CreateSummaryCard(
        title As String,
        valueLabel As Label,
        accentColor As Color
    ) As Panel

        Dim card As New Panel With {
            .Size = New Size(220, 84),
            .BackColor = CardColor,
            .Margin = New Padding(4, 4, 10, 4),
            .Padding = New Padding(12)
        }

        AddHandler card.Paint,
            Sub(sender, e)
                Using pen As New Pen(BorderColor, 1)
                    e.Graphics.DrawRectangle(
                        pen,
                        0,
                        0,
                        card.Width - 1,
                        card.Height - 1
                    )
                End Using
            End Sub

        Dim accent As New Panel With {
            .BackColor = accentColor,
            .Location = New Point(0, 0),
            .Size = New Size(4, card.Height)
        }

        Dim lblTitle As New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .Location = New Point(15, 12),
            .AutoSize = True
        }

        valueLabel.Text = "0"
        valueLabel.Font = New Font(
            "Segoe UI",
            20,
            FontStyle.Bold
        )
        valueLabel.ForeColor = accentColor
        valueLabel.Location = New Point(14, 32)
        valueLabel.AutoSize = True

        card.Controls.Add(accent)
        card.Controls.Add(lblTitle)
        card.Controls.Add(valueLabel)

        Return card

    End Function
    '========================================================
    ' LOAD BOOKINGS FROM MYSQL
    '========================================================

    Private Sub LoadBookings()

        If isLoading Then Return

        isLoading = True

        Try

            If lblMessage IsNot Nothing Then
                lblMessage.Visible = False
                lblMessage.Text = ""
            End If

            Using conn As MySqlConnection =
            DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Dim sql As String =
                "SELECT " &
                "b.booking_id, " &
                "b.booking_reference, " &
                "b.guest_id, " &
                "CONCAT(g.first_name, ' ', g.last_name) AS guest_name, " &
                "COALESCE(" &
                "GROUP_CONCAT(DISTINCT r.room_number " &
                "ORDER BY r.room_number SEPARATOR ', '), " &
                "'Unassigned') AS room_number, " &
                "b.booking_date, " &
                "b.check_in_date, " &
                "b.check_out_date, " &
                "b.booking_status, " &
                "b.created_at " &
                "FROM bookings b " &
                "INNER JOIN guests g " &
                "ON g.guest_id = b.guest_id " &
                "LEFT JOIN booking_rooms br " &
                "ON br.booking_id = b.booking_id " &
                "LEFT JOIN rooms r " &
                "ON r.room_id = br.room_id " &
                "GROUP BY " &
                "b.booking_id, " &
                "b.booking_reference, " &
                "b.guest_id, " &
                "g.first_name, " &
                "g.last_name, " &
                "b.booking_date, " &
                "b.check_in_date, " &
                "b.check_out_date, " &
                "b.booking_status, " &
                "b.created_at " &
                "ORDER BY b.check_in_date DESC"

                Using adapter As New MySqlDataAdapter(sql, conn)

                    sourceTable = New DataTable()
                    adapter.Fill(sourceTable)

                End Using

            End Using

            BuildDisplayTable()
            UpdateSummary()
            ApplyFilters(Nothing, EventArgs.Empty)

        Catch ex As Exception

            If sourceTable Is Nothing Then
                sourceTable = New DataTable()
            End If

            BuildDisplayTable()
            UpdateSummary()

            If dgvBookings IsNot Nothing Then
                dgvBookings.DataSource = displayTable
            End If

            If lblMessage IsNot Nothing Then

                lblMessage.Text =
                "Unable to load bookings: " & ex.Message

                lblMessage.ForeColor =
                Color.FromArgb(185, 28, 28)

                lblMessage.Visible = True

            End If

        Finally

            isLoading = False

        End Try

    End Sub
    '========================================================
    ' NORMALIZE DATABASE COLUMNS
    '========================================================

    Private Sub BuildDisplayTable()

        displayTable = New DataTable()

        displayTable.Columns.Add("Booking ID")
        displayTable.Columns.Add("Guest")
        displayTable.Columns.Add("Room")
        displayTable.Columns.Add("Check-In")
        displayTable.Columns.Add("Check-Out")
        displayTable.Columns.Add("Booking Date")
        displayTable.Columns.Add("Status")

        If sourceTable Is Nothing Then Return

        For Each sourceRow As DataRow In sourceTable.Rows

            Dim newRow As DataRow =
                displayTable.NewRow()

            newRow("Booking ID") =
                GetSourceValue(
                    sourceRow,
                    "booking_id",
                    "bookingid",
                    "reservation_id",
                    "reservationid",
                    "id"
                )

            newRow("Guest") =
                GetSourceValue(
                    sourceRow,
                    "guest_name",
                    "guestname",
                    "customer_name",
                    "customername",
                    "full_name",
                    "fullname",
                    "guest",
                    "customer"
                )

            newRow("Room") =
                GetSourceValue(
                    sourceRow,
                    "room_number",
                    "roomnumber",
                    "room_no",
                    "roomno",
                    "room_name",
                    "roomname",
                    "room_id",
                    "roomid",
                    "room"
                )

            newRow("Check-In") =
                FormatDateValue(
                    GetSourceValue(
                        sourceRow,
                        "check_in",
                        "checkin",
                        "check_in_date",
                        "checkindate",
                        "arrival_date",
                        "arrivaldate"
                    )
                )

            newRow("Check-Out") =
                FormatDateValue(
                    GetSourceValue(
                        sourceRow,
                        "check_out",
                        "checkout",
                        "check_out_date",
                        "checkoutdate",
                        "departure_date",
                        "departuredate"
                    )
                )

            newRow("Booking Date") =
                FormatDateValue(
                    GetSourceValue(
                        sourceRow,
                        "booking_date",
                        "bookingdate",
                        "reservation_date",
                        "reservationdate",
                        "created_at",
                        "createdat"
                    )
                )

            newRow("Status") =
                GetSourceValue(
                    sourceRow,
                    "booking_status",
                    "bookingstatus",
                    "reservation_status",
                    "reservationstatus",
                    "status"
                )

            displayTable.Rows.Add(newRow)

        Next

    End Sub

    Private Function GetSourceValue(
        sourceRow As DataRow,
        ParamArray possibleNames() As String
    ) As String

        For Each column As DataColumn In sourceRow.Table.Columns

            Dim normalizedName As String =
                column.ColumnName.
                Replace("_", "").
                Replace(" ", "").
                Replace("-", "").
                ToLowerInvariant()

            For Each possibleName As String In possibleNames

                Dim normalizedPossibleName As String =
                    possibleName.
                    Replace("_", "").
                    Replace(" ", "").
                    Replace("-", "").
                    ToLowerInvariant()

                If normalizedName = normalizedPossibleName Then

                    If sourceRow.IsNull(column) Then
                        Return ""
                    End If

                    Return Convert.ToString(
                        sourceRow(column)
                    )

                End If

            Next

        Next

        Return ""

    End Function


    Private Function FormatDateValue(
    value As String
) As String

        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim parsedDate As DateTime

        If DateTime.TryParse(value, parsedDate) Then

            Return parsedDate.ToString(
            "MMM dd, yyyy h:mm tt",
            Globalization.CultureInfo.InvariantCulture
        )

        End If

        Return value

    End Function


    '========================================================
    ' SUMMARY COUNTS
    '========================================================

    Private Sub UpdateSummary()

        Dim total As Integer = 0
        Dim pending As Integer = 0
        Dim confirmed As Integer = 0
        Dim cancelled As Integer = 0

        If sourceTable IsNot Nothing Then

            total = sourceTable.Rows.Count

            For Each row As DataRow In sourceTable.Rows

                Dim status As String =
                    GetSourceValue(
                        row,
                        "booking_status",
                        "bookingstatus",
                        "reservation_status",
                        "reservationstatus",
                        "status"
                    ).Trim().ToLowerInvariant()

                Select Case status

                    Case "pending"
                        pending += 1

                    Case "confirmed", "approved"
                        confirmed += 1

                    Case "cancelled", "canceled"
                        cancelled += 1

                End Select

            Next

        End If

        lblTotal.Text = total.ToString()
        lblPending.Text = pending.ToString()
        lblConfirmed.Text = confirmed.ToString()
        lblCancelled.Text = cancelled.ToString()

    End Sub

    '========================================================
    ' SEARCH AND STATUS FILTER
    '========================================================

    Private Sub ApplyFilters(
        sender As Object,
        e As EventArgs
    )

        If displayTable Is Nothing Then Return

        Dim view As New DataView(displayTable)

        Dim searchText As String =
            If(txtSearch Is Nothing,
               "",
               txtSearch.Text.Trim())

        Dim selectedStatus As String =
            If(cboStatus Is Nothing OrElse
               cboStatus.SelectedItem Is Nothing,
               "All Statuses",
               cboStatus.SelectedItem.ToString())

        Dim filters As New List(Of String)

        If searchText <> "" Then

            Dim escapedSearch As String =
                searchText.Replace("'", "''").
                Replace("[", "[[]").
                Replace("%", "[%]").
                Replace("*", "[*]")

            Dim searchFilter As String =
                "Convert([Booking ID], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Guest], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Room], 'System.String') LIKE '%" &
                escapedSearch & "%'"

            filters.Add("(" & searchFilter & ")")

        End If

        If selectedStatus <> "All Statuses" Then

            filters.Add(
                "[Status] = '" &
                selectedStatus.Replace("'", "''") &
                "'"
            )

        End If

        Try

            view.RowFilter = String.Join(" AND ", filters)

            dgvBookings.DataSource = view

            lblRecordCount.Text =
                view.Count.ToString() &
                If(view.Count = 1,
                   " record",
                   " records")


            If pnlTableHeader IsNot Nothing AndAlso
               lblRecordCount IsNot Nothing Then

                lblRecordCount.Location =
                    New Point(
                        Math.Max(
                            4,
                            pnlTableHeader.ClientSize.Width -
                            lblRecordCount.Width - 4
                        ),
                        15
                    )

            End If


        Catch

            dgvBookings.DataSource = displayTable

        End Try

    End Sub

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub RefreshBookings(
        sender As Object,
        e As EventArgs
    )

        LoadBookings()

    End Sub

End Class