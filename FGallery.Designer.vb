<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FGallery
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FGallery))
        Me.PCover = New System.Windows.Forms.PictureBox()
        Me.LPlot = New System.Windows.Forms.Label()
        Me.THidden = New System.Windows.Forms.TextBox()
        Me.LYear = New System.Windows.Forms.Label()
        Me.LIYear = New System.Windows.Forms.Label()
        Me.LRating = New System.Windows.Forms.Label()
        Me.LIRating = New System.Windows.Forms.Label()
        Me.LDuration = New System.Windows.Forms.Label()
        Me.LIDuration = New System.Windows.Forms.Label()
        Me.LGenre = New System.Windows.Forms.Label()
        Me.LIGenre = New System.Windows.Forms.Label()
        Me.LCountry = New System.Windows.Forms.Label()
        Me.LICountry = New System.Windows.Forms.Label()
        Me.LDirector = New System.Windows.Forms.Label()
        Me.LIDirector = New System.Windows.Forms.Label()
        Me.LActor = New System.Windows.Forms.Label()
        Me.LIActor = New System.Windows.Forms.Label()
        Me.LFYear = New System.Windows.Forms.Label()
        Me.LFLanguage = New System.Windows.Forms.Label()
        Me.LFCountry = New System.Windows.Forms.Label()
        Me.LFDirector = New System.Windows.Forms.Label()
        Me.LFActor = New System.Windows.Forms.Label()
        Me.LFGenre = New System.Windows.Forms.Label()
        Me.LTitle = New System.Windows.Forms.Label()
        Me.LFRating = New System.Windows.Forms.Label()
        Me.LFDurationThan = New System.Windows.Forms.Label()
        Me.LLanguage = New System.Windows.Forms.Label()
        Me.LILanguage = New System.Windows.Forms.Label()
        Me.LFRatingThan = New System.Windows.Forms.Label()
        Me.LFDuration = New System.Windows.Forms.Label()
        Me.LAbout = New System.Windows.Forms.Label()
        Me.LAdded = New System.Windows.Forms.Label()
        Me.LIAdded = New System.Windows.Forms.Label()
        Me.LCountInfo = New System.Windows.Forms.Label()
        Me.LCountImdb = New System.Windows.Forms.Label()
        Me.LRandom = New System.Windows.Forms.Label()
        Me.LCYear = New System.Windows.Forms.Label()
        Me.LCGenre = New System.Windows.Forms.Label()
        Me.LCCountry = New System.Windows.Forms.Label()
        Me.LCActor = New System.Windows.Forms.Label()
        Me.LCLanguage = New System.Windows.Forms.Label()
        Me.LCDirector = New System.Windows.Forms.Label()
        Me.LBMovies = New System.Windows.Forms.Label()
        Me.LCSort = New System.Windows.Forms.Label()
        Me.LSort = New System.Windows.Forms.Label()
        Me.TCGenre = New KMovies.NSTextBox()
        Me.Separator = New KMovies.LineSeparator()
        Me.LAz = New KMovies.FlatText()
        Me.TSort = New KMovies.NSTextBox()
        Me.CSort = New KMovies.FlatComboBox()
        Me.TCDirector = New KMovies.NSTextBox()
        Me.TCLanguage = New KMovies.NSTextBox()
        Me.TCActor = New KMovies.NSTextBox()
        Me.TCCountry = New KMovies.NSTextBox()
        Me.TCYear = New KMovies.NSTextBox()
        Me.CActor = New KMovies.FlatComboBox()
        Me.CDirector = New KMovies.FlatComboBox()
        Me.CCountry = New KMovies.FlatComboBox()
        Me.CLanguage = New KMovies.FlatComboBox()
        Me.TSearch = New KMovies.NSTextBox()
        Me.TDuration = New KMovies.NSTextBox()
        Me.TRating = New KMovies.NSTextBox()
        Me.CYear = New KMovies.FlatComboBox()
        Me.BClose = New KMovies.FlatLabel()
        Me.BMinimize = New KMovies.FlatLabel()
        Me.LGallery = New KMovies.FlatLabel()
        Me.MGMenu = New KMovies.FlatContextMenuStrip()
        Me.MGAdd = New System.Windows.Forms.ToolStripMenuItem()
        Me.MGSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MGSettings = New System.Windows.Forms.ToolStripMenuItem()
        Me.MGSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.MGExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.Gallery = New KMovies.ImageGalleryVB()
        Me.TBMovies = New KMovies.NSTextBox()
        Me.TAz = New KMovies.NSTextBox()
        Me.CMode = New KMovies.FlatComboBox()
        Me.CGenre = New KMovies.FlatComboBox()
        Me.MPMenu = New KMovies.FlatContextMenuStrip()
        Me.MPlay = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.MEdit = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator0 = New System.Windows.Forms.ToolStripSeparator()
        Me.MAdd = New System.Windows.Forms.ToolStripMenuItem()
        Me.MAddTo = New System.Windows.Forms.ToolStripMenuItem()
        Me.MWatched = New System.Windows.Forms.ToolStripMenuItem()
        Me.MPending = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.MCopyto = New System.Windows.Forms.ToolStripMenuItem()
        Me.MMoveto = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.MTrailer = New System.Windows.Forms.ToolStripMenuItem()
        Me.MImdb = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.MSettings = New System.Windows.Forms.ToolStripMenuItem()
        Me.MSeparator5 = New System.Windows.Forms.ToolStripSeparator()
        Me.MExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.LZoomIn = New KMovies.FlatText()
        Me.TZoomIn = New KMovies.NSTextBox()
        Me.LZoomOut = New KMovies.FlatText()
        Me.TZoomOut = New KMovies.NSTextBox()
        Me.LTransfer = New System.Windows.Forms.Label()
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MGMenu.SuspendLayout()
        Me.MPMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'PCover
        '
        Me.PCover.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PCover.BackColor = System.Drawing.Color.Transparent
        Me.PCover.Cursor = System.Windows.Forms.Cursors.Default
        Me.PCover.Location = New System.Drawing.Point(862, 96)
        Me.PCover.Name = "PCover"
        Me.PCover.Size = New System.Drawing.Size(439, 630)
        Me.PCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PCover.TabIndex = 34
        Me.PCover.TabStop = False
        '
        'LPlot
        '
        Me.LPlot.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LPlot.BackColor = System.Drawing.Color.Transparent
        Me.LPlot.Font = New System.Drawing.Font("Corbel", 14.25!)
        Me.LPlot.ForeColor = System.Drawing.Color.DarkGray
        Me.LPlot.Location = New System.Drawing.Point(744, 626)
        Me.LPlot.Name = "LPlot"
        Me.LPlot.Size = New System.Drawing.Size(562, 125)
        Me.LPlot.TabIndex = 41
        Me.LPlot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'THidden
        '
        Me.THidden.Location = New System.Drawing.Point(-1000, -1000)
        Me.THidden.Name = "THidden"
        Me.THidden.ReadOnly = True
        Me.THidden.ShortcutsEnabled = False
        Me.THidden.Size = New System.Drawing.Size(18, 20)
        Me.THidden.TabIndex = 0
        '
        'LYear
        '
        Me.LYear.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LYear.BackColor = System.Drawing.Color.Transparent
        Me.LYear.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LYear.ForeColor = System.Drawing.Color.DarkGray
        Me.LYear.Location = New System.Drawing.Point(121, 604)
        Me.LYear.Name = "LYear"
        Me.LYear.Size = New System.Drawing.Size(60, 27)
        Me.LYear.TabIndex = 46
        Me.LYear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIYear
        '
        Me.LIYear.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIYear.BackColor = System.Drawing.Color.Transparent
        Me.LIYear.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIYear.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIYear.Location = New System.Drawing.Point(19, 604)
        Me.LIYear.Name = "LIYear"
        Me.LIYear.Size = New System.Drawing.Size(96, 27)
        Me.LIYear.TabIndex = 46
        Me.LIYear.Text = "Year:"
        Me.LIYear.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LRating
        '
        Me.LRating.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LRating.BackColor = System.Drawing.Color.Transparent
        Me.LRating.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LRating.ForeColor = System.Drawing.Color.DarkGray
        Me.LRating.Location = New System.Drawing.Point(121, 631)
        Me.LRating.Name = "LRating"
        Me.LRating.Size = New System.Drawing.Size(60, 27)
        Me.LRating.TabIndex = 46
        Me.LRating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIRating
        '
        Me.LIRating.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIRating.BackColor = System.Drawing.Color.Transparent
        Me.LIRating.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIRating.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIRating.Location = New System.Drawing.Point(41, 631)
        Me.LIRating.Name = "LIRating"
        Me.LIRating.Size = New System.Drawing.Size(74, 27)
        Me.LIRating.TabIndex = 46
        Me.LIRating.Text = "Rating:"
        Me.LIRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LDuration
        '
        Me.LDuration.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LDuration.BackColor = System.Drawing.Color.Transparent
        Me.LDuration.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LDuration.ForeColor = System.Drawing.Color.DarkGray
        Me.LDuration.Location = New System.Drawing.Point(121, 658)
        Me.LDuration.Name = "LDuration"
        Me.LDuration.Size = New System.Drawing.Size(80, 27)
        Me.LDuration.TabIndex = 46
        Me.LDuration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIDuration
        '
        Me.LIDuration.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIDuration.BackColor = System.Drawing.Color.Transparent
        Me.LIDuration.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIDuration.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIDuration.Location = New System.Drawing.Point(19, 658)
        Me.LIDuration.Name = "LIDuration"
        Me.LIDuration.Size = New System.Drawing.Size(96, 27)
        Me.LIDuration.TabIndex = 46
        Me.LIDuration.Text = "Duration:"
        Me.LIDuration.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LGenre
        '
        Me.LGenre.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LGenre.BackColor = System.Drawing.Color.Transparent
        Me.LGenre.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LGenre.ForeColor = System.Drawing.Color.DarkGray
        Me.LGenre.Location = New System.Drawing.Point(121, 685)
        Me.LGenre.Name = "LGenre"
        Me.LGenre.Size = New System.Drawing.Size(185, 27)
        Me.LGenre.TabIndex = 46
        Me.LGenre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIGenre
        '
        Me.LIGenre.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIGenre.BackColor = System.Drawing.Color.Transparent
        Me.LIGenre.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIGenre.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIGenre.Location = New System.Drawing.Point(26, 685)
        Me.LIGenre.Name = "LIGenre"
        Me.LIGenre.Size = New System.Drawing.Size(89, 27)
        Me.LIGenre.TabIndex = 46
        Me.LIGenre.Text = "Genre:"
        Me.LIGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LCountry
        '
        Me.LCountry.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LCountry.BackColor = System.Drawing.Color.Transparent
        Me.LCountry.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LCountry.ForeColor = System.Drawing.Color.DarkGray
        Me.LCountry.Location = New System.Drawing.Point(410, 631)
        Me.LCountry.Name = "LCountry"
        Me.LCountry.Size = New System.Drawing.Size(328, 27)
        Me.LCountry.TabIndex = 46
        Me.LCountry.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LICountry
        '
        Me.LICountry.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LICountry.BackColor = System.Drawing.Color.Transparent
        Me.LICountry.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LICountry.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LICountry.Location = New System.Drawing.Point(308, 631)
        Me.LICountry.Name = "LICountry"
        Me.LICountry.Size = New System.Drawing.Size(96, 27)
        Me.LICountry.TabIndex = 46
        Me.LICountry.Text = "Country:"
        Me.LICountry.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LDirector
        '
        Me.LDirector.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LDirector.BackColor = System.Drawing.Color.Transparent
        Me.LDirector.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LDirector.ForeColor = System.Drawing.Color.DarkGray
        Me.LDirector.Location = New System.Drawing.Point(410, 658)
        Me.LDirector.Name = "LDirector"
        Me.LDirector.Size = New System.Drawing.Size(328, 27)
        Me.LDirector.TabIndex = 46
        Me.LDirector.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIDirector
        '
        Me.LIDirector.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIDirector.BackColor = System.Drawing.Color.Transparent
        Me.LIDirector.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIDirector.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIDirector.Location = New System.Drawing.Point(315, 658)
        Me.LIDirector.Name = "LIDirector"
        Me.LIDirector.Size = New System.Drawing.Size(89, 27)
        Me.LIDirector.TabIndex = 46
        Me.LIDirector.Text = "Director:"
        Me.LIDirector.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LActor
        '
        Me.LActor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LActor.BackColor = System.Drawing.Color.Transparent
        Me.LActor.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LActor.ForeColor = System.Drawing.Color.DarkGray
        Me.LActor.Location = New System.Drawing.Point(410, 689)
        Me.LActor.Name = "LActor"
        Me.LActor.Size = New System.Drawing.Size(328, 62)
        Me.LActor.TabIndex = 46
        '
        'LIActor
        '
        Me.LIActor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIActor.BackColor = System.Drawing.Color.Transparent
        Me.LIActor.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIActor.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIActor.Location = New System.Drawing.Point(315, 685)
        Me.LIActor.Name = "LIActor"
        Me.LIActor.Size = New System.Drawing.Size(89, 27)
        Me.LIActor.TabIndex = 46
        Me.LIActor.Text = "Actor:"
        Me.LIActor.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFYear
        '
        Me.LFYear.BackColor = System.Drawing.Color.Transparent
        Me.LFYear.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFYear.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFYear.Location = New System.Drawing.Point(-11, 5)
        Me.LFYear.Name = "LFYear"
        Me.LFYear.Size = New System.Drawing.Size(79, 27)
        Me.LFYear.TabIndex = 46
        Me.LFYear.Text = "Year:"
        Me.LFYear.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFLanguage
        '
        Me.LFLanguage.BackColor = System.Drawing.Color.Transparent
        Me.LFLanguage.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFLanguage.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFLanguage.Location = New System.Drawing.Point(308, 5)
        Me.LFLanguage.Name = "LFLanguage"
        Me.LFLanguage.Size = New System.Drawing.Size(96, 27)
        Me.LFLanguage.TabIndex = 46
        Me.LFLanguage.Text = "Language:"
        Me.LFLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFCountry
        '
        Me.LFCountry.BackColor = System.Drawing.Color.Transparent
        Me.LFCountry.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFCountry.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFCountry.Location = New System.Drawing.Point(308, 39)
        Me.LFCountry.Name = "LFCountry"
        Me.LFCountry.Size = New System.Drawing.Size(96, 27)
        Me.LFCountry.TabIndex = 46
        Me.LFCountry.Text = "Country:"
        Me.LFCountry.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFDirector
        '
        Me.LFDirector.BackColor = System.Drawing.Color.Transparent
        Me.LFDirector.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFDirector.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFDirector.Location = New System.Drawing.Point(538, 5)
        Me.LFDirector.Name = "LFDirector"
        Me.LFDirector.Size = New System.Drawing.Size(79, 27)
        Me.LFDirector.TabIndex = 46
        Me.LFDirector.Text = "Director:"
        Me.LFDirector.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFActor
        '
        Me.LFActor.BackColor = System.Drawing.Color.Transparent
        Me.LFActor.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFActor.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFActor.Location = New System.Drawing.Point(542, 41)
        Me.LFActor.Name = "LFActor"
        Me.LFActor.Size = New System.Drawing.Size(75, 27)
        Me.LFActor.TabIndex = 46
        Me.LFActor.Text = "Actor:"
        Me.LFActor.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFGenre
        '
        Me.LFGenre.BackColor = System.Drawing.Color.Transparent
        Me.LFGenre.Font = New System.Drawing.Font("Corbel", 13.0!)
        Me.LFGenre.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFGenre.Location = New System.Drawing.Point(130, 5)
        Me.LFGenre.Name = "LFGenre"
        Me.LFGenre.Size = New System.Drawing.Size(70, 27)
        Me.LFGenre.TabIndex = 46
        Me.LFGenre.Text = "Genre:"
        Me.LFGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LTitle
        '
        Me.LTitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LTitle.BackColor = System.Drawing.Color.Transparent
        Me.LTitle.Font = New System.Drawing.Font("Bell MT", 36.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LTitle.ForeColor = System.Drawing.Color.DarkGray
        Me.LTitle.Location = New System.Drawing.Point(1010, 16)
        Me.LTitle.Name = "LTitle"
        Me.LTitle.Size = New System.Drawing.Size(296, 59)
        Me.LTitle.TabIndex = 54
        Me.LTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LFRating
        '
        Me.LFRating.BackColor = System.Drawing.Color.Transparent
        Me.LFRating.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.LFRating.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFRating.Location = New System.Drawing.Point(18, 37)
        Me.LFRating.Name = "LFRating"
        Me.LFRating.Size = New System.Drawing.Size(53, 16)
        Me.LFRating.TabIndex = 46
        Me.LFRating.Text = "Rating"
        Me.LFRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFDurationThan
        '
        Me.LFDurationThan.BackColor = System.Drawing.Color.Transparent
        Me.LFDurationThan.Font = New System.Drawing.Font("Corbel", 8.0!)
        Me.LFDurationThan.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFDurationThan.Location = New System.Drawing.Point(131, 51)
        Me.LFDurationThan.Name = "LFDurationThan"
        Me.LFDurationThan.Size = New System.Drawing.Size(76, 16)
        Me.LFDurationThan.TabIndex = 46
        Me.LFDurationThan.Text = "Longer than"
        Me.LFDurationThan.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LLanguage
        '
        Me.LLanguage.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LLanguage.BackColor = System.Drawing.Color.Transparent
        Me.LLanguage.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LLanguage.ForeColor = System.Drawing.Color.DarkGray
        Me.LLanguage.Location = New System.Drawing.Point(410, 604)
        Me.LLanguage.Name = "LLanguage"
        Me.LLanguage.Size = New System.Drawing.Size(328, 27)
        Me.LLanguage.TabIndex = 46
        Me.LLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LILanguage
        '
        Me.LILanguage.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LILanguage.BackColor = System.Drawing.Color.Transparent
        Me.LILanguage.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LILanguage.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LILanguage.Location = New System.Drawing.Point(294, 604)
        Me.LILanguage.Name = "LILanguage"
        Me.LILanguage.Size = New System.Drawing.Size(110, 27)
        Me.LILanguage.TabIndex = 46
        Me.LILanguage.Text = "Language:"
        Me.LILanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFRatingThan
        '
        Me.LFRatingThan.BackColor = System.Drawing.Color.Transparent
        Me.LFRatingThan.Font = New System.Drawing.Font("Corbel", 8.0!)
        Me.LFRatingThan.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFRatingThan.Location = New System.Drawing.Point(-7, 51)
        Me.LFRatingThan.Name = "LFRatingThan"
        Me.LFRatingThan.Size = New System.Drawing.Size(88, 16)
        Me.LFRatingThan.TabIndex = 46
        Me.LFRatingThan.Text = "Greater than"
        Me.LFRatingThan.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LFDuration
        '
        Me.LFDuration.BackColor = System.Drawing.Color.Transparent
        Me.LFDuration.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.LFDuration.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LFDuration.Location = New System.Drawing.Point(138, 37)
        Me.LFDuration.Name = "LFDuration"
        Me.LFDuration.Size = New System.Drawing.Size(65, 16)
        Me.LFDuration.TabIndex = 46
        Me.LFDuration.Text = "Duration"
        Me.LFDuration.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LAbout
        '
        Me.LAbout.BackColor = System.Drawing.Color.Transparent
        Me.LAbout.Font = New System.Drawing.Font("Bell MT", 22.0!)
        Me.LAbout.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LAbout.Location = New System.Drawing.Point(796, 3)
        Me.LAbout.Name = "LAbout"
        Me.LAbout.Size = New System.Drawing.Size(208, 30)
        Me.LAbout.TabIndex = 54
        Me.LAbout.Text = "K-Movies 4.0"
        Me.LAbout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LAdded
        '
        Me.LAdded.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LAdded.BackColor = System.Drawing.Color.Transparent
        Me.LAdded.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LAdded.ForeColor = System.Drawing.Color.DarkGray
        Me.LAdded.Location = New System.Drawing.Point(121, 716)
        Me.LAdded.Name = "LAdded"
        Me.LAdded.Size = New System.Drawing.Size(106, 27)
        Me.LAdded.TabIndex = 46
        Me.LAdded.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LIAdded
        '
        Me.LIAdded.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LIAdded.BackColor = System.Drawing.Color.Transparent
        Me.LIAdded.Font = New System.Drawing.Font("Corbel", 15.0!)
        Me.LIAdded.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIAdded.Location = New System.Drawing.Point(19, 712)
        Me.LIAdded.Name = "LIAdded"
        Me.LIAdded.Size = New System.Drawing.Size(96, 27)
        Me.LIAdded.TabIndex = 46
        Me.LIAdded.Text = "Added:"
        Me.LIAdded.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LCountInfo
        '
        Me.LCountInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LCountInfo.BackColor = System.Drawing.Color.Transparent
        Me.LCountInfo.Font = New System.Drawing.Font("Corbel", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LCountInfo.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LCountInfo.Location = New System.Drawing.Point(17, 577)
        Me.LCountInfo.Name = "LCountInfo"
        Me.LCountInfo.Size = New System.Drawing.Size(217, 17)
        Me.LCountInfo.TabIndex = 46
        Me.LCountInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LCountImdb
        '
        Me.LCountImdb.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.LCountImdb.BackColor = System.Drawing.Color.Transparent
        Me.LCountImdb.Font = New System.Drawing.Font("Corbel", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LCountImdb.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LCountImdb.Location = New System.Drawing.Point(623, 577)
        Me.LCountImdb.Name = "LCountImdb"
        Me.LCountImdb.Size = New System.Drawing.Size(217, 17)
        Me.LCountImdb.TabIndex = 46
        Me.LCountImdb.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.LCountImdb.Visible = False
        '
        'LRandom
        '
        Me.LRandom.AutoSize = True
        Me.LRandom.BackColor = System.Drawing.Color.Transparent
        Me.LRandom.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LRandom.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LRandom.Location = New System.Drawing.Point(793, 6)
        Me.LRandom.Name = "LRandom"
        Me.LRandom.Size = New System.Drawing.Size(23, 24)
        Me.LRandom.TabIndex = 59
        Me.LRandom.Text = "₪"
        '
        'LCYear
        '
        Me.LCYear.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCYear.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCYear.ForeColor = System.Drawing.Color.DarkGray
        Me.LCYear.Location = New System.Drawing.Point(74, 10)
        Me.LCYear.Name = "LCYear"
        Me.LCYear.Size = New System.Drawing.Size(36, 19)
        Me.LCYear.TabIndex = 67
        Me.LCYear.Text = "All"
        Me.LCYear.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCGenre
        '
        Me.LCGenre.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCGenre.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCGenre.ForeColor = System.Drawing.Color.DarkGray
        Me.LCGenre.Location = New System.Drawing.Point(209, 10)
        Me.LCGenre.Name = "LCGenre"
        Me.LCGenre.Size = New System.Drawing.Size(73, 19)
        Me.LCGenre.TabIndex = 67
        Me.LCGenre.Text = "All"
        Me.LCGenre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCCountry
        '
        Me.LCCountry.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCCountry.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCCountry.ForeColor = System.Drawing.Color.DarkGray
        Me.LCCountry.Location = New System.Drawing.Point(413, 45)
        Me.LCCountry.Name = "LCCountry"
        Me.LCCountry.Size = New System.Drawing.Size(93, 19)
        Me.LCCountry.TabIndex = 67
        Me.LCCountry.Text = "All"
        Me.LCCountry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCActor
        '
        Me.LCActor.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCActor.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCActor.ForeColor = System.Drawing.Color.DarkGray
        Me.LCActor.Location = New System.Drawing.Point(626, 45)
        Me.LCActor.Name = "LCActor"
        Me.LCActor.Size = New System.Drawing.Size(132, 19)
        Me.LCActor.TabIndex = 67
        Me.LCActor.Text = "All"
        Me.LCActor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCLanguage
        '
        Me.LCLanguage.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCLanguage.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCLanguage.ForeColor = System.Drawing.Color.DarkGray
        Me.LCLanguage.Location = New System.Drawing.Point(413, 10)
        Me.LCLanguage.Name = "LCLanguage"
        Me.LCLanguage.Size = New System.Drawing.Size(93, 19)
        Me.LCLanguage.TabIndex = 67
        Me.LCLanguage.Text = "All"
        Me.LCLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCDirector
        '
        Me.LCDirector.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCDirector.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LCDirector.ForeColor = System.Drawing.Color.DarkGray
        Me.LCDirector.Location = New System.Drawing.Point(626, 10)
        Me.LCDirector.Name = "LCDirector"
        Me.LCDirector.Size = New System.Drawing.Size(132, 19)
        Me.LCDirector.TabIndex = 67
        Me.LCDirector.Text = "All"
        Me.LCDirector.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LBMovies
        '
        Me.LBMovies.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LBMovies.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.LBMovies.ForeColor = System.Drawing.Color.DarkGray
        Me.LBMovies.Location = New System.Drawing.Point(19, 73)
        Me.LBMovies.Name = "LBMovies"
        Me.LBMovies.Size = New System.Drawing.Size(63, 17)
        Me.LBMovies.TabIndex = 67
        Me.LBMovies.Text = "Movies"
        Me.LBMovies.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LCSort
        '
        Me.LCSort.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LCSort.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LCSort.ForeColor = System.Drawing.Color.DarkGray
        Me.LCSort.Location = New System.Drawing.Point(413, 73)
        Me.LCSort.Name = "LCSort"
        Me.LCSort.Size = New System.Drawing.Size(72, 16)
        Me.LCSort.TabIndex = 75
        Me.LCSort.Text = "Name"
        Me.LCSort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'LSort
        '
        Me.LSort.BackColor = System.Drawing.Color.Transparent
        Me.LSort.Font = New System.Drawing.Font("Corbel", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LSort.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LSort.Location = New System.Drawing.Point(343, 67)
        Me.LSort.Name = "LSort"
        Me.LSort.Size = New System.Drawing.Size(58, 27)
        Me.LSort.TabIndex = 72
        Me.LSort.Text = "Sort by:"
        Me.LSort.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TCGenre
        '
        Me.TCGenre.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCGenre.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCGenre.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCGenre.Enabled = False
        Me.TCGenre.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCGenre.ForeColor = System.Drawing.Color.DarkGray
        Me.TCGenre.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCGenre.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCGenre.HideSelection = False
        Me.TCGenre.Location = New System.Drawing.Point(207, 8)
        Me.TCGenre.MaxLength = 32767
        Me.TCGenre.Multiline = False
        Me.TCGenre.Name = "TCGenre"
        Me.TCGenre.ReadOnly = False
        Me.TCGenre.Rounded = 0
        Me.TCGenre.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCGenre.SelectedText = ""
        Me.TCGenre.SelectionLength = 0
        Me.TCGenre.SelectionStart = 0
        Me.TCGenre.ShortcutsEnabled = False
        Me.TCGenre.Size = New System.Drawing.Size(77, 23)
        Me.TCGenre.TabIndex = 66
        Me.TCGenre.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCGenre.UseSystemPasswordChar = False
        Me.TCGenre.WordWrap = False
        '
        'Separator
        '
        Me.Separator.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Separator.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.Separator.ForeColor = System.Drawing.Color.DimGray
        Me.Separator.Location = New System.Drawing.Point(15, 599)
        Me.Separator.MaximumSize = New System.Drawing.Size(6000, 2)
        Me.Separator.MinimumSize = New System.Drawing.Size(0, 2)
        Me.Separator.Name = "Separator"
        Me.Separator.Size = New System.Drawing.Size(1300, 2)
        Me.Separator.TabIndex = 57
        '
        'LAz
        '
        Me.LAz.BackColor = System.Drawing.Color.White
        Me.LAz.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LAz.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LAz.Location = New System.Drawing.Point(512, 73)
        Me.LAz.Name = "LAz"
        Me.LAz.Size = New System.Drawing.Size(16, 16)
        Me.LAz.TabIndex = 92
        Me.LAz.Text = "Az"
        Me.LAz.TextColor = System.Drawing.Color.DarkGray
        Me.LAz.TextHoverColor = System.Drawing.Color.DarkGreen
        '
        'TSort
        '
        Me.TSort.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TSort.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TSort.Cursor = System.Windows.Forms.Cursors.Default
        Me.TSort.Enabled = False
        Me.TSort.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TSort.ForeColor = System.Drawing.Color.DarkGray
        Me.TSort.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TSort.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TSort.HideSelection = False
        Me.TSort.Location = New System.Drawing.Point(411, 71)
        Me.TSort.MaxLength = 32767
        Me.TSort.Multiline = False
        Me.TSort.Name = "TSort"
        Me.TSort.ReadOnly = False
        Me.TSort.Rounded = 0
        Me.TSort.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TSort.SelectedText = ""
        Me.TSort.SelectionLength = 0
        Me.TSort.SelectionStart = 0
        Me.TSort.ShortcutsEnabled = False
        Me.TSort.Size = New System.Drawing.Size(76, 20)
        Me.TSort.TabIndex = 74
        Me.TSort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TSort.UseSystemPasswordChar = False
        Me.TSort.WordWrap = False
        '
        'CSort
        '
        Me.CSort.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CSort.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CSort.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CSort.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CSort.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CSort.BorderColor3 = System.Drawing.Color.Empty
        Me.CSort.DoubleText = False
        Me.CSort.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CSort.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CSort.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CSort.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CSort.ForeColor2 = System.Drawing.Color.Black
        Me.CSort.FormattingEnabled = True
        Me.CSort.GradientAngle = 90
        Me.CSort.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CSort.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CSort.GradientTransparency = 0
        Me.CSort.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CSort.HoverColor2 = System.Drawing.Color.Black
        Me.CSort.Image = Nothing
        Me.CSort.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CSort.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CSort.ImageSize = New System.Drawing.Size(19, 17)
        Me.CSort.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CSort.ItemColor = System.Drawing.Color.DarkGray
        Me.CSort.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CSort.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CSort.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CSort.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CSort.Items.AddRange(New Object() {"Name", "Year", "Added", "Rating", "Duration"})
        Me.CSort.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CSort.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CSort.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CSort.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CSort.Location = New System.Drawing.Point(410, 70)
        Me.CSort.Name = "CSort"
        Me.CSort.Rounded = 1
        Me.CSort.Size = New System.Drawing.Size(99, 22)
        Me.CSort.TabIndex = 73
        Me.CSort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TCDirector
        '
        Me.TCDirector.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCDirector.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCDirector.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCDirector.Enabled = False
        Me.TCDirector.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCDirector.ForeColor = System.Drawing.Color.DarkGray
        Me.TCDirector.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCDirector.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCDirector.HideSelection = False
        Me.TCDirector.Location = New System.Drawing.Point(624, 8)
        Me.TCDirector.MaxLength = 32767
        Me.TCDirector.Multiline = False
        Me.TCDirector.Name = "TCDirector"
        Me.TCDirector.ReadOnly = False
        Me.TCDirector.Rounded = 0
        Me.TCDirector.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCDirector.SelectedText = ""
        Me.TCDirector.SelectionLength = 0
        Me.TCDirector.SelectionStart = 0
        Me.TCDirector.ShortcutsEnabled = False
        Me.TCDirector.Size = New System.Drawing.Size(136, 23)
        Me.TCDirector.TabIndex = 66
        Me.TCDirector.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCDirector.UseSystemPasswordChar = False
        Me.TCDirector.WordWrap = False
        '
        'TCLanguage
        '
        Me.TCLanguage.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCLanguage.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCLanguage.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCLanguage.Enabled = False
        Me.TCLanguage.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCLanguage.ForeColor = System.Drawing.Color.DarkGray
        Me.TCLanguage.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCLanguage.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCLanguage.HideSelection = False
        Me.TCLanguage.Location = New System.Drawing.Point(411, 8)
        Me.TCLanguage.MaxLength = 32767
        Me.TCLanguage.Multiline = False
        Me.TCLanguage.Name = "TCLanguage"
        Me.TCLanguage.ReadOnly = False
        Me.TCLanguage.Rounded = 0
        Me.TCLanguage.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCLanguage.SelectedText = ""
        Me.TCLanguage.SelectionLength = 0
        Me.TCLanguage.SelectionStart = 0
        Me.TCLanguage.ShortcutsEnabled = False
        Me.TCLanguage.Size = New System.Drawing.Size(97, 23)
        Me.TCLanguage.TabIndex = 66
        Me.TCLanguage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCLanguage.UseSystemPasswordChar = False
        Me.TCLanguage.WordWrap = False
        '
        'TCActor
        '
        Me.TCActor.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCActor.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCActor.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCActor.Enabled = False
        Me.TCActor.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCActor.ForeColor = System.Drawing.Color.DarkGray
        Me.TCActor.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCActor.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCActor.HideSelection = False
        Me.TCActor.Location = New System.Drawing.Point(624, 43)
        Me.TCActor.MaxLength = 32767
        Me.TCActor.Multiline = False
        Me.TCActor.Name = "TCActor"
        Me.TCActor.ReadOnly = False
        Me.TCActor.Rounded = 0
        Me.TCActor.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCActor.SelectedText = ""
        Me.TCActor.SelectionLength = 0
        Me.TCActor.SelectionStart = 0
        Me.TCActor.ShortcutsEnabled = False
        Me.TCActor.Size = New System.Drawing.Size(136, 23)
        Me.TCActor.TabIndex = 66
        Me.TCActor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCActor.UseSystemPasswordChar = False
        Me.TCActor.WordWrap = False
        '
        'TCCountry
        '
        Me.TCCountry.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCCountry.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCCountry.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCCountry.Enabled = False
        Me.TCCountry.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCCountry.ForeColor = System.Drawing.Color.DarkGray
        Me.TCCountry.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCCountry.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCCountry.HideSelection = False
        Me.TCCountry.Location = New System.Drawing.Point(411, 43)
        Me.TCCountry.MaxLength = 32767
        Me.TCCountry.Multiline = False
        Me.TCCountry.Name = "TCCountry"
        Me.TCCountry.ReadOnly = False
        Me.TCCountry.Rounded = 0
        Me.TCCountry.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCCountry.SelectedText = ""
        Me.TCCountry.SelectionLength = 0
        Me.TCCountry.SelectionStart = 0
        Me.TCCountry.ShortcutsEnabled = False
        Me.TCCountry.Size = New System.Drawing.Size(97, 23)
        Me.TCCountry.TabIndex = 66
        Me.TCCountry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCCountry.UseSystemPasswordChar = False
        Me.TCCountry.WordWrap = False
        '
        'TCYear
        '
        Me.TCYear.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCYear.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCYear.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCYear.Enabled = False
        Me.TCYear.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TCYear.ForeColor = System.Drawing.Color.DarkGray
        Me.TCYear.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCYear.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCYear.HideSelection = False
        Me.TCYear.Location = New System.Drawing.Point(72, 8)
        Me.TCYear.MaxLength = 32767
        Me.TCYear.Multiline = False
        Me.TCYear.Name = "TCYear"
        Me.TCYear.ReadOnly = False
        Me.TCYear.Rounded = 0
        Me.TCYear.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCYear.SelectedText = ""
        Me.TCYear.SelectionLength = 0
        Me.TCYear.SelectionStart = 0
        Me.TCYear.ShortcutsEnabled = False
        Me.TCYear.Size = New System.Drawing.Size(40, 23)
        Me.TCYear.TabIndex = 66
        Me.TCYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCYear.UseSystemPasswordChar = False
        Me.TCYear.WordWrap = False
        '
        'CActor
        '
        Me.CActor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CActor.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CActor.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CActor.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CActor.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CActor.BorderColor3 = System.Drawing.Color.Empty
        Me.CActor.DoubleText = False
        Me.CActor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CActor.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CActor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CActor.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CActor.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CActor.ForeColor2 = System.Drawing.Color.Black
        Me.CActor.FormattingEnabled = True
        Me.CActor.GradientAngle = 90
        Me.CActor.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CActor.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CActor.GradientTransparency = 0
        Me.CActor.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CActor.HoverColor2 = System.Drawing.Color.Black
        Me.CActor.Image = Nothing
        Me.CActor.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CActor.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CActor.ImageSize = New System.Drawing.Size(19, 17)
        Me.CActor.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CActor.ItemColor = System.Drawing.Color.DarkGray
        Me.CActor.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CActor.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CActor.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CActor.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CActor.Items.AddRange(New Object() {"All"})
        Me.CActor.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CActor.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CActor.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CActor.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CActor.Location = New System.Drawing.Point(623, 42)
        Me.CActor.Name = "CActor"
        Me.CActor.Rounded = 1
        Me.CActor.Size = New System.Drawing.Size(159, 25)
        Me.CActor.TabIndex = 65
        Me.CActor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'CDirector
        '
        Me.CDirector.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CDirector.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CDirector.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CDirector.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CDirector.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CDirector.BorderColor3 = System.Drawing.Color.Empty
        Me.CDirector.DoubleText = False
        Me.CDirector.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CDirector.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CDirector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CDirector.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CDirector.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CDirector.ForeColor2 = System.Drawing.Color.Black
        Me.CDirector.FormattingEnabled = True
        Me.CDirector.GradientAngle = 90
        Me.CDirector.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CDirector.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CDirector.GradientTransparency = 0
        Me.CDirector.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CDirector.HoverColor2 = System.Drawing.Color.Black
        Me.CDirector.Image = Nothing
        Me.CDirector.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CDirector.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CDirector.ImageSize = New System.Drawing.Size(19, 17)
        Me.CDirector.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CDirector.ItemColor = System.Drawing.Color.DarkGray
        Me.CDirector.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CDirector.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CDirector.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CDirector.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CDirector.Items.AddRange(New Object() {"All"})
        Me.CDirector.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CDirector.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CDirector.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CDirector.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CDirector.Location = New System.Drawing.Point(623, 7)
        Me.CDirector.Name = "CDirector"
        Me.CDirector.Rounded = 1
        Me.CDirector.Size = New System.Drawing.Size(159, 25)
        Me.CDirector.TabIndex = 65
        Me.CDirector.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'CCountry
        '
        Me.CCountry.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CCountry.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CCountry.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CCountry.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CCountry.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CCountry.BorderColor3 = System.Drawing.Color.Empty
        Me.CCountry.DoubleText = False
        Me.CCountry.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CCountry.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CCountry.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CCountry.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CCountry.ForeColor2 = System.Drawing.Color.Black
        Me.CCountry.FormattingEnabled = True
        Me.CCountry.GradientAngle = 90
        Me.CCountry.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CCountry.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CCountry.GradientTransparency = 0
        Me.CCountry.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CCountry.HoverColor2 = System.Drawing.Color.Black
        Me.CCountry.Image = Nothing
        Me.CCountry.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CCountry.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CCountry.ImageSize = New System.Drawing.Size(19, 17)
        Me.CCountry.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CCountry.ItemColor = System.Drawing.Color.DarkGray
        Me.CCountry.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CCountry.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CCountry.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CCountry.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CCountry.Items.AddRange(New Object() {"All"})
        Me.CCountry.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CCountry.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CCountry.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CCountry.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CCountry.Location = New System.Drawing.Point(410, 42)
        Me.CCountry.Name = "CCountry"
        Me.CCountry.Rounded = 1
        Me.CCountry.Size = New System.Drawing.Size(120, 25)
        Me.CCountry.TabIndex = 65
        Me.CCountry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'CLanguage
        '
        Me.CLanguage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CLanguage.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CLanguage.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CLanguage.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CLanguage.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CLanguage.BorderColor3 = System.Drawing.Color.Empty
        Me.CLanguage.DoubleText = False
        Me.CLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CLanguage.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CLanguage.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CLanguage.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CLanguage.ForeColor2 = System.Drawing.Color.Black
        Me.CLanguage.FormattingEnabled = True
        Me.CLanguage.GradientAngle = 90
        Me.CLanguage.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CLanguage.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CLanguage.GradientTransparency = 0
        Me.CLanguage.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CLanguage.HoverColor2 = System.Drawing.Color.Black
        Me.CLanguage.Image = Nothing
        Me.CLanguage.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CLanguage.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CLanguage.ImageSize = New System.Drawing.Size(19, 17)
        Me.CLanguage.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CLanguage.ItemColor = System.Drawing.Color.DarkGray
        Me.CLanguage.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CLanguage.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CLanguage.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CLanguage.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CLanguage.Items.AddRange(New Object() {"All"})
        Me.CLanguage.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CLanguage.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CLanguage.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CLanguage.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CLanguage.Location = New System.Drawing.Point(410, 7)
        Me.CLanguage.Name = "CLanguage"
        Me.CLanguage.Rounded = 1
        Me.CLanguage.Size = New System.Drawing.Size(120, 25)
        Me.CLanguage.TabIndex = 65
        Me.CLanguage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'TSearch
        '
        Me.TSearch.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TSearch.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TSearch.Cursor = System.Windows.Forms.Cursors.Default
        Me.TSearch.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TSearch.ForeColor = System.Drawing.Color.DarkGray
        Me.TSearch.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TSearch.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TSearch.HideSelection = False
        Me.TSearch.Location = New System.Drawing.Point(796, 43)
        Me.TSearch.MaxLength = 32767
        Me.TSearch.Multiline = False
        Me.TSearch.Name = "TSearch"
        Me.TSearch.ReadOnly = False
        Me.TSearch.Rounded = 0
        Me.TSearch.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TSearch.SelectedText = ""
        Me.TSearch.SelectionLength = 0
        Me.TSearch.SelectionStart = 0
        Me.TSearch.ShortcutsEnabled = False
        Me.TSearch.Size = New System.Drawing.Size(208, 23)
        Me.TSearch.TabIndex = 64
        Me.TSearch.Text = "Search for a movie"
        Me.TSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TSearch.UseSystemPasswordChar = False
        Me.TSearch.WordWrap = False
        '
        'TDuration
        '
        Me.TDuration.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TDuration.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TDuration.Cursor = System.Windows.Forms.Cursors.Default
        Me.TDuration.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TDuration.ForeColor = System.Drawing.Color.DarkGray
        Me.TDuration.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TDuration.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TDuration.HideSelection = False
        Me.TDuration.Location = New System.Drawing.Point(213, 43)
        Me.TDuration.MaxLength = 3
        Me.TDuration.Multiline = False
        Me.TDuration.Name = "TDuration"
        Me.TDuration.ReadOnly = False
        Me.TDuration.Rounded = 0
        Me.TDuration.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TDuration.SelectedText = ""
        Me.TDuration.SelectionLength = 0
        Me.TDuration.SelectionStart = 0
        Me.TDuration.ShortcutsEnabled = False
        Me.TDuration.Size = New System.Drawing.Size(93, 23)
        Me.TDuration.TabIndex = 64
        Me.TDuration.Text = "0 min"
        Me.TDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TDuration.UseSystemPasswordChar = False
        Me.TDuration.WordWrap = False
        '
        'TRating
        '
        Me.TRating.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TRating.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TRating.Cursor = System.Windows.Forms.Cursors.Default
        Me.TRating.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TRating.ForeColor = System.Drawing.Color.DarkGray
        Me.TRating.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TRating.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TRating.HideSelection = False
        Me.TRating.Location = New System.Drawing.Point(87, 43)
        Me.TRating.MaxLength = 4
        Me.TRating.Multiline = False
        Me.TRating.Name = "TRating"
        Me.TRating.ReadOnly = False
        Me.TRating.Rounded = 0
        Me.TRating.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TRating.SelectedText = ""
        Me.TRating.SelectionLength = 0
        Me.TRating.SelectionStart = 0
        Me.TRating.ShortcutsEnabled = False
        Me.TRating.Size = New System.Drawing.Size(42, 23)
        Me.TRating.TabIndex = 61
        Me.TRating.Text = "0.0"
        Me.TRating.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TRating.UseSystemPasswordChar = False
        Me.TRating.WordWrap = False
        '
        'CYear
        '
        Me.CYear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CYear.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CYear.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CYear.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CYear.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CYear.BorderColor3 = System.Drawing.Color.Empty
        Me.CYear.DoubleText = False
        Me.CYear.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CYear.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CYear.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CYear.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CYear.ForeColor2 = System.Drawing.Color.Black
        Me.CYear.FormattingEnabled = True
        Me.CYear.GradientAngle = 90
        Me.CYear.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CYear.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CYear.GradientTransparency = 0
        Me.CYear.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CYear.HoverColor2 = System.Drawing.Color.Black
        Me.CYear.Image = Nothing
        Me.CYear.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CYear.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CYear.ImageSize = New System.Drawing.Size(19, 17)
        Me.CYear.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CYear.ItemColor = System.Drawing.Color.DarkGray
        Me.CYear.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CYear.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CYear.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CYear.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CYear.Items.AddRange(New Object() {"All"})
        Me.CYear.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CYear.ItemSelectedColor = System.Drawing.Color.DarkGray
        Me.CYear.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CYear.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CYear.Location = New System.Drawing.Point(71, 7)
        Me.CYear.Name = "CYear"
        Me.CYear.Rounded = 1
        Me.CYear.Size = New System.Drawing.Size(63, 25)
        Me.CYear.TabIndex = 60
        Me.CYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'BClose
        '
        Me.BClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BClose.AutoSize = True
        Me.BClose.BackColor = System.Drawing.Color.Transparent
        Me.BClose.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BClose.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BClose.Location = New System.Drawing.Point(1308, -3)
        Me.BClose.Name = "BClose"
        Me.BClose.Size = New System.Drawing.Size(21, 26)
        Me.BClose.TabIndex = 35
        Me.BClose.Text = "ꭗ"
        '
        'BMinimize
        '
        Me.BMinimize.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BMinimize.AutoSize = True
        Me.BMinimize.BackColor = System.Drawing.Color.Transparent
        Me.BMinimize.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BMinimize.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BMinimize.Location = New System.Drawing.Point(1283, -6)
        Me.BMinimize.Name = "BMinimize"
        Me.BMinimize.Size = New System.Drawing.Size(22, 26)
        Me.BMinimize.TabIndex = 35
        Me.BMinimize.Text = "_"
        '
        'LGallery
        '
        Me.LGallery.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LGallery.BackColor = System.Drawing.Color.Transparent
        Me.LGallery.ContextMenuStrip = Me.MGMenu
        Me.LGallery.Font = New System.Drawing.Font("Calibri Light", 18.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LGallery.ForeColor = System.Drawing.Color.DarkGray
        Me.LGallery.Location = New System.Drawing.Point(13, 96)
        Me.LGallery.Name = "LGallery"
        Me.LGallery.Size = New System.Drawing.Size(827, 480)
        Me.LGallery.TabIndex = 3
        Me.LGallery.Text = "NO MOVIES"
        Me.LGallery.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.LGallery.Visible = False
        '
        'MGMenu
        '
        Me.MGMenu.AutoSize = False
        Me.MGMenu.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.MGMenu.ForeColor = System.Drawing.Color.White
        Me.MGMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MGAdd, Me.MGSeparator1, Me.MGSettings, Me.MGSeparator2, Me.MGExit})
        Me.MGMenu.Name = "MLista"
        Me.MGMenu.ShowImageMargin = False
        Me.MGMenu.Size = New System.Drawing.Size(80, 54)
        '
        'MGAdd
        '
        Me.MGAdd.AutoSize = False
        Me.MGAdd.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MGAdd.Name = "MGAdd"
        Me.MGAdd.Size = New System.Drawing.Size(79, 22)
        Me.MGAdd.Text = "Add"
        '
        'MGSeparator1
        '
        Me.MGSeparator1.Name = "MGSeparator1"
        Me.MGSeparator1.Size = New System.Drawing.Size(76, 6)
        '
        'MGSettings
        '
        Me.MGSettings.AutoSize = False
        Me.MGSettings.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MGSettings.Name = "MGSettings"
        Me.MGSettings.Size = New System.Drawing.Size(79, 22)
        Me.MGSettings.Text = "Settings"
        '
        'MGSeparator2
        '
        Me.MGSeparator2.ForeColor = System.Drawing.Color.White
        Me.MGSeparator2.Name = "MGSeparator2"
        Me.MGSeparator2.Size = New System.Drawing.Size(76, 6)
        '
        'MGExit
        '
        Me.MGExit.AutoSize = False
        Me.MGExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MGExit.Name = "MGExit"
        Me.MGExit.Size = New System.Drawing.Size(79, 22)
        Me.MGExit.Text = "Exit"
        '
        'Gallery
        '
        Me.Gallery.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Gallery.AutoScroll = True
        Me.Gallery.BackColor = System.Drawing.Color.Transparent
        Me.Gallery.ContextMenuStrip = Me.MGMenu
        Me.Gallery.Location = New System.Drawing.Point(13, 96)
        Me.Gallery.Name = "Gallery"
        Me.Gallery.Size = New System.Drawing.Size(716, 480)
        Me.Gallery.TabIndex = 2
        '
        'TBMovies
        '
        Me.TBMovies.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TBMovies.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TBMovies.Cursor = System.Windows.Forms.Cursors.Default
        Me.TBMovies.Enabled = False
        Me.TBMovies.Font = New System.Drawing.Font("Corbel", 9.75!)
        Me.TBMovies.ForeColor = System.Drawing.Color.DarkGray
        Me.TBMovies.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TBMovies.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TBMovies.HideSelection = False
        Me.TBMovies.Location = New System.Drawing.Point(17, 71)
        Me.TBMovies.MaxLength = 32767
        Me.TBMovies.Multiline = False
        Me.TBMovies.Name = "TBMovies"
        Me.TBMovies.ReadOnly = False
        Me.TBMovies.Rounded = 0
        Me.TBMovies.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TBMovies.SelectedText = ""
        Me.TBMovies.SelectionLength = 0
        Me.TBMovies.SelectionStart = 0
        Me.TBMovies.ShortcutsEnabled = False
        Me.TBMovies.Size = New System.Drawing.Size(67, 21)
        Me.TBMovies.TabIndex = 66
        Me.TBMovies.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TBMovies.UseSystemPasswordChar = False
        Me.TBMovies.WordWrap = False
        '
        'TAz
        '
        Me.TAz.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TAz.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TAz.Cursor = System.Windows.Forms.Cursors.Default
        Me.TAz.Enabled = False
        Me.TAz.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TAz.ForeColor = System.Drawing.Color.DarkGray
        Me.TAz.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TAz.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TAz.HideSelection = False
        Me.TAz.Location = New System.Drawing.Point(509, 71)
        Me.TAz.MaxLength = 32767
        Me.TAz.Multiline = False
        Me.TAz.Name = "TAz"
        Me.TAz.ReadOnly = False
        Me.TAz.Rounded = 0
        Me.TAz.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TAz.SelectedText = ""
        Me.TAz.SelectionLength = 0
        Me.TAz.SelectionStart = 0
        Me.TAz.ShortcutsEnabled = False
        Me.TAz.Size = New System.Drawing.Size(21, 20)
        Me.TAz.TabIndex = 76
        Me.TAz.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TAz.UseSystemPasswordChar = False
        Me.TAz.WordWrap = False
        '
        'CMode
        '
        Me.CMode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CMode.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CMode.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CMode.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CMode.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CMode.BorderColor3 = System.Drawing.Color.Empty
        Me.CMode.DoubleText = False
        Me.CMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CMode.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CMode.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CMode.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CMode.ForeColor2 = System.Drawing.Color.Black
        Me.CMode.FormattingEnabled = True
        Me.CMode.GradientAngle = 90
        Me.CMode.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CMode.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CMode.GradientTransparency = 0
        Me.CMode.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CMode.HoverColor2 = System.Drawing.Color.Black
        Me.CMode.Image = Nothing
        Me.CMode.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CMode.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CMode.ImageSize = New System.Drawing.Size(-1, 17)
        Me.CMode.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CMode.ItemColor = System.Drawing.Color.DarkGray
        Me.CMode.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CMode.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CMode.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CMode.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CMode.Items.AddRange(New Object() {"Movies", "Pending", "Watched"})
        Me.CMode.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CMode.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CMode.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CMode.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CMode.Location = New System.Drawing.Point(18, 70)
        Me.CMode.Name = "CMode"
        Me.CMode.Rounded = 1
        Me.CMode.Size = New System.Drawing.Size(66, 22)
        Me.CMode.TabIndex = 73
        Me.CMode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'CGenre
        '
        Me.CGenre.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.CGenre.BaseColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CGenre.BaseColor2 = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.CGenre.BorderColor1 = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.CGenre.BorderColor2 = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CGenre.BorderColor3 = System.Drawing.Color.Empty
        Me.CGenre.DoubleText = False
        Me.CGenre.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.CGenre.DropdownBorderColor = System.Drawing.Color.Gray
        Me.CGenre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CGenre.Font = New System.Drawing.Font("Corbel", 10.0!)
        Me.CGenre.ForeColor1 = System.Drawing.Color.DarkGray
        Me.CGenre.ForeColor2 = System.Drawing.Color.Black
        Me.CGenre.FormattingEnabled = True
        Me.CGenre.GradientAngle = 90
        Me.CGenre.GradientColor1 = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.CGenre.GradientColor2 = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CGenre.GradientTransparency = 0
        Me.CGenre.HoverColor1 = System.Drawing.Color.DarkGray
        Me.CGenre.HoverColor2 = System.Drawing.Color.Black
        Me.CGenre.Image = Nothing
        Me.CGenre.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.CGenre.ImageMode = KMovies.FlatComboBox.STImageMode.Normal
        Me.CGenre.ImageSize = New System.Drawing.Size(19, 17)
        Me.CGenre.ItemBackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.CGenre.ItemColor = System.Drawing.Color.DarkGray
        Me.CGenre.ItemFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CGenre.ItemHoverBackColor = System.Drawing.Color.FromArgb(CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.CGenre.ItemHoverColor = System.Drawing.Color.DarkGray
        Me.CGenre.ItemHoverFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CGenre.Items.AddRange(New Object() {"All"})
        Me.CGenre.ItemSelectedBackColor = System.Drawing.Color.Blue
        Me.CGenre.ItemSelectedColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer), CType(CType(102, Byte), Integer))
        Me.CGenre.ItemSelectedFont = New System.Drawing.Font("Corbel", 10.0!)
        Me.CGenre.ItemTextAlign = System.Windows.Forms.HorizontalAlignment.Left
        Me.CGenre.Location = New System.Drawing.Point(206, 7)
        Me.CGenre.Name = "CGenre"
        Me.CGenre.Rounded = 1
        Me.CGenre.Size = New System.Drawing.Size(100, 25)
        Me.CGenre.TabIndex = 65
        Me.CGenre.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'MPMenu
        '
        Me.MPMenu.AutoSize = False
        Me.MPMenu.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.MPMenu.ForeColor = System.Drawing.Color.White
        Me.MPMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MPlay, Me.MSeparator1, Me.MDelete, Me.MEdit, Me.MSeparator0, Me.MAdd, Me.MAddTo, Me.MSeparator2, Me.MCopyto, Me.MMoveto, Me.MSeparator4, Me.MTrailer, Me.MImdb, Me.MSeparator3, Me.MSettings, Me.MSeparator5, Me.MExit})
        Me.MPMenu.Name = "MLista"
        Me.MPMenu.ShowImageMargin = False
        Me.MPMenu.Size = New System.Drawing.Size(80, 238)
        '
        'MPlay
        '
        Me.MPlay.AutoSize = False
        Me.MPlay.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MPlay.Name = "MPlay"
        Me.MPlay.Size = New System.Drawing.Size(79, 22)
        Me.MPlay.Text = "Play"
        '
        'MSeparator1
        '
        Me.MSeparator1.Name = "MSeparator1"
        Me.MSeparator1.Size = New System.Drawing.Size(76, 6)
        '
        'MDelete
        '
        Me.MDelete.AutoSize = False
        Me.MDelete.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MDelete.Name = "MDelete"
        Me.MDelete.Size = New System.Drawing.Size(79, 22)
        Me.MDelete.Text = "Delete"
        '
        'MEdit
        '
        Me.MEdit.AutoSize = False
        Me.MEdit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MEdit.Name = "MEdit"
        Me.MEdit.Size = New System.Drawing.Size(79, 22)
        Me.MEdit.Text = "Edit Info"
        '
        'MSeparator0
        '
        Me.MSeparator0.Name = "MSeparator0"
        Me.MSeparator0.Size = New System.Drawing.Size(76, 6)
        '
        'MAdd
        '
        Me.MAdd.AutoSize = False
        Me.MAdd.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MAdd.Name = "MAdd"
        Me.MAdd.Size = New System.Drawing.Size(79, 22)
        Me.MAdd.Text = "Add"
        '
        'MAddTo
        '
        Me.MAddTo.AutoSize = False
        Me.MAddTo.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MWatched, Me.MPending})
        Me.MAddTo.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MAddTo.Name = "MAddTo"
        Me.MAddTo.Size = New System.Drawing.Size(79, 22)
        Me.MAddTo.Text = "Add to..."
        '
        'MWatched
        '
        Me.MWatched.AutoSize = False
        Me.MWatched.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MWatched.Name = "MWatched"
        Me.MWatched.Size = New System.Drawing.Size(79, 22)
        Me.MWatched.Text = "Watched"
        '
        'MPending
        '
        Me.MPending.AutoSize = False
        Me.MPending.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MPending.Name = "MPending"
        Me.MPending.Size = New System.Drawing.Size(79, 22)
        Me.MPending.Text = "Pending"
        '
        'MSeparator2
        '
        Me.MSeparator2.Name = "MSeparator2"
        Me.MSeparator2.Size = New System.Drawing.Size(76, 6)
        '
        'MCopyto
        '
        Me.MCopyto.AutoSize = False
        Me.MCopyto.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MCopyto.Name = "MCopyto"
        Me.MCopyto.Size = New System.Drawing.Size(79, 22)
        Me.MCopyto.Text = "Copy to..."
        '
        'MMoveto
        '
        Me.MMoveto.AutoSize = False
        Me.MMoveto.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MMoveto.Name = "MMoveto"
        Me.MMoveto.Size = New System.Drawing.Size(79, 22)
        Me.MMoveto.Text = "Move to..."
        '
        'MSeparator4
        '
        Me.MSeparator4.Name = "MSeparator4"
        Me.MSeparator4.Size = New System.Drawing.Size(76, 6)
        '
        'MTrailer
        '
        Me.MTrailer.AutoSize = False
        Me.MTrailer.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MTrailer.Name = "MTrailer"
        Me.MTrailer.Size = New System.Drawing.Size(80, 22)
        Me.MTrailer.Text = "Trailer"
        '
        'MImdb
        '
        Me.MImdb.AutoSize = False
        Me.MImdb.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MImdb.Name = "MImdb"
        Me.MImdb.Size = New System.Drawing.Size(79, 22)
        Me.MImdb.Text = "Imdb"
        '
        'MSeparator3
        '
        Me.MSeparator3.Name = "MSeparator3"
        Me.MSeparator3.Size = New System.Drawing.Size(76, 6)
        '
        'MSettings
        '
        Me.MSettings.AutoSize = False
        Me.MSettings.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MSettings.Name = "MSettings"
        Me.MSettings.Size = New System.Drawing.Size(79, 22)
        Me.MSettings.Text = "Settings"
        '
        'MSeparator5
        '
        Me.MSeparator5.Name = "MSeparator5"
        Me.MSeparator5.Size = New System.Drawing.Size(76, 6)
        '
        'MExit
        '
        Me.MExit.AutoSize = False
        Me.MExit.ForeColor = System.Drawing.Color.FromArgb(CType(CType(161, Byte), Integer), CType(CType(158, Byte), Integer), CType(CType(156, Byte), Integer))
        Me.MExit.Name = "MExit"
        Me.MExit.Size = New System.Drawing.Size(79, 22)
        Me.MExit.Text = "E&xit"
        '
        'LZoomIn
        '
        Me.LZoomIn.BackColor = System.Drawing.Color.White
        Me.LZoomIn.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LZoomIn.Font = New System.Drawing.Font("Corbel", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LZoomIn.Location = New System.Drawing.Point(555, 73)
        Me.LZoomIn.Name = "LZoomIn"
        Me.LZoomIn.Size = New System.Drawing.Size(16, 16)
        Me.LZoomIn.TabIndex = 99
        Me.LZoomIn.Text = "+"
        Me.LZoomIn.TextColor = System.Drawing.Color.DarkGray
        Me.LZoomIn.TextHoverColor = System.Drawing.Color.DarkGreen
        '
        'TZoomIn
        '
        Me.TZoomIn.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TZoomIn.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TZoomIn.Cursor = System.Windows.Forms.Cursors.Default
        Me.TZoomIn.Enabled = False
        Me.TZoomIn.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TZoomIn.ForeColor = System.Drawing.Color.DarkGray
        Me.TZoomIn.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TZoomIn.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TZoomIn.HideSelection = False
        Me.TZoomIn.Location = New System.Drawing.Point(552, 71)
        Me.TZoomIn.MaxLength = 32767
        Me.TZoomIn.Multiline = False
        Me.TZoomIn.Name = "TZoomIn"
        Me.TZoomIn.ReadOnly = False
        Me.TZoomIn.Rounded = 0
        Me.TZoomIn.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TZoomIn.SelectedText = ""
        Me.TZoomIn.SelectionLength = 0
        Me.TZoomIn.SelectionStart = 0
        Me.TZoomIn.ShortcutsEnabled = False
        Me.TZoomIn.Size = New System.Drawing.Size(21, 20)
        Me.TZoomIn.TabIndex = 97
        Me.TZoomIn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TZoomIn.UseSystemPasswordChar = False
        Me.TZoomIn.WordWrap = False
        '
        'LZoomOut
        '
        Me.LZoomOut.BackColor = System.Drawing.Color.White
        Me.LZoomOut.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.LZoomOut.Font = New System.Drawing.Font("Corbel", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LZoomOut.Location = New System.Drawing.Point(535, 73)
        Me.LZoomOut.Name = "LZoomOut"
        Me.LZoomOut.Size = New System.Drawing.Size(16, 16)
        Me.LZoomOut.TabIndex = 100
        Me.LZoomOut.Text = "-"
        Me.LZoomOut.TextColor = System.Drawing.Color.DarkGray
        Me.LZoomOut.TextHoverColor = System.Drawing.Color.DarkGreen
        '
        'TZoomOut
        '
        Me.TZoomOut.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TZoomOut.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TZoomOut.Cursor = System.Windows.Forms.Cursors.Default
        Me.TZoomOut.Enabled = False
        Me.TZoomOut.Font = New System.Drawing.Font("Corbel", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TZoomOut.ForeColor = System.Drawing.Color.DarkGray
        Me.TZoomOut.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TZoomOut.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TZoomOut.HideSelection = False
        Me.TZoomOut.Location = New System.Drawing.Point(532, 71)
        Me.TZoomOut.MaxLength = 32767
        Me.TZoomOut.Multiline = False
        Me.TZoomOut.Name = "TZoomOut"
        Me.TZoomOut.ReadOnly = False
        Me.TZoomOut.Rounded = 0
        Me.TZoomOut.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TZoomOut.SelectedText = ""
        Me.TZoomOut.SelectionLength = 0
        Me.TZoomOut.SelectionStart = 0
        Me.TZoomOut.ShortcutsEnabled = False
        Me.TZoomOut.Size = New System.Drawing.Size(21, 20)
        Me.TZoomOut.TabIndex = 98
        Me.TZoomOut.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TZoomOut.UseSystemPasswordChar = False
        Me.TZoomOut.WordWrap = False
        '
        'LTransfer
        '
        Me.LTransfer.BackColor = System.Drawing.Color.Transparent
        Me.LTransfer.Cursor = System.Windows.Forms.Cursors.Hand
        Me.LTransfer.Font = New System.Drawing.Font("Corbel", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LTransfer.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LTransfer.Location = New System.Drawing.Point(108, 72)
        Me.LTransfer.Name = "LTransfer"
        Me.LTransfer.Size = New System.Drawing.Size(199, 17)
        Me.LTransfer.TabIndex = 46
        Me.LTransfer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.LTransfer.Visible = False
        '
        'FGallery
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1331, 755)
        Me.ControlBox = False
        Me.Controls.Add(Me.LZoomIn)
        Me.Controls.Add(Me.TZoomIn)
        Me.Controls.Add(Me.LZoomOut)
        Me.Controls.Add(Me.TZoomOut)
        Me.Controls.Add(Me.LCGenre)
        Me.Controls.Add(Me.TCGenre)
        Me.Controls.Add(Me.LBMovies)
        Me.Controls.Add(Me.Separator)
        Me.Controls.Add(Me.LPlot)
        Me.Controls.Add(Me.PCover)
        Me.Controls.Add(Me.LAz)
        Me.Controls.Add(Me.LCSort)
        Me.Controls.Add(Me.TSort)
        Me.Controls.Add(Me.CSort)
        Me.Controls.Add(Me.LSort)
        Me.Controls.Add(Me.LCCountry)
        Me.Controls.Add(Me.LCActor)
        Me.Controls.Add(Me.LCDirector)
        Me.Controls.Add(Me.LCLanguage)
        Me.Controls.Add(Me.TCDirector)
        Me.Controls.Add(Me.TCLanguage)
        Me.Controls.Add(Me.TCActor)
        Me.Controls.Add(Me.TCCountry)
        Me.Controls.Add(Me.LCYear)
        Me.Controls.Add(Me.TCYear)
        Me.Controls.Add(Me.CActor)
        Me.Controls.Add(Me.CDirector)
        Me.Controls.Add(Me.CCountry)
        Me.Controls.Add(Me.CLanguage)
        Me.Controls.Add(Me.TSearch)
        Me.Controls.Add(Me.TDuration)
        Me.Controls.Add(Me.TRating)
        Me.Controls.Add(Me.CYear)
        Me.Controls.Add(Me.LRandom)
        Me.Controls.Add(Me.LIRating)
        Me.Controls.Add(Me.BClose)
        Me.Controls.Add(Me.BMinimize)
        Me.Controls.Add(Me.LFRating)
        Me.Controls.Add(Me.LTitle)
        Me.Controls.Add(Me.LILanguage)
        Me.Controls.Add(Me.LICountry)
        Me.Controls.Add(Me.LIActor)
        Me.Controls.Add(Me.LIDirector)
        Me.Controls.Add(Me.LIGenre)
        Me.Controls.Add(Me.LFActor)
        Me.Controls.Add(Me.LFCountry)
        Me.Controls.Add(Me.LIDuration)
        Me.Controls.Add(Me.LFGenre)
        Me.Controls.Add(Me.LFRatingThan)
        Me.Controls.Add(Me.LFDurationThan)
        Me.Controls.Add(Me.LFDirector)
        Me.Controls.Add(Me.LFDuration)
        Me.Controls.Add(Me.LFYear)
        Me.Controls.Add(Me.LLanguage)
        Me.Controls.Add(Me.LIAdded)
        Me.Controls.Add(Me.LTransfer)
        Me.Controls.Add(Me.LCountImdb)
        Me.Controls.Add(Me.LCountInfo)
        Me.Controls.Add(Me.LIYear)
        Me.Controls.Add(Me.LCountry)
        Me.Controls.Add(Me.LActor)
        Me.Controls.Add(Me.LDirector)
        Me.Controls.Add(Me.LGenre)
        Me.Controls.Add(Me.LDuration)
        Me.Controls.Add(Me.LAdded)
        Me.Controls.Add(Me.LRating)
        Me.Controls.Add(Me.LYear)
        Me.Controls.Add(Me.THidden)
        Me.Controls.Add(Me.LGallery)
        Me.Controls.Add(Me.Gallery)
        Me.Controls.Add(Me.LAbout)
        Me.Controls.Add(Me.TBMovies)
        Me.Controls.Add(Me.LFLanguage)
        Me.Controls.Add(Me.TAz)
        Me.Controls.Add(Me.CMode)
        Me.Controls.Add(Me.CGenre)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(1331, 755)
        Me.Name = "FGallery"
        Me.Opacity = 0R
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "K-Movies 4.0"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MGMenu.ResumeLayout(False)
        Me.MPMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MPMenu As KMovies.FlatContextMenuStrip
    Friend WithEvents MPlay As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MDelete As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MSettings As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Gallery As KMovies.ImageGalleryVB
    Friend WithEvents MCopyto As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MTrailer As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents MSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents MSeparator5 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents MExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MGMenu As KMovies.FlatContextMenuStrip
    Friend WithEvents MGSettings As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MGSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents MGExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LGallery As KMovies.FlatLabel
    Friend WithEvents MSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents PCover As PictureBox
    Friend WithEvents LPlot As Label
    Friend WithEvents THidden As TextBox
    Friend WithEvents LYear As Label
    Friend WithEvents LIYear As Label
    Friend WithEvents LRating As Label
    Friend WithEvents LIRating As Label
    Friend WithEvents LDuration As Label
    Friend WithEvents LIDuration As Label
    Friend WithEvents LGenre As Label
    Friend WithEvents LIGenre As Label
    Friend WithEvents LCountry As Label
    Friend WithEvents LICountry As Label
    Friend WithEvents LDirector As Label
    Friend WithEvents LIDirector As Label
    Friend WithEvents LActor As Label
    Friend WithEvents LIActor As Label
    Friend WithEvents LFYear As Label
    Friend WithEvents LFLanguage As Label
    Friend WithEvents LFCountry As Label
    Friend WithEvents LFDirector As Label
    Friend WithEvents LFActor As Label
    Friend WithEvents LFGenre As Label
    Friend WithEvents LTitle As Label
    Friend WithEvents LFRating As Label
    Friend WithEvents LFDurationThan As Label
    Friend WithEvents LLanguage As Label
    Friend WithEvents LILanguage As Label
    Friend WithEvents LFRatingThan As Label
    Friend WithEvents LFDuration As Label
    Friend WithEvents LAbout As Label
    Friend WithEvents BClose As FlatLabel
    Friend WithEvents BMinimize As FlatLabel
    Friend WithEvents Separator As LineSeparator
    Friend WithEvents LAdded As Label
    Friend WithEvents LIAdded As Label
    Friend WithEvents LCountInfo As Label
    Friend WithEvents MEdit As ToolStripMenuItem
    Friend WithEvents MImdb As ToolStripMenuItem
    Friend WithEvents MSeparator0 As ToolStripSeparator
    Friend WithEvents LCountImdb As Label
    Friend WithEvents LRandom As Label
    Friend WithEvents CYear As FlatComboBox
    Friend WithEvents TRating As NSTextBox
    Friend WithEvents TDuration As NSTextBox
    Friend WithEvents TSearch As NSTextBox
    Friend WithEvents CGenre As FlatComboBox
    Friend WithEvents CLanguage As FlatComboBox
    Friend WithEvents CDirector As FlatComboBox
    Friend WithEvents CCountry As FlatComboBox
    Friend WithEvents CActor As FlatComboBox
    Friend WithEvents TCYear As NSTextBox
    Friend WithEvents LCYear As Label
    Friend WithEvents TCGenre As NSTextBox
    Friend WithEvents LCGenre As Label
    Friend WithEvents TCCountry As NSTextBox
    Friend WithEvents LCCountry As Label
    Friend WithEvents TCActor As NSTextBox
    Friend WithEvents LCActor As Label
    Friend WithEvents TCLanguage As NSTextBox
    Friend WithEvents LCLanguage As Label
    Friend WithEvents TCDirector As NSTextBox
    Friend WithEvents LCDirector As Label
    Friend WithEvents TBMovies As NSTextBox
    Friend WithEvents LBMovies As Label
    Friend WithEvents LCSort As Label
    Friend WithEvents TSort As NSTextBox
    Friend WithEvents CSort As FlatComboBox
    Friend WithEvents LSort As Label
    Friend WithEvents TAz As NSTextBox
    Friend WithEvents LAz As FlatText
    Friend WithEvents MAddTo As ToolStripMenuItem
    Friend WithEvents MPending As ToolStripMenuItem
    Friend WithEvents MWatched As ToolStripMenuItem
    Friend WithEvents MSeparator4 As ToolStripSeparator
    Friend WithEvents CMode As FlatComboBox
    Friend WithEvents MGAdd As ToolStripMenuItem
    Friend WithEvents MGSeparator1 As ToolStripSeparator
    Friend WithEvents MMoveto As ToolStripMenuItem
    Friend WithEvents MAdd As ToolStripMenuItem
    Friend WithEvents LZoomIn As FlatText
    Friend WithEvents TZoomIn As NSTextBox
    Friend WithEvents LZoomOut As FlatText
    Friend WithEvents TZoomOut As NSTextBox
    Friend WithEvents LTransfer As Label
End Class
