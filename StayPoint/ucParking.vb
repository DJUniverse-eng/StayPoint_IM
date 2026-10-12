
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucParking
    Inherits UserControl

    '========================================================
    ' COLORS
    '========================================================

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly PageBackground As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly White As Color = Color.White
    Private ReadOnly TextColor As Color = Color.FromArgb(51, 65, 85)
    Private ReadOnly BorderColor As Color = Color.FromArgb(220, 226, 234)

    '========================================================
    ' CONTROLS
    '========================================================

    Private mainPanel As Panel
    Private headerPanel As Panel
    Private cardsPanel As FlowLayoutPanel
    Private searchBox As TextBox
    Private cmbStatus As ComboBox
    Private dgvParking As DataGridView

    Private lblTotal As Label
    Private lblAvailable As Label
    Private lblOccupied As Label
    Private lblMaintenance As Label

    Private btnAdd As Button
    Private btnEdit As Button
    Private btnDelete As Button
    Private btnRefresh As Button

    Private ReadOnly MoneyCulture As CultureInfo =
        CultureInfo.GetCultureInfo("en-PH")

    '========================================================
    ' INITIALIZATION
    '========================================================

    Public Sub New()
        MyBase.New()

        Me.Dock = DockStyle.Fill
        Me.BackColor = PageBackground
        Me.Font = New Font("Segoe UI", 9.0F)
        Me.AutoScroll = True

        BuildInterface()
        LoadParkingSpaces()
        InitializeComponent()
    End Sub



    '========================================================
    ' BUILD INTERFACE
    '========================================================

    Private Sub BuildInterface()

        mainPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = PageBackground,
            .Padding = New Padding(20),
            .AutoScroll = True
        }

        Me.Controls.Add(mainPanel)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 1,
            .RowCount = 5,
            .BackColor = PageBackground
        }

        root.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100))

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 115))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 360))

        mainPanel.Controls.Add(root)

        '----------------------------------------------------
        ' HEADER
        '----------------------------------------------------

        headerPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = PageBackground
        }

        Dim lblTitle As New Label With {
            .Text = "Parking Management",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 8)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Manage parking spaces and view configured parking prices.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = Color.Gray,
            .AutoSize = True,
            .Location = New Point(2, 42)
        }

        headerPanel.Controls.Add(lblTitle)
        headerPanel.Controls.Add(lblSubtitle)
        root.Controls.Add(headerPanel, 0, 0)

        '----------------------------------------------------
        ' SUMMARY CARDS
        '----------------------------------------------------

        cardsPanel = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .BackColor = PageBackground,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(0, 5, 0, 5)
        }

        lblTotal = CreateSummaryCard(
            "Total Spaces", Navy, cardsPanel)

        lblAvailable = CreateSummaryCard(
            "Available", Teal, cardsPanel)

        lblOccupied = CreateSummaryCard(
            "Occupied", Color.FromArgb(220, 130, 30), cardsPanel)

        lblMaintenance = CreateSummaryCard(
            "Maintenance", Color.FromArgb(190, 60, 60), cardsPanel)

        root.Controls.Add(cardsPanel, 0, 1)

        '----------------------------------------------------
        ' SEARCH AND FILTER
        '----------------------------------------------------

        Dim filterPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = PageBackground
        }

        searchBox = New TextBox With {
            .Location = New Point(0, 13),
            .Size = New Size(260, 30),
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle
        }

        searchBox.Text = "Search parking spaces..."
        searchBox.ForeColor = Color.Gray

        AddHandler searchBox.Enter,
            Sub()
                If searchBox.Text = "Search parking spaces..." Then
                    searchBox.Clear()
                    searchBox.ForeColor = TextColor
                End If
            End Sub

        AddHandler searchBox.Leave,
            Sub()
                If String.IsNullOrWhiteSpace(searchBox.Text) Then
                    searchBox.Text = "Search parking spaces..."
                    searchBox.ForeColor = Color.Gray
                End If
            End Sub

        AddHandler searchBox.TextChanged,
            AddressOf ApplyFilters

        cmbStatus = New ComboBox With {
            .Location = New Point(275, 12),
            .Size = New Size(165, 31),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cmbStatus.Items.AddRange(
            New Object() {
                "All Statuses",
                "AVAILABLE",
                "OCCUPIED",
                "MAINTENANCE",
                "INACTIVE"
            })

        cmbStatus.SelectedIndex = 0

        AddHandler cmbStatus.SelectedIndexChanged,
            AddressOf ApplyFilters

        filterPanel.Controls.Add(searchBox)
        filterPanel.Controls.Add(cmbStatus)

        root.Controls.Add(filterPanel, 0, 2)

        '----------------------------------------------------
        ' ACTION BUTTONS
        '----------------------------------------------------

        Dim actionPanel As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .BackColor = PageBackground,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False
        }

        btnAdd = CreateButton("+ Add Space", Teal)
        btnEdit = CreateButton("Edit Space", Navy)
        btnDelete = CreateButton("Delete Space", Color.FromArgb(190, 60, 60))
        btnRefresh = CreateButton("Refresh", SidebarColor)

        AddHandler btnAdd.Click, AddressOf AddSpace
        AddHandler btnEdit.Click, AddressOf EditSpace
        AddHandler btnDelete.Click, AddressOf DeleteSpace
        AddHandler btnRefresh.Click, AddressOf RefreshData

        actionPanel.Controls.Add(btnAdd)
        actionPanel.Controls.Add(btnEdit)
        actionPanel.Controls.Add(btnDelete)
        actionPanel.Controls.Add(btnRefresh)

        root.Controls.Add(actionPanel, 0, 3)

        '----------------------------------------------------
        ' DATA GRID
        '----------------------------------------------------

        dgvParking = New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = White,
            .BorderStyle = BorderStyle.None,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False,
            .GridColor = BorderColor,
            .Font = New Font("Segoe UI", 9),
            .AutoGenerateColumns = True
        }

        dgvParking.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvParking.ColumnHeadersDefaultCellStyle.ForeColor = White
        dgvParking.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)
        dgvParking.ColumnHeadersDefaultCellStyle.Padding =
            New Padding(8, 0, 0, 0)
        dgvParking.ColumnHeadersHeight = 44

        dgvParking.DefaultCellStyle.BackColor = White
        dgvParking.DefaultCellStyle.ForeColor = TextColor
        dgvParking.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(220, 239, 239)
        dgvParking.DefaultCellStyle.SelectionForeColor = Navy
        dgvParking.DefaultCellStyle.Padding = New Padding(6, 4, 6, 4)

        dgvParking.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)

        dgvParking.RowTemplate.Height = 38

        AddHandler dgvParking.CellDoubleClick,
            AddressOf ParkingRowDoubleClick

        root.Controls.Add(dgvParking, 0, 4)

    End Sub

    '========================================================
    ' SUMMARY CARD
    '========================================================

    Private Function CreateSummaryCard(
        title As String,
        accent As Color,
        container As FlowLayoutPanel
    ) As Label

        Dim card As New Panel With {
            .Size = New Size(210, 90),
            .BackColor = White,
            .Margin = New Padding(0, 0, 12, 0),
            .Padding = New Padding(12)
        }

        card.BorderStyle = BorderStyle.FixedSingle

        Dim lblHeading As New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = Color.Gray,
            .Location = New Point(12, 10),
            .AutoSize = True
        }

        Dim lblValue As New Label With {
            .Text = "0",
            .Font = New Font("Segoe UI", 21, FontStyle.Bold),
            .ForeColor = accent,
            .Location = New Point(12, 32),
            .AutoSize = True
        }

        card.Controls.Add(lblHeading)
        card.Controls.Add(lblValue)
        container.Controls.Add(card)

        Return lblValue

    End Function

    Private Function CreateButton(
        caption As String,
        buttonColor As Color
    ) As Button

        Return New Button With {
            .Text = caption,
            .Size = New Size(125, 34),
            .BackColor = buttonColor,
            .ForeColor = White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Margin = New Padding(0, 0, 8, 0)
        }

    End Function

    '========================================================
    ' LOAD PARKING SPACES AND SYSTEM SETTINGS PRICING
    '========================================================

    Private Sub LoadParkingSpaces()

        Try
            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                conn.Open()

                ' Pricing comes from System Settings.
                ' GROUP_CONCAT prevents a space from appearing
                ' multiple times when multiple pricing options exist.

                Dim sql As String =
                    "SELECT " &
                    "ps.parking_space_id, " &
                    "ps.space_code AS `Space Code`, " &
                    "ps.location_description AS `Location`, " &
                    "ps.status AS `Status`, " &
                    "COALESCE((" &
                    " SELECT GROUP_CONCAT(" &
                    " CONCAT(" &
                    " pro.vehicle_type, ' / ', " &
                    " pro.parking_type, ' - ', " &
                    " '₱', FORMAT(pro.hourly_rate, 2)" &
                    " ) ORDER BY pro.vehicle_type, pro.parking_type " &
                    " SEPARATOR ' | ')" &
                    " FROM parking_rate_options pro " &
                    " WHERE pro.is_active = 1" &
                    "), 'No active pricing') AS `Configured Pricing` " &
                    "FROM parking_spaces ps " &
                    "ORDER BY ps.space_code"

                Using cmd As New MySqlCommand(sql, conn)
                    Using adapter As New MySqlDataAdapter(cmd)

                        Dim dt As New DataTable()
                        adapter.Fill(dt)

                        dgvParking.DataSource = dt

                    End Using
                End Using

            End Using

            If dgvParking.Columns.Contains("parking_space_id") Then
                dgvParking.Columns("parking_space_id").Visible = False
            End If

            If dgvParking.Columns.Contains("Space Code") Then
                dgvParking.Columns("Space Code").FillWeight = 65
            End If

            If dgvParking.Columns.Contains("Location") Then
                dgvParking.Columns("Location").FillWeight = 85
            End If

            If dgvParking.Columns.Contains("Status") Then
                dgvParking.Columns("Status").FillWeight = 65
            End If

            If dgvParking.Columns.Contains("Configured Pricing") Then
                dgvParking.Columns("Configured Pricing").FillWeight = 180
            End If

            UpdateSummaryCards()
            ApplyFilters(Nothing, EventArgs.Empty)

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load parking spaces." &
                Environment.NewLine & ex.Message,
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try

    End Sub

    '========================================================
    ' SUMMARY COUNTS
    '========================================================

    Private Sub UpdateSummaryCards()

        If dgvParking.DataSource Is Nothing Then Return

        Dim dt As DataTable =
            TryCast(dgvParking.DataSource, DataTable)

        If dt Is Nothing Then Return

        lblTotal.Text = dt.Rows.Count.ToString()

        lblAvailable.Text =
            dt.Select("Status = 'AVAILABLE'").Length.ToString()

        lblOccupied.Text =
            dt.Select("Status = 'OCCUPIED'").Length.ToString()

        lblMaintenance.Text =
            dt.Select("Status = 'MAINTENANCE'").Length.ToString()

    End Sub

    '========================================================
    ' SEARCH AND FILTER
    '========================================================

    Private Sub ApplyFilters(
        sender As Object,
        e As EventArgs
    )

        Dim dt As DataTable =
            TryCast(dgvParking.DataSource, DataTable)

        If dt Is Nothing Then Return

        Dim searchText As String = ""

        If searchBox IsNot Nothing AndAlso
            searchBox.Text <> "Search parking spaces..." Then

            searchText = searchBox.Text.Trim()

        End If

        Dim statusFilter As String = ""

        If cmbStatus IsNot Nothing AndAlso
            cmbStatus.SelectedItem IsNot Nothing Then

            statusFilter = cmbStatus.SelectedItem.ToString()

        End If

        Dim view As New DataView(dt)
        Dim filters As New System.Collections.Generic.List(Of String)

        If searchText <> "" Then

            Dim safeText As String =
                searchText.Replace("'", "''").
                Replace("[", "[[]").
                Replace("%", "[%]").
                Replace("*", "[*]")

            filters.Add(
                "Convert([Space Code], 'System.String') LIKE '%" &
                safeText & "%' OR " &
                "Convert([Location], 'System.String') LIKE '%" &
                safeText & "%' OR " &
                "Convert([Status], 'System.String') LIKE '%" &
                safeText & "%' OR " &
                "Convert([Configured Pricing], 'System.String') LIKE '%" &
                safeText & "%'")

        End If

        If statusFilter <> "" AndAlso
            statusFilter <> "All Statuses" Then

            filters.Add("[Status] = '" &
                        statusFilter.Replace("'", "''") & "'")

        End If

        If filters.Count > 0 Then
            view.RowFilter = String.Join(" AND ", filters)
        Else
            view.RowFilter = ""
        End If

        dgvParking.DataSource = view

        If dgvParking.Columns.Contains("parking_space_id") Then
            dgvParking.Columns("parking_space_id").Visible = False
        End If

    End Sub

    '========================================================
    ' SELECTED PARKING SPACE
    '========================================================

    Private Function GetSelectedSpaceId() As Integer?

        If dgvParking.CurrentRow Is Nothing Then
            Return Nothing
        End If

        Dim value As Object =
            dgvParking.CurrentRow.Cells("parking_space_id").Value

        If value Is Nothing OrElse value Is DBNull.Value Then
            Return Nothing
        End If

        Return Convert.ToInt32(value)

    End Function

    Private Function GetSelectedStatus() As String

        If dgvParking.CurrentRow Is Nothing Then Return ""

        Return Convert.ToString(
            dgvParking.CurrentRow.Cells("Status").Value
        ).Trim().ToUpperInvariant()

    End Function

    '========================================================
    ' ADD SPACE
    '========================================================

    Private Sub AddSpace(
        sender As Object,
        e As EventArgs
    )

        Using editor As New FrmParkingSpaceEditor()

            If editor.ShowDialog() = DialogResult.OK Then
                LoadParkingSpaces()
            End If

        End Using

    End Sub

    '========================================================
    ' EDIT SPACE
    '========================================================

    Private Sub EditSpace(
        sender As Object,
        e As EventArgs
    )

        Dim selectedId As Integer? = GetSelectedSpaceId()

        If Not selectedId.HasValue Then
            MessageBox.Show(
                "Please select a parking space to edit.",
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If

        Using editor As New FrmParkingSpaceEditor(selectedId.Value)

            If editor.ShowDialog() = DialogResult.OK Then
                LoadParkingSpaces()
            End If

        End Using

    End Sub

    Private Sub ParkingRowDoubleClick(
        sender As Object,
        e As DataGridViewCellEventArgs
    )

        If e.RowIndex >= 0 Then
            EditSpace(Nothing, EventArgs.Empty)
        End If

    End Sub

    '========================================================
    ' DELETE SPACE
    '========================================================

    Private Sub DeleteSpace(
        sender As Object,
        e As EventArgs
    )

        Dim selectedId As Integer? = GetSelectedSpaceId()

        If Not selectedId.HasValue Then
            MessageBox.Show(
                "Please select a parking space to delete.",
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)
            Return
        End If

        If GetSelectedStatus() = "OCCUPIED" Then
            MessageBox.Show(
                "An occupied parking space cannot be deleted.",
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show(
            "Are you sure you want to delete this parking space?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) <> DialogResult.Yes Then

            Return
        End If

        Try
            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                conn.Open()

                ' Preserve spaces with existing assignment history.

                Dim checkSql As String =
                    "SELECT COUNT(*) FROM parking_assignments " &
                    "WHERE parking_space_id = @spaceId"

                Using checkCmd As New MySqlCommand(checkSql, conn)

                    checkCmd.Parameters.AddWithValue(
                        "@spaceId", selectedId.Value)

                    If Convert.ToInt32(checkCmd.ExecuteScalar()) > 0 Then

                        MessageBox.Show(
                            "This parking space has assignment history " &
                            "and cannot be deleted. Change its status " &
                            "instead.",
                            "Parking Management",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

                        Return

                    End If

                End Using

                Dim deleteSql As String =
                    "DELETE FROM parking_spaces " &
                    "WHERE parking_space_id = @spaceId"

                Using cmd As New MySqlCommand(deleteSql, conn)

                    cmd.Parameters.AddWithValue(
                        "@spaceId", selectedId.Value)

                    cmd.ExecuteNonQuery()

                End Using

            End Using

            MessageBox.Show(
                "Parking space deleted successfully.",
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            LoadParkingSpaces()

        Catch ex As Exception

            MessageBox.Show(
                "Unable to delete the parking space." &
                Environment.NewLine & ex.Message,
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub

    '========================================================
    ' REFRESH
    '========================================================

    Private Sub RefreshData(
        sender As Object,
        e As EventArgs
    )

        LoadParkingSpaces()

    End Sub

End Class


'============================================================
' PARKING SPACE EDITOR FORM
'============================================================

Public Class FrmParkingSpaceEditor
    Inherits Form

    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly TextColor As Color =
        Color.FromArgb(51, 65, 85)

    Private txtSpaceCode As TextBox
    Private txtLocation As TextBox
    Private txtNotes As TextBox
    Private cmbStatus As ComboBox

    Private btnSave As Button
    Private btnCancel As Button

    Private ReadOnly editingId As Integer?
    Private ReadOnly isEditMode As Boolean

    Public Sub New(Optional spaceId As Integer? = Nothing)

        editingId = spaceId
        isEditMode = spaceId.HasValue

        BuildForm()

        If isEditMode Then
            LoadSpaceDetails(spaceId.Value)
        End If

    End Sub

    '========================================================
    ' BUILD FORM
    '========================================================

    Private Sub BuildForm()

        Me.Text = If(isEditMode, "Edit Parking Space", "Add Parking Space")
        Me.Size = New Size(520, 465)
        Me.MinimumSize = New Size(520, 465)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.FromArgb(240, 244, 248)
        Me.Font = New Font("Segoe UI", 9)

        Dim titlePanel As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 72,
            .BackColor = Navy
        }

        Dim lblTitle As New Label With {
            .Text = If(isEditMode, "Edit Parking Space", "Add Parking Space"),
            .Font = New Font("Segoe UI", 17, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(22, 19),
            .AutoSize = True
        }

        titlePanel.Controls.Add(lblTitle)
        Me.Controls.Add(titlePanel)

        Dim formPanel As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(24),
            .BackColor = Color.White
        }

        Me.Controls.Add(formPanel)

        AddFieldLabel(formPanel, "Space Code", 20, 20)

        txtSpaceCode = New TextBox With {
            .Location = New Point(20, 45),
            .Size = New Size(430, 30),
            .Font = New Font("Segoe UI", 10)
        }

        formPanel.Controls.Add(txtSpaceCode)

        AddFieldLabel(formPanel, "Location / Description", 20, 88)

        txtLocation = New TextBox With {
            .Location = New Point(20, 113),
            .Size = New Size(430, 30),
            .Font = New Font("Segoe UI", 10)
        }

        formPanel.Controls.Add(txtLocation)

        AddFieldLabel(formPanel, "Status", 20, 156)

        cmbStatus = New ComboBox With {
            .Location = New Point(20, 181),
            .Size = New Size(430, 30),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }

        cmbStatus.Items.AddRange(
            New Object() {
                "AVAILABLE",
                "MAINTENANCE",
                "INACTIVE"
            })

        cmbStatus.SelectedIndex = 0
        formPanel.Controls.Add(cmbStatus)

        AddFieldLabel(formPanel, "Notes (optional)", 20, 224)

        txtNotes = New TextBox With {
            .Location = New Point(20, 249),
            .Size = New Size(430, 45),
            .Multiline = True,
            .Font = New Font("Segoe UI", 10)
        }

        formPanel.Controls.Add(txtNotes)

        Dim lblPricingInfo As New Label With {
            .Text = "Parking prices are managed in System Settings.",
            .Location = New Point(20, 304),
            .Size = New Size(430, 25),
            .ForeColor = Color.Gray,
            .Font = New Font("Segoe UI", 9, FontStyle.Italic)
        }

        formPanel.Controls.Add(lblPricingInfo)

        btnSave = New Button With {
            .Text = "Save",
            .Location = New Point(215, 343),
            .Size = New Size(105, 35),
            .BackColor = Teal,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }

        btnCancel = New Button With {
            .Text = "Cancel",
            .Location = New Point(330, 343),
            .Size = New Size(120, 35),
            .BackColor = Navy,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .DialogResult = DialogResult.Cancel
        }

        AddHandler btnSave.Click, AddressOf SaveSpace

        formPanel.Controls.Add(btnSave)
        formPanel.Controls.Add(btnCancel)

        Me.AcceptButton = btnSave
        Me.CancelButton = btnCancel

    End Sub

    Private Sub AddFieldLabel(
        parent As Control,
        caption As String,
        x As Integer,
        y As Integer
    )

        parent.Controls.Add(
            New Label With {
                .Text = caption,
                .Location = New Point(x, y),
                .AutoSize = True,
                .Font = New Font("Segoe UI", 9, FontStyle.Bold),
                .ForeColor = TextColor
            })

    End Sub

    '========================================================
    ' LOAD EXISTING SPACE
    '========================================================

    Private Sub LoadSpaceDetails(spaceId As Integer)

        Try
            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                conn.Open()

                Dim sql As String =
                    "SELECT space_code, location_description, status " &
                    "FROM parking_spaces " &
                    "WHERE parking_space_id = @spaceId"

                Using cmd As New MySqlCommand(sql, conn)

                    cmd.Parameters.AddWithValue("@spaceId", spaceId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()

                        If Not reader.Read() Then
                            MessageBox.Show(
                                "Parking space not found.",
                                "Parking Management",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)

                            Me.DialogResult = DialogResult.Cancel
                            Me.Close()
                            Return
                        End If

                        txtSpaceCode.Text =
                            Convert.ToString(reader("space_code"))

                        txtLocation.Text =
                            If(reader("location_description") Is DBNull.Value,
                               "",
                               Convert.ToString(reader("location_description")))

                        Dim currentStatus As String =
                            Convert.ToString(reader("status")).ToUpperInvariant()

                        ' An occupied space must not be changed to
                        ' AVAILABLE manually from this admin editor.

                        If currentStatus = "OCCUPIED" Then
                            cmbStatus.Items.Add("OCCUPIED")
                            cmbStatus.SelectedItem = "OCCUPIED"
                            cmbStatus.Enabled = False
                        ElseIf cmbStatus.Items.Contains(currentStatus) Then
                            cmbStatus.SelectedItem = currentStatus
                        Else
                            cmbStatus.SelectedIndex = 0
                        End If

                    End Using
                End Using

            End Using

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load parking space details." &
                Environment.NewLine & ex.Message,
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try

    End Sub

    '========================================================
    ' SAVE SPACE
    '========================================================

    Private Sub SaveSpace(
        sender As Object,
        e As EventArgs
    )

        Dim spaceCode As String = txtSpaceCode.Text.Trim()
        Dim location As String = txtLocation.Text.Trim()
        Dim status As String = Convert.ToString(cmbStatus.SelectedItem)

        If spaceCode = "" Then
            MessageBox.Show(
                "Please enter a parking space code.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtSpaceCode.Focus()
            Return
        End If

        If location = "" Then
            MessageBox.Show(
                "Please enter the parking space location.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            txtLocation.Focus()
            Return
        End If

        Try
            Using conn As MySqlConnection =
                DbConnection.GetConnection()

                conn.Open()

                ' Prevent duplicate space codes.

                Dim duplicateSql As String =
                    "SELECT COUNT(*) FROM parking_spaces " &
                    "WHERE space_code = @spaceCode " &
                    "AND parking_space_id <> @spaceId"

                Using checkCmd As New MySqlCommand(duplicateSql, conn)

                    checkCmd.Parameters.AddWithValue(
                        "@spaceCode", spaceCode)

                    checkCmd.Parameters.AddWithValue(
                        "@spaceId",
                        If(editingId.HasValue,
                           editingId.Value,
                           0))

                    If Convert.ToInt32(checkCmd.ExecuteScalar()) > 0 Then

                        MessageBox.Show(
                            "This parking space code already exists.",
                            "Duplicate Space Code",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)

                        txtSpaceCode.Focus()
                        Return

                    End If

                End Using

                If isEditMode Then

                    ' Never change an occupied space's status
                    ' through this editor.

                    Dim updateSql As String =
                        "UPDATE parking_spaces SET " &
                        "space_code = @spaceCode, " &
                        "location_description = @location, " &
                        "status = CASE WHEN status = 'OCCUPIED' " &
                        "THEN status ELSE @status END " &
                        "WHERE parking_space_id = @spaceId"

                    Using cmd As New MySqlCommand(updateSql, conn)

                        cmd.Parameters.AddWithValue(
                            "@spaceCode", spaceCode)

                        cmd.Parameters.AddWithValue(
                            "@location", location)

                        cmd.Parameters.AddWithValue(
                            "@status", status)

                        cmd.Parameters.AddWithValue(
                            "@spaceId", editingId.Value)

                        cmd.ExecuteNonQuery()

                    End Using

                Else

                    Dim insertSql As String =
                        "INSERT INTO parking_spaces " &
                        "(space_code, location_description, status) " &
                        "VALUES (@spaceCode, @location, @status)"

                    Using cmd As New MySqlCommand(insertSql, conn)

                        cmd.Parameters.AddWithValue(
                            "@spaceCode", spaceCode)

                        cmd.Parameters.AddWithValue(
                            "@location", location)

                        cmd.Parameters.AddWithValue(
                            "@status", status)

                        cmd.ExecuteNonQuery()

                    End Using

                End If

            End Using

            MessageBox.Show(
                If(isEditMode,
                   "Parking space updated successfully.",
                   "Parking space added successfully."),
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception

            MessageBox.Show(
                "Unable to save the parking space." &
                Environment.NewLine & ex.Message,
                "Parking Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

        End Try

    End Sub

End Class
