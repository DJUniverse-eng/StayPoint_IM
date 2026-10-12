Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports System.Globalization

Public Class ucPayments

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

    Private ReadOnly PesoCulture As CultureInfo =
        CultureInfo.GetCultureInfo("en-PH")

    '========================================================
    ' CONTROLS
    '========================================================

    Private pnlHeader As Panel
    Private pnlSummary As FlowLayoutPanel
    Private pnlPayments As Panel
    Private pnlTableHeader As Panel
    Private pnlFilters As Panel

    Private lblTotalPayments As Label
    Private lblCompleted As Label
    Private lblVoided As Label
    Private lblTotalCollected As Label

    Private lblRecordCount As Label
    Private lblMessage As Label

    Private txtSearch As TextBox
    Private cboStatus As ComboBox

    Private btnRefresh As Button
    Private dgvPayments As DataGridView

    Private sourceTable As DataTable
    Private displayTable As DataTable

    Private isLoading As Boolean = False

    '========================================================
    ' INITIALIZE
    '========================================================

    Public Sub New()

        InitializeComponent()
        BuildInterface()

    End Sub

    Private Sub ucPayments_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        LoadPayments()

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
            .Text = "Payments",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(24, 12)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Manage cash payments, receipts, and payment statuses.",
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

        lblTotalPayments = New Label()
        lblCompleted = New Label()
        lblVoided = New Label()
        lblTotalCollected = New Label()

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Total Payments",
                lblTotalPayments,
                Navy
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Completed",
                lblCompleted,
                Teal
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Voided",
                lblVoided,
                Color.FromArgb(220, 38, 38)
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Total Collected",
                lblTotalCollected,
                Color.FromArgb(37, 99, 235)
            )
        )

        '------------------------------
        ' PAYMENTS CONTAINER
        '------------------------------

        pnlPayments = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        '------------------------------
        ' TABLE HEADER
        '------------------------------

        pnlTableHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 48,
            .BackColor = Color.White
        }

        Dim lblTableTitle As New Label With {
            .Text = "Cash Payment Records",
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

        AddHandler pnlTableHeader.Resize,
            AddressOf PositionRecordCount

        pnlTableHeader.Controls.Add(lblTableTitle)
        pnlTableHeader.Controls.Add(lblRecordCount)

        '------------------------------
        ' SEARCH AND STATUS FILTER
        '------------------------------

        pnlFilters = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .BackColor = Color.White
        }

        Dim lblSearch As New Label With {
            .Text = "Search by Payment ID, Receipt Number, or Folio ID",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(2, 2)
        }

        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(2, 27),
            .Size = New Size(300, 32)
        }

        Dim lblStatus As New Label With {
            .Text = "Payment Status",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(315, 2)
        }

        cboStatus = New ComboBox With {
            .Font = New Font("Segoe UI", 10),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(315, 27),
            .Size = New Size(150, 32)
        }

        cboStatus.Items.AddRange(
            New Object() {
                "All Statuses",
                "COMPLETED",
                "VOIDED"
            }
        )

        cboStatus.SelectedIndex = 0

        btnRefresh = CreateButton(
            "Refresh",
            Navy,
            New Size(95, 32)
        )

        btnRefresh.Location = New Point(478, 27)

        AddHandler txtSearch.TextChanged,
            AddressOf ApplyFilters

        AddHandler cboStatus.SelectedIndexChanged,
            AddressOf ApplyFilters

        AddHandler btnRefresh.Click,
            AddressOf RefreshPayments

        pnlFilters.Controls.Add(lblSearch)
        pnlFilters.Controls.Add(txtSearch)
        pnlFilters.Controls.Add(lblStatus)
        pnlFilters.Controls.Add(cboStatus)
        pnlFilters.Controls.Add(btnRefresh)

        '------------------------------
        ' DATA GRID
        '------------------------------

        dgvPayments = New DataGridView With {
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

        dgvPayments.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvPayments.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White

        dgvPayments.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvPayments.ColumnHeadersHeight = 42

        dgvPayments.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        dgvPayments.DefaultCellStyle.Font =
            New Font("Segoe UI", 9)

        dgvPayments.DefaultCellStyle.ForeColor =
            Color.FromArgb(51, 65, 85)

        dgvPayments.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)

        dgvPayments.DefaultCellStyle.SelectionForeColor = Navy

        dgvPayments.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        dgvPayments.RowTemplate.Height = 38

        '------------------------------
        ' MESSAGE
        '------------------------------

        lblMessage = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 32,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Visible = False
        }

        '------------------------------
        ' ASSEMBLE
        '------------------------------

        pnlPayments.Controls.Add(dgvPayments)
        pnlPayments.Controls.Add(lblMessage)
        pnlPayments.Controls.Add(pnlFilters)
        pnlPayments.Controls.Add(pnlTableHeader)

        Me.Controls.Add(pnlPayments)
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

    '========================================================
    ' BUTTON
    '========================================================

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

    '========================================================
    ' RECORD COUNT POSITION
    '========================================================

    Private Sub PositionRecordCount(
        sender As Object,
        e As EventArgs
    )

        If pnlTableHeader Is Nothing OrElse
           lblRecordCount Is Nothing Then Return

        lblRecordCount.Location = New Point(
            Math.Max(
                4,
                pnlTableHeader.ClientSize.Width -
                lblRecordCount.Width - 4
            ),
            15
        )

    End Sub

    '========================================================
    ' LOAD PAYMENTS
    '========================================================

    Private Sub LoadPayments()

        If isLoading Then Return

        isLoading = True

        Try

            lblMessage.Visible = False

            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Dim sql As String =
                    "SELECT payment_id, folio_id, receipt_number, " &
                    "amount, payment_status, payment_date, received_by " &
                    "FROM payments " &
                    "ORDER BY payment_date DESC, payment_id DESC"

                Using adapter As New MySqlDataAdapter(sql, conn)

                    sourceTable = New DataTable()
                    adapter.Fill(sourceTable)

                End Using

            End Using

            BuildDisplayTable()
            UpdateSummary()
            ApplyFilters(Nothing, EventArgs.Empty)

        Catch ex As Exception

            sourceTable = New DataTable()

            BuildDisplayTable()
            UpdateSummary()

            dgvPayments.DataSource = displayTable

            ShowMessage(
                "Unable to load payments. Verify the payments table " &
                "and database connection. Details: " & ex.Message,
                Color.FromArgb(185, 28, 28)
            )

        Finally

            isLoading = False

        End Try

    End Sub

    '========================================================
    ' DISPLAY TABLE
    '========================================================

    Private Sub BuildDisplayTable()

        displayTable = New DataTable()

        displayTable.Columns.Add("Payment ID")
        displayTable.Columns.Add("Receipt Number")
        displayTable.Columns.Add("Folio ID")

        displayTable.Columns.Add(
            "Amount",
            GetType(Decimal)
        )

        displayTable.Columns.Add("Payment Date")
        displayTable.Columns.Add("Received By")
        displayTable.Columns.Add("Status")

        If sourceTable Is Nothing Then Return

        For Each sourceRow As DataRow In sourceTable.Rows

            Dim newRow As DataRow = displayTable.NewRow()

            newRow("Payment ID") =
                GetValue(sourceRow, "payment_id")

            newRow("Receipt Number") =
                GetValue(sourceRow, "receipt_number")

            newRow("Folio ID") =
                GetValue(sourceRow, "folio_id")

            Dim amountValue As Decimal = 0D

            Decimal.TryParse(
                GetValue(sourceRow, "amount"),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                amountValue
            )

            newRow("Amount") = amountValue

            newRow("Payment Date") =
                FormatDateValue(
                    GetValue(sourceRow, "payment_date")
                )

            newRow("Received By") =
                GetValue(sourceRow, "received_by")

            newRow("Status") =
                GetValue(sourceRow, "payment_status").
                Trim().ToUpperInvariant()

            displayTable.Rows.Add(newRow)

        Next

    End Sub

    '========================================================
    ' SAFE VALUE RETRIEVAL
    '========================================================

    Private Function GetValue(
        row As DataRow,
        columnName As String
    ) As String

        If row Is Nothing OrElse
           row.Table Is Nothing OrElse
           Not row.Table.Columns.Contains(columnName) OrElse
           row.IsNull(columnName) Then

            Return ""

        End If

        Return Convert.ToString(
            row(columnName),
            CultureInfo.InvariantCulture
        )

    End Function

    '========================================================
    ' FORMAT DATE AND TIME
    '========================================================

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
                CultureInfo.InvariantCulture
            )

        End If

        Return value

    End Function

    '========================================================
    ' SUMMARY COUNTS
    '========================================================

    Private Sub UpdateSummary()

        Dim total As Integer = 0
        Dim completed As Integer = 0
        Dim voided As Integer = 0
        Dim collected As Decimal = 0D

        If sourceTable IsNot Nothing Then

            total = sourceTable.Rows.Count

            For Each row As DataRow In sourceTable.Rows

                Dim status As String =
                    GetValue(row, "payment_status").
                    Trim().ToUpperInvariant()

                Dim amount As Decimal = 0D

                Decimal.TryParse(
                    GetValue(row, "amount"),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    amount
                )

                Select Case status

                    Case "COMPLETED"

                        completed += 1
                        collected += amount

                    Case "VOIDED"

                        voided += 1

                End Select

            Next

        End If

        lblTotalPayments.Text = total.ToString()
        lblCompleted.Text = completed.ToString()
        lblVoided.Text = voided.ToString()

        lblTotalCollected.Text =
            "₱ " & collected.ToString("N2", PesoCulture)

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
                "Convert([Payment ID], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Receipt Number], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Folio ID], 'System.String') LIKE '%" &
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

            dgvPayments.DataSource = view

            If dgvPayments.Columns.Contains("Amount") Then

                With dgvPayments.Columns("Amount").DefaultCellStyle

                    .Format = """₱ ""#,##0.00"
                    .FormatProvider = PesoCulture
                    .Alignment =
                        DataGridViewContentAlignment.MiddleRight

                End With

            End If

            If dgvPayments.Columns.Contains("Payment ID") Then
                dgvPayments.Columns("Payment ID").FillWeight = 65
            End If

            If dgvPayments.Columns.Contains("Receipt Number") Then
                dgvPayments.Columns("Receipt Number").FillWeight = 115
            End If

            If dgvPayments.Columns.Contains("Folio ID") Then
                dgvPayments.Columns("Folio ID").FillWeight = 65
            End If

            If dgvPayments.Columns.Contains("Amount") Then
                dgvPayments.Columns("Amount").FillWeight = 95
            End If

            If dgvPayments.Columns.Contains("Payment Date") Then
                dgvPayments.Columns("Payment Date").FillWeight = 145
            End If

            If dgvPayments.Columns.Contains("Received By") Then
                dgvPayments.Columns("Received By").FillWeight = 100
            End If

            If dgvPayments.Columns.Contains("Status") Then
                dgvPayments.Columns("Status").FillWeight = 80
            End If

            lblRecordCount.Text =
                view.Count.ToString() &
                If(view.Count = 1, " record", " records")

            PositionRecordCount(Nothing, EventArgs.Empty)

        Catch ex As Exception

            ShowMessage(
                "Unable to filter payment records.",
                Color.FromArgb(185, 28, 28)
            )

        End Try

    End Sub

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub RefreshPayments(
        sender As Object,
        e As EventArgs
    )

        LoadPayments()

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