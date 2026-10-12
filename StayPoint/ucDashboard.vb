
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting
Imports MySql.Data.MySqlClient

Public Class ucDashboard
    Private Sub ucDashboard_Load(
    sender As Object,
    e As EventArgs
) Handles MyBase.Load

        LoadRoomSummary()

    End Sub
    '========================================================
    ' LOAD ROOM SUMMARY FROM MYSQL
    '========================================================
    Private Sub LoadRoomSummary()

        Try
            Using conn As MySqlConnection = DbConnection.GetConnection()

                conn.Open()

                Dim totalRooms As Integer = 0
                Dim availableRooms As Integer = 0
                Dim occupiedRooms As Integer = 0
                Dim maintenanceRooms As Integer = 0

                Dim sql As String =
                "SELECT " &
                "COUNT(*) AS total_rooms, " &
                "COALESCE(SUM(CASE WHEN UPPER(status) = 'AVAILABLE' THEN 1 ELSE 0 END), 0) AS available_rooms, " &
                "COALESCE(SUM(CASE WHEN UPPER(status) = 'OCCUPIED' THEN 1 ELSE 0 END), 0) AS occupied_rooms, " &
                "COALESCE(SUM(CASE WHEN UPPER(status) = 'MAINTENANCE' THEN 1 ELSE 0 END), 0) AS maintenance_rooms " &
                "FROM rooms"

                Using cmd As New MySqlCommand(sql, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()

                        If reader.Read() Then
                            totalRooms = Convert.ToInt32(reader("total_rooms"))
                            availableRooms = Convert.ToInt32(reader("available_rooms"))
                            occupiedRooms = Convert.ToInt32(reader("occupied_rooms"))
                            maintenanceRooms = Convert.ToInt32(reader("maintenance_rooms"))
                        End If

                    End Using
                End Using

                ' Update the dashboard's existing room summary and chart.
                UpdateOccupancy(occupiedRooms, availableRooms)

                ' Update any dashboard labels/cards that use these control names.
                UpdateNamedLabel("lblTotalRooms", totalRooms.ToString())
                UpdateNamedLabel("lblAvailableRooms", availableRooms.ToString())
                UpdateNamedLabel("lblOccupiedRooms", occupiedRooms.ToString())
                UpdateNamedLabel("lblMaintenanceRooms", maintenanceRooms.ToString())

            End Using

        Catch ex As Exception
            MessageBox.Show(
            "Unable to load room summary." &
            Environment.NewLine & ex.Message,
            "StayPoint HMS",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )
        End Try

    End Sub

    '========================================================
    ' UPDATE A LABEL BY CONTROL NAME
    '========================================================
    Private Sub UpdateNamedLabel(controlName As String, value As String)

        For Each ctrl As Control In GetAllControls(Me)

            If String.Equals(
            ctrl.Name,
            controlName,
            StringComparison.OrdinalIgnoreCase
        ) AndAlso TypeOf ctrl Is Label Then

                ctrl.Text = value
                Return

            End If

        Next

    End Sub

    Private Iterator Function GetAllControls(
    parent As Control
) As IEnumerable(Of Control)

        For Each child As Control In parent.Controls

            Yield child

            For Each nested As Control In GetAllControls(child)
                Yield nested
            Next

        Next

    End Function

    '========================================================
    ' COLORS
    '========================================================

    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly SidebarColor As Color =
        Color.FromArgb(32, 58, 95)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly BgColor As Color =
        Color.FromArgb(240, 244, 248)

    Private ReadOnly CardColor As Color = Color.White

    Private ReadOnly BorderColor As Color =
        Color.FromArgb(220, 226, 233)

    '========================================================
    ' DYNAMICALLY CREATED CONTROLS
    '
    ' pnlOccupancy, pnlActivity, pnlQuickActions,
    ' chartOccupancy and Panel1 are in Designer.vb.
    '========================================================

    Private pnlSummary As System.Windows.Forms.Panel
    Private pnlActivityList As System.Windows.Forms.Panel
    Private pnlQuickActionButtons As System.Windows.Forms.Panel

    Private lblOccupancyPercent As System.Windows.Forms.Label
    Private lblOccupancyDetails As System.Windows.Forms.Label
    Private lblActivityEmpty As System.Windows.Forms.Label

    Private lblTotalRoomsValue As System.Windows.Forms.Label
    Private lblOccupiedRoomsValue As System.Windows.Forms.Label
    Private lblAvailableRoomsValue As System.Windows.Forms.Label
    Private lblPendingBookingsValue As System.Windows.Forms.Label

    Private Const SummaryHeight As Integer = 118
    Private Const SummaryGap As Integer = 12
    Private Const ContentGap As Integer = 16

    Public Sub New()

        InitializeComponent()
        BuildDashboard()

    End Sub

    '========================================================
    ' MAIN DASHBOARD
    '========================================================

    Private Sub BuildDashboard()

        Me.SuspendLayout()

        Try

            Me.Controls.Clear()

            Me.Dock = DockStyle.Fill
            Me.BackColor = BgColor
            Me.AutoScroll = False
            Me.MinimumSize = New Size(700, 500)
            Me.Font = New Font("Segoe UI", 9.0F)

            Dim root As New TableLayoutPanel With {
                .Name = "tblDashboard",
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = BgColor,
                .Padding = New Padding(20),
                .Margin = Padding.Empty
            }

            root.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100.0F)
            )

            root.RowStyles.Add(
                New RowStyle(
                    SizeType.Absolute,
                    SummaryHeight + 12
                )
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100.0F)
            )

            ' Summary cards

            pnlSummary = New System.Windows.Forms.Panel With {
                .Name = "pnlSummary",
                .Dock = DockStyle.Fill,
                .BackColor = BgColor,
                .Margin = Padding.Empty
            }

            ' Main content layout

            Dim content As New TableLayoutPanel With {
                .Name = "tblDashboardContent",
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 2,
                .BackColor = BgColor,
                .Margin = Padding.Empty,
                .Padding = Padding.Empty
            }

            content.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 60.0F)
            )

            content.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 40.0F)
            )

            content.RowStyles.Add(
                New RowStyle(SizeType.Percent, 44.0F)
            )

            content.RowStyles.Add(
                New RowStyle(SizeType.Percent, 56.0F)
            )

            ' Occupancy

            pnlOccupancy =
                New System.Windows.Forms.Panel With {
                    .Name = "pnlOccupancy",
                    .Dock = DockStyle.Fill,
                    .BackColor = CardColor,
                    .Margin = New Padding(
                        0, 0, ContentGap \ 2, ContentGap
                    )
                }

            ' Quick actions

            pnlQuickActions =
                New System.Windows.Forms.Panel With {
                    .Name = "pnlQuickActions",
                    .Dock = DockStyle.Fill,
                    .BackColor = CardColor,
                    .Margin = New Padding(
                        ContentGap \ 2, 0, 0, ContentGap
                    )
                }

            ' Recent activity

            pnlActivity =
                New System.Windows.Forms.Panel With {
                    .Name = "pnlActivity",
                    .Dock = DockStyle.Fill,
                    .BackColor = CardColor,
                    .Margin = Padding.Empty
                }

            StylePanel(pnlOccupancy)
            StylePanel(pnlQuickActions)
            StylePanel(pnlActivity)

            BuildSummaryCards()
            BuildOccupancyPanel()
            BuildQuickActionsPanel()
            BuildActivityPanel()

            content.Controls.Add(pnlOccupancy, 0, 0)
            content.Controls.Add(pnlQuickActions, 1, 0)
            content.Controls.Add(pnlActivity, 0, 1)

            content.SetColumnSpan(pnlActivity, 2)

            root.Controls.Add(pnlSummary, 0, 0)
            root.Controls.Add(content, 0, 1)

            Me.Controls.Add(root)

        Finally

            Me.ResumeLayout(True)

        End Try

    End Sub

    '========================================================
    ' SUMMARY CARDS
    '========================================================

    Private Sub BuildSummaryCards()

        pnlSummary.Controls.Clear()

        Dim cards As New TableLayoutPanel With {
            .Name = "tblSummaryCards",
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 1,
            .BackColor = BgColor,
            .Margin = Padding.Empty,
            .Padding = Padding.Empty
        }

        For i As Integer = 1 To 4

            cards.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 25.0F)
            )

        Next

        cards.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F)
        )

        AddSummaryCard(
            cards, 0,
            "Total Rooms", "0",
            "Registered rooms", Navy
        )

        AddSummaryCard(
            cards, 1,
            "Occupied Rooms", "0",
            "Currently occupied", Teal
        )

        AddSummaryCard(
            cards, 2,
            "Available Rooms", "0",
            "Ready for guests",
            Color.FromArgb(46, 125, 50)
        )

        AddSummaryCard(
            cards, 3,
            "Pending Bookings", "0",
            "Awaiting confirmation",
            Color.FromArgb(230, 126, 34)
        )

        pnlSummary.Controls.Add(cards)

    End Sub

    Private Sub AddSummaryCard(
        container As TableLayoutPanel,
        column As Integer,
        title As String,
        value As String,
        description As String,
        accent As Color
    )

        Dim leftMargin As Integer =
            If(column = 0, 0, SummaryGap \ 2)

        Dim rightMargin As Integer =
            If(column = 3, 0, SummaryGap \ 2)

        Dim card As New System.Windows.Forms.Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = CardColor,
            .Margin = New Padding(
                leftMargin, 0, rightMargin, 10
            ),
            .Padding = New Padding(12)
        }

        StylePanel(card)

        Dim accentBar As New System.Windows.Forms.Panel With {
            .Name = "pnlAccent",
            .Dock = DockStyle.Left,
            .Width = 4,
            .BackColor = accent,
            .Margin = New Padding(0, 2, 10, 2)
        }

        Dim lblTitle As New System.Windows.Forms.Label With {
            .Text = title,
            .Dock = DockStyle.Top,
            .Height = 23,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.DimGray,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        Dim lblValue As New System.Windows.Forms.Label With {
            .Text = value,
            .Dock = DockStyle.Top,
            .Height = 38,
            .Font = New Font(
                "Segoe UI", 21.0F, FontStyle.Bold
            ),
            .ForeColor = Navy,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        Dim lblDescription As New System.Windows.Forms.Label With {
            .Text = description,
            .Dock = DockStyle.Fill,
            .Font = New Font("Segoe UI", 8.0F),
            .ForeColor = Color.Gray,
            .TextAlign = ContentAlignment.TopLeft
        }

        card.Controls.Add(lblDescription)
        card.Controls.Add(lblValue)
        card.Controls.Add(lblTitle)
        card.Controls.Add(accentBar)

        container.Controls.Add(card, column, 0)

        Select Case column

            Case 0
                lblTotalRoomsValue = lblValue

            Case 1
                lblOccupiedRoomsValue = lblValue

            Case 2
                lblAvailableRoomsValue = lblValue

            Case 3
                lblPendingBookingsValue = lblValue

        End Select

    End Sub

    '========================================================
    ' OCCUPANCY PANEL
    '========================================================

    Private Sub BuildOccupancyPanel()

        pnlOccupancy.Controls.Clear()

        Dim lblTitle As New System.Windows.Forms.Label With {
            .Name = "lblRoomOccupancyTitle",
            .Text = "Room Occupancy",
            .Location = New Point(18, 14),
            .Size = New Size(300, 28),
            .Font = New Font(
                "Segoe UI", 13.0F, FontStyle.Bold
            ),
            .ForeColor = Navy,
            .BackColor = Color.Transparent
        }

        chartOccupancy =
            New System.Windows.Forms.DataVisualization.Charting.Chart With {
                .Name = "chartOccupancy",
                .BackColor = Color.White,
                .Location = New Point(15, 48),
                .Size = New Size(180, 150),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left
            }

        chartOccupancy.ChartAreas.Clear()
        chartOccupancy.Series.Clear()
        chartOccupancy.Legends.Clear()

        Dim area As New ChartArea("OccupancyArea") With {
            .BackColor = Color.White
        }

        area.AxisX.Enabled = AxisEnabled.False
        area.AxisY.Enabled = AxisEnabled.False
        area.AxisX.MajorGrid.Enabled = False
        area.AxisY.MajorGrid.Enabled = False

        area.Position.Auto = False
        area.Position = New ElementPosition(0, 0, 100, 100)

        area.InnerPlotPosition.Auto = False
        area.InnerPlotPosition =
            New ElementPosition(5, 5, 90, 90)

        chartOccupancy.ChartAreas.Add(area)

        Dim series As New Series("Rooms") With {
            .ChartType = SeriesChartType.Doughnut,
            .ChartArea = "OccupancyArea",
            .IsValueShownAsLabel = False,
            .BorderWidth = 0
        }

        series.Points.AddXY("Occupied", 0)
        series.Points.AddXY("Available", 0)

        series.Points(0).Color = Teal
        series.Points(1).Color =
            Color.FromArgb(225, 232, 240)

        series("DoughnutRadius") = "65"
        series("PieLabelStyle") = "Disabled"

        chartOccupancy.Series.Add(series)

        lblOccupancyPercent =
            New System.Windows.Forms.Label With {
                .Name = "lblOccupancyPercent",
                .Text = "0%",
                .Location = New Point(205, 62),
                .Size = New Size(180, 43),
                .Font = New Font(
                    "Segoe UI", 26.0F, FontStyle.Bold
                ),
                .ForeColor = Navy,
                .BackColor = Color.Transparent
            }

        Dim lblOccupied As New System.Windows.Forms.Label With {
            .Name = "lblOccupiedDescription",
            .Text = "Room occupancy",
            .Location = New Point(209, 106),
            .Size = New Size(200, 23),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.DimGray,
            .BackColor = Color.Transparent
        }

        lblOccupancyDetails =
            New System.Windows.Forms.Label With {
                .Name = "lblOccupancyDetails",
                .Text = "0 occupied rooms" &
                        Environment.NewLine &
                        "0 available rooms",
                .Location = New Point(209, 137),
                .Size = New Size(220, 45),
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = Color.DimGray,
                .BackColor = Color.Transparent
            }

        pnlOccupancy.Controls.Add(lblTitle)
        pnlOccupancy.Controls.Add(chartOccupancy)
        pnlOccupancy.Controls.Add(lblOccupancyPercent)
        pnlOccupancy.Controls.Add(lblOccupied)
        pnlOccupancy.Controls.Add(lblOccupancyDetails)

        lblTitle.BringToFront()

        AddHandler pnlOccupancy.Resize,
            AddressOf ResizeOccupancyControls

        ResizeOccupancyControls(Nothing, EventArgs.Empty)

    End Sub

    Private Sub ResizeOccupancyControls(
        sender As Object,
        e As EventArgs
    )

        If pnlOccupancy Is Nothing OrElse
           chartOccupancy Is Nothing OrElse
           lblOccupancyPercent Is Nothing OrElse
           lblOccupancyDetails Is Nothing Then
            Return
        End If

        Dim panelWidth As Integer =
            pnlOccupancy.ClientSize.Width

        Dim panelHeight As Integer =
            pnlOccupancy.ClientSize.Height

        If panelWidth < 100 OrElse panelHeight < 100 Then
            Return
        End If

        Dim innerWidth As Integer = panelWidth - 30

        Dim chartWidth As Integer =
            Math.Min(
                185,
                Math.Max(115, CInt(innerWidth * 0.42))
            )

        Dim chartHeight As Integer =
            Math.Min(155, Math.Max(100, panelHeight - 65))

        chartOccupancy.SetBounds(
            12, 45, chartWidth, chartHeight
        )

        Dim infoLeft As Integer = chartWidth + 22

        Dim infoWidth As Integer =
            Math.Max(90, panelWidth - infoLeft - 15)

        Dim infoTop As Integer =
            Math.Max(48, (panelHeight - 115) \ 2)

        lblOccupancyPercent.SetBounds(
            infoLeft, infoTop, infoWidth, 43
        )

        For Each ctrl As Control In pnlOccupancy.Controls

            If ctrl.Name = "lblOccupiedDescription" Then

                ctrl.SetBounds(
                    infoLeft + 3,
                    infoTop + 43,
                    infoWidth - 3,
                    23
                )

            End If

        Next

        lblOccupancyDetails.SetBounds(
            infoLeft + 3,
            infoTop + 73,
            infoWidth - 3,
            42
        )

    End Sub

    '========================================================
    ' UPDATE OCCUPANCY
    '========================================================

    Public Sub UpdateOccupancy(
        occupiedRooms As Integer,
        availableRooms As Integer
    )

        If occupiedRooms < 0 OrElse availableRooms < 0 Then

            Throw New ArgumentOutOfRangeException(
                NameOf(occupiedRooms),
                "Room counts cannot be negative."
            )

        End If

        Dim totalRooms As Integer =
            occupiedRooms + availableRooms

        Dim percent As Integer = 0

        If totalRooms > 0 Then

            percent = CInt(
                Math.Round(
                    occupiedRooms * 100.0 / totalRooms,
                    MidpointRounding.AwayFromZero
                )
            )

        End If

        If lblOccupancyPercent IsNot Nothing Then
            lblOccupancyPercent.Text = percent.ToString() & "%"
        End If

        If lblOccupancyDetails IsNot Nothing Then

            lblOccupancyDetails.Text =
                occupiedRooms.ToString() &
                " occupied rooms" &
                Environment.NewLine &
                availableRooms.ToString() &
                " available rooms"

        End If

        If lblTotalRoomsValue IsNot Nothing Then
            lblTotalRoomsValue.Text = totalRooms.ToString()
        End If

        If lblOccupiedRoomsValue IsNot Nothing Then
            lblOccupiedRoomsValue.Text = occupiedRooms.ToString()
        End If

        If lblAvailableRoomsValue IsNot Nothing Then
            lblAvailableRoomsValue.Text = availableRooms.ToString()
        End If

        If chartOccupancy Is Nothing OrElse
           chartOccupancy.Series.Count = 0 Then
            Return
        End If

        Dim series As Series = chartOccupancy.Series("Rooms")

        series.Points.Clear()

        series.Points.AddXY("Occupied", occupiedRooms)
        series.Points.AddXY("Available", availableRooms)

        series.Points(0).Color = Teal
        series.Points(1).Color =
            Color.FromArgb(225, 232, 240)

    End Sub

    '========================================================
    ' UPDATE SUMMARY COUNTS
    '
    ' Call after retrieving actual values from MySQL.
    '========================================================

    Public Sub UpdateSummaryCounts(
        totalRooms As Integer,
        occupiedRooms As Integer,
        availableRooms As Integer,
        pendingBookings As Integer
    )

        If totalRooms < 0 OrElse
           occupiedRooms < 0 OrElse
           availableRooms < 0 OrElse
           pendingBookings < 0 Then

            Throw New ArgumentOutOfRangeException(
                NameOf(totalRooms),
                "Summary values cannot be negative."
            )

        End If

        If occupiedRooms + availableRooms <> totalRooms Then

            Throw New ArgumentException(
                "Total rooms must equal occupied plus available rooms."
            )

        End If

        If lblTotalRoomsValue IsNot Nothing Then
            lblTotalRoomsValue.Text = totalRooms.ToString()
        End If

        If lblOccupiedRoomsValue IsNot Nothing Then
            lblOccupiedRoomsValue.Text = occupiedRooms.ToString()
        End If

        If lblAvailableRoomsValue IsNot Nothing Then
            lblAvailableRoomsValue.Text = availableRooms.ToString()
        End If

        If lblPendingBookingsValue IsNot Nothing Then
            lblPendingBookingsValue.Text = pendingBookings.ToString()
        End If

        UpdateOccupancy(occupiedRooms, availableRooms)

    End Sub

    '========================================================
    ' RECENT ACTIVITY PANEL
    '========================================================

    Private Sub BuildActivityPanel()

        pnlActivity.Controls.Clear()

        Dim lblTitle As New System.Windows.Forms.Label With {
            .Name = "lblRecentActivityTitle",
            .Text = "Recent Activity",
            .Location = New Point(18, 14),
            .Size = New Size(300, 30),
            .Font = New Font(
                "Segoe UI", 13.0F, FontStyle.Bold
            ),
            .ForeColor = Navy
        }

        Dim lblSubtitle As New System.Windows.Forms.Label With {
            .Name = "lblActivitySubtitle",
            .Text = "A summary of activity recorded in the system.",
            .Location = New Point(20, 43),
            .Size = New Size(500, 22),
            .Font = New Font("Segoe UI", 8.5F),
            .ForeColor = Color.DimGray
        }

        pnlActivityList =
            New System.Windows.Forms.Panel With {
                .Name = "pnlActivityList",
                .Location = New Point(18, 73),
                .Size = New Size(500, 150),
                .Anchor = AnchorStyles.Top Or
                          AnchorStyles.Bottom Or
                          AnchorStyles.Left Or
                          AnchorStyles.Right,
                .AutoScroll = True,
                .BackColor = Color.White,
                .BorderStyle = BorderStyle.None
            }

        lblActivityEmpty =
            New System.Windows.Forms.Label With {
                .Name = "lblActivityEmpty",
                .Text = "No recent activity has been recorded yet." &
                        Environment.NewLine &
                        "Activities appear here when they are added.",
                .Location = New Point(8, 8),
                .Size = New Size(650, 45),
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = Color.Gray
            }

        pnlActivityList.Controls.Add(lblActivityEmpty)

        pnlActivity.Controls.Add(lblTitle)
        pnlActivity.Controls.Add(lblSubtitle)
        pnlActivity.Controls.Add(pnlActivityList)

        AddHandler pnlActivity.Resize,
            AddressOf ResizeActivityList

        ResizeActivityList(Nothing, EventArgs.Empty)

    End Sub

    Private Sub ResizeActivityList(
        sender As Object,
        e As EventArgs
    )

        If pnlActivity Is Nothing OrElse
           pnlActivityList Is Nothing Then
            Return
        End If

        pnlActivityList.SetBounds(
            18,
            73,
            Math.Max(100, pnlActivity.ClientSize.Width - 36),
            Math.Max(50, pnlActivity.ClientSize.Height - 87)
        )

        For Each item As Control In pnlActivityList.Controls

            If item.Name = "lblActivityEmpty" Then

                item.Width =
                    Math.Max(250, pnlActivityList.ClientSize.Width - 25)

            ElseIf item.Tag IsNot Nothing AndAlso
                   item.Tag.ToString() = "ActivityItem" Then

                Dim itemWidth As Integer =
                    Math.Max(250, pnlActivityList.ClientSize.Width - 25)

                item.Width = itemWidth

                For Each child As Control In item.Controls

                    If TypeOf child Is System.Windows.Forms.Label Then
                        child.Width = Math.Max(100, itemWidth - 20)
                    End If

                Next

            End If

        Next

    End Sub

    '========================================================
    ' ADD RECENT ACTIVITY
    '========================================================

    Public Sub AddActivity(
        activityTitle As String,
        description As String,
        Optional activityTime As String = ""
    )

        If String.IsNullOrWhiteSpace(activityTitle) Then
            Return
        End If

        If pnlActivityList Is Nothing Then
            Return
        End If

        If lblActivityEmpty IsNot Nothing AndAlso
           pnlActivityList.Controls.Contains(lblActivityEmpty) Then

            pnlActivityList.Controls.Remove(lblActivityEmpty)
            lblActivityEmpty.Dispose()
            lblActivityEmpty = Nothing

        End If

        Dim itemWidth As Integer =
            Math.Max(250, pnlActivityList.ClientSize.Width - 25)

        Dim currentTop As Integer = 0

        For Each existingControl As Control In pnlActivityList.Controls

            currentTop = Math.Max(
                currentTop,
                existingControl.Bottom
            )

        Next

        Dim activityPanel As New System.Windows.Forms.Panel With {
            .Name = "pnlActivityItem",
            .Tag = "ActivityItem",
            .Height = 70,
            .Width = itemWidth,
            .Location = New Point(0, currentTop),
            .BackColor = Color.White,
            .Margin = Padding.Empty
        }

        Dim lblItemTitle As New System.Windows.Forms.Label With {
            .Text = activityTitle,
            .Location = New Point(8, 5),
            .Size = New Size(itemWidth - 20, 22),
            .Font = New Font(
                "Segoe UI", 9.0F, FontStyle.Bold
            ),
            .ForeColor = Navy
        }

        Dim lblDescription As New System.Windows.Forms.Label With {
            .Text = description,
            .Location = New Point(8, 28),
            .Size = New Size(itemWidth - 20, 19),
            .Font = New Font("Segoe UI", 8.5F),
            .ForeColor = Color.DimGray
        }

        Dim lblTime As New System.Windows.Forms.Label With {
            .Text = activityTime,
            .Location = New Point(8, 48),
            .Size = New Size(itemWidth - 20, 17),
            .Font = New Font("Segoe UI", 7.5F),
            .ForeColor = Color.Gray
        }

        Dim separator As New System.Windows.Forms.Panel With {
            .Height = 1,
            .Dock = DockStyle.Bottom,
            .BackColor = BorderColor
        }

        activityPanel.Controls.Add(lblItemTitle)
        activityPanel.Controls.Add(lblDescription)
        activityPanel.Controls.Add(lblTime)
        activityPanel.Controls.Add(separator)

        pnlActivityList.Controls.Add(activityPanel)

        pnlActivityList.AutoScrollMinSize =
            New Size(0, activityPanel.Bottom + 5)

    End Sub

    '========================================================
    ' QUICK ACTIONS
    '========================================================


    Private Sub BuildQuickActionsPanel()

        pnlQuickActions.SuspendLayout()
        pnlQuickActions.Controls.Clear()

        pnlQuickActions.BackColor = Color.White
        pnlQuickActions.Padding = New Padding(20)

        Dim lblTitle As New Label With {
        .Text = "Quick Actions",
        .Font = New Font("Segoe UI", 15, FontStyle.Bold),
        .ForeColor = Navy,
        .AutoSize = True,
        .Location = New Point(22, 16)
    }

        Dim lblSubtitle As New Label With {
        .Text = "Common administration tasks",
        .Font = New Font("Segoe UI", 10),
        .ForeColor = Color.FromArgb(100, 116, 139),
        .AutoSize = True,
        .Location = New Point(22, 51)
    }

        Dim flp As New FlowLayoutPanel With {
        .Name = "flpQuickActions",
        .Location = New Point(20, 88),
        .Size = New Size(
            Math.Max(100, pnlQuickActions.ClientSize.Width - 40),
            Math.Max(80, pnlQuickActions.ClientSize.Height - 108)
        ),
        .Anchor = AnchorStyles.Top Or
                  AnchorStyles.Bottom Or
                  AnchorStyles.Left Or
                  AnchorStyles.Right,
        .FlowDirection = FlowDirection.LeftToRight,
        .WrapContents = True,
        .AutoScroll = True,
        .BackColor = Color.White,
        .Padding = New Padding(0, 0, 5, 5),
        .Margin = New Padding(0)
    }

        pnlQuickActions.Controls.Add(lblTitle)
        pnlQuickActions.Controls.Add(lblSubtitle)
        pnlQuickActions.Controls.Add(flp)

        AddQuickActionButton(flp, "New Booking", "＋",
                         AddressOf OpenBookings)

        AddQuickActionButton(flp, "Manage Rooms", "▦",
                         AddressOf OpenRooms)

        AddQuickActionButton(flp, "Process Payment", "₱",
                         AddressOf OpenPayments)

        AddQuickActionButton(flp, "Manage Parking", "P",
                         AddressOf OpenParking)

        pnlQuickActions.ResumeLayout(True)

    End Sub

    Private Sub AddQuickActionButton(
    parent As FlowLayoutPanel,
    caption As String,
    iconText As String,
    action As EventHandler
)

        Dim btn As New Button With {
        .Text = iconText & "    " & caption,
        .Width = 160,
        .Height = 45,
        .Margin = New Padding(0, 0, 12, 12),
        .FlatStyle = FlatStyle.Flat,
        .BackColor = Color.FromArgb(13, 148, 136),
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 8, FontStyle.Bold),
        .TextAlign = ContentAlignment.MiddleLeft,
        .Padding = New Padding(14, 0, 6, 0),
        .Cursor = Cursors.Hand,
        .UseVisualStyleBackColor = False
    }

        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor =
        Color.FromArgb(15, 118, 110)

        AddHandler btn.Click, action
        parent.Controls.Add(btn)

    End Sub


    Private Sub AddQuickActionButton(
    parent As FlowLayoutPanel,
    caption As String,
    action As EventHandler
)

        Dim btn As New Button With {
        .Text = caption,
        .Height = 46,
        .Width = Math.Max(100, parent.ClientSize.Width - 30),
        .Margin = New Padding(0, 0, 0, 10),
        .FlatStyle = FlatStyle.Flat,
        .BackColor = Teal,
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 10, FontStyle.Bold),
        .Cursor = Cursors.Hand,
        .UseVisualStyleBackColor = False
    }

        btn.FlatAppearance.BorderSize = 0

        AddHandler btn.Click, action

        parent.Controls.Add(btn)

    End Sub

    Private Sub AddQuickAction(
        caption As String,
        action As EventHandler
    )

        Dim btn As New System.Windows.Forms.Button With {
            .Text = caption,
            .Height = 44,
            .Dock = DockStyle.Top,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Teal,
            .ForeColor = Color.White,
            .Font = New Font(
                "Segoe UI", 9.0F, FontStyle.Bold
            ),
            .Cursor = Cursors.Hand,
            .Margin = New Padding(0, 0, 0, 10),
            .UseVisualStyleBackColor = False
        }

        btn.FlatAppearance.BorderSize = 0

        btn.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(10, 126, 116)

        btn.FlatAppearance.MouseDownBackColor =
            Color.FromArgb(8, 105, 97)

        AddHandler btn.Click, action

        pnlQuickActionButtons.Controls.Add(btn)

        ' With DockStyle.Top, reverse child order keeps
        ' the buttons displayed in the order added.
        pnlQuickActionButtons.Controls.SetChildIndex(
            btn, 0
        )

    End Sub

    Private Sub ManageRooms_Click(sender As Object, e As EventArgs)

        Dim current As Control = Me

        While current IsNot Nothing

            If TypeOf current Is FrmAdminDashboard Then

                DirectCast(current, FrmAdminDashboard).LoadModule(New ucRooms())
                Return

            End If

            current = current.Parent

        End While

        MessageBox.Show(
        "Unable to find the admin dashboard.",
        "StayPoint HMS",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    )

    End Sub

    Private Sub ResizeQuickActionButtons(
        sender As Object,
        e As EventArgs
    )

        If pnlQuickActions Is Nothing OrElse
           pnlQuickActionButtons Is Nothing Then
            Return
        End If

        pnlQuickActionButtons.SetBounds(
            18,
            82,
            Math.Max(100, pnlQuickActions.ClientSize.Width - 36),
            Math.Max(100, pnlQuickActions.ClientSize.Height - 100)
        )

    End Sub

    '========================================================
    ' NAVIGATION
    '========================================================

    Private Sub OpenBookings(
        sender As Object,
        e As EventArgs
    )

        RequestModuleNavigation("Bookings")

    End Sub

    Private Sub OpenRooms(
        sender As Object,
        e As EventArgs
    )

        RequestModuleNavigation("Rooms")

    End Sub

    Private Sub OpenPayments(
        sender As Object,
        e As EventArgs
    )

        RequestModuleNavigation("Payments")

    End Sub

    Private Sub OpenParking(
        sender As Object,
        e As EventArgs
    )

        RequestModuleNavigation("Parking")

    End Sub

    Private Sub RequestModuleNavigation(
        moduleName As String
    )

        Dim parentControl As Control = Me.Parent

        While parentControl IsNot Nothing

            If TypeOf parentControl Is FrmAdminDashboard Then

                Dim dashboard As FrmAdminDashboard =
                    DirectCast(
                        parentControl,
                        FrmAdminDashboard
                    )

                dashboard.NavigateToModule(moduleName)
                Return

            End If

            parentControl = parentControl.Parent

        End While

        MessageBox.Show(
            "The dashboard navigation host was not found.",
            "Navigation",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

    End Sub

    '========================================================
    ' PANEL STYLE
    '========================================================

    Private Sub StylePanel(
        targetPanel As System.Windows.Forms.Panel
    )

        targetPanel.BorderStyle = BorderStyle.FixedSingle

    End Sub

    '========================================================
    ' DESIGNER PAINT EVENT
    ' Retained for Designer.vb compatibility.
    '========================================================

    Private Sub Panel1_Paint(
        sender As Object,
        e As PaintEventArgs
    ) Handles Panel1.Paint

    End Sub
End Class
