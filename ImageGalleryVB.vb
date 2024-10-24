
#Region " Gallery "

Public Class ImageGalleryVB

#Region " Variables "

#Region " Eventos "

    Public Event Movie_Click(CMovie As Object, e As MouseEventArgs)
    Public Event Movie_Enter(CMovie As Object, e As EventArgs)
    Public Event Movie_Leave(CMovie As Object)
    Public Event Movie_Move(CMovie As Object, e As MouseEventArgs)
    Public Event MouseScroll(sender As Object, e As MouseEventArgs)
    Public Event Updated()

#End Region

    Public PicWidth As Integer = 135
    Public PicHeight As Integer = 190

    Dim Edit_Page As Integer

    Dim Zooms(8) As ZoomST
    Dim SepWidth As Integer = 1
    Dim SepHeight As Integer = 1
    Dim nXLocation As Integer = 0
    Dim nYLocation As Integer = 0
    Dim CtrlWidth As Integer = Width
    'Dim CtrlHeight As Integer = Height

#Region " Estructuras "

    Structure ST_Movie
        Dim Ruta As String
        Dim Title As String
        Dim Size As Integer
        Dim Number As Integer
        Dim Rating As String
        Dim Year As String
        Dim Duration As Integer
        Dim Language() As String
        Dim Country() As String
        Dim Genre() As String
        Dim Director() As String
        Dim Actor() As String
        Dim Plot As String
        Dim Subtitle As String
        Dim Cover As Image
        Dim NoCover As Boolean
    End Structure

    Structure ZoomST
        Dim Column As Integer
        Dim Row As Integer
    End Structure

#End Region

#Region " Constructor "

    Public Sub New()
        InitializeComponent() : DoubleBuffered = True : SetStyle(ControlStyles.AllPaintingInWmPaint, True) : BorderStyle = BorderStyle.None
        CalcZooms()
    End Sub

    Public Sub CalcZooms()
        Select Case Screen.FromControl(Me).Bounds.Width
            Case 1440
                Zooms(0).Column = 5 : Zooms(0).Row = 2
                Zooms(1).Column = 8 : Zooms(1).Row = 3
                Zooms(2).Column = 11 : Zooms(2).Row = 4
                Zooms(3).Column = 14 : Zooms(3).Row = 5
                Zooms(4).Column = 17 : Zooms(4).Row = 6
                Zooms(5).Column = 20 : Zooms(5).Row = 7
                Zooms(6).Column = 23 : Zooms(6).Row = 8
                Zooms(7).Column = 26 : Zooms(7).Row = 9
                Zooms(8).Column = 29 : Zooms(8).Row = 10
            Case 1680
                Zooms(0).Column = 5 : Zooms(0).Row = 2
                Zooms(1).Column = 8 : Zooms(1).Row = 3
                Zooms(2).Column = 11 : Zooms(2).Row = 4
                Zooms(3).Column = 14 : Zooms(3).Row = 5
                Zooms(4).Column = 17 : Zooms(4).Row = 6
                Zooms(5).Column = 20 : Zooms(5).Row = 7
                Zooms(6).Column = 23 : Zooms(6).Row = 8
                Zooms(7).Column = 26 : Zooms(7).Row = 9
                Zooms(8).Column = 29 : Zooms(8).Row = 10
            Case 1920
                Zooms(0).Column = 3 : Zooms(0).Row = 2
                Zooms(1).Column = 5 : Zooms(1).Row = 3
                Zooms(2).Column = 7 : Zooms(2).Row = 4
                Zooms(3).Column = 9 : Zooms(3).Row = 5
                Zooms(4).Column = 11 : Zooms(4).Row = 6
                Zooms(5).Column = 13 : Zooms(5).Row = 7
                Zooms(6).Column = 15 : Zooms(6).Row = 8
                Zooms(7).Column = 17 : Zooms(7).Row = 9
                Zooms(8).Column = 19 : Zooms(8).Row = 10
            Case 3440
                Zooms(0).Column = 5 : Zooms(0).Row = 2
                Zooms(1).Column = 8 : Zooms(1).Row = 3
                Zooms(2).Column = 11 : Zooms(2).Row = 4
                Zooms(3).Column = 14 : Zooms(3).Row = 5
                Zooms(4).Column = 17 : Zooms(4).Row = 6
                Zooms(5).Column = 20 : Zooms(5).Row = 7
                Zooms(6).Column = 23 : Zooms(6).Row = 8
                Zooms(7).Column = 26 : Zooms(7).Row = 9
                Zooms(8).Column = 29 : Zooms(8).Row = 10
            Case Else
                Zooms(0).Column = 3 : Zooms(0).Row = 2
                Zooms(1).Column = 5 : Zooms(1).Row = 3
                Zooms(2).Column = 7 : Zooms(2).Row = 4
                Zooms(3).Column = 9 : Zooms(3).Row = 5
                Zooms(4).Column = 11 : Zooms(4).Row = 6
                Zooms(5).Column = 13 : Zooms(5).Row = 7
                Zooms(6).Column = 15 : Zooms(6).Row = 8
                Zooms(7).Column = 17 : Zooms(7).Row = 9
                Zooms(8).Column = 19 : Zooms(8).Row = 10
        End Select
    End Sub

#End Region

#End Region

#Region " Events "

    Private Sub ImageGalleryVB_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        CtrlWidth = Width 'CtrlHeight = Height
    End Sub

    Private Sub ImageGalleryVB_MouseWheel(sender As Object, e As MouseEventArgs) Handles MyBase.MouseWheel
        RaiseEvent MouseScroll(sender, e)
    End Sub

    Private Sub Pic1_MouseMove(sender As Object, e As MouseEventArgs)
        RaiseEvent Movie_Move(sender, e)
    End Sub

    Private Sub Pic1_MouseEnter(sender As Object, e As EventArgs)
        RaiseEvent Movie_Enter(sender, e)
    End Sub

    Private Sub Pic1_MouseLeave(sender As Object, e As EventArgs)
        RaiseEvent Movie_Leave(sender)
    End Sub

    Private Sub Pic1_MouseClick(sender As Object, e As MouseEventArgs)
        RaiseEvent Movie_Click(sender, e)
    End Sub

#End Region

#Region " Methods "

#Region " Zoom "

    Public Sub ZoomIn()
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        cMovies.Zoom = Math.Max(cMovies.Zoom - 1, 0)
        UpdateGallery()
    End Sub

    Public Sub ZoomOut()
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        cMovies.Zoom = Math.Min(cMovies.Zoom + 1, Zooms.Count - 1)
        UpdateGallery()
    End Sub

#End Region

#Region " Pages "

    Public Sub HomePage()
        FGallery.Get_cMovies(FGallery.Mode).Page = 0 : UpdateGallery()
    End Sub

    Public Sub EndPage()
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        cMovies.Page = Math.Ceiling((cMovies.Movies_F.Count / cMovies.MoviesPerPage) - 1) : UpdateGallery()
    End Sub

    Public Sub NextPage()
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        If (cMovies.Page + 1) * cMovies.MoviesPerPage < cMovies.Movies_F.Count Then cMovies.Page += 1 : UpdateGallery()
    End Sub

    Public Sub PreviousPage()
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        If cMovies.Page > 0 Then cMovies.Page -= 1 : UpdateGallery()
    End Sub

#End Region

#Region " Go to Movie "

    Public Sub GoToMovie(Letter As String, Optional isRuta As Boolean = False)
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        If cMovies.Movies_F Is Nothing Then Exit Sub
        If isRuta = False Then
            For Cuenta = 0 To cMovies.Movies_F.Count - 1
                If cMovies.Movies_F(Cuenta).Title IsNot Nothing AndAlso cMovies.Movies_F(Cuenta).Title.ToLower.StartsWith(Letter.ToLower) Then
                    cMovies.Page = Math.Ceiling(((Cuenta + 1) / cMovies.MoviesPerPage) - 1) : UpdateGallery() : CancelFlash()
                    For Each Movie As GalleryImage In Controls
                        If Movie.Indice = Cuenta Then FGallery.SelectMovie(Cuenta) : Movie.CancelFlash() : Movie.Flash() : Exit Sub
                    Next
                End If
            Next
        Else
            For Cuenta = 0 To cMovies.Movies_F.Count - 1
                If cMovies.Movies_F(Cuenta).Ruta = Letter Then
                    cMovies.Page = Math.Ceiling(((Cuenta + 1) / cMovies.MoviesPerPage) - 1) : UpdateGallery() : CancelFlash()
                    For Each Movie As GalleryImage In Controls
                        If Movie.Indice = Cuenta Then FGallery.SelectMovie(Cuenta) : Exit Sub
                    Next
                End If
            Next
        End If
    End Sub

    Public Sub CancelFlash()
        For Each Pic As GalleryImage In Controls
            Pic.CancelFlash()
        Next
    End Sub

#End Region

#Region " Select Movie "

    Public Sub SelectMovie(Ruta As String)
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        For Cuenta = 0 To cMovies.Movies_F.Count - 1
            If cMovies.Movies_F(Cuenta).Ruta = Ruta Then
                cMovies.Page = Math.Ceiling(((Cuenta + 1) / cMovies.MoviesPerPage) - 1) : UpdateGallery()
                FGallery.SelectMovie(Cuenta)
                Exit Sub
            End If
        Next
    End Sub

#End Region

#End Region

#Region " Galleries "

#Region " Gallery "

#Region " Create Gallery "

    Public Sub CreateGallery()
        On Error Resume Next
        Dim XLocation, YLocation As Integer : Dim i As Integer = 1
        Dim Column As Integer : Dim Row As Integer : Dim cMoviesPerPage As Integer
        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)

        Column = Zooms(cMovies.Zoom).Column : Row = Zooms(cMovies.Zoom).Row
        cMovies.MoviesPerPage = Zooms(cMovies.Zoom).Column * Zooms(cMovies.Zoom).Row

        PicWidth = (Width - ((nXLocation * 2) + (SepWidth * Column) + 25)) / Column
        PicHeight = ((Height - (nYLocation + (SepHeight * Row))) / Row) - 1
        XLocation = nXLocation : YLocation = nYLocation : i = 1
        Controls.Clear() : Visible = True : For Cuenta = 0 To cMovies.MoviesPerPage - 1
            Dim Pic1 As New GalleryImage : Pic1.Visible = False : Pic1.Name = "Movie" + i.ToString : i += 1
            Pic1.TabStop = False : Pic1.TabIndex = 0 : Pic1.Location = New Point(XLocation, YLocation)
            XLocation = XLocation + PicWidth + SepWidth : If XLocation + PicWidth >= CtrlWidth Then XLocation = nXLocation : YLocation = YLocation + PicHeight + SepHeight
            Pic1.Size = New Size(PicWidth, PicHeight) : Pic1.ContextMenuStrip = FGallery.MPMenu
            AddHandler Pic1.MouseEnter, AddressOf Pic1_MouseEnter : AddHandler Pic1.MouseLeave, AddressOf Pic1_MouseLeave : AddHandler Pic1.MouseClick, AddressOf Pic1_MouseClick : AddHandler Pic1.MouseMove, AddressOf Pic1_MouseMove : Controls.Add(Pic1)
        Next
    End Sub

#End Region

#Region " Update Gallery "

    Public Sub UpdateGallery()
        FGallery.Create_Folders()

        Dim cMovies As Movies_CL = FGallery.Get_cMovies(FGallery.Mode)
        If cMovies.Movies_F Is Nothing OrElse cMovies.Movies_F.Count = 0 Then Refresh() : RaiseEvent Updated() : Exit Sub

        cMovies.MoviesPerPage = Zooms(cMovies.Zoom).Column * Zooms(cMovies.Zoom).Row
        If Controls.Count <> cMovies.MoviesPerPage Then CreateGallery()

        If cMovies.Page > Math.Ceiling((cMovies.Movies_F.Count / cMovies.MoviesPerPage) - 1) Then cMovies.Page = Math.Ceiling((cMovies.Movies_F.Count / cMovies.MoviesPerPage) - 1)

        If cMovies.Page = -1 Then cMovies.Page = 0
        If cMovies.Movies_F.Count = 1 AndAlso cMovies.Movies_F(0).Title = "" Then
            For Cuenta = 0 To cMovies.MoviesPerPage - 1 : Controls(Cuenta).Visible = False : Next
        ElseIf cMovies.Movies_F.Count > (cMovies.Page * cMovies.MoviesPerPage) + cMovies.MoviesPerPage Then
            For Cuenta = 0 To cMovies.MoviesPerPage - 1
                Controls(Cuenta).Visible = True
            Next
            For Cuenta = 0 To cMovies.MoviesPerPage - 1
                UpdateControl(cMovies, (cMovies.Page * cMovies.MoviesPerPage) + Cuenta, Cuenta)
            Next
        Else
            Try : Dim MaxControls As Integer = (cMovies.Movies_F.Count - 1) - (cMovies.Page * cMovies.MoviesPerPage)
                If MaxControls > Controls.Count - 1 Then MaxControls = Controls.Count - 1
                For Cuenta = 0 To Controls.Count - 1 : Controls(Cuenta).Visible = False : Next
                If MaxControls <> -1 Then
                    For Cuenta = 0 To MaxControls : Controls(Cuenta).Visible = True : Next
                    For Cuenta = 0 To MaxControls : UpdateControl(cMovies, (cMovies.Page * cMovies.MoviesPerPage) + Cuenta, Cuenta) : Next
                End If
            Catch : End Try
        End If
        Refresh() : RaiseEvent Updated()
    End Sub

#End Region

#Region " Update Control "

    Private Sub UpdateControl(ByRef cMovies As Movies_CL, cIndice As Integer, ControlID As Integer)
        Dim Movie As GalleryImage = CType(Controls(ControlID), GalleryImage) : If Movie Is Nothing Or cMovies.Movies_F.Count = 0 Then Exit Sub

        Movie.Ruta = cMovies.Movies_F(cIndice).Ruta : Movie.Title = cMovies.Movies_F(cIndice).Title : Movie.Mode = FGallery.Mode : Movie.Indice = cIndice

        If cMovies.Movies_F(cIndice).NoCover = False Then
            Dim Movie_Path As String = cMovies.MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(cIndice).Ruta).Replace("\\", "\")
            Dim Movie_Path_Cache As String = cMovies.MoviePath + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(cIndice).Ruta).Replace("\\", "\")
            If IO.File.Exists(Movie_Path + ".jpg") = True And IO.File.Exists(cMovies.Movies_F(cIndice).CoverCachedPath) = False Then FGallery.CacheImage(FGallery.LoadImageClone(Movie_Path + ".jpg"), 190, 135).Save(cMovies.Movies_F(cIndice).CoverCachedPath + ".jpg")
            If IO.File.Exists(cMovies.Movies_F(cIndice).CoverCachedPath) = True Then cMovies.Movies_F(cIndice).CoverCached = FGallery.LoadImageClone(cMovies.Movies_F(cIndice).CoverCachedPath)
        Else
            cMovies.Movies_F(cIndice).CoverCached = FGallery.NoImage(cMovies.Movies_F(cIndice).Title)
        End If

        Movie.Image = cMovies.Movies_F(cIndice).CoverCached
    End Sub

#End Region

#Region " Update Cover "

    Public Sub UpdateCover(Ruta As String)
        Try : For Each Movie As GalleryImage In Controls
                If Movie.Ruta = Ruta Then Movie.Image = FGallery.Get_cMovies(FGallery.Mode).Movies_F(Movie.Indice).CoverCached : Movie.Refresh() : Exit Sub
            Next
        Catch : End Try
    End Sub

#End Region

#End Region

#Region " Edit Gallery "

    Public Sub EditHomePage(MovieList() As IMDB_Worker.ST_MovieList)
        Edit_Page = 0 : UpdateEditGallery(MovieList)
    End Sub

    Public Sub EditEndPage(MovieList() As IMDB_Worker.ST_MovieList)
        Dim EditMoviesPerPage As Integer = 4
        Edit_Page = Math.Ceiling((MovieList.Count / EditMoviesPerPage) - 1) : UpdateEditGallery(MovieList) : Exit Sub
    End Sub

    Public Sub EditNextPage(MovieList() As IMDB_Worker.ST_MovieList)
        Dim EditMoviesPerPage As Integer = 4
        If (Edit_Page + 1) * EditMoviesPerPage < MovieList.Count Then Edit_Page += 1 : UpdateEditGallery(MovieList)
    End Sub

    Public Sub EditPreviousPage(MovieList() As IMDB_Worker.ST_MovieList)
        If Edit_Page > 0 Then Edit_Page -= 1 : UpdateEditGallery(MovieList)
    End Sub

    Public Sub CreateEditGallery(MovieList() As IMDB_Worker.ST_MovieList)
        Dim XLocation, YLocation As Integer : Dim i As Integer = 1
        Dim EditMoviesPerPage As Integer = 4

        Dim PicWidth As Integer = (Width - ((nXLocation * 2) + (SepWidth * 2) + 25)) / 2
        Dim PicHeight As Integer = (Height - (nYLocation + (SepHeight * 2))) / 2
        XLocation = nXLocation : YLocation = nYLocation : i = 1
        Controls.Clear() : Visible = True : For Cuenta = 0 To EditMoviesPerPage - 1
            If Cuenta = MovieList.Count Then Exit For
            If MovieList(Cuenta).Title <> "" And MovieList(Cuenta).Type = "movie" And MovieList(Cuenta).CoverURL <> "" Then
                Dim Pic1 As New GalleryImage : Pic1.Visible = True : Pic1.Name = "Edit" + i.ToString : i += 1
                Pic1.Indice = Cuenta : Pic1.Title = MovieList(Cuenta).Title : Pic1.EditYear = MovieList(Cuenta).Year : Pic1.EditImdb = MovieList(Cuenta).Imdb : Pic1.Image = MovieList(Cuenta).Cover
                Pic1.TabStop = False : Pic1.TabIndex = 0 : Pic1.Location = New Point(XLocation, YLocation)
                XLocation = XLocation + PicWidth + SepWidth : If XLocation + PicWidth >= CtrlWidth Then XLocation = nXLocation : YLocation = YLocation + PicHeight + SepHeight
                Pic1.Size = New Size(PicWidth, PicHeight)
                AddHandler Pic1.MouseEnter, AddressOf Pic1_MouseEnter : AddHandler Pic1.MouseLeave, AddressOf Pic1_MouseLeave : AddHandler Pic1.MouseClick, AddressOf Pic1_MouseClick : AddHandler Pic1.MouseMove, AddressOf Pic1_MouseMove : Controls.Add(Pic1)
            End If
        Next : UpdateEditGallery(MovieList)
    End Sub

    Public Sub UpdateEditGallery(MovieList() As IMDB_Worker.ST_MovieList)
        Dim EditMoviesPerPage As Integer = 4
        If Edit_Page > Math.Ceiling((MovieList.Count / EditMoviesPerPage) - 1) Then Edit_Page = Math.Ceiling((MovieList.Count / EditMoviesPerPage) - 1)
        For Cuenta = 0 To EditMoviesPerPage - 1
            If Cuenta >= Controls.Count Then Exit For
            Controls(Cuenta).Visible = False
        Next
        If MovieList.Count = 1 And MovieList(0).Title = "" Then
            For Cuenta = 0 To EditMoviesPerPage - 1 : Controls(Cuenta).Visible = False : Next
        ElseIf MovieList.Count > (Edit_Page * EditMoviesPerPage) + EditMoviesPerPage Then
            For Cuenta = 0 To EditMoviesPerPage - 1
                UpdateEditControl(Cuenta, (Edit_Page * EditMoviesPerPage) + Cuenta, MovieList)
            Next
        Else
            Try : Dim MaxControls As Integer = (MovieList.Count - 1) - (Edit_Page * EditMoviesPerPage)
                If MaxControls > Controls.Count - 1 Then MaxControls = Controls.Count - 1
                For Cuenta = 0 To Controls.Count - 1 : Controls(Cuenta).Visible = False : Next
                For Cuenta = 0 To MaxControls : Controls(Cuenta).Visible = True : Next
                For Cuenta = 0 To MaxControls : UpdateEditControl(Cuenta, (Edit_Page * EditMoviesPerPage) + Cuenta, MovieList) : Next
            Catch : End Try
        End If
        Refresh() : RaiseEvent Updated()
    End Sub

    Private Sub UpdateEditControl(ControlID As Integer, MovieID As Integer, MovieList() As IMDB_Worker.ST_MovieList)
        Dim Movie As GalleryImage = CType(Controls(ControlID), GalleryImage)
        If Movie Is Nothing Then Exit Sub
        If MovieID >= MovieList.Count Then Exit Sub
        Controls(ControlID).Visible = True
        Movie.Indice = MovieID
        Movie.Title = MovieList(MovieID).Title
        Movie.EditImdb = MovieList(MovieID).Imdb
        Movie.EditYear = MovieList(MovieID).Year
        Movie.Image = MovieList(MovieID).Cover
    End Sub

#End Region

#End Region

End Class

#End Region

#Region " Gallery Image "

Class GalleryImage : Inherits Control

#Region " Variables "

    Public Ruta, Title, EditYear, EditImdb, Mode As String
    Public Indice As Integer
    Public Image As Image
    Dim TempColor As Color
    Dim Flashing As Boolean

#Region " Constructor "

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor, True) : DoubleBuffered = True
        Size = New Size(106, 32) : BackColor = Color.Black : Font = New Font("Segoe UI", 12)
    End Sub

#End Region

#End Region

#Region " Flash "

    Public Sub Flash(Optional Number As Integer = 6)
        If Flashing = True Then Exit Sub Else Flashing = True
        TempColor = BackColor
        For Cuenta = 1 To Number
            If Flashing = False Then Exit Sub
            BackColor = Color.SlateGray : Refresh() : Application.DoEvents() : Threading.Thread.Sleep(100)
            If Flashing = False Then Exit Sub
            BackColor = TempColor : Refresh() : Application.DoEvents() : Threading.Thread.Sleep(100)
        Next : Flashing = False
    End Sub

    Public Sub CancelFlash()
        If Flashing = False Then Exit Sub
        Flashing = False
        BackColor = TempColor : Refresh() : Application.DoEvents()
    End Sub

#End Region

#Region " Paint "

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If Image Is Nothing Then Exit Sub
        e.Graphics.SmoothingMode = 2 : e.Graphics.PixelOffsetMode = 2 : e.Graphics.TextRenderingHint = 5 : e.Graphics.InterpolationMode = 7 : e.Graphics.Clear(BackColor)
        e.Graphics.DrawImage(Image, New Rectangle(1, 1, Width - 2, Height - 2), New Rectangle(0, 0, Image.Width, Image.Height), GraphicsUnit.Pixel)

        If Name.StartsWith("E") = True Or FGallery.Mode <> "Movies" Then Exit Sub
        Dim cIndice As Integer = -1
        For Cuenta = 0 To FGallery.Movies.Movies.Count - 1
            If Cuenta = FGallery.Movies.Movies.Count Then Exit Sub
            If FGallery.Movies.Movies(Cuenta).Ruta = Ruta Then cIndice = Cuenta : Exit For
        Next : If cIndice = -1 Then Exit Sub

        If FGallery.Movies.Movies(cIndice).Copying = True Then
            Dim Percent As Integer = (FGallery.Movies.Movies(cIndice).CopySize * (Width - 2)) / FGallery.Movies.Movies(cIndice).Size
            Dim PColor As SolidBrush
            If FGallery.Movies.Movies(cIndice).Importing = True Then PColor = New SolidBrush(Color.Green) Else PColor = New SolidBrush(Color.Purple)
            e.Graphics.FillRectangle(PColor, New Rectangle(1, Height - 6, Percent, 5))
        End If
    End Sub

#End Region

End Class

#End Region