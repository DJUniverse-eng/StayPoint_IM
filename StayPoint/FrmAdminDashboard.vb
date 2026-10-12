Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmAdminDashboard

    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly SidebarColor As Color =
        Color.FromArgb(32, 58, 95)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly BgColor As Color =
        Color.FromArgb(240, 244, 248)

    ' Unique names to avoid conflicts with Designer controls.
    Private pnlAppSidebar As Panel
    Private pnlAppHeader As Panel
    Private pnlModuleHost As Panel

    Private currentModule As UserControl

    Private Sub FrmAdminDashboard_Load(
    sender As Object,
    e As EventArgs
) Handles MyBase.Load

        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Maximized

    End Sub

    Public Sub New()
        InitializeComponent()
        BuildApplicationShell()

        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Normal
    End Sub
    '========================================================
    ' LOAD USER CONTROL INTO DASHBOARD CONTENT AREA
    '========================================================

    '========================================================
    ' APPLICATION SHELL
    '========================================================

    Private Sub BuildApplicationShell()

        Me.SuspendLayout()

        Try
            ' Remove the old controls from the visible form.
            ' Designer declarations remain intact.
            Me.Controls.Clear()

            Me.Text = "StayPoint HMS - Admin Dashboard"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.WindowState = FormWindowState.Maximized
            Me.MinimumSize = New Size(1100, 700)
            Me.BackColor = BgColor
            Me.Font = New Font("Segoe UI", 9.0F)

            pnlAppSidebar = New Panel With {
                .Name = "pnlAppSidebar",
                .Dock = DockStyle.Left,
                .Width = 220,
                .BackColor = SidebarColor
            }

            Dim pnlWorkspace As New Panel With {
                .Name = "pnlWorkspace",
                .Dock = DockStyle.Fill,
                .BackColor = BgColor
            }

            pnlAppHeader = New Panel With {
                .Name = "pnlAppHeader",
                .Dock = DockStyle.Top,
                .Height = 75,
                .BackColor = Color.White
            }

            pnlModuleHost = New Panel With {
                .Name = "pnlModuleHost",
                .Dock = DockStyle.Fill,
                .BackColor = BgColor,
                .Padding = New Padding(0),
                .AutoScroll = False
            }

            BuildHeader()
            BuildSidebar()

            ' Add content first, then header.
            ' Docking keeps the header above the module area.
            pnlWorkspace.Controls.Add(pnlModuleHost)
            pnlWorkspace.Controls.Add(pnlAppHeader)

            Me.Controls.Add(pnlWorkspace)
            Me.Controls.Add(pnlAppSidebar)

        Finally
            Me.ResumeLayout(True)
        End Try

        LoadModule(New ucDashboard())

    End Sub

    '========================================================
    ' HEADER
    '========================================================

    Private Sub BuildHeader()

        Dim lblTitle As New Label With {
            .Text = "StayPoint HMS",
            .Location = New Point(25, 10),
            .Size = New Size(400, 32),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Navy
        }

        Dim lblWelcome As New Label With {
            .Text = "Hotel Management System",
            .Location = New Point(27, 43),
            .Size = New Size(350, 20),
            .ForeColor = Color.DimGray
        }

        pnlAppHeader.Controls.Add(lblTitle)
        pnlAppHeader.Controls.Add(lblWelcome)

    End Sub

    '========================================================
    ' SIDEBAR
    '========================================================

    Private Sub BuildSidebar()

        Dim lblLogo As New Label With {
            .Text = "StayPoint",
            .Location = New Point(20, 25),
            .Size = New Size(185, 40),
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Color.White
        }

        Dim lblSubtitle As New Label With {
            .Text = "HOTEL MANAGEMENT",
            .Location = New Point(22, 67),
            .Size = New Size(185, 22),
            .Font = New Font("Segoe UI", 8, FontStyle.Bold),
            .ForeColor = Color.LightGray
        }

        pnlAppSidebar.Controls.Add(lblLogo)
        pnlAppSidebar.Controls.Add(lblSubtitle)

        AddNavigationButton("Dashboard", 120, AddressOf ShowDashboard)
        AddNavigationButton("Bookings", 172, AddressOf ShowBookings)
        AddNavigationButton("Check-In / Check-Out", 224, AddressOf ShowCheckInOut)
        AddNavigationButton("Rooms", 276, AddressOf ShowRooms)
        AddNavigationButton("Guests", 328, AddressOf ShowGuests)
        AddNavigationButton("Payments", 380, AddressOf ShowPayments)
        AddNavigationButton("Parking", 432, AddressOf ShowParking)
        AddNavigationButton("Reports", 484, AddressOf ShowReports)
        AddNavigationButton("Staff Management", 536, AddressOf ShowStaffManagement)
        AddNavigationButton("System Settings", 588, AddressOf ShowSystemSettings)

        Dim btnLogout As Button = CreateSidebarButton(
            "Logout",
            New Point(10, 650),
            New Size(200, 42)
        )

        btnLogout.BackColor = Color.FromArgb(183, 55, 55)

        AddHandler btnLogout.Click, AddressOf Logout_Click

        pnlAppSidebar.Controls.Add(btnLogout)

    End Sub

    Private Sub AddNavigationButton(
        caption As String,
        top As Integer,
        action As EventHandler
    )

        Dim btn As Button = CreateSidebarButton(
            caption,
            New Point(10, top),
            New Size(200, 44)
        )

        AddHandler btn.Click, action

        pnlAppSidebar.Controls.Add(btn)

    End Sub

    Private Function CreateSidebarButton(
        caption As String,
        position As Point,
        buttonSize As Size
    ) As Button

        Dim btn As New Button With {
            .Text = "   " & caption,
            .Location = position,
            .Size = buttonSize,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = SidebarColor,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 9.5F),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Cursor = Cursors.Hand
        }

        btn.FlatAppearance.BorderSize = 0

        Return btn

    End Function

    '========================================================
    ' USERCONTROL HOST
    '========================================================

    Public Sub LoadModule(moduleControl As UserControl)

        If moduleControl Is Nothing Then Return

        pnlModuleHost.SuspendLayout()

        Try
            If currentModule IsNot Nothing Then
                pnlModuleHost.Controls.Remove(currentModule)
                currentModule.Dispose()
            End If

            currentModule = moduleControl
            currentModule.Dock = DockStyle.Fill
            currentModule.Margin = Padding.Empty

            pnlModuleHost.Controls.Add(currentModule)
            currentModule.BringToFront()

        Finally
            pnlModuleHost.ResumeLayout(True)
        End Try

    End Sub

    '========================================================
    ' NAVIGATION
    ' These classes must exist as UserControls in the project.
    '========================================================

    Private Sub ShowDashboard(sender As Object, e As EventArgs)
        LoadModule(New ucDashboard())
    End Sub

    Private Sub ShowBookings(sender As Object, e As EventArgs)
        LoadModule(New ucBookings())
    End Sub

    Private Sub ShowCheckInOut(sender As Object, e As EventArgs)
        LoadModule(New ucCheckInOut())
    End Sub

    Private Sub ShowRooms(sender As Object, e As EventArgs)
        LoadModule(New ucRooms())
    End Sub

    Private Sub ShowGuests(sender As Object, e As EventArgs)
        LoadModule(New ucGuests())
    End Sub

    Private Sub ShowPayments(sender As Object, e As EventArgs)
        LoadModule(New ucPayments())
    End Sub

    Private Sub ShowParking(sender As Object, e As EventArgs)
        LoadModule(New ucParking())
    End Sub

    Private Sub ShowReports(sender As Object, e As EventArgs)
        LoadModule(New ucReports())
    End Sub

    Private Sub ShowStaffManagement(sender As Object, e As EventArgs)
        LoadModule(New ucStaffManagement())
    End Sub

    Private Sub ShowSystemSettings(sender As Object, e As EventArgs)
        LoadModule(New ucSystemSettings())
    End Sub

    Public Sub NavigateToModule(moduleName As String)

        Select Case moduleName.Trim().ToLowerInvariant()

            Case "dashboard"
                ShowDashboard(Me, EventArgs.Empty)

            Case "bookings"
                ShowBookings(Me, EventArgs.Empty)

            Case "rooms"
                ShowRooms(Me, EventArgs.Empty)

            Case "payments"
                ShowPayments(Me, EventArgs.Empty)

            Case "parking"
                ShowParking(Me, EventArgs.Empty)

            Case "guests"
                ShowGuests(Me, EventArgs.Empty)

            Case "checkinout"
                ShowCheckInOut(Me, EventArgs.Empty)

            Case "reports"
                ShowReports(Me, EventArgs.Empty)

            Case "staffmanagement"
                ShowStaffManagement(Me, EventArgs.Empty)

            Case "systemsettings"
                ShowSystemSettings(Me, EventArgs.Empty)

        End Select

    End Sub

    '========================================================
    ' LOGOUT
    '========================================================

    Private Sub Logout_Click(
        sender As Object,
        e As EventArgs
    )

        Dim answer As DialogResult = MessageBox.Show(
            "Are you sure you want to log out?",
            "Confirm Logout",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If answer = DialogResult.Yes Then
            Me.Close()
        End If

    End Sub
End Class