Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucCheckInOut

    '========================================================
    ' COLORS
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

    '========================================================
    ' CONTROLS
    '========================================================

    Private pnlHeader As Panel
    Private pnlSummary As FlowLayoutPanel
    Private pnlRecords As Panel
    Private pnlRecordsHeader As Panel
    Private pnlFilters As Panel
    Private pnlActions As Panel

    Private lblTotalExpected As Label
    Private lblCurrentlyIn As Label
    Private lblExpectedOut As Label
    Private lblCompleted As Label

    Private lblRecordCount As Label
    Private lblMessage As Label

    Private txtSearch As TextBox
    Private cboView As ComboBox

    Private btnCheckIn As Button
    Private btnCheckOut As Button
    Private btnRefresh As Button

    Private dgvRecords As DataGridView

    Private sourceTable As DataTable
    Private displayTable As DataTable

    Private isLoading As Boolean = False

    '========================================================
    ' INITIALIZE
    '========================================================

    Private Function NormalizeBookingStatus(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then
            Return ""
        End If

        Return Convert.ToString(value).
        Trim().
        Replace("-", "_").
        Replace(" ", "_").
        ToUpperInvariant()
    End Function


    Public Sub New()

        InitializeComponent()
        BuildInterface()

    End Sub

    Private Sub ucCheckInCheckOut_Load(
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
        ' HEADER
        '------------------------------

        pnlHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 90,
            .BackColor = BgColor
        }

        Dim lblTitle As New Label With {
            .Text = "Check-In / Check-Out",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(24, 12)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Manage guest arrivals, active stays, and departures.",
            .Font = New Font("Segoe UI", 10),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(27, 53)
        }

        pnlHeader.Controls.Add(lblTitle)
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

        lblTotalExpected = New Label()
        lblCurrentlyIn = New Label()
        lblExpectedOut = New Label()
        lblCompleted = New Label()

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Expected Check-Ins",
                lblTotalExpected,
                Navy
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Currently Checked-In",
                lblCurrentlyIn,
                Teal
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Expected Check-Outs",
                lblExpectedOut,
                Color.FromArgb(217, 119, 6)
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Completed Check-Outs",
                lblCompleted,
                Color.FromArgb(100, 116, 139)
            )
        )

        '------------------------------
        ' RECORDS CONTAINER
        '------------------------------

        pnlRecords = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        '------------------------------
        ' RECORDS HEADER
        '------------------------------

        pnlRecordsHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 48,
            .BackColor = Color.White
        }

        Dim lblRecordsTitle As New Label With {
            .Text = "Guest Stay Records",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(2, 10)
        }

        lblRecordCount = New Label With {
            .Text = "0 records",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True
        }

        AddHandler pnlRecordsHeader.Resize,
            AddressOf PositionRecordCount

        pnlRecordsHeader.Controls.Add(lblRecordsTitle)
        pnlRecordsHeader.Controls.Add(lblRecordCount)

        '------------------------------
        ' FILTERS
        '------------------------------

        pnlFilters = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .BackColor = Color.White
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
            .Size = New Size(270, 32)
        }

        Dim lblView As New Label With {
            .Text = "Record Type",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(288, 2)
        }

        cboView = New ComboBox With {
            .Font = New Font("Segoe UI", 10),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(288, 27),
            .Size = New Size(165, 32)
        }

        cboView.Items.AddRange(
            New Object() {
                "All Records",
                "Check-In",
                "Checked-In",
                "Check-Out",
                "Checked-Out"
            }
        )

        cboView.SelectedIndex = 0

        btnRefresh = CreateButton(
            "Refresh",
            Navy,
            New Size(95, 32)
        )

        btnRefresh.Location = New Point(465, 27)

        AddHandler txtSearch.TextChanged,
            AddressOf ApplyFilters

        AddHandler cboView.SelectedIndexChanged,
            AddressOf ApplyFilters

        AddHandler btnRefresh.Click,
            AddressOf RefreshBookings

        pnlFilters.Controls.Add(lblSearch)
        pnlFilters.Controls.Add(txtSearch)
        pnlFilters.Controls.Add(lblView)
        pnlFilters.Controls.Add(cboView)
        pnlFilters.Controls.Add(btnRefresh)

        '------------------------------
        ' DATA GRID
        '------------------------------

        dgvRecords = New DataGridView With {
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
            .EnableHeadersVisualStyles = False
        }

        dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White

        dgvRecords.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvRecords.ColumnHeadersHeight = 42

        dgvRecords.DefaultCellStyle.Font =
            New Font("Segoe UI", 9)

        dgvRecords.DefaultCellStyle.ForeColor =
            Color.FromArgb(51, 65, 85)

        dgvRecords.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)

        dgvRecords.DefaultCellStyle.SelectionForeColor = Navy

        dgvRecords.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        dgvRecords.RowTemplate.Height = 38

        AddHandler dgvRecords.SelectionChanged,
            AddressOf UpdateActionButtons

        UpdateActionButtons(Nothing, EventArgs.Empty)

        '------------------------------
        ' ACTION BUTTONS
        '------------------------------

        pnlActions = New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 58,
            .BackColor = Color.White,
            .Padding = New Padding(0, 8, 0, 4)
        }

        btnCheckIn = CreateButton(
    "Check In Guest",
    Teal,
    New Size(145, 36)
)

        btnCheckOut = CreateButton(
    "Check Out Guest",
    Navy,
    New Size(155, 36)
)

        btnCheckIn.Location = New Point(0, 8)
        btnCheckOut.Location = New Point(155, 8)

        AddHandler btnCheckIn.Click,
            AddressOf CheckInGuest

        AddHandler btnCheckOut.Click,
            AddressOf CheckOutGuest

        btnCheckIn.Enabled = False
        btnCheckOut.Enabled = False

        pnlActions.Controls.Add(btnCheckIn)
        pnlActions.Controls.Add(btnCheckOut)

        '------------------------------
        ' MESSAGE
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
        ' ASSEMBLE
        '------------------------------

        pnlRecords.Controls.Add(dgvRecords)
        pnlRecords.Controls.Add(lblMessage)
        pnlRecords.Controls.Add(pnlActions)
        pnlRecords.Controls.Add(pnlFilters)
        pnlRecords.Controls.Add(pnlRecordsHeader)

        Me.Controls.Add(pnlRecords)
        Me.Controls.Add(pnlSummary)
        Me.Controls.Add(pnlHeader)

        Me.ResumeLayout(True)

    End Sub

    '========================================================
    ' REUSABLE SUMMARY CARD
    '========================================================

    Private Function CreateSummaryCard(
        title As String,
        valueLabel As Label,
        accentColor As Color
    ) As Panel

        Dim card As New Panel With {
            .Size = New Size(220, 84),
            .BackColor = Color.White,
            .Margin = New Padding(4, 4, 10, 4)
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

    Private Function CreateButton(
        caption As String,
        buttonColor As Color,
        buttonSize As Size
    ) As Button

        Dim result As New Button With {
            .Text = caption,
            .Size = buttonSize,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .BackColor = buttonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand
        }

        result.FlatAppearance.BorderSize = 0

        Return result

    End Function

    Private Sub PositionRecordCount(
        sender As Object,
        e As EventArgs
    )

        If pnlRecordsHeader Is Nothing OrElse
           lblRecordCount Is Nothing Then Return

        lblRecordCount.Location = New Point(
            Math.Max(
                4,
                pnlRecordsHeader.ClientSize.Width -
                lblRecordCount.Width - 4
            ),
            15
        )

    End Sub

    '========================================================
    ' LOAD BOOKINGS
    '========================================================


    Private Sub LoadBookings()

        If isLoading Then Return

        isLoading = True

        Try
            lblMessage.Visible = False

            Using conn As MySqlConnection = DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then conn.Open()

                Dim sql As String =
                "SELECT b.booking_id, " &
                "CONCAT(g.first_name, ' ', g.last_name) AS guest_name, " &
                "COALESCE(GROUP_CONCAT(r.room_number " &
                "ORDER BY r.room_number SEPARATOR ', '), 'Unassigned') AS room_numbers, " &
                "b.check_in_date, b.check_out_date, b.booking_status " &
                "FROM bookings b " &
                "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                "LEFT JOIN booking_rooms br ON br.booking_id = b.booking_id " &
                "LEFT JOIN rooms r ON r.room_id = br.room_id " &
                "GROUP BY b.booking_id, g.first_name, g.last_name, " &
                "b.check_in_date, b.check_out_date, b.booking_status " &
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

            ShowMessage(
            "Unable to load bookings: " & ex.Message,
            Color.FromArgb(185, 28, 28)
        )

        Finally

            isLoading = False
            UpdateActionButtons(Nothing, EventArgs.Empty)

        End Try

    End Sub


    '========================================================
    ' BUILD DISPLAY TABLE
    '========================================================


    Private Sub BuildDisplayTable()

        displayTable = New DataTable()

        displayTable.Columns.Add("Booking ID")
        displayTable.Columns.Add("Guest")
        displayTable.Columns.Add("Room")
        displayTable.Columns.Add("Check-In Date")
        displayTable.Columns.Add("Check-Out Date")
        displayTable.Columns.Add("Status")

        If sourceTable Is Nothing Then Return

        For Each sourceRow As DataRow In sourceTable.Rows

            Dim row As DataRow = displayTable.NewRow()

            row("Booking ID") = sourceRow("booking_id")
            row("Guest") = Convert.ToString(sourceRow("guest_name"))
            row("Room") = Convert.ToString(sourceRow("room_numbers"))
            row("Check-In Date") = FormatDateValue(sourceRow("check_in_date"))
            row("Check-Out Date") = FormatDateValue(sourceRow("check_out_date"))
            row("Status") = Convert.ToString(sourceRow("booking_status"))

            displayTable.Rows.Add(row)

        Next

    End Sub


    Private Function GetSourceValue(
        sourceRow As DataRow,
        ParamArray possibleNames() As String
    ) As String

        If sourceRow Is Nothing Then Return ""

        For Each column As DataColumn In sourceRow.Table.Columns

            Dim actualName As String =
                NormalizeName(column.ColumnName)

            For Each possibleName As String In possibleNames

                If actualName = NormalizeName(possibleName) Then

                    If sourceRow.IsNull(column) Then Return ""

                    Return Convert.ToString(sourceRow(column))

                End If

            Next

        Next

        Return ""

    End Function

    Private Function FindColumnName(
        table As DataTable,
        ParamArray possibleNames() As String
    ) As String

        If table Is Nothing Then Return Nothing

        For Each column As DataColumn In table.Columns

            For Each possibleName As String In possibleNames

                If NormalizeName(column.ColumnName) =
                   NormalizeName(possibleName) Then

                    Return column.ColumnName

                End If

            Next

        Next

        Return Nothing

    End Function

    Private Function NormalizeName(value As String) As String

        Return value.Replace("_", "").
                     Replace(" ", "").
                     Replace("-", "").
                     ToLowerInvariant()

    End Function

    Private Function FormatDateValue(value As Object) As String

        If value Is Nothing OrElse value Is DBNull.Value Then
            Return ""
        End If

        Dim parsedDate As DateTime

        If TypeOf value Is DateTime Then
            parsedDate = DirectCast(value, DateTime)
        ElseIf Not DateTime.TryParse(
        Convert.ToString(value),
        parsedDate
    ) Then
            Return Convert.ToString(value)
        End If

        Return parsedDate.ToString(
        "MMM dd, yyyy h:mm tt",
        Globalization.CultureInfo.InvariantCulture
    )

    End Function



    '========================================================
    ' SUMMARY COUNTS
    '========================================================

    Private Sub UpdateSummary()

        Dim expectedIn As Integer = 0
        Dim currentlyIn As Integer = 0
        Dim expectedOut As Integer = 0
        Dim completed As Integer = 0

        If sourceTable IsNot Nothing Then

            For Each row As DataRow In sourceTable.Rows


                Dim status As String =
                    NormalizeBookingStatus(
                        GetSourceValue(
                            row,
                            "booking_status",
                            "bookingstatus",
                            "reservation_status",
                            "reservationstatus",
                            "status"
                        )
                    )

                Select Case status

                    Case "CONFIRMED", "APPROVED"
                        expectedIn += 1

                    Case "CHECKED_IN"
                        currentlyIn += 1
                        expectedOut += 1

                    Case "CHECKED_OUT"
                        completed += 1

                End Select


            Next

        End If

        lblTotalExpected.Text = expectedIn.ToString()
        lblCurrentlyIn.Text = currentlyIn.ToString()
        lblExpectedOut.Text = expectedOut.ToString()
        lblCompleted.Text = completed.ToString()

    End Sub

    '========================================================
    ' SEARCH AND FILTER
    '========================================================


    Private Sub ApplyFilters(
    sender As Object,
    e As EventArgs
)

        If displayTable Is Nothing OrElse
       dgvRecords Is Nothing Then Return

        Dim view As New DataView(displayTable)

        Dim searchText As String =
        If(txtSearch Is Nothing, "", txtSearch.Text.Trim())

        Dim selectedView As String =
        If(cboView Is Nothing OrElse cboView.SelectedItem Is Nothing,
           "All Records",
           cboView.SelectedItem.ToString())

        Dim filters As New List(Of String)

        If searchText <> "" Then

            Dim escapedSearch As String =
            searchText.Replace("'", "''").
                       Replace("[", "[[]").
                       Replace("%", "[%]").
                       Replace("*", "[*]")

            filters.Add(
            "(Convert([Booking ID], 'System.String') LIKE '%" &
            escapedSearch & "%' OR " &
            "[Guest] LIKE '%" & escapedSearch & "%' OR " &
            "[Room] LIKE '%" & escapedSearch & "%')"
        )

        End If

        Select Case selectedView

            Case "Check-In"
                filters.Add(
                "[Status] = 'CONFIRMED' OR " &
                "[Status] = 'APPROVED'"
            )

            Case "Checked-In"
                filters.Add("[Status] = 'CHECKED_IN'")

            Case "Check-Out"
                filters.Add("[Status] = 'CHECKED_IN'")

            Case "Checked-Out"
                filters.Add("[Status] = 'CHECKED_OUT'")

        End Select

        Try

            view.RowFilter = String.Join(" AND ", filters)

            dgvRecords.DataSource = view

            lblRecordCount.Text =
            view.Count.ToString() &
            If(view.Count = 1, " record", " records")

            PositionRecordCount(Nothing, EventArgs.Empty)

        Catch ex As Exception

            ShowMessage(
            "Unable to apply the selected filter: " & ex.Message,
            Color.FromArgb(185, 28, 28)
        )

        Finally

            UpdateActionButtons(Nothing, EventArgs.Empty)

        End Try

    End Sub


    '========================================================
    ' ENABLE ACTIONS BASED ON SELECTED BOOKING
    '========================================================




    Private Sub UpdateActionButtons(
    sender As Object,
    e As EventArgs
)
        If btnCheckIn Is Nothing OrElse
       btnCheckOut Is Nothing Then Return

        btnCheckIn.Enabled = False
        btnCheckOut.Enabled = False

        If dgvRecords Is Nothing OrElse
       dgvRecords.CurrentRow Is Nothing OrElse
       dgvRecords.CurrentRow.IsNewRow Then Return

        If Not dgvRecords.Columns.Contains("Status") Then Return

        Dim status As String =
        NormalizeBookingStatus(
            dgvRecords.CurrentRow.Cells("Status").Value
        )

        Select Case status
            Case "CONFIRMED"
                btnCheckIn.Enabled = True

            Case "CHECKED_IN"
                btnCheckOut.Enabled = True
        End Select
    End Sub



    '========================================================
    ' CHECK IN
    '========================================================

    Private Sub CheckInGuest(
        sender As Object,
        e As EventArgs
    )

        ChangeBookingStatus(
            "Confirmed",
            "Checked-In",
            "check in"
        )

    End Sub

    '========================================================
    ' CHECK OUT
    '========================================================

    Private Sub CheckOutGuest(
        sender As Object,
        e As EventArgs
    )

        ChangeBookingStatus(
            "Checked-In",
            "Checked-Out",
            "check out"
        )

    End Sub

    '========================================================
    ' UPDATE BOOKING STATUS
    '========================================================


    Private Sub ChangeBookingStatus(
    expectedStatus As String,
    newStatus As String,
    actionDescription As String
)

        If dgvRecords.CurrentRow Is Nothing Then
            ShowMessage("Select a booking first.", Color.FromArgb(185, 28, 28))
            Return
        End If

        Dim bookingID As Integer

        If Not Integer.TryParse(
        Convert.ToString(dgvRecords.CurrentRow.Cells("Booking ID").Value),
        bookingID
    ) Then
            ShowMessage("Invalid booking ID.", Color.FromArgb(185, 28, 28))
            Return
        End If

        Dim currentStatus As String =
        NormalizeBookingStatus(
            dgvRecords.CurrentRow.Cells("Status").Value
        )

        Dim expected As String = NormalizeBookingStatus(expectedStatus)
        Dim target As String = NormalizeBookingStatus(newStatus)

        If currentStatus <> expected Then
            ShowMessage(
            "This booking is no longer eligible. Refresh and try again.",
            Color.FromArgb(185, 28, 28)
        )
            Return
        End If

        Dim guestName As String =
        Convert.ToString(dgvRecords.CurrentRow.Cells("Guest").Value)

        If MessageBox.Show(
        "Confirm guest " & actionDescription & "?" &
        Environment.NewLine & Environment.NewLine &
        "Guest: " & guestName &
        Environment.NewLine &
        "Booking ID: " & bookingID &
        Environment.NewLine &
        "New status: " & target,
        "Confirm " & actionDescription,
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    ) <> DialogResult.Yes Then Return

        Try

            Using conn As MySqlConnection = DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then conn.Open()

                Using tx As MySqlTransaction = conn.BeginTransaction()

                    Try

                        ' Lock the booking and verify its current status.
                        Using cmd As New MySqlCommand(
                        "SELECT booking_status FROM bookings " &
                        "WHERE booking_id = @id FOR UPDATE",
                        conn, tx
                    )

                            cmd.Parameters.AddWithValue("@id", bookingID)

                            Dim dbStatus As Object = cmd.ExecuteScalar()

                            If dbStatus Is Nothing OrElse
                           dbStatus Is DBNull.Value OrElse
                           NormalizeBookingStatus(dbStatus) <> expected Then

                                Throw New InvalidOperationException(
                                "The booking status changed. Refresh and try again."
                            )

                            End If

                        End Using

                        ' Lock and validate all assigned rooms.
                        Dim roomIDs As New List(Of Integer)

                        Using cmd As New MySqlCommand(
                        "SELECT r.room_id, r.status " &
                        "FROM booking_rooms br " &
                        "INNER JOIN rooms r ON r.room_id = br.room_id " &
                        "WHERE br.booking_id = @id " &
                        "ORDER BY r.room_id FOR UPDATE",
                        conn, tx
                    )

                            cmd.Parameters.AddWithValue("@id", bookingID)

                            Using reader As MySqlDataReader = cmd.ExecuteReader()

                                While reader.Read()

                                    Dim roomID As Integer =
                                    Convert.ToInt32(reader("room_id"))

                                    Dim roomStatus As String =
                                    Convert.ToString(reader("status")).
                                    Trim().
                                    ToUpperInvariant()

                                    If target = "CHECKED_IN" AndAlso
                                   roomStatus <> "AVAILABLE" Then

                                        Throw New InvalidOperationException(
                                        "Room " & roomID &
                                        " is not available. Check its status " &
                                        "before checking in this booking."
                                    )

                                    End If

                                    roomIDs.Add(roomID)

                                End While

                            End Using

                        End Using

                        If roomIDs.Count = 0 Then
                            Throw New InvalidOperationException(
                            "This booking has no assigned rooms. " &
                            "Assign a room before checking in."
                        )
                        End If

                        ' Update the booking only if its status still matches.
                        Using cmd As New MySqlCommand(
                        "UPDATE bookings SET booking_status = @newStatus " &
                        "WHERE booking_id = @id AND booking_status = @oldStatus",
                        conn, tx
                    )

                            cmd.Parameters.AddWithValue("@newStatus", target)
                            cmd.Parameters.AddWithValue("@id", bookingID)
                            cmd.Parameters.AddWithValue("@oldStatus", expected)

                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException(
                                "The booking could not be updated. Refresh and retry."
                            )
                            End If

                        End Using

                        ' Check-in occupies rooms; checkout sends them to cleaning.
                        For Each roomID As Integer In roomIDs

                            Dim newRoomStatus As String =
                            If(target = "CHECKED_IN", "OCCUPIED", "CLEANING")

                            Dim oldRoomStatus As String =
                            If(target = "CHECKED_IN", "AVAILABLE", "OCCUPIED")

                            Using cmd As New MySqlCommand(
                            "UPDATE rooms SET status = @newStatus " &
                            "WHERE room_id = @roomID AND status = @oldStatus",
                            conn, tx
                        )

                                cmd.Parameters.AddWithValue("@newStatus", newRoomStatus)
                                cmd.Parameters.AddWithValue("@roomID", roomID)
                                cmd.Parameters.AddWithValue("@oldStatus", oldRoomStatus)

                                If cmd.ExecuteNonQuery() <> 1 Then
                                    Throw New InvalidOperationException(
                                    "Room " & roomID &
                                    " changed status during this operation. " &
                                    "No changes have been committed."
                                )
                                End If

                            End Using

                        Next

                        tx.Commit()

                    Catch
                        Try
                            tx.Rollback()
                        Catch
                            ' Preserve the original transaction error.
                        End Try

                        Throw
                    End Try

                End Using

            End Using

            ShowMessage(
            "Guest " & actionDescription &
            " completed successfully.",
            Teal
        )

            LoadBookings()

        Catch ex As Exception

            ShowMessage(
            "Unable to " & actionDescription & ": " & ex.Message,
            Color.FromArgb(185, 28, 28)
        )

            LoadBookings()

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

    '========================================================
    ' MESSAGE
    '========================================================

    Private Sub ShowMessage(
        message As String,
        messageColor As Color
    )

        If lblMessage Is Nothing Then Return

        lblMessage.Text = message
        lblMessage.ForeColor = messageColor
        lblMessage.Visible = True

    End Sub

End Class

