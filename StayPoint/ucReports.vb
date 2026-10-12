
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports MySql.Data.MySqlClient

Public Class ucReports

    '========================================================
    ' STAYPOINT COLORS
    '========================================================
    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly SidebarColor As Color =
        Color.FromArgb(32, 58, 95)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly BgColor As Color =
        Color.FromArgb(240, 244, 248)

    Private ReadOnly Muted As Color =
        Color.FromArgb(100, 116, 139)

    Private ReadOnly BorderColor As Color =
        Color.FromArgb(226, 232, 240)

    Private ReadOnly PesoCulture As CultureInfo =
        CultureInfo.GetCultureInfo("en-PH")

    Private currentReport As DataTable

    Private pnlHeader As Panel
    Private pnlCards As FlowLayoutPanel
    Private pnlFilters As Panel
    Private pnlTable As Panel

    Private lblTotalBookings As Label
    Private lblTotalRooms As Label
    Private lblPayments As Label
    Private lblParking As Label
    Private lblRecordCount As Label

    Private cboReportType As ComboBox
    Private dtpStart As DateTimePicker
    Private dtpEnd As DateTimePicker

    Private btnGenerate As Button
    Private btnRefresh As Button

    Private dgvReports As DataGridView

    '========================================================
    ' CONSTRUCTOR
    '========================================================
    Public Sub New()

        InitializeComponent()
        BuildReportsUI()

    End Sub

    '========================================================
    ' BUILD USER INTERFACE
    '========================================================
    Private Sub BuildReportsUI()

        Me.SuspendLayout()
        Me.Controls.Clear()

        Me.BackColor = BgColor
        Me.Dock = DockStyle.Fill
        Me.Padding = New Padding(0)
        Me.AutoScroll = False

        '---------------- HEADER ----------------

        pnlHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 72,
            .BackColor = BgColor
        }

        Dim lblTitle As New Label With {
            .Text = "Reports",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(22, 8)
        }

        Dim lblSubtitle As New Label With {
            .Text = "View bookings, rooms, payments, and parking records.",
            .Font = New Font("Segoe UI", 9.5F),
            .ForeColor = Muted,
            .AutoSize = True,
            .Location = New Point(24, 43)
        }

        pnlHeader.Controls.Add(lblTitle)
        pnlHeader.Controls.Add(lblSubtitle)

        '---------------- SUMMARY CARDS ----------------

        pnlCards = New FlowLayoutPanel With {
            .Name = "pnlCards",
            .Dock = DockStyle.Top,
            .Height = 102,
            .Padding = New Padding(14, 4, 14, 4),
            .BackColor = BgColor,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .AutoScroll = True,
            .Margin = New Padding(0)
        }

        lblTotalBookings = AddSummaryCard(
            "Total Bookings",
            "0",
            "Bookings recorded"
        )

        lblTotalRooms = AddSummaryCard(
            "Total Rooms",
            "0",
            "Room inventory"
        )

        lblPayments = AddSummaryCard(
            "Payments Collected",
            "₱0.00",
            "Recorded payments"
        )

        lblParking = AddSummaryCard(
            "Parking Revenue",
            "N/A",
            "Parking records"
        )

        '---------------- FILTERS ----------------

        pnlFilters = New Panel With {
            .Name = "pnlFilters",
            .Dock = DockStyle.Top,
            .Height = 76,
            .BackColor = Color.White,
            .Padding = New Padding(14, 8, 14, 6)
        }

        Dim lblReport As New Label With {
            .Text = "Report Type",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .Location = New Point(16, 5),
            .AutoSize = True
        }

        cboReportType = New ComboBox With {
            .Name = "cboReportType",
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9),
            .Location = New Point(16, 27),
            .Size = New Size(175, 28)
        }

        cboReportType.Items.AddRange(New Object() {
            "Bookings",
            "Rooms",
            "Payments",
            "Parking"
        })

        cboReportType.SelectedIndex = 0

        Dim lblStart As New Label With {
            .Text = "Start Date",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .Location = New Point(205, 5),
            .AutoSize = True
        }

        dtpStart = New DateTimePicker With {
            .Name = "dtpStart",
            .Format = DateTimePickerFormat.Short,
            .Location = New Point(205, 27),
            .Size = New Size(120, 28),
            .Value = Date.Today.AddDays(-30)
        }

        Dim lblEnd As New Label With {
            .Text = "End Date",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .Location = New Point(340, 5),
            .AutoSize = True
        }

        dtpEnd = New DateTimePicker With {
            .Name = "dtpEnd",
            .Format = DateTimePickerFormat.Short,
            .Location = New Point(340, 27),
            .Size = New Size(120, 28),
            .Value = Date.Today
        }

        btnGenerate = CreateButton("Generate Report", Teal)
        btnGenerate.Location = New Point(475, 25)
        btnGenerate.Size = New Size(135, 32)

        btnRefresh = CreateButton("Refresh", SidebarColor)
        btnRefresh.Location = New Point(620, 25)
        btnRefresh.Size = New Size(85, 32)

        pnlFilters.Controls.AddRange(New Control() {
            lblReport,
            cboReportType,
            lblStart,
            dtpStart,
            lblEnd,
            dtpEnd,
            btnGenerate,
            btnRefresh
        })

        '---------------- RESULTS PANEL ----------------

        pnlTable = New Panel With {
            .Name = "pnlTable",
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim lblTableTitle As New Label With {
            .Text = "Report Results",
            .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(16, 10)
        }

        lblRecordCount = New Label With {
            .Text = "0 records",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = Muted,
            .AutoSize = True,
            .Location = New Point(18, 37)
        }

        dgvReports = New DataGridView With {
            .Name = "dgvReports",
            .Location = New Point(16, 62),
            .Size = New Size(700, 300),
            .Anchor = AnchorStyles.Top Or
                      AnchorStyles.Bottom Or
                      AnchorStyles.Left Or
                      AnchorStyles.Right,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False,
            .GridColor = BorderColor,
            .AutoGenerateColumns = True
        }

        dgvReports.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvReports.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dgvReports.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvReports.DefaultCellStyle.Font =
            New Font("Segoe UI", 9)

        dgvReports.DefaultCellStyle.ForeColor = Navy
        dgvReports.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(204, 251, 241)

        dgvReports.DefaultCellStyle.SelectionForeColor = Navy
        dgvReports.RowTemplate.Height = 30

        dgvReports.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        pnlTable.Controls.Add(lblTableTitle)
        pnlTable.Controls.Add(lblRecordCount)
        pnlTable.Controls.Add(dgvReports)

        ' Add in reverse docking order.
        Me.Controls.Add(pnlTable)
        Me.Controls.Add(pnlFilters)
        Me.Controls.Add(pnlCards)
        Me.Controls.Add(pnlHeader)

        AddHandler btnGenerate.Click,
            AddressOf GenerateReport_Click

        AddHandler btnRefresh.Click,
            AddressOf RefreshReport_Click

        AddHandler cboReportType.SelectedIndexChanged,
            AddressOf ReportType_Changed

        AddHandler Me.Load, AddressOf ucReports_Load
        AddHandler Me.Resize, AddressOf AdjustLayout
        AddHandler pnlTable.Resize, AddressOf AdjustLayout

        AdjustLayout(Me, EventArgs.Empty)

        Me.ResumeLayout(True)

    End Sub

    '========================================================
    ' SUMMARY CARDS
    '========================================================
    Private Function AddSummaryCard(
        title As String,
        initialValue As String,
        description As String
    ) As Label

        Dim card As New Panel With {
            .Width = 240,
            .Height = 88,
            .BackColor = Color.White,
            .Margin = New Padding(0, 0, 12, 0),
            .Padding = New Padding(10)
        }

        Dim lblTitle As New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Muted,
            .Location = New Point(10, 6),
            .AutoSize = True
        }

        Dim lblValue As New Label With {
            .Text = initialValue,
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = Navy,
            .Location = New Point(10, 26),
            .AutoSize = True
        }

        Dim lblDescription As New Label With {
            .Text = description,
            .Font = New Font("Segoe UI", 8),
            .ForeColor = Muted,
            .Location = New Point(10, 61),
            .AutoSize = True
        }

        card.Controls.Add(lblTitle)
        card.Controls.Add(lblValue)
        card.Controls.Add(lblDescription)

        pnlCards.Controls.Add(card)

        Return lblValue

    End Function

    '========================================================
    ' BUTTON STYLE
    '========================================================
    Private Function CreateButton(
        caption As String,
        buttonColor As Color
    ) As Button

        Dim btn As New Button With {
            .Text = caption,
            .BackColor = buttonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }

        btn.FlatAppearance.BorderSize = 0

        Return btn

    End Function

    '========================================================
    ' INITIAL LOAD
    '========================================================
    Private Sub ucReports_Load(
        sender As Object,
        e As EventArgs
    )

        LoadSummaryCards()
        GenerateReport()

    End Sub

    '========================================================
    ' RESPONSIVE LAYOUT
    '========================================================
    Private Sub AdjustLayout(
        sender As Object,
        e As EventArgs
    )

        If pnlCards Is Nothing Then Return

        Dim availableWidth As Integer =
            Math.Max(200,
                pnlCards.ClientSize.Width -
                pnlCards.Padding.Left -
                pnlCards.Padding.Right)

        Dim cardCount As Integer = pnlCards.Controls.Count

        If cardCount > 0 Then

            Dim spacing As Integer = 12

            Dim cardWidth As Integer =
                Math.Max(180,
                    (availableWidth -
                    spacing * (cardCount - 1)) \ cardCount)

            For Each card As Control In pnlCards.Controls
                card.Width = cardWidth
            Next

        End If

        If dgvReports IsNot Nothing AndAlso
           pnlTable IsNot Nothing Then

            dgvReports.Location = New Point(16, 62)

            dgvReports.Size = New Size(
                Math.Max(100, pnlTable.ClientSize.Width - 32),
                Math.Max(80, pnlTable.ClientSize.Height - 78)
            )

        End If

    End Sub

    '========================================================
    ' DATABASE CONNECTION
    '========================================================
    Private Function OpenDatabaseConnection() As MySqlConnection

        Return DbConnection.GetConnection()

    End Function

    '========================================================
    ' SUMMARY CARDS
    '========================================================
    Private Sub LoadSummaryCards()

        Try
            Using conn As MySqlConnection =
                OpenDatabaseConnection()

                conn.Open()

                Try
                    lblTotalBookings.Text =
                        GetCount(conn, "bookings")
                Catch
                    lblTotalBookings.Text = "N/A"
                End Try

                Try
                    lblTotalRooms.Text =
                        GetCount(conn, "rooms")
                Catch
                    lblTotalRooms.Text = "N/A"
                End Try

                Try
                    lblPayments.Text =
                        "₱" & GetPaymentTotal(conn).ToString(
                            "N2", PesoCulture)
                Catch
                    lblPayments.Text = "N/A"
                End Try

                Try
                    lblParking.Text =
                        "₱" & GetParkingTotal(conn).ToString(
                            "N2", PesoCulture)
                Catch
                    lblParking.Text = "N/A"
                End Try

            End Using

        Catch ex As Exception

            MessageBox.Show(
                "Unable to connect to the database." &
                Environment.NewLine & ex.Message,
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

        End Try

    End Sub

    Private Function GetCount(
        conn As MySqlConnection,
        tableName As String
    ) As String

        Dim sql As String =
            "SELECT COUNT(*) FROM `" & tableName & "`"

        Using cmd As New MySqlCommand(sql, conn)
            Return Convert.ToInt32(
                cmd.ExecuteScalar()).ToString()
        End Using

    End Function

    Private Function GetPaymentTotal(
        conn As MySqlConnection
    ) As Decimal

        Dim sql As String =
            "SELECT COALESCE(SUM(amount), 0) FROM payments"

        Using cmd As New MySqlCommand(sql, conn)
            Return Convert.ToDecimal(cmd.ExecuteScalar())
        End Using

    End Function

    Private Function GetParkingTotal(
        conn As MySqlConnection
    ) As Decimal

        ' Replace these identifiers only after verifying
        ' the actual parking table and fee column.
        Dim sql As String =
            "SELECT COALESCE(SUM(fee), 0) FROM parking"

        Using cmd As New MySqlCommand(sql, conn)
            Return Convert.ToDecimal(cmd.ExecuteScalar())
        End Using

    End Function

    '========================================================
    ' REPORT BUTTON EVENTS
    '========================================================
    Private Sub GenerateReport_Click(
        sender As Object,
        e As EventArgs
    )

        GenerateReport()

    End Sub

    Private Sub RefreshReport_Click(
        sender As Object,
        e As EventArgs
    )

        LoadSummaryCards()
        GenerateReport()

    End Sub

    Private Sub ReportType_Changed(
        sender As Object,
        e As EventArgs
    )

        If Me.IsHandleCreated Then
            GenerateReport()
        End If

    End Sub

    '========================================================
    ' GENERATE REPORT
    '========================================================
    Private Sub GenerateReport()

        If cboReportType.SelectedItem Is Nothing Then Return

        If dtpStart.Value.Date > dtpEnd.Value.Date Then

            MessageBox.Show(
                "The start date must not be later than the end date.",
                "Invalid Date Range",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

            Return

        End If

        Try
            Using conn As MySqlConnection =
                OpenDatabaseConnection()

                conn.Open()

                Dim reportType As String =
                    cboReportType.SelectedItem.ToString()

                Dim sql As String = ""

                Select Case reportType

                    Case "Bookings"

                        sql =
                            "SELECT * FROM bookings " &
                            "WHERE created_at >= @startDate " &
                            "AND created_at < @endDate " &
                            "ORDER BY created_at DESC"

                    Case "Rooms"

                        sql =
                            "SELECT * FROM rooms"

                    Case "Payments"

                        sql =
                            "SELECT * FROM payments " &
                            "WHERE DATE(payment_date) >= @startDate " &
                            "AND DATE(payment_date) <= @endDate " &
                            "ORDER BY payment_date DESC"

                    Case "Parking"

                        sql =
                            "SELECT * FROM parking"

                End Select

                Using cmd As New MySqlCommand(sql, conn)

                    If reportType = "Bookings" OrElse
                       reportType = "Payments" Then


                        cmd.Parameters.AddWithValue(
                            "@startDate",
                            dtpStart.Value.Date
                        )

                        cmd.Parameters.AddWithValue(
                            "@endDate",
                            dtpEnd.Value.Date.AddDays(1)
                        )


                    End If

                    Using adapter As New MySqlDataAdapter(cmd)

                        currentReport = New DataTable()
                        adapter.Fill(currentReport)

                    End Using

                End Using

            End Using

            dgvReports.DataSource = currentReport

            ApplyFriendlyColumnNames()

            lblRecordCount.Text =
                currentReport.Rows.Count.ToString() &
                If(currentReport.Rows.Count = 1,
                   " record", " records")

        Catch ex As Exception

            MessageBox.Show(
                "Unable to load the " &
                cboReportType.SelectedItem.ToString() &
                " report." & Environment.NewLine &
                "Please check the database table and column names." &
                Environment.NewLine & ex.Message,
                "Report Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

        End Try

    End Sub

    '========================================================
    ' FRIENDLY COLUMN HEADINGS
    '========================================================
    Private Sub ApplyFriendlyColumnNames()

        If dgvReports.Columns.Count = 0 Then Return

        Dim friendlyNames As New Dictionary(Of String, String)(
            StringComparer.OrdinalIgnoreCase) From {
                {"booking_id", "Booking ID"},
                {"booking_ref", "Booking No."},
                {"guest_id", "Guest ID"},
                {"guest_name", "Guest Name"},
                {"booking_date", "Booking Date"},
                {"check_in_date", "Check-in"},
                {"check_out_date", "Check-out"},
                {"adults", "Adults"},
                {"children", "Children"},
                {"booking_status", "Status"},
                {"special_requests", "Special Requests"},
                {"created_by", "Created By"},
                {"created_at", "Date Created"},
                {"updated_at", "Last Updated"},
                {"room_id", "Room ID"},
                {"room_number", "Room No."},
                {"room_type", "Room Type"},
                {"room_status", "Room Status"},
                {"status", "Status"},
                {"price", "Room Rate"},
                {"rate", "Room Rate"},
                {"amount", "Amount (₱)"},
                {"payment_id", "Payment ID"},
                {"payment_date", "Payment Date"},
                {"payment_method", "Payment Method"},
                {"payment_status", "Payment Status"},
                {"reference_number", "Reference No."},
                {"parking_id", "Parking ID"},
                {"vehicle_number", "Plate Number"},
                {"plate_number", "Plate Number"},
                {"entry_time", "Entry Time"},
                {"exit_time", "Exit Time"},
                {"duration", "Duration"},
                {"fee", "Parking Fee (₱)"},
                {"total_fee", "Total Fee (₱)"},
                {"balance", "Balance (₱)"},
                {"first_name", "First Name"},
                {"last_name", "Last Name"},
                {"email", "Email"},
                {"phone", "Phone"},
                {"contact_number", "Contact Number"}
            }

        For Each column As DataGridViewColumn In dgvReports.Columns

            Dim originalName As String = column.Name

            If friendlyNames.ContainsKey(originalName) Then

                column.HeaderText = friendlyNames(originalName)

            Else

                ' Make other column names readable without
                ' changing the underlying database fields.
                Dim readableName As String =
                    originalName.Replace("_", " ")

                If readableName.Length > 0 Then
                    readableName =
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                            readableName)
                End If

                column.HeaderText = readableName

            End If

        Next

    End Sub

End Class
