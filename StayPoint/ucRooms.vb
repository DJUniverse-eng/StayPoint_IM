
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucRooms
    Inherits UserControl

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
    Private ReadOnly Teal As Color = Color.FromArgb(22, 160, 145)
    Private ReadOnly PageBackground As Color = Color.FromArgb(245, 247, 250)
    Private ReadOnly White As Color = Color.White
    Private ReadOnly TextColor As Color = Color.FromArgb(55, 65, 81)
    Private ReadOnly MutedColor As Color = Color.FromArgb(107, 114, 128)
    Private ReadOnly BorderColor As Color = Color.FromArgb(226, 232, 240)

    Private headerPanel As Panel
    Private summaryPanel As FlowLayoutPanel
    Private searchPanel As Panel
    Private txtSearch As TextBox
    Private cboStatus As ComboBox
    Private dgvRooms As DataGridView

    Private lblTotal As Label
    Private lblAvailable As Label
    Private lblOccupied As Label
    Private lblMaintenance As Label

    Private btnAdd As Button
    Private btnEdit As Button
    Private btnDelete As Button
    Private btnRefresh As Button

    Private roomTable As DataTable

    Public Sub New()
        MyBase.New()
        BuildInterface()
        LoadRooms()
        InitializeComponent()
    End Sub

    Private Sub BuildInterface()
        SuspendLayout()

        Dock = DockStyle.Fill
        BackColor = PageBackground
        Padding = New Padding(12)

        headerPanel = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 65,
            .BackColor = PageBackground
        }

        headerPanel.Controls.Add(New Label With {
            .Text = "Room Management",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 0)
        })

        headerPanel.Controls.Add(New Label With {
            .Text = "Manage room numbers, room types, pricing and availability.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(2, 40)
        })

        summaryPanel = New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 92,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = PageBackground,
            .Padding = New Padding(0, 3, 0, 5)
        }

        lblTotal = CreateSummaryCard()
        lblAvailable = CreateSummaryCard()
        lblOccupied = CreateSummaryCard()
        lblMaintenance = CreateSummaryCard()

        summaryPanel.Controls.Add(CreateCardPanel("Total Rooms", lblTotal))
        summaryPanel.Controls.Add(CreateCardPanel("Available", lblAvailable))
        summaryPanel.Controls.Add(CreateCardPanel("Occupied", lblOccupied))
        summaryPanel.Controls.Add(CreateCardPanel("Maintenance", lblMaintenance))

        searchPanel = New Panel With {
            .Dock = DockStyle.Top,
            .Height = 60,
            .BackColor = White,
            .Padding = New Padding(8)
        }

        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle
        }

        AddHandler txtSearch.TextChanged,
            Sub(sender, e) ApplyFilters()

        cboStatus = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 9)
        }

        cboStatus.Items.Clear()
        cboStatus.Items.AddRange(
    New Object() {
        "All Statuses",
        "AVAILABLE",
        "OCCUPIED",
        "CLEANING",
        "UNDER MAINTENANCE",
        "RESERVED",
        "INACTIVE"
    }
)
        cboStatus.SelectedIndex = 0

        AddHandler cboStatus.SelectedIndexChanged,
            Sub(sender, e) ApplyFilters()

        btnAdd = CreateButton("Add Room", Teal)
        btnEdit = CreateButton("Edit", Navy)
        btnDelete = CreateButton("Delete", Color.FromArgb(190, 55, 55))
        btnRefresh = CreateButton("Refresh", SidebarColor)

        AddHandler btnAdd.Click, Sub(sender, e) OpenRoomEditor(0)
        AddHandler btnEdit.Click, Sub(sender, e) EditSelectedRoom()
        AddHandler btnDelete.Click, Sub(sender, e) DeleteSelectedRoom()
        AddHandler btnRefresh.Click, Sub(sender, e) LoadRooms()

        searchPanel.Controls.AddRange(New Control() {
            txtSearch, cboStatus, btnAdd, btnEdit, btnDelete, btnRefresh
        })

        AddHandler searchPanel.Resize, Sub(sender, e) ArrangeToolbar()

        dgvRooms = New DataGridView With {
            .Dock = DockStyle.Fill,
            .BackgroundColor = White,
            .BorderStyle = BorderStyle.None,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .ReadOnly = True,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .AutoGenerateColumns = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .RowHeadersVisible = False,
            .EnableHeadersVisualStyles = False,
            .GridColor = BorderColor,
            .Font = New Font("Segoe UI", 9),
            .ColumnHeadersHeight = 42,
            .ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            .RowTemplate = New DataGridViewRow With {.Height = 36}
        }

        dgvRooms.ColumnHeadersDefaultCellStyle.BackColor = Navy
        dgvRooms.ColumnHeadersDefaultCellStyle.ForeColor = White
        dgvRooms.ColumnHeadersDefaultCellStyle.Font =
            New Font("Segoe UI", 9, FontStyle.Bold)
        dgvRooms.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(220, 240, 237)
        dgvRooms.DefaultCellStyle.SelectionForeColor = Navy
        dgvRooms.DefaultCellStyle.Padding = New Padding(5, 2, 5, 2)
        dgvRooms.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 252)
        dgvRooms.DefaultCellStyle.ForeColor = TextColor

        AddGridColumn("room_id", "ID", 5, 45)
        AddGridColumn("room_number", "Room No.", 8, 75)
        AddGridColumn("type_name", "Room Type", 12, 100)
        AddGridColumn("bed_size", "Bed Size", 10, 90)
        AddGridColumn("floor_number", "Floor", 5, 50)
        AddGridColumn("hourly_rate", "Hourly Rate", 10, 95, True)
        AddGridColumn("additional_hourly_rate", "Extra Hour", 10, 95, True)
        AddGridColumn("daily_rate", "Daily Rate", 10, 95, True)
        AddGridColumn("status", "Status", 10, 95)
        AddGridColumn("notes", "Notes", 15, 110)

        AddHandler dgvRooms.CellDoubleClick,
            Sub(sender, e)
                If e.RowIndex >= 0 Then EditSelectedRoom()
            End Sub

        Controls.Add(dgvRooms)
        Controls.Add(searchPanel)
        Controls.Add(summaryPanel)
        Controls.Add(headerPanel)

        ResumeLayout(True)
        ArrangeToolbar()
    End Sub

    Private Sub ArrangeToolbar()
        If searchPanel Is Nothing OrElse btnRefresh Is Nothing Then Return

        Dim rightEdge As Integer = searchPanel.ClientSize.Width - 10
        Dim buttonY As Integer = 9

        btnRefresh.SetBounds(rightEdge - 88, buttonY, 88, 36)
        btnDelete.SetBounds(btnRefresh.Left - 86, buttonY, 80, 36)
        btnEdit.SetBounds(btnDelete.Left - 78, buttonY, 72, 36)
        btnAdd.SetBounds(btnEdit.Left - 106, buttonY, 100, 36)

        txtSearch.SetBounds(8, 13, 210, 28)
        cboStatus.SetBounds(226, 12,
            Math.Max(100, btnAdd.Left - 238), 28)
    End Sub

    Private Function CreateSummaryCard() As Label
        Return New Label With {
            .Text = "0",
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True
        }
    End Function

    Private Function CreateCardPanel(title As String,
                                     valueLabel As Label) As Panel
        Dim card As New Panel With {
            .Width = 185,
            .Height = 78,
            .BackColor = White,
            .Margin = New Padding(0, 0, 12, 0),
            .Padding = New Padding(12)
        }

        card.Controls.Add(New Label With {
            .Text = title,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = MutedColor,
            .AutoSize = True,
            .Location = New Point(12, 8)
        })

        valueLabel.Location = New Point(12, 29)
        card.Controls.Add(valueLabel)
        Return card
    End Function

    Private Function CreateButton(caption As String,
                                  buttonColor As Color) As Button
        Return New Button With {
            .Text = caption,
            .BackColor = buttonColor,
            .ForeColor = White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Height = 36,
            .UseVisualStyleBackColor = False
        }
    End Function

    Private Sub AddGridColumn(columnName As String,
                              headerText As String,
                              fillWeight As Single,
                              minimumWidth As Integer,
                              Optional isCurrency As Boolean = False)

        Dim col As New DataGridViewTextBoxColumn With {
            .Name = columnName,
            .DataPropertyName = columnName,
            .HeaderText = headerText,
            .FillWeight = fillWeight,
            .MinimumWidth = minimumWidth,
            .SortMode = DataGridViewColumnSortMode.Automatic
        }

        If isCurrency Then
            col.DefaultCellStyle.Format = "₱#,##0.00"
            col.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleRight
        End If

        If columnName = "room_id" Then col.Visible = False
        dgvRooms.Columns.Add(col)
    End Sub

    Private Sub LoadRooms()
        Try
            Using con As MySqlConnection = DbConnection.GetConnection()
                con.Open()

                Dim sql As String =
                    "SELECT r.room_id, r.room_number, r.room_type_id, " &
                    "r.room_type_option_id, r.floor_number, " &
                    "COALESCE(rt.type_name, 'Unassigned') AS type_name, " &
                    "COALESCE(ro.bed_size, '') AS bed_size, " &
                    "COALESCE(ro.hourly_rate, 0) AS hourly_rate, " &
                    "COALESCE(ro.additional_hourly_rate, 0) AS additional_hourly_rate, " &
                    "COALESCE(ro.daily_rate, 0) AS daily_rate, " &
                    "UPPER(COALESCE(r.status, 'AVAILABLE')) AS status, " &
                    "COALESCE(r.notes, '') AS notes " &
                    "FROM rooms r " &
                    "LEFT JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                    "LEFT JOIN room_type_options ro " &
                    "ON ro.option_id = r.room_type_option_id " &
                    "ORDER BY r.floor_number, r.room_number"

                roomTable = New DataTable()
                Using adapter As New MySqlDataAdapter(sql, con)
                    adapter.Fill(roomTable)
                End Using
            End Using

            ApplyFilters()
            UpdateSummary()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load room records." & Environment.NewLine & ex.Message,
                "StayPoint - Room Management",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ApplyFilters()
        If roomTable Is Nothing OrElse dgvRooms Is Nothing Then Return

        Try
            Dim view As New DataView(roomTable)
            Dim conditions As New List(Of String)
            Dim searchText As String = txtSearch.Text.Trim()

            If searchText <> "" Then
                Dim escaped As String =
                    searchText.Replace("'", "''").
                    Replace("[", "[[]").
                    Replace("%", "[%]").
                    Replace("*", "[*]")

                conditions.Add(
                    "(Convert(room_number, 'System.String') LIKE '%" & escaped & "%' " &
                    "OR Convert(type_name, 'System.String') LIKE '%" & escaped & "%' " &
                    "OR Convert(bed_size, 'System.String') LIKE '%" & escaped & "%' " &
                    "OR Convert(floor_number, 'System.String') LIKE '%" & escaped & "%' " &
                    "OR Convert(notes, 'System.String') LIKE '%" & escaped & "%')")
            End If

            If cboStatus.Text <> "" AndAlso
               Not cboStatus.Text.Equals(
                   "All Statuses", StringComparison.OrdinalIgnoreCase) Then

                conditions.Add(
                    "status = '" &
                    cboStatus.Text.Replace("'", "''").ToUpperInvariant() & "'")
            End If

            view.RowFilter = String.Join(" AND ", conditions)
            dgvRooms.DataSource = view

        Catch ex As Exception
            MessageBox.Show(
                "Unable to display room records." & Environment.NewLine & ex.Message,
                "Room Display Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateSummary()
        If roomTable Is Nothing Then Return

        lblTotal.Text = roomTable.Rows.Count.ToString()
        lblAvailable.Text = CountStatus("AVAILABLE").ToString()
        lblOccupied.Text = CountStatus("OCCUPIED").ToString()
        lblMaintenance.Text =
        (CountStatus("MAINTENANCE") +
         CountStatus("UNDER MAINTENANCE")).ToString()
    End Sub

    Private Function CountStatus(statusValue As String) As Integer
        If roomTable Is Nothing Then Return 0

        Dim count As Integer = 0
        For Each row As DataRow In roomTable.Rows
            If Not row.IsNull("status") AndAlso
               String.Equals(row("status").ToString(), statusValue,
                             StringComparison.OrdinalIgnoreCase) Then
                count += 1
            End If
        Next
        Return count
    End Function

    Private Function SelectedRoomId() As Integer
        If dgvRooms.CurrentRow Is Nothing Then Return 0

        Dim value As Object = dgvRooms.CurrentRow.Cells("room_id").Value
        If value Is Nothing OrElse value Is DBNull.Value Then Return 0

        Return Convert.ToInt32(value)
    End Function

    Private Sub EditSelectedRoom()
        Dim roomId As Integer = SelectedRoomId()

        If roomId <= 0 Then
            MessageBox.Show("Select a room first.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        OpenRoomEditor(roomId)
    End Sub

    Private Sub OpenRoomEditor(roomId As Integer)
        Using editor As New FrmRoomEditor(roomId)
            If editor.ShowDialog(Me) = DialogResult.OK Then LoadRooms()
        End Using
    End Sub

    Private Sub DeleteSelectedRoom()
        Dim roomId As Integer = SelectedRoomId()

        If roomId <= 0 Then
            MessageBox.Show("Select a room to delete.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim roomNumber As String =
            Convert.ToString(dgvRooms.CurrentRow.Cells("room_number").Value)
        Dim roomStatus As String =
            Convert.ToString(dgvRooms.CurrentRow.Cells("status").Value)

        If roomStatus.Equals("OCCUPIED", StringComparison.OrdinalIgnoreCase) Then
            MessageBox.Show("This room is occupied and cannot be deleted.",
                            "Cannot Delete Room", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show(
            "Delete room " & roomNumber & "?" & Environment.NewLine &
            "Rooms linked to booking history cannot be deleted.",
            "Confirm Delete", MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            Return
        End If

        Try
            Using con As MySqlConnection = DbConnection.GetConnection()
                con.Open()

                Using transaction As MySqlTransaction = con.BeginTransaction()
                    Using checkCmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM booking_rooms WHERE room_id = @id",
                        con, transaction)

                        checkCmd.Parameters.AddWithValue("@id", roomId)

                        If Convert.ToInt32(checkCmd.ExecuteScalar()) > 0 Then
                            transaction.Rollback()
                            MessageBox.Show(
                                "This room is linked to booking history and cannot be deleted." &
                                Environment.NewLine &
                                "Change its status to Maintenance instead.",
                                "Room Has Booking History", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                            Return
                        End If
                    End Using

                    Using deleteCmd As New MySqlCommand(
                        "DELETE FROM rooms WHERE room_id = @id", con, transaction)

                        deleteCmd.Parameters.AddWithValue("@id", roomId)

                        If deleteCmd.ExecuteNonQuery() = 0 Then
                            transaction.Rollback()
                            MessageBox.Show("The room no longer exists.")
                            Return
                        End If
                    End Using

                    transaction.Commit()
                End Using
            End Using

            MessageBox.Show("Room " & roomNumber & " was deleted.",
                            "StayPoint", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadRooms()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to delete the room. It may be referenced by another record." &
                Environment.NewLine & ex.Message,
                "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class

Public Class FrmRoomEditor
    Inherits Form

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly Teal As Color = Color.FromArgb(22, 160, 145)
    Private ReadOnly BackgroundColor As Color =
        Color.FromArgb(245, 247, 250)

    Private ReadOnly currentRoomId As Integer

    Private roomTypesTable As DataTable
    Private roomOptionsTable As DataTable

    Private savedOptionId As Integer = 0
    Private isLoading As Boolean = False
    Private roomLoaded As Boolean = False

    Private txtRoomNumber As TextBox
    Private cboRoomType As ComboBox
    Private cboBedSize As ComboBox
    Private nudFloor As NumericUpDown
    Private cboStatus As ComboBox
    Private txtNotes As TextBox
    Private lblPricing As Label
    Private lblNumberHelp As Label
    Private btnSave As Button
    Private btnCancel As Button

    Public Sub New(roomId As Integer)
        currentRoomId = roomId

        BuildInterface()
        LoadRoomTypes()

        If roomTypesTable Is Nothing OrElse
           roomTypesTable.Rows.Count = 0 Then

            btnSave.Enabled = False
            Return
        End If

        If currentRoomId > 0 Then
            LoadRoom()
        Else
            SetDefaultValues()
            roomLoaded = True
        End If

        If Not roomLoaded Then
            btnSave.Enabled = False
        End If
    End Sub

    '========================================================
    ' INTERFACE
    '========================================================

    Private Sub BuildInterface()
        Text = If(currentRoomId = 0, "Add Room", "Edit Room")
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ClientSize = New Size(600, 650)
        BackColor = BackgroundColor
        Font = New Font("Segoe UI", 9)

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 75,
            .BackColor = Navy
        }

        header.Controls.Add(New Label With {
            .Text = If(currentRoomId = 0,
                       "Add New Room", "Edit Room"),
            .Font = New Font("Segoe UI", 17, FontStyle.Bold),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(22, 17)
        })

        Dim content As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(26, 16, 26, 10),
            .AutoScroll = True
        }

        Dim y As Integer = 5
        Dim fieldWidth As Integer = 510

        AddField(content, "Room Number (Automatically Generated)", y)
        y += 25

        txtRoomNumber = New TextBox With {
            .Location = New Point(0, y),
            .Width = fieldWidth,
            .MaxLength = 3,
            .Font = New Font("Segoe UI", 11),
            .ReadOnly = True,
            .BackColor = Color.FromArgb(235, 240, 245)
        }
        content.Controls.Add(txtRoomNumber)
        y += 35

        lblNumberHelp = New Label With {
            .Text = "Number format: floor + room type code + sequence.",
            .Location = New Point(0, y),
            .AutoSize = True,
            .ForeColor = Color.DimGray
        }
        content.Controls.Add(lblNumberHelp)
        y += 30

        AddField(content, "Room Type", y)
        y += 25

        cboRoomType = New ComboBox With {
            .Location = New Point(0, y),
            .Width = fieldWidth,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }
        content.Controls.Add(cboRoomType)

        AddHandler cboRoomType.SelectedIndexChanged,
            Sub(sender, e)
                If isLoading Then Return

                LoadBedSizes()
                UpdatePricing()
                GenerateRoomNumber()
            End Sub

        y += 43

        AddField(content, "Bed Size", y)
        y += 25

        cboBedSize = New ComboBox With {
            .Location = New Point(0, y),
            .Width = fieldWidth,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }
        content.Controls.Add(cboBedSize)

        AddHandler cboBedSize.SelectedIndexChanged,
            Sub(sender, e)
                If isLoading Then Return
                UpdatePricing()
            End Sub

        y += 43

        AddField(content, "Floor", y)
        y += 25

        nudFloor = New NumericUpDown With {
            .Location = New Point(0, y),
            .Width = 150,
            .Minimum = 1,
            .Maximum = 9,
            .Value = 1,
            .Font = New Font("Segoe UI", 10)
        }
        content.Controls.Add(nudFloor)

        AddHandler nudFloor.ValueChanged,
            Sub(sender, e)
                If Not isLoading Then GenerateRoomNumber()
            End Sub

        y += 43

        AddField(content, "Room Status", y)
        y += 25

        cboStatus = New ComboBox With {
            .Location = New Point(0, y),
            .Width = 230,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10)
        }

        cboStatus.Items.AddRange(New Object() {
            "AVAILABLE",
            "OCCUPIED",
            "RESERVED",
            "MAINTENANCE"
        })
        content.Controls.Add(cboStatus)
        y += 43

        AddField(content, "Fixed Pricing", y)
        y += 25

        lblPricing = New Label With {
            .Location = New Point(0, y),
            .Width = fieldWidth,
            .Height = 76,
            .BackColor = Color.White,
            .ForeColor = Navy,
            .Padding = New Padding(10),
            .Text = "Select a room type and bed size."
        }
        content.Controls.Add(lblPricing)
        y += 86

        AddField(content, "Notes (Optional)", y)
        y += 25

        txtNotes = New TextBox With {
            .Location = New Point(0, y),
            .Width = fieldWidth,
            .Height = 58,
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .MaxLength = 500
        }
        content.Controls.Add(txtNotes)

        Dim footer As New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 60,
            .BackColor = Color.White
        }

        btnSave = CreateButton("Save Room", Teal)
        btnSave.SetBounds(370, 12, 105, 36)
        AddHandler btnSave.Click,
            Sub(sender, e) SaveRoom()

        btnCancel = CreateButton("Cancel", Color.Gray)
        btnCancel.SetBounds(483, 12, 85, 36)
        btnCancel.DialogResult = DialogResult.Cancel

        footer.Controls.Add(btnSave)
        footer.Controls.Add(btnCancel)

        Controls.Add(content)
        Controls.Add(footer)
        Controls.Add(header)

        AcceptButton = btnSave
        CancelButton = btnCancel
    End Sub

    Private Sub AddField(
        parent As Control,
        caption As String,
        y As Integer)

        parent.Controls.Add(New Label With {
            .Text = caption,
            .Location = New Point(0, y),
            .AutoSize = True,
            .ForeColor = Navy,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        })
    End Sub

    Private Function CreateButton(
        caption As String,
        buttonColor As Color) As Button

        Dim result As New Button With {
            .Text = caption,
            .BackColor = buttonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Height = 35,
            .UseVisualStyleBackColor = False,
            .Cursor = Cursors.Hand
        }

        result.FlatAppearance.BorderSize = 0
        Return result
    End Function

    '========================================================
    ' LOAD ROOM TYPES
    '========================================================

    Private Sub LoadRoomTypes()
        Try
            Using con As MySqlConnection =
                DbConnection.GetConnection()

                con.Open()

                Dim sql As String =
                    "SELECT room_type_id, type_name " &
                    "FROM room_types ORDER BY type_name"

                roomTypesTable = New DataTable()

                Using adapter As New MySqlDataAdapter(sql, con)
                    adapter.Fill(roomTypesTable)
                End Using
            End Using

            cboRoomType.DataSource = Nothing
            cboRoomType.DisplayMember = "type_name"
            cboRoomType.ValueMember = "room_type_id"
            cboRoomType.DataSource = roomTypesTable

            If roomTypesTable.Rows.Count = 0 Then
                MessageBox.Show(
                    "No room types are configured. " &
                    "Add room types in System Settings first.",
                    "No Room Types",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            End If

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load room types." &
                Environment.NewLine & ex.Message,
                "StayPoint - Room Editor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    '========================================================
    ' LOAD BED SIZES AND PRICING OPTIONS
    '========================================================

    Private Sub LoadBedSizes(
        Optional preferredOptionId As Integer = 0)

        Dim typeId As Integer = SelectedTypeId()

        If typeId <= 0 Then
            cboBedSize.DataSource = Nothing
            roomOptionsTable = Nothing
            Return
        End If

        Try
            Using con As MySqlConnection =
                DbConnection.GetConnection()

                con.Open()

                Dim sql As String =
                    "SELECT option_id, room_type_id, bed_size, " &
                    "hourly_rate, additional_hourly_rate, " &
                    "daily_rate, minimum_hours, is_active " &
                    "FROM room_type_options " &
                    "WHERE room_type_id = @typeId " &
                    "AND (is_active = 1 OR option_id = @savedOptionId) " &
                    "ORDER BY bed_size"

                roomOptionsTable = New DataTable()

                Using cmd As New MySqlCommand(sql, con)
                    cmd.Parameters.AddWithValue("@typeId", typeId)
                    cmd.Parameters.AddWithValue(
                        "@savedOptionId", preferredOptionId)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(roomOptionsTable)
                    End Using
                End Using
            End Using

            cboBedSize.DataSource = Nothing
            cboBedSize.DisplayMember = "bed_size"
            cboBedSize.ValueMember = "option_id"
            cboBedSize.DataSource = roomOptionsTable

            If roomOptionsTable.Rows.Count = 0 Then
                lblPricing.Text =
                    "No active bed-size pricing options exist " &
                    "for this room type."
                Return
            End If

            Dim found As Boolean = False

            If preferredOptionId > 0 Then
                For i As Integer = 0 To roomOptionsTable.Rows.Count - 1
                    If Convert.ToInt32(
                        roomOptionsTable.Rows(i)("option_id")) =
                        preferredOptionId Then

                        cboBedSize.SelectedIndex = i
                        found = True
                        Exit For
                    End If
                Next
            End If

            If Not found Then cboBedSize.SelectedIndex = 0

        Catch ex As Exception
            roomOptionsTable = Nothing
            cboBedSize.DataSource = Nothing

            lblPricing.Text = "Unable to load pricing options."

            MessageBox.Show(
                "Unable to load bed-size pricing options." &
                Environment.NewLine & ex.Message,
                "StayPoint - Room Editor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function SelectedTypeId() As Integer
        If cboRoomType Is Nothing OrElse
           cboRoomType.SelectedValue Is Nothing Then
            Return 0
        End If

        Dim result As Integer

        If Integer.TryParse(
            Convert.ToString(cboRoomType.SelectedValue),
            result) Then

            Return result
        End If

        Return 0
    End Function

    Private Function SelectedOptionRow() As DataRow
        If roomOptionsTable Is Nothing OrElse
           cboBedSize Is Nothing OrElse
           cboBedSize.SelectedValue Is Nothing Then

            Return Nothing
        End If

        Dim optionId As Integer

        If Not Integer.TryParse(
            Convert.ToString(cboBedSize.SelectedValue),
            optionId) Then

            Return Nothing
        End If

        For Each row As DataRow In roomOptionsTable.Rows
            If Convert.ToInt32(row("option_id")) = optionId Then
                Return row
            End If
        Next

        Return Nothing
    End Function

    Private Function SelectedOptionId() As Integer
        Dim row As DataRow = SelectedOptionRow()

        If row Is Nothing Then Return 0

        Return Convert.ToInt32(row("option_id"))
    End Function

    Private Sub UpdatePricing()
        If lblPricing Is Nothing Then Return

        Dim row As DataRow = SelectedOptionRow()

        If row Is Nothing Then
            lblPricing.Text =
                "Select an available bed-size pricing option."
            Return
        End If

        Dim hourly As Decimal =
            Convert.ToDecimal(row("hourly_rate"))

        Dim extra As Decimal =
            Convert.ToDecimal(row("additional_hourly_rate"))

        Dim daily As Decimal =
            Convert.ToDecimal(row("daily_rate"))

        Dim minimumHours As Integer =
            Convert.ToInt32(row("minimum_hours"))

        lblPricing.Text =
            "Hourly: " & hourly.ToString("₱#,##0.00") &
            "    Extra Hour: " & extra.ToString("₱#,##0.00") &
            Environment.NewLine &
            "Daily: " & daily.ToString("₱#,##0.00") &
            "    Minimum Stay: " & minimumHours.ToString() &
            " hour(s)"
    End Sub

    '========================================================
    ' ROOM NUMBER GENERATION
    '
    ' Standard = 0
    ' Deluxe   = 1
    ' Suite    = 2
    '
    ' Other types receive codes from 3 onward, alphabetically.
    '========================================================

    Private Function GetRoomTypeCode(
        typeName As String) As Integer

        If String.Equals(
            typeName, "Standard",
            StringComparison.OrdinalIgnoreCase) Then Return 0

        If String.Equals(
            typeName, "Deluxe",
            StringComparison.OrdinalIgnoreCase) Then Return 1

        If String.Equals(
            typeName, "Suite",
            StringComparison.OrdinalIgnoreCase) Then Return 2

        If roomTypesTable Is Nothing Then Return -1

        Dim names As New List(Of String)

        For Each row As DataRow In roomTypesTable.Rows
            Dim name As String =
                Convert.ToString(row("type_name")).Trim()

            If name <> "" AndAlso
               Not names.Exists(
                   Function(existing) String.Equals(
                       existing, name,
                       StringComparison.OrdinalIgnoreCase)) Then

                names.Add(name)
            End If
        Next

        names.Sort(StringComparer.OrdinalIgnoreCase)

        Dim nextCode As Integer = 3

        For Each name As String In names
            If String.Equals(
                name, "Standard",
                StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(
                name, "Deluxe",
                StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(
                name, "Suite",
                StringComparison.OrdinalIgnoreCase) Then

                Continue For
            End If

            If String.Equals(
                name, typeName,
                StringComparison.OrdinalIgnoreCase) Then

                If nextCode > 9 Then Return -1

                Return nextCode
            End If

            nextCode += 1
        Next

        Return -1
    End Function

    Private Sub GenerateRoomNumber()
        If isLoading Then Return

        Dim typeId As Integer = SelectedTypeId()

        If typeId <= 0 Then
            txtRoomNumber.Clear()
            Return
        End If

        Dim floorNo As Integer =
            Convert.ToInt32(nudFloor.Value)

        Dim typeCode As Integer =
            GetRoomTypeCode(cboRoomType.Text)

        If typeCode < 0 OrElse typeCode > 9 Then
            txtRoomNumber.Clear()
            lblNumberHelp.Text =
                "No room type code is available for this type."
            Return
        End If

        Dim prefix As String =
            floorNo.ToString() & typeCode.ToString()

        Try
            Using con As MySqlConnection =
                DbConnection.GetConnection()

                con.Open()

                For sequence As Integer = 1 To 9
                    Dim candidate As String =
                        prefix & sequence.ToString()

                    Using cmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM rooms " &
                        "WHERE room_number = @number " &
                        "AND room_id <> @id",
                        con)

                        cmd.Parameters.AddWithValue(
                            "@number", candidate)

                        cmd.Parameters.AddWithValue(
                            "@id", currentRoomId)

                        If Convert.ToInt32(
                            cmd.ExecuteScalar()) = 0 Then

                            txtRoomNumber.Text = candidate

                            lblNumberHelp.Text =
                                "Number generated successfully."
                            Return
                        End If
                    End Using
                Next
            End Using

            txtRoomNumber.Clear()
            lblNumberHelp.Text =
                "No room numbers remain for this floor and type."

            MessageBox.Show(
                "All nine room numbers for this floor and room type " &
                "are already used. Change the floor or review your " &
                "room-numbering scheme.",
                "Room Numbers Exhausted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

        Catch ex As Exception
            txtRoomNumber.Clear()
            lblNumberHelp.Text =
                "Unable to generate a room number."

            MessageBox.Show(
                "Unable to generate a room number." &
                Environment.NewLine & ex.Message,
                "StayPoint",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SetDefaultValues()
        isLoading = True

        nudFloor.Value = 1
        cboStatus.SelectedItem = "AVAILABLE"

        isLoading = False

        If roomTypesTable IsNot Nothing AndAlso
           roomTypesTable.Rows.Count > 0 Then

            cboRoomType.SelectedIndex = 0
            LoadBedSizes()
            UpdatePricing()
            GenerateRoomNumber()
        End If
    End Sub

    '========================================================
    ' LOAD EXISTING ROOM
    '========================================================

    Private Sub LoadRoom()
        Try
            Dim savedTypeId As Integer
            Dim savedFloor As Integer
            Dim savedStatus As String
            Dim savedNumber As String
            Dim savedNotes As String

            Using con As MySqlConnection =
                DbConnection.GetConnection()

                con.Open()

                Using cmd As New MySqlCommand(
                    "SELECT room_number, room_type_id, " &
                    "room_type_option_id, floor_number, status, notes " &
                    "FROM rooms WHERE room_id = @id",
                    con)

                    cmd.Parameters.AddWithValue("@id", currentRoomId)

                    Using reader As MySqlDataReader =
                        cmd.ExecuteReader()

                        If Not reader.Read() Then
                            MessageBox.Show(
                                "The selected room no longer exists.",
                                "Room Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                            Return
                        End If

                        savedNumber =
                            Convert.ToString(reader("room_number"))

                        savedTypeId =
                            Convert.ToInt32(reader("room_type_id"))

                        savedOptionId = If(
                            reader.IsDBNull(
                                reader.GetOrdinal("room_type_option_id")),
                            0,
                            Convert.ToInt32(
                                reader("room_type_option_id")))

                        savedFloor =
                            Convert.ToInt32(reader("floor_number"))

                        savedStatus =
                            Convert.ToString(reader("status")).
                            ToUpperInvariant()

                        savedNotes = If(
                            reader.IsDBNull(reader.GetOrdinal("notes")),
                            "",
                            Convert.ToString(reader("notes")))
                    End Using
                End Using
            End Using

            isLoading = True

            nudFloor.Value = Math.Max(
                CDec(nudFloor.Minimum),
                Math.Min(CDec(nudFloor.Maximum), CDec(savedFloor)))

            Dim typeFound As Boolean = False

            For i As Integer = 0 To roomTypesTable.Rows.Count - 1
                If Convert.ToInt32(
                    roomTypesTable.Rows(i)("room_type_id")) =
                    savedTypeId Then

                    cboRoomType.SelectedIndex = i
                    typeFound = True
                    Exit For
                End If
            Next

            isLoading = False

            If Not typeFound Then
                MessageBox.Show(
                    "The room's saved room type no longer exists. " &
                    "Choose a valid room type before saving.",
                    "Room Type Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Return
            End If

            LoadBedSizes(savedOptionId)

            isLoading = True

            If Not cboStatus.Items.Contains(savedStatus) Then
                cboStatus.Items.Add(savedStatus)
            End If

            cboStatus.SelectedItem = savedStatus
            txtNotes.Text = savedNotes
            txtRoomNumber.Text = savedNumber

            isLoading = False

            UpdatePricing()

            lblNumberHelp.Text =
                "Existing room number. Changing the floor or " &
                "room type may generate a different number."

            roomLoaded = True

        Catch ex As Exception
            isLoading = False

            MessageBox.Show(
                "Unable to load the room." &
                Environment.NewLine & ex.Message,
                "StayPoint",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    '========================================================
    ' VALIDATE ROOM NUMBER
    '========================================================

    Private Function ValidateRoomNumber() As Boolean
        Dim number As String = txtRoomNumber.Text.Trim()

        If number.Length <> 3 Then Return False

        For Each character As Char In number
            If Not Char.IsDigit(character) Then Return False
        Next

        Dim typeCode As Integer =
            GetRoomTypeCode(cboRoomType.Text)

        If typeCode < 0 Then Return False

        Return Convert.ToInt32(number.Substring(0, 1)) =
                   Convert.ToInt32(nudFloor.Value) AndAlso
               Convert.ToInt32(number.Substring(1, 1)) =
                   typeCode AndAlso
               Convert.ToInt32(number.Substring(2, 1)) >= 1
    End Function

    '========================================================
    ' SAVE ROOM
    '========================================================

    Private Sub SaveRoom()
        If Not roomLoaded Then
            MessageBox.Show(
                "The room information did not load correctly. " &
                "Close this window and try again.",
                "Cannot Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Dim typeId As Integer = SelectedTypeId()
        Dim optionId As Integer = SelectedOptionId()

        If typeId <= 0 Then
            MessageBox.Show(
                "Select a room type.",
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If optionId <= 0 Then
            MessageBox.Show(
                "Select a bed size with a configured pricing option. " &
                "Add or activate the option in System Settings first.",
                "Pricing Option Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Dim selectedOption As DataRow = SelectedOptionRow()

        If selectedOption Is Nothing OrElse
           Convert.ToInt32(selectedOption("room_type_id")) <> typeId Then

            MessageBox.Show(
                "The selected bed size does not belong to the " &
                "selected room type.",
                "Invalid Pricing Option",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        If Not ValidateRoomNumber() Then
            MessageBox.Show(
                "The room number does not match the selected floor " &
                "and room type. Select the floor or room type again.",
                "Invalid Room Number",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using con As MySqlConnection =
                DbConnection.GetConnection()

                con.Open()

                Using transaction As MySqlTransaction =
                    con.BeginTransaction()

                    ' Confirm that the room type still exists.
                    Using typeCmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM room_types " &
                        "WHERE room_type_id = @typeId",
                        con, transaction)

                        typeCmd.Parameters.AddWithValue(
                            "@typeId", typeId)

                        If Convert.ToInt32(
                            typeCmd.ExecuteScalar()) = 0 Then

                            Throw New Exception(
                                "The selected room type no longer exists.")
                        End If
                    End Using

                    ' Verify the selected pricing option is valid.
                    ' An inactive option is allowed only when retaining
                    ' the option already assigned to this room.
                    Using optionCmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM room_type_options " &
                        "WHERE option_id = @optionId " &
                        "AND room_type_id = @typeId " &
                        "AND (is_active = 1 OR " &
                        "(option_id = @oldOptionId AND @roomId > 0))",
                        con, transaction)

                        optionCmd.Parameters.AddWithValue(
                            "@optionId", optionId)

                        optionCmd.Parameters.AddWithValue(
                            "@typeId", typeId)

                        optionCmd.Parameters.AddWithValue(
                            "@oldOptionId", savedOptionId)

                        optionCmd.Parameters.AddWithValue(
                            "@roomId", currentRoomId)

                        If Convert.ToInt32(
                            optionCmd.ExecuteScalar()) = 0 Then

                            Throw New Exception(
                                "This pricing option is no longer active. " &
                                "Choose an active bed-size option.")
                        End If
                    End Using

                    ' Check room number duplication inside the transaction.
                    Using checkCmd As New MySqlCommand(
                        "SELECT COUNT(*) FROM rooms " &
                        "WHERE room_number = @number " &
                        "AND room_id <> @id",
                        con, transaction)

                        checkCmd.Parameters.AddWithValue(
                            "@number", txtRoomNumber.Text.Trim())

                        checkCmd.Parameters.AddWithValue(
                            "@id", currentRoomId)

                        If Convert.ToInt32(
                            checkCmd.ExecuteScalar()) > 0 Then

                            Throw New Exception(
                                "DUPLICATE_ROOM_NUMBER")
                        End If
                    End Using

                    Dim sql As String

                    If currentRoomId = 0 Then
                        sql =
                            "INSERT INTO rooms " &
                            "(room_number, room_type_id, " &
                            "room_type_option_id, floor_number, status, notes) " &
                            "VALUES (@number, @typeId, @optionId, " &
                            "@floor, @status, @notes)"
                    Else
                        sql =
                            "UPDATE rooms SET " &
                            "room_number = @number, " &
                            "room_type_id = @typeId, " &
                            "room_type_option_id = @optionId, " &
                            "floor_number = @floor, " &
                            "status = @status, notes = @notes " &
                            "WHERE room_id = @id"
                    End If
                    If currentRoomId > 0 Then
                        Dim currentDbStatus As String = ""

                        Using statusCmd As New MySqlCommand(
        "SELECT status FROM rooms " &
        "WHERE room_id = @id FOR UPDATE",
        con, transaction
    )
                            statusCmd.Parameters.AddWithValue("@id", currentRoomId)

                            Dim statusValue As Object = statusCmd.ExecuteScalar()

                            If statusValue Is Nothing OrElse statusValue Is DBNull.Value Then
                                Throw New InvalidOperationException(
                "This room no longer exists. Refresh the room list."
            )
                            End If

                            currentDbStatus =
            Convert.ToString(statusValue).Trim().ToUpperInvariant()
                        End Using

                        If currentDbStatus = "OCCUPIED" AndAlso
       Not cboStatus.Text.Trim().Equals(
           "OCCUPIED",
           StringComparison.OrdinalIgnoreCase
       ) Then

                            Throw New InvalidOperationException(
            "An occupied room cannot be marked available, cleaned, " &
            "or placed under maintenance through the room editor. " &
            "Complete the checkout workflow first."
        )
                        End If
                    End If
                    Using cmd As New MySqlCommand(
                        sql, con, transaction)

                        cmd.Parameters.AddWithValue(
                            "@number", txtRoomNumber.Text.Trim())

                        cmd.Parameters.AddWithValue(
                            "@typeId", typeId)

                        cmd.Parameters.AddWithValue(
                            "@optionId", optionId)

                        cmd.Parameters.AddWithValue(
                            "@floor", Convert.ToInt32(nudFloor.Value))

                        cmd.Parameters.AddWithValue(
                            "@status", cboStatus.Text)

                        cmd.Parameters.AddWithValue(
                            "@notes",
                            If(
                                String.IsNullOrWhiteSpace(txtNotes.Text),
                                CType(DBNull.Value, Object),
                                txtNotes.Text.Trim()))

                        If currentRoomId > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@id", currentRoomId)
                        End If

                        Dim affected As Integer =
                            cmd.ExecuteNonQuery()

                        If currentRoomId > 0 AndAlso affected = 0 Then
                            Throw New Exception(
                                "The room no longer exists or no changes were saved.")
                        End If
                    End Using

                    transaction.Commit()
                End Using
            End Using

            MessageBox.Show(
                "Room saved successfully.",
                "StayPoint",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            DialogResult = DialogResult.OK
            Close()

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show(
                    "That room number already exists. " &
                    "Choose another floor or room type.",
                    "Duplicate Room",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                MessageBox.Show(
                    "Unable to save the room." &
                    Environment.NewLine & ex.Message,
                    "StayPoint",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            If ex.Message = "DUPLICATE_ROOM_NUMBER" Then
                MessageBox.Show(
                    "That room number already exists. " &
                    "Choose another floor or room type.",
                    "Duplicate Room",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                MessageBox.Show(
                    "Unable to save the room." &
                    Environment.NewLine & ex.Message,
                    "StayPoint",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End If
        End Try
    End Sub

End Class