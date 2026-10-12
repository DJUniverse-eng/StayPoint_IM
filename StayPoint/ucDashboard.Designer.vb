<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucDashboard
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim ChartArea4 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend4 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series4 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.pnlQuickActions = New System.Windows.Forms.Panel()
        Me.btnAssignParking = New System.Windows.Forms.Button()
        Me.btnRecordPayment = New System.Windows.Forms.Button()
        Me.btnCheckIn = New System.Windows.Forms.Button()
        Me.btnNewBooking = New System.Windows.Forms.Button()
        Me.lblQuickActions = New System.Windows.Forms.Label()
        Me.pnlActivity = New System.Windows.Forms.Panel()
        Me.pnlActivityParking = New System.Windows.Forms.Panel()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblParkingTitle = New System.Windows.Forms.Label()
        Me.lblParkingIcon = New System.Windows.Forms.Label()
        Me.pnlActivityPayment = New System.Windows.Forms.Panel()
        Me.lblPaymentDescription = New System.Windows.Forms.Label()
        Me.lblPaymentTitle = New System.Windows.Forms.Label()
        Me.lblPaymentIcon = New System.Windows.Forms.Label()
        Me.pnlActivityCheckIn = New System.Windows.Forms.Panel()
        Me.Label4lblCheckInDescription = New System.Windows.Forms.Label()
        Me.lblCheckInTitle = New System.Windows.Forms.Label()
        Me.pnlCheckInIcon = New System.Windows.Forms.Label()
        Me.lblActivityTitle = New System.Windows.Forms.Label()
        Me.pnlOccupancy = New System.Windows.Forms.Panel()
        Me.lblAvailableCount = New System.Windows.Forms.Label()
        Me.lblOccupiedCount = New System.Windows.Forms.Label()
        Me.chartOccupancy = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.lblOccupancyRate = New System.Windows.Forms.Label()
        Me.lblOccupancyTitle = New System.Windows.Forms.Label()
        Me.pnlParkingCard = New System.Windows.Forms.Panel()
        Me.pnlBookingCard = New System.Windows.Forms.Panel()
        Me.pnlGuestCard = New System.Windows.Forms.Panel()
        Me.pnlRoomCard = New System.Windows.Forms.Panel()
        Me.lblCurrentDate = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblPageTitle = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        Me.pnlQuickActions.SuspendLayout()
        Me.pnlActivity.SuspendLayout()
        Me.pnlActivityParking.SuspendLayout()
        Me.pnlActivityPayment.SuspendLayout()
        Me.pnlActivityCheckIn.SuspendLayout()
        Me.pnlOccupancy.SuspendLayout()
        CType(Me.chartOccupancy, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.pnlQuickActions)
        Me.Panel1.Controls.Add(Me.pnlActivity)
        Me.Panel1.Controls.Add(Me.pnlOccupancy)
        Me.Panel1.Controls.Add(Me.pnlParkingCard)
        Me.Panel1.Controls.Add(Me.pnlBookingCard)
        Me.Panel1.Controls.Add(Me.pnlGuestCard)
        Me.Panel1.Controls.Add(Me.pnlRoomCard)
        Me.Panel1.Controls.Add(Me.lblCurrentDate)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.lblPageTitle)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Padding = New System.Windows.Forms.Padding(25)
        Me.Panel1.Size = New System.Drawing.Size(1406, 667)
        Me.Panel1.TabIndex = 5
        '
        'pnlQuickActions
        '
        Me.pnlQuickActions.Controls.Add(Me.btnAssignParking)
        Me.pnlQuickActions.Controls.Add(Me.btnRecordPayment)
        Me.pnlQuickActions.Controls.Add(Me.btnCheckIn)
        Me.pnlQuickActions.Controls.Add(Me.btnNewBooking)
        Me.pnlQuickActions.Controls.Add(Me.lblQuickActions)
        Me.pnlQuickActions.Location = New System.Drawing.Point(604, 245)
        Me.pnlQuickActions.Name = "pnlQuickActions"
        Me.pnlQuickActions.Size = New System.Drawing.Size(451, 220)
        Me.pnlQuickActions.TabIndex = 7
        '
        'btnAssignParking
        '
        Me.btnAssignParking.Location = New System.Drawing.Point(37, 196)
        Me.btnAssignParking.Name = "btnAssignParking"
        Me.btnAssignParking.Size = New System.Drawing.Size(348, 48)
        Me.btnAssignParking.TabIndex = 4
        Me.btnAssignParking.Text = "Parking"
        Me.btnAssignParking.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnAssignParking.UseVisualStyleBackColor = True
        '
        'btnRecordPayment
        '
        Me.btnRecordPayment.Location = New System.Drawing.Point(37, 142)
        Me.btnRecordPayment.Name = "btnRecordPayment"
        Me.btnRecordPayment.Size = New System.Drawing.Size(348, 48)
        Me.btnRecordPayment.TabIndex = 3
        Me.btnRecordPayment.Text = "Record Payment"
        Me.btnRecordPayment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnRecordPayment.UseVisualStyleBackColor = True
        '
        'btnCheckIn
        '
        Me.btnCheckIn.Location = New System.Drawing.Point(37, 88)
        Me.btnCheckIn.Name = "btnCheckIn"
        Me.btnCheckIn.Size = New System.Drawing.Size(348, 48)
        Me.btnCheckIn.TabIndex = 2
        Me.btnCheckIn.Text = "Check-In"
        Me.btnCheckIn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnCheckIn.UseVisualStyleBackColor = True
        '
        'btnNewBooking
        '
        Me.btnNewBooking.Location = New System.Drawing.Point(37, 34)
        Me.btnNewBooking.Name = "btnNewBooking"
        Me.btnNewBooking.Size = New System.Drawing.Size(348, 48)
        Me.btnNewBooking.TabIndex = 1
        Me.btnNewBooking.Text = "New Booking"
        Me.btnNewBooking.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnNewBooking.UseVisualStyleBackColor = True
        '
        'lblQuickActions
        '
        Me.lblQuickActions.AutoSize = True
        Me.lblQuickActions.Location = New System.Drawing.Point(15, 15)
        Me.lblQuickActions.Name = "lblQuickActions"
        Me.lblQuickActions.Size = New System.Drawing.Size(88, 16)
        Me.lblQuickActions.TabIndex = 0
        Me.lblQuickActions.Text = "Quick Actions"
        '
        'pnlActivity
        '
        Me.pnlActivity.AutoScroll = True
        Me.pnlActivity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlActivity.Controls.Add(Me.pnlActivityParking)
        Me.pnlActivity.Controls.Add(Me.pnlActivityPayment)
        Me.pnlActivity.Controls.Add(Me.pnlActivityCheckIn)
        Me.pnlActivity.Controls.Add(Me.lblActivityTitle)
        Me.pnlActivity.Location = New System.Drawing.Point(25, 490)
        Me.pnlActivity.Name = "pnlActivity"
        Me.pnlActivity.Size = New System.Drawing.Size(1030, 230)
        Me.pnlActivity.TabIndex = 6
        '
        'pnlActivityParking
        '
        Me.pnlActivityParking.Controls.Add(Me.Label5)
        Me.pnlActivityParking.Controls.Add(Me.lblParkingTitle)
        Me.pnlActivityParking.Controls.Add(Me.lblParkingIcon)
        Me.pnlActivityParking.Location = New System.Drawing.Point(15, 157)
        Me.pnlActivityParking.Name = "pnlActivityParking"
        Me.pnlActivityParking.Size = New System.Drawing.Size(468, 48)
        Me.pnlActivityParking.TabIndex = 3
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(55, 24)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(177, 16)
        Me.Label5.TabIndex = 7
        Me.Label5.Text = "A parking slot was assigned."
        '
        'lblParkingTitle
        '
        Me.lblParkingTitle.AutoSize = True
        Me.lblParkingTitle.Location = New System.Drawing.Point(55, 4)
        Me.lblParkingTitle.Name = "lblParkingTitle"
        Me.lblParkingTitle.Size = New System.Drawing.Size(113, 16)
        Me.lblParkingTitle.TabIndex = 6
        Me.lblParkingTitle.Text = "Parking Assigned"
        '
        'lblParkingIcon
        '
        Me.lblParkingIcon.AutoSize = True
        Me.lblParkingIcon.Location = New System.Drawing.Point(8, 5)
        Me.lblParkingIcon.Name = "lblParkingIcon"
        Me.lblParkingIcon.Size = New System.Drawing.Size(21, 16)
        Me.lblParkingIcon.TabIndex = 5
        Me.lblParkingIcon.Text = "$$"
        '
        'pnlActivityPayment
        '
        Me.pnlActivityPayment.Controls.Add(Me.lblPaymentDescription)
        Me.pnlActivityPayment.Controls.Add(Me.lblPaymentTitle)
        Me.pnlActivityPayment.Controls.Add(Me.lblPaymentIcon)
        Me.pnlActivityPayment.Location = New System.Drawing.Point(15, 101)
        Me.pnlActivityPayment.Name = "pnlActivityPayment"
        Me.pnlActivityPayment.Size = New System.Drawing.Size(468, 48)
        Me.pnlActivityPayment.TabIndex = 3
        '
        'lblPaymentDescription
        '
        Me.lblPaymentDescription.AutoSize = True
        Me.lblPaymentDescription.Location = New System.Drawing.Point(55, 24)
        Me.lblPaymentDescription.Name = "lblPaymentDescription"
        Me.lblPaymentDescription.Size = New System.Drawing.Size(195, 16)
        Me.lblPaymentDescription.TabIndex = 7
        Me.lblPaymentDescription.Text = "A guest payment was recorded."
        '
        'lblPaymentTitle
        '
        Me.lblPaymentTitle.AutoSize = True
        Me.lblPaymentTitle.Location = New System.Drawing.Point(55, 4)
        Me.lblPaymentTitle.Name = "lblPaymentTitle"
        Me.lblPaymentTitle.Size = New System.Drawing.Size(122, 16)
        Me.lblPaymentTitle.TabIndex = 6
        Me.lblPaymentTitle.Text = "Payment Recieved"
        '
        'lblPaymentIcon
        '
        Me.lblPaymentIcon.AutoSize = True
        Me.lblPaymentIcon.Location = New System.Drawing.Point(8, 5)
        Me.lblPaymentIcon.Name = "lblPaymentIcon"
        Me.lblPaymentIcon.Size = New System.Drawing.Size(21, 16)
        Me.lblPaymentIcon.TabIndex = 5
        Me.lblPaymentIcon.Text = "$$"
        '
        'pnlActivityCheckIn
        '
        Me.pnlActivityCheckIn.Controls.Add(Me.Label4lblCheckInDescription)
        Me.pnlActivityCheckIn.Controls.Add(Me.lblCheckInTitle)
        Me.pnlActivityCheckIn.Controls.Add(Me.pnlCheckInIcon)
        Me.pnlActivityCheckIn.Location = New System.Drawing.Point(15, 45)
        Me.pnlActivityCheckIn.Name = "pnlActivityCheckIn"
        Me.pnlActivityCheckIn.Size = New System.Drawing.Size(468, 48)
        Me.pnlActivityCheckIn.TabIndex = 2
        '
        'Label4lblCheckInDescription
        '
        Me.Label4lblCheckInDescription.AutoSize = True
        Me.Label4lblCheckInDescription.Location = New System.Drawing.Point(55, 24)
        Me.Label4lblCheckInDescription.Name = "Label4lblCheckInDescription"
        Me.Label4lblCheckInDescription.Size = New System.Drawing.Size(181, 16)
        Me.Label4lblCheckInDescription.TabIndex = 4
        Me.Label4lblCheckInDescription.Text = "A Guest checked into a room."
        '
        'lblCheckInTitle
        '
        Me.lblCheckInTitle.AutoSize = True
        Me.lblCheckInTitle.Location = New System.Drawing.Point(55, 4)
        Me.lblCheckInTitle.Name = "lblCheckInTitle"
        Me.lblCheckInTitle.Size = New System.Drawing.Size(97, 16)
        Me.lblCheckInTitle.TabIndex = 3
        Me.lblCheckInTitle.Text = "Guest Check-in"
        '
        'pnlCheckInIcon
        '
        Me.pnlCheckInIcon.AutoSize = True
        Me.pnlCheckInIcon.Location = New System.Drawing.Point(8, 5)
        Me.pnlCheckInIcon.Name = "pnlCheckInIcon"
        Me.pnlCheckInIcon.Size = New System.Drawing.Size(21, 16)
        Me.pnlCheckInIcon.TabIndex = 2
        Me.pnlCheckInIcon.Text = "$$"
        '
        'lblActivityTitle
        '
        Me.lblActivityTitle.AutoSize = True
        Me.lblActivityTitle.Location = New System.Drawing.Point(15, 10)
        Me.lblActivityTitle.Name = "lblActivityTitle"
        Me.lblActivityTitle.Size = New System.Drawing.Size(95, 16)
        Me.lblActivityTitle.TabIndex = 1
        Me.lblActivityTitle.Text = "Recent Activity"
        '
        'pnlOccupancy
        '
        Me.pnlOccupancy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlOccupancy.Controls.Add(Me.lblAvailableCount)
        Me.pnlOccupancy.Controls.Add(Me.lblOccupiedCount)
        Me.pnlOccupancy.Controls.Add(Me.chartOccupancy)
        Me.pnlOccupancy.Controls.Add(Me.lblOccupancyRate)
        Me.pnlOccupancy.Controls.Add(Me.lblOccupancyTitle)
        Me.pnlOccupancy.Location = New System.Drawing.Point(25, 245)
        Me.pnlOccupancy.Name = "pnlOccupancy"
        Me.pnlOccupancy.Size = New System.Drawing.Size(556, 220)
        Me.pnlOccupancy.TabIndex = 5
        '
        'lblAvailableCount
        '
        Me.lblAvailableCount.AutoSize = True
        Me.lblAvailableCount.Location = New System.Drawing.Point(257, 125)
        Me.lblAvailableCount.Name = "lblAvailableCount"
        Me.lblAvailableCount.Size = New System.Drawing.Size(64, 16)
        Me.lblAvailableCount.TabIndex = 4
        Me.lblAvailableCount.Text = "Available"
        '
        'lblOccupiedCount
        '
        Me.lblOccupiedCount.AutoSize = True
        Me.lblOccupiedCount.Location = New System.Drawing.Point(255, 75)
        Me.lblOccupiedCount.Name = "lblOccupiedCount"
        Me.lblOccupiedCount.Size = New System.Drawing.Size(65, 16)
        Me.lblOccupiedCount.TabIndex = 3
        Me.lblOccupiedCount.Text = "Occupied"
        '
        'chartOccupancy
        '
        ChartArea4.Name = "ChartArea1"
        Me.chartOccupancy.ChartAreas.Add(ChartArea4)
        Legend4.Name = "Legend1"
        Me.chartOccupancy.Legends.Add(Legend4)
        Me.chartOccupancy.Location = New System.Drawing.Point(50, 40)
        Me.chartOccupancy.Name = "chartOccupancy"
        Series4.ChartArea = "ChartArea1"
        Series4.Legend = "Legend1"
        Series4.Name = "Series1"
        Me.chartOccupancy.Series.Add(Series4)
        Me.chartOccupancy.Size = New System.Drawing.Size(200, 165)
        Me.chartOccupancy.TabIndex = 2
        Me.chartOccupancy.Text = "Chart1"
        '
        'lblOccupancyRate
        '
        Me.lblOccupancyRate.AutoSize = True
        Me.lblOccupancyRate.Location = New System.Drawing.Point(60, 112)
        Me.lblOccupancyRate.Name = "lblOccupancyRate"
        Me.lblOccupancyRate.Size = New System.Drawing.Size(87, 16)
        Me.lblOccupancyRate.TabIndex = 1
        Me.lblOccupancyRate.Text = "0% Occupied"
        '
        'lblOccupancyTitle
        '
        Me.lblOccupancyTitle.AutoSize = True
        Me.lblOccupancyTitle.Location = New System.Drawing.Point(15, 12)
        Me.lblOccupancyTitle.Name = "lblOccupancyTitle"
        Me.lblOccupancyTitle.Size = New System.Drawing.Size(134, 16)
        Me.lblOccupancyTitle.TabIndex = 0
        Me.lblOccupancyTitle.Text = "Occupancy Overview"
        '
        'pnlParkingCard
        '
        Me.pnlParkingCard.Location = New System.Drawing.Point(805, 105)
        Me.pnlParkingCard.Name = "pnlParkingCard"
        Me.pnlParkingCard.Size = New System.Drawing.Size(250, 120)
        Me.pnlParkingCard.TabIndex = 4
        '
        'pnlBookingCard
        '
        Me.pnlBookingCard.Location = New System.Drawing.Point(545, 105)
        Me.pnlBookingCard.Name = "pnlBookingCard"
        Me.pnlBookingCard.Padding = New System.Windows.Forms.Padding(0, 0, 10, 0)
        Me.pnlBookingCard.Size = New System.Drawing.Size(250, 120)
        Me.pnlBookingCard.TabIndex = 4
        '
        'pnlGuestCard
        '
        Me.pnlGuestCard.Location = New System.Drawing.Point(285, 105)
        Me.pnlGuestCard.Name = "pnlGuestCard"
        Me.pnlGuestCard.Padding = New System.Windows.Forms.Padding(0, 0, 10, 0)
        Me.pnlGuestCard.Size = New System.Drawing.Size(250, 120)
        Me.pnlGuestCard.TabIndex = 4
        '
        'pnlRoomCard
        '
        Me.pnlRoomCard.Location = New System.Drawing.Point(25, 105)
        Me.pnlRoomCard.Name = "pnlRoomCard"
        Me.pnlRoomCard.Padding = New System.Windows.Forms.Padding(0, 0, 10, 0)
        Me.pnlRoomCard.Size = New System.Drawing.Size(250, 120)
        Me.pnlRoomCard.TabIndex = 3
        '
        'lblCurrentDate
        '
        Me.lblCurrentDate.AutoSize = True
        Me.lblCurrentDate.Location = New System.Drawing.Point(850, 25)
        Me.lblCurrentDate.Name = "lblCurrentDate"
        Me.lblCurrentDate.Size = New System.Drawing.Size(79, 16)
        Me.lblCurrentDate.TabIndex = 2
        Me.lblCurrentDate.Text = "Current date"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(25, 58)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(182, 16)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Welcome back, Administrator"
        '
        'lblPageTitle
        '
        Me.lblPageTitle.AutoSize = True
        Me.lblPageTitle.Location = New System.Drawing.Point(25, 20)
        Me.lblPageTitle.Name = "lblPageTitle"
        Me.lblPageTitle.Size = New System.Drawing.Size(134, 16)
        Me.lblPageTitle.TabIndex = 0
        Me.lblPageTitle.Text = "Dashboard Overview"
        '
        'ucDashboard
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Panel1)
        Me.Name = "ucDashboard"
        Me.Size = New System.Drawing.Size(1406, 667)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.pnlQuickActions.ResumeLayout(False)
        Me.pnlQuickActions.PerformLayout()
        Me.pnlActivity.ResumeLayout(False)
        Me.pnlActivity.PerformLayout()
        Me.pnlActivityParking.ResumeLayout(False)
        Me.pnlActivityParking.PerformLayout()
        Me.pnlActivityPayment.ResumeLayout(False)
        Me.pnlActivityPayment.PerformLayout()
        Me.pnlActivityCheckIn.ResumeLayout(False)
        Me.pnlActivityCheckIn.PerformLayout()
        Me.pnlOccupancy.ResumeLayout(False)
        Me.pnlOccupancy.PerformLayout()
        CType(Me.chartOccupancy, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents pnlQuickActions As Panel
    Friend WithEvents btnAssignParking As Button
    Friend WithEvents btnRecordPayment As Button
    Friend WithEvents btnCheckIn As Button
    Friend WithEvents btnNewBooking As Button
    Friend WithEvents lblQuickActions As Label
    Friend WithEvents pnlActivity As Panel
    Friend WithEvents pnlActivityParking As Panel
    Friend WithEvents Label5 As Label
    Friend WithEvents lblParkingTitle As Label
    Friend WithEvents lblParkingIcon As Label
    Friend WithEvents pnlActivityPayment As Panel
    Friend WithEvents lblPaymentDescription As Label
    Friend WithEvents lblPaymentTitle As Label
    Friend WithEvents lblPaymentIcon As Label
    Friend WithEvents pnlActivityCheckIn As Panel
    Friend WithEvents Label4lblCheckInDescription As Label
    Friend WithEvents lblCheckInTitle As Label
    Friend WithEvents pnlCheckInIcon As Label
    Friend WithEvents lblActivityTitle As Label
    Friend WithEvents pnlOccupancy As Panel
    Friend WithEvents lblAvailableCount As Label
    Friend WithEvents lblOccupiedCount As Label
    Friend WithEvents chartOccupancy As DataVisualization.Charting.Chart
    Friend WithEvents lblOccupancyRate As Label
    Friend WithEvents lblOccupancyTitle As Label
    Friend WithEvents pnlParkingCard As Panel
    Friend WithEvents pnlBookingCard As Panel
    Friend WithEvents pnlGuestCard As Panel
    Friend WithEvents pnlRoomCard As Panel
    Friend WithEvents lblCurrentDate As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents lblPageTitle As Label
End Class
