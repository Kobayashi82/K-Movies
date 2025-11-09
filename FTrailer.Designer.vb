<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FTrailer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FTrailer))
        Me.PPanel = New System.Windows.Forms.Panel()
        Me.LTitle = New System.Windows.Forms.Label()
        Me.PLogo = New System.Windows.Forms.PictureBox()
        Me.BMin = New KMovies.FlatLabel()
        Me.BMax = New KMovies.FlatLabel()
        Me.BClose = New KMovies.FlatLabel()
        Me.LTrailer = New System.Windows.Forms.Label()
        Me.Separador = New System.Windows.Forms.GroupBox()
        Me.WTrailer = New Microsoft.Web.WebView2.WinForms.WebView2()
        Me.PPanel.SuspendLayout()
        CType(Me.PLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WTrailer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PPanel
        '
        Me.PPanel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PPanel.BackColor = System.Drawing.Color.FromArgb(CType(CType(45, Byte), Integer), CType(CType(47, Byte), Integer), CType(CType(49, Byte), Integer))
        Me.PPanel.Controls.Add(Me.LTitle)
        Me.PPanel.Controls.Add(Me.PLogo)
        Me.PPanel.Controls.Add(Me.BMin)
        Me.PPanel.Controls.Add(Me.BMax)
        Me.PPanel.Controls.Add(Me.BClose)
        Me.PPanel.Location = New System.Drawing.Point(1, 1)
        Me.PPanel.Name = "PPanel"
        Me.PPanel.Size = New System.Drawing.Size(798, 37)
        Me.PPanel.TabIndex = 49
        '
        'LTitle
        '
        Me.LTitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LTitle.Font = New System.Drawing.Font("Bell MT", 17.0!)
        Me.LTitle.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LTitle.Location = New System.Drawing.Point(82, 1)
        Me.LTitle.Name = "LTitle"
        Me.LTitle.Size = New System.Drawing.Size(634, 34)
        Me.LTitle.TabIndex = 50
        Me.LTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'PLogo
        '
        Me.PLogo.BackColor = System.Drawing.Color.Transparent
        Me.PLogo.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PLogo.Image = CType(resources.GetObject("PLogo.Image"), System.Drawing.Image)
        Me.PLogo.Location = New System.Drawing.Point(6, 2)
        Me.PLogo.Name = "PLogo"
        Me.PLogo.Size = New System.Drawing.Size(37, 31)
        Me.PLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PLogo.TabIndex = 42
        Me.PLogo.TabStop = False
        '
        'BMin
        '
        Me.BMin.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BMin.AutoSize = True
        Me.BMin.BackColor = System.Drawing.Color.Transparent
        Me.BMin.Font = New System.Drawing.Font("Marlett", 11.25!)
        Me.BMin.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BMin.Location = New System.Drawing.Point(721, 9)
        Me.BMin.Name = "BMin"
        Me.BMin.Size = New System.Drawing.Size(22, 15)
        Me.BMin.TabIndex = 44
        Me.BMin.Text = "0"
        '
        'BMax
        '
        Me.BMax.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BMax.AutoSize = True
        Me.BMax.BackColor = System.Drawing.Color.Transparent
        Me.BMax.Font = New System.Drawing.Font("Marlett", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(2, Byte))
        Me.BMax.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BMax.Location = New System.Drawing.Point(745, 10)
        Me.BMax.Name = "BMax"
        Me.BMax.Size = New System.Drawing.Size(23, 15)
        Me.BMax.TabIndex = 44
        Me.BMax.Text = "1"
        '
        'BClose
        '
        Me.BClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BClose.AutoSize = True
        Me.BClose.BackColor = System.Drawing.Color.Transparent
        Me.BClose.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BClose.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BClose.Location = New System.Drawing.Point(773, 1)
        Me.BClose.Name = "BClose"
        Me.BClose.Size = New System.Drawing.Size(21, 26)
        Me.BClose.TabIndex = 45
        Me.BClose.Text = "ꭗ"
        '
        'LTrailer
        '
        Me.LTrailer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LTrailer.BackColor = System.Drawing.Color.Transparent
        Me.LTrailer.Font = New System.Drawing.Font("Bell MT", 18.0!)
        Me.LTrailer.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LTrailer.Location = New System.Drawing.Point(287, 405)
        Me.LTrailer.Name = "LTrailer"
        Me.LTrailer.Size = New System.Drawing.Size(245, 31)
        Me.LTrailer.TabIndex = 48
        Me.LTrailer.Text = "Loading, Please Wait... "
        Me.LTrailer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Separador
        '
        Me.Separador.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Separador.BackColor = System.Drawing.Color.Black
        Me.Separador.Location = New System.Drawing.Point(0, 36)
        Me.Separador.Name = "Separador"
        Me.Separador.Size = New System.Drawing.Size(803, 2)
        Me.Separador.TabIndex = 47
        Me.Separador.TabStop = False
        '
        'WTrailer
        '
        Me.WTrailer.AllowExternalDrop = True
        Me.WTrailer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.WTrailer.BackColor = System.Drawing.Color.Black
        Me.WTrailer.CreationProperties = Nothing
        Me.WTrailer.DefaultBackgroundColor = System.Drawing.Color.Black
        Me.WTrailer.Location = New System.Drawing.Point(2, 39)
        Me.WTrailer.Name = "WTrailer"
        Me.WTrailer.Size = New System.Drawing.Size(796, 402)
        Me.WTrailer.TabIndex = 46
        Me.WTrailer.ZoomFactor = 1.0R
        '
        'FTrailer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(800, 443)
        Me.ControlBox = False
        Me.Controls.Add(Me.LTrailer)
        Me.Controls.Add(Me.Separador)
        Me.Controls.Add(Me.WTrailer)
        Me.Controls.Add(Me.PPanel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "FTrailer"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Trailer"
        Me.TransparencyKey = System.Drawing.Color.Fuchsia
        Me.PPanel.ResumeLayout(False)
        Me.PPanel.PerformLayout()
        CType(Me.PLogo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.WTrailer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PPanel As Panel
    Friend WithEvents PLogo As PictureBox
    Friend WithEvents LTrailer As Label
    Friend WithEvents Separador As GroupBox
    Friend WithEvents BMax As FlatLabel
    Friend WithEvents BClose As FlatLabel
    Friend WithEvents WTrailer As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents LTitle As Label
    Friend WithEvents BMin As FlatLabel
End Class
