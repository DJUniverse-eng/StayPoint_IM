
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucGuests

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
    Private pnlGuests As Panel
    Private pnlGuestsHeader As Panel
    Private pnlFilters As Panel
    Private pnlActions As Panel

    Private lblTotalGuests As Label
    Private lblWithPhone As Label
    Private lblWithEmail As Label
    Private lblWithAddress As Label

    Private lblRecordCount As Label
    Private lblMessage As Label

    Private txtSearch As TextBox

    Private btnRefresh As Button
    Private btnViewDetails As Button

    Private dgvGuests As DataGridView

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

    Private Sub ucGuests_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        LoadGuests()

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
            .Text = "Guests",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(24, 12)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Manage guest profiles and contact information.",
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

        lblTotalGuests = New Label()
        lblWithPhone = New Label()
        lblWithEmail = New Label()
        lblWithAddress = New Label()

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "Total Guests",
                lblTotalGuests,
                Navy
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "With Phone Number",
                lblWithPhone,
                Teal
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "With Email",
                lblWithEmail,
                Color.FromArgb(37, 99, 235)
            )
        )

        pnlSummary.Controls.Add(
            CreateSummaryCard(
                "With Address",
                lblWithAddress,
                Color.FromArgb(217, 119, 6)
            )
        )

        '------------------------------
        ' GUEST DIRECTORY CONTAINER
        '------------------------------

        pnlGuests = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        '------------------------------
        ' TABLE HEADER
        '------------------------------

        pnlGuestsHeader = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 48,
            .BackColor = Color.White
        }

        Dim lblDirectoryTitle As New Label With {
            .Text = "Guest Directory",
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

        AddHandler pnlGuestsHeader.Resize,
            AddressOf PositionRecordCount

        pnlGuestsHeader.Controls.Add(lblDirectoryTitle)
        pnlGuestsHeader.Controls.Add(lblRecordCount)

        '------------------------------
        ' SEARCH
        '------------------------------

        pnlFilters = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 86,
            .BackColor = Color.White
        }

        Dim lblSearch As New Label With {
            .Text = "Search by Guest ID, Name, Email, or Phone",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(2, 2)
        }

        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle,
            .Location = New Point(2, 27),
            .Size = New Size(340, 32)
        }

        btnRefresh = CreateButton(
            "Refresh",
            Navy,
            New Size(95, 32)
        )

        btnRefresh.Location = New Point(355, 27)

        AddHandler txtSearch.TextChanged,
            AddressOf ApplySearch

        AddHandler btnRefresh.Click,
            AddressOf RefreshGuests

        pnlFilters.Controls.Add(lblSearch)
        pnlFilters.Controls.Add(txtSearch)
        pnlFilters.Controls.Add(btnRefresh)

        '------------------------------
        ' GUEST TABLE
        '------------------------------

        dgvGuests = New DataGridView With {
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

        dgvGuests.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvGuests.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White

        dgvGuests.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)

        dgvGuests.ColumnHeadersHeight = 42

        dgvGuests.DefaultCellStyle.Font =
            New Font("Segoe UI", 9)

        dgvGuests.DefaultCellStyle.ForeColor =
            Color.FromArgb(51, 65, 85)

        dgvGuests.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(219, 234, 254)

        dgvGuests.DefaultCellStyle.SelectionForeColor = Navy

        dgvGuests.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        dgvGuests.RowTemplate.Height = 38

        AddHandler dgvGuests.SelectionChanged,
            AddressOf UpdateActionButtons

        AddHandler dgvGuests.CellDoubleClick,
            AddressOf ViewGuestDetails

        '------------------------------
        ' ACTIONS
        '------------------------------

        pnlActions = New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 58,
            .BackColor = Color.White
        }

        btnViewDetails = CreateButton(
            "View Guest Details",
            Teal,
            New Size(175, 36)
        )

        btnViewDetails.Location = New Point(0, 10)
        btnViewDetails.Enabled = False

        AddHandler btnViewDetails.Click,
            AddressOf ViewGuestDetails

        pnlActions.Controls.Add(btnViewDetails)

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

        pnlGuests.Controls.Add(dgvGuests)
        pnlGuests.Controls.Add(lblMessage)
        pnlGuests.Controls.Add(pnlActions)
        pnlGuests.Controls.Add(pnlFilters)
        pnlGuests.Controls.Add(pnlGuestsHeader)

        Me.Controls.Add(pnlGuests)
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

        If pnlGuestsHeader Is Nothing OrElse
           lblRecordCount Is Nothing Then Return

        lblRecordCount.Location = New Point(
            Math.Max(
                4,
                pnlGuestsHeader.ClientSize.Width -
                lblRecordCount.Width - 4
            ),
            15
        )

    End Sub

    '========================================================
    ' LOAD GUESTS FROM MYSQL
    '========================================================

    Private Sub LoadGuests()

        If isLoading Then Return

        isLoading = True

        Try

            lblMessage.Visible = False

            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                Using adapter As New MySqlDataAdapter(
                    "SELECT * FROM guests",
                    conn
                )

                    sourceTable = New DataTable()
                    adapter.Fill(sourceTable)

                End Using

            End Using

            BuildDisplayTable()
            UpdateSummary()
            ApplySearch(Nothing, EventArgs.Empty)

        Catch ex As Exception

            If sourceTable Is Nothing Then
                sourceTable = New DataTable()
            End If

            BuildDisplayTable()
            UpdateSummary()

            dgvGuests.DataSource = displayTable

            ShowMessage(
                "Unable to load guests. Verify the database " &
                "connection and confirm that the guests table exists.",
                Color.FromArgb(185, 28, 28)
            )

        Finally

            isLoading = False
            UpdateActionButtons(Nothing, EventArgs.Empty)

        End Try

    End Sub

    '========================================================
    ' MAP DATABASE COLUMNS TO DISPLAY COLUMNS
    '========================================================

    Private Sub BuildDisplayTable()

        displayTable = New DataTable()

        displayTable.Columns.Add("Guest ID")
        displayTable.Columns.Add("Full Name")
        displayTable.Columns.Add("Phone")
        displayTable.Columns.Add("Email")
        displayTable.Columns.Add("Address")

        If sourceTable Is Nothing Then Return

        For Each sourceRow As DataRow In sourceTable.Rows

            Dim newRow As DataRow = displayTable.NewRow()

            newRow("Guest ID") =
                GetSourceValue(
                    sourceRow,
                    "guest_id",
                    "guestid",
                    "customer_id",
                    "customerid",
                    "id"
                )

            newRow("Full Name") =
                GetSourceValue(
                    sourceRow,
                    "full_name",
                    "fullname",
                    "guest_name",
                    "guestname",
                    "customer_name",
                    "customername",
                    "name"
                )

            newRow("Phone") =
                GetSourceValue(
                    sourceRow,
                    "phone",
                    "phone_number",
                    "phonenumber",
                    "contact_number",
                    "contactnumber",
                    "mobile",
                    "mobile_number"
                )

            newRow("Email") =
                GetSourceValue(
                    sourceRow,
                    "email",
                    "email_address",
                    "emailaddress"
                )

            newRow("Address") =
                GetSourceValue(
                    sourceRow,
                    "address",
                    "home_address",
                    "homeaddress",
                    "residential_address"
                )

            displayTable.Rows.Add(newRow)

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

    Private Function NormalizeName(value As String) As String

        Return value.Replace("_", "").
                     Replace(" ", "").
                     Replace("-", "").
                     ToLowerInvariant()

    End Function

    '========================================================
    ' UPDATE SUMMARY COUNTS
    '========================================================

    Private Sub UpdateSummary()

        Dim total As Integer = 0
        Dim withPhone As Integer = 0
        Dim withEmail As Integer = 0
        Dim withAddress As Integer = 0

        If sourceTable IsNot Nothing Then

            total = sourceTable.Rows.Count

            For Each row As DataRow In sourceTable.Rows

                Dim phone As String =
                    GetSourceValue(
                        row,
                        "phone",
                        "phone_number",
                        "phonenumber",
                        "contact_number",
                        "contactnumber",
                        "mobile",
                        "mobile_number"
                    )

                Dim email As String =
                    GetSourceValue(
                        row,
                        "email",
                        "email_address",
                        "emailaddress"
                    )

                Dim address As String =
                    GetSourceValue(
                        row,
                        "address",
                        "home_address",
                        "homeaddress",
                        "residential_address"
                    )

                If Not String.IsNullOrWhiteSpace(phone) Then
                    withPhone += 1
                End If

                If Not String.IsNullOrWhiteSpace(email) Then
                    withEmail += 1
                End If

                If Not String.IsNullOrWhiteSpace(address) Then
                    withAddress += 1
                End If

            Next

        End If

        lblTotalGuests.Text = total.ToString()
        lblWithPhone.Text = withPhone.ToString()
        lblWithEmail.Text = withEmail.ToString()
        lblWithAddress.Text = withAddress.ToString()

    End Sub

    '========================================================
    ' SEARCH GUESTS
    '========================================================

    Private Sub ApplySearch(
        sender As Object,
        e As EventArgs
    )

        If displayTable Is Nothing Then Return

        Dim view As New DataView(displayTable)
        Dim searchText As String = txtSearch.Text.Trim()

        If searchText <> "" Then

            Dim escapedSearch As String =
                searchText.Replace("'", "''").
                Replace("[", "[[]").
                Replace("%", "[%]").
                Replace("*", "[*]")

            Dim searchFilter As String =
                "Convert([Guest ID], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Full Name], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Phone], 'System.String') LIKE '%" &
                escapedSearch & "%' OR " &
                "Convert([Email], 'System.String') LIKE '%" &
                escapedSearch & "%'"

            Try

                view.RowFilter = "(" & searchFilter & ")"

            Catch ex As Exception

                ShowMessage(
                    "Unable to filter the guest list.",
                    Color.FromArgb(185, 28, 28)
                )

                Return

            End Try

        End If

        dgvGuests.DataSource = view

        lblRecordCount.Text =
            view.Count.ToString() &
            If(view.Count = 1, " record", " records")

        PositionRecordCount(Nothing, EventArgs.Empty)

        UpdateActionButtons(Nothing, EventArgs.Empty)

    End Sub

    '========================================================
    ' SELECTED GUEST
    '========================================================

    Private Sub UpdateActionButtons(
        sender As Object,
        e As EventArgs
    )

        If btnViewDetails Is Nothing Then Return

        btnViewDetails.Enabled =
            dgvGuests IsNot Nothing AndAlso
            dgvGuests.CurrentRow IsNot Nothing AndAlso
            Not dgvGuests.CurrentRow.IsNewRow

    End Sub

    '========================================================
    ' VIEW GUEST DETAILS
    '========================================================

    Private Sub ViewGuestDetails(
        sender As Object,
        e As EventArgs
    )

        If dgvGuests Is Nothing OrElse
           dgvGuests.CurrentRow Is Nothing Then

            MessageBox.Show(
                "Please select a guest first.",
                "Guest Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            Return

        End If

        Dim selectedRow As DataGridViewRow =
            dgvGuests.CurrentRow

        Dim guestID As String =
            Convert.ToString(
                selectedRow.Cells("Guest ID").Value
            )

        Dim guestName As String =
            Convert.ToString(
                selectedRow.Cells("Full Name").Value
            )

        Dim phone As String =
            Convert.ToString(
                selectedRow.Cells("Phone").Value
            )

        Dim email As String =
            Convert.ToString(
                selectedRow.Cells("Email").Value
            )

        Dim address As String =
            Convert.ToString(
                selectedRow.Cells("Address").Value
            )

        Dim details As String =
            "Guest ID: " & guestID &
            Environment.NewLine &
            "Full Name: " & guestName &
            Environment.NewLine &
            "Phone: " & phone &
            Environment.NewLine &
            "Email: " & email &
            Environment.NewLine &
            "Address: " & address

        MessageBox.Show(
            details,
            "Guest Details",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

    End Sub

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub RefreshGuests(
        sender As Object,
        e As EventArgs
    )

        LoadGuests()

    End Sub

    '========================================================
    ' DISPLAY MESSAGE
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
