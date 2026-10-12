
Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmStaffDashboard

    '========================================================
    ' STAYPOINT COLORS — MATCH ADMIN DASHBOARD
    '========================================================

    Private ReadOnly Navy As Color =
        Color.FromArgb(23, 43, 77)

    Private ReadOnly SidebarColor As Color =
        Color.FromArgb(32, 58, 95)

    Private ReadOnly Teal As Color =
        Color.FromArgb(13, 148, 136)

    Private ReadOnly BgColor As Color =
        Color.FromArgb(240, 244, 248)

    '========================================================
    ' APPLICATION SHELL
    '========================================================

    Private pnlAppSidebar As Panel
    Private pnlAppHeader As Panel
    Private pnlModuleHost As Panel

    Private currentModule As UserControl

    '========================================================
    ' CONSTRUCTOR
    '========================================================

    Public Sub New()

        InitializeComponent()

        ' Match Admin Dashboard window behavior.
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.WindowState = FormWindowState.Maximized
        Me.MinimumSize = New Size(1100, 700)
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MaximizeBox = True

        BuildApplicationShell()

    End Sub

    Private Sub FrmStaffDashboard_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        Me.Text = "StayPoint HMS - Staff Dashboard"

    End Sub

    '========================================================
    ' APPLICATION SHELL
    '========================================================

    Private Sub BuildApplicationShell()

        Me.SuspendLayout()

        Try
            ' Remove visible Designer controls.
            ' Keep the Designer-generated InitializeComponent.
            Me.Controls.Clear()

            Me.Text = "StayPoint HMS - Staff Dashboard"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.WindowState = FormWindowState.Maximized
            Me.MinimumSize = New Size(1100, 700)
            Me.FormBorderStyle = FormBorderStyle.Sizable
            Me.MaximizeBox = True
            Me.BackColor = BgColor
            Me.Font = New Font("Segoe UI", 9.0F)

            '------------------------------------------------
            ' SIDEBAR
            '------------------------------------------------

            pnlAppSidebar = New Panel With {
                .Name = "pnlStaffSidebar",
                .Dock = DockStyle.Left,
                .Width = 220,
                .BackColor = SidebarColor
            }

            '------------------------------------------------
            ' WORKSPACE
            '------------------------------------------------

            Dim pnlWorkspace As New Panel With {
                .Name = "pnlStaffWorkspace",
                .Dock = DockStyle.Fill,
                .BackColor = BgColor
            }

            '------------------------------------------------
            ' HEADER
            '------------------------------------------------

            pnlAppHeader = New Panel With {
                .Name = "pnlStaffHeader",
                .Dock = DockStyle.Top,
                .Height = 75,
                .BackColor = Color.White
            }

            '------------------------------------------------
            ' MODULE HOST
            '------------------------------------------------

            pnlModuleHost = New Panel With {
                .Name = "pnlStaffModuleHost",
                .Dock = DockStyle.Fill,
                .BackColor = BgColor,
                .Padding = New Padding(0),
                .AutoScroll = False
            }

            BuildHeader()
            BuildSidebar()

            ' Dock order: header at the top, module area below.
            pnlWorkspace.Controls.Add(pnlModuleHost)
            pnlWorkspace.Controls.Add(pnlAppHeader)

            Me.Controls.Add(pnlWorkspace)
            Me.Controls.Add(pnlAppSidebar)

        Finally
            Me.ResumeLayout(True)
        End Try

        ' Display the staff home screen.
        ShowDashboard(Me, EventArgs.Empty)

    End Sub

    '========================================================
    ' HEADER — MATCH ADMIN DASHBOARD
    '========================================================

    Private Sub BuildHeader()

        Dim lblTitle As New Label With {
            .Name = "lblStaffAppTitle",
            .Text = "StayPoint HMS",
            .Location = New Point(25, 10),
            .Size = New Size(400, 32),
            .Font = New Font("Segoe UI", 18, FontStyle.Bold),
            .ForeColor = Navy
        }

        Dim lblSubtitle As New Label With {
            .Name = "lblStaffAppSubtitle",
            .Text = "Staff Operations Portal",
            .Location = New Point(27, 43),
            .Size = New Size(350, 20),
            .ForeColor = Color.DimGray
        }

        pnlAppHeader.Controls.Add(lblTitle)
        pnlAppHeader.Controls.Add(lblSubtitle)

    End Sub

    '========================================================
    ' SIDEBAR — STAFF MODULES ONLY
    '========================================================

    Private Sub BuildSidebar()

        pnlAppSidebar.Controls.Clear()

        Dim lblLogo As New Label With {
            .Name = "lblStaffLogo",
            .Text = "StayPoint",
            .Location = New Point(20, 25),
            .Size = New Size(185, 40),
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = Color.White
        }

        Dim lblSubtitle As New Label With {
            .Name = "lblStaffSidebarSubtitle",
            .Text = "STAFF PORTAL",
            .Location = New Point(22, 67),
            .Size = New Size(185, 22),
            .Font = New Font("Segoe UI", 8, FontStyle.Bold),
            .ForeColor = Color.LightGray
        }

        pnlAppSidebar.Controls.Add(lblLogo)
        pnlAppSidebar.Controls.Add(lblSubtitle)

        AddNavigationButton(
            "Dashboard",
            120,
            AddressOf ShowDashboard
        )

        AddNavigationButton(
            "Bookings & Walk-ins",
            172,
            AddressOf ShowBookings
        )

        AddNavigationButton(
            "Room Assignment",
            224,
            AddressOf ShowRoomAssignment
        )

        AddNavigationButton(
            "Check-In / Check-Out",
            276,
            AddressOf ShowCheckInOut
        )

        AddNavigationButton(
            "Payments & Folio",
            328,
            AddressOf ShowPayments
        )

        AddNavigationButton(
            "Parking Assignment",
            380,
            AddressOf ShowParking
        )

        AddNavigationButton(
            "Guest Search",
            432,
            AddressOf ShowGuestSearch
        )

        '------------------------------------------------
        ' LOGOUT — ANCHORED TO THE BOTTOM
        '------------------------------------------------

        Dim btnLogout As Button = CreateSidebarButton(
            "Logout",
            New Point(10, 0),
            New Size(200, 42)
        )

        btnLogout.Name = "btnStaffLogout"
        btnLogout.Anchor = AnchorStyles.Left Or
                           AnchorStyles.Right Or
                           AnchorStyles.Bottom
        btnLogout.BackColor = Color.FromArgb(183, 55, 55)

        AddHandler btnLogout.Click, AddressOf Logout_Click

        pnlAppSidebar.Controls.Add(btnLogout)

        ' Keep Logout near the bottom as the window resizes.
        AddHandler pnlAppSidebar.Resize,
            Sub(sender, e)
                btnLogout.Location = New Point(
                    10,
                    pnlAppSidebar.ClientSize.Height -
                    btnLogout.Height - 8
                )
            End Sub

        btnLogout.Location = New Point(
            10,
            pnlAppSidebar.ClientSize.Height -
            btnLogout.Height - 8
        )

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

        btn.Name = "btnStaff" &
                   caption.Replace(" ", "").Replace("/", "")

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
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }

        btn.FlatAppearance.BorderSize = 0
        btn.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(42, 75, 112)
        btn.FlatAppearance.MouseDownBackColor = Teal

        Return btn

    End Function

    '========================================================
    ' USERCONTROL HOST
    '========================================================

    Public Sub LoadModule(moduleControl As UserControl)

        If moduleControl Is Nothing Then Return
        If pnlModuleHost Is Nothing Then Return

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
    ' STAFF NAVIGATION
    ' Each screen uses a STAFF UserControl.
    ' Do not substitute Admin UserControls here.
    '========================================================

    Private Sub ShowDashboard(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffHome())

    End Sub

    Private Sub ShowBookings(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffBookings())

    End Sub

    Private Sub ShowRoomAssignment(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucRoomAssignment())

    End Sub

    Private Sub ShowCheckInOut(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffCheckInOut())

    End Sub

    Private Sub ShowPayments(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffPayments())

    End Sub

    Private Sub ShowParking(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffParking())

    End Sub

    Private Sub ShowGuestSearch(
        sender As Object,
        e As EventArgs
    )

        LoadModule(New ucStaffGuestSearch())

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

        If answer <> DialogResult.Yes Then Return

        FrmLogin.txtUsername.Clear()
        FrmLogin.txtPassword.Clear()

        FrmLogin.Show()
        Me.Close()

    End Sub

End Class
