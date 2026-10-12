
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffHome
    Inherits UserControl

    '========================================================
    ' COLORS
    '========================================================

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly TextColor As Color = Color.FromArgb(51, 65, 85)
    Private ReadOnly MutedColor As Color = Color.FromArgb(100, 116, 139)
    Private ReadOnly BorderColor As Color = Color.FromArgb(226, 232, 240)

    '========================================================
    ' CONTROLS
    '========================================================

    Private pnlHeader As Panel
    Private tblSummary As TableLayoutPanel
    Private pnlRecent As Panel
    Private dgvRecent As DataGridView

    Private lblLastUpdated As Label
    Private lblRecentTitle As Label
    Private btnRefresh As Button

    Private lblArrivalsValue As Label
    Private lblDeparturesValue As Label
    Private lblCheckedInValue As Label
    Private lblPendingValue As Label

    Private isLoading As Boolean = False

    '========================================================
    ' CONSTRUCTOR
    '========================================================

    Public Sub New()
        InitializeComponent()
        BuildInterface()
    End Sub

    '========================================================
    ' BUILD INTERFACE
    '========================================================

    Private Sub BuildInterface()

        Me.SuspendLayout()

        Me.Dock = DockStyle.Fill
        Me.BackColor = BgColor
        Me.Padding = New Padding(18)
        Me.AutoScroll = False

        '----------------------------------------------------
        ' MAIN LAYOUT
        '----------------------------------------------------

        Dim mainLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = BgColor,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        }

        mainLayout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F)
        )

        ' Reduced header height.
        mainLayout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 100.0F)
        )

        ' Reduced summary height to move Recent Bookings upward.
        mainLayout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 145.0F)
        )

        ' Recent Bookings occupies all remaining space.
        mainLayout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        '----------------------------------------------------
        ' HEADER
        '----------------------------------------------------

        pnlHeader = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = BgColor,
            .Margin = New Padding(0)
        }

        Dim lblTitle As New Label With {
            .Text = "Welcome to StayPoint",
            .Font = New Font("Segoe UI", 23, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 2)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Staff Operations Portal  |  Front Desk Overview",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(3, 47)
        }

        btnRefresh = New Button With {
            .Text = "Refresh Data",
            .Size = New Size(145, 42),
            .BackColor = Teal,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        btnRefresh.FlatAppearance.BorderSize = 0

        AddHandler btnRefresh.Click, AddressOf RefreshDashboard

        pnlHeader.Controls.Add(lblTitle)
        pnlHeader.Controls.Add(lblSubtitle)
        pnlHeader.Controls.Add(btnRefresh)

        AddHandler pnlHeader.Resize,
            Sub(sender As Object, e As EventArgs)

                btnRefresh.Left = Math.Max(
                    0,
                    pnlHeader.ClientSize.Width - btnRefresh.Width - 8
                )

                btnRefresh.Top = 12

            End Sub

        '----------------------------------------------------
        ' SUMMARY CARDS
        '----------------------------------------------------

        tblSummary = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 1,
            .BackColor = BgColor,
            .Margin = New Padding(0),
            .Padding = New Padding(0, 5, 0, 5),
            .GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        }

        For i As Integer = 0 To 3
            tblSummary.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 25.0F)
            )
        Next

        tblSummary.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        lblArrivalsValue = New Label()
        lblDeparturesValue = New Label()
        lblCheckedInValue = New Label()
        lblPendingValue = New Label()

        tblSummary.Controls.Add(
            CreateSummaryCard(
                "Today's Arrivals",
                lblArrivalsValue,
                "Scheduled check-ins",
                Teal
            ),
            0, 0
        )

        tblSummary.Controls.Add(
            CreateSummaryCard(
                "Today's Departures",
                lblDeparturesValue,
                "Scheduled check-outs",
                Color.FromArgb(59, 130, 246)
            ),
            1, 0
        )

        tblSummary.Controls.Add(
            CreateSummaryCard(
                "Currently Checked In",
                lblCheckedInValue,
                "Active stays",
                Color.FromArgb(139, 92, 246)
            ),
            2, 0
        )

        tblSummary.Controls.Add(
            CreateSummaryCard(
                "Outstanding Folios",
                lblPendingValue,
                "Balances awaiting settlement",
                Color.FromArgb(234, 88, 12)
            ),
            3, 0
        )

        '----------------------------------------------------
        ' RECENT BOOKINGS PANEL
        '----------------------------------------------------

        pnlRecent = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(0),
            .Margin = New Padding(0)
        }

        lblRecentTitle = New Label With {
            .Text = "Recent Bookings",
            .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(18, 13)
        }

        ' Fixed width and right alignment prevent the timestamp
        ' from extending beyond the panel's right edge.
        lblLastUpdated = New Label With {
            .Text = "Last updated: Not yet loaded",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = False,
            .Width = 285,
            .Height = 24,
            .TextAlign = ContentAlignment.MiddleRight,
            .AutoEllipsis = True,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .Location = New Point(0, 14)
        }

        dgvRecent = New DataGridView With {
            .Dock = DockStyle.None,
            .Location = New Point(18, 52),
            .Size = New Size(800, 300),
            .Anchor = AnchorStyles.Top Or
                      AnchorStyles.Bottom Or
                      AnchorStyles.Left Or
                      AnchorStyles.Right,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False,
            .ColumnHeadersHeight = 38,
            .ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        }

        dgvRecent.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvRecent.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvRecent.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvRecent.DefaultCellStyle.Font = New Font("Segoe UI", 9)
        dgvRecent.DefaultCellStyle.ForeColor = TextColor
        dgvRecent.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)
        dgvRecent.DefaultCellStyle.SelectionForeColor = Navy

        dgvRecent.RowTemplate.Height = 34

        dgvRecent.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        pnlRecent.Controls.Add(lblRecentTitle)
        pnlRecent.Controls.Add(lblLastUpdated)
        pnlRecent.Controls.Add(dgvRecent)

        '----------------------------------------------------
        ' RESPONSIVE RECENT BOOKINGS LAYOUT
        '----------------------------------------------------

        AddHandler pnlRecent.Resize,
            Sub(sender As Object, e As EventArgs)

                Dim availableWidth As Integer =
                    pnlRecent.ClientSize.Width

                Dim availableHeight As Integer =
                    pnlRecent.ClientSize.Height

                ' Keep timestamp inside the right boundary.
                lblLastUpdated.Left = Math.Max(
                    0,
                    availableWidth - lblLastUpdated.Width - 18
                )

                lblLastUpdated.Top = 14

                ' Hide the timestamp if the panel is too narrow
                ' to show it without overlapping the title.
                lblLastUpdated.Visible =
                    (lblLastUpdated.Left >= lblRecentTitle.Right + 12)

                ' Move the grid upward slightly and let it fill
                ' the available panel space.
                Dim gridLeft As Integer = 18
                Dim gridTop As Integer = 52
                Dim rightPadding As Integer = 18
                Dim bottomPadding As Integer = 18

                Dim gridWidth As Integer =
                    Math.Max(
                        100,
                        availableWidth - gridLeft - rightPadding
                    )

                Dim gridHeight As Integer =
                    Math.Max(
                        80,
                        availableHeight - gridTop - bottomPadding
                    )

                dgvRecent.SetBounds(
                    gridLeft,
                    gridTop,
                    gridWidth,
                    gridHeight
                )

            End Sub

        '----------------------------------------------------
        ' ADD SECTIONS TO MAIN LAYOUT
        '----------------------------------------------------

        mainLayout.Controls.Add(pnlHeader, 0, 0)
        mainLayout.Controls.Add(tblSummary, 0, 1)
        mainLayout.Controls.Add(pnlRecent, 0, 2)

        Me.Controls.Add(mainLayout)

        Me.ResumeLayout(True)

        AddHandler Me.Load, AddressOf StaffHome_Load

    End Sub

    '========================================================
    ' SUMMARY CARD
    '========================================================

    Private Function CreateSummaryCard(
        heading As String,
        valueLabel As Label,
        description As String,
        accentColor As Color
    ) As Panel

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(0, 0, 10, 0),
            .Padding = New Padding(0)
        }

        AddHandler card.Paint,
            Sub(sender As Object, e As PaintEventArgs)

                Using pen As New Pen(BorderColor)
                    e.Graphics.DrawRectangle(
                        pen,
                        0,
                        0,
                        card.Width - 1,
                        card.Height - 1
                    )
                End Using

                Using brush As New SolidBrush(accentColor)
                    e.Graphics.FillRectangle(
                        brush,
                        0,
                        0,
                        5,
                        card.Height
                    )
                End Using

            End Sub

        Dim lblHeading As New Label With {
            .Text = heading,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = TextColor,
            .AutoSize = False,
            .Location = New Point(20, 12),
            .Size = New Size(230, 24),
            .AutoEllipsis = True
        }

        valueLabel.Text = "—"
        valueLabel.Font = New Font("Segoe UI", 25, FontStyle.Bold)
        valueLabel.ForeColor = accentColor
        valueLabel.AutoSize = True
        valueLabel.Location = New Point(20, 36)

        Dim lblDescription As New Label With {
            .Text = description,
            .Font = New Font("Segoe UI", 8),
            .ForeColor = MutedColor,
            .AutoSize = False,
            .AutoEllipsis = True,
            .Location = New Point(20, 80),
            .Size = New Size(230, 22),
            .TextAlign = ContentAlignment.MiddleLeft
        }

        ' Adjust text widths when the card changes size.
        AddHandler card.Resize,
            Sub(sender As Object, e As EventArgs)

                Dim textWidth As Integer =
                    Math.Max(20, card.ClientSize.Width - 35)

                lblHeading.Width = textWidth
                lblDescription.Width = textWidth

            End Sub

        card.Controls.Add(lblHeading)
        card.Controls.Add(valueLabel)
        card.Controls.Add(lblDescription)

        Return card

    End Function

    '========================================================
    ' LOAD / REFRESH
    '========================================================

    Private Sub StaffHome_Load(
        sender As Object,
        e As EventArgs
    )

        RefreshDashboard(Nothing, EventArgs.Empty)

    End Sub

    Private Sub RefreshDashboard(
        sender As Object,
        e As EventArgs
    )

        If isLoading Then Return

        isLoading = True
        btnRefresh.Enabled = False
        btnRefresh.Text = "Refreshing..."

        Try

            LoadDashboardData()

            lblLastUpdated.Text =
                "Last updated: " &
                DateTime.Now.ToString("MMM dd, yyyy hh:mm tt")

        Catch ex As Exception

            MessageBox.Show(
                "Unable to load the staff dashboard." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

        Finally

            btnRefresh.Enabled = True
            btnRefresh.Text = "Refresh Data"
            isLoading = False

        End Try

    End Sub

    '========================================================
    ' LOAD DATABASE DATA
    '========================================================

    Private Sub LoadDashboardData()

        Using conn As MySqlConnection = DbConnection.GetConnection()

            If conn.State <> ConnectionState.Open Then
                conn.Open()
            End If

            Dim bookingColumns As List(Of String) =
                GetColumns(conn, "bookings")

            Dim folioColumns As List(Of String) =
                GetColumns(conn, "guest_folios")

            ' Today's arrivals
            Dim arrivalColumn As String =
                FindColumn(
                    bookingColumns,
                    "check_in_date",
                    "checkin_date",
                    "arrival_date",
                    "start_date"
                )

            lblArrivalsValue.Text =
                CountBookingsForDate(
                    conn,
                    bookingColumns,
                    arrivalColumn,
                    Date.Today
                )

            ' Today's departures
            Dim departureColumn As String =
                FindColumn(
                    bookingColumns,
                    "check_out_date",
                    "checkout_date",
                    "departure_date",
                    "end_date"
                )

            lblDeparturesValue.Text =
                CountBookingsForDate(
                    conn,
                    bookingColumns,
                    departureColumn,
                    Date.Today
                )

            ' Currently checked-in bookings
            Dim statusColumn As String =
                FindColumn(
                    bookingColumns,
                    "booking_status",
                    "status"
                )

            lblCheckedInValue.Text =
                CountCheckedInBookings(
                    conn,
                    bookingColumns,
                    statusColumn
                )

            ' Outstanding folios
            lblPendingValue.Text =
                CountOutstandingFolios(
                    conn,
                    folioColumns
                )

            ' Recent bookings
            LoadRecentBookings(
                conn,
                bookingColumns
            )

        End Using

    End Sub

    '========================================================
    ' DISCOVER TABLE COLUMNS
    '========================================================

    Private Function GetColumns(
        conn As MySqlConnection,
        tableName As String
    ) As List(Of String)

        Dim result As New List(Of String)

        Const sql As String =
            "SELECT COLUMN_NAME " &
            "FROM INFORMATION_SCHEMA.COLUMNS " &
            "WHERE TABLE_SCHEMA = DATABASE() " &
            "AND TABLE_NAME = @tableName"

        Using cmd As New MySqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@tableName", tableName)

            Using reader As MySqlDataReader = cmd.ExecuteReader()

                While reader.Read()
                    result.Add(
                        Convert.ToString(reader("COLUMN_NAME"))
                    )
                End While

            End Using

        End Using

        Return result

    End Function

    Private Function FindColumn(
        columns As List(Of String),
        ParamArray candidates() As String
    ) As String

        For Each candidate As String In candidates

            For Each actualColumn As String In columns

                If String.Equals(
                    actualColumn,
                    candidate,
                    StringComparison.OrdinalIgnoreCase
                ) Then

                    Return actualColumn

                End If

            Next

        Next

        Return Nothing

    End Function

    Private Function QuoteIdentifier(
        identifier As String
    ) As String

        Return "`" & identifier.Replace("`", "``") & "`"

    End Function

    '========================================================
    ' ARRIVALS / DEPARTURES
    '========================================================

    Private Function CountBookingsForDate(
        conn As MySqlConnection,
        columns As List(Of String),
        dateColumn As String,
        targetDate As Date
    ) As String

        If columns.Count = 0 OrElse
           String.IsNullOrWhiteSpace(dateColumn) Then

            Return "—"

        End If

        Dim sql As String =
            "SELECT COUNT(*) FROM `bookings` " &
            "WHERE DATE(" & QuoteIdentifier(dateColumn) &
            ") = @targetDate"

        Using cmd As New MySqlCommand(sql, conn)

            cmd.Parameters.AddWithValue("@targetDate", targetDate.Date)

            Return Convert.ToInt32(
                cmd.ExecuteScalar()
            ).ToString()

        End Using

    End Function

    '========================================================
    ' CURRENTLY CHECKED-IN BOOKINGS
    '========================================================

    Private Function CountCheckedInBookings(
        conn As MySqlConnection,
        columns As List(Of String),
        statusColumn As String
    ) As String

        If columns.Count = 0 OrElse
           String.IsNullOrWhiteSpace(statusColumn) Then

            Return "—"

        End If

        Dim sql As String =
            "SELECT COUNT(*) FROM `bookings` " &
            "WHERE LOWER(REPLACE(REPLACE(TRIM(" &
            QuoteIdentifier(statusColumn) &
            "), '_', ' '), '-', ' ')) " &
            "IN ('checked in', 'checked-in', 'occupied')"

        Using cmd As New MySqlCommand(sql, conn)

            Return Convert.ToInt32(
                cmd.ExecuteScalar()
            ).ToString()

        End Using

    End Function

    '========================================================
    ' OUTSTANDING FOLIOS
    '========================================================

    Private Function CountOutstandingFolios(
        conn As MySqlConnection,
        columns As List(Of String)
    ) As String

        If columns.Count = 0 Then Return "—"

        Dim balanceColumn As String =
            FindColumn(
                columns,
                "balance_due",
                "outstanding_balance",
                "remaining_balance",
                "balance"
            )

        If String.IsNullOrWhiteSpace(balanceColumn) Then
            Return "—"
        End If

        Dim sql As String =
            "SELECT COUNT(*) FROM `guest_folios` " &
            "WHERE " & QuoteIdentifier(balanceColumn) & " > 0"

        Using cmd As New MySqlCommand(sql, conn)

            Return Convert.ToInt32(
                cmd.ExecuteScalar()
            ).ToString()

        End Using

    End Function

    '========================================================
    ' RECENT BOOKINGS
    '========================================================

    Private Sub LoadRecentBookings(
        conn As MySqlConnection,
        columns As List(Of String)
    )

        dgvRecent.DataSource = Nothing
        dgvRecent.Columns.Clear()

        If columns.Count = 0 Then

            ShowRecentBookingsMessage(
                "The bookings table or its columns could not be found."
            )

            Return

        End If

        Dim idColumn As String =
            FindColumn(columns, "booking_id", "id")

        Dim statusColumn As String =
            FindColumn(columns, "booking_status", "status")

        Dim arrivalColumn As String =
            FindColumn(
                columns,
                "check_in_date",
                "checkin_date",
                "arrival_date",
                "start_date"
            )

        Dim departureColumn As String =
            FindColumn(
                columns,
                "check_out_date",
                "checkout_date",
                "departure_date",
                "end_date"
            )

        Dim createdColumn As String =
            FindColumn(
                columns,
                "created_at",
                "booking_date",
                "created_on"
            )

        Dim selectParts As New List(Of String)

        If Not String.IsNullOrWhiteSpace(idColumn) Then
            selectParts.Add(
                QuoteIdentifier(idColumn) & " AS `Booking ID`"
            )
        End If

        If Not String.IsNullOrWhiteSpace(statusColumn) Then
            selectParts.Add(
                QuoteIdentifier(statusColumn) & " AS `Status`"
            )
        End If

        If Not String.IsNullOrWhiteSpace(arrivalColumn) Then
            selectParts.Add(
                QuoteIdentifier(arrivalColumn) & " AS `Arrival`"
            )
        End If

        If Not String.IsNullOrWhiteSpace(departureColumn) Then
            selectParts.Add(
                QuoteIdentifier(departureColumn) & " AS `Departure`"
            )
        End If

        If selectParts.Count = 0 Then

            ShowRecentBookingsMessage(
                "No supported booking columns were found."
            )

            Return

        End If

        Dim orderColumn As String =
            If(
                Not String.IsNullOrWhiteSpace(createdColumn),
                createdColumn,
                If(
                    Not String.IsNullOrWhiteSpace(idColumn),
                    idColumn,
                    Nothing
                )
            )

        Dim sql As String =
            "SELECT " & String.Join(", ", selectParts) &
            " FROM `bookings`"

        If Not String.IsNullOrWhiteSpace(orderColumn) Then
            sql &= " ORDER BY " &
                   QuoteIdentifier(orderColumn) & " DESC"
        End If

        sql &= " LIMIT 10"

        Dim table As New DataTable()

        Using adapter As New MySqlDataAdapter(sql, conn)
            adapter.Fill(table)
        End Using

        dgvRecent.DataSource = table

        If table.Rows.Count = 0 Then
            ShowRecentBookingsMessage(
                "No booking records are available yet."
            )
        End If

    End Sub

    '========================================================
    ' EMPTY-STATE MESSAGE
    '========================================================

    Private Sub ShowRecentBookingsMessage(
        message As String
    )

        Dim table As New DataTable()

        table.Columns.Add("Information", GetType(String))
        table.Rows.Add(message)

        dgvRecent.DataSource = table

    End Sub

End Class
