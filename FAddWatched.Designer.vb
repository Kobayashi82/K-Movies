<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FAddWatched
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
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

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FAddWatched))
        Me.PCover = New System.Windows.Forms.PictureBox()
        Me.BFCancel = New System.Windows.Forms.Button()
        Me.BFAccept = New System.Windows.Forms.Button()
        Me.LMessage = New System.Windows.Forms.Label()
        Me.LTitle = New System.Windows.Forms.Label()
        Me.BCancel = New KMovies.FlatButton()
        Me.BAccept = New KMovies.FlatButton()
        Me.BClose = New KMovies.FlatLabel()
        Me.BFRemove = New System.Windows.Forms.Button()
        Me.BRemove = New KMovies.FlatButton()
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PCover
        '
        Me.PCover.BackColor = System.Drawing.Color.Transparent
        Me.PCover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PCover.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PCover.Location = New System.Drawing.Point(12, 12)
        Me.PCover.Name = "PCover"
        Me.PCover.Size = New System.Drawing.Size(210, 300)
        Me.PCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PCover.TabIndex = 58
        Me.PCover.TabStop = False
        '
        'BFCancel
        '
        Me.BFCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFCancel.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFCancel.Location = New System.Drawing.Point(367, 282)
        Me.BFCancel.Name = "BFCancel"
        Me.BFCancel.Size = New System.Drawing.Size(88, 30)
        Me.BFCancel.TabIndex = 68
        Me.BFCancel.TabStop = False
        Me.BFCancel.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFCancel.UseVisualStyleBackColor = True
        '
        'BFAccept
        '
        Me.BFAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFAccept.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFAccept.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFAccept.Location = New System.Drawing.Point(235, 282)
        Me.BFAccept.Name = "BFAccept"
        Me.BFAccept.Size = New System.Drawing.Size(118, 30)
        Me.BFAccept.TabIndex = 69
        Me.BFAccept.TabStop = False
        Me.BFAccept.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFAccept.UseVisualStyleBackColor = True
        '
        'LMessage
        '
        Me.LMessage.BackColor = System.Drawing.Color.Transparent
        Me.LMessage.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LMessage.ForeColor = System.Drawing.Color.LightGray
        Me.LMessage.Location = New System.Drawing.Point(228, 108)
        Me.LMessage.Name = "LMessage"
        Me.LMessage.Size = New System.Drawing.Size(236, 123)
        Me.LMessage.TabIndex = 70
        Me.LMessage.Text = "It's look like you watched this movie" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Do you want to delete it?"
        Me.LMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LTitle
        '
        Me.LTitle.BackColor = System.Drawing.Color.Transparent
        Me.LTitle.Font = New System.Drawing.Font("Bell MT", 21.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LTitle.ForeColor = System.Drawing.Color.LightGray
        Me.LTitle.Location = New System.Drawing.Point(232, 28)
        Me.LTitle.Name = "LTitle"
        Me.LTitle.Size = New System.Drawing.Size(232, 42)
        Me.LTitle.TabIndex = 71
        Me.LTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BCancel
        '
        Me.BCancel.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.BCancel.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BCancel.ForeColor = System.Drawing.Color.DimGray
        Me.BCancel.Hover = True
        Me.BCancel.Location = New System.Drawing.Point(368, 283)
        Me.BCancel.Name = "BCancel"
        Me.BCancel.Rounded = False
        Me.BCancel.Size = New System.Drawing.Size(86, 28)
        Me.BCancel.TabIndex = 66
        Me.BCancel.Text = "Keep it"
        Me.BCancel.TextColor = System.Drawing.Color.DarkGray
        '
        'BAccept
        '
        Me.BAccept.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BAccept.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BAccept.Cursor = System.Windows.Forms.Cursors.Default
        Me.BAccept.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BAccept.ForeColor = System.Drawing.Color.DimGray
        Me.BAccept.Hover = True
        Me.BAccept.Location = New System.Drawing.Point(236, 283)
        Me.BAccept.Name = "BAccept"
        Me.BAccept.Rounded = False
        Me.BAccept.Size = New System.Drawing.Size(116, 28)
        Me.BAccept.TabIndex = 67
        Me.BAccept.Text = "Delete it"
        Me.BAccept.TextColor = System.Drawing.Color.DarkGray
        '
        'BClose
        '
        Me.BClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BClose.AutoSize = True
        Me.BClose.BackColor = System.Drawing.Color.Transparent
        Me.BClose.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BClose.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BClose.Location = New System.Drawing.Point(452, -3)
        Me.BClose.Name = "BClose"
        Me.BClose.Size = New System.Drawing.Size(21, 26)
        Me.BClose.TabIndex = 37
        Me.BClose.Text = "ꭗ"
        '
        'BFRemove
        '
        Me.BFRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFRemove.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFRemove.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFRemove.Location = New System.Drawing.Point(259, 248)
        Me.BFRemove.Name = "BFRemove"
        Me.BFRemove.Size = New System.Drawing.Size(173, 30)
        Me.BFRemove.TabIndex = 69
        Me.BFRemove.TabStop = False
        Me.BFRemove.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFRemove.UseVisualStyleBackColor = True
        '
        'BRemove
        '
        Me.BRemove.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BRemove.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BRemove.Cursor = System.Windows.Forms.Cursors.Default
        Me.BRemove.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BRemove.ForeColor = System.Drawing.Color.DimGray
        Me.BRemove.Hover = True
        Me.BRemove.Location = New System.Drawing.Point(260, 249)
        Me.BRemove.Name = "BRemove"
        Me.BRemove.Rounded = False
        Me.BRemove.Size = New System.Drawing.Size(171, 28)
        Me.BRemove.TabIndex = 67
        Me.BRemove.Text = "Remove from Watched"
        Me.BRemove.TextColor = System.Drawing.Color.DarkGray
        '
        'FAddWatched
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.ClientSize = New System.Drawing.Size(476, 324)
        Me.ControlBox = False
        Me.Controls.Add(Me.LTitle)
        Me.Controls.Add(Me.LMessage)
        Me.Controls.Add(Me.BCancel)
        Me.Controls.Add(Me.BFCancel)
        Me.Controls.Add(Me.BRemove)
        Me.Controls.Add(Me.BFRemove)
        Me.Controls.Add(Me.BAccept)
        Me.Controls.Add(Me.BFAccept)
        Me.Controls.Add(Me.PCover)
        Me.Controls.Add(Me.BClose)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.KeyPreview = True
        Me.Name = "FAddWatched"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BClose As FlatLabel
    Friend WithEvents PCover As PictureBox
    Friend WithEvents BCancel As FlatButton
    Friend WithEvents BFCancel As Button
    Friend WithEvents BAccept As FlatButton
    Friend WithEvents BFAccept As Button
    Friend WithEvents LMessage As Label
    Friend WithEvents LTitle As Label
    Friend WithEvents BFRemove As Button
    Friend WithEvents BRemove As FlatButton
End Class
