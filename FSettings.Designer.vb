<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FSettings
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FSettings))
        Me.BFPlayerPath = New System.Windows.Forms.Button()
        Me.BFAccept = New System.Windows.Forms.Button()
        Me.BFCancel = New System.Windows.Forms.Button()
        Me.THidden = New System.Windows.Forms.TextBox()
        Me.CAutoWatched = New KMovies.NSCheckBox()
        Me.BClose = New KMovies.FlatLabel()
        Me.BCancel = New KMovies.FlatButton()
        Me.BAccept = New KMovies.FlatButton()
        Me.BPlayerPath = New KMovies.FlatButton()
        Me.LAutoWatched = New KMovies.FlatLabel()
        Me.LPlayerPath = New KMovies.FlatLabel()
        Me.LMoviesPath = New KMovies.FlatLabel()
        Me.LFormTitle = New KMovies.FlatLabel()
        Me.TPlayerPath = New KMovies.NSTextBox()
        Me.BFMoviesPath = New System.Windows.Forms.Button()
        Me.BMoviesPath = New KMovies.FlatButton()
        Me.TMoviesPath = New KMovies.NSTextBox()
        Me.BFMin = New System.Windows.Forms.Button()
        Me.TMin = New KMovies.NSTextBox()
        Me.LMin = New KMovies.FlatLabel()
        Me.LDeleteImported = New KMovies.FlatLabel()
        Me.CDeleteImported = New KMovies.NSCheckBox()
        Me.SuspendLayout()
        '
        'BFPlayerPath
        '
        Me.BFPlayerPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFPlayerPath.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFPlayerPath.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFPlayerPath.Location = New System.Drawing.Point(101, 62)
        Me.BFPlayerPath.Name = "BFPlayerPath"
        Me.BFPlayerPath.Size = New System.Drawing.Size(246, 21)
        Me.BFPlayerPath.TabIndex = 38
        Me.BFPlayerPath.TabStop = False
        Me.BFPlayerPath.Text = "..."
        Me.BFPlayerPath.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFPlayerPath.UseVisualStyleBackColor = True
        '
        'BFAccept
        '
        Me.BFAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFAccept.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFAccept.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFAccept.Location = New System.Drawing.Point(32, 148)
        Me.BFAccept.Name = "BFAccept"
        Me.BFAccept.Size = New System.Drawing.Size(125, 25)
        Me.BFAccept.TabIndex = 39
        Me.BFAccept.TabStop = False
        Me.BFAccept.Text = "..."
        Me.BFAccept.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFAccept.UseVisualStyleBackColor = True
        '
        'BFCancel
        '
        Me.BFCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFCancel.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFCancel.Location = New System.Drawing.Point(207, 148)
        Me.BFCancel.Name = "BFCancel"
        Me.BFCancel.Size = New System.Drawing.Size(125, 25)
        Me.BFCancel.TabIndex = 40
        Me.BFCancel.TabStop = False
        Me.BFCancel.Text = "..."
        Me.BFCancel.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFCancel.UseVisualStyleBackColor = True
        '
        'THidden
        '
        Me.THidden.Location = New System.Drawing.Point(-150, -150)
        Me.THidden.Multiline = True
        Me.THidden.Name = "THidden"
        Me.THidden.ReadOnly = True
        Me.THidden.ShortcutsEnabled = False
        Me.THidden.Size = New System.Drawing.Size(31, 32)
        Me.THidden.TabIndex = 0
        '
        'CAutoWatched
        '
        Me.CAutoWatched.BackColor = System.Drawing.Color.Black
        Me.CAutoWatched.Checked = True
        Me.CAutoWatched.Location = New System.Drawing.Point(33, 94)
        Me.CAutoWatched.Name = "CAutoWatched"
        Me.CAutoWatched.Size = New System.Drawing.Size(15, 19)
        Me.CAutoWatched.TabIndex = 41
        '
        'BClose
        '
        Me.BClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BClose.AutoSize = True
        Me.BClose.BackColor = System.Drawing.Color.Transparent
        Me.BClose.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BClose.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BClose.Location = New System.Drawing.Point(340, -4)
        Me.BClose.Name = "BClose"
        Me.BClose.Size = New System.Drawing.Size(21, 26)
        Me.BClose.TabIndex = 36
        Me.BClose.Text = "ꭗ"
        '
        'BCancel
        '
        Me.BCancel.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.BCancel.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BCancel.ForeColor = System.Drawing.Color.DimGray
        Me.BCancel.Hover = True
        Me.BCancel.Location = New System.Drawing.Point(208, 149)
        Me.BCancel.Name = "BCancel"
        Me.BCancel.Rounded = False
        Me.BCancel.Size = New System.Drawing.Size(123, 23)
        Me.BCancel.TabIndex = 5
        Me.BCancel.Text = "Cancel"
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
        Me.BAccept.Location = New System.Drawing.Point(33, 149)
        Me.BAccept.Name = "BAccept"
        Me.BAccept.Rounded = False
        Me.BAccept.Size = New System.Drawing.Size(123, 23)
        Me.BAccept.TabIndex = 4
        Me.BAccept.Text = "Accept"
        Me.BAccept.TextColor = System.Drawing.Color.DarkGray
        '
        'BPlayerPath
        '
        Me.BPlayerPath.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BPlayerPath.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BPlayerPath.Cursor = System.Windows.Forms.Cursors.Default
        Me.BPlayerPath.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BPlayerPath.ForeColor = System.Drawing.Color.DarkGray
        Me.BPlayerPath.Hover = True
        Me.BPlayerPath.Location = New System.Drawing.Point(315, 64)
        Me.BPlayerPath.Name = "BPlayerPath"
        Me.BPlayerPath.Rounded = False
        Me.BPlayerPath.Size = New System.Drawing.Size(30, 16)
        Me.BPlayerPath.TabIndex = 3
        Me.BPlayerPath.TabStop = False
        Me.BPlayerPath.Text = "..."
        Me.BPlayerPath.TextColor = System.Drawing.Color.DarkGray
        '
        'LAutoWatched
        '
        Me.LAutoWatched.AutoSize = True
        Me.LAutoWatched.BackColor = System.Drawing.Color.Transparent
        Me.LAutoWatched.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LAutoWatched.ForeColor = System.Drawing.Color.DarkGray
        Me.LAutoWatched.Location = New System.Drawing.Point(50, 94)
        Me.LAutoWatched.Name = "LAutoWatched"
        Me.LAutoWatched.Size = New System.Drawing.Size(132, 17)
        Me.LAutoWatched.TabIndex = 3
        Me.LAutoWatched.Text = "Auto add to Watched"
        '
        'LPlayerPath
        '
        Me.LPlayerPath.AutoSize = True
        Me.LPlayerPath.BackColor = System.Drawing.Color.Transparent
        Me.LPlayerPath.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LPlayerPath.ForeColor = System.Drawing.Color.DarkGray
        Me.LPlayerPath.Location = New System.Drawing.Point(21, 61)
        Me.LPlayerPath.Name = "LPlayerPath"
        Me.LPlayerPath.Size = New System.Drawing.Size(75, 17)
        Me.LPlayerPath.TabIndex = 3
        Me.LPlayerPath.Text = "Player Path:"
        '
        'LMoviesPath
        '
        Me.LMoviesPath.AutoSize = True
        Me.LMoviesPath.BackColor = System.Drawing.Color.Transparent
        Me.LMoviesPath.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LMoviesPath.ForeColor = System.Drawing.Color.DarkGray
        Me.LMoviesPath.Location = New System.Drawing.Point(15, 38)
        Me.LMoviesPath.Name = "LMoviesPath"
        Me.LMoviesPath.Size = New System.Drawing.Size(82, 17)
        Me.LMoviesPath.TabIndex = 5
        Me.LMoviesPath.Text = "Movies Path:"
        '
        'LFormTitle
        '
        Me.LFormTitle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LFormTitle.BackColor = System.Drawing.Color.Transparent
        Me.LFormTitle.Font = New System.Drawing.Font("Calibri Light", 17.0!)
        Me.LFormTitle.ForeColor = System.Drawing.Color.DarkGray
        Me.LFormTitle.Location = New System.Drawing.Point(12, -1)
        Me.LFormTitle.Name = "LFormTitle"
        Me.LFormTitle.Size = New System.Drawing.Size(343, 34)
        Me.LFormTitle.TabIndex = 36
        Me.LFormTitle.Text = "Settings"
        Me.LFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TPlayerPath
        '
        Me.TPlayerPath.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TPlayerPath.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TPlayerPath.Cursor = System.Windows.Forms.Cursors.Default
        Me.TPlayerPath.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TPlayerPath.ForeColor = System.Drawing.Color.DarkGray
        Me.TPlayerPath.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TPlayerPath.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TPlayerPath.HideSelection = False
        Me.TPlayerPath.Location = New System.Drawing.Point(102, 63)
        Me.TPlayerPath.MaxLength = 32767
        Me.TPlayerPath.Multiline = False
        Me.TPlayerPath.Name = "TPlayerPath"
        Me.TPlayerPath.ReadOnly = False
        Me.TPlayerPath.Rounded = 0
        Me.TPlayerPath.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TPlayerPath.SelectedText = ""
        Me.TPlayerPath.SelectionLength = 0
        Me.TPlayerPath.SelectionStart = 0
        Me.TPlayerPath.ShortcutsEnabled = True
        Me.TPlayerPath.Size = New System.Drawing.Size(212, 19)
        Me.TPlayerPath.TabIndex = 62
        Me.TPlayerPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TPlayerPath.UseSystemPasswordChar = False
        Me.TPlayerPath.WordWrap = False
        '
        'BFMoviesPath
        '
        Me.BFMoviesPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFMoviesPath.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFMoviesPath.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFMoviesPath.Location = New System.Drawing.Point(101, 37)
        Me.BFMoviesPath.Name = "BFMoviesPath"
        Me.BFMoviesPath.Size = New System.Drawing.Size(246, 21)
        Me.BFMoviesPath.TabIndex = 38
        Me.BFMoviesPath.TabStop = False
        Me.BFMoviesPath.Text = "..."
        Me.BFMoviesPath.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFMoviesPath.UseVisualStyleBackColor = True
        '
        'BMoviesPath
        '
        Me.BMoviesPath.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BMoviesPath.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BMoviesPath.Cursor = System.Windows.Forms.Cursors.Default
        Me.BMoviesPath.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BMoviesPath.ForeColor = System.Drawing.Color.DarkGray
        Me.BMoviesPath.Hover = True
        Me.BMoviesPath.Location = New System.Drawing.Point(315, 39)
        Me.BMoviesPath.Name = "BMoviesPath"
        Me.BMoviesPath.Rounded = False
        Me.BMoviesPath.Size = New System.Drawing.Size(30, 16)
        Me.BMoviesPath.TabIndex = 3
        Me.BMoviesPath.TabStop = False
        Me.BMoviesPath.Text = "..."
        Me.BMoviesPath.TextColor = System.Drawing.Color.DarkGray
        '
        'TMoviesPath
        '
        Me.TMoviesPath.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TMoviesPath.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TMoviesPath.Cursor = System.Windows.Forms.Cursors.Default
        Me.TMoviesPath.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TMoviesPath.ForeColor = System.Drawing.Color.DarkGray
        Me.TMoviesPath.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TMoviesPath.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TMoviesPath.HideSelection = False
        Me.TMoviesPath.Location = New System.Drawing.Point(102, 38)
        Me.TMoviesPath.MaxLength = 32767
        Me.TMoviesPath.Multiline = False
        Me.TMoviesPath.Name = "TMoviesPath"
        Me.TMoviesPath.ReadOnly = False
        Me.TMoviesPath.Rounded = 0
        Me.TMoviesPath.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TMoviesPath.SelectedText = ""
        Me.TMoviesPath.SelectionLength = 0
        Me.TMoviesPath.SelectionStart = 0
        Me.TMoviesPath.ShortcutsEnabled = True
        Me.TMoviesPath.Size = New System.Drawing.Size(212, 19)
        Me.TMoviesPath.TabIndex = 62
        Me.TMoviesPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TMoviesPath.UseSystemPasswordChar = False
        Me.TMoviesPath.WordWrap = False
        '
        'BFMin
        '
        Me.BFMin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFMin.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFMin.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFMin.Location = New System.Drawing.Point(181, 93)
        Me.BFMin.Name = "BFMin"
        Me.BFMin.Size = New System.Drawing.Size(40, 21)
        Me.BFMin.TabIndex = 38
        Me.BFMin.TabStop = False
        Me.BFMin.Text = "..."
        Me.BFMin.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFMin.UseVisualStyleBackColor = True
        '
        'TMin
        '
        Me.TMin.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TMin.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TMin.Cursor = System.Windows.Forms.Cursors.Default
        Me.TMin.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TMin.ForeColor = System.Drawing.Color.DarkGray
        Me.TMin.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TMin.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TMin.HideSelection = False
        Me.TMin.Location = New System.Drawing.Point(182, 94)
        Me.TMin.MaxLength = 3
        Me.TMin.Multiline = False
        Me.TMin.Name = "TMin"
        Me.TMin.ReadOnly = False
        Me.TMin.Rounded = 0
        Me.TMin.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TMin.SelectedText = ""
        Me.TMin.SelectionLength = 0
        Me.TMin.SelectionStart = 0
        Me.TMin.ShortcutsEnabled = True
        Me.TMin.Size = New System.Drawing.Size(38, 19)
        Me.TMin.TabIndex = 62
        Me.TMin.Text = "20"
        Me.TMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TMin.UseSystemPasswordChar = False
        Me.TMin.WordWrap = False
        '
        'LMin
        '
        Me.LMin.AutoSize = True
        Me.LMin.BackColor = System.Drawing.Color.Transparent
        Me.LMin.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LMin.ForeColor = System.Drawing.Color.DarkGray
        Me.LMin.Location = New System.Drawing.Point(220, 94)
        Me.LMin.Name = "LMin"
        Me.LMin.Size = New System.Drawing.Size(32, 17)
        Me.LMin.TabIndex = 3
        Me.LMin.Text = "min."
        '
        'LDeleteImported
        '
        Me.LDeleteImported.AutoSize = True
        Me.LDeleteImported.BackColor = System.Drawing.Color.Transparent
        Me.LDeleteImported.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LDeleteImported.ForeColor = System.Drawing.Color.DarkGray
        Me.LDeleteImported.Location = New System.Drawing.Point(50, 117)
        Me.LDeleteImported.Name = "LDeleteImported"
        Me.LDeleteImported.Size = New System.Drawing.Size(179, 17)
        Me.LDeleteImported.TabIndex = 3
        Me.LDeleteImported.Text = "Delete movies after imported"
        '
        'CDeleteImported
        '
        Me.CDeleteImported.BackColor = System.Drawing.Color.Black
        Me.CDeleteImported.Checked = True
        Me.CDeleteImported.Location = New System.Drawing.Point(33, 117)
        Me.CDeleteImported.Name = "CDeleteImported"
        Me.CDeleteImported.Size = New System.Drawing.Size(15, 19)
        Me.CDeleteImported.TabIndex = 41
        '
        'FSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.ClientSize = New System.Drawing.Size(356, 184)
        Me.ControlBox = False
        Me.Controls.Add(Me.TMoviesPath)
        Me.Controls.Add(Me.TMin)
        Me.Controls.Add(Me.TPlayerPath)
        Me.Controls.Add(Me.CDeleteImported)
        Me.Controls.Add(Me.CAutoWatched)
        Me.Controls.Add(Me.THidden)
        Me.Controls.Add(Me.BClose)
        Me.Controls.Add(Me.BCancel)
        Me.Controls.Add(Me.BMoviesPath)
        Me.Controls.Add(Me.BAccept)
        Me.Controls.Add(Me.BPlayerPath)
        Me.Controls.Add(Me.LPlayerPath)
        Me.Controls.Add(Me.BFMoviesPath)
        Me.Controls.Add(Me.BFMin)
        Me.Controls.Add(Me.LMoviesPath)
        Me.Controls.Add(Me.BFPlayerPath)
        Me.Controls.Add(Me.BFAccept)
        Me.Controls.Add(Me.BFCancel)
        Me.Controls.Add(Me.LFormTitle)
        Me.Controls.Add(Me.LMin)
        Me.Controls.Add(Me.LDeleteImported)
        Me.Controls.Add(Me.LAutoWatched)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FSettings"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.TransparencyKey = System.Drawing.Color.Fuchsia
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BCancel As FlatButton
    Friend WithEvents BAccept As FlatButton
    Friend WithEvents BPlayerPath As FlatButton
    Friend WithEvents LPlayerPath As FlatLabel
    Friend WithEvents LMoviesPath As FlatLabel
    Friend WithEvents BClose As FlatLabel
    Friend WithEvents LFormTitle As FlatLabel
    Friend WithEvents BFPlayerPath As Button
    Friend WithEvents BFAccept As Button
    Friend WithEvents BFCancel As Button
    Friend WithEvents THidden As TextBox
    Friend WithEvents LAutoWatched As FlatLabel
    Friend WithEvents CAutoWatched As NSCheckBox
    Friend WithEvents TPlayerPath As NSTextBox
    Friend WithEvents BFMoviesPath As Button
    Friend WithEvents BMoviesPath As FlatButton
    Friend WithEvents TMoviesPath As NSTextBox
    Friend WithEvents BFMin As Button
    Friend WithEvents TMin As NSTextBox
    Friend WithEvents LMin As FlatLabel
    Friend WithEvents LDeleteImported As FlatLabel
    Friend WithEvents CDeleteImported As NSCheckBox
End Class
