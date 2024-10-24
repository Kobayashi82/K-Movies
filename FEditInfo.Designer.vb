<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FEditInfo
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FEditInfo))
        Me.LIYear = New System.Windows.Forms.Label()
        Me.LIRating = New System.Windows.Forms.Label()
        Me.LIDuration = New System.Windows.Forms.Label()
        Me.LIGenre = New System.Windows.Forms.Label()
        Me.LILanguage = New System.Windows.Forms.Label()
        Me.LICountry = New System.Windows.Forms.Label()
        Me.LIDirector = New System.Windows.Forms.Label()
        Me.LIActor = New System.Windows.Forms.Label()
        Me.PCover = New System.Windows.Forms.PictureBox()
        Me.LSTitle = New System.Windows.Forms.Label()
        Me.LSImdb = New System.Windows.Forms.Label()
        Me.LIImdb = New System.Windows.Forms.Label()
        Me.THidden = New System.Windows.Forms.TextBox()
        Me.LRemoveCover = New System.Windows.Forms.Label()
        Me.LSynopsis = New System.Windows.Forms.Label()
        Me.LGallery = New System.Windows.Forms.Label()
        Me.BFAccept = New System.Windows.Forms.Button()
        Me.BFCancel = New System.Windows.Forms.Button()
        Me.BFTrailer = New System.Windows.Forms.Button()
        Me.BFImdbWeb = New System.Windows.Forms.Button()
        Me.BImdbWeb = New KMovies.FlatButton()
        Me.BCancel = New KMovies.FlatButton()
        Me.BTrailer = New KMovies.FlatButton()
        Me.BAccept = New KMovies.FlatButton()
        Me.TSImdb = New KMovies.NSTextBox()
        Me.TSTitle = New KMovies.NSTextBox()
        Me.TPlot = New KMovies.NSTextBox()
        Me.TActor = New KMovies.NSTextBox()
        Me.TDirector = New KMovies.NSTextBox()
        Me.TCountry = New KMovies.NSTextBox()
        Me.TLanguage = New KMovies.NSTextBox()
        Me.TGenre = New KMovies.NSTextBox()
        Me.TImdb = New KMovies.NSTextBox()
        Me.TDuration = New KMovies.NSTextBox()
        Me.TRating = New KMovies.NSTextBox()
        Me.TYear = New KMovies.NSTextBox()
        Me.Gallery = New KMovies.ImageGalleryVB()
        Me.BClose = New KMovies.FlatLabel()
        Me.LTitle = New KMovies.FlatLabel()
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LIYear
        '
        Me.LIYear.BackColor = System.Drawing.Color.Transparent
        Me.LIYear.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIYear.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIYear.Location = New System.Drawing.Point(22, 71)
        Me.LIYear.Name = "LIYear"
        Me.LIYear.Size = New System.Drawing.Size(90, 27)
        Me.LIYear.TabIndex = 56
        Me.LIYear.Text = "Year:"
        Me.LIYear.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIRating
        '
        Me.LIRating.BackColor = System.Drawing.Color.Transparent
        Me.LIRating.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIRating.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIRating.Location = New System.Drawing.Point(22, 107)
        Me.LIRating.Name = "LIRating"
        Me.LIRating.Size = New System.Drawing.Size(90, 27)
        Me.LIRating.TabIndex = 56
        Me.LIRating.Text = "Rating:"
        Me.LIRating.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIDuration
        '
        Me.LIDuration.BackColor = System.Drawing.Color.Transparent
        Me.LIDuration.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIDuration.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIDuration.Location = New System.Drawing.Point(22, 143)
        Me.LIDuration.Name = "LIDuration"
        Me.LIDuration.Size = New System.Drawing.Size(90, 27)
        Me.LIDuration.TabIndex = 56
        Me.LIDuration.Text = "Duration:"
        Me.LIDuration.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIGenre
        '
        Me.LIGenre.BackColor = System.Drawing.Color.Transparent
        Me.LIGenre.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIGenre.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIGenre.Location = New System.Drawing.Point(22, 213)
        Me.LIGenre.Name = "LIGenre"
        Me.LIGenre.Size = New System.Drawing.Size(90, 27)
        Me.LIGenre.TabIndex = 56
        Me.LIGenre.Text = "Genre:"
        Me.LIGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LILanguage
        '
        Me.LILanguage.BackColor = System.Drawing.Color.Transparent
        Me.LILanguage.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LILanguage.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LILanguage.Location = New System.Drawing.Point(22, 249)
        Me.LILanguage.Name = "LILanguage"
        Me.LILanguage.Size = New System.Drawing.Size(90, 27)
        Me.LILanguage.TabIndex = 56
        Me.LILanguage.Text = "Language:"
        Me.LILanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LICountry
        '
        Me.LICountry.BackColor = System.Drawing.Color.Transparent
        Me.LICountry.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LICountry.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LICountry.Location = New System.Drawing.Point(22, 285)
        Me.LICountry.Name = "LICountry"
        Me.LICountry.Size = New System.Drawing.Size(90, 27)
        Me.LICountry.TabIndex = 56
        Me.LICountry.Text = "Country:"
        Me.LICountry.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIDirector
        '
        Me.LIDirector.BackColor = System.Drawing.Color.Transparent
        Me.LIDirector.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIDirector.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIDirector.Location = New System.Drawing.Point(22, 321)
        Me.LIDirector.Name = "LIDirector"
        Me.LIDirector.Size = New System.Drawing.Size(90, 27)
        Me.LIDirector.TabIndex = 56
        Me.LIDirector.Text = "Director:"
        Me.LIDirector.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIActor
        '
        Me.LIActor.BackColor = System.Drawing.Color.Transparent
        Me.LIActor.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIActor.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIActor.Location = New System.Drawing.Point(22, 357)
        Me.LIActor.Name = "LIActor"
        Me.LIActor.Size = New System.Drawing.Size(90, 27)
        Me.LIActor.TabIndex = 56
        Me.LIActor.Text = "Actor:"
        Me.LIActor.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'PCover
        '
        Me.PCover.BackColor = System.Drawing.Color.Transparent
        Me.PCover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.PCover.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PCover.Location = New System.Drawing.Point(343, 71)
        Me.PCover.Name = "PCover"
        Me.PCover.Size = New System.Drawing.Size(265, 363)
        Me.PCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PCover.TabIndex = 57
        Me.PCover.TabStop = False
        '
        'LSTitle
        '
        Me.LSTitle.BackColor = System.Drawing.Color.Transparent
        Me.LSTitle.Font = New System.Drawing.Font("Bell MT", 12.0!)
        Me.LSTitle.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LSTitle.Location = New System.Drawing.Point(612, 25)
        Me.LSTitle.Name = "LSTitle"
        Me.LSTitle.Size = New System.Drawing.Size(59, 27)
        Me.LSTitle.TabIndex = 56
        Me.LSTitle.Text = "Title:"
        Me.LSTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LSImdb
        '
        Me.LSImdb.BackColor = System.Drawing.Color.Transparent
        Me.LSImdb.Font = New System.Drawing.Font("Bell MT", 12.0!)
        Me.LSImdb.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LSImdb.Location = New System.Drawing.Point(846, 25)
        Me.LSImdb.Name = "LSImdb"
        Me.LSImdb.Size = New System.Drawing.Size(47, 27)
        Me.LSImdb.TabIndex = 56
        Me.LSImdb.Text = "Imdb:"
        Me.LSImdb.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LIImdb
        '
        Me.LIImdb.BackColor = System.Drawing.Color.Transparent
        Me.LIImdb.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LIImdb.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LIImdb.Location = New System.Drawing.Point(22, 178)
        Me.LIImdb.Name = "LIImdb"
        Me.LIImdb.Size = New System.Drawing.Size(90, 27)
        Me.LIImdb.TabIndex = 56
        Me.LIImdb.Text = "Imdb:"
        Me.LIImdb.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'THidden
        '
        Me.THidden.Location = New System.Drawing.Point(-500, 238)
        Me.THidden.Name = "THidden"
        Me.THidden.ReadOnly = True
        Me.THidden.ShortcutsEnabled = False
        Me.THidden.Size = New System.Drawing.Size(18, 20)
        Me.THidden.TabIndex = 0
        '
        'LRemoveCover
        '
        Me.LRemoveCover.AutoSize = True
        Me.LRemoveCover.BackColor = System.Drawing.Color.Black
        Me.LRemoveCover.Cursor = System.Windows.Forms.Cursors.Hand
        Me.LRemoveCover.ForeColor = System.Drawing.Color.Maroon
        Me.LRemoveCover.Location = New System.Drawing.Point(593, 72)
        Me.LRemoveCover.Name = "LRemoveCover"
        Me.LRemoveCover.Size = New System.Drawing.Size(14, 13)
        Me.LRemoveCover.TabIndex = 60
        Me.LRemoveCover.Text = "X"
        '
        'LSynopsis
        '
        Me.LSynopsis.BackColor = System.Drawing.Color.Transparent
        Me.LSynopsis.Font = New System.Drawing.Font("Bell MT", 13.0!)
        Me.LSynopsis.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LSynopsis.Location = New System.Drawing.Point(17, 420)
        Me.LSynopsis.Name = "LSynopsis"
        Me.LSynopsis.Size = New System.Drawing.Size(90, 27)
        Me.LSynopsis.TabIndex = 56
        Me.LSynopsis.Text = "Synopsis:"
        Me.LSynopsis.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LGallery
        '
        Me.LGallery.BackColor = System.Drawing.Color.Transparent
        Me.LGallery.Font = New System.Drawing.Font("Corbel", 12.0!)
        Me.LGallery.ForeColor = System.Drawing.Color.LightSlateGray
        Me.LGallery.Location = New System.Drawing.Point(627, 62)
        Me.LGallery.Name = "LGallery"
        Me.LGallery.Size = New System.Drawing.Size(391, 463)
        Me.LGallery.TabIndex = 61
        Me.LGallery.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'BFAccept
        '
        Me.BFAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFAccept.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFAccept.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFAccept.Location = New System.Drawing.Point(343, 495)
        Me.BFAccept.Name = "BFAccept"
        Me.BFAccept.Size = New System.Drawing.Size(124, 30)
        Me.BFAccept.TabIndex = 65
        Me.BFAccept.TabStop = False
        Me.BFAccept.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFAccept.UseVisualStyleBackColor = True
        '
        'BFCancel
        '
        Me.BFCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFCancel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFCancel.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFCancel.Location = New System.Drawing.Point(483, 495)
        Me.BFCancel.Name = "BFCancel"
        Me.BFCancel.Size = New System.Drawing.Size(124, 30)
        Me.BFCancel.TabIndex = 65
        Me.BFCancel.TabStop = False
        Me.BFCancel.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFCancel.UseVisualStyleBackColor = True
        '
        'BFTrailer
        '
        Me.BFTrailer.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFTrailer.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFTrailer.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFTrailer.Location = New System.Drawing.Point(343, 448)
        Me.BFTrailer.Name = "BFTrailer"
        Me.BFTrailer.Size = New System.Drawing.Size(124, 30)
        Me.BFTrailer.TabIndex = 65
        Me.BFTrailer.TabStop = False
        Me.BFTrailer.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFTrailer.UseVisualStyleBackColor = True
        '
        'BFImdbWeb
        '
        Me.BFImdbWeb.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BFImdbWeb.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.BFImdbWeb.ForeColor = System.Drawing.SystemColors.GrayText
        Me.BFImdbWeb.Location = New System.Drawing.Point(482, 448)
        Me.BFImdbWeb.Name = "BFImdbWeb"
        Me.BFImdbWeb.Size = New System.Drawing.Size(124, 30)
        Me.BFImdbWeb.TabIndex = 65
        Me.BFImdbWeb.TabStop = False
        Me.BFImdbWeb.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.BFImdbWeb.UseVisualStyleBackColor = True
        '
        'BImdbWeb
        '
        Me.BImdbWeb.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BImdbWeb.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BImdbWeb.Cursor = System.Windows.Forms.Cursors.Default
        Me.BImdbWeb.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BImdbWeb.ForeColor = System.Drawing.Color.DimGray
        Me.BImdbWeb.Hover = True
        Me.BImdbWeb.Location = New System.Drawing.Point(483, 449)
        Me.BImdbWeb.Name = "BImdbWeb"
        Me.BImdbWeb.Rounded = False
        Me.BImdbWeb.Size = New System.Drawing.Size(122, 28)
        Me.BImdbWeb.TabIndex = 164
        Me.BImdbWeb.Text = "IMDB"
        Me.BImdbWeb.TextColor = System.Drawing.Color.DarkGray
        '
        'BCancel
        '
        Me.BCancel.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.BCancel.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BCancel.ForeColor = System.Drawing.Color.DimGray
        Me.BCancel.Hover = True
        Me.BCancel.Location = New System.Drawing.Point(484, 496)
        Me.BCancel.Name = "BCancel"
        Me.BCancel.Rounded = False
        Me.BCancel.Size = New System.Drawing.Size(122, 28)
        Me.BCancel.TabIndex = 164
        Me.BCancel.Text = "Cancel"
        Me.BCancel.TextColor = System.Drawing.Color.DarkGray
        '
        'BTrailer
        '
        Me.BTrailer.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BTrailer.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BTrailer.Cursor = System.Windows.Forms.Cursors.Default
        Me.BTrailer.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BTrailer.ForeColor = System.Drawing.Color.DimGray
        Me.BTrailer.Hover = True
        Me.BTrailer.Location = New System.Drawing.Point(344, 449)
        Me.BTrailer.Name = "BTrailer"
        Me.BTrailer.Rounded = False
        Me.BTrailer.Size = New System.Drawing.Size(122, 28)
        Me.BTrailer.TabIndex = 164
        Me.BTrailer.Text = "Trailer"
        Me.BTrailer.TextColor = System.Drawing.Color.DarkGray
        '
        'BAccept
        '
        Me.BAccept.BackColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BAccept.BaseColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer), CType(CType(50, Byte), Integer))
        Me.BAccept.Cursor = System.Windows.Forms.Cursors.Default
        Me.BAccept.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BAccept.ForeColor = System.Drawing.Color.DimGray
        Me.BAccept.Hover = True
        Me.BAccept.Location = New System.Drawing.Point(344, 496)
        Me.BAccept.Name = "BAccept"
        Me.BAccept.Rounded = False
        Me.BAccept.Size = New System.Drawing.Size(122, 28)
        Me.BAccept.TabIndex = 73
        Me.BAccept.Text = "Accept"
        Me.BAccept.TextColor = System.Drawing.Color.DarkGray
        '
        'TSImdb
        '
        Me.TSImdb.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TSImdb.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TSImdb.Cursor = System.Windows.Forms.Cursors.Default
        Me.TSImdb.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TSImdb.ForeColor = System.Drawing.Color.DarkGray
        Me.TSImdb.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TSImdb.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TSImdb.HideSelection = False
        Me.TSImdb.Location = New System.Drawing.Point(895, 29)
        Me.TSImdb.MaxLength = 32767
        Me.TSImdb.Multiline = False
        Me.TSImdb.Name = "TSImdb"
        Me.TSImdb.ReadOnly = False
        Me.TSImdb.Rounded = 0
        Me.TSImdb.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TSImdb.SelectedText = ""
        Me.TSImdb.SelectionLength = 0
        Me.TSImdb.SelectionStart = 0
        Me.TSImdb.ShortcutsEnabled = True
        Me.TSImdb.Size = New System.Drawing.Size(95, 19)
        Me.TSImdb.TabIndex = 61
        Me.TSImdb.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TSImdb.UseSystemPasswordChar = False
        Me.TSImdb.WordWrap = False
        '
        'TSTitle
        '
        Me.TSTitle.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TSTitle.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TSTitle.Cursor = System.Windows.Forms.Cursors.Default
        Me.TSTitle.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TSTitle.ForeColor = System.Drawing.Color.DarkGray
        Me.TSTitle.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TSTitle.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TSTitle.HideSelection = False
        Me.TSTitle.Location = New System.Drawing.Point(672, 29)
        Me.TSTitle.MaxLength = 32767
        Me.TSTitle.Multiline = False
        Me.TSTitle.Name = "TSTitle"
        Me.TSTitle.ReadOnly = False
        Me.TSTitle.Rounded = 0
        Me.TSTitle.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TSTitle.SelectedText = ""
        Me.TSTitle.SelectionLength = 0
        Me.TSTitle.SelectionStart = 0
        Me.TSTitle.ShortcutsEnabled = True
        Me.TSTitle.Size = New System.Drawing.Size(170, 19)
        Me.TSTitle.TabIndex = 60
        Me.TSTitle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TSTitle.UseSystemPasswordChar = False
        Me.TSTitle.WordWrap = False
        '
        'TPlot
        '
        Me.TPlot.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TPlot.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TPlot.Cursor = System.Windows.Forms.Cursors.Default
        Me.TPlot.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TPlot.ForeColor = System.Drawing.Color.DarkGray
        Me.TPlot.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TPlot.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TPlot.HideSelection = False
        Me.TPlot.Location = New System.Drawing.Point(26, 448)
        Me.TPlot.MaxLength = 32767
        Me.TPlot.Multiline = True
        Me.TPlot.Name = "TPlot"
        Me.TPlot.ReadOnly = False
        Me.TPlot.Rounded = 0
        Me.TPlot.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TPlot.SelectedText = ""
        Me.TPlot.SelectionLength = 0
        Me.TPlot.SelectionStart = 0
        Me.TPlot.ShortcutsEnabled = True
        Me.TPlot.Size = New System.Drawing.Size(301, 77)
        Me.TPlot.TabIndex = 72
        Me.TPlot.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TPlot.UseSystemPasswordChar = False
        Me.TPlot.WordWrap = False
        '
        'TActor
        '
        Me.TActor.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TActor.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TActor.Cursor = System.Windows.Forms.Cursors.Default
        Me.TActor.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TActor.ForeColor = System.Drawing.Color.DarkGray
        Me.TActor.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TActor.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TActor.HideSelection = False
        Me.TActor.Location = New System.Drawing.Point(121, 359)
        Me.TActor.MaxLength = 32767
        Me.TActor.Multiline = True
        Me.TActor.Name = "TActor"
        Me.TActor.ReadOnly = False
        Me.TActor.Rounded = 0
        Me.TActor.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TActor.SelectedText = ""
        Me.TActor.SelectionLength = 0
        Me.TActor.SelectionStart = 0
        Me.TActor.ShortcutsEnabled = True
        Me.TActor.Size = New System.Drawing.Size(206, 75)
        Me.TActor.TabIndex = 71
        Me.TActor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TActor.UseSystemPasswordChar = False
        Me.TActor.WordWrap = False
        '
        'TDirector
        '
        Me.TDirector.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TDirector.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TDirector.Cursor = System.Windows.Forms.Cursors.Default
        Me.TDirector.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TDirector.ForeColor = System.Drawing.Color.DarkGray
        Me.TDirector.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TDirector.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TDirector.HideSelection = False
        Me.TDirector.Location = New System.Drawing.Point(121, 326)
        Me.TDirector.MaxLength = 32767
        Me.TDirector.Multiline = False
        Me.TDirector.Name = "TDirector"
        Me.TDirector.ReadOnly = False
        Me.TDirector.Rounded = 0
        Me.TDirector.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TDirector.SelectedText = ""
        Me.TDirector.SelectionLength = 0
        Me.TDirector.SelectionStart = 0
        Me.TDirector.ShortcutsEnabled = True
        Me.TDirector.Size = New System.Drawing.Size(206, 19)
        Me.TDirector.TabIndex = 70
        Me.TDirector.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TDirector.UseSystemPasswordChar = False
        Me.TDirector.WordWrap = False
        '
        'TCountry
        '
        Me.TCountry.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TCountry.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TCountry.Cursor = System.Windows.Forms.Cursors.Default
        Me.TCountry.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TCountry.ForeColor = System.Drawing.Color.DarkGray
        Me.TCountry.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TCountry.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TCountry.HideSelection = False
        Me.TCountry.Location = New System.Drawing.Point(121, 291)
        Me.TCountry.MaxLength = 32767
        Me.TCountry.Multiline = False
        Me.TCountry.Name = "TCountry"
        Me.TCountry.ReadOnly = False
        Me.TCountry.Rounded = 0
        Me.TCountry.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TCountry.SelectedText = ""
        Me.TCountry.SelectionLength = 0
        Me.TCountry.SelectionStart = 0
        Me.TCountry.ShortcutsEnabled = True
        Me.TCountry.Size = New System.Drawing.Size(206, 19)
        Me.TCountry.TabIndex = 69
        Me.TCountry.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TCountry.UseSystemPasswordChar = False
        Me.TCountry.WordWrap = False
        '
        'TLanguage
        '
        Me.TLanguage.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TLanguage.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TLanguage.Cursor = System.Windows.Forms.Cursors.Default
        Me.TLanguage.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TLanguage.ForeColor = System.Drawing.Color.DarkGray
        Me.TLanguage.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TLanguage.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TLanguage.HideSelection = False
        Me.TLanguage.Location = New System.Drawing.Point(121, 255)
        Me.TLanguage.MaxLength = 32767
        Me.TLanguage.Multiline = False
        Me.TLanguage.Name = "TLanguage"
        Me.TLanguage.ReadOnly = False
        Me.TLanguage.Rounded = 0
        Me.TLanguage.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TLanguage.SelectedText = ""
        Me.TLanguage.SelectionLength = 0
        Me.TLanguage.SelectionStart = 0
        Me.TLanguage.ShortcutsEnabled = True
        Me.TLanguage.Size = New System.Drawing.Size(206, 19)
        Me.TLanguage.TabIndex = 68
        Me.TLanguage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TLanguage.UseSystemPasswordChar = False
        Me.TLanguage.WordWrap = False
        '
        'TGenre
        '
        Me.TGenre.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TGenre.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TGenre.Cursor = System.Windows.Forms.Cursors.Default
        Me.TGenre.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TGenre.ForeColor = System.Drawing.Color.DarkGray
        Me.TGenre.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TGenre.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TGenre.HideSelection = False
        Me.TGenre.Location = New System.Drawing.Point(121, 219)
        Me.TGenre.MaxLength = 32767
        Me.TGenre.Multiline = False
        Me.TGenre.Name = "TGenre"
        Me.TGenre.ReadOnly = False
        Me.TGenre.Rounded = 0
        Me.TGenre.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TGenre.SelectedText = ""
        Me.TGenre.SelectionLength = 0
        Me.TGenre.SelectionStart = 0
        Me.TGenre.ShortcutsEnabled = True
        Me.TGenre.Size = New System.Drawing.Size(206, 19)
        Me.TGenre.TabIndex = 67
        Me.TGenre.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TGenre.UseSystemPasswordChar = False
        Me.TGenre.WordWrap = False
        '
        'TImdb
        '
        Me.TImdb.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TImdb.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TImdb.Cursor = System.Windows.Forms.Cursors.Default
        Me.TImdb.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TImdb.ForeColor = System.Drawing.Color.DarkGray
        Me.TImdb.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TImdb.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TImdb.HideSelection = False
        Me.TImdb.Location = New System.Drawing.Point(121, 184)
        Me.TImdb.MaxLength = 32767
        Me.TImdb.Multiline = False
        Me.TImdb.Name = "TImdb"
        Me.TImdb.ReadOnly = False
        Me.TImdb.Rounded = 0
        Me.TImdb.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TImdb.SelectedText = ""
        Me.TImdb.SelectionLength = 0
        Me.TImdb.SelectionStart = 0
        Me.TImdb.ShortcutsEnabled = True
        Me.TImdb.Size = New System.Drawing.Size(93, 19)
        Me.TImdb.TabIndex = 66
        Me.TImdb.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TImdb.UseSystemPasswordChar = False
        Me.TImdb.WordWrap = False
        '
        'TDuration
        '
        Me.TDuration.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TDuration.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TDuration.Cursor = System.Windows.Forms.Cursors.Default
        Me.TDuration.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TDuration.ForeColor = System.Drawing.Color.DarkGray
        Me.TDuration.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TDuration.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TDuration.HideSelection = False
        Me.TDuration.Location = New System.Drawing.Point(121, 148)
        Me.TDuration.MaxLength = 32767
        Me.TDuration.Multiline = False
        Me.TDuration.Name = "TDuration"
        Me.TDuration.ReadOnly = False
        Me.TDuration.Rounded = 0
        Me.TDuration.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TDuration.SelectedText = ""
        Me.TDuration.SelectionLength = 0
        Me.TDuration.SelectionStart = 0
        Me.TDuration.ShortcutsEnabled = True
        Me.TDuration.Size = New System.Drawing.Size(93, 19)
        Me.TDuration.TabIndex = 65
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
        Me.TRating.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TRating.ForeColor = System.Drawing.Color.DarkGray
        Me.TRating.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TRating.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TRating.HideSelection = False
        Me.TRating.Location = New System.Drawing.Point(121, 112)
        Me.TRating.MaxLength = 32767
        Me.TRating.Multiline = False
        Me.TRating.Name = "TRating"
        Me.TRating.ReadOnly = False
        Me.TRating.Rounded = 0
        Me.TRating.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TRating.SelectedText = ""
        Me.TRating.SelectionLength = 0
        Me.TRating.SelectionStart = 0
        Me.TRating.ShortcutsEnabled = True
        Me.TRating.Size = New System.Drawing.Size(93, 19)
        Me.TRating.TabIndex = 64
        Me.TRating.Text = "0.0"
        Me.TRating.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TRating.UseSystemPasswordChar = False
        Me.TRating.WordWrap = False
        '
        'TYear
        '
        Me.TYear.BorderColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.TYear.ControlCursor = System.Windows.Forms.Cursors.Default
        Me.TYear.Cursor = System.Windows.Forms.Cursors.Default
        Me.TYear.Font = New System.Drawing.Font("Calibri", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TYear.ForeColor = System.Drawing.Color.DarkGray
        Me.TYear.ForeColor1 = System.Drawing.Color.DarkGray
        Me.TYear.ForeColor2 = System.Drawing.Color.DarkGray
        Me.TYear.HideSelection = False
        Me.TYear.Location = New System.Drawing.Point(121, 76)
        Me.TYear.MaxLength = 32767
        Me.TYear.Multiline = False
        Me.TYear.Name = "TYear"
        Me.TYear.ReadOnly = False
        Me.TYear.Rounded = 0
        Me.TYear.ScrollBars = System.Windows.Forms.ScrollBars.None
        Me.TYear.SelectedText = ""
        Me.TYear.SelectionLength = 0
        Me.TYear.SelectionStart = 0
        Me.TYear.ShortcutsEnabled = True
        Me.TYear.Size = New System.Drawing.Size(93, 19)
        Me.TYear.TabIndex = 63
        Me.TYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.TYear.UseSystemPasswordChar = False
        Me.TYear.WordWrap = False
        '
        'Gallery
        '
        Me.Gallery.AutoScroll = True
        Me.Gallery.BackColor = System.Drawing.Color.Transparent
        Me.Gallery.Location = New System.Drawing.Point(632, 62)
        Me.Gallery.Name = "Gallery"
        Me.Gallery.Size = New System.Drawing.Size(391, 463)
        Me.Gallery.TabIndex = 39
        '
        'BClose
        '
        Me.BClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BClose.AutoSize = True
        Me.BClose.BackColor = System.Drawing.Color.Transparent
        Me.BClose.Font = New System.Drawing.Font("Calibri Light", 15.75!)
        Me.BClose.ForeColor = System.Drawing.Color.LightSlateGray
        Me.BClose.Location = New System.Drawing.Point(983, -4)
        Me.BClose.Name = "BClose"
        Me.BClose.Size = New System.Drawing.Size(21, 26)
        Me.BClose.TabIndex = 37
        Me.BClose.Text = "ꭗ"
        '
        'LTitle
        '
        Me.LTitle.BackColor = System.Drawing.Color.Transparent
        Me.LTitle.Font = New System.Drawing.Font("Calibri Light", 25.0!)
        Me.LTitle.ForeColor = System.Drawing.Color.DarkGray
        Me.LTitle.Location = New System.Drawing.Point(12, 8)
        Me.LTitle.Name = "LTitle"
        Me.LTitle.Size = New System.Drawing.Size(596, 50)
        Me.LTitle.TabIndex = 38
        Me.LTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'FEditInfo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(1006, 540)
        Me.ControlBox = False
        Me.Controls.Add(Me.BImdbWeb)
        Me.Controls.Add(Me.BCancel)
        Me.Controls.Add(Me.BFImdbWeb)
        Me.Controls.Add(Me.BFCancel)
        Me.Controls.Add(Me.BTrailer)
        Me.Controls.Add(Me.BFTrailer)
        Me.Controls.Add(Me.BAccept)
        Me.Controls.Add(Me.BFAccept)
        Me.Controls.Add(Me.TSImdb)
        Me.Controls.Add(Me.TSTitle)
        Me.Controls.Add(Me.TPlot)
        Me.Controls.Add(Me.TActor)
        Me.Controls.Add(Me.TDirector)
        Me.Controls.Add(Me.TCountry)
        Me.Controls.Add(Me.TLanguage)
        Me.Controls.Add(Me.TGenre)
        Me.Controls.Add(Me.TImdb)
        Me.Controls.Add(Me.TDuration)
        Me.Controls.Add(Me.TRating)
        Me.Controls.Add(Me.TYear)
        Me.Controls.Add(Me.LGallery)
        Me.Controls.Add(Me.LRemoveCover)
        Me.Controls.Add(Me.THidden)
        Me.Controls.Add(Me.PCover)
        Me.Controls.Add(Me.LSynopsis)
        Me.Controls.Add(Me.LIActor)
        Me.Controls.Add(Me.LIDirector)
        Me.Controls.Add(Me.LICountry)
        Me.Controls.Add(Me.LSImdb)
        Me.Controls.Add(Me.LSTitle)
        Me.Controls.Add(Me.LILanguage)
        Me.Controls.Add(Me.LIGenre)
        Me.Controls.Add(Me.LIImdb)
        Me.Controls.Add(Me.LIDuration)
        Me.Controls.Add(Me.LIRating)
        Me.Controls.Add(Me.LIYear)
        Me.Controls.Add(Me.Gallery)
        Me.Controls.Add(Me.BClose)
        Me.Controls.Add(Me.LTitle)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.KeyPreview = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FEditInfo"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        CType(Me.PCover, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BClose As FlatLabel
    Friend WithEvents LTitle As FlatLabel
    Friend WithEvents Gallery As ImageGalleryVB
    Friend WithEvents LIYear As Label
    Friend WithEvents LIRating As Label
    Friend WithEvents LIDuration As Label
    Friend WithEvents LIGenre As Label
    Friend WithEvents LILanguage As Label
    Friend WithEvents LICountry As Label
    Friend WithEvents LIDirector As Label
    Friend WithEvents LIActor As Label
    Friend WithEvents PCover As PictureBox
    Friend WithEvents LSTitle As Label
    Friend WithEvents LSImdb As Label
    Friend WithEvents LIImdb As Label
    Friend WithEvents THidden As TextBox
    Friend WithEvents LRemoveCover As Label
    Friend WithEvents LSynopsis As Label
    Friend WithEvents LGallery As Label
    Friend WithEvents TYear As NSTextBox
    Friend WithEvents TRating As NSTextBox
    Friend WithEvents TDuration As NSTextBox
    Friend WithEvents TImdb As NSTextBox
    Friend WithEvents TGenre As NSTextBox
    Friend WithEvents TLanguage As NSTextBox
    Friend WithEvents TCountry As NSTextBox
    Friend WithEvents TDirector As NSTextBox
    Friend WithEvents TActor As NSTextBox
    Friend WithEvents TPlot As NSTextBox
    Friend WithEvents TSTitle As NSTextBox
    Friend WithEvents TSImdb As NSTextBox
    Friend WithEvents BAccept As FlatButton
    Friend WithEvents BFAccept As Button
    Friend WithEvents BFCancel As Button
    Friend WithEvents BCancel As FlatButton
    Friend WithEvents BFTrailer As Button
    Friend WithEvents BTrailer As FlatButton
    Friend WithEvents BFImdbWeb As Button
    Friend WithEvents BImdbWeb As FlatButton
End Class
