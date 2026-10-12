
Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ucSystemSettings
    Inherits UserControl

    '========================================================
    ' COLORS
    '========================================================

    Private ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Private ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
    Private ReadOnly Teal As Color = Color.FromArgb(13, 148, 136)
    Private ReadOnly BgColor As Color = Color.FromArgb(240, 244, 248)
    Private ReadOnly TextColor As Color = Color.FromArgb(51, 65, 85)
    Private ReadOnly BorderColor As Color = Color.FromArgb(220, 228, 237)

    '========================================================
    ' GENERAL SETTINGS
    '========================================================

    Private txtHotelName As TextBox
    Private txtAddress As TextBox
    Private txtPhone As TextBox
    Private txtEmail As TextBox
    Private txtCheckIn As TextBox
    Private txtCheckOut As TextBox
    Private btnSaveGeneral As Button

    '========================================================
    ' ROOM PRICING
    '========================================================

    Private dgvRooms As DataGridView
    Private cboRoomType As ComboBox
    Private cboBedSize As ComboBox
    Private txtRoomHourly As TextBox
    Private txtRoomAdditionalHourly As TextBox
    Private txtRoomDaily As TextBox
    Private txtRoomMinimumHours As TextBox
    Private btnSaveRoom As Button
    Private btnCancelRoom As Button
    Private btnDeleteRoom As Button
    Private selectedRoomOptionId As Integer

    '========================================================
    ' PARKING PRICING
    '========================================================

    Private dgvParking As DataGridView
    Private cboVehicleType As ComboBox
    Private cboParkingType As ComboBox
    Private txtParkingHourly As TextBox
    Private btnSaveParking As Button
    Private btnCancelParking As Button
    Private btnDeleteParking As Button
    Private selectedParkingRateId As Integer

    Private isLoadingRoomTypes As Boolean = False

    '========================================================
    ' CONSTRUCTOR
    '========================================================

    Public Sub New()
        InitializeComponent()
        BuildInterface()

        Try
            LoadGeneralSettings()
            LoadRoomTypes()
            LoadRoomPricing()
            LoadParkingPricing()

            ResetRoomEditor()
            ResetParkingEditor()

        Catch ex As Exception
            MessageBox.Show(
                "Unable to load system settings." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "StayPoint Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    '========================================================
    ' MAIN INTERFACE
    '========================================================

    Private Sub BuildInterface()

        Me.Controls.Clear()
        Me.BackColor = BgColor
        Me.Dock = DockStyle.Fill

        Dim main As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = BgColor,
            .Padding = New Padding(20)
        }

        main.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
        main.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Me.Controls.Add(main)

        Dim header As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = BgColor
        }

        Dim lblTitle As New Label With {
            .Text = "System Settings",
            .Font = New Font("Segoe UI", 19, FontStyle.Bold),
            .ForeColor = Navy,
            .AutoSize = True,
            .Location = New Point(0, 0)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Manage hotel information and configured pricing.",
            .Font = New Font("Segoe UI", 9),
            .ForeColor = TextColor,
            .AutoSize = True,
            .Location = New Point(2, 36)
        }

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblSubtitle)
        main.Controls.Add(header, 0, 0)

        Dim tabs As New TabControl With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
        }

        tabs.TabPages.Add(CreateGeneralTab())
        tabs.TabPages.Add(CreateRoomTab())
        tabs.TabPages.Add(CreateParkingTab())

        main.Controls.Add(tabs, 0, 1)

    End Sub

    '========================================================
    ' GENERAL SETTINGS TAB
    '========================================================

    Private Function CreateGeneralTab() As TabPage

        Dim page As New TabPage("Hotel Information") With {
            .BackColor = BgColor,
            .Padding = New Padding(15)
        }

        Dim panel As New Panel With {
            .Dock = DockStyle.Fill,
            .AutoScroll = True,
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }

        page.Controls.Add(panel)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .ColumnCount = 2,
            .RowCount = 7,
            .Padding = New Padding(5)
        }

        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        panel.Controls.Add(layout)

        txtHotelName = CreateTextBox()
        txtAddress = CreateTextBox()
        txtPhone = CreateTextBox()
        txtEmail = CreateTextBox()
        txtCheckIn = CreateTextBox()
        txtCheckOut = CreateTextBox()

        AddSettingRow(layout, 0, "Hotel Name", txtHotelName)
        AddSettingRow(layout, 1, "Address", txtAddress)
        AddSettingRow(layout, 2, "Phone Number", txtPhone)
        AddSettingRow(layout, 3, "Email Address", txtEmail)
        AddSettingRow(layout, 4, "Check-in Time", txtCheckIn)
        AddSettingRow(layout, 5, "Check-out Time", txtCheckOut)

        btnSaveGeneral = CreateButton("Save Hotel Settings")
        AddSettingRow(layout, 6, "", btnSaveGeneral)

        AddHandler btnSaveGeneral.Click, AddressOf SaveGeneralSettings

        Return page

    End Function

    '========================================================
    ' ROOM PRICING TAB
    '========================================================

    Private Function CreateRoomTab() As TabPage

        Dim page As New TabPage("Room Pricing") With {
            .BackColor = BgColor,
            .Padding = New Padding(10)
        }

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = BgColor
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 195))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        page.Controls.Add(root)

        Dim editor As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12)
        }

        root.Controls.Add(editor, 0, 0)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 3
        }

        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 115))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        editor.Controls.Add(layout)

        cboRoomType = CreateComboBox()
        cboBedSize = CreateComboBox()

        cboRoomType.DropDownStyle = ComboBoxStyle.DropDownList
        cboBedSize.DropDownStyle = ComboBoxStyle.DropDownList

        txtRoomHourly = CreateTextBox()
        txtRoomAdditionalHourly = CreateTextBox()
        txtRoomDaily = CreateTextBox()
        txtRoomMinimumHours = CreateTextBox()

        AddCell(layout, 0, 0, "Room Type")
        AddCell(layout, 1, 0, cboRoomType)
        AddCell(layout, 2, 0, "Bed Size")
        AddCell(layout, 3, 0, cboBedSize)

        AddCell(layout, 0, 1, "Hourly Rate (₱)")
        AddCell(layout, 1, 1, txtRoomHourly)
        AddCell(layout, 2, 1, "Extra Hour (₱)")
        AddCell(layout, 3, 1, txtRoomAdditionalHourly)

        AddCell(layout, 0, 2, "Daily Rate (₱)")
        AddCell(layout, 1, 2, txtRoomDaily)
        AddCell(layout, 2, 2, "Minimum Hours")
        AddCell(layout, 3, 2, txtRoomMinimumHours)

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 42,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(0, 4, 0, 0)
        }

        btnSaveRoom = CreateButton("Save Pricing")
        btnCancelRoom = CreateButton("Cancel")
        btnDeleteRoom = CreateButton("Delete")

        actions.Controls.Add(btnSaveRoom)
        actions.Controls.Add(btnCancelRoom)
        actions.Controls.Add(btnDeleteRoom)
        editor.Controls.Add(actions)

        AddHandler btnSaveRoom.Click, AddressOf SaveRoomPricing
        AddHandler btnCancelRoom.Click, AddressOf CancelRoomPricing
        AddHandler btnDeleteRoom.Click, AddressOf DeleteRoomPricing

        dgvRooms = CreateGrid()
        root.Controls.Add(dgvRooms, 0, 1)

        AddHandler dgvRooms.CellClick, AddressOf RoomGridCellClick

        ' FIX: connect the event so bed sizes reload when room type changes.
        AddHandler cboRoomType.SelectedIndexChanged,
            AddressOf RoomTypeChanged

        Return page

    End Function

    '========================================================
    ' PARKING PRICING TAB
    '========================================================

    Private Function CreateParkingTab() As TabPage

        Dim page As New TabPage("Parking Pricing") With {
            .BackColor = BgColor,
            .Padding = New Padding(10)
        }

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = BgColor
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 130))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        page.Controls.Add(root)

        Dim editor As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12)
        }

        root.Controls.Add(editor, 0, 0)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 75,
            .ColumnCount = 4,
            .RowCount = 2
        }

        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 125))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        editor.Controls.Add(layout)

        cboVehicleType = CreateComboBox()
        cboParkingType = CreateComboBox()

        cboVehicleType.Items.AddRange(
            New Object() {"Motorcycle", "Sedan", "SUV", "Van"})

        cboParkingType.Items.AddRange(
            New Object() {"Standard", "Covered"})

        txtParkingHourly = CreateTextBox()

        AddCell(layout, 0, 0, "Vehicle Type")
        AddCell(layout, 1, 0, cboVehicleType)
        AddCell(layout, 2, 0, "Parking Type")
        AddCell(layout, 3, 0, cboParkingType)

        AddCell(layout, 0, 1, "Hourly Rate (₱)")
        AddCell(layout, 1, 1, txtParkingHourly)

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 42,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(0, 4, 0, 0)
        }

        btnSaveParking = CreateButton("Save Pricing")
        btnCancelParking = CreateButton("Cancel")
        btnDeleteParking = CreateButton("Delete")

        actions.Controls.Add(btnSaveParking)
        actions.Controls.Add(btnCancelParking)
        actions.Controls.Add(btnDeleteParking)
        editor.Controls.Add(actions)

        AddHandler btnSaveParking.Click, AddressOf SaveParkingPricing
        AddHandler btnCancelParking.Click, AddressOf CancelParkingPricing
        AddHandler btnDeleteParking.Click, AddressOf DeleteParkingPricing

        dgvParking = CreateGrid()
        root.Controls.Add(dgvParking, 0, 1)

        AddHandler dgvParking.CellClick, AddressOf ParkingGridCellClick

        Return page

    End Function

    '========================================================
    ' UI HELPERS
    '========================================================

    Private Function CreateTextBox() As TextBox

        Return New TextBox With {
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 10),
            .BorderStyle = BorderStyle.FixedSingle,
            .Margin = New Padding(4, 5, 8, 5)
        }

    End Function

    Private Function CreateComboBox() As ComboBox

        Return New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Segoe UI", 10),
            .Margin = New Padding(4, 5, 8, 5)
        }

    End Function

    Private Function CreateButton(caption As String) As Button

        Return New Button With {
            .Text = caption,
            .AutoSize = True,
            .Height = 32,
            .BackColor = If(
                caption = "Delete",
                Color.FromArgb(185, 28, 28),
                If(caption = "Cancel", SidebarColor, Teal)),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Margin = New Padding(4)
        }

    End Function

    Private Function CreateGrid() As DataGridView

        Return New DataGridView With {
            .Dock = DockStyle.Fill,
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
            .AutoGenerateColumns = True,
            .Font = New Font("Segoe UI", 9),
            .GridColor = BorderColor,
            .EnableHeadersVisualStyles = False
        }

    End Function

    Private Sub AddSettingRow(
        layout As TableLayoutPanel,
        row As Integer,
        caption As String,
        control As Control)

        Dim lbl As New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 10),
            .ForeColor = TextColor,
            .Margin = New Padding(4)
        }

        control.Margin = New Padding(4, 7, 8, 7)

        layout.Controls.Add(lbl, 0, row)
        layout.Controls.Add(control, 1, row)
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 45))

    End Sub

    Private Sub AddCell(
        layout As TableLayoutPanel,
        column As Integer,
        row As Integer,
        value As Object)

        If TypeOf value Is String Then

            Dim lbl As New Label With {
                .Text = CStr(value),
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font = New Font("Segoe UI", 9),
                .ForeColor = TextColor,
                .Margin = New Padding(3)
            }

            layout.Controls.Add(lbl, column, row)

        Else
            layout.Controls.Add(DirectCast(value, Control), column, row)
        End If

    End Sub

    Private Sub ShowDatabaseError(action As String, ex As Exception)

        MessageBox.Show(
            action & Environment.NewLine & Environment.NewLine & ex.Message,
            "StayPoint Settings",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

    End Sub

    '========================================================
    ' GENERAL SETTINGS - LOAD
    '========================================================

    Private Sub LoadGeneralSettings()

        Using conn As MySqlConnection = DbConnection.GetConnection()

            conn.Open()

            Dim values As New Dictionary(Of String, String)(
                StringComparer.OrdinalIgnoreCase)

            Using cmd As New MySqlCommand(
                "SELECT setting_key, setting_value FROM app_settings", conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()

                    While reader.Read()

                        values(reader("setting_key").ToString()) =
                            If(reader.IsDBNull(1), "", reader("setting_value").ToString())

                    End While

                End Using
            End Using

            txtHotelName.Text = GetSetting(values, "hotel_name", "StayPoint Hotel")
            txtAddress.Text = GetSetting(values, "hotel_address")
            txtPhone.Text = GetSetting(values, "hotel_phone")
            txtEmail.Text = GetSetting(values, "hotel_email")
            txtCheckIn.Text = GetSetting(values, "check_in_time", "14:00")
            txtCheckOut.Text = GetSetting(values, "check_out_time", "12:00")

        End Using

    End Sub

    Private Function GetSetting(
        values As Dictionary(Of String, String),
        key As String,
        Optional defaultValue As String = "") As String

        If values.ContainsKey(key) Then Return values(key)
        Return defaultValue

    End Function

    Private Sub SaveGeneralSettings(sender As Object, e As EventArgs)

        If String.IsNullOrWhiteSpace(txtHotelName.Text) Then
            MessageBox.Show("Please enter the hotel name.", "Validation",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtHotelName.Focus()
            Return
        End If

        If Not IsValidTime(txtCheckIn.Text) OrElse
           Not IsValidTime(txtCheckOut.Text) Then

            MessageBox.Show(
                "Enter valid times, for example 14:00 and 12:00.",
                "Invalid Time",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Using transaction As MySqlTransaction = conn.BeginTransaction()

                    SaveSetting(conn, transaction, "hotel_name", txtHotelName.Text.Trim())
                    SaveSetting(conn, transaction, "hotel_address", txtAddress.Text.Trim())
                    SaveSetting(conn, transaction, "hotel_phone", txtPhone.Text.Trim())
                    SaveSetting(conn, transaction, "hotel_email", txtEmail.Text.Trim())
                    SaveSetting(conn, transaction, "check_in_time", NormalizeTime(txtCheckIn.Text))
                    SaveSetting(conn, transaction, "check_out_time", NormalizeTime(txtCheckOut.Text))

                    transaction.Commit()

                End Using
            End Using

            MessageBox.Show("Hotel settings saved successfully.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            ShowDatabaseError("Unable to save hotel settings.", ex)
        End Try

    End Sub

    Private Sub SaveSetting(
        conn As MySqlConnection,
        transaction As MySqlTransaction,
        settingKey As String,
        settingValue As String)

        Using cmd As New MySqlCommand(
            "INSERT INTO app_settings (setting_key, setting_value) " &
            "VALUES (@key, @value) " &
            "ON DUPLICATE KEY UPDATE setting_value = @value",
            conn, transaction)

            cmd.Parameters.AddWithValue("@key", settingKey)
            cmd.Parameters.AddWithValue("@value", settingValue)
            cmd.ExecuteNonQuery()

        End Using

    End Sub

    Private Function IsValidTime(value As String) As Boolean

        Dim parsed As DateTime

        Return DateTime.TryParseExact(
            value.Trim(),
            New String() {"HH:mm", "H:mm", "hh:mm tt", "h:mm tt"},
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            parsed)

    End Function

    Private Function NormalizeTime(value As String) As String

        Dim parsed As DateTime

        DateTime.TryParseExact(
            value.Trim(),
            New String() {"HH:mm", "H:mm", "hh:mm tt", "h:mm tt"},
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            parsed)

        Return parsed.ToString("HH:mm", CultureInfo.InvariantCulture)

    End Function

    '========================================================
    ' ROOM TYPES
    '========================================================

    Private Class ComboItem

        Public Property Id As Integer
        Public Property Name As String

        Public Sub New(idValue As Integer, nameValue As String)
            Id = idValue
            Name = nameValue
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

    End Class

    Private Sub LoadRoomTypes()

        isLoadingRoomTypes = True

        Try
            cboRoomType.Items.Clear()

            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT room_type_id, type_name " &
                    "FROM room_types " &
                    "WHERE is_active = 1 " &
                    "ORDER BY type_name", conn)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()

                        While reader.Read()
                            cboRoomType.Items.Add(
                                New ComboItem(
                                    Convert.ToInt32(reader("room_type_id")),
                                    reader("type_name").ToString()))
                        End While

                    End Using
                End Using
            End Using

            If cboRoomType.Items.Count > 0 Then
                cboRoomType.SelectedIndex = 0
            End If

        Finally
            isLoadingRoomTypes = False
        End Try

        RoomTypeChanged(Nothing, EventArgs.Empty)

    End Sub

    '========================================================
    ' BED SIZES
    '========================================================

    Private Sub RoomTypeChanged(sender As Object, e As EventArgs)

        If isLoadingRoomTypes OrElse
           cboRoomType Is Nothing OrElse
           cboBedSize Is Nothing Then Return

        Dim selectedType As ComboItem =
            TryCast(cboRoomType.SelectedItem, ComboItem)

        If selectedType Is Nothing Then
            cboBedSize.Items.Clear()
            Return
        End If

        Try
            LoadBedSizes(selectedType.Id)

        Catch ex As Exception
            ShowDatabaseError("Unable to load bed sizes.", ex)
        End Try

    End Sub

    Private Sub LoadBedSizes(
        roomTypeId As Integer,
        Optional preferredBedSize As String = "")

        Dim bedSizes As New List(Of String)

        Using conn As MySqlConnection = DbConnection.GetConnection()

            conn.Open()

            ' Read existing options for this room type.
            Using cmd As New MySqlCommand(
                "SELECT DISTINCT TRIM(bed_size) " &
                "FROM room_type_options " &
                "WHERE room_type_id = @roomTypeId " &
                "AND bed_size IS NOT NULL " &
                "AND TRIM(bed_size) <> '' " &
                "ORDER BY TRIM(bed_size)", conn)

                cmd.Parameters.AddWithValue("@roomTypeId", roomTypeId)

                Using reader As MySqlDataReader = cmd.ExecuteReader()

                    While reader.Read()

                        Dim size As String = reader.GetString(0).Trim()

                        If Not bedSizes.Exists(
                            Function(x) String.Equals(
                                x, size, StringComparison.OrdinalIgnoreCase)) Then

                            bedSizes.Add(size)
                        End If

                    End While
                End Using
            End Using

            ' Include the legacy bed size defined on room_types.
            Using cmd As New MySqlCommand(
                "SELECT TRIM(bed_size) " &
                "FROM room_types " &
                "WHERE room_type_id = @roomTypeId " &
                "AND bed_size IS NOT NULL " &
                "AND TRIM(bed_size) <> ''", conn)

                cmd.Parameters.AddWithValue("@roomTypeId", roomTypeId)

                Dim result As Object = cmd.ExecuteScalar()

                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then

                    Dim size As String = result.ToString().Trim()

                    If Not bedSizes.Exists(
                        Function(x) String.Equals(
                            x, size, StringComparison.OrdinalIgnoreCase)) Then

                        bedSizes.Add(size)
                    End If

                End If
            End Using

        End Using

        cboBedSize.BeginUpdate()

        Try
            cboBedSize.Items.Clear()

            For Each size As String In bedSizes
                cboBedSize.Items.Add(size)
            Next

            If preferredBedSize <> "" Then

                For i As Integer = 0 To cboBedSize.Items.Count - 1

                    If String.Equals(
                        cboBedSize.Items(i).ToString(),
                        preferredBedSize,
                        StringComparison.OrdinalIgnoreCase) Then

                        cboBedSize.SelectedIndex = i
                        Exit For
                    End If

                Next

            End If

        Finally
            cboBedSize.EndUpdate()
        End Try

    End Sub

    '========================================================
    ' ROOM PRICING - LOAD
    '========================================================

    Private Sub LoadRoomPricing()

        Dim dt As New DataTable()

        Using conn As MySqlConnection = DbConnection.GetConnection()

            conn.Open()

            Dim sql As String =
                "SELECT o.option_id, o.room_type_id, " &
                "rt.type_name AS `Room Type`, " &
                "o.bed_size AS `Bed Size`, " &
                "o.hourly_rate AS `Hourly Rate`, " &
                "o.additional_hourly_rate AS `Extra Hour Rate`, " &
                "o.daily_rate AS `Daily Rate`, " &
                "o.minimum_hours AS `Minimum Hours`, " &
                "o.is_active AS `Active` " &
                "FROM room_type_options o " &
                "INNER JOIN room_types rt ON rt.room_type_id = o.room_type_id " &
                "ORDER BY rt.type_name, o.bed_size"

            Using adapter As New MySqlDataAdapter(sql, conn)
                adapter.Fill(dt)
            End Using

        End Using

        dgvRooms.DataSource = dt

        If dgvRooms.Columns.Contains("option_id") Then
            dgvRooms.Columns("option_id").Visible = False
        End If

        If dgvRooms.Columns.Contains("room_type_id") Then
            dgvRooms.Columns("room_type_id").Visible = False
        End If

        If dgvRooms.Columns.Contains("Active") Then
            dgvRooms.Columns("Active").HeaderText = "Available"
        End If

    End Sub

    '========================================================
    ' ROOM PRICING - VALIDATION
    '========================================================

    Private Function TryGetRoomValues(
        ByRef roomTypeId As Integer,
        ByRef bedSize As String,
        ByRef hourlyRate As Decimal,
        ByRef extraRate As Decimal,
        ByRef dailyRate As Decimal,
        ByRef minimumHours As Integer) As Boolean

        If cboRoomType.SelectedItem Is Nothing Then
            MessageBox.Show("Please select an active room type.",
                            "Validation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return False
        End If

        If cboBedSize.SelectedItem Is Nothing Then
            MessageBox.Show("Select a bed size from the configured options.",
                            "Validation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            cboBedSize.Focus()
            Return False
        End If

        roomTypeId = DirectCast(cboRoomType.SelectedItem, ComboItem).Id
        bedSize = cboBedSize.SelectedItem.ToString().Trim()

        If bedSize.Length > 50 Then
            MessageBox.Show("Bed size cannot exceed 50 characters.")
            Return False
        End If

        If Not Decimal.TryParse(
            txtRoomHourly.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            hourlyRate) OrElse hourlyRate < 0 Then

            MessageBox.Show("Enter a valid non-negative hourly rate.")
            txtRoomHourly.Focus()
            Return False
        End If

        If Not Decimal.TryParse(
            txtRoomAdditionalHourly.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            extraRate) OrElse extraRate < 0 Then

            MessageBox.Show("Enter a valid non-negative extra-hour rate.")
            txtRoomAdditionalHourly.Focus()
            Return False
        End If

        If Not Decimal.TryParse(
            txtRoomDaily.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            dailyRate) OrElse dailyRate < 0 Then

            MessageBox.Show("Enter a valid non-negative daily rate.")
            txtRoomDaily.Focus()
            Return False
        End If

        If Not Integer.TryParse(
            txtRoomMinimumHours.Text.Trim(), minimumHours) OrElse
            minimumHours < 1 Then

            MessageBox.Show("Minimum hours must be a whole number of at least 1.")
            txtRoomMinimumHours.Focus()
            Return False
        End If

        Return True

    End Function

    '========================================================
    ' ROOM PRICING - DUPLICATE CHECK
    '========================================================

    Private Function RoomOptionExists(
        conn As MySqlConnection,
        transaction As MySqlTransaction,
        roomTypeId As Integer,
        bedSize As String,
        excludedOptionId As Integer) As Boolean

        Using cmd As New MySqlCommand(
            "SELECT COUNT(*) FROM room_type_options " &
            "WHERE room_type_id = @typeId " &
            "AND LOWER(TRIM(bed_size)) = LOWER(TRIM(@bedSize)) " &
            "AND option_id <> @optionId",
            conn, transaction)

            cmd.Parameters.AddWithValue("@typeId", roomTypeId)
            cmd.Parameters.AddWithValue("@bedSize", bedSize)
            cmd.Parameters.AddWithValue("@optionId", excludedOptionId)

            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0

        End Using

    End Function

    '========================================================
    ' ROOM PRICING - SAVE / UPDATE
    '========================================================

    Private Sub SaveRoomPricing(sender As Object, e As EventArgs)

        Dim roomTypeId As Integer
        Dim bedSize As String = ""
        Dim hourlyRate As Decimal
        Dim extraRate As Decimal
        Dim dailyRate As Decimal
        Dim minimumHours As Integer

        If Not TryGetRoomValues(
            roomTypeId, bedSize, hourlyRate,
            extraRate, dailyRate, minimumHours) Then Return

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Using transaction As MySqlTransaction = conn.BeginTransaction()

                    If RoomOptionExists(
                        conn, transaction, roomTypeId,
                        bedSize, selectedRoomOptionId) Then

                        transaction.Rollback()

                        MessageBox.Show(
                            "This room type and bed size combination already exists.",
                            "Duplicate Room Pricing",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
                        Return
                    End If

                    Dim sql As String

                    If selectedRoomOptionId = 0 Then

                        sql =
                            "INSERT INTO room_type_options " &
                            "(room_type_id, bed_size, hourly_rate, " &
                            "additional_hourly_rate, daily_rate, minimum_hours, is_active) " &
                            "VALUES (@typeId, @bed, @hourly, @extra, @daily, @minimum, 1)"

                    Else

                        sql =
                            "UPDATE room_type_options SET " &
                            "room_type_id = @typeId, " &
                            "bed_size = @bed, " &
                            "hourly_rate = @hourly, " &
                            "additional_hourly_rate = @extra, " &
                            "daily_rate = @daily, " &
                            "minimum_hours = @minimum " &
                            "WHERE option_id = @id"

                    End If

                    Using cmd As New MySqlCommand(sql, conn, transaction)

                        cmd.Parameters.AddWithValue("@typeId", roomTypeId)
                        cmd.Parameters.AddWithValue("@bed", bedSize)
                        cmd.Parameters.AddWithValue("@hourly", hourlyRate)
                        cmd.Parameters.AddWithValue("@extra", extraRate)
                        cmd.Parameters.AddWithValue("@daily", dailyRate)
                        cmd.Parameters.AddWithValue("@minimum", minimumHours)

                        If selectedRoomOptionId <> 0 Then
                            cmd.Parameters.AddWithValue("@id", selectedRoomOptionId)
                        End If

                        If cmd.ExecuteNonQuery() = 0 Then
                            Throw New Exception(
                                "The pricing record was not updated. Refresh the list and try again.")
                        End If

                    End Using

                    transaction.Commit()

                End Using
            End Using

            LoadRoomPricing()
            ResetRoomEditor()

            MessageBox.Show("Room pricing saved successfully.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As MySqlException

            If ex.Number = 1062 Then
                MessageBox.Show(
                    "This room type and bed size combination already exists.",
                    "Duplicate Room Pricing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                ShowDatabaseError("Unable to save room pricing.", ex)
            End If

        Catch ex As Exception
            ShowDatabaseError("Unable to save room pricing.", ex)
        End Try

    End Sub

    '========================================================
    ' ROOM PRICING - SELECT / RESET
    '========================================================

    Private Sub RoomGridCellClick(sender As Object, e As DataGridViewCellEventArgs)

        If e.RowIndex < 0 Then Return

        Dim row As DataGridViewRow = dgvRooms.Rows(e.RowIndex)

        selectedRoomOptionId =
            Convert.ToInt32(row.Cells("option_id").Value)

        Dim roomTypeId As Integer =
            Convert.ToInt32(row.Cells("room_type_id").Value)

        For Each item As Object In cboRoomType.Items

            Dim comboItem As ComboItem = DirectCast(item, ComboItem)

            If comboItem.Id = roomTypeId Then
                cboRoomType.SelectedItem = item
                Exit For
            End If

        Next

        LoadBedSizes(
            roomTypeId,
            Convert.ToString(row.Cells("Bed Size").Value))

        txtRoomHourly.Text =
            Convert.ToDecimal(row.Cells("Hourly Rate").Value).ToString("0.00")

        txtRoomAdditionalHourly.Text =
            Convert.ToDecimal(row.Cells("Extra Hour Rate").Value).ToString("0.00")

        txtRoomDaily.Text =
            Convert.ToDecimal(row.Cells("Daily Rate").Value).ToString("0.00")

        txtRoomMinimumHours.Text =
            Convert.ToString(row.Cells("Minimum Hours").Value)

        btnSaveRoom.Text = "Update Pricing"

    End Sub

    Private Sub CancelRoomPricing(sender As Object, e As EventArgs)
        ResetRoomEditor()
    End Sub

    Private Sub ResetRoomEditor()

        selectedRoomOptionId = 0

        If cboRoomType IsNot Nothing AndAlso
           cboRoomType.SelectedItem IsNot Nothing Then

            Dim selectedType As ComboItem =
                DirectCast(cboRoomType.SelectedItem, ComboItem)

            LoadBedSizes(selectedType.Id)
        End If

        If cboBedSize IsNot Nothing Then
            cboBedSize.SelectedIndex = -1
        End If

        If txtRoomHourly IsNot Nothing Then txtRoomHourly.Text = "0.00"
        If txtRoomAdditionalHourly IsNot Nothing Then txtRoomAdditionalHourly.Text = "0.00"
        If txtRoomDaily IsNot Nothing Then txtRoomDaily.Text = "0.00"
        If txtRoomMinimumHours IsNot Nothing Then txtRoomMinimumHours.Text = "1"

        If btnSaveRoom IsNot Nothing Then btnSaveRoom.Text = "Save Pricing"
        If dgvRooms IsNot Nothing Then dgvRooms.ClearSelection()

    End Sub

    '========================================================
    ' ROOM PRICING - DELETE
    '========================================================

    Private Sub DeleteRoomPricing(sender As Object, e As EventArgs)

        If selectedRoomOptionId <= 0 Then
            MessageBox.Show("Select a room pricing entry first.",
                            "Delete Room Pricing",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return
        End If

        Dim optionId As Integer = selectedRoomOptionId

        If MessageBox.Show(
            "Delete this room pricing option?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Using transaction As MySqlTransaction = conn.BeginTransaction()

                    Using checkRooms As New MySqlCommand(
                        "SELECT COUNT(*) FROM rooms WHERE room_type_option_id = @id",
                        conn, transaction)

                        checkRooms.Parameters.AddWithValue("@id", optionId)

                        If Convert.ToInt32(checkRooms.ExecuteScalar()) > 0 Then
                            transaction.Rollback()

                            MessageBox.Show(
                                "This pricing option is assigned to one or more rooms. " &
                                "Reassign those rooms before deleting it.",
                                "Pricing Option In Use",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                            Return
                        End If

                    End Using

                    Using cmd As New MySqlCommand(
                        "DELETE FROM room_type_options WHERE option_id = @id",
                        conn, transaction)

                        cmd.Parameters.AddWithValue("@id", optionId)

                        If cmd.ExecuteNonQuery() = 0 Then
                            transaction.Rollback()
                            Throw New Exception("The pricing option no longer exists.")
                        End If

                    End Using

                    transaction.Commit()

                End Using
            End Using

            LoadRoomPricing()
            ResetRoomEditor()

            MessageBox.Show("Room pricing deleted successfully.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As MySqlException

            If ex.Number = 1451 Then
                MessageBox.Show(
                    "This pricing option is referenced by another record and cannot be deleted.",
                    "Pricing Option In Use",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                ShowDatabaseError("Unable to delete room pricing.", ex)
            End If

        Catch ex As Exception
            ShowDatabaseError("Unable to delete room pricing.", ex)
        End Try

    End Sub

    '========================================================
    ' PARKING PRICING - LOAD
    '========================================================

    Private Sub LoadParkingPricing()

        Dim dt As New DataTable()

        Using conn As MySqlConnection = DbConnection.GetConnection()

            conn.Open()

            Dim sql As String =
                "SELECT option_id, " &
                "vehicle_type AS `Vehicle Type`, " &
                "parking_type AS `Parking Type`, " &
                "hourly_rate AS `Hourly Rate`, " &
                "is_active AS `Active` " &
                "FROM parking_rate_options " &
                "ORDER BY vehicle_type, parking_type"

            Using adapter As New MySqlDataAdapter(sql, conn)
                adapter.Fill(dt)
            End Using

        End Using

        dgvParking.DataSource = dt

        If dgvParking.Columns.Contains("option_id") Then
            dgvParking.Columns("option_id").Visible = False
        End If

        If dgvParking.Columns.Contains("Active") Then
            dgvParking.Columns("Active").HeaderText = "Available"
        End If

    End Sub

    '========================================================
    ' PARKING PRICING - DUPLICATE CHECK
    '========================================================

    Private Function ParkingRateExists(
        conn As MySqlConnection,
        transaction As MySqlTransaction,
        vehicleType As String,
        parkingType As String,
        excludedRateId As Integer) As Boolean

        Using cmd As New MySqlCommand(
            "SELECT COUNT(*) FROM parking_rate_options " &
            "WHERE LOWER(TRIM(vehicle_type)) = LOWER(TRIM(@vehicle)) " &
            "AND LOWER(TRIM(parking_type)) = LOWER(TRIM(@parking)) " &
            "AND option_id <> @id",
            conn, transaction)

            cmd.Parameters.AddWithValue("@vehicle", vehicleType)
            cmd.Parameters.AddWithValue("@parking", parkingType)
            cmd.Parameters.AddWithValue("@id", excludedRateId)

            Return Convert.ToInt32(cmd.ExecuteScalar()) > 0

        End Using

    End Function

    '========================================================
    ' PARKING PRICING - SAVE / UPDATE
    '========================================================

    Private Sub SaveParkingPricing(sender As Object, e As EventArgs)

        If cboVehicleType.SelectedItem Is Nothing OrElse
           cboParkingType.SelectedItem Is Nothing Then

            MessageBox.Show("Select both a vehicle type and parking type.",
                            "Validation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return
        End If

        Dim vehicleType As String = cboVehicleType.Text.Trim()
        Dim parkingType As String = cboParkingType.Text.Trim()
        Dim hourlyRate As Decimal

        If Not Decimal.TryParse(
            txtParkingHourly.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            hourlyRate) OrElse hourlyRate < 0 Then

            MessageBox.Show("Enter a valid non-negative parking hourly rate.",
                            "Validation", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            txtParkingHourly.Focus()
            Return
        End If

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Using transaction As MySqlTransaction = conn.BeginTransaction()

                    If ParkingRateExists(
                        conn, transaction, vehicleType,
                        parkingType, selectedParkingRateId) Then

                        transaction.Rollback()

                        MessageBox.Show(
                            "This vehicle type and parking type combination already exists.",
                            "Duplicate Parking Pricing",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
                        Return
                    End If

                    Dim sql As String

                    If selectedParkingRateId = 0 Then

                        sql =
                            "INSERT INTO parking_rate_options " &
                            "(vehicle_type, parking_type, hourly_rate, is_active) " &
                            "VALUES (@vehicle, @parking, @hourly, 1)"

                    Else

                        sql =
                            "UPDATE parking_rate_options SET " &
                            "vehicle_type = @vehicle, " &
                            "parking_type = @parking, " &
                            "hourly_rate = @hourly " &
                            "WHERE option_id = @id"

                    End If

                    Using cmd As New MySqlCommand(sql, conn, transaction)

                        cmd.Parameters.AddWithValue("@vehicle", vehicleType)
                        cmd.Parameters.AddWithValue("@parking", parkingType)
                        cmd.Parameters.AddWithValue("@hourly", hourlyRate)

                        If selectedParkingRateId <> 0 Then
                            cmd.Parameters.AddWithValue("@id", selectedParkingRateId)
                        End If

                        If cmd.ExecuteNonQuery() = 0 Then
                            Throw New Exception(
                                "The pricing record was not updated. Refresh the list and try again.")
                        End If

                    End Using

                    transaction.Commit()

                End Using
            End Using

            LoadParkingPricing()
            ResetParkingEditor()

            MessageBox.Show("Parking pricing saved successfully.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As MySqlException

            If ex.Number = 1062 Then
                MessageBox.Show(
                    "This vehicle type and parking type combination already exists.",
                    "Duplicate Parking Pricing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                ShowDatabaseError("Unable to save parking pricing.", ex)
            End If

        Catch ex As Exception
            ShowDatabaseError("Unable to save parking pricing.", ex)
        End Try

    End Sub

    '========================================================
    ' PARKING PRICING - SELECT / RESET
    '========================================================

    Private Sub ParkingGridCellClick(
        sender As Object,
        e As DataGridViewCellEventArgs)

        If e.RowIndex < 0 Then Return

        Dim row As DataGridViewRow = dgvParking.Rows(e.RowIndex)

        selectedParkingRateId =
            Convert.ToInt32(row.Cells("option_id").Value)

        cboVehicleType.Text =
            Convert.ToString(row.Cells("Vehicle Type").Value)

        cboParkingType.Text =
            Convert.ToString(row.Cells("Parking Type").Value)

        txtParkingHourly.Text =
            Convert.ToDecimal(row.Cells("Hourly Rate").Value).ToString("0.00")

        btnSaveParking.Text = "Update Pricing"

    End Sub

    Private Sub CancelParkingPricing(sender As Object, e As EventArgs)
        ResetParkingEditor()
    End Sub

    Private Sub ResetParkingEditor()

        selectedParkingRateId = 0

        If cboVehicleType IsNot Nothing AndAlso
           cboVehicleType.Items.Count > 0 Then

            cboVehicleType.SelectedIndex = 0
        End If

        If cboParkingType IsNot Nothing AndAlso
           cboParkingType.Items.Count > 0 Then

            cboParkingType.SelectedIndex = 0
        End If

        If txtParkingHourly IsNot Nothing Then
            txtParkingHourly.Text = "0.00"
        End If

        If btnSaveParking IsNot Nothing Then
            btnSaveParking.Text = "Save Pricing"
        End If

        If dgvParking IsNot Nothing Then
            dgvParking.ClearSelection()
        End If

    End Sub

    '========================================================
    ' PARKING PRICING - DELETE
    '========================================================

    Private Sub DeleteParkingPricing(sender As Object, e As EventArgs)

        If selectedParkingRateId <= 0 Then
            MessageBox.Show("Select a parking pricing entry first.",
                            "Delete Parking Pricing",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return
        End If

        Dim rateId As Integer = selectedParkingRateId

        If MessageBox.Show(
            "Delete this parking pricing entry?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                ' Check references before deleting. Existing assignments
                ' must retain their agreed rates and historical records.
                Using cmd As New MySqlCommand(
                    "SELECT COUNT(*) FROM parking_assignments " &
                    "WHERE vehicle_type = @vehicle " &
                    "AND billing_type = @parking",
                    conn)

                    cmd.Parameters.AddWithValue("@vehicle", cboVehicleType.Text.Trim())
                    cmd.Parameters.AddWithValue("@parking", cboParkingType.Text.Trim())

                    ' The current schema does not link parking_assignments
                    ' directly to parking_rate_options by option_id.
                    ' Do not use this count as a reliable FK check.
                End Using

                Using transaction As MySqlTransaction = conn.BeginTransaction()

                    Using cmd As New MySqlCommand(
                        "DELETE FROM parking_rate_options WHERE option_id = @id",
                        conn, transaction)

                        cmd.Parameters.AddWithValue("@id", rateId)

                        If cmd.ExecuteNonQuery() = 0 Then
                            transaction.Rollback()
                            Throw New Exception(
                                "The pricing entry no longer exists. Refresh the list.")
                        End If

                    End Using

                    transaction.Commit()

                End Using
            End Using

            LoadParkingPricing()
            ResetParkingEditor()

            MessageBox.Show("Parking pricing deleted successfully.", "StayPoint",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As MySqlException

            If ex.Number = 1451 Then
                MessageBox.Show(
                    "This pricing entry is referenced by another record and cannot be deleted.",
                    "Pricing Option In Use",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            Else
                ShowDatabaseError("Unable to delete parking pricing.", ex)
            End If

        Catch ex As Exception
            ShowDatabaseError("Unable to delete parking pricing.", ex)
        End Try

    End Sub

End Class
