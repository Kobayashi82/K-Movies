
#Region " Edit Info "

Public Class FEditInfo

#Region " Variables "

    Public MovieList() As IMDB_Worker.ST_MovieList
    Public OriginalMovie, Movie, ST_Movie() As Movies_CL.ST_Movie
    Public OriginalCover, Cover As Image
    Public UpdateFilters, UpdateCover, NoCover, NewMovie, Repite As Boolean
    Public Cancelado As Boolean = True

#End Region

#Region " Eventos del Formulario "

#Region " Load "

    Private Sub FEditInfo_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True) : SetStyle(ControlStyles.UserPaint, True)
        If NewMovie = False Then
            If My.Computer.Network.IsAvailable = False Then LGallery.Text = "NO CONNECTION" Else LGallery.Text = "SEARCHING..."
            LGallery.Visible = True
        Else
            If Repite = True Then
                Repite = False
            Else
                OriginalMovie.Title = ""
                OriginalMovie.Actor = ""
                OriginalMovie.Director = ""
                OriginalMovie.Genre = ""
                OriginalMovie.Country = ""
                OriginalMovie.Plot = ""
                OriginalMovie.Language = ""
                OriginalMovie.ImdbID = ""
                OriginalMovie.Rating = ""
                OriginalMovie.Year = ""
                OriginalMovie.Ruta = ""
                OriginalMovie.Size = 0
                OriginalMovie.Added = Now
                OriginalMovie.Duration = 0
                OriginalMovie.Subtitle = ""
            End If
        End If
    End Sub

    Private Sub FEditInfo_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If NewMovie = True Then TSTitle.SelectAll() : TSTitle.Focus()
        Refresh() : Application.DoEvents() : Opacity = 100
        If NewMovie = False AndAlso My.Computer.Network.IsAvailable = True Then GetMovieList()
    End Sub

#End Region

#Region " Others "

#Region " Move "

    Private Sub LTitle_MouseMove(sender As Object, e As MouseEventArgs) Handles LTitle.MouseMove
        If e.Button = MouseButtons.Left Then FGallery.ReleaseCapture() : FGallery.SendMessage(Handle, &H112&, &HF012&, 0)
    End Sub

#End Region

#Region " Keypress "

    Private Sub FEditInfo_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        If Asc(e.KeyChar) = 13 Then
            If TYear.IsFocused Or TRating.IsFocused Or TDuration.IsFocused Or TImdb.IsFocused Or TGenre.IsFocused Or TLanguage.IsFocused Or TCountry.IsFocused Or TDirector.IsFocused Or TActor.IsFocused Or TSTitle.IsFocused Or TSImdb.IsFocused Or TPlot.IsFocused Then
                e.Handled = True
            Else
                e.Handled = True
                ': BAccept_Click(sender, Nothing)
            End If
        End If
        If Asc(e.KeyChar) = 27 Then
            If TYear.IsFocused Or TRating.IsFocused Or TDuration.IsFocused Or TImdb.IsFocused Or TGenre.IsFocused Or TLanguage.IsFocused Or TCountry.IsFocused Or TDirector.IsFocused Or TActor.IsFocused Or TSTitle.IsFocused Or TSImdb.IsFocused Or TPlot.IsFocused Then e.Handled = True : THidden.Focus() Else Close()
        End If
    End Sub

#End Region

#Region " ScrollBar "

    Private Sub Gallery_MouseScroll(sender As Object, e As MouseEventArgs) Handles Gallery.MouseScroll
        If e.Delta > 0 Then Gallery.EditPreviousPage(MovieList) Else Gallery.EditNextPage(MovieList)
    End Sub

#End Region

#End Region

#Region " Buttons "

#Region " Accept "

    Private Sub BAccept_Click(sender As Object, e As MouseEventArgs) Handles BAccept.Click
        If e IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Exit Sub
        If TYear.Text <> "" Then
            If CInt(TYear.Text) < 1900 Or CInt(TYear.Text) > Now.Year Then
                MsgBox("Year must be between 1900 and " + Now.Year.ToString, MsgBoxStyle.Information, "K-Movies " + FGallery.Version)
                TYear.SelectAll()
                TYear.Focus()
                Exit Sub
            End If
        End If
        If TImdb.Text <> "" Then
            If TImdb.Text.StartsWith("tt") = False Or IsNumeric(TImdb.Text.Substring(2)) = False Or (TImdb.Text.Length <> 9 And TImdb.Text.Length <> 10) Then
                MsgBox("Imdb ID is not valid", MsgBoxStyle.Information, "K-Movies " + FGallery.Version)
                TImdb.SelectAll()
                TImdb.Focus()
                Exit Sub
            End If
        End If
        If NewMovie = True Then
            If OriginalMovie.Title.Trim = "" And TSTitle.Text.Trim = "" Then MsgBox("You need to enter a title for the movie", MsgBoxStyle.Information, "K-Movies " + FGallery.Version) : TSTitle.Focus() : Exit Sub
            If OriginalMovie.Title.Trim = "" Then OriginalMovie.Title = TSTitle.Text.Trim
            OriginalMovie.Rating = TRating.Text
            OriginalMovie.Duration = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", ""))
            OriginalMovie.Year = TYear.Text
            OriginalMovie.ImdbID = TImdb.Text
            OriginalMovie.Rating = TRating.Text
            OriginalMovie.Genre = TGenre.Text
            OriginalMovie.Language = TLanguage.Text
            OriginalMovie.Country = TCountry.Text
            OriginalMovie.Director = TDirector.Text
            OriginalMovie.Actor = TActor.Text
            OriginalMovie.Plot = TPlot.Text
        Else
            If OriginalMovie.Rating Is Nothing Then OriginalMovie.Rating = ""
            If PCover.Image IsNot OriginalCover Then UpdateCover = True
            If OriginalMovie.Year <> TYear.Text Then UpdateFilters = True
            If OriginalMovie.Rating = "" Then OriginalMovie.Rating = TRating.Text
            If OriginalMovie.Rating.Replace("N/A", "") <> TRating.Text Then UpdateFilters = True
            If OriginalMovie.Duration <> Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")) Then UpdateFilters = True
            If OriginalMovie.ImdbID.Replace("N/A", "") <> TImdb.Text Then UpdateFilters = True
            If OriginalMovie.Plot.Replace("N/A", "") <> TPlot.Text Then UpdateFilters = True

            Dim DataArray1(), DataArray2() As String
            DataArray1 = OriginalMovie.Genre.Split(",") : DataArray2 = TGenre.Text.Split(",")
            For Cuenta1 = 0 To DataArray1.Length - 1
                For Cuenta2 = 0 To DataArray2.Length - 1
                    If Trim(DataArray1(Cuenta1).Replace("N/A", "")) = Trim(DataArray2(Cuenta2)) Then
                        GoTo Sigue1
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue1:     Next
            For Cuenta2 = 0 To DataArray2.Length - 1
                For Cuenta1 = 0 To DataArray1.Length - 1
                    If Trim(DataArray2(Cuenta2)) = Trim(DataArray1(Cuenta1).Replace("N/A", "")) Then
                        GoTo Sigue1b
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue1b:    Next
            DataArray1 = OriginalMovie.Language.Split(",") : DataArray2 = TLanguage.Text.Split(",")
            For Cuenta1 = 0 To DataArray1.Length - 1
                For Cuenta2 = 0 To DataArray2.Length - 1
                    If Trim(DataArray1(Cuenta1).Replace("N/A", "")) = Trim(DataArray2(Cuenta2)) Then
                        GoTo Sigue2
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue2:     Next
            For Cuenta2 = 0 To DataArray2.Length - 1
                For Cuenta1 = 0 To DataArray1.Length - 1
                    If Trim(DataArray2(Cuenta2)) = Trim(DataArray1(Cuenta1).Replace("N/A", "")) Then
                        GoTo Sigue2b
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue2b:    Next
            DataArray1 = OriginalMovie.Country.Split(",") : DataArray2 = TCountry.Text.Split(",")
            For Cuenta1 = 0 To DataArray1.Length - 1
                For Cuenta2 = 0 To DataArray2.Length - 1
                    If Trim(DataArray1(Cuenta1).Replace("N/A", "")) = Trim(DataArray2(Cuenta2)) Then
                        GoTo Sigue3
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue3:     Next
            For Cuenta2 = 0 To DataArray2.Length - 1
                For Cuenta1 = 0 To DataArray1.Length - 1
                    If Trim(DataArray2(Cuenta2)) = Trim(DataArray1(Cuenta1).Replace("N/A", "")) Then
                        GoTo Sigue3b
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue3b:    Next
            DataArray1 = OriginalMovie.Director.Split(",") : DataArray2 = TDirector.Text.Split(",")
            For Cuenta1 = 0 To DataArray1.Length - 1
                For Cuenta2 = 0 To DataArray2.Length - 1
                    If Trim(DataArray1(Cuenta1).Replace("N/A", "")) = Trim(DataArray2(Cuenta2)) Then
                        GoTo Sigue4
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue4:     Next
            For Cuenta2 = 0 To DataArray2.Length - 1
                For Cuenta1 = 0 To DataArray1.Length - 1
                    If Trim(DataArray2(Cuenta2)) = Trim(DataArray1(Cuenta1).Replace("N/A", "")) Then
                        GoTo Sigue4b
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue4b:    Next
            DataArray1 = OriginalMovie.Actor.Split(",") : DataArray2 = TActor.Text.Split(",")
            For Cuenta1 = 0 To DataArray1.Length - 1
                For Cuenta2 = 0 To DataArray2.Length - 1
                    If Trim(DataArray1(Cuenta1).Replace("N/A", "")) = Trim(DataArray2(Cuenta2)) Then
                        GoTo Sigue5
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue5:     Next
            For Cuenta2 = 0 To DataArray2.Length - 1
                For Cuenta1 = 0 To DataArray1.Length - 1
                    If Trim(DataArray2(Cuenta2)) = Trim(DataArray1(Cuenta1).Replace("N/A", "")) Then
                        GoTo Sigue5b
                    End If
                Next : UpdateFilters = True : GoTo Fin
Sigue5b:    Next
        End If
Fin:    Cancelado = False
        Close()
    End Sub

#End Region

#Region " Cancel / Close "

    Private Sub BClose_Click(sender As Object, e As MouseEventArgs) Handles BClose.Click, BCancel.Click
        If e.Button = MouseButtons.Left Then Close()
    End Sub

    Private Sub BClose_MouseEnter(sender As Object, e As EventArgs) Handles BClose.MouseEnter
        Dim CButton As FlatLabel = CType(sender, FlatLabel)
        CButton.ForeColor = Color.DarkGreen
    End Sub

    Private Sub BClose_MouseLeave(sender As Object, e As EventArgs) Handles BClose.MouseLeave
        Dim CButton As FlatLabel = CType(sender, FlatLabel)
        CButton.ForeColor = Color.SlateGray
    End Sub

#End Region

#Region " Trailer "

    Private Sub BTrailer_Click(sender As Object, e As MouseEventArgs) Handles BTrailer.Click
        If e.Button <> MouseButtons.Left And e.Button <> MouseButtons.Right Then Exit Sub
        Dim Youtube As New YouTube
        Dim Result As JsVideo = YouTube.Search(LTitle.Text + " Trailer")
        If e.Button = MouseButtons.Left Then
            If Result IsNot Nothing Then
                Dim Trailer As New FTrailer
                Trailer.VideoId = Result.Items(0).ID.VideoID
                Trailer.Text = LTitle.Text : Trailer.LTitle.Text = Trailer.Text
                Trailer.Show()
            End If
        ElseIf e.Button = MouseButtons.Right Then
            If Result IsNot Nothing Then Process.Start(("https://www.youtube.com/v/" + Result.Items(0).ID.VideoID).Replace("v/", "watch?v="))
        End If
    End Sub

#End Region

#Region " Imdb Web "

    Private Sub BImdbWeb_Click(sender As Object, e As MouseEventArgs) Handles BImdbWeb.Click
        If e.Button = MouseButtons.Left Then
            If TImdb.Text <> "" Then
                Process.Start("http://www.imdb.com/title/" + TImdb.Text + "/?ref_=rvi_tt")
            Else
                If LTitle.Text.Trim <> "" Then Process.Start("https://www.imdb.com/find/?q=" + LTitle.Text.Replace(" ", "%20") + "&ref_=nv_sr_sm")
            End If
        End If
    End Sub

#End Region

#End Region

#Region " TControls "

    Private Sub LTitle_Click(sender As Object, e As MouseEventArgs) Handles LTitle.Click, LTitle.DoubleClick
        If NewMovie = True Then TSTitle.Text = LTitle.Text
    End Sub

    Private Sub LTitle_TextChanged(sender As Object, e As EventArgs) Handles LTitle.TextChanged
        FGallery.SetFontSize(LTitle, 25, 5)
    End Sub

    Private Sub TSTitle_TextChanged(sender As Object, e As EventArgs) Handles TSTitle.TextChanged
        'If NewMovie = True AndAlso OriginalMovie.Title.Trim = "" Then LTitle.Text = TSTitle.Text : TSTitle.Refresh()
        If NewMovie = True Then LTitle.Text = TSTitle.Text : TSTitle.Refresh()
    End Sub

    Private Sub TControls_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TYear.KeyPress, TRating.KeyPress, TDuration.KeyPress, TImdb.KeyPress, TGenre.KeyPress, TLanguage.KeyPress, TCountry.KeyPress, TDirector.KeyPress, TActor.KeyPress, TSTitle.KeyPress, TSImdb.KeyPress, TPlot.KeyPress
        Dim TFilter As NSTextBox = CType(sender, NSTextBox)
        Select Case TFilter.Name
            Case "TYear"
                If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TYear.Text.Length = 0 And e.KeyChar <> "1" And e.KeyChar <> "2" Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TYear.SelectionStart = 0 And TYear.SelectedText.Length = TYear.Text.Length And e.KeyChar <> "1" And e.KeyChar <> "2" AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
            Case "TRating"
                If TRating.Text.Length = 1 And TRating.Text.Substring(0, 1) = "1" And e.KeyChar = "0" Then Exit Sub
                If TRating.Text = "10" And TRating.SelectedText.Length = 0 AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.Text.Length = 2 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length > 0 And TRating.SelectedText = TRating.Text.Substring(1) And e.KeyChar = "." Or e.KeyChar = "0" Then Exit Sub
                If TRating.Text.Length = 3 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length > 0 And TRating.SelectedText = TRating.Text.Substring(1) And e.KeyChar = "." Or e.KeyChar = "0" Then Exit Sub
                If TRating.Text.Length = TRating.SelectedText.Length And e.KeyChar <> "." Then Exit Sub

                If TRating.Text.Length = 2 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length = 0 AndAlso Not Char.IsControl(e.KeyChar) And TRating.Text.Contains(".") = False Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.SelectionStart = 1 And TRating.SelectedText.Length > 0 And e.KeyChar <> "." AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.SelectedText.Length = TRating.Text.Length And e.KeyChar = "." Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.Text = "" And e.KeyChar = "." Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.Text.Contains(".") And e.KeyChar = "." And TRating.SelectedText.Contains(".") = False Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If TRating.Text.Length = 1 And e.KeyChar <> "." AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
                If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> "." Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
            Case "TDuration"
                If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
                    e.KeyChar = ""
                    e.Handled = True
                End If
            Case "TGenre"

            Case "TLanguage"

            Case "TCountry"

            Case "TDirector"

            Case "TActor"

            Case "TSTitle"
                If TSTitle.Text = "" Or TSTitle.SelectedText = TSTitle.Text Then e.KeyChar = e.KeyChar.ToString.ToUpper
        End Select
    End Sub

    Private Sub TControls_KeyDown(sender As Object, e As KeyEventArgs) Handles TYear.KeyDown, TRating.KeyDown, TDuration.KeyDown, TImdb.KeyDown, TGenre.KeyDown, TLanguage.KeyDown, TCountry.KeyDown, TDirector.KeyDown, TActor.KeyDown, TSTitle.KeyDown, TSImdb.KeyDown, TPlot.KeyDown
        Dim TFilter As NSTextBox = CType(sender, NSTextBox)
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            Select Case TFilter.Name
                Case "TYear"
                    TRating.Focus()
                Case "TRating"
                    TDuration.Focus()
                Case "TDuration"
                    TImdb.Focus()
                Case "TImdb"
                    TGenre.Focus()
                Case "TGenre"
                    TLanguage.Focus()
                Case "TLanguage"
                    TCountry.Focus()
                Case "TCountry"
                    TDirector.Focus()
                Case "TDirector"
                    TActor.Focus()
                Case "TActor"
                    TPlot.Focus()
                Case "TPlot"
                    THidden.Focus()
                Case "TSTitle"
                    If TSTitle.Text.Trim <> "" And My.Computer.Network.IsAvailable = True Then GetMovieList() : THidden.Focus()
                    If TSTitle.Text.Trim = "" Then TSTitle.Text = "" : Gallery.Controls.Clear() : LGallery.Visible = False
                    If NewMovie = True AndAlso TSTitle.Text.Trim = "" Then
                        TSTitle.Text = ""
                        OriginalMovie.Title = ""
                        OriginalMovie.Actor = ""
                        OriginalMovie.Director = ""
                        OriginalMovie.Genre = ""
                        OriginalMovie.Country = ""
                        OriginalMovie.Plot = ""
                        OriginalMovie.Language = ""
                        OriginalMovie.ImdbID = ""
                        OriginalMovie.Rating = ""
                        OriginalMovie.Year = ""
                        OriginalMovie.Ruta = ""
                        OriginalMovie.Size = 0
                        OriginalMovie.Added = Now
                        OriginalMovie.Duration = 0
                        OriginalMovie.Subtitle = ""

                        LTitle.Text = ""
                        TActor.Text = ""
                        TDirector.Text = ""
                        TGenre.Text = ""
                        TCountry.Text = ""
                        TPlot.Text = ""
                        TLanguage.Text = ""
                        TImdb.Text = ""
                        TRating.Text = "0.0"
                        TDuration.Text = "0 min"
                        TYear.Text = ""
                        PCover.Image = My.Resources.NoCover
                    End If
                Case "TSImdb"
                    If TSImdb.Text <> "" Then
                        If TSImdb.Text.StartsWith("tt") = False Or IsNumeric(TSImdb.Text.Substring(2)) = False Or (TSImdb.Text.Length <> 9 And TSImdb.Text.Length <> 10) Then
                            MsgBox("Imdb ID is not valid", MsgBoxStyle.Information, "K-Movies " + FGallery.Version)
                            TSImdb.SelectAll() : TSImdb.Focus() : Exit Sub
                        End If
                    Else
                        If TSTitle.Text.Trim <> "" And My.Computer.Network.IsAvailable = True Then GetMovieList()
                        If TSTitle.Text.Trim = "" Then TSTitle.Text = "" : Gallery.Controls.Clear() : LGallery.Visible = False : Exit Sub
                        THidden.Focus() : Exit Sub
                    End If
                    If My.Computer.Network.IsAvailable = True Then GetMovie(TSImdb.Text, True)
                    THidden.Focus()
            End Select
        End If
    End Sub

    Private Sub TControls_GotFocus(sender As Object, e As EventArgs) Handles TYear.GotFocus, TRating.GotFocus, TDuration.GotFocus, TImdb.GotFocus, TGenre.GotFocus, TLanguage.GotFocus, TCountry.GotFocus, TDirector.GotFocus, TActor.GotFocus, TSTitle.GotFocus, TSImdb.GotFocus, TPlot.GotFocus
        Dim TFilter As NSTextBox = CType(sender, NSTextBox)
        If TFilter.Name = "TDuration" Then TDuration.Text = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString
    End Sub

    Private Sub TControls_LostFocus(sender As Object, e As EventArgs) Handles TYear.LostFocus, TRating.LostFocus, TDuration.LostFocus, TImdb.LostFocus, TGenre.LostFocus, TLanguage.LostFocus, TCountry.LostFocus, TDirector.LostFocus, TActor.LostFocus, TSTitle.LostFocus, TSImdb.LostFocus, TPlot.LostFocus
        Dim TFilter As NSTextBox = CType(sender, NSTextBox)
        Select Case TFilter.Name
            Case "TDuration"
                If TDuration.Text = "" Then TDuration.Text = "0"
                If TDuration.Text.Length > 0 Then TDuration.Text = TDuration.Text + " min" Else TDuration.Text = "0 min"
            Case "TRating"
                If Trim(TRating.Text) = "" Then TRating.Text = "0.0"
                If Trim(TRating.Text).Length = 1 And Trim(TRating.Text).Contains(".") = False Then TRating.Text = Trim(TRating.Text) + ".0"
                If Trim(TRating.Text).Length = 2 And Trim(TRating.Text).Contains(".") = True Then TRating.Text = Trim(TRating.Text) + "0"
                If Trim(TRating.Text).StartsWith("0") = True And Trim(TRating.Text).Substring(1, 1) <> "." Then TRating.Text = Trim(TRating.Text).Substring(1)
                Dim IntRating As Integer : Int32.TryParse(TRating.Text, IntRating) : If IntRating > 10 Then TRating.Text = "10"
            Case "TGenre"
                'TGenre.Text = TGenre.Text.ToLower.Replace("comedia", "comedy").Replace("accion", "action").Replace("acción", "action").Replace("terror", "horror").Replace("aventura", "adventure").Replace("crimen", "crime").Replace("fantasia", "fantasy").Replace("historica", "historical").Replace("ficcion historica", "historical fiction").Replace("ficción historica", "historical fiction").Replace("misterio", "mistery").Replace("filosofica", "philosophical").Replace("politica", "political").Replace("satira", "satire").Replace("ciencia ficcion", "science fiction").Replace("ciencia ficción", "science fiction").Replace("oeste", "western").Replace("documental", "documentary").Replace("corto", "short").Replace("animacion", "animation").Replace("animación", "animation").Replace("artes marciales", "martial arts").Replace("deporte", "sport").Replace("familiar", "family").Replace("misterio", "mistery").Replace("historia", "history").Replace("guerra", "war")
                Dim DataArray() As String = TGenre.Text.Split(",") : TGenre.Text = ""
                For Cuenta = 0 To DataArray.Length - 1
                    If Trim(DataArray(Cuenta)) <> "" Then TGenre.Text += ", " + Trim(DataArray(Cuenta)).Substring(0, 1).ToUpper + Trim(DataArray(Cuenta)).Substring(1)
                Next : If TGenre.Text.Length > 2 Then TGenre.Text = TGenre.Text.Substring(2)
            Case "TLanguage"
                'TLanguage.Text = TLanguage.Text.ToLower.Replace("ingles", "english").Replace("inglés", "english").Replace("americano", "american").Replace("español", "spanish").Replace("frances", "french").Replace("aleman", "german").Replace("alemán", "german").Replace("britanico", "british").Replace("irlandes", "irish").Replace("escoces", "scotish").Replace("ruso", "russian").Replace("sueco", "swedish").Replace("turco", "turkish").Replace("vietnamita", "vietnamese").Replace("noruego", "norwegian").Replace("persa", "persian").Replace("portgues", "portuguese").Replace("coerano", "korean").Replace("japones", "japanese").Replace("italiano", "italian").Replace("indonesio", "indonesian").Replace("hindu", "hindi").Replace("hindú", "hindi").Replace("hungaro", "hungarian").Replace("holandes", "dutch").Replace("holandés", "dutch").Replace("croata", "croatian").Replace("chino", "chinese").Replace("cantones", "cantonese").Replace("arabe", "arabic").Replace("árabe", "arabic").Replace("belga", "belgian").Replace("rumano", "romanian")
                Dim DataArray() As String = TLanguage.Text.Split(",") : TLanguage.Text = ""
                For Cuenta = 0 To DataArray.Length - 1
                    If Trim(DataArray(Cuenta)) <> "" Then TLanguage.Text += ", " + Trim(DataArray(Cuenta)).Substring(0, 1).ToUpper + Trim(DataArray(Cuenta)).Substring(1)
                Next : If TLanguage.Text.Length > 2 Then TLanguage.Text = TLanguage.Text.Substring(2)
            Case "TCountry"
                'TCountry.Text = TCountry.Text.ToLower.Replace("americano", "american").Replace("belgica", "belgium").Replace("republica checa", "czech republic").Replace("república checa", "czech republic").Replace("dinamarca", "demmark").Replace("finlandia", "finland").Replace("francia", "france").Replace("alemania", "germany").Replace("hungria", "hungary").Replace("irlanda", "ireland").Replace("italia", "italy").Replace("japon", "japan").Replace("paises bajos", "netherlands").Replace("nueva zelanda", "new zeland").Replace("noruega", "norway").Replace("rumania", "romania").Replace("rusia", "russia").Replace("singapur", "singapore").Replace("sur africa", "south africa").Replace("sur corea", "south korea").Replace("españa", "spain").Replace("suecia", "sweden").Replace("suiza", "switzerland").Replace("turquia", "turkey").Replace("reino unido", "uk").Replace("inglaterra", "england")
                Dim DataArray() As String = TCountry.Text.Split(",") : TCountry.Text = ""
                For Cuenta = 0 To DataArray.Length - 1
                    If Trim(DataArray(Cuenta)) <> "" Then TCountry.Text += ", " + Trim(DataArray(Cuenta)).Substring(0, 1).ToUpper + Trim(DataArray(Cuenta)).Substring(1)
                Next : If TCountry.Text.Length > 2 Then TCountry.Text = TCountry.Text.Substring(2)
            Case "TDirector"
                Dim DataArray() As String = TDirector.Text.Split(",") : TDirector.Text = ""
                For Cuenta = 0 To DataArray.Length - 1
                    If Trim(DataArray(Cuenta)) <> "" Then TDirector.Text += ", " + Trim(DataArray(Cuenta)).Substring(0, 1).ToUpper + Trim(DataArray(Cuenta)).Substring(1)
                Next : If TDirector.Text.Length > 2 Then TDirector.Text = TDirector.Text.Substring(2)
            Case "TActor"
                Dim DataArray() As String = TActor.Text.Split(",") : TActor.Text = ""
                For Cuenta = 0 To DataArray.Length - 1
                    If Trim(DataArray(Cuenta)) <> "" Then TActor.Text += ", " + Trim(DataArray(Cuenta)).Substring(0, 1).ToUpper + Trim(DataArray(Cuenta)).Substring(1)
                Next : If TActor.Text.Length > 2 Then TActor.Text = TActor.Text.Substring(2)
        End Select
    End Sub

    Private Sub PCover_Click(sender As Object, e As MouseEventArgs) Handles PCover.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Dim Archivo As New OpenFileDialog
        Archivo.CheckFileExists = True
        Archivo.CheckPathExists = True
        Archivo.DefaultExt = "*.jpg"
        Archivo.Filter = ("Image files (*.jpg)|*.jpg")
        Archivo.Title = "Image cover for '" + LTitle.Text + "'"
        Archivo.FileName = IO.Path.GetFileName(LTitle.Text)
        If Archivo.ShowDialog() = Windows.Forms.DialogResult.OK Then
            PCover.Image = FGallery.LoadImageClone(Archivo.FileName)
            NoCover = False
            LRemoveCover.Visible = True
        End If
        Archivo.Dispose()
    End Sub

    Private Sub LSTitle_DoubleClick(sender As Object, e As MouseEventArgs) Handles LSTitle.DoubleClick
        If e.Button = MouseButtons.Left Then TSTitle.Text = LTitle.Text : GetMovieList()
    End Sub

    Private Sub LRemoveCover_Click(sender As Object, e As MouseEventArgs) Handles LRemoveCover.Click
        If e.Button = MouseButtons.Left Then PCover.Image = My.Resources.NoCover : NoCover = True : LRemoveCover.Visible = False
    End Sub

#End Region

#End Region

#Region " Get Movie List "

    Private Sub GetMovieList()
        Try
            Dim IMDB_APIKeys As New List(Of String) From {"&apikey=fa718ef5", "&apikey=2b89a6c7", "&apikey=7d98db3a"}
            Dim OMDB_APIKey As String : Randomize() : OMDB_APIKey = IMDB_APIKeys((IMDB_APIKeys.Count - 1) * Rnd())
            LGallery.Text = "SEARCHING..." : LGallery.Refresh() : LGallery.Visible = True
            Dim url As String = "http://www.omdbapi.com/?s=" + TSTitle.Text + OMDB_APIKey
            Dim wc As New Net.WebClient() : wc.Encoding = System.Text.Encoding.UTF8 : Dim json = wc.DownloadString(url)
            If json.ToLower.Contains("movie not found") Then
                LGallery.Text = "NO MOVIES FOUND" : LGallery.Refresh() : LGallery.Visible = True
                Gallery.Controls.Clear()
                Exit Sub
            End If
            Dim DataArray1(), DataArray2() As String
            DataArray1 = json.Replace("},{", "|").Split("|")
            ReDim MovieList(DataArray1.Length - 1)
            For Cuenta1 = 0 To DataArray1.Length - 1
                Application.DoEvents()
                DataArray2 = DataArray1(Cuenta1).Replace("{""Search"":[{", "").Replace(""",""", "|").Split("|")
                MovieList(Cuenta1).Title = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(DataArray2(0).Substring(9), "")
                MovieList(Cuenta1).Year = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(DataArray2(1).Substring(7))
                MovieList(Cuenta1).Imdb = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(DataArray2(2).Substring(9))
                MovieList(Cuenta1).Type = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(DataArray2(3).Substring(7))
                MovieList(Cuenta1).CoverURL = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(DataArray2(4).Substring(9).Replace("""", ""))
                If MovieList(Cuenta1).CoverURL.Contains(".jpg") Then
                    IO.File.Delete((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                    Dim DownloadPoster As New Net.WebClient
                    DownloadPoster.DownloadFile(MovieList(Cuenta1).CoverURL, (IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                    IO.File.SetAttributes((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
                    MovieList(Cuenta1).Cover = FGallery.LoadImageClone((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                Else
                    MovieList(Cuenta1).CoverURL = ""
                    MovieList(Cuenta1).Cover = FGallery.NoImage(MovieList(Cuenta1).Title)
                End If
            Next
            For Cuenta = MovieList.Count - 1 To 0 Step -1
                If MovieList(Cuenta).Type <> "movie" Then RemoveMovie(MovieList, Cuenta)
            Next
            Gallery.CreateEditGallery(MovieList)
            If Gallery.Controls.Count > 0 Then
                LGallery.Visible = False
            Else
                LGallery.Text = "NO MOVIES FOUND" : LGallery.Refresh() : LGallery.Visible = True
            End If
        Catch ex As Exception
            If ex.Message.ToLower.Contains("omdbapi") Then LGallery.Text = "NO CONNECTION" : LGallery.Refresh() : LGallery.Visible = True Else LGallery.Text = "NO MOVIES FOUND"
            LGallery.Refresh() : LGallery.Visible = True
            Gallery.Controls.Clear()
        End Try
    End Sub

    Private Sub RemoveMovie(Of T)(ByRef Array_Name() As T, Index As Integer)
        Array.Copy(Array_Name, Index + 1, Array_Name, Index, UBound(Array_Name) - Index)
        ReDim Preserve Array_Name(UBound(Array_Name) - 1)
    End Sub

    Private Sub GetMovie(ImdbID As String, Optional SearchImdb As Boolean = False)
        Try
            Dim IMDB_APIKeys As New List(Of String) From {"&apikey=fa718ef5", "&apikey=2b89a6c7", "&apikey=7d98db3a"}
            Dim OMDB_APIKey As String : Randomize() : OMDB_APIKey = IMDB_APIKeys((IMDB_APIKeys.Count - 1) * Rnd())

            If SearchImdb = True Then LGallery.Text = "SEARCHING..." : LGallery.Refresh() : LGallery.Visible = True
            Dim url As String = "http://www.omdbapi.com/?i=" + ImdbID + OMDB_APIKey
            Dim wc As New Net.WebClient() : wc.Encoding = System.Text.Encoding.UTF8 : Dim json = wc.DownloadString(url)
            wc = New Net.WebClient() : wc.Encoding = System.Text.Encoding.UTF8 : json = wc.DownloadString(url) : Dim oJS As New Web.Script.Serialization.JavaScriptSerializer() : Dim obj As New ImdbEntity()
            obj = oJS.Deserialize(Of ImdbEntity)(json) : If obj.Response = "True" Then
                IO.File.Delete((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                Dim DownloadPoster As New Net.WebClient
                DownloadPoster.DownloadFile(obj.Poster, (IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                IO.File.SetAttributes((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
                If SearchImdb = True Then
                    ReDim MovieList(0)
                    MovieList(0).Title = obj.Title
                    MovieList(0).Year = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Year)
                    MovieList(0).Imdb = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.ImdbID)
                    MovieList(0).Type = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Type)
                    MovieList(0).CoverURL = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Poster)
                    MovieList(0).Cover = FGallery.LoadImageClone((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
                    Gallery.CreateEditGallery(MovieList)
                    If Gallery.Controls.Count > 0 Then LGallery.Visible = False Else LGallery.Text = "NO MOVIES FOUND" : LGallery.Refresh() : LGallery.Visible = True
                Else
                    If NewMovie = True Then OriginalMovie.Title = obj.Title : LTitle.Text = obj.Title
                    PCover.Image = FGallery.LoadImageClone((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\")) : NoCover = False : LRemoveCover.Visible = True
                    TYear.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Year)
                    If FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.ImdbRating) = "" Then TRating.Text = "0.0" Else TRating.Text = obj.ImdbRating
                    If FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Runtime) = "" Then TDuration.Text = "0 min" Else TDuration.Text = obj.Runtime
                    TImdb.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.ImdbID)
                    TGenre.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Genre.Replace("Music", "Musical").Replace("Musicalal", "Musical"))
                    TLanguage.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Language)
                    TCountry.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Country)
                    TDirector.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Director)
                    TActor.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Actors)
                    TPlot.Text = FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(obj.Plot)
                End If
                If NewMovie = True Then
                    OriginalMovie.Title = obj.Title
                    OriginalMovie.Actor = TActor.Text.Replace("N/A", "")
                    OriginalMovie.Director = TDirector.Text.Replace("N/A", "")
                    OriginalMovie.Genre = TGenre.Text.Replace("N/A", "")
                    OriginalMovie.Country = TCountry.Text.Replace("N/A", "")
                    OriginalMovie.Plot = TPlot.Text.Replace("N/A", "")
                    OriginalMovie.Language = TLanguage.Text.Replace("N/A", "")
                    OriginalMovie.ImdbID = TImdb.Text.Replace("N/A", "")
                    OriginalMovie.Rating = TRating.Text.Replace("N/A", "")
                    OriginalMovie.Year = TYear.Text.Replace("N/A", "")
                    OriginalMovie.Ruta = ""
                    OriginalMovie.Size = 0
                    OriginalMovie.Added = Now
                    OriginalMovie.Duration = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", ""))
                    OriginalMovie.Subtitle = ""
                    LTitle.Text = obj.Title
                End If
            Else
                LGallery.Text = "NO MOVIES FOUND" : LGallery.Refresh() : LGallery.Visible = True
            End If
        Catch ex As Exception
            If SearchImdb = True Then
                If ex.Message.ToLower.Contains("omdbapi") Then LGallery.Text = "NO CONNECTION" Else LGallery.Text = "NO MOVIES FOUND"
                LGallery.Refresh() : LGallery.Visible = True
                Gallery.Controls.Clear()
            End If
        End Try
    End Sub

    Private Sub Gallery_Movie_Click(CMovie As Object, e As MouseEventArgs) Handles Gallery.Movie_Click
        Dim Movie As GalleryImage = CType(CMovie, GalleryImage)
        Select Case e.Button
            Case MouseButtons.Left
                If FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(Movie.EditImdb) <> "" Then GetMovie(Movie.EditImdb)
            Case MouseButtons.Right
                If FGallery.Movies.IMDB.IMDB_Worker.CheckEmpty(Movie.EditImdb) <> "" Then Process.Start("http://www.imdb.com/title/" + Movie.EditImdb + "/?ref_=rvi_tt")
        End Select
    End Sub

#End Region

End Class

#End Region