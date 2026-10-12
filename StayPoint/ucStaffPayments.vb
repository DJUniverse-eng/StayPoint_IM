
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucStaffPayments
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly Muted As Color = Color.FromArgb(100, 116, 139)

    Private txtSearch As TextBox
    Private dgvBookings As DataGridView
    Private dgvCharges As DataGridView
    Private dgvPayments As DataGridView

    Private lblGuest As Label
    Private lblFolio As Label
    Private lblCharges As Label
    Private lblPaid As Label
    Private lblRefunds As Label
    Private lblBalance As Label
    Private lblMessage As Label

    Private cboMethod As ComboBox
    Private txtAmount As TextBox
    Private txtReference As TextBox
    Private txtNotes As TextBox
    Private btnRecordPayment As Button

    Private selectedBookingId As Integer
    Private selectedFolioId As Integer
    Private selectedFolioStatus As String = ""
    Private currentBalance As Decimal
    Private isLoading As Boolean

    Public Sub New()
        InitializeComponent()
        BuildInterface()
        LoadBookings("")
    End Sub

    '========================================================
    ' INTERFACE
    '========================================================

    Private Sub BuildInterface()
        Controls.Clear()

        Dock = DockStyle.Fill
        BackColor = BgColor
        Font = New Font("Segoe UI", 9.0F)
        MinimumSize = New Size(650, 550)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(12),
            .ColumnCount = 1,
            .RowCount = 6,
            .BackColor = BgColor,
            .Margin = New Padding(0)
        }

        ' Responsive sections: the bottom summary no longer has a
        ' fixed height that clips the guest and payment history.
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 28))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 28))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 44))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))

        Dim heading As New Label With {
            .Text = "Payments & Guest Folio",
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 19, FontStyle.Bold),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Margin = New Padding(0)
        }

        root.Controls.Add(heading, 0, 0)

        '---------------- SEARCH ----------------

        Dim searchPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(6),
            .Margin = New Padding(0, 2, 0, 5)
        }

        txtSearch = New TextBox With {
            .Location = New Point(8, 7),
            .Size = New Size(300, 29),
            .Anchor = AnchorStyles.Left Or AnchorStyles.Top,
            .Font = New Font("Segoe UI", 10)
        }

        Dim btnSearch As Button = CreateButton("Search", Teal)
        btnSearch.SetBounds(318, 4, 85, 34)

        Dim btnRefresh As Button = CreateButton("Refresh", Navy)
        btnRefresh.SetBounds(411, 4, 85, 34)

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

        '---------------- BOOKINGS ----------------

        Dim bookingPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(7),
            .Margin = New Padding(0, 0, 0, 5)
        }

        Dim bookingTitle As New Label With {
            .Text = "Bookings and Guest Folios",
            .Dock = DockStyle.Top,
            .Height = 27,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Navy
        }

        dgvBookings = CreateGrid()
        dgvBookings.Name = "dgvFolioBookings"
        dgvBookings.Dock = DockStyle.Fill

        AddHandler dgvBookings.SelectionChanged,
            AddressOf BookingSelectionChanged

        bookingPanel.Controls.Add(dgvBookings)
        bookingPanel.Controls.Add(bookingTitle)
        root.Controls.Add(bookingPanel, 0, 2)

        '---------------- FOLIO CHARGES ----------------

        Dim chargesPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(7),
            .Margin = New Padding(0, 0, 0, 5)
        }

        Dim chargesTitle As New Label With {
            .Text = "Itemized Folio Charges",
            .Dock = DockStyle.Top,
            .Height = 27,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Navy
        }

        dgvCharges = CreateGrid()
        dgvCharges.Name = "dgvFolioCharges"
        dgvCharges.Dock = DockStyle.Fill

        chargesPanel.Controls.Add(dgvCharges)
        chargesPanel.Controls.Add(chargesTitle)
        root.Controls.Add(chargesPanel, 0, 3)

        '---------------- SUMMARY AND PAYMENT FORM ----------------

        Dim bottom As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = BgColor,
            .Margin = New Padding(0)
        }

        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 53))
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 47))

        ' Left side: summary and payment history.
        Dim summaryPanel As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = Color.White,
            .Padding = New Padding(9),
            .Margin = New Padding(0, 0, 5, 0)
        }

        summaryPanel.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 132))
        summaryPanel.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100))

        Dim summaryTop As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        lblGuest = New Label With {
            .Text = "Guest: Select a booking",
            .Dock = DockStyle.Top,
            .Height = 23,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        }

        Dim summaryFlow As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(0, 2, 0, 0),
            .Margin = New Padding(0)
        }

        lblFolio = MakeSummaryLabel("Folio: None")
        lblCharges = MakeSummaryLabel("Total charges: ₱0.00")
        lblPaid = MakeSummaryLabel("Completed payments: ₱0.00")
        lblRefunds = MakeSummaryLabel("Refunds: ₱0.00")
        lblBalance = MakeSummaryLabel("Outstanding balance: ₱0.00")

        lblBalance.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblBalance.ForeColor = Teal

        summaryFlow.Controls.Add(lblFolio)
        summaryFlow.Controls.Add(lblCharges)
        summaryFlow.Controls.Add(lblPaid)
        summaryFlow.Controls.Add(lblRefunds)
        summaryFlow.Controls.Add(lblBalance)

        summaryTop.Controls.Add(summaryFlow)
        summaryTop.Controls.Add(lblGuest)

        Dim historyPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(0)
        }

        Dim historyTitle As New Label With {
            .Text = "Payment History",
            .Dock = DockStyle.Top,
            .Height = 25,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Navy
        }

        ' Initialize the payment history grid before any database load.
        dgvPayments = CreateGrid()
        dgvPayments.Name = "dgvStaffPaymentHistory"
        dgvPayments.Dock = DockStyle.Fill

        historyPanel.Controls.Add(dgvPayments)
        historyPanel.Controls.Add(historyTitle)

        summaryPanel.Controls.Add(historyPanel, 0, 1)
        summaryPanel.Controls.Add(summaryTop, 0, 0)

        ' Right side: payment entry.
        Dim paymentPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(10),
            .Margin = New Padding(5, 0, 0, 0)
        }

        Dim paymentTitle As New Label With {
            .Text = "Record Cash Payment",
            .Dock = DockStyle.Top,
            .Height = 28,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold)
        }

        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 5,
            .Padding = New Padding(0, 5, 0, 0),
            .Margin = New Padding(0)
        }

        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

        For i As Integer = 0 To 4
            fields.RowStyles.Add(New RowStyle(SizeType.Percent, 20))
        Next

        txtAmount = New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10)
        }

        ' This local desktop application accepts cash only.
        cboMethod = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10),
            .Enabled = False
        }

        cboMethod.Items.Add("CASH")
        cboMethod.SelectedIndex = 0

        txtReference = New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10)
        }

        txtNotes = New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10)
        }

        AddField(fields, 0, "Amount (₱)", txtAmount)
        AddField(fields, 1, "Method", cboMethod)
        AddField(fields, 2, "Reference", txtReference)
        AddField(fields, 3, "Notes", txtNotes)

        btnRecordPayment = CreateButton("Record Cash Payment", Teal)
        btnRecordPayment.Dock = DockStyle.Fill
        btnRecordPayment.Enabled = False

        AddHandler btnRecordPayment.Click,
            AddressOf RecordPayment_Click

        fields.Controls.Add(MakeFieldLabel(""), 0, 4)
        fields.Controls.Add(btnRecordPayment, 1, 4)

        paymentPanel.Controls.Add(fields)
        paymentPanel.Controls.Add(paymentTitle)

        bottom.Controls.Add(summaryPanel, 0, 0)
        bottom.Controls.Add(paymentPanel, 1, 0)
        root.Controls.Add(bottom, 0, 4)

        lblMessage = New Label With {
            .Text = "",
            .Dock = DockStyle.Fill,
            .Height = 24,
            .ForeColor = Muted,
            .BackColor = BgColor,
            .TextAlign = ContentAlignment.MiddleLeft,
            .AutoEllipsis = True,
            .Margin = New Padding(0)
        }

        root.Controls.Add(lblMessage, 0, 5)

        Controls.Add(root)
        root.BringToFront()
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
            .AutoGenerateColumns = True,
            .Margin = New Padding(0)
        }

        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Navy
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)
        grid.ColumnHeadersHeight = 30
        grid.RowTemplate.Height = 27
        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(246, 249, 252)

        Return grid
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

    Private Function MakeSummaryLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .AutoSize = True,
            .Margin = New Padding(0, 2, 0, 2),
            .ForeColor = Muted,
            .Font = New Font("Segoe UI", 9)
        }
    End Function

    Private Function MakeFieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Navy
        }
    End Function

    Private Sub AddField(
        layout As TableLayoutPanel,
        rowIndex As Integer,
        caption As String,
        control As Control
    )
        layout.Controls.Add(MakeFieldLabel(caption), 0, rowIndex)
        layout.Controls.Add(control, 1, rowIndex)
    End Sub

    '========================================================
    ' LOAD BOOKINGS
    '========================================================

    Private Sub LoadBookings(searchText As String)
        Dim previousBookingId As Integer = selectedBookingId

        isLoading = True
        selectedBookingId = 0
        selectedFolioId = 0
        selectedFolioStatus = ""
        currentBalance = 0D

        btnRecordPayment.Enabled = False
        dgvCharges.DataSource = Nothing
        dgvPayments.DataSource = Nothing
        ClearSummary()

        Try
            Dim dt As New DataTable()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const sql As String =
                    "SELECT b.booking_id, " &
                    "b.booking_reference AS `Reference`, " &
                    "CONCAT(g.first_name, ' ', g.last_name) AS `Guest`, " &
                    "b.check_in_date AS `Check-In`, " &
                    "b.check_out_date AS `Check-Out`, " &
                    "b.booking_status AS `Booking Status`, " &
                    "gf.folio_id, gf.folio_status " &
                    "FROM bookings b " &
                    "INNER JOIN guests g ON g.guest_id = b.guest_id " &
                    "LEFT JOIN guest_folios gf ON gf.booking_id = b.booking_id " &
                    "WHERE (@search = '' " &
                    "OR b.booking_reference LIKE @pattern " &
                    "OR g.first_name LIKE @pattern " &
                    "OR g.last_name LIKE @pattern " &
                    "OR CONCAT(g.first_name, ' ', g.last_name) LIKE @pattern) " &
                    "ORDER BY b.booking_date DESC, b.booking_id DESC;"

                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@search", searchText)
                    cmd.Parameters.AddWithValue("@pattern", "%" & searchText & "%")

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            dgvBookings.DataSource = dt

            If dgvBookings.Columns.Contains("booking_id") Then
                dgvBookings.Columns("booking_id").Visible = False
            End If

            If dgvBookings.Columns.Contains("folio_id") Then
                dgvBookings.Columns("folio_id").Visible = False
            End If

            If dgvBookings.Columns.Contains("folio_status") Then
                dgvBookings.Columns("folio_status").Visible = False
            End If

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No bookings found."
            Else
                lblMessage.Text = ""

                If previousBookingId > 0 Then
                    For Each row As DataGridViewRow In dgvBookings.Rows
                        If Convert.ToInt32(
                            row.Cells("booking_id").Value) = previousBookingId Then

                            row.Selected = True
                            dgvBookings.CurrentCell = row.Cells("Guest")
                            Exit For
                        End If
                    Next
                End If
            End If

        Catch ex As Exception
            ShowError("Unable to load bookings.", ex)
        Finally
            isLoading = False
        End Try

        ' Restore selection after loading is complete.
        If dgvBookings.Rows.Count > 0 AndAlso
           dgvBookings.CurrentRow IsNot Nothing Then

            BookingSelectionChanged(dgvBookings, EventArgs.Empty)
        End If
    End Sub

    Private Sub BookingSelectionChanged(sender As Object, e As EventArgs)
        If isLoading OrElse dgvBookings.CurrentRow Is Nothing Then Return

        Try
            Dim row As DataGridViewRow = dgvBookings.CurrentRow

            selectedBookingId =
                Convert.ToInt32(row.Cells("booking_id").Value)

            lblGuest.Text =
                "Guest: " & Convert.ToString(row.Cells("Guest").Value)

            selectedFolioId = 0
            selectedFolioStatus = ""

            If row.Cells("folio_id").Value IsNot Nothing AndAlso
               row.Cells("folio_id").Value IsNot DBNull.Value Then

                selectedFolioId =
                    Convert.ToInt32(row.Cells("folio_id").Value)

                If row.Cells("folio_status").Value IsNot DBNull.Value Then
                    selectedFolioStatus =
                        Convert.ToString(row.Cells("folio_status").Value)
                End If
            End If

            If selectedFolioId <= 0 Then
                ClearFolioData()
                lblFolio.Text = "Folio: Not created"
                lblMessage.Text =
                    "No folio exists for this booking. Payment cannot be recorded."
                Return
            End If

            LoadFolio()

        Catch ex As Exception
            ShowError("Unable to select the booking.", ex)
        End Try
    End Sub

    '========================================================
    ' LOAD FOLIO AND PAYMENT HISTORY
    '========================================================

    Private Sub LoadFolio()
        If selectedFolioId <= 0 Then Return

        Try
            Dim chargeTable As New DataTable()
            Dim paymentTable As New DataTable()
            Dim charges As Decimal = 0D
            Dim paid As Decimal = 0D
            Dim refunds As Decimal = 0D

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Const chargeSql As String =
                    "SELECT charge_date AS `Date`, charge_type AS `Type`, " &
                    "description AS `Description`, quantity AS `Quantity`, " &
                    "unit_price AS `Unit Price`, amount AS `Amount` " &
                    "FROM folio_charges WHERE folio_id = @folio " &
                    "ORDER BY charge_date DESC, charge_id DESC;"

                Using cmd As New MySqlCommand(chargeSql, conn)
                    cmd.Parameters.AddWithValue("@folio", selectedFolioId)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(chargeTable)
                    End Using
                End Using

                Const paymentSql As String =
                    "SELECT payment_id AS `Payment ID`, " &
                    "receipt_number AS `Receipt`, amount AS `Amount`, " &
                    "payment_method AS `Method`, " &
                    "payment_status AS `Status`, " &
                    "payment_date AS `Date`, " &
                    "reference_number AS `Reference` " &
                    "FROM payments WHERE folio_id = @folio " &
                    "ORDER BY payment_date DESC, payment_id DESC;"

                Using cmd As New MySqlCommand(paymentSql, conn)
                    cmd.Parameters.AddWithValue("@folio", selectedFolioId)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(paymentTable)
                    End Using
                End Using

                Const totalsSql As String =
                    "SELECT " &
                    "COALESCE((SELECT SUM(amount) FROM folio_charges " &
                    "WHERE folio_id = @folio), 0) AS charges, " &
                    "COALESCE((SELECT SUM(amount) FROM payments " &
                    "WHERE folio_id = @folio " &
                    "AND payment_status = 'COMPLETED'), 0) AS paid, " &
                    "COALESCE((SELECT SUM(r.refund_amount) " &
                    "FROM refunds r INNER JOIN payments p " &
                    "ON p.payment_id = r.payment_id " &
                    "WHERE p.folio_id = @folio), 0) AS refunds;"

                Using cmd As New MySqlCommand(totalsSql, conn)
                    cmd.Parameters.AddWithValue("@folio", selectedFolioId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            charges = Convert.ToDecimal(reader("charges"))
                            paid = Convert.ToDecimal(reader("paid"))
                            refunds = Convert.ToDecimal(reader("refunds"))
                        End If
                    End Using
                End Using
            End Using

            dgvCharges.DataSource = chargeTable
            dgvPayments.DataSource = paymentTable

            currentBalance = charges - paid + refunds

            lblFolio.Text =
                "Folio #" & selectedFolioId.ToString() &
                " — " & selectedFolioStatus

            lblCharges.Text = "Total charges: " & Peso(charges)
            lblPaid.Text = "Completed payments: " & Peso(paid)
            lblRefunds.Text = "Refunds: " & Peso(refunds)
            lblBalance.Text = "Outstanding balance: " & Peso(currentBalance)

            If currentBalance < -0.01D Then
                lblBalance.Text &= " (Credit)"
            End If

            btnRecordPayment.Enabled =
                selectedFolioStatus = "OPEN" AndAlso
                currentBalance > 0.01D

            lblMessage.Text = ""

        Catch ex As Exception
            ShowError("Unable to load folio details.", ex)
        End Try
    End Sub

    Private Sub ClearSummary()
        lblGuest.Text = "Guest: Select a booking"
        lblFolio.Text = "Folio: None"
        lblCharges.Text = "Total charges: ₱0.00"
        lblPaid.Text = "Completed payments: ₱0.00"
        lblRefunds.Text = "Refunds: ₱0.00"
        lblBalance.Text = "Outstanding balance: ₱0.00"
    End Sub

    Private Sub ClearFolioData()
        selectedFolioId = 0
        selectedFolioStatus = ""
        currentBalance = 0D

        dgvCharges.DataSource = Nothing
        dgvPayments.DataSource = Nothing

        lblCharges.Text = "Total charges: ₱0.00"
        lblPaid.Text = "Completed payments: ₱0.00"
        lblRefunds.Text = "Refunds: ₱0.00"
        lblBalance.Text = "Outstanding balance: ₱0.00"

        btnRecordPayment.Enabled = False
    End Sub

    '========================================================
    ' RECORD CASH PAYMENT
    '========================================================

    Private Sub RecordPayment_Click(sender As Object, e As EventArgs)
        If selectedBookingId <= 0 OrElse selectedFolioId <= 0 Then
            MessageBox.Show("Select a booking with an existing folio.")
            Return
        End If

        If FrmLogin.CurrentUserId <= 0 Then
            MessageBox.Show(
                "Your login session is unavailable. Please log in again.")
            Return
        End If

        If selectedFolioStatus <> "OPEN" Then
            MessageBox.Show("Only an open folio can accept a payment.")
            Return
        End If

        Dim amount As Decimal

        If Not Decimal.TryParse(
            txtAmount.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            amount) OrElse amount <= 0D Then

            MessageBox.Show(
                "Enter a valid payment amount greater than zero.",
                "Invalid Amount",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtAmount.Focus()
            Return
        End If

        If amount > currentBalance + 0.01D Then
            MessageBox.Show(
                "The payment exceeds the outstanding balance of " &
                Peso(currentBalance) & ".",
                "Amount Exceeds Balance",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show(
            "Record cash payment of " & Peso(amount) & "?" &
            Environment.NewLine &
            "Payment method: CASH",
            "Confirm Cash Payment",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        btnRecordPayment.Enabled = False

        Try
            Dim receipt As String =
                "SP-" & DateTime.Now.ToString("yyyyMMddHHmmss") &
                "-" & Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant()

            Using conn As MySqlConnection = DbConnection.GetConnection()
                conn.Open()

                Using tx As MySqlTransaction = conn.BeginTransaction()
                    Try
                        Dim status As String = ""

                        Using cmd As New MySqlCommand(
                            "SELECT folio_status FROM guest_folios " &
                            "WHERE folio_id = @folio " &
                            "AND booking_id = @booking FOR UPDATE;",
                            conn, tx)

                            cmd.Parameters.AddWithValue("@folio", selectedFolioId)
                            cmd.Parameters.AddWithValue("@booking", selectedBookingId)

                            Dim result As Object = cmd.ExecuteScalar()

                            If result Is Nothing OrElse result Is DBNull.Value Then
                                Throw New Exception("The selected folio was not found.")
                            End If

                            status = result.ToString()
                        End Using

                        If status <> "OPEN" Then
                            Throw New Exception(
                                "This folio is closed and cannot accept payments.")
                        End If

                        Dim charges As Decimal = 0D
                        Dim paid As Decimal = 0D
                        Dim refunds As Decimal = 0D

                        Const totalsSql As String =
                            "SELECT " &
                            "COALESCE((SELECT SUM(amount) FROM folio_charges " &
                            "WHERE folio_id = @folio), 0) AS charges, " &
                            "COALESCE((SELECT SUM(amount) FROM payments " &
                            "WHERE folio_id = @folio " &
                            "AND payment_status = 'COMPLETED'), 0) AS paid, " &
                            "COALESCE((SELECT SUM(r.refund_amount) " &
                            "FROM refunds r INNER JOIN payments p " &
                            "ON p.payment_id = r.payment_id " &
                            "WHERE p.folio_id = @folio), 0) AS refunds;"

                        Using cmd As New MySqlCommand(totalsSql, conn, tx)
                            cmd.Parameters.AddWithValue("@folio", selectedFolioId)

                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                If reader.Read() Then
                                    charges = Convert.ToDecimal(reader("charges"))
                                    paid = Convert.ToDecimal(reader("paid"))
                                    refunds = Convert.ToDecimal(reader("refunds"))
                                End If
                            End Using
                        End Using

                        Dim balance As Decimal = charges - paid + refunds

                        If balance <= 0.01D Then
                            Throw New Exception(
                                "This folio has no outstanding balance.")
                        End If

                        If amount > balance + 0.01D Then
                            Throw New Exception(
                                "The balance changed. The current outstanding " &
                                "balance is " & Peso(balance) & ".")
                        End If

                        Const insertSql As String =
                            "INSERT INTO payments " &
                            "(folio_id, receipt_number, amount, payment_method, " &
                            "payment_status, reference_number, payment_date, " &
                            "received_by, notes) " &
                            "VALUES (@folio, @receipt, @amount, 'CASH', " &
                            "'COMPLETED', @reference, NOW(), @user, @notes);"

                        Dim paymentId As Integer

                        Using cmd As New MySqlCommand(insertSql, conn, tx)
                            cmd.Parameters.AddWithValue("@folio", selectedFolioId)
                            cmd.Parameters.AddWithValue("@receipt", receipt)
                            cmd.Parameters.AddWithValue("@amount", amount)

                            If String.IsNullOrWhiteSpace(txtReference.Text) Then
                                cmd.Parameters.AddWithValue("@reference", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue(
                                    "@reference", txtReference.Text.Trim())
                            End If

                            cmd.Parameters.AddWithValue(
                                "@user", FrmLogin.CurrentUserId)

                            If String.IsNullOrWhiteSpace(txtNotes.Text) Then
                                cmd.Parameters.AddWithValue("@notes", DBNull.Value)
                            Else
                                cmd.Parameters.AddWithValue(
                                    "@notes", txtNotes.Text.Trim())
                            End If

                            cmd.ExecuteNonQuery()
                            paymentId = Convert.ToInt32(cmd.LastInsertedId)
                        End Using

                        Const auditSql As String =
                            "INSERT INTO audit_logs " &
                            "(user_id, action, table_name, record_id, details) " &
                            "VALUES (@user, 'RECORD_PAYMENT', 'payments', " &
                            "@record, @details);"

                        Using cmd As New MySqlCommand(auditSql, conn, tx)
                            cmd.Parameters.AddWithValue(
                                "@user", FrmLogin.CurrentUserId)
                            cmd.Parameters.AddWithValue("@record", paymentId)
                            cmd.Parameters.AddWithValue(
                                "@details",
                                "Cash receipt " & receipt &
                                "; folio #" & selectedFolioId &
                                "; amount " & amount.ToString("0.00") &
                                "; method CASH")

                            cmd.ExecuteNonQuery()
                        End Using

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
                "Cash payment recorded successfully." &
                Environment.NewLine &
                "Receipt: " & receipt &
                Environment.NewLine &
                "Amount: " & Peso(amount),
                "Payment Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            txtAmount.Clear()
            txtReference.Clear()
            txtNotes.Clear()

            LoadFolio()

        Catch ex As Exception
            ShowError("Unable to record cash payment.", ex)
        Finally
            btnRecordPayment.Enabled =
                selectedFolioId > 0 AndAlso
                selectedFolioStatus = "OPEN" AndAlso
                currentBalance > 0.01D
        End Try
    End Sub

    '========================================================
    ' HELPERS
    '========================================================

    Private Function Peso(amount As Decimal) As String
        Return "₱" & amount.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    Private Sub ShowError(message As String, ex As Exception)
        MessageBox.Show(
            message & Environment.NewLine & ex.Message,
            "StayPoint HMS",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

End Class
