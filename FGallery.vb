
#Region " K-Movies "

#Region " FGallery "

Public Class FGallery

#Region " Variables "

#Region " Declaraciones "

    Public Declare Function ReleaseCapture Lib "user32" () As Long
    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (hWnd As IntPtr, wMsg As Integer, wParam As Integer, lParam As Integer) As Long

#End Region


    Public Version As String = "4.0"
    Public Mode As String = "Movies"

    Public Player_Path As String
    Public Auto_Watched, Delete_Imported As Boolean

    Public General_Tooltip As New ToolTip With {.InitialDelay = 1000, .ReshowDelay = 250, .AutoPopDelay = 50000, .ShowAlways = True, .Tag = ""}
    Dim Youtube As New YouTube

    Dim VLC_MoviePlaying As String
    Dim VLC_TimePlayed As Integer
    Dim VLC_MinPlayTime As Integer = 20
    Dim VLC_Timer As New Timer With {.Interval = 2000, .Enabled = True}
    Dim Transfering_Timer As New Timer With {.Interval = 100, .Enabled = True}

    Dim CFilterTemp As String
    Dim isMinimize As Boolean

    Public Movies As New Movies_CL("Movies")
    Public Pending As New Movies_CL("Pending")
    Public Watched As New Movies_CL("Watched")

#End Region

#Region " Eventos del Formulario "

#Region " Load / Close "

#Region " Load "

    Private Sub FMenu_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Opacity = 0 : Application.DoEvents() : FSplash.Show() : Application.DoEvents() : FSplash.Refresh()
        CheckForIllegalCrossThreadCalls = False : SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True) : SetStyle(ControlStyles.UserPaint, True) : Text = "K-Movies " + Version
        Width = Screen.FromControl(Me).WorkingArea.Width + 1 : Height = Screen.FromControl(Me).WorkingArea.Size.Height + 1
        'Settings
        Settings_Load()
        'Prep. Layout
        PCover.Width = PCover.Height / 1.4 : PCover.Left = Width - (PCover.Width + 35) : Gallery.Width = Width - (PCover.Width + 35)
        LGallery.Width = Gallery.Width
        LTitle.Left = PCover.Left : LTitle.Width = PCover.Width
        LCountImdb.Left = (LGallery.Left + LGallery.Width) - (LCountImdb.Width + 35)
        LPlot.Parent = PCover : LPlot.Left = 0 : LPlot.Top = PCover.Height - LPlot.Height : LPlot.Width = PCover.Width
        'Menus
        CType(MAddTo.DropDown, ToolStripDropDownMenu).AutoSize = False
        CType(MAddTo.DropDown, ToolStripDropDownMenu).Size = New Size(80, 48)
        CType(MAddTo.DropDown, ToolStripDropDownMenu).ShowCheckMargin = False
        CType(MAddTo.DropDown, ToolStripDropDownMenu).ShowImageMargin = False

        If IO.File.Exists(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vlc", "vlcrc")) = True Then VLC_Server()
        AddHandler VLC_Timer.Tick, AddressOf VLC_Timer_Tick
        AddHandler Transfering_Timer.Tick, AddressOf Transfering_Tick
    End Sub

    Private Sub VLC_Server()
        Try : Dim configText As String = IO.File.ReadAllText(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vlc", "vlcrc"))
            configText = New Text.RegularExpressions.Regex("#?http-password=.*").Replace(configText, "http-password=2501")
            configText = New Text.RegularExpressions.Regex("#?extraintf=.*").Replace(configText, "extraintf=http")
            IO.File.WriteAllText(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vlc", "vlcrc"), configText)
        Catch : End Try
    End Sub

    Private Sub FGallery_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Movies.Load_Movies() : Pending.Load_Movies() : Watched.Load_Movies()
        Application.DoEvents() : Refresh() : Opacity = 1 : Activate() : FSplash.Close() : Check_Auto_Watched() : Check_Auto_Pending()
    End Sub

    Private Sub FGallery_SizeChanged(sender As Object, e As EventArgs) Handles Me.Move
        If Opacity = 0 Then Exit Sub
        If WindowState = FormWindowState.Minimized Then isMinimize = True : Exit Sub
        If isMinimize = True Then isMinimize = False : Exit Sub
        Width = Screen.FromControl(Me).WorkingArea.Size.Width + 1 : Height = Screen.FromControl(Me).WorkingArea.Size.Height + 1
        'Prep. Layout
        PCover.Width = PCover.Height / 1.4 : PCover.Left = Width - (PCover.Width + 35) : Gallery.Width = Width - (PCover.Width + 35)
        LGallery.Width = Gallery.Width
        LTitle.Left = PCover.Left : LTitle.Width = PCover.Width
        LCountImdb.Left = (LGallery.Left + LGallery.Width) - (LCountImdb.Width + 35)
        LPlot.Parent = PCover : LPlot.Left = 0 : LPlot.Top = PCover.Height - LPlot.Height : LPlot.Width = PCover.Width
        Gallery.CalcZooms() : Gallery.CreateGallery() : Gallery.UpdateGallery() : Refresh()
    End Sub

#End Region

#Region " Close "

    Private Sub FGallery_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        For Cuenta = 0 To Movies.Movies.Count - 1
            If Movies.Movies(Cuenta).Copying = True Then
                If MsgBox("There is one or more movies in transfer" + vbCrLf + vbCrLf + "Do you want to close K-Movies?", MsgBoxStyle.OkCancel, "K-Movies " + Version) = MsgBoxResult.Cancel Then
                    e.Cancel = True : Exit Sub
                Else
                    Exit For
                End If
            End If
        Next
        Opacity = 0 : ShowInTaskbar = False
        For Cuenta = 0 To Movies.Movies.Count - 1
            If Movies.Movies(Cuenta).Copying = True Then Movies.Movies(Cuenta).Copying = False
        Next
        Do Until LTransfer.Visible = False : Application.DoEvents() : Loop
    End Sub

    Private Sub FGallery_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If IO.File.Exists((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Application.ExecutablePath) + "\temp.jpg").Replace("\\", "\"))
    End Sub

    Private Sub Minimize_Close_Click(sender As Object, e As MouseEventArgs) Handles BMinimize.Click, BClose.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Select Case sender.Name
            Case "BClose" : Close()
            Case "BMinimize" : WindowState = FormWindowState.Minimized
        End Select
    End Sub

    Private Sub Buttons_MouseEnter(sender As Object, e As EventArgs) Handles BMinimize.MouseEnter, BClose.MouseEnter, LRandom.MouseEnter
        sender.ForeColor = Color.DarkGreen
    End Sub

    Private Sub Buttons_MouseLeave(sender As Object, e As EventArgs) Handles BMinimize.MouseLeave, BClose.MouseLeave, LRandom.MouseLeave
        sender.ForeColor = Color.SlateGray
    End Sub

#End Region

#End Region

#Region " Settings "

    Private Sub Settings_Load()
        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + Version) : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + Version, True)
        If Reg.GetValue("Player_Path") Is Nothing Then Reg.SetValue("Player_Path", "")
        If Reg.GetValue("Auto_Watched") Is Nothing Then Reg.SetValue("Auto_Watched", "True")
        If Reg.GetValue("Auto_Watched_Min") Is Nothing Then Reg.SetValue("Auto_Watched_Min", "20")
        If Reg.GetValue("Delete_Imported") Is Nothing Then Reg.SetValue("Delete_Imported", "True")
        Player_Path = Reg.GetValue("Player_Path")
        Auto_Watched = Reg.GetValue("Auto_Watched")
        VLC_MinPlayTime = CInt(Reg.GetValue("Auto_Watched_Min"))
        Delete_Imported = Reg.GetValue("Delete_Imported")
        Reg.Close()
        If Movies.Filter.NOaz = True Then LAz.Text = "Za" Else LAz.Text = "Az"
        LAz.Refresh() : LCSort.Text = Movies.Filter.Sorted
    End Sub

#End Region

#Region " Autos Checks "

#Region " Auto Watch "

    Private Sub Check_Auto_Watched()
        If (Movies.Movies Is Nothing OrElse Movies.Movies.Count = 0) Or (Watched.Movies Is Nothing OrElse Watched.Movies.Count = 0) Then Exit Sub
        If Auto_Watched = True Then
Repite:     For Cuenta = 0 To Movies.Movies.Count - 1
                If Movies.Movies(Cuenta).NO_Auto_Watch = False Then
                    For Cuenta2 = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Title) = IO.Path.GetFileNameWithoutExtension(Watched.Movies(Cuenta2).Title) Then
                            If Movies.Movies(Cuenta).ImdbID <> "" AndAlso Watched.Movies(Cuenta2).ImdbID <> "" AndAlso Movies.Movies(Cuenta).ImdbID = Watched.Movies(Cuenta2).ImdbID Then
                                Auto_Watched_MSG(Cuenta, Cuenta2) : GoTo Repite
                            End If
                        End If
                    Next
                    For Cuenta2 = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Title) = IO.Path.GetFileNameWithoutExtension(Watched.Movies(Cuenta2).Title) Then Auto_Watched_MSG(Cuenta, Cuenta2) : GoTo Repite
                    Next
                End If
            Next
        End If
    End Sub

    Dim cAddWatched As New FAddWatched

    Private Sub Auto_Watched_MSG(cIndice As Integer, tIndice As Integer)
        If Movies.Movies(cIndice).NO_Auto_Watch = True Then Exit Sub
        'Dim cAddWatched As New FAddWatched
        If Movies.Movies(cIndice).NoCover = False Then
            cAddWatched.PCover.Image = LoadImageClone((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".jpg").Replace("\\", "\"))
        Else
            cAddWatched.PCover.Image = My.Resources.NoCover
        End If
        cAddWatched.LTitle.Text = Movies.Movies(cIndice).Title
        cAddWatched.ShowDialog()
        If cAddWatched.Cancelado = False Then
            If cAddWatched.Remove = False Then Movies.DeleteMovie(cIndice, False, True) Else Watched.DeleteMovie(tIndice, False, True)
        Else
            Movies.Movies(cIndice).NO_Auto_Watch = True
            For Cuenta = 0 To Movies.Movies_F.Count - 1
                If Movies.Movies_F(Cuenta).Ruta = Movies.Movies(cIndice).Ruta Then Movies.Movies_F(cIndice).NO_Auto_Watch = True : Exit For
            Next
            If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"))
            Dim Guardar As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"))
            Guardar.Write(Movies.Movies(cIndice).ImdbID + "|" + Movies.Movies(cIndice).Rating + "|" + Movies.Movies(cIndice).Year + "|" + Movies.Movies(cIndice).Duration.ToString + "|" + Movies.Movies(cIndice).Language + "|" + Movies.Movies(cIndice).Country + "|" + Movies.Movies(cIndice).Genre + "|" + Movies.Movies(cIndice).Director + "|" + Movies.Movies(cIndice).Actor + "|" + Movies.Movies(cIndice).Plot + "|" + Movies.Movies(cIndice).NO_Auto_Watch.ToString + "|" + Movies.Movies(cIndice).NO_Auto_Pending.ToString)
            Guardar.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If : cAddWatched.Cancelado = True : cAddWatched.Remove = False
    End Sub

    Private Sub FGallery_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        If cAddWatched.Visible = True Then cAddWatched.TopMost = True : cAddWatched.TopMost = False
    End Sub

#End Region

#Region " Auto Pending "

    Private Sub Check_Auto_Pending()
        If (Movies.Movies Is Nothing OrElse Movies.Movies.Count = 0) Or (Pending.Movies Is Nothing OrElse Pending.Movies.Count = 0) Then Exit Sub
Repite: For Cuenta = 0 To Movies.Movies.Count - 1
            If Movies.Movies(Cuenta).NO_Auto_Pending = False Then
                For Cuenta2 = 0 To Pending.Movies.Count - 1
                    If IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Title) = IO.Path.GetFileNameWithoutExtension(Pending.Movies(Cuenta2).Title) Then
                        If Movies.Movies(Cuenta).ImdbID <> "" AndAlso Pending.Movies(Cuenta2).ImdbID <> "" AndAlso Movies.Movies(Cuenta).ImdbID = Pending.Movies(Cuenta2).ImdbID Then
                            Auto_Pending_MSG(Cuenta, Cuenta2) : GoTo Repite
                        End If
                    End If
                Next
                For Cuenta2 = 0 To Pending.Movies.Count - 1
                    If IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Title) = IO.Path.GetFileNameWithoutExtension(Pending.Movies(Cuenta2).Title) Then Auto_Pending_MSG(Cuenta, Cuenta2) : GoTo Repite
                Next
            End If
        Next
    End Sub

    Private Sub Auto_Pending_MSG(cIndice As Integer, tIndice As Integer)
        If Movies.Movies(cIndice).NO_Auto_Pending = True Then Exit Sub
        Dim cAddWatched As New FAddWatched
        cAddWatched.LMessage.Text = "This movie is in Pending" + vbCrLf + vbCrLf + "Do you want to remove it from Pending?"
        cAddWatched.BFRemove.Location = cAddWatched.BFAccept.Location : cAddWatched.BFRemove.Size = cAddWatched.BFAccept.Size
        cAddWatched.BRemove.Location = cAddWatched.BAccept.Location : cAddWatched.BRemove.Size = cAddWatched.BAccept.Size
        cAddWatched.BRemove.Text = "Remove it" : cAddWatched.BRemove.BringToFront()

        If Movies.Movies(cIndice).NoCover = False Then
            cAddWatched.PCover.Image = LoadImageClone((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".jpg").Replace("\\", "\"))
        Else
            cAddWatched.PCover.Image = My.Resources.NoCover
        End If
        cAddWatched.LTitle.Text = Movies.Movies(cIndice).Title
        cAddWatched.ShowDialog()
        If cAddWatched.Cancelado = False Then
            If cAddWatched.Remove = False Then Movies.DeleteMovie(cIndice, False, True) Else Pending.DeleteMovie(tIndice, False, True)
        Else
            Movies.Movies(cIndice).NO_Auto_Pending = True
            For Cuenta = 0 To Movies.Movies_F.Count - 1
                If Movies.Movies_F(Cuenta).Ruta = Movies.Movies(cIndice).Ruta Then Movies.Movies_F(cIndice).NO_Auto_Pending = True : Exit For
            Next
            If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"))
            Dim Guardar As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"))
            Guardar.Write(Movies.Movies(cIndice).ImdbID + "|" + Movies.Movies(cIndice).Rating + "|" + Movies.Movies(cIndice).Year + "|" + Movies.Movies(cIndice).Duration.ToString + "|" + Movies.Movies(cIndice).Language + "|" + Movies.Movies(cIndice).Country + "|" + Movies.Movies(cIndice).Genre + "|" + Movies.Movies(cIndice).Director + "|" + Movies.Movies(cIndice).Actor + "|" + Movies.Movies(cIndice).Plot + "|" + Movies.Movies(cIndice).NO_Auto_Watch.ToString + "|" + Movies.Movies(cIndice).NO_Auto_Pending.ToString)
            Guardar.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(cIndice).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If
    End Sub

#End Region

#Region " Reset Autos "

    Private Sub Reset_Autos()
        If Movies.Movies Is Nothing OrElse Movies.Movies.Count = 0 Then Exit Sub
        For Cuenta = 0 To Movies.Movies.Count - 1
            Movies.Movies(Cuenta).NO_Auto_Watch = False : Movies.Movies(Cuenta).NO_Auto_Pending = False
            If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
            Dim Guardar As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
            Guardar.Write(Movies.Movies(Cuenta).ImdbID + "|" + Movies.Movies(Cuenta).Rating + "|" + Movies.Movies(Cuenta).Year + "|" + Movies.Movies(Cuenta).Duration.ToString + "|" + Movies.Movies(Cuenta).Language + "|" + Movies.Movies(Cuenta).Country + "|" + Movies.Movies(Cuenta).Genre + "|" + Movies.Movies(Cuenta).Director + "|" + Movies.Movies(Cuenta).Actor + "|" + Movies.Movies(Cuenta).Plot + "|" + Movies.Movies(Cuenta).NO_Auto_Watch.ToString + "|" + Movies.Movies(Cuenta).NO_Auto_Pending.ToString)
            Guardar.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
        Next
    End Sub

#End Region

#End Region

#Region " About "

    Private Sub LAbout_DoubleClick(sender As Object, e As MouseEventArgs) Handles LAbout.DoubleClick
        If e.Button <> MouseButtons.Left Then Exit Sub
        Dim Mensaje As String = ""
        Mensaje = "K-Movies 3.0 was finished in April 2018 when 410 movies were waiting to be watched" + vbCrLf + vbCrLf + vbCrLf
        Mensaje += "K-Movies 4.0 was finished in August 2022 when 193 movies were waiting to be watched" + vbCrLf + vbCrLf + vbCrLf
        Dim TotalSize As Long
        Dim TotalDuration As Integer
        For Cuenta = 0 To Movies.Movies.Count - 1
            TotalSize += My.Computer.FileSystem.GetFileInfo(Movies.Movies(Cuenta).Ruta).Length
            TotalDuration += Movies.Movies(Cuenta).Duration
        Next
        Mensaje += "Total Movies:       " + Movies.Movies.Count.ToString + vbCrLf
        Mensaje += "Total Size:             " + FormatSize(TotalSize) + vbCrLf
        Mensaje += "Total Duration:  " + FormatDuration(TotalDuration) + vbCrLf
        Mensaje += vbCrLf + vbCrLf + "By: Kobayashi" + vbCrLf + vbCrLf
        MsgBox(Mensaje, MsgBoxStyle.Information, "K-Movies " + Version)
    End Sub

    Private Function FormatDuration(Duration As Integer) As String
        Dim t As TimeSpan = TimeSpan.FromMinutes(Duration)
        Dim years As Integer
        Dim months As Integer
        Dim days As Integer = t.Days
        Dim hours As Integer = t.Hours
        Dim minutes As Integer = t.Minutes
        Dim Resultado As String = ""

        Dim toDate As Date = Now.AddDays(days)
        Dim fromDate As Date = Now : days = 0

        Do Until toDate.AddYears(-1) < fromDate : years += 1 : toDate = toDate.AddYears(-1) : Loop
        Do Until toDate.AddMonths(-1) < fromDate : months += 1 : toDate = toDate.AddMonths(-1) : Loop
        Do Until toDate.AddDays(-1) < fromDate : days += 1 : toDate = toDate.AddDays(-1) : Loop

        Select Case years
            Case 0 : Resultado += " "
            Case 1 : Resultado += years.ToString + " Year - "
            Case > 1 : Resultado += years.ToString + " Years - "
        End Select
        Select Case months
            Case 0 : Resultado += " "
            Case 1 : Resultado += months.ToString + " Month - "
            Case > 1 : Resultado += months.ToString + " Months - "
        End Select
        Select Case days
            Case 0 : Resultado += " "
            Case 1 : Resultado += days.ToString + " Day - "
            Case > 1 : Resultado += days.ToString + " Days - "
        End Select
        Select Case hours
            Case 0 : Resultado += " "
            Case 1 : Resultado += hours.ToString + " Hour - "
            Case > 1 : Resultado += hours.ToString + " Hours - "
        End Select
        Select Case minutes
            Case 1 : Resultado += minutes.ToString + " Minute - "
            Case > 1 : Resultado += minutes.ToString + " Minutes - "
        End Select

        Return Resultado.Substring(0, Resultado.Length - 3)
    End Function

    Private Sub LAbout_MouseEnter(sender As Object, e As EventArgs) Handles LAbout.MouseEnter
        Dim InfoToolTipText As String = vbCrLf + "  " + "Search Movie:         F3  "
        InfoToolTipText += vbCrLf + "  " + "--------------------" + vbCrLf + "  " + "Update Movies:      F5  "
        InfoToolTipText += vbCrLf + "  " + "--------------------" + vbCrLf + "  " + "Update Info:           F7  "
        InfoToolTipText += vbCrLf + "  " + "--------------------" + vbCrLf + "  " + "Random Movie:      F9  " + vbCrLf + " "
        Tooltip_Show(sender, InfoToolTipText)
    End Sub

    Private Sub LRandom_Click(sender As Object, e As MouseEventArgs) Handles LRandom.Click
        If e.Button = MouseButtons.Left Then Play_Random_Movie()
    End Sub

#End Region

#Region " Controls "

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim CP As CreateParams = MyBase.CreateParams
            CP.Style = &HA0000
            Return CP
        End Get
    End Property

    Private Sub Controls_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick, LFYear.Click, LDuration.Click, LFRating.Click, LFDurationThan.Click, LFRatingThan.Click, LFGenre.Click, LFLanguage.Click, LFCountry.Click, LFDirector.Click, LFActor.Click, LTitle.Click, LAbout.Click, LIYear.Click, LIRating.Click, LIDuration.Click, LIGenre.Click, LIAdded.Click, LILanguage.Click, LICountry.Click, LIDirector.Click, LIActor.Click, LYear.Click, LRating.Click, LDuration.Click, LGenre.Click, LAdded.Click, LLanguage.Click, LCountry.Click, LDirector.Click, LActor.Click, LPlot.Click, LCountInfo.Click, LGallery.Click, PCover.Click, Gallery.Click, Separator.Click, LRandom.Click
        THidden.Focus() : CFilterTemp = ""
    End Sub

    Private Sub THidden_KeyPress(sender As Object, e As KeyPressEventArgs) Handles THidden.KeyPress
        e.Handled = True
    End Sub

#End Region

#End Region

#Region " Functions "

#Region " Create Folders "

    Public Sub Create_Folders()
        If IO.Directory.Exists(Pending.MoviePath) = False Then IO.Directory.CreateDirectory(Pending.MoviePath) : IO.File.SetAttributes(Pending.MoviePath, IO.FileAttributes.Hidden)
        If IO.Directory.Exists(Watched.MoviePath) = False Then IO.Directory.CreateDirectory(Watched.MoviePath) : IO.File.SetAttributes(Watched.MoviePath, IO.FileAttributes.Hidden)
        If IO.Directory.Exists((Movies.MoviePath + "\cache").Replace("\\", "\")) = False Then IO.Directory.CreateDirectory((Movies.MoviePath + "\cache").Replace("\\", "\")) : IO.File.SetAttributes((Movies.MoviePath + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
        If IO.Directory.Exists((Pending.MoviePath + "\cache").Replace("\\", "\")) = False Then IO.Directory.CreateDirectory((Pending.MoviePath + "\cache").Replace("\\", "\")) : IO.File.SetAttributes((Pending.MoviePath + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
        If IO.Directory.Exists((Watched.MoviePath + "\cache").Replace("\\", "\")) = False Then IO.Directory.CreateDirectory((Watched.MoviePath + "\cache").Replace("\\", "\")) : IO.File.SetAttributes((Watched.MoviePath + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
    End Sub

#End Region

#Region " Get cMovies "

    Public Function Get_cMovies(Optional Group As String = "") As Movies_CL
        If Group = "" Then Group = Mode
        Select Case Group
            Case "Movies" : Return Movies
            Case "Pending" : Return Pending
            Case "Watched" : Return Watched
        End Select : Return Nothing
    End Function

#End Region

#Region " No Image "

    Public Function NoImage(MovieName As String) As Image
        Dim bmp As New Bitmap(Gallery.PicWidth, Gallery.PicHeight)
        Dim g As Graphics = Graphics.FromImage(bmp)
        g.Clear(Color.Black) : g.DrawRectangle(New Pen(Color.SlateGray, 1), 0, 0, bmp.Width - 1, bmp.Height - 1)
        Dim FontSize As Integer = 8 : Dim LineSep As Integer = 10
        Dim StringSize As SizeF : Dim Title As String = "" : Dim AddLine As Boolean
        Dim MovieNameArray() As String = MovieName.Split(" ")
        Dim Line As String = MovieNameArray(0)

        If MovieNameArray.Length > 1 Then
            For Cuenta = 1 To MovieNameArray.Length
Repite1:        StringSize = g.MeasureString(Line, New Font("Calibri Light", FontSize))
                If StringSize.Width > Gallery.PicWidth - 4 Then
                    If Line.Contains(" ") = False Then FontSize -= 1 : GoTo Repite1
                    Title += "|" + Line.Substring(0, Line.Length - MovieNameArray(Cuenta - 1).Length)
                    AddLine = True : Line = MovieNameArray(Cuenta - 1)
                    If Cuenta < MovieNameArray.Length Then Cuenta -= 1 : AddLine = False
                Else
                    AddLine = True : If Cuenta < MovieNameArray.Length Then Line += " " + MovieNameArray(Cuenta)
                End If
            Next
            If AddLine = True Then Title = (Title + "|" + Line).Substring(1)
        Else
Repite2:    StringSize = g.MeasureString(Line, New Font("Calibri Light", FontSize))
            If StringSize.Width > Gallery.PicWidth - 4 Then FontSize -= 1 : GoTo Repite2
            Title = Line
        End If

        If Title.Substring(0, 1) = "|" Then Title = Title.Substring(1)
        MovieNameArray = Title.Split("|")

        Dim y As Integer = (Gallery.PicHeight - (g.MeasureString(MovieNameArray(0), New Font("Calibri Light", FontSize)).Height / 2 + LineSep) * MovieNameArray.Length) / 2
        Dim x, TempFontSize As Integer
        For Cuenta = 0 To MovieNameArray.Length - 1
            TempFontSize = FontSize
Repite3:    StringSize = g.MeasureString(MovieNameArray(Cuenta), New Font("Calibri Light", TempFontSize))
            If StringSize.Width > Gallery.PicWidth - 4 Then TempFontSize -= 1 : GoTo Repite3
            x = (Gallery.PicWidth - g.MeasureString(MovieNameArray(Cuenta), New Font("Calibri Light", TempFontSize)).Width) / 2
            g.DrawString(MovieNameArray(Cuenta), New Font("Calibri Light", TempFontSize), New SolidBrush(Color.SlateGray), x, y)
            y += g.MeasureString(MovieNameArray(Cuenta), New Font("Calibri Light", TempFontSize)).Height / 2 + LineSep
        Next : g.Dispose() : Return bmp
    End Function

#End Region

#Region " Cache Image "

    Public Function CacheImage(OldImage As Image, TargetHeight As Integer, TargetWidth As Integer) As Image
        Try : Dim NewHeight As Integer = TargetHeight
            Dim NewWidth As Integer = NewHeight / OldImage.Height * OldImage.Width
            If NewWidth > TargetWidth Then NewWidth = TargetWidth : NewHeight = NewWidth / OldImage.Width * OldImage.Height
            Return New Bitmap(OldImage, NewWidth, NewHeight)
        Catch : End Try : Return My.Resources.NoCover
    End Function

#End Region

#Region " Load Image Clone "

    Public Function LoadImageClone(Ruta As String) As Image
        Try : Dim bmpOriginal As Image = Image.FromFile(Ruta) : Dim bmpClone As New Bitmap(bmpOriginal.Width, bmpOriginal.Height) : Dim gr As Graphics = Graphics.FromImage(bmpClone)
            gr.SmoothingMode = Drawing2D.SmoothingMode.None : gr.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor : gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighSpeed
            gr.DrawImage(bmpOriginal, 0, 0, bmpOriginal.Width, bmpOriginal.Height) : gr.Dispose() : bmpOriginal.Dispose() : Return bmpClone
        Catch : End Try : Return My.Resources.NoCover
    End Function

#End Region

#Region " Draw Reflection "

    Private Sub DrawReflection(ByRef PImage As PictureBox)
        Try : Dim img As Image
            If PCover.Image Is Nothing Then Exit Sub
            If PCover.Image.Width > 400 Or PCover.Image.Height > 533 Then
                Dim bm_dest As New Bitmap(400, 533) : Dim gr_dest As Graphics = Graphics.FromImage(bm_dest)
                gr_dest.DrawImage(New Bitmap(PCover.Image), 0, 0, bm_dest.Width + 1, bm_dest.Height + 1)
                img = bm_dest
            Else
                img = PImage.Image
            End If

            Dim height As Integer = img.Height + 100
            Dim bmp As New Bitmap(img.Width, height, Imaging.PixelFormat.Format64bppPArgb)
            Dim brsh As New Drawing2D.LinearGradientBrush(New Rectangle(0, 0, img.Width + 10, height), Color.Transparent, Color.Black, Drawing2D.LinearGradientMode.Vertical)
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution)
            Using grfx As Graphics = Graphics.FromImage(bmp)
                Dim bm As Bitmap = CType(img, Bitmap)
                grfx.DrawImage(bm, 0, 0, img.Width, img.Height)
                Dim bm1 As Bitmap = CType(img, Bitmap)
                bm1.RotateFlip(RotateFlipType.Rotate180FlipX)
                grfx.DrawImage(bm1, 0, img.Height)
            End Using

            Dim bm_src1 As New Bitmap(bmp) : Dim bm_out = New Bitmap(bm_src1.Width, bm_src1.Height)
            Using gr As Graphics = Graphics.FromImage(bm_out)
                Dim alpha As Integer : Dim x As Integer = 0, loopTo As Integer = bm_src1.Width - 1
                While x <= loopTo
                    Dim y As Integer = bm_src1.Height / 2, loopTo1 As Integer = bm_src1.Height - 1
                    While y <= loopTo1
                        'alpha = (255 * (bm_src1.Height - y - 1)) / ((1 / 2) * bm_src1.Height - 1)
                        'alpha = (1020 * (bm_src1.Height - y - 1)) / (bm_src1.Height - 4)
                        alpha = (255 * ((bm_src1.Height - 1) - y)) / ((bm_src1.Height - 1) - (bm_src1.Height - (bm_src1.Height / 2)))
                        Dim clr As Color = bm_src1.GetPixel(x, y)
                        clr = Color.FromArgb(alpha, clr.R, clr.G, clr.B)
                        bm_src1.SetPixel(x, y, clr)
                        y += 1
                    End While : x += 1
                End While : gr.DrawImage(bm_src1, 0, 0)
            End Using : If bm_out IsNot Nothing Then PImage.Image = bm_out
        Catch : End Try
    End Sub

#End Region

#Region " File Size "

    Public Function FileSize(MoviePath As String) As String
        If Not IO.File.Exists(MoviePath) Or MoviePath.Length = 0 Then Return "──"
        Return FormatSize(My.Computer.FileSystem.GetFileInfo(MoviePath).Length)
    End Function

    Public Function FileSize(MovieLength As Long) As String
        If MovieLength = 0 Then Return "──" Else Return FormatSize(MovieLength)
    End Function

    Public Function FileSizeCompare(Ruta_1 As String, Ruta_2 As String) As Boolean
        Try : If FileSize(Ruta_1) = FileSize(Ruta_2) Then Return True Else Return False
        Catch : Return False : End Try
    End Function

    Private Function FormatSize(TheSize As Long) As String
        Dim DoubleBytes As Double
        Try : Select Case TheSize
                Case Is >= 1099511627776
                    DoubleBytes = TheSize / 1099511627776 'TB
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = TheSize / 1073741824 'GB
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = TheSize / 1048576 'MB
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = TheSize / 1024 'KB
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = TheSize ' bytes
                    Return FormatNumber(DoubleBytes, 2) & " bytes"
                Case Else
                    Return ""
            End Select
        Catch : Return "" : End Try
    End Function

#End Region

#Region " Font Size "

    Public Sub SetFontSize(ByRef Control As Object, MaxFontSize As Single, Optional MinFontSize As Single = 5)
        Dim TextSize As Integer = MaxFontSize : Dim Fuente As Font = Control.Font : Dim Height As Integer = Control.Height
        Do Until TextRenderer.MeasureText(Control.Text, New Font(Fuente.Name, TextSize, FontStyle.Regular)).Width < Control.Width Or TextSize <= MinFontSize : TextSize -= 1 : Loop
        If TextSize < MinFontSize Then TextSize = MinFontSize
        Control.Font = New Font(Fuente.Name, TextSize, FontStyle.Regular) : Control.Height = Height
    End Sub

    Private Function GetTextSize(Control As Object) As Size
        Return New Size(TextRenderer.MeasureText(Control.Text, Control.Font).Width, TextRenderer.MeasureText(Control.Text, Control.Font).Height)
    End Function

    Private Function GetTextSize(Texto As String, Fuente As Font) As Size
        Return New Size(TextRenderer.MeasureText(Texto, Fuente).Width, TextRenderer.MeasureText(Texto, Fuente).Height)
    End Function

    Private Function GetTextSize(Texto As String, FontName As String, FontSize As Single, FontStyle As FontStyle) As Size
        Return New Size(TextRenderer.MeasureText(Texto, New Font(FontName, FontSize, FontStyle)).Width, TextRenderer.MeasureText(Texto, New Font(FontName, FontSize, FontStyle)).Height)
    End Function

#End Region

#Region " Tooltip "

    Dim ToolTip_sender As Object
    Public Sub Tooltip_Show(sender As Object, Texto As String)
        If sender Is Nothing Or Texto = "" Then General_Tooltip.Dispose() : General_Tooltip = New ToolTip : Exit Sub
        General_Tooltip.Dispose() : General_Tooltip = New ToolTip With {.InitialDelay = 1000, .ReshowDelay = 10, .AutoPopDelay = 10000, .ShowAlways = True, .Tag = sender.name, .OwnerDraw = True, .ForeColor = Color.DarkGray}
        AddHandler General_Tooltip.Draw, AddressOf Tooltip_Draw
        ToolTip_sender = sender
        General_Tooltip.SetToolTip(sender, Texto)
    End Sub

    Private Sub Tooltip_Draw(sender As Object, e As DrawToolTipEventArgs)
        e.Graphics.FillRectangle(New SolidBrush(Color.FromArgb(50, 50, 50)), e.Bounds)
        e.DrawBorder()
        Dim TextFormat As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or TextFormatFlags.NoFullWidthCharacterBreak
        e.DrawText(TextFormat)
    End Sub

#End Region

#Region " VLC "

#Region " VLC Timer "

    Private Sub VLC_Timer_Tick()

        'En VLC Herramientas - Preferencias - Todo - Interfaz - Interfaces Principales:  Check "Web"
        'Luego en Interfaces Principales - Lua:  Establece la contraseña de Lua HTTP a "2501"

        If Movies.Movies Is Nothing OrElse Movies.Movies.Count = 0 Then Exit Sub
        If Process.GetProcessesByName("vlc").Count = 0 Or Auto_Watched = False Then VLC_MoviePlaying = "" : VLC_TimePlayed = 0 : Exit Sub

        Dim HTML As String = ""
        Dim Name As String = ""
        Dim State As String = ""

        Try : Dim request As Net.WebRequest = Net.WebRequest.Create("http://127.0.0.1:8080/requests/status.xml")
            request.Headers(Net.HttpRequestHeader.Authorization) = "Basic " + Convert.ToBase64String(System.Text.Encoding.[Default].GetBytes(":2501"))
            request.PreAuthenticate = True
            Using response As Net.WebResponse = request.GetResponse()
                Using reader As New IO.StreamReader(response.GetResponseStream()) : HTML = reader.ReadToEnd : End Using
            End Using
        Catch : End Try
        If HTML = "" Then VLC_MoviePlaying = "" : VLC_TimePlayed = 0 : Exit Sub

        Try : State = HTML.Split({"<state>"}, StringSplitOptions.None)(1).Split({"</state>"}, StringSplitOptions.None)(0)
            Name = HTML.Split({"<info name='filename'>"}, StringSplitOptions.None)(1).Split({"</info>"}, StringSplitOptions.None)(0)
        Catch : End Try

        If Watched.Movies IsNot Nothing AndAlso Watched.Movies.Count > 0 Then
            For Cuenta = 0 To Watched.Movies.Count - 1
                If IO.Path.GetFileName(Watched.Movies(Cuenta).Ruta) = Name Then Exit Sub
            Next
        End If

        If VLC_MoviePlaying <> Name Then
            VLC_MoviePlaying = Name : VLC_TimePlayed = 0
        Else
            If State = "playing" Then VLC_TimePlayed += VLC_Timer.Interval / 1000
        End If

        If VLC_TimePlayed >= VLC_MinPlayTime * 60 Then
            VLC_MoviePlaying = "" : VLC_TimePlayed = 0
            For Cuenta = 0 To Movies.Movies.Count - 1
                If IO.Path.GetFileName(Movies.Movies(Cuenta).Ruta) = Name Then
                    VLC_Add_Watched(Cuenta)
                    If Movies.Movies(Cuenta).NO_Auto_Watch = True Then Exit Sub
                    For Cuenta2 = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileName(Watched.Movies(Cuenta2).Ruta) = Name Then Auto_Watched_MSG(Cuenta, Cuenta2) : Exit Sub
                    Next
                End If
            Next
        End If
    End Sub

#End Region

#Region " VLC Add Watched "

    Private Sub VLC_Add_Watched(cIndice As Integer)
        If IO.File.Exists((Watched.MoviePath + "\" + IO.Path.GetFileName(Movies.Movies(cIndice).Ruta)).Replace("\\", "\")) = True Then Exit Sub

        Dim Source As String = Movies.Movies(cIndice).Ruta
        Dim Destination As String = (Watched.MoviePath + "\" + IO.Path.GetFileName(Movies.Movies(cIndice).Ruta)).Replace("\\", "\")
        Dim SourceNoExt = (IO.Path.GetDirectoryName(Source) + "\" + IO.Path.GetFileNameWithoutExtension(Source)).Replace("\\", "\")
        Dim DestinationNoExt = (IO.Path.GetDirectoryName(Destination) + "\" + IO.Path.GetFileNameWithoutExtension(Destination)).Replace("\\", "\")
        Dim Guardar As New IO.StreamWriter(DestinationNoExt + ".mp4") : Guardar.Write(Movies.Movies(cIndice).Title) : Guardar.Close()
        IO.File.SetAttributes(DestinationNoExt + ".mp4", IO.FileAttributes.ReadOnly)

        'jpg
        If IO.File.Exists(DestinationNoExt + ".jpg") = True Then IO.File.Delete(DestinationNoExt + ".jpg")
        If IO.File.Exists(SourceNoExt + ".jpg") = True Then
            IO.File.Copy(SourceNoExt + ".jpg", DestinationNoExt + ".jpg")
            IO.File.SetAttributes(DestinationNoExt + ".jpg", IO.FileAttributes.Hidden)
        End If
        'jpg cache
        If IO.Directory.Exists((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\")) = False Then
            IO.Directory.CreateDirectory((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\"))
            IO.File.SetAttributes((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If
        If IO.File.Exists((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"))
        If IO.File.Exists((IO.Path.GetDirectoryName(Source) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Source) + ".jpg").Replace("\\", "\")) = True Then
            IO.File.Copy((IO.Path.GetDirectoryName(Source) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Source) + ".jpg").Replace("\\", "\"), (IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"))
            IO.File.SetAttributes((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If
        'txt
        If IO.File.Exists(DestinationNoExt + ".txt") = True Then IO.File.Delete(DestinationNoExt + ".txt")
        If IO.File.Exists(SourceNoExt + ".txt") = True Then
            IO.File.Copy(SourceNoExt + ".txt", DestinationNoExt + ".txt")
            IO.File.SetAttributes(DestinationNoExt + ".txt", IO.FileAttributes.Hidden)
        End If

        If Watched.Movies Is Nothing Then ReDim Watched.Movies(0) Else ReDim Preserve Watched.Movies(Watched.Movies.Count)
        Watched.Movies(Watched.Movies.Count - 1) = Movies.Movies(cIndice)
        Watched.Movies(Watched.Movies.Count - 1).Added = Now
        Watched.Movies(Watched.Movies.Count - 1).Ruta = Destination
        Watched.Movies(Watched.Movies.Count - 1).CoverCachedPath = (Watched.MoviePath + "\cache\" + IO.Path.GetFileName(Movies.Movies(cIndice).CoverCachedPath)).Replace("\\", "\")

        Watched.Add_Filter(Watched.Movies.Count - 1)

        If Mode = Watched.Group Then Watched.FilterMovies()
    End Sub

#End Region

#End Region

#Region " Transfering Timer "

    Private Sub Transfering_Tick()
        If Transfering = 0 Then
            If LTransfer.Visible = True Then
                LTransfer.Visible = False
                If LTransfer.Tag <> "" AndAlso Mode = "Movies" Then LTransfer.Tag = "" : Movies.FilterMovies() : SelectMovie()
                LTransfer.Tag = ""
            End If
        Else
            Dim Mensaje As String
            If Transfering > 1 Then Mensaje = "Transfering " + Transfering.ToString + " movies " Else Mensaje = "Transfering " + Transfering.ToString + " movie "
            If LTransfer.Text <> Mensaje Then
                LTransfer.Text = Mensaje ': LTransfer.Refresh()
                If Mode = "Movies" And LTransfer.Tag = "Transfering" Then Movies.FilterMovies() : SelectMovie()
            End If
            If LTransfer.Visible = False Then LTransfer.Visible = True
        End If
    End Sub

#End Region

#End Region

#Region " Controls "

#Region " Modes "

    Private Sub LTransfer_Click(sender As Object, e As MouseEventArgs) Handles LTransfer.Click, LTransfer.DoubleClick
        If e.Button = MouseButtons.Left Then
            If Mode <> "Movies" Then LTransfer.Tag = "Transfering" : LBMovies.Text = "Movies" : Mode = "Movies" : Show_Movies() : Exit Sub
            If LTransfer.Tag = "Transfering" Then LTransfer.Tag = "" Else LTransfer.Tag = "Transfering"
            If Mode = "Movies" Then Movies.FilterMovies() : SelectMovie()
        End If
    End Sub

    Private Sub LBButtons_Click(sender As Object, e As MouseEventArgs) Handles LBMovies.Click
        If Control.ModifierKeys = Keys.Control AndAlso e.Button = MouseButtons.Right Then Reset_Autos() : Beep()
        'If e.Button = MouseButtons.Right Then
        '    Movies.Movies(0).Copying = True
        '    Movies.Movies(40).Copying = True
        '    Transfering = 2
        '    Exit Sub
        'End If
        If e.Button <> MouseButtons.Left Then Exit Sub
        If CMode.DroppedDown = False Then CMode.DroppedDown = True : CMode.Focus()
    End Sub

    Private Sub CMovies_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles CMode.SelectionChangeCommitted
        LBMovies.Text = sender.Text : CMode.DroppedDown = False : Application.DoEvents()
        If sender.Text <> Mode Then Mode = sender.Text : Show_Movies()
    End Sub

#Region " Show Movies "

    Private Sub Show_Movies()
        LCountImdb.Text = "" : LCountImdb.Refresh() : LCountImdb.Visible = False

        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        LCSort.Text = cMovies.Filter.Sorted : If cMovies.Filter.NOaz = True Then LAz.Text = "Za" Else LAz.Text = "Az"
        LAz.Refresh()

        If cMovies.Filter.Year.SelectedIndex = -1 Then cMovies.Filter.Year.SelectedIndex = 0
        If cMovies.Filter.Genre.SelectedIndex = -1 Then cMovies.Filter.Genre.SelectedIndex = 0
        If cMovies.Filter.Language.SelectedIndex = -1 Then cMovies.Filter.Language.SelectedIndex = 0
        If cMovies.Filter.Country.SelectedIndex = -1 Then cMovies.Filter.Country.SelectedIndex = 0
        If cMovies.Filter.Director.SelectedIndex = -1 Then cMovies.Filter.Director.SelectedIndex = 0
        If cMovies.Filter.Actor.SelectedIndex = -1 Then cMovies.Filter.Actor.SelectedIndex = 0

        Dim Items(cMovies.Filter.Year.Items.Count - 1) As Object : cMovies.Filter.Year.Items.CopyTo(Items, 0) : CYear.Items.Clear() : CYear.Items.AddRange(Items) : CYear.SelectedIndex = cMovies.Filter.Year.SelectedIndex : LCYear.Text = CYear.Text
        ReDim Items(cMovies.Filter.Genre.Items.Count - 1) : cMovies.Filter.Genre.Items.CopyTo(Items, 0) : CGenre.Items.Clear() : CGenre.Items.AddRange(Items) : CGenre.SelectedIndex = cMovies.Filter.Genre.SelectedIndex : LCGenre.Text = CGenre.Text
        ReDim Items(cMovies.Filter.Language.Items.Count - 1) : cMovies.Filter.Language.Items.CopyTo(Items, 0) : CLanguage.Items.Clear() : CLanguage.Items.AddRange(Items) : CLanguage.SelectedIndex = cMovies.Filter.Language.SelectedIndex : LCLanguage.Text = CLanguage.Text
        ReDim Items(cMovies.Filter.Country.Items.Count - 1) : cMovies.Filter.Country.Items.CopyTo(Items, 0) : CCountry.Items.Clear() : CCountry.Items.AddRange(Items) : CCountry.SelectedIndex = cMovies.Filter.Country.SelectedIndex : LCCountry.Text = CCountry.Text
        ReDim Items(cMovies.Filter.Director.Items.Count - 1) : cMovies.Filter.Director.Items.CopyTo(Items, 0) : CDirector.Items.Clear() : CDirector.Items.AddRange(Items) : CDirector.SelectedIndex = cMovies.Filter.Director.SelectedIndex : LCDirector.Text = CDirector.Text
        ReDim Items(cMovies.Filter.Actor.Items.Count - 1) : cMovies.Filter.Actor.Items.CopyTo(Items, 0) : CActor.Items.Clear() : CActor.Items.AddRange(Items) : CActor.SelectedIndex = cMovies.Filter.Actor.SelectedIndex : LCActor.Text = CActor.Text

        LFRatingThan.Text = cMovies.Filter.Rating_Mode : TRating.Text = cMovies.Filter.Rating
        LFDurationThan.Text = cMovies.Filter.Duration_Mode : TDuration.Text = cMovies.Filter.Duration
        If Mode = "Watched" Then LIAdded.Text = "Watched:" Else LIAdded.Text = "Added:"
        TSearch.Text = cMovies.Filter.Search
        If TSearch.Text = "" Then TSearch.Text = "Search for a movie"

        cMovies.FilterMovies(False)
        If cMovies.IMDB.Updating = False Then SelectMovie(cMovies.Selected)
        Gallery.UpdateGallery()

        THidden.Focus()
    End Sub

#End Region

#End Region

#Region " Combos "

    Private Sub Filters_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles CYear.SelectionChangeCommitted, CLanguage.SelectionChangeCommitted, CCountry.SelectionChangeCommitted, CDirector.SelectionChangeCommitted, CActor.SelectionChangeCommitted, CGenre.SelectionChangeCommitted
        Try : Dim CFilter As FlatComboBox = CType(sender, FlatComboBox)
            CFilter.DroppedDown = False
            Select Case CFilter.Name
                Case "CYear"
                    Get_cMovies(Mode).Filter.Year.SelectedIndex = CYear.SelectedIndex : LCYear.Text = CYear.Text
                Case "CGenre"
                    Get_cMovies(Mode).Filter.Genre.SelectedIndex = CGenre.SelectedIndex : LCGenre.Text = CGenre.Text
                Case "CLanguage"
                    Get_cMovies(Mode).Filter.Language.SelectedIndex = CLanguage.SelectedIndex : LCLanguage.Text = CLanguage.Text
                Case "CCountry"
                    Get_cMovies(Mode).Filter.Country.SelectedIndex = CCountry.SelectedIndex : LCCountry.Text = CCountry.Text
                Case "CDirector"
                    Get_cMovies(Mode).Filter.Director.SelectedIndex = CDirector.SelectedIndex : LCDirector.Text = CDirector.Text
                Case "CActor"
                    Get_cMovies(Mode).Filter.Actor.SelectedIndex = CActor.SelectedIndex : LCActor.Text = CActor.Text
            End Select : Application.DoEvents()
            If Opacity = 1 And Gallery.Visible = True Then Get_cMovies(Mode).FilterMovies() : THidden.Focus()
            SelectMovie()
        Catch : End Try
    End Sub

    Private Sub Filters_MouseDown(sender As Object, e As MouseEventArgs) Handles CYear.MouseDown, CLanguage.MouseDown, CCountry.MouseDown, CDirector.MouseDown, CActor.MouseDown, CGenre.MouseDown
        If e.Button = MouseButtons.Right Then sender.SelectedIndex = 0 : Filters_SelectionChangeCommitted(sender, e)
    End Sub

    Private Sub LFilters_MouseDown(sender As Object, e As MouseEventArgs) Handles LFYear.MouseDown, LFGenre.MouseDown, LFLanguage.MouseDown, LFCountry.MouseDown, LFDirector.MouseDown, LFActor.MouseDown, LCYear.MouseDown, LCGenre.MouseDown, LCLanguage.MouseDown, LCCountry.MouseDown, LCDirector.MouseDown, LCActor.MouseDown, TCYear.MouseDown, TCGenre.MouseDown, TCLanguage.MouseDown, TCCountry.MouseDown, TCDirector.MouseDown, TCActor.MouseDown
        If e.Button = MouseButtons.Right Then
            Select Case sender.Name
                Case "LFYear", "LCYear", "TCYear" : CYear.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CYear, e)
                Case "LFGenre", "LCGenre", "TCGenre" : CGenre.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CGenre, e)
                Case "LFLanguage", "LCLanguage", "TCLanguage" : CLanguage.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CLanguage, e)
                Case "LFCountry", "LCCountry", "TCCountry" : CCountry.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CCountry, e)
                Case "LFDirector", "LCDirector", "TCDirector" : CDirector.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CDirector, e)
                Case "LFActor", "LCActor", "TCActor" : CActor.SelectedIndex = 0 : Filters_SelectionChangeCommitted(CActor, e)
            End Select
        End If
    End Sub

    Private Sub LFilters_Click(sender As Object, e As MouseEventArgs) Handles LCYear.Click, LCGenre.Click, LCLanguage.Click, LCCountry.Click, LCDirector.Click, LCActor.Click, TCYear.Click, TCGenre.Click, TCLanguage.Click, TCCountry.Click, TCDirector.Click, TCActor.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Select Case sender.Name
            Case "LCYear", "TCYear" : CYear.DroppedDown = True : CYear.Focus()
            Case "LCGenre", "TCGenre" : CGenre.DroppedDown = True : CGenre.Focus()
            Case "LCLanguage", "TCLanguage" : CLanguage.DroppedDown = True : CLanguage.Focus()
            Case "LCCountry", "TCCountry" : CCountry.DroppedDown = True : CCountry.Focus()
            Case "LCDirector", "TCDirector" : CDirector.DroppedDown = True : CDirector.Focus()
            Case "LCActor", "TCActor" : CActor.DroppedDown = True : CActor.Focus()
        End Select
    End Sub

#End Region

#Region " TextBox "

#Region " Rating "

    Private Sub LFRatingThan_Click(sender As Object, e As MouseEventArgs) Handles LFRatingThan.Click, LFRating.Click
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then Exit Sub
        If e.Button = MouseButtons.Left Then
            Select Case LFRatingThan.Text
                Case "Greater than" : LFRatingThan.Text = "Smaller than"
                Case "Smaller than" : LFRatingThan.Text = "Equal to"
                Case "Equal to" : LFRatingThan.Text = "Greater than"
            End Select
        ElseIf e.Button = MouseButtons.Right Then
            LFRatingThan.Text = "Greater than" : TRating.Text = "0.0"
        End If : THidden.Focus()

        If Get_cMovies(Mode).Filter.Rating_Mode <> LFRatingThan.Text Or Get_cMovies(Mode).Filter.Rating <> TRating.Text Then
            Get_cMovies(Mode).Filter.Rating_Mode = LFRatingThan.Text : Get_cMovies(Mode).Filter.Rating = TRating.Text
            Get_cMovies(Mode).FilterMovies()
            SelectMovie()
        End If
    End Sub

    Private Sub TRating_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TRating.KeyPress
        If TRating.Text.Length = 1 And TRating.Text.Substring(0, 1) = "1" And e.KeyChar = "0" Then Exit Sub
        If TRating.Text = "10" And TRating.SelectedText.Length = 0 AndAlso Not Char.IsControl(e.KeyChar) Then e.KeyChar = "" : e.Handled = True
        If TRating.Text.Length = 2 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length > 0 And TRating.SelectedText = TRating.Text.Substring(1) And e.KeyChar = "." Or e.KeyChar = "0" Then Exit Sub
        If TRating.Text.Length = 3 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length > 0 And TRating.SelectedText = TRating.Text.Substring(1) And e.KeyChar = "." Or e.KeyChar = "0" Then Exit Sub
        If TRating.Text.Length = TRating.SelectedText.Length And e.KeyChar <> "." Then Exit Sub
        If TRating.Text.Length = 2 And TRating.Text.Substring(0, 1) = "1" And TRating.SelectedText.Length = 0 AndAlso Not Char.IsControl(e.KeyChar) And TRating.Text.Contains(".") = False Then e.KeyChar = "" : e.Handled = True
        If TRating.SelectionStart = 1 And TRating.SelectedText.Length > 0 And e.KeyChar <> "." AndAlso Not Char.IsControl(e.KeyChar) Then e.KeyChar = "" : e.Handled = True
        If TRating.SelectedText.Length = TRating.Text.Length And e.KeyChar = "." Then e.KeyChar = "" : e.Handled = True
        If TRating.Text = "" And e.KeyChar = "." Then e.KeyChar = "" : e.Handled = True
        If TRating.Text.Contains(".") And e.KeyChar = "." And TRating.SelectedText.Contains(".") = False Then e.KeyChar = "" : e.Handled = True
        If TRating.Text.Length = 1 And e.KeyChar <> "." AndAlso Not Char.IsControl(e.KeyChar) Then e.KeyChar = "" : e.Handled = True
        If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> "." Then e.KeyChar = "" : e.Handled = True
    End Sub

    Private Sub TRating_KeyDown(sender As Object, e As KeyEventArgs) Handles TRating.KeyDown
        If e.KeyCode = Keys.Enter Then : e.Handled = True
            If Trim(TRating.Text) = "" Then TRating.Text = "0.0"
            If Trim(TRating.Text).Length = 1 And Trim(TRating.Text).Contains(".") = False Then TRating.Text = Trim(TRating.Text) + ".0"
            If Trim(TRating.Text).Length = 2 And Trim(TRating.Text).Contains(".") = True Then TRating.Text = Trim(TRating.Text) + "0"
            If Trim(TRating.Text).StartsWith("0") = True And Trim(TRating.Text).Substring(1, 1) <> "." Then TRating.Text = Trim(TRating.Text).Substring(1)
            Dim IntRating As Integer : Integer.TryParse(TRating.Text, IntRating)
            If IntRating > 10 Then TRating.Text = "10"
            CFilterTemp = TRating.Text : THidden.Focus()
            If Get_cMovies(Mode).Filter.Rating <> TRating.Text Then
                Get_cMovies(Mode).Filter.Rating = TRating.Text
                Get_cMovies(Mode).FilterMovies()
                SelectMovie()
            End If
        End If
    End Sub

    Private Sub TRating_GotFocus(sender As Object, e As EventArgs) Handles TRating.GotFocus
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then THidden.Focus() : Exit Sub
        CFilterTemp = TRating.Text
    End Sub

    Private Sub TRating_LostFocus(sender As Object, e As EventArgs) Handles TRating.LostFocus
        If CFilterTemp <> TRating.Text And CFilterTemp <> "" Then TRating.Text = CFilterTemp : Exit Sub
        If Trim(TRating.Text) = "" Then TRating.Text = "0.0"
        If Trim(TRating.Text).Length = 1 And Trim(TRating.Text).Contains(".") = False Then TRating.Text = Trim(TRating.Text) + ".0"
        If Trim(TRating.Text).Length = 2 And Trim(TRating.Text).Contains(".") = True Then TRating.Text = Trim(TRating.Text) + "0"
        If Trim(TRating.Text).StartsWith("0") = True And Trim(TRating.Text).Substring(1, 1) <> "." Then TRating.Text = Trim(TRating.Text).Substring(1)
        Dim IntRating As Integer : Integer.TryParse(TRating.Text, IntRating)
        If IntRating > 10 Then TRating.Text = "10"

    End Sub

#End Region

#Region " Duration "

    Private Sub LFDurationThan_Click(sender As Object, e As MouseEventArgs) Handles LFDurationThan.Click, LFDuration.Click
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then Exit Sub
        If e.Button = MouseButtons.Left Then
            Select Case LFDurationThan.Text
                Case "Longer than" : LFDurationThan.Text = "Shorter than"
                Case "Shorter than" : LFDurationThan.Text = "Equal to"
                Case "Equal to" : LFDurationThan.Text = "Longer than"
            End Select
        ElseIf e.Button = MouseButtons.Right Then
            LFDurationThan.Text = "Longer than" : TDuration.Text = "0 min"
        End If : THidden.Focus()

        If Get_cMovies(Mode).Filter.Duration_Mode <> LFDurationThan.Text Or Get_cMovies(Mode).Filter.Duration <> TDuration.Text Then
            Get_cMovies(Mode).Filter.Duration_Mode = LFDurationThan.Text : Get_cMovies(Mode).Filter.Duration = TDuration.Text
            Get_cMovies(Mode).FilterMovies()
            SelectMovie()
        End If
    End Sub

    Private Sub TDuration_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TDuration.KeyPress
        If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then e.KeyChar = "" : e.Handled = True
    End Sub
    Private Sub TDuration_KeyDown(sender As Object, e As KeyEventArgs) Handles TDuration.KeyDown
        If e.KeyCode = Keys.Enter Then : e.Handled = True
            If TDuration.Text = "" Then TDuration.Text = "0"
            If CFilterTemp <> Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString Then CFilterTemp = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString : Get_cMovies(Mode).FilterMovies()
            THidden.Focus()
        End If
    End Sub

    Private Sub TDuration_GotFocus(sender As Object, e As EventArgs) Handles TDuration.GotFocus
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then THidden.Focus() : Exit Sub
        TDuration.Text = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString
        CFilterTemp = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString
    End Sub

    Private Sub TDuration_LostFocus(sender As Object, e As EventArgs) Handles TDuration.LostFocus
        If CFilterTemp <> Integer.Parse(System.Text.RegularExpressions.Regex.Replace(TDuration.Text, "[^\d]", "")).ToString Then TDuration.Text = CFilterTemp
        If TDuration.Text = "" Then TDuration.Text = "0"
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then Exit Sub
        If TDuration.Text.Length > 0 Then TDuration.Text = TDuration.Text + " min" Else TDuration.Text = "0 min"

        If Get_cMovies(Mode).Filter.Duration <> TDuration.Text Then
            Get_cMovies(Mode).Filter.Duration = TDuration.Text
            Get_cMovies(Mode).FilterMovies()
            SelectMovie()
        End If
    End Sub

#End Region

#Region " Search "

    Private Sub TSearch_TextChanged(sender As Object, e As EventArgs) Handles TSearch.TextChanged
        Dim CursorLocation As Integer = TSearch.SelectionStart
        If TSearch.Text.Length = 1 AndAlso TSearch.Text.Substring(0, 1) <> TSearch.Text.Substring(0, 1).ToUpper Then TSearch.Text = TSearch.Text.Substring(0, 1).ToUpper + TSearch.Text.Substring(1) : TSearch.SelectionStart = CursorLocation
    End Sub

    Private Sub TSearch_GotFocus(sender As Object, e As EventArgs) Handles TSearch.GotFocus
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then THidden.Focus() : Exit Sub
        CFilterTemp = TSearch.Text : If TSearch.Text.ToLower = "search for a movie" Then TSearch.Text = ""
        TSearch.ForeColor = TDuration.ForeColor
    End Sub

    Private Sub TSearch_LostFocus(sender As Object, e As EventArgs) Handles TSearch.LostFocus
        If CFilterTemp <> TSearch.Text Then TSearch.Text = CFilterTemp
        If LGallery.Visible = True And LGallery.Text <> "NO MOVIES" Then Exit Sub
        If TSearch.Text = "" Then TSearch.Text = "Search for a movie"
    End Sub

    Private Sub TSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles TSearch.KeyDown
        If e.KeyCode = Keys.Enter Then : e.Handled = True
            CFilterTemp = TSearch.Text
            If Get_cMovies(Mode).Filter.Search <> TSearch.Text Then
                Get_cMovies(Mode).Filter.Search = TSearch.Text
                Get_cMovies(Mode).FilterMovies()
                SelectMovie()
            End If : THidden.Focus()
            If TSearch.Text = "" Then TSearch.Text = "Search for a movie"
        End If
    End Sub

    Private Sub TSearch_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TSearch.KeyPress
        If TSearch.Text = "" Or TSearch.SelectedText = TSearch.Text Then e.KeyChar = e.KeyChar.ToString.ToUpper
    End Sub

    Private Sub LTitle_TextChanged(sender As Object, e As EventArgs) Handles LTitle.TextChanged
        SetFontSize(LTitle, 36, 5)
    End Sub

#End Region

#End Region

#Region " Sorting "

    Private Sub LCSort_Click(sender As Object, e As MouseEventArgs) Handles LCSort.Click, LCSort.DoubleClick
        If e.Button <> MouseButtons.Left Then Exit Sub
        If CSort.DroppedDown = False Then CSort.DroppedDown = True : CSort.Focus()
    End Sub

    Private Sub CSort_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles CSort.SelectionChangeCommitted
        LCSort.Text = sender.Text : CSort.DroppedDown = False : Application.DoEvents()
        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + Version) : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + Version, True)
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        If sender.Text <> cMovies.Filter.Sorted Then cMovies.Filter.Sorted = sender.Text : Reg.SetValue(cMovies.Group + "_Sorted", sender.Text) : cMovies.FilterMovies()
        Reg.Close()
    End Sub

    Private Sub LAz_Click(sender As Object, e As MouseEventArgs) Handles LAz.Click, LAz.DoubleClick
        If e.Button <> MouseButtons.Left Then Exit Sub
        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + Version) : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + Version, True)
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        cMovies.Filter.NOaz = Not cMovies.Filter.NOaz : Reg.SetValue(cMovies.Group + "_NOaz", cMovies.Filter.NOaz.ToString) : If cMovies.Filter.NOaz = True Then LAz.Text = "Za" Else LAz.Text = "Az"
        LAz.Refresh() : Reg.Close() : cMovies.FilterMovies()
    End Sub

#End Region

#Region " Zoom "

    Private Sub LZoomIn_Click(sender As Object, e As MouseEventArgs) Handles LZoomIn.Click, LZoomIn.DoubleClick
        If sender IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Exit Sub
        Gallery.ZoomIn() : Gallery.CancelFlash()
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + "4.0") : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + "4.0", True)
        Reg.SetValue(cMovies.Group + "_Zoom", cMovies.Zoom.ToString) : Reg.Close()
    End Sub

    Private Sub LZoomOut_Click(sender As Object, e As MouseEventArgs) Handles LZoomOut.Click, LZoomOut.DoubleClick
        If sender IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Exit Sub
        Gallery.ZoomOut() : Gallery.CancelFlash()
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + "4.0") : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + "4.0", True)
        Reg.SetValue(cMovies.Group + "_Zoom", cMovies.Zoom.ToString) : Reg.Close()
    End Sub

#End Region

#End Region

#Region " Gallery "

#Region " Updated "

    Private Sub Gallery_Updated() Handles Gallery.Updated
        Gallery.Update()
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        If cMovies.Movies_F Is Nothing OrElse cMovies.Movies_F.Count = 0 Or (Gallery.Controls.Count = 0 OrElse Gallery.Controls(0).Visible = False) Then
            LGallery.Visible = True
            LGallery.Text = "NO MOVIES" : LGallery.BringToFront()
            LCountInfo.Text = "No movies" : LCountInfo.Refresh()
            SelectMovie(-99)
        Else
            LGallery.Visible = False
            LCountInfo.Text = "Showing " + cMovies.Movies_F.Count.ToString + " of " + cMovies.Movies.Count.ToString + " movies" : LCountInfo.Refresh()
        End If
    End Sub

#End Region

#Region " KeyPress "

    Private Sub FGallery_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        If FSplash.Visible = True Then Exit Sub
        If Asc(e.KeyChar) = 13 Then e.Handled = True
        If Asc(e.KeyChar) = 27 Then e.Handled = True : CYear.DroppedDown = False : CGenre.DroppedDown = False : CLanguage.DroppedDown = False : CCountry.DroppedDown = False : CDirector.DroppedDown = False : CActor.DroppedDown = False : THidden.Focus()
    End Sub

    Private Sub FGallery_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp
        If FSplash.Visible = True Then Exit Sub
        If CYear.DroppedDown = False AndAlso CGenre.DroppedDown = False AndAlso CLanguage.DroppedDown = False AndAlso CCountry.DroppedDown = False AndAlso CDirector.DroppedDown = False AndAlso CActor.DroppedDown = False AndAlso TRating.IsFocused = False AndAlso TDuration.IsFocused = False AndAlso TSearch.IsFocused = False Then
            If e.KeyCode = Keys.Right Then Gallery.CancelFlash() : Gallery.NextPage()
            If e.KeyCode = Keys.Left Then Gallery.CancelFlash() : Gallery.PreviousPage()
            If e.KeyCode = Keys.PageUp Then Gallery.CancelFlash() : Gallery.PreviousPage()
            If e.KeyCode = Keys.PageDown Then Gallery.CancelFlash() : Gallery.NextPage()
            If e.KeyCode = Keys.Up Then Gallery.CancelFlash() : Gallery.HomePage()
            If e.KeyCode = Keys.Down Then Gallery.CancelFlash() : Gallery.EndPage()
            If e.KeyCode = Keys.Home Then Gallery.CancelFlash() : Gallery.HomePage()
            If e.KeyCode = Keys.End Then Gallery.CancelFlash() : Gallery.EndPage()
            If e.KeyCode = Keys.F3 Then TSearch.Focus()
            If e.KeyCode = Keys.F5 Then Gallery.CancelFlash() : SelectMovie(-99) : Get_cMovies(Mode).Load_Movies()
            If e.KeyCode = Keys.F7 Then Get_cMovies(Mode).GetMovieInfo()
            If e.KeyCode = Keys.F9 Then Play_Random_Movie()
            If (e.KeyCode.ToString.Length = 1 Or e.KeyCode.ToString.StartsWith("D") = True) AndAlso (Char.IsLetterOrDigit(Convert.ToChar(e.KeyCode)) And e.Control = False) Then
                If e.KeyCode.ToString.Length = 2 Then Gallery.GoToMovie(e.KeyCode.ToString) Else Gallery.GoToMovie(e.KeyCode.ToString.Replace("D", ""))
            End If
        End If
    End Sub

    Private Sub Gallery_MouseScroll(sender As Object, e As MouseEventArgs) Handles Gallery.MouseScroll
        If (ModifierKeys And Keys.Control) = Keys.Control Then
            If e.Delta > 0 Then LZoomIn_Click(Nothing, e) Else LZoomOut_Click(Nothing, e)
        Else
            If e.Delta > 0 Then Gallery.CancelFlash() : Gallery.PreviousPage() Else Gallery.CancelFlash() : Gallery.NextPage()
        End If
    End Sub

#End Region

#Region " Tooltips "

    Private Sub Movie_Enter(CMovie As Object, e As EventArgs) Handles Gallery.Movie_Enter
        Dim Pic As GalleryImage = CMovie
        If Pic.Name.StartsWith("Edit") = True Then
            If Pic.EditYear = "" Then
                Tooltip_Show(Pic, "Year:     -" + vbCrLf + "Imdb:   " + Pic.EditImdb)
            Else
                Tooltip_Show(Pic, "Year:     " + Pic.EditYear + vbCrLf + "Imdb:   " + Pic.EditImdb)
            End If
        Else
            If Pic.Mode <> Mode Then Exit Sub
            Dim cMovies As Movies_CL = Get_cMovies(Mode)

            If Pic.Indice >= cMovies.Movies_F.Count Then Exit Sub
            Dim cRating As String = cMovies.Movies_F(Pic.Indice).Rating : If cRating = "0.0" Then cRating = "-"
            Dim cDuration As String = cMovies.Movies_F(Pic.Indice).Duration.ToString + " min" : If cDuration = "0 min" Then cDuration = "-"
            Dim MovieToolTipText As String = "Rating:         " + cRating
            If cMovies.Movies_F(Pic.Indice).Year.ToString = "" Then MovieToolTipText += vbCrLf + "Year:             -" Else MovieToolTipText += vbCrLf + "Year:             " + cMovies.Movies_F(Pic.Indice).Year.ToString
            MovieToolTipText += vbCrLf + "Duration:     " + cDuration
            Select Case Mode
                Case "Movies"
                    MovieToolTipText += vbCrLf + "Added:        " + cMovies.Movies_F(Pic.Indice).Added.ToString("dd/MM/yyyy")
                Case "Pending"

                Case "Watched"
                    MovieToolTipText += vbCrLf + "Watched:     " + cMovies.Movies_F(Pic.Indice).Added.ToString("dd/MM/yyyy")
            End Select
            Tooltip_Show(CMovie, MovieToolTipText)
        End If
    End Sub

#End Region

#Region " Select Movie "

    Private Sub Movie_Click(CMovie As Object, e As MouseEventArgs) Handles Gallery.Movie_Click
        Dim Movie As GalleryImage = CType(CMovie, GalleryImage) : THidden.Focus()
        If e.Button = MouseButtons.Left Then Gallery.CancelFlash() : SelectMovie(Movie.Indice)
    End Sub

    Public Sub SelectMovie(Optional cIndice As Integer = -1)
        If cIndice = -99 Or Gallery.Controls().Count = 0 Then
            PCover.Image = Nothing : LTitle.Text = "" : LYear.Text = "" : LRating.Text = "" : LDuration.Text = "" : LAdded.Text = "" : LGenre.Text = "" : LLanguage.Text = "" : LCountry.Text = "" : LDirector.Text = "" : LActor.Text = "" : LPlot.Text = ""
            PCover.Refresh() : LTitle.Refresh() : LYear.Refresh() : LRating.Refresh() : LDuration.Refresh() : LAdded.Refresh() : LGenre.Refresh() : LLanguage.Refresh() : LCountry.Refresh() : LDirector.Refresh() : LActor.Refresh() : LPlot.Refresh() : Exit Sub
        End If
        Dim Movie As GalleryImage = CType(Gallery.Controls(0), GalleryImage)
        Dim cMovies As Movies_CL = Get_cMovies(Mode)

        If cIndice = -1 Then cIndice = Movie.Indice : cMovies.Selected = Movie.Indice Else cMovies.Selected = cIndice
        If Gallery.Controls.Count = 0 Then Exit Sub
        If Gallery.Controls(0).Visible = False Or cMovies.Movies_F Is Nothing OrElse cMovies.Movies_F.Count = 0 OrElse cMovies.Movies_F(cIndice).Title Is Nothing Then Exit Sub

        If cMovies.Movies_F(cIndice).NoCover = False Then PCover.Image = LoadImageClone((IO.Path.GetDirectoryName(cMovies.Movies_F(cIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(cIndice).Ruta) + ".jpg").Replace("\\", "\")) Else PCover.Image = My.Resources.NoCover
        DrawReflection(PCover) : Application.DoEvents()

        If cMovies.Movies_F(cIndice).Title.Contains("(") = True Then LTitle.Text = Trim(cMovies.Movies_F(cIndice).Title.Split("(")(0)) Else LTitle.Text = Trim(cMovies.Movies_F(cIndice).Title)
        SetFontSize(LTitle, 36)

        LYear.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Year, " - ")
        If cMovies.Movies_F(cIndice).Rating = "0.0" Then LRating.Text = " - " Else LRating.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Rating, " - ")
        LGenre.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Genre, " - ") : SetFontSize(LGenre, 12)
        LAdded.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Added.ToString("dd/MM/yyyy"), " - ")
        LLanguage.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Language, " - ") : SetFontSize(LLanguage, 12)
        LCountry.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Country, " - ") : SetFontSize(LCountry, 12)
        If cMovies.Movies_F(cIndice).Duration = 0 Then LDuration.Text = " - " Else LDuration.Text = cMovies.Movies_F(cIndice).Duration.ToString + " min"
        LDirector.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Director, " - ") : SetFontSize(LDirector, 12)
        LActor.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Actor, " - ")
        LPlot.Text = Movies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(cIndice).Plot, "NO SYNOPSIS")
        PCover.Refresh() : LTitle.Refresh() : LYear.Refresh() : LRating.Refresh() : LDuration.Refresh() : LAdded.Refresh() : LGenre.Refresh() : LLanguage.Refresh() : LCountry.Refresh() : LDirector.Refresh() : LActor.Refresh() : LPlot.Refresh()
        Application.DoEvents()
    End Sub

#End Region

#End Region

#Region " Menus "

#Region " Opening "

    Private Sub MPMenu_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MPMenu.Opening
        Dim Movie As GalleryImage = CType(CType(sender, FlatContextMenuStrip).SourceControl, GalleryImage)
        MPMenu.Tag = CType(CType(sender, FlatContextMenuStrip).SourceControl, GalleryImage)

        Select Case Mode
            Case "Movies"
                MAdd.Text = "Import" : MAdd.Visible = True : MAddTo.Visible = True : MSeparator4.Visible = True : MCopyto.Text = "Copy to..." : MCopyto.Visible = True : MMoveto.Visible = True : MSeparator0.Visible = True
                MPlay.Visible = True : MDelete.Visible = True : MSeparator1.Visible = True : MSeparator2.Visible = True : MSettings.Visible = True : MSeparator5.Visible = True : MPMenu.Height = 282 : Dim MenuModifier As Integer
                MWatched.Visible = True : MPending.Visible = True : MAddTo.DropDown.Height = 48

                If Movies.Movies_F(Movie.Indice).ImdbID = "" Then MImdb.Visible = False : MenuModifier -= 22 Else MImdb.Visible = True
                Dim cIndice As Integer = -1
                For Cuenta = 0 To Movies.Movies.Length - 1
                    If Movies.Movies(Cuenta).Ruta = Movies.Movies_F(Movie.Indice).Ruta Then cIndice = Cuenta : Exit For
                Next

                If cIndice <> -1 AndAlso Movies.Movies(cIndice).Copying = True Then
                    If Movies.Movies(cIndice).Importing = True Then MPlay.Visible = False : MSeparator1.Visible = False : MenuModifier -= 28
                    MDelete.Visible = False
                    MCopyto.Text = "[Cancel]" : MMoveto.Visible = False : MenuModifier -= 44
                End If

                Dim HidePending, HideWatched As Boolean

                If Watched.Movies IsNot Nothing AndAlso Watched.Movies.Count > 0 Then
                    For Cuenta = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileName(Watched.Movies(Cuenta).Ruta) = IO.Path.GetFileName(Movies.Movies(cIndice).Ruta) Then HideWatched = True : MWatched.Visible = False : Exit For
                    Next
                End If
                If Pending.Movies IsNot Nothing AndAlso Pending.Movies.Count > 0 Then
                    For Cuenta = 0 To Pending.Movies.Count - 1
                        If IO.Path.GetFileName(Pending.Movies(Cuenta).Ruta) = IO.Path.GetFileName(Movies.Movies(cIndice).Ruta) Then HidePending = True : MPending.Visible = False : Exit For
                    Next
                End If

                If (HidePending = True And HideWatched = False) Or (HidePending = False And HideWatched = True) Then
                    MAddTo.DropDown.Height = 26
                ElseIf HidePending = True And HideWatched = True Then
                    MAddTo.Visible = False : MSeparator2.Visible = False : MenuModifier -= 25
                Else
                    MAddTo.Visible = True : MSeparator2.Visible = True
                End If

                MPMenu.Height = MPMenu.Height + MenuModifier
            Case "Pending"
                MAdd.Text = "Add" : MAdd.Visible = True : MAddTo.Visible = True : MSeparator4.Visible = True : MCopyto.Visible = False : MMoveto.Visible = False : MSeparator0.Visible = True
                MPlay.Visible = False : MSeparator1.Visible = False : MSeparator2.Visible = False : MSettings.Visible = True : MSeparator5.Visible = True : MPMenu.Height = 204 : Dim MenuModifier As Integer
                MWatched.Visible = True : MPending.Visible = False : MAddTo.DropDown.Height = 26

                If Watched.Movies IsNot Nothing AndAlso Watched.Movies.Count > 0 Then
                    For Cuenta = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileName(Pending.Movies_F(Movie.Indice).Ruta) = IO.Path.GetFileName(Watched.Movies(Cuenta).Ruta) Then MAddTo.Visible = False : MSeparator4.Visible = False : MenuModifier -= 28 : Exit For
                    Next
                End If

                If Pending.Movies_F IsNot Nothing AndAlso Pending.Movies_F.Count > 0 AndAlso Pending.Movies_F(Movie.Indice).ImdbID = "" Then MImdb.Visible = False : MenuModifier -= 22 Else MImdb.Visible = True
                MPMenu.Height = MPMenu.Height + MenuModifier
            Case "Watched"
                MAdd.Text = "Add" : MAdd.Visible = True : MAddTo.Visible = True : MSeparator4.Visible = True : MCopyto.Visible = False : MMoveto.Visible = False : MSeparator0.Visible = True
                MPlay.Visible = False : MSeparator1.Visible = False : MSeparator2.Visible = False : MSettings.Visible = True : MSeparator5.Visible = True : MPMenu.Height = 204 : Dim MenuModifier As Integer
                MWatched.Visible = False : MPending.Visible = True : MAddTo.DropDown.Height = 26

                If Pending.Movies IsNot Nothing AndAlso Pending.Movies.Count > 0 Then
                    For Cuenta = 0 To Pending.Movies.Count - 1
                        If IO.Path.GetFileName(Watched.Movies_F(Movie.Indice).Ruta) = IO.Path.GetFileName(Pending.Movies(Cuenta).Ruta) Then MAddTo.Visible = False : MSeparator4.Visible = False : MenuModifier -= 28 : Exit For
                    Next
                End If

                If Watched.Movies_F IsNot Nothing AndAlso Watched.Movies_F.Count > 0 AndAlso Watched.Movies_F(Movie.Indice).ImdbID = "" Then MImdb.Visible = False : MenuModifier -= 22 Else MImdb.Visible = True
                MPMenu.Height = MPMenu.Height + MenuModifier
        End Select
    End Sub

    Private Sub MGMenu_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MGMenu.Opening
        Select Case Mode
            Case "Movies"
                MGAdd.Text = "Import" : MGAdd.Visible = True : MGSeparator1.Visible = True : MGSettings.Visible = True : MGSeparator2.Visible = True : MGMenu.Height = 82
                If IO.Directory.Exists(Movies.MoviePath) = False Then MGAdd.Visible = False : MGSeparator1.Visible = False : MGMenu.Height = 54
            Case "Pending" : MGAdd.Text = "Add" : MGAdd.Visible = True : MGSeparator1.Visible = True : MGSettings.Visible = True : MGSeparator2.Visible = True : MGMenu.Height = 82
            Case "Watched" : MGAdd.Text = "Add" : MGAdd.Visible = True : MGSeparator1.Visible = True : MGSettings.Visible = True : MGSeparator2.Visible = True : MGMenu.Height = 82
        End Select
    End Sub

#End Region

#Region " Import / Add "

    Private Sub MGAdd_Click(sender As Object, e As EventArgs) Handles MGAdd.Click, MAdd.Click
        If sender.Text = "Import" Then Import_Movie() Else Add_Movie()
    End Sub

#Region " Import "

    Dim Transfering As Integer

    Private Sub Import_Movie()
        Dim OpenFiles As New OpenFileDialog With {.Multiselect = True, .CheckFileExists = True, .Title = "Import movies", .ValidateNames = True, .Filter = "Movies|*.mp4;*.mkv;*.avi|All files|*.*"}
        If OpenFiles.ShowDialog() <> DialogResult.Cancel Then
            Dim AddMovies() As String = OpenFiles.FileNames : Dim No_Auto_Watched As Boolean : Dim Imported As Boolean

            For Cuenta = 0 To OpenFiles.FileNames.Count - 1
                No_Auto_Watched = False
                If Movies.Movies IsNot Nothing AndAlso Movies.Movies.Count > 0 Then
                    For Cuenta2 = 0 To Movies.Movies.Count - 1
                        If IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta2).Ruta).ToLower = IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta).ToLower) Then
                            If MsgBox("The movie '" + IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta).ToLower) + "' is already added" + vbCrLf + vbCrLf + "Do you want to overwrite it?", MsgBoxStyle.OkCancel, "K-Movies " + Version) = MsgBoxResult.Cancel Then
                                GoTo Salta
                            Else
                                If Movies.Movies(Cuenta).Copying = True Then
                                    MsgBox("The movie '" + IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta).ToLower) + "' is in transfer" + vbCrLf + vbCrLf + "Can't import the movie", MsgBoxStyle.OkCancel, "K-Movies " + Version)
                                    GoTo Salta
                                End If
                                Movies.DeleteMovie(Cuenta2, False, True)
                                Exit For
                            End If
                        End If
                    Next
                End If
                If Watched.Movies IsNot Nothing AndAlso Watched.Movies.Count > 0 Then
                    For Cuenta2 = 0 To Watched.Movies.Count - 1
                        If IO.Path.GetFileNameWithoutExtension(Watched.Movies(Cuenta2).Ruta).ToLower = IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta).ToLower) Then
                            If MsgBox("The movie '" + IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta).ToLower) + "' is already watched" + vbCrLf + vbCrLf + "Do you want to import it?", MsgBoxStyle.OkCancel, "K-Movies " + Version) = MsgBoxResult.Cancel Then GoTo Salta Else No_Auto_Watched = True : Exit For
                        End If
                    Next
                End If

                If Movies.Movies Is Nothing Then ReDim Preserve Movies.Movies(0) Else ReDim Preserve Movies.Movies(Movies.Movies.Count)
                Imported = True
                Dim Guardar As IO.StreamWriter : Guardar = New IO.StreamWriter(Movies.MoviePath + "\" + IO.Path.GetFileName(OpenFiles.FileNames(Cuenta)).Replace("\\", "\")) : Guardar.Write(IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta))) : Guardar.Close()
                Movies.Movies(Movies.Movies.Count - 1).Title = IO.Path.GetFileNameWithoutExtension(OpenFiles.FileNames(Cuenta))
                Movies.Movies(Movies.Movies.Count - 1).Ruta = Movies.MoviePath + "\" + IO.Path.GetFileName(OpenFiles.FileNames(Cuenta)).Replace("\\", "\")
                Movies.Movies(Movies.Movies.Count - 1).Added = Now
                Movies.Movies(Movies.Movies.Count - 1).NO_Auto_Watch = No_Auto_Watched
                Movies.Movies(Movies.Movies.Count - 1).CoverCachedPath = (Movies.MoviePath + "\cache\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Movies.Movies.Count - 1).Ruta) + ".jpg").Replace("\\", "\")
                Movies.Movies(Movies.Movies.Count - 1).CoverCached = NoImage(Movies.Movies(Movies.Movies.Count - 1).Title)

                Movies.Movies(Movies.Movies.Count - 1).Importing = True
                Movies.Movies(Movies.Movies.Count - 1).CopyTo = OpenFiles.FileNames(Cuenta)
                Movies.Movies(Movies.Movies.Count - 1).Size = My.Computer.FileSystem.GetFileInfo(Movies.Movies(Movies.Movies.Count - 1).CopyTo).Length

                Movies.Movies(Movies.Movies.Count - 1).Genre = ""
                Movies.Movies(Movies.Movies.Count - 1).Actor = ""
                Movies.Movies(Movies.Movies.Count - 1).Language = ""
                Movies.Movies(Movies.Movies.Count - 1).Country = ""
                Movies.Movies(Movies.Movies.Count - 1).Director = ""
Salta:      Next

            Get_cMovies(Mode).FilterMovies()
            If Imported = True Then Movies.GetMovieInfo()

            Dim OFiles() As String = OpenFiles.FileNames : OpenFiles.Dispose()
            For Cuenta = 0 To OFiles.Count - 1
                For Cuenta2 = 0 To Movies.Movies.Count - 1
                    If OFiles(Cuenta) = Movies.Movies(Cuenta2).CopyTo Then
                        Transfering += 1
                        Movies.Movies(Cuenta2).ImportThread = New Threading.Thread(AddressOf ImportThread_Thread) With {.IsBackground = True}
                        Movies.Movies(Cuenta2).ImportThread.Start(Cuenta2)
                    End If
                Next
            Next
        End If
    End Sub

#Region " Thread "

    Private Delegate Sub Delegate_ImportThread_Thread(Indice As Integer)
    Private Sub ImportThread_Thread(Indice As Integer)
        If Indice >= 9999 Then
            Indice -= 9999
            Movies.FilterMovies()
Repite:     For Cuenta = 0 To Movies.Movies.Count - 1
                Application.DoEvents()
                If Movies.Movies(Cuenta).Title = "[BORRADO]" Then Movies.DeleteMovie(Cuenta, False, True) : Exit For
            Next
            If Transfering = 0 Then Check_Auto_Watched() : Check_Auto_Pending()
            Exit Sub
        End If
        Dim SourceFolder As String = IO.Path.GetDirectoryName(Movies.Movies(Indice).CopyTo) + "\"
        Dim DestinationFolder As String = IO.Path.GetDirectoryName(Movies.Movies(Indice).Ruta) + "\"
        If IO.File.Exists(Movies.Movies(Indice).Ruta) = True Then IO.File.Delete(Movies.Movies(Indice).Ruta)

        Dim Ruta As String = Movies.Movies(Indice).Ruta
        Dim streamRead As IO.FileStream
        Dim streamWrite As IO.FileStream

        Try
            streamRead = New IO.FileStream(Movies.Movies(Indice).CopyTo, IO.FileMode.Open)
            streamWrite = New IO.FileStream(Movies.Movies(Indice).Ruta, IO.FileMode.Create)
            Dim lngLen As Long = streamRead.Length - 1
            Dim byteBuffer(1048576) As Byte
            Dim intBytesRead As Integer

            Movies.Movies(Indice).CopySize = 0
            Movies.Movies(Indice).Copying = True

            While streamRead.Position < lngLen

                Indice = Get_NewIndice(Ruta, Indice)
                If Movies.Movies(Indice).Copying = False Then
                    streamWrite.Flush()
                    streamWrite.Close()
                    streamWrite.Dispose()
                    streamRead.Close()
                    Movies.Movies(Indice).Title = "[BORRADO]"
                    'Movies.DeleteMovie(Indice, False, True)
                    Transfering -= 1
                    GoTo Salta
                End If

                intBytesRead = (streamRead.Read(byteBuffer, 0, 1048576))
                streamWrite.Write(byteBuffer, 0, intBytesRead)
                Movies.Movies(Indice).CopySize = streamRead.Position
                For Each Pic As GalleryImage In Gallery.Controls
                    Application.DoEvents()
                    If Pic.Ruta = Movies.Movies(Indice).Ruta Then Pic.Invalidate() : Exit For
                Next : Application.DoEvents()
            End While

            If IO.File.Exists((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).CopyTo) + ".srt").Replace("\\", "\")) = True Then
                If IO.File.Exists((DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\")) = True Then IO.File.Delete((DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\"))
                IO.File.Copy((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).CopyTo) + ".srt").Replace("\\", "\"), (DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\"), True)
                IO.File.SetAttributes((DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\"), IO.FileAttributes.Hidden)
            End If
            streamWrite.Flush()
            streamWrite.Close()
            streamRead.Close()
            IO.File.SetLastWriteTime(Movies.Movies(Indice).Ruta, Now)

        Catch ex As Exception
            MsgBox("Error importing movie" + vbCrLf + vbCrLf + ex.Message, MsgBoxStyle.Critical, "K-Movies " + Version) : Movies.DeleteMovie(Indice, False, True) : Transfering -= 1 : Exit Sub
        End Try
        Indice = Get_NewIndice(Ruta, Indice)
        Movies.Movies(Indice).CopySize = 0
        Movies.Movies(Indice).Importing = False
        Movies.Movies(Indice).Copying = False
        Movies.Movies(Indice).Moving = False
        Transfering -= 1
        If Delete_Imported = True Then
            If IO.File.Exists(Movies.Movies(Indice).CopyTo) = True Then My.Computer.FileSystem.DeleteFile(Movies.Movies(Indice).CopyTo, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
            If IO.File.Exists(Movies.Movies(Indice).CopyTo.Replace(IO.Path.GetExtension(Movies.Movies(Indice).CopyTo), ".srt")) = True Then My.Computer.FileSystem.DeleteFile(Movies.Movies(Indice).CopyTo.Replace(IO.Path.GetExtension(Movies.Movies(Indice).CopyTo), ".srt"), FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        End If

        For Each Pic As GalleryImage In Gallery.Controls
            If Pic.Ruta = Movies.Movies(Indice).Ruta Then Pic.Invalidate() : Exit For
        Next
Salta:  If InvokeRequired Then : Invoke(New Delegate_ImportThread_Thread(AddressOf ImportThread_Thread), 9999 + Indice) : Else
            '            Movies.DeleteMovie(Indice, False, True)
            'Repite:     For Cuenta = 0 To Movies.Movies.Count - 1
            '                If Movies.Movies(Cuenta).Title = "[BORRADO]" Then
            '                    Movies.DeleteMovie(Indice, False, True)
            '                    GoTo Repite
            '                End If
            '            Next
            '            Movies.FilterMovies()
        End If
    End Sub

    Private Function Get_NewIndice(Ruta As String, Indice As Integer) As Integer
        Dim cIndice As Integer = Indice
        If Indice >= Movies.Movies.Count OrElse Movies.Movies(Indice).Ruta <> Ruta Then
            For Cuenta = 0 To Movies.Movies.Count - 1
                Application.DoEvents()
                If Movies.Movies(Cuenta).Ruta = Ruta Then cIndice = Cuenta : Exit For
            Next
        End If : Return cIndice
    End Function

#End Region

#End Region

#Region " Add "

    Private Sub Add_Movie()
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        Dim EditInfo As New FEditInfo
        EditInfo.NewMovie = True
        EditInfo.LTitle.Text = ""
        EditInfo.TSTitle.Text = ""
        EditInfo.TYear.Text = ""
        EditInfo.TRating.Text = "0.0"
        EditInfo.TDuration.Text = "0 min"
        EditInfo.TImdb.Text = ""
        EditInfo.TGenre.Text = ""
        EditInfo.TLanguage.Text = ""
        EditInfo.TCountry.Text = ""
        EditInfo.TDirector.Text = ""
        EditInfo.TActor.Text = ""
        EditInfo.TPlot.Text = ""
        EditInfo.OriginalCover = My.Resources.NoCover
        EditInfo.PCover.Image = EditInfo.OriginalCover
        EditInfo.NoCover = True
        EditInfo.LRemoveCover.Visible = False
Repite: EditInfo.ShowDialog()

        If EditInfo.Cancelado = False Then
            If EditInfo.OriginalMovie.Title = "" Then Exit Sub Else EditInfo.OriginalMovie.Title = EditInfo.OriginalMovie.Title.Replace(":", " - ").Replace("/", " - ").Replace("¡", "").Replace("!", " - ")

            If IO.File.Exists((cMovies.MoviePath + "\" + EditInfo.OriginalMovie.Title).Replace("\\", "\") + ".mp4") = True Then
                For Cuenta = 0 To cMovies.Movies.Count - 1
                    If cMovies.Movies(Cuenta).Ruta = ((cMovies.MoviePath + "\" + EditInfo.OriginalMovie.Title).Replace("\\", "\") + ".mp4") Then
                        If EditInfo.OriginalMovie.ImdbID <> "" AndAlso cMovies.Movies(Cuenta).ImdbID <> "" AndAlso cMovies.Movies(Cuenta).ImdbID = EditInfo.OriginalMovie.ImdbID Then GoTo Salta
                    End If
                Next

                If EditInfo.OriginalMovie.ImdbID <> "" Then
                    EditInfo.OriginalMovie.Title = EditInfo.OriginalMovie.Title + " (" + EditInfo.OriginalMovie.ImdbID + ")"
                Else
                    Dim Contador As Integer = 1
                    Do Until IO.File.Exists((cMovies.MoviePath + "\" + EditInfo.OriginalMovie.Title).Replace("\\", "\") + " (" + Contador.ToString + ")" + ".mp4") : Contador += 1 : Loop
                    EditInfo.OriginalMovie.Title = EditInfo.OriginalMovie.Title + " (" + Contador.ToString + ")" + ".mp4"
                End If
            End If

Salta:      If cMovies.Movies IsNot Nothing AndAlso cMovies.Movies.Count > 0 Then
                For Cuenta = 0 To cMovies.Movies.Count - 1
                    If cMovies.Movies(Cuenta).Title IsNot Nothing AndAlso cMovies.Movies(Cuenta).Title.ToLower = EditInfo.OriginalMovie.Title.ToLower AndAlso cMovies.Movies(Cuenta).ImdbID = EditInfo.OriginalMovie.ImdbID Then
                        MsgBox("The movie '" + EditInfo.OriginalMovie.Title + "' is already added", MsgBoxStyle.OkOnly, "K-Movies " + Version)
                        EditInfo.Cancelado = True : EditInfo.Repite = True : GoTo Repite
                    End If
                Next
            End If

            Dim tMovie As Movies_CL : If Mode = "Watched" Then tMovie = Pending Else tMovie = Watched
            If tMovie.Movies IsNot Nothing AndAlso tMovie.Movies.Count > 0 Then
                For Cuenta = 0 To tMovie.Movies.Count - 1
                    If tMovie.Movies(Cuenta).Title IsNot Nothing AndAlso tMovie.Movies(Cuenta).Title.ToLower = EditInfo.OriginalMovie.Title.ToLower AndAlso tMovie.Movies(Cuenta).ImdbID = EditInfo.OriginalMovie.ImdbID Then
                        Dim Result As MsgBoxResult = MsgBox("The movie '" + EditInfo.OriginalMovie.Title + "' is in " + tMovie.Group + vbCrLf + vbCrLf + "Do you want to remove it from " + tMovie.Group + "?", MsgBoxStyle.YesNoCancel, "K-Movies " + Version)
                        Select Case Result
                            Case MsgBoxResult.Cancel : EditInfo.Cancelado = True : EditInfo.Repite = True : GoTo Repite
                            Case MsgBoxResult.Yes : tMovie.DeleteMovie(Cuenta, False, True) : Exit For
                            Case MsgBoxResult.No : Exit For
                        End Select
                    End If
                Next
            End If

            If Mode = "Watched" Then
                If Movies.Movies IsNot Nothing AndAlso Movies.Movies.Count > 0 Then
                    For Cuenta = 0 To Movies.Movies.Count - 1
                        If Movies.Movies(Cuenta).Title IsNot Nothing AndAlso Movies.Movies(Cuenta).Title.ToLower = EditInfo.OriginalMovie.Title.ToLower AndAlso Movies.Movies(Cuenta).ImdbID = EditInfo.OriginalMovie.ImdbID Then
                            Dim Result As MsgBoxResult = MsgBox("The movie '" + EditInfo.OriginalMovie.Title + "' is in Movies" + vbCrLf + vbCrLf + "Do you want to delete it from Movies?", MsgBoxStyle.YesNoCancel, "K-Movies " + Version)
                            Select Case Result
                                Case MsgBoxResult.Cancel : EditInfo.Cancelado = True : EditInfo.Repite = True : GoTo Repite
                                Case MsgBoxResult.Yes : Movies.DeleteMovie(Cuenta, False, True) : Exit For
                                Case MsgBoxResult.No
                                    Movies.Movies(Cuenta).NO_Auto_Watch = True
                                    If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                                    Dim Guardar2 As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                                    Guardar2.Write(Movies.Movies(Cuenta).ImdbID + "|" + Movies.Movies(Cuenta).Rating + "|" + Movies.Movies(Cuenta).Year + "|" + Movies.Movies(Cuenta).Duration.ToString + "|" + Movies.Movies(Cuenta).Language + "|" + Movies.Movies(Cuenta).Country + "|" + Movies.Movies(Cuenta).Genre + "|" + Movies.Movies(Cuenta).Director + "|" + Movies.Movies(Cuenta).Actor + "|" + Movies.Movies(Cuenta).Plot + "|" + Movies.Movies(Cuenta).NO_Auto_Watch.ToString + "|" + Movies.Movies(Cuenta).NO_Auto_Pending.ToString)
                                    Guardar2.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
                                    Exit For
                            End Select
                        End If
                    Next
                End If
            End If

            If Mode = "Pending" Then
                If Movies.Movies IsNot Nothing AndAlso Movies.Movies.Count > 0 Then
                    For Cuenta = 0 To Movies.Movies.Count - 1
                        If Movies.Movies(Cuenta).Title IsNot Nothing AndAlso Movies.Movies(Cuenta).Title.ToLower = EditInfo.OriginalMovie.Title.ToLower AndAlso Movies.Movies(Cuenta).ImdbID = EditInfo.OriginalMovie.ImdbID Then
                            Dim Result As MsgBoxResult = MsgBox("The movie '" + EditInfo.OriginalMovie.Title + "' is in Movies" + vbCrLf + vbCrLf + "Do you want to delete it from Movies?", MsgBoxStyle.YesNoCancel, "K-Movies " + Version)
                            Select Case Result
                                Case MsgBoxResult.Cancel : EditInfo.Cancelado = True : EditInfo.Repite = True : GoTo Repite
                                Case MsgBoxResult.Yes : Movies.DeleteMovie(Cuenta, False, True) : Exit For
                                Case MsgBoxResult.No
                                    Movies.Movies(Cuenta).NO_Auto_Pending = True
                                    If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                                    Dim Guardar2 As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                                    Guardar2.Write(Movies.Movies(Cuenta).ImdbID + "|" + Movies.Movies(Cuenta).Rating + "|" + Movies.Movies(Cuenta).Year + "|" + Movies.Movies(Cuenta).Duration.ToString + "|" + Movies.Movies(Cuenta).Language + "|" + Movies.Movies(Cuenta).Country + "|" + Movies.Movies(Cuenta).Genre + "|" + Movies.Movies(Cuenta).Director + "|" + Movies.Movies(Cuenta).Actor + "|" + Movies.Movies(Cuenta).Plot + "|" + Movies.Movies(Cuenta).NO_Auto_Watch.ToString + "|" + Movies.Movies(Cuenta).NO_Auto_Pending.ToString)
                                    Guardar2.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
                                    Exit For
                            End Select
                        End If
                    Next
                End If
            End If

            Dim DestinationNoExt As String = (cMovies.MoviePath + "\" + EditInfo.OriginalMovie.Title).Replace("\\", "\")
                Dim DestinationCacheNoExt As String = (cMovies.MoviePath + "\cache\" + EditInfo.OriginalMovie.Title).Replace("\\", "\")
                If IO.Directory.Exists((cMovies.MoviePath + "\cache").Replace("\\", "\")) = False Then IO.Directory.CreateDirectory((cMovies.MoviePath + "\cache").Replace("\\", "\")) : IO.File.SetAttributes((cMovies.MoviePath + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)

                If cMovies.Movies Is Nothing Then ReDim cMovies.Movies(0) Else ReDim Preserve cMovies.Movies(cMovies.Movies.Count)
                cMovies.Movies(cMovies.Movies.Count - 1) = EditInfo.OriginalMovie
            cMovies.Movies(cMovies.Movies.Count - 1).Ruta = DestinationNoExt + ".mp4"
            cMovies.Movies(cMovies.Movies.Count - 1).Added = Now

            Dim Guardar As IO.StreamWriter
            Guardar = New IO.StreamWriter(cMovies.Movies(cMovies.Movies.Count - 1).Ruta) : Guardar.Write(EditInfo.OriginalMovie.Title) : Guardar.Close()

            IO.File.SetAttributes(DestinationNoExt + ".mp4", IO.FileAttributes.ReadOnly)

            IO.File.Delete((cMovies.MoviePath + "\cache\" + EditInfo.OriginalMovie.Title + ".jpg").Replace("\\", "\"))
                IO.File.Delete(DestinationNoExt + ".jpg") : IO.File.Delete(DestinationNoExt + ".nojpg")
                IO.File.Delete(DestinationNoExt + ".jpg") : IO.File.Delete(DestinationNoExt + ".txt")

                If EditInfo.NoCover = False Then
                    EditInfo.PCover.Image.Save(DestinationNoExt + ".jpg") : IO.File.SetAttributes(DestinationNoExt + ".jpg", IO.FileAttributes.Hidden)
                    CacheImage(EditInfo.PCover.Image, 190, 135).Save(DestinationCacheNoExt + ".jpg")
                    cMovies.Movies(cMovies.Movies.Count - 1).NoCover = False
                    cMovies.Movies(cMovies.Movies.Count - 1).CoverCachedPath = DestinationCacheNoExt + ".jpg"
                    cMovies.Movies(cMovies.Movies.Count - 1).CoverCached = LoadImageClone(DestinationCacheNoExt + ".jpg")
                ElseIf EditInfo.NoCover = True Then
                    Guardar = New IO.StreamWriter(DestinationNoExt + ".nojpg") : Guardar.Write("NoCover") : Guardar.Close() : IO.File.SetAttributes(DestinationNoExt + ".nojpg", IO.FileAttributes.Hidden)
                    cMovies.Movies(cMovies.Movies.Count - 1).NoCover = True
                    cMovies.Movies(cMovies.Movies.Count - 1).CoverCachedPath = ""
                    cMovies.Movies(cMovies.Movies.Count - 1).CoverCached = NoImage(EditInfo.OriginalMovie.Title)
                End If

                If IO.File.Exists(DestinationNoExt + ".txt") = True Then IO.File.Delete(DestinationNoExt + ".txt")
                Guardar = New IO.StreamWriter(DestinationNoExt + ".txt")
            Guardar.Write(EditInfo.OriginalMovie.ImdbID + "|" + EditInfo.OriginalMovie.Rating + "|" + EditInfo.OriginalMovie.Year + "|" + EditInfo.OriginalMovie.Duration.ToString + "|" + EditInfo.OriginalMovie.Language + "|" + EditInfo.OriginalMovie.Country + "|" + EditInfo.OriginalMovie.Genre.Replace("Music", "Musical").Replace("Musicalal", "Musical") + "|" + EditInfo.OriginalMovie.Director + "|" + EditInfo.OriginalMovie.Actor + "|" + EditInfo.OriginalMovie.Plot + "|" + EditInfo.OriginalMovie.NO_Auto_Watch.ToString + "|" + EditInfo.OriginalMovie.NO_Auto_Pending.ToString)
            Guardar.Close() : IO.File.SetAttributes(DestinationNoExt + ".txt", IO.FileAttributes.Hidden)

                cMovies.Add_Filter(cMovies.Movies.Count - 1)

                cMovies.Filter_Update_Combos()
                Get_cMovies(Mode).FilterMovies()

                For Cuenta = 0 To cMovies.Movies_F.Count - 1
                    If cMovies.Movies_F(Cuenta).Title = EditInfo.OriginalMovie.Title Then Gallery.GoToMovie(cMovies.Movies_F(Cuenta).Ruta, True) : Exit Sub
                Next
            End If
    End Sub

#End Region

#End Region

#Region " Copy / Move "

#Region " Copy "

    Private Sub MCopyTo_Click(sender As Object, e As EventArgs) Handles MCopyto.Click
        Dim cIndice As Integer = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage).Indice

        Dim Indice As Integer = -1
        For Cuenta = 0 To Movies.Movies.Count - 1
            If Movies.Movies(Cuenta).Ruta = Movies.Movies_F(cIndice).Ruta Then Indice = Cuenta : Exit For
        Next : If Indice = -1 Then Exit Sub

        If MCopyto.Text = "Copy to..." Then
            Dim OpenFolder As New FolderBrowserDialog With {.ShowNewFolderButton = True, .Description = "Copy movie to..."}
            If OpenFolder.ShowDialog = DialogResult.Cancel Then Exit Sub
            If IO.File.Exists((OpenFolder.SelectedPath + "\" + IO.Path.GetFileName(Movies.Movies(Indice).Ruta)).Replace("\\", "\")) Then
                If MsgBox("Overwrite movie '" + Movies.Movies(Indice).Title + "'?", MsgBoxStyle.Question + MsgBoxStyle.OkCancel, "K-Movies " + Version) = MsgBoxResult.Cancel Then Exit Sub
            End If
            Movies.Movies(Indice).CopyTo = (OpenFolder.SelectedPath + "\" + IO.Path.GetFileName(Movies.Movies(Indice).Ruta)).Replace("\\", "\")
            Transfering += 1
            Movies.Movies(Indice).CopyThread = New Threading.Thread(AddressOf CopyThread_Thread) : Movies.Movies(Indice).CopyThread.IsBackground = True : Movies.Movies(Indice).CopyThread.Start(Indice)
        Else
            Movies.Movies(Indice).Importing = False
            Movies.Movies(Indice).Copying = False
            Movies.Movies(Indice).Moving = False
        End If
    End Sub

#End Region

#Region " Move "

    Private Sub MMoveTo_Click(sender As Object, e As EventArgs) Handles MMoveto.Click
        Dim cIndice As Integer = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage).Indice

        Dim Indice As Integer = -1
        For Cuenta = 0 To Movies.Movies.Count - 1
            If Movies.Movies(Cuenta).Ruta = Movies.Movies_F(cIndice).Ruta Then Indice = Cuenta : Exit For
        Next : If Indice = -1 Then Exit Sub

        If MCopyto.Text = "Copy to..." Then
            Dim OpenFolder As New FolderBrowserDialog With {.ShowNewFolderButton = True, .Description = "Move movie to..."}
            If OpenFolder.ShowDialog = DialogResult.Cancel Then Exit Sub
            If IO.File.Exists((OpenFolder.SelectedPath + "\" + IO.Path.GetFileName(Movies.Movies(Indice).Ruta)).Replace("\\", "\")) Then
                If MsgBox("Overwrite movie '" + Movies.Movies(Indice).Title + "'?", MsgBoxStyle.Question + MsgBoxStyle.OkCancel, "K-Movies " + Version) = MsgBoxResult.Cancel Then Exit Sub
            End If
            Movies.Movies(Indice).Moving = True
            Movies.Movies(Indice).CopyTo = (OpenFolder.SelectedPath + "\" + IO.Path.GetFileName(Movies.Movies(Indice).Ruta)).Replace("\\", "\")
            Transfering += 1
            Movies.Movies(Indice).CopyThread = New Threading.Thread(AddressOf CopyThread_Thread) : Movies.Movies(Indice).CopyThread.IsBackground = True : Movies.Movies(Indice).CopyThread.Start(Indice)
        Else
            Movies.Movies(Indice).Importing = False
            Movies.Movies(Indice).Copying = False
            Movies.Movies(Indice).Moving = False
        End If
    End Sub

#End Region

#Region " Thread "

    Private Delegate Sub Delegate_CopyThread_Thread(Indice As Integer)
    Private Sub CopyThread_Thread(Indice As Integer)
        If InvokeRequired Then : Invoke(New Delegate_CopyThread_Thread(AddressOf CopyThread_Thread), Indice) : Else

            Dim SourceFolder As String = IO.Path.GetDirectoryName(Movies.Movies(Indice).Ruta) + "\"
        Dim DestinationFolder As String = IO.Path.GetDirectoryName(Movies.Movies(Indice).CopyTo) + "\"

        Try : Dim streamRead As New IO.FileStream(Movies.Movies(Indice).Ruta, IO.FileMode.Open)
            Dim streamWrite As New IO.FileStream(Movies.Movies(Indice).CopyTo, IO.FileMode.Create)
            Dim lngLen As Long = streamRead.Length - 1
            Dim byteBuffer(1048576) As Byte
            Dim intBytesRead As Integer

            Movies.Movies(Indice).CopySize = 0
            Movies.Movies(Indice).Copying = True

            While streamRead.Position < lngLen

                If Movies.Movies(Indice).Copying = False Then
                    streamWrite.Flush()
                    streamWrite.Close()
                    streamRead.Close()
                    IO.File.Delete(Movies.Movies(Indice).CopyTo)
                    Movies.Movies(Indice).Moving = False
                    GoTo Salir
                End If

                intBytesRead = (streamRead.Read(byteBuffer, 0, 1048576))
                streamWrite.Write(byteBuffer, 0, intBytesRead)
                Movies.Movies(Indice).CopySize = streamRead.Position
                For Each Pic As GalleryImage In Gallery.Controls
                    If Pic.Ruta = Movies.Movies(Indice).Ruta Then Pic.Invalidate() : Exit For
                Next : Application.DoEvents()

            End While

            If IO.File.Exists((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\")) = True Then IO.File.Copy((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\"), (DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).CopyTo) + ".srt").Replace("\\", "\"), True)
            If IO.File.Exists((DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).CopyTo) + ".srt").Replace("\\", "\")) = True Then IO.File.SetAttributes((DestinationFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).CopyTo) + ".srt").Replace("\\", "\"), IO.FileAttributes.Normal)
            streamWrite.Flush()
            streamWrite.Close()
            streamRead.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
            Movies.Movies(Indice).Moving = False
        End Try
Salir:  Movies.Movies(Indice).CopySize = 0
        Movies.Movies(Indice).Importing = False
        Movies.Movies(Indice).Copying = False

        If Movies.Movies(Indice).Moving = True Then
            Movies.Movies(Indice).Moving = False
            If IO.File.Exists(Movies.Movies(Indice).Ruta) = True Then IO.File.Delete(Movies.Movies(Indice).Ruta)
            If IO.File.Exists((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".jpg").Replace("\\", "\")) = True Then IO.File.Delete((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".jpg").Replace("\\", "\"))
            If IO.File.Exists((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\")) = True Then IO.File.Delete((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".srt").Replace("\\", "\"))
            If IO.File.Exists((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((SourceFolder + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".txt").Replace("\\", "\"))
            If IO.File.Exists((SourceFolder + "\cache\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".jpg").Replace("\\", "\")) = True Then IO.File.Delete((SourceFolder + "\cache\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Indice).Ruta) + ".jpg").Replace("\\", "\"))
            Movies.DeleteMovie(Indice, False, True)
        Else
            Movies.Movies(Indice).Moving = False
            For Each Pic As GalleryImage In Gallery.Controls
                If Pic.Ruta = Movies.Movies(Indice).Ruta Then Pic.Invalidate() : Exit For
            Next
        End If
        Transfering -= 1
        End If
    End Sub

#End Region

#End Region

#Region " Add To "

    Private Sub MAddTo_Click(sender As Object, e As EventArgs) Handles MPending.Click, MWatched.Click
        Dim cIndice As Integer = CType(MPMenu.Tag, GalleryImage).Indice
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        Dim tMovies As Movies_CL = Get_cMovies(sender.Text)

        If IO.File.Exists((tMovies.MoviePath + "\" + IO.Path.GetFileName(cMovies.Movies_F(cIndice).Ruta)).Replace("\\", "\")) = True Then Exit Sub

        Dim Source As String = cMovies.Movies_F(cIndice).Ruta
        If Mode = "Movies" AndAlso Movies.Movies IsNot Nothing Then
            For Cuenta = 0 To Movies.Movies.Count - 1
                If Movies.Movies(Cuenta).Ruta = cMovies.Movies_F(cIndice).Ruta Then
                    If sender.Text = "Watched" Then Movies.Movies(Cuenta).NO_Auto_Watch = True Else Movies.Movies(Cuenta).NO_Auto_Pending = True
                    If IO.File.Exists((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                    Dim Guardar2 As New IO.StreamWriter((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"))
                    Guardar2.Write(Movies.Movies(Cuenta).ImdbID + "|" + Movies.Movies(Cuenta).Rating + "|" + Movies.Movies(Cuenta).Year + "|" + Movies.Movies(Cuenta).Duration.ToString + "|" + Movies.Movies(Cuenta).Language + "|" + Movies.Movies(Cuenta).Country + "|" + Movies.Movies(Cuenta).Genre + "|" + Movies.Movies(Cuenta).Director + "|" + Movies.Movies(Cuenta).Actor + "|" + Movies.Movies(Cuenta).Plot + "|" + Movies.Movies(Cuenta).NO_Auto_Watch.ToString + "|" + Movies.Movies(Cuenta).NO_Auto_Pending.ToString)
                    Guardar2.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(Movies.Movies(Cuenta).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies.Movies(Cuenta).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
                    Exit For
                End If
            Next
        End If
        Dim Destination As String = (tMovies.MoviePath + "\" + IO.Path.GetFileName(cMovies.Movies_F(cIndice).Ruta)).Replace("\\", "\")
        Dim SourceNoExt = (IO.Path.GetDirectoryName(Source) + "\" + IO.Path.GetFileNameWithoutExtension(Source)).Replace("\\", "\")
        Dim DestinationNoExt = (IO.Path.GetDirectoryName(Destination) + "\" + IO.Path.GetFileNameWithoutExtension(Destination)).Replace("\\", "\")

        Dim Guardar As New IO.StreamWriter(DestinationNoExt + ".mp4") : Guardar.Write(cMovies.Movies_F(cIndice).Title) : Guardar.Close() : IO.File.SetAttributes(DestinationNoExt + ".mp4", IO.FileAttributes.ReadOnly)

        'jpg
        If IO.File.Exists(DestinationNoExt + ".jpg") = True Then IO.File.Delete(DestinationNoExt + ".jpg")
        If IO.File.Exists(SourceNoExt + ".jpg") = True Then
            IO.File.Copy(SourceNoExt + ".jpg", DestinationNoExt + ".jpg")
            IO.File.SetAttributes(DestinationNoExt + ".jpg", IO.FileAttributes.Hidden)
        End If
        'jpg cache
        If IO.Directory.Exists((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\")) = False Then
            IO.Directory.CreateDirectory((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\"))
            IO.File.SetAttributes((IO.Path.GetDirectoryName(Destination) + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If
        If IO.File.Exists((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"))
        If IO.File.Exists((IO.Path.GetDirectoryName(Source) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Source) + ".jpg").Replace("\\", "\")) = True Then
            IO.File.Copy((IO.Path.GetDirectoryName(Source) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Source) + ".jpg").Replace("\\", "\"), (IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"))
            IO.File.SetAttributes((IO.Path.GetDirectoryName(Destination) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Destination) + ".jpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
        End If
        'txt
        If IO.File.Exists(DestinationNoExt + ".txt") = True Then IO.File.Delete(DestinationNoExt + ".txt")
        If IO.File.Exists(SourceNoExt + ".txt") = True Then
            IO.File.Copy(SourceNoExt + ".txt", DestinationNoExt + ".txt")
            IO.File.SetAttributes(DestinationNoExt + ".txt", IO.FileAttributes.Hidden)
        End If

        If tMovies.Movies Is Nothing Then ReDim Preserve tMovies.Movies(0) Else ReDim Preserve tMovies.Movies(tMovies.Movies.Count)
        tMovies.Movies(tMovies.Movies.Count - 1) = cMovies.Movies_F(cIndice)
        tMovies.Movies(tMovies.Movies.Count - 1).Ruta = Destination
        tMovies.Movies(tMovies.Movies.Count - 1).Added = Now
        tMovies.Movies(tMovies.Movies.Count - 1).CoverCachedPath = (tMovies.MoviePath + "\cache\" + IO.Path.GetFileName(cMovies.Movies_F(cIndice).CoverCachedPath)).Replace("\\", "\")

        tMovies.Add_Filter(tMovies.Movies.Count - 1)
    End Sub

#End Region

#Region " Play "

    Private Sub Play_Random_Movie()
        Cursor.Position = PointToScreen(New Point(CMode.Left + CMode.Width + 1, CMode.Top + (CMode.Height / 2)))
        If Gallery.Controls(0).Visible = False Then Exit Sub
        Dim RandomMovie As New Random : Dim RandomMovieID As Integer = RandomMovie.Next(0, Movies.Movies_F.Count - 1)
        VLC_Timer.Enabled = False : VLC_MoviePlaying = IO.Path.GetFileName(Movies.Movies_F(RandomMovieID).Ruta) : VLC_TimePlayed = 0 : VLC_Timer.Enabled = True
        If IO.File.Exists(Player_Path) = False Then Process.Start(Movies.Movies_F(RandomMovieID).Ruta) Else Process.Start(Player_Path, "--fullscreen " + """" + Movies.Movies_F(RandomMovieID).Ruta + """")
        Opacity = 0 : Gallery.GoToMovie(Movies.Movies_F(RandomMovieID).Ruta, True) : isMinimize = True : WindowState = FormWindowState.Minimized : Opacity = 1
    End Sub

    Private Sub MPlay_Click(sender As Object, e As EventArgs) Handles MPlay.Click
        Cursor.Position = PointToScreen(New Point(CMode.Left + CMode.Width + 1, CMode.Top + (CMode.Height / 2)))
        Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        VLC_Timer.Enabled = False : VLC_MoviePlaying = IO.Path.GetFileName(Movies.Movies_F(Movie.Indice).Ruta) : VLC_TimePlayed = 0 : VLC_Timer.Enabled = True
        If IO.File.Exists(Player_Path) = False Then Process.Start(Movies.Movies_F(Movie.Indice).Ruta) Else Process.Start(Player_Path, "--fullscreen " + """" + Movies.Movies_F(Movie.Indice).Ruta + """")
    End Sub

#End Region

#Region " Delete "

    Private Sub MDelete_Click(sender As Object, e As EventArgs) Handles MDelete.Click
        Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        If Movie IsNot Nothing Then Get_cMovies(Mode).DeleteMovie(Movie.Indice)
    End Sub

#End Region

#Region " Edit Info "

    Private Sub MEdit_Info_Click(sender As Object, e As EventArgs) Handles MEdit.Click
        Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        Dim cMovies As Movies_CL = Get_cMovies(Mode)
        Dim cMovies_Selected As Integer = cMovies.Selected

        If cMovies.IMDB.Updating = True Then Exit Sub
        Dim EditInfo As New FEditInfo

        EditInfo.Opacity = 0
        EditInfo.OriginalMovie = cMovies.Movies_F(Movie.Indice)

        EditInfo.LTitle.Text = Trim(cMovies.Movies_F(Movie.Indice).Title.Split("(")(0))
        EditInfo.TSTitle.Text = Trim(cMovies.Movies_F(Movie.Indice).Title.Split("(")(0))
        EditInfo.TYear.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Year)
        If cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Rating) = "" Then EditInfo.TRating.Text = "0.0" Else EditInfo.TRating.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Rating)
        EditInfo.TDuration.Text = cMovies.Movies_F(Movie.Indice).Duration.ToString + " min"
        EditInfo.TImdb.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).ImdbID)
        EditInfo.TGenre.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Genre)
        EditInfo.TLanguage.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Language)
        EditInfo.TCountry.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Country)
        EditInfo.TDirector.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Director)
        EditInfo.TActor.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Actor)
        EditInfo.TPlot.Text = cMovies.IMDB.IMDB_Worker.CheckEmpty(cMovies.Movies_F(Movie.Indice).Plot)
        If cMovies.Movies_F(Movie.Indice).NoCover = False Then
            EditInfo.OriginalCover = LoadImageClone((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
            EditInfo.PCover.Image = EditInfo.OriginalCover
        Else
            EditInfo.OriginalCover = My.Resources.NoCover
            EditInfo.PCover.Image = EditInfo.OriginalCover
        End If
        EditInfo.ShowDialog()
        If EditInfo.Cancelado = False Then
            Dim Guardar As IO.StreamWriter
            If EditInfo.UpdateCover = True Then
                If EditInfo.NoCover = False Then
                    If IO.Directory.Exists((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache").Replace("\\", "\")) = False Then
                        IO.Directory.CreateDirectory((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache").Replace("\\", "\"))
                        IO.File.SetAttributes((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache").Replace("\\", "\"), IO.FileAttributes.Hidden)
                    End If
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".nojpg").Replace("\\", "\"))
                    EditInfo.PCover.Image.Save((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    IO.File.SetAttributes((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
                    CacheImage(EditInfo.PCover.Image, 190, 135).Save((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    cMovies.Movies_F(Movie.Indice).NoCover = False
                    cMovies.Movies_F(Movie.Indice).CoverCachedPath = (IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\")
                    cMovies.Movies_F(Movie.Indice).CoverCached = LoadImageClone((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                ElseIf EditInfo.NoCover = True Then
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".jpg").Replace("\\", "\"))
                    IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".nojpg").Replace("\\", "\"))
                    Guardar = New IO.StreamWriter((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".nojpg").Replace("\\", "\"))
                    Guardar.Write("NoCover") : Guardar.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".nojpg").Replace("\\", "\"), IO.FileAttributes.Hidden)
                    cMovies.Movies_F(Movie.Indice).NoCover = True
                    cMovies.Movies_F(Movie.Indice).CoverCachedPath = ""
                    cMovies.Movies_F(Movie.Indice).CoverCached = NoImage(cMovies.Movies_F(Movie.Indice).Title)
                End If
                For Cuenta = 0 To cMovies.Movies.Count - 1
                    If cMovies.Movies(Cuenta).Ruta = Movies.Movies_F(Movie.Indice).Ruta Then
                        cMovies.Movies(Cuenta).CoverCachedPath = Movies.Movies_F(Movie.Indice).CoverCachedPath
                        cMovies.Movies(Cuenta).CoverCached = Movies.Movies_F(Movie.Indice).CoverCached
                        cMovies.Movies(Cuenta).NoCover = Movies.Movies_F(Movie.Indice).NoCover
                        Gallery.UpdateCover(cMovies.Movies(Cuenta).Ruta)
                        Exit For
                    End If
                Next
                Gallery.UpdateCover(cMovies.Movies_F(Movie.Indice).Ruta)
            End If
            If EditInfo.UpdateFilters = True Then
                cMovies.Movies_F(Movie.Indice).Year = EditInfo.TYear.Text
                cMovies.Movies_F(Movie.Indice).Rating = EditInfo.TRating.Text
                cMovies.Movies_F(Movie.Indice).Duration = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(EditInfo.TDuration.Text, "[^\d]", ""))
                cMovies.Movies_F(Movie.Indice).ImdbID = EditInfo.TImdb.Text
                cMovies.Movies_F(Movie.Indice).Genre = EditInfo.TGenre.Text
                cMovies.Movies_F(Movie.Indice).Language = EditInfo.TLanguage.Text
                cMovies.Movies_F(Movie.Indice).Country = EditInfo.TCountry.Text
                cMovies.Movies_F(Movie.Indice).Director = EditInfo.TDirector.Text
                cMovies.Movies_F(Movie.Indice).Actor = EditInfo.TActor.Text
                cMovies.Movies_F(Movie.Indice).Plot = EditInfo.TPlot.Text
                If IO.File.Exists((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".txt").Replace("\\", "\")) = True Then IO.File.Delete((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".txt").Replace("\\", "\"))
                Guardar = New IO.StreamWriter((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".txt").Replace("\\", "\"))
                Guardar.Write(cMovies.Movies_F(Movie.Indice).ImdbID + "|" + cMovies.Movies_F(Movie.Indice).Rating + "|" + cMovies.Movies_F(Movie.Indice).Year + "|" + cMovies.Movies_F(Movie.Indice).Duration.ToString + "|" + cMovies.Movies_F(Movie.Indice).Language + "|" + cMovies.Movies_F(Movie.Indice).Country + "|" + cMovies.Movies_F(Movie.Indice).Genre + "|" + cMovies.Movies_F(Movie.Indice).Director + "|" + cMovies.Movies_F(Movie.Indice).Actor + "|" + cMovies.Movies_F(Movie.Indice).Plot + "|" + cMovies.Movies_F(Movie.Indice).NO_Auto_Watch.ToString + "|" + cMovies.Movies_F(Movie.Indice).NO_Auto_Pending.ToString)
                Guardar.Close() : IO.File.SetAttributes((IO.Path.GetDirectoryName(cMovies.Movies_F(Movie.Indice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.Movies_F(Movie.Indice).Ruta) + ".txt").Replace("\\", "\"), IO.FileAttributes.Hidden)
                For Cuenta = 0 To cMovies.Movies.Count - 1
                    If cMovies.Movies(Cuenta).Ruta = cMovies.Movies_F(Movie.Indice).Ruta Then
                        cMovies.Movies(Cuenta).Year = cMovies.Movies_F(Movie.Indice).Year
                        cMovies.Movies(Cuenta).Rating = cMovies.Movies_F(Movie.Indice).Rating
                        cMovies.Movies(Cuenta).Duration = cMovies.Movies_F(Movie.Indice).Duration
                        cMovies.Movies(Cuenta).ImdbID = cMovies.Movies_F(Movie.Indice).ImdbID
                        cMovies.Movies(Cuenta).Genre = cMovies.Movies_F(Movie.Indice).Genre
                        cMovies.Movies(Cuenta).Language = cMovies.Movies_F(Movie.Indice).Language
                        cMovies.Movies(Cuenta).Country = cMovies.Movies_F(Movie.Indice).Country
                        cMovies.Movies(Cuenta).Director = cMovies.Movies_F(Movie.Indice).Director
                        cMovies.Movies(Cuenta).Actor = cMovies.Movies_F(Movie.Indice).Actor
                        cMovies.Movies(Cuenta).Plot = cMovies.Movies_F(Movie.Indice).Plot
                        Dim FilterAfter As Boolean = cMovies.Delete_Filter(Cuenta) : cMovies.Add_Filter(Cuenta)

                        If FilterAfter = True Then cMovies.FilterMovies() Else cMovies.FilterMovies(False)
                        Exit For
                    End If
                Next
            End If
            If EditInfo.UpdateCover = True Or EditInfo.UpdateFilters = True Then
                Gallery.UpdateGallery() : SelectMovie(0)
            End If
        End If
    End Sub

#End Region

#Region " Trailer "

    Private Sub MTrailer_MouseDown(sender As Object, e As MouseEventArgs) Handles MTrailer.MouseDown
        If e.Button <> MouseButtons.Left And e.Button <> MouseButtons.Right Then Exit Sub
        Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        Threading.Thread.Sleep(150)
        MPMenu.Close()
        Dim Result As JsVideo = Youtube.Search(Get_cMovies(Mode).Movies_F(Movie.Indice).Title + " Trailer")
        If e.Button = MouseButtons.Left Then
            If Result IsNot Nothing Then
                Dim Trailer As New FTrailer
                Trailer.VideoId = Result.Items(0).ID.VideoID
                Trailer.Text = Get_cMovies(Mode).Movies_F(Movie.Indice).Title : Trailer.LTitle.Text = Trailer.Text
                Trailer.Show()
            End If
        ElseIf e.Button = MouseButtons.Right Then
            If Result IsNot Nothing Then Process.Start(("https://www.youtube.com/v/" + Result.Items(0).ID.VideoID).Replace("v/", "watch?v="))
        End If

        '("https://www.youtube.com/v/" + Result.ID.VideoID).Replace("v/", "watch?v=")

        'Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        'Dim Result As JsVideo = Youtube.Search(Get_cMovies(Mode).Movies_F(Movie.Indice).Title + " Trailer")
        'If Result IsNot Nothing Then Process.Start(Result.Items(0).ID.VideoID.Replace("v/", "watch?v="))
    End Sub

    Private Sub MTrailer_Click(sender As Object, e As EventArgs) Handles MTrailer.Click
    End Sub

#End Region

#Region " Imdb "

    Private Sub Mimdb_Click(sender As Object, e As EventArgs) Handles MImdb.Click
        Dim Movie As GalleryImage = CType(CType(CType(sender, ToolStripMenuItem).Owner, FlatContextMenuStrip).SourceControl, GalleryImage)
        Process.Start("http://www.imdb.com/title/" + Get_cMovies(Mode).Movies_F(Movie.Indice).ImdbID + "/?ref_=rvi_tt")
    End Sub

#End Region

#Region " Settings "

    Private Sub MSettings_Click(sender As Object, e As EventArgs) Handles MSettings.Click, MGSettings.Click
        Dim MoviesPathTemp As String = Movies.MoviePath
        Dim PlayerPathTemp As String = Player_Path
        Dim Settings As New FSettings
        Settings.Location = New Point(Gallery.Left + ((Gallery.Width - Settings.Width) / 2), Gallery.Top + ((Gallery.Height - Settings.Height) / 2))
        Settings.TMoviesPath.Text = Movies.MoviePath : Settings.TPlayerPath.Text = Player_Path : Settings.CAutoWatched.Checked = Auto_Watched : Settings.TMin.Text = VLC_MinPlayTime.ToString : Settings.CDeleteImported.Checked = Delete_Imported
        Settings.ShowDialog()
        If Settings.Cancelado = False Then
            Movies.MoviePath = Trim(Settings.TMoviesPath.Text) : Player_Path = Trim(Settings.TPlayerPath.Text) : Auto_Watched = Settings.CAutoWatched.Checked : Delete_Imported = Settings.CDeleteImported.Checked
            My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + Version) : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + Version, True)
            Reg.SetValue("Movies_Path", Movies.MoviePath) : Reg.SetValue("Player_Path", Player_Path) : Reg.SetValue("Auto_Watched", Auto_Watched.ToString)
            If Settings.TMin.Text = "" Then Settings.TMin.Text = "0"
            VLC_MinPlayTime = CInt(Settings.TMin.Text)
            Reg.SetValue("Auto_Watched_Min", VLC_MinPlayTime.ToString)
            Reg.SetValue("Delete_Imported", Delete_Imported.ToString) : Reg.Close()

            If IO.Directory.Exists(Movies.MoviePath) = True AndAlso MoviesPathTemp <> Movies.MoviePath Then Movies.Load_Movies()
            If IO.Directory.Exists(Movies.MoviePath) = False Then
                Movies.Movies = Nothing : Movies.Movies_F = Nothing
                Movies.Add_Filter(-1, True)
                If Mode = Movies.Group Then Movies.Filter_Update_Combos()
                Gallery.UpdateGallery()
            End If
        End If
    End Sub

#End Region

#Region " Exit "

    Private Sub MExit_Click(sender As Object, e As EventArgs) Handles MExit.Click, MGExit.Click
        End
    End Sub

#End Region

#End Region

End Class

#End Region

#Region " Movies "

Public Class Movies_CL

#Region " Variables "

#Region " Estructuras "

    Structure ST_Movie
        Dim Ruta As String
        Dim Title As String
        Dim Size As Long
        Dim Added As Date
        Dim Number As Integer
        Dim Rating As String
        Dim Year As String
        Dim Duration As Integer
        Dim Language As String
        Dim Country As String
        Dim Genre As String
        Dim Director As String
        Dim Actor As String
        Dim Plot As String
        Dim Subtitle As String
        Dim ImdbID As String
        Dim CoverCachedPath As String
        Dim CoverCached As Image
        Dim NoCover As Boolean
        Dim CopyPercent As Integer
        Dim CopySize As Long
        Dim Importing As Boolean
        Dim Copying As Boolean
        Dim Moving As Boolean
        Dim Export As Boolean
        Dim CopyTo As String
        Dim CopyThread As Threading.Thread
        Dim ImportThread As Threading.Thread
        Dim CopyUpdate As Boolean
        Dim NO_Auto_Watch As Boolean
        Dim NO_Auto_Pending As Boolean
    End Structure

    Structure ST_Filter
        Dim Year As ComboBox
        Dim Genre As ComboBox
        Dim Language As ComboBox
        Dim Country As ComboBox
        Dim Director As ComboBox
        Dim Actor As ComboBox

        Dim Rating As String
        Dim Rating_Mode As String
        Dim Duration As String
        Dim Duration_Mode As String
        Dim Search As String

        Dim Sorted As String
        Dim NOaz As Boolean
        Dim Updating As Boolean
    End Structure

    Structure ST_IMDB
        Dim Updating As Boolean
        Dim IMDB_BWorker As System.ComponentModel.BackgroundWorker
        Dim IMDB_Worker As IMDB_Worker
        Dim IMDB_List As ListBox
        Dim Indice As Integer
    End Structure

#End Region

    Public Group As String
    Public MoviePath As String
    Public Movies() As ST_Movie
    Public Movies_F() As ST_Movie
    Public Selected As Integer
    Public MoviesLoading As Boolean
    Public Filter As ST_Filter
    Public IMDB As ST_IMDB
    Public Zoom As Integer
    Public Page As Integer
    Public MoviesPerPage As Integer = 28

#Region " Constructor "

    Sub New(cGroup As String)
        Group = cGroup
        Filter.Year = New ComboBox
        Filter.Genre = New ComboBox
        Filter.Language = New ComboBox
        Filter.Country = New ComboBox
        Filter.Director = New ComboBox
        Filter.Actor = New ComboBox
        IMDB.IMDB_List = New ListBox
        IMDB.IMDB_Worker = New IMDB_Worker
        IMDB.IMDB_BWorker = New ComponentModel.BackgroundWorker With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}
        RemoveHandler IMDB.IMDB_BWorker.DoWork, AddressOf IMDB_BWorker_DoWork
        RemoveHandler IMDB.IMDB_BWorker.ProgressChanged, AddressOf IMDB_BWorker_ProgressChanged
        AddHandler IMDB.IMDB_BWorker.DoWork, AddressOf IMDB_BWorker_DoWork
        AddHandler IMDB.IMDB_BWorker.ProgressChanged, AddressOf IMDB_BWorker_ProgressChanged

        My.Computer.Registry.CurrentUser.CreateSubKey("Software\KMovies " + "4.0") : Dim Reg = My.Computer.Registry.CurrentUser.OpenSubKey("Software\KMovies " + "4.0", True)
        If Reg.GetValue(Group + "_Zoom") Is Nothing Then Reg.SetValue(Group + "_Zoom", "4")
        If Reg.GetValue(Group + "_Sorted") Is Nothing Then Reg.SetValue(Group + "_Sorted", "Name")
        If Reg.GetValue(Group + "_NOaz") Is Nothing Then Reg.SetValue(Group + "_NOaz", "False")
        Zoom = Reg.GetValue(Group + "_Zoom")
        Filter.Sorted = Reg.GetValue(Group + "_Sorted")
        Filter.NOaz = CBool(Reg.GetValue(Group + "_NOaz"))

        If Group = "Movies" Then
            If Reg.GetValue(Group + "_Path") Is Nothing Then Reg.SetValue(Group + "_Path", "")
            MoviePath = Reg.GetValue(Group + "_Path")
        Else
            MoviePath = (Application.StartupPath + "\KMLocal\" + Group).Replace("\\", "\") : IO.Directory.CreateDirectory(MoviePath)
        End If
        Reg.Close()
    End Sub

#End Region

#End Region

#Region " Movies "

#Region " Load "

    Public Sub Load_Movies()
        If MoviesLoading = True Then Exit Sub Else MoviesLoading = True
        FGallery.Create_Folders() : Delete_Filter(-99)
        Try
            Dim TotalMovies As Integer
            If IO.Directory.Exists(MoviePath) = True Then TotalMovies = IO.Directory.GetFiles(MoviePath, "*.avi").Count + IO.Directory.GetFiles(MoviePath, "*.mp4").Count + IO.Directory.GetFiles(MoviePath, "*.mkv").Count

            If TotalMovies = 0 Then
                MoviesLoading = False
                Movies = Nothing : Movies_F = Nothing
                Add_Filter(-1, True)
                If FGallery.Mode = Group Then Filter_Update_Combos()
                FGallery.Gallery.UpdateGallery()
                Exit Sub
            End If

            Dim cIndice As Integer = 0 : ReDim Movies(TotalMovies - 1)
            Dim SacaNombre As String = Dir((MoviePath + "\*.*").Replace("\\", "\"), FileAttribute.Normal)
            Do Until SacaNombre = ""
                If IO.Path.GetExtension(SacaNombre) = ".avi" Or IO.Path.GetExtension(SacaNombre) = ".mp4" Or IO.Path.GetExtension(SacaNombre) = ".mkv" Then
                    'mp4
                    Movies(cIndice).Ruta = (MoviePath + "\" + SacaNombre).Replace("\\", "\")
                    Movies(cIndice).Title = IO.Path.GetFileNameWithoutExtension(Movies(cIndice).Ruta)
                    Movies(cIndice).Size = My.Computer.FileSystem.GetFileInfo(Movies(cIndice).Ruta).Length
                    Movies(cIndice).Added = IO.File.GetLastWriteTime(Movies(cIndice).Ruta)
                    Dim Movie_Path As String = MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(Movies(cIndice).Ruta).Replace("\\", "\")
                    Dim Movie_Path_Cache As String = MoviePath + "\cache\" + IO.Path.GetFileNameWithoutExtension(Movies(cIndice).Ruta).Replace("\\", "\")
                    'jpg
                    If IO.File.Exists(Movie_Path + ".jpg") = True And IO.File.Exists(Movie_Path_Cache + ".jpg") = False Then FGallery.CacheImage(FGallery.LoadImageClone(Movie_Path + ".jpg"), 190, 135).Save(Movie_Path_Cache + ".jpg")
                    Movies(cIndice).CoverCachedPath = Movie_Path_Cache + ".jpg"
                    If IO.File.Exists(Movies(cIndice).CoverCachedPath) = True Then IO.File.SetAttributes(Movies(cIndice).CoverCachedPath, IO.FileAttributes.Hidden)
                    If IO.File.Exists(Movies(cIndice).CoverCachedPath) = False Then Movies(cIndice).NoCover = True : Movies(cIndice).CoverCached = FGallery.NoImage(Movies(cIndice).Title)
                    'srt, ssa
                    If IO.File.Exists(Movie_Path + ".srt") = True Or IO.File.Exists(Movie_Path + ".ssa") = True Then Movies(cIndice).Subtitle = "Yes" Else Movies(cIndice).Subtitle = "No"
                    'txt
                    If Movies(cIndice).Year Is Nothing Then Movies(cIndice).Year = ""
                    If Movies(cIndice).Genre Is Nothing Then Movies(cIndice).Genre = ""
                    If Movies(cIndice).Language Is Nothing Then Movies(cIndice).Language = ""
                    If Movies(cIndice).Country Is Nothing Then Movies(cIndice).Country = ""
                    If Movies(cIndice).Director Is Nothing Then Movies(cIndice).Director = ""
                    If Movies(cIndice).Actor Is Nothing Then Movies(cIndice).Actor = ""
                    If Movies(cIndice).Language Is Nothing Then Movies(cIndice).Language = ""
                    If Movies(cIndice).Plot Is Nothing Then Movies(cIndice).Plot = ""
                    If Movies(cIndice).Rating Is Nothing Then Movies(cIndice).Rating = "0.0"

                    If IO.File.Exists(Movie_Path + ".txt") = True Then LoadInfo_TXT(cIndice)
                    Add_Filter(cIndice)
                    cIndice += 1
                End If : SacaNombre = Dir()
            Loop

            Movies_F = Movies.Clone : MoviesLoading = False : GetMovieInfo()

            If FGallery.Mode = Group Then
                Filter_Update_Combos()
                FilterMovies()
                FGallery.LGallery.Visible = False
                FGallery.SelectMovie(Selected)
                FGallery.THidden.Focus()
            End If

        Catch : MoviesLoading = False : End Try
    End Sub

#Region " Load TXT "

    Public Sub LoadInfo_TXT(cIndex As Integer)
        Try : Dim Leer As IO.StreamReader : Dim DataArray(), LanguageTemp() As String
            Leer = New IO.StreamReader((IO.Path.GetDirectoryName(Movies(cIndex).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies(cIndex).Ruta)).Replace("\\", "\") + ".txt")
            DataArray = Leer.ReadToEnd.Split("|")
            Movies(cIndex).ImdbID = DataArray(0) : If Movies(cIndex).ImdbID Is Nothing Then Movies(cIndex).ImdbID = ""
            Movies(cIndex).Rating = DataArray(1) : If Movies(cIndex).Rating Is Nothing Or Movies(cIndex).Rating = "N/A" Then Movies(cIndex).Rating = "0.0"
            Movies(cIndex).Year = DataArray(2) : If Movies(cIndex).Year Is Nothing Then Movies(cIndex).Year = ""
            If DataArray(3) = "N/A" Or DataArray(3) = "" Then Movies(cIndex).Duration = 0 Else Movies(cIndex).Duration = Integer.Parse(System.Text.RegularExpressions.Regex.Replace(DataArray(3), "[^\d]", ""))
            Movies(cIndex).Language = ""
            If DataArray(4).Contains(" Sign Language") Then : LanguageTemp = DataArray(4).Split(", ")
                For Cuenta = 0 To LanguageTemp.Count - 1
                    If Movies(cIndex).Language.Contains(LanguageTemp(Cuenta).Replace(" Sign Language", "")) = False Then Movies(cIndex).Language += ", " + LanguageTemp(Cuenta).Replace(" Sign Language", "")
                Next : Movies(cIndex).Language = Movies(cIndex).Language.Substring(2)
            Else
                Movies(cIndex).Language = DataArray(4)
            End If : If Movies(cIndex).Language Is Nothing Then Movies(cIndex).Language = ""
            Movies(cIndex).Country = DataArray(5) : If Movies(cIndex).Country Is Nothing Then Movies(cIndex).Country = ""
            Movies(cIndex).Genre = DataArray(6) : If Movies(cIndex).Genre Is Nothing Then Movies(cIndex).Genre = ""
            Movies(cIndex).Director = DataArray(7).Replace("(co-director)", "") : If Movies(cIndex).Director Is Nothing Then Movies(cIndex).Director = ""
            Movies(cIndex).Actor = DataArray(8) : If Movies(cIndex).Actor Is Nothing Then Movies(cIndex).Actor = ""
            Movies(cIndex).Plot = DataArray(9) : If Movies(cIndex).Plot Is Nothing Then Movies(cIndex).Plot = ""
            If DataArray.Count > 10 Then Movies(cIndex).NO_Auto_Watch = DataArray(10)
            If DataArray.Count > 11 Then Movies(cIndex).NO_Auto_Pending = DataArray(11)
            Leer.Close()
        Catch : End Try
    End Sub

#End Region

#End Region

#Region " Delete "

    Public Sub DeleteMovie(cIndice As Integer, Optional Msg As Boolean = True, Optional isMovie As Boolean = False)
        Dim mIndice As Integer = -1
        Dim fIndice As Integer = -1

        If isMovie = False Then
            fIndice = cIndice
            For Cuenta = 0 To Movies.Count - 1
                If Movies(Cuenta).Ruta = Movies_F(cIndice).Ruta Then mIndice = Cuenta : Exit For
            Next
        Else
            mIndice = cIndice
            If mIndice >= Movies.Count Then Exit Sub
            For Cuenta = 0 To Movies_F.Count - 1
                If Movies_F(Cuenta).Ruta = Movies(cIndice).Ruta Then fIndice = Cuenta : Exit For
            Next
        End If

        If mIndice = -1 Then Exit Sub

        If Msg = True Then
            Dim NoMoviesMSG As String = " from " + Group : If Group = "Movies" Then NoMoviesMSG = ""
            If MsgBox("Delete '" + Movies(mIndice).Title + "'" + NoMoviesMSG + "?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "K-Movies " + FGallery.Version) = MsgBoxResult.No Then Exit Sub
        End If

        Dim DestinationNoExt As String = (IO.Path.GetDirectoryName(Movies(mIndice).Ruta) + "\" + IO.Path.GetFileNameWithoutExtension(Movies(mIndice).Ruta)).Replace("\\", "\")
        Dim DestinationCacheNoExt As String = (IO.Path.GetDirectoryName(Movies(mIndice).Ruta) + "\cache\" + IO.Path.GetFileNameWithoutExtension(Movies(mIndice).Ruta)).Replace("\\", "\")

        If IO.File.Exists(DestinationCacheNoExt + ".jpg") = True Then IO.File.Delete(DestinationCacheNoExt + ".jpg")
        If IO.File.Exists(DestinationNoExt + ".jpg") = True Then IO.File.Delete(DestinationNoExt + ".jpg")
        If IO.File.Exists(DestinationNoExt + ".nojpg") = True Then IO.File.Delete(DestinationNoExt + ".nojpg")
        If IO.File.Exists(DestinationNoExt + ".txt") = True Then IO.File.Delete(DestinationNoExt + ".txt")
        If IO.File.Exists(DestinationNoExt + ".avi") = True Then My.Computer.FileSystem.DeleteFile(DestinationNoExt + ".avi", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        If IO.File.Exists(DestinationNoExt + ".mp4") = True Then My.Computer.FileSystem.DeleteFile(DestinationNoExt + ".mp4", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        If IO.File.Exists(DestinationNoExt + ".mkv") = True Then My.Computer.FileSystem.DeleteFile(DestinationNoExt + ".mkv", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
        If IO.File.Exists(DestinationNoExt + ".srt") = True Then My.Computer.FileSystem.DeleteFile(DestinationNoExt + ".srt", FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)

        Dim FilterAfter As Boolean = Delete_Filter(mIndice)
        RemoveMovie(Movies, mIndice)

        If fIndice <> -1 Then RemoveMovie(Movies_F, fIndice)

        FilterMovies()

        If Selected >= Movies_F.Count Then Selected = Movies_F.Count - 1
        If Selected = -1 Then Selected = 0
        If FGallery.Controls(0).Visible = False Then
            FGallery.SelectMovie(-99)
        Else
            If Movies_F IsNot Nothing AndAlso Movies_F.Count > 0 Then FGallery.Gallery.GoToMovie(Movies_F(Selected).Ruta, True)
        End If
    End Sub

    Private Sub RemoveMovie(Of T)(ByRef Array_Name() As T, Index As Integer)
        Array.Copy(Array_Name, Index + 1, Array_Name, Index, UBound(Array_Name) - Index)
        ReDim Preserve Array_Name(UBound(Array_Name) - 1)
    End Sub

#End Region

#End Region

#Region " Filters "

#Region " Add "

    Public Sub Add_Filter(cIndice As Integer, Optional InsertAll As Boolean = False)
        If InsertAll = True Then
            Filter.Year.Sorted = False : Filter.Year.Items.Insert(0, "All")
            Filter.Language.Sorted = False : Filter.Language.Items.Insert(0, "All")
            Filter.Country.Sorted = False : Filter.Country.Items.Insert(0, "All")
            Filter.Director.Sorted = False : Filter.Director.Items.Insert(0, "All")
            Filter.Actor.Sorted = False : Filter.Actor.Items.Insert(0, "All")
            Filter.Genre.Sorted = False : Filter.Genre.Items.Insert(0, "All")
            Exit Sub
        End If

        If Movies.Count <= cIndice Or Movies.Count = 0 Then Exit Sub

        If Movies(cIndice).Year Is Nothing Then Movies(cIndice).Year = ""
        If Movies(cIndice).Genre Is Nothing Then Movies(cIndice).Genre = ""
        If Movies(cIndice).Language Is Nothing Then Movies(cIndice).Language = ""
        If Movies(cIndice).Country Is Nothing Then Movies(cIndice).Country = ""
        If Movies(cIndice).Director Is Nothing Then Movies(cIndice).Director = ""
        If Movies(cIndice).Actor Is Nothing Then Movies(cIndice).Actor = ""

        Dim CYearIndex As Integer = Filter.Year.SelectedIndex : Dim CYearItem As String = Filter.Year.Text
        Dim CGenreIndex As Integer = Filter.Genre.SelectedIndex : Dim CGenreItem As String = Filter.Genre.Text
        Dim CLanguageIndex As Integer = Filter.Language.SelectedIndex : Dim CLanguageItem As String = Filter.Language.Text
        Dim CCountryIndex As Integer = Filter.Country.SelectedIndex : Dim CCountryItem As String = Filter.Country.Text
        Dim CDirectorIndex As Integer = Filter.Director.SelectedIndex : Dim CDirectorItem As String = Filter.Director.Text
        Dim CActorItem As String = Filter.Actor.Text : Dim CActorIndex As Integer = Filter.Actor.SelectedIndex

        Filter.Year.BeginUpdate()
        Filter.Genre.BeginUpdate()
        Filter.Language.BeginUpdate()
        Filter.Country.BeginUpdate()
        Filter.Director.BeginUpdate()
        Filter.Actor.BeginUpdate()

        If Filter.Year.Items.Count > 0 AndAlso Filter.Year.Items(0) = "All" Then Filter.Year.Items.RemoveAt(0)
        If Filter.Genre.Items.Count > 0 AndAlso Filter.Genre.Items(0) = "All" Then Filter.Genre.Items.RemoveAt(0)
        If Filter.Language.Items.Count > 0 AndAlso Filter.Language.Items(0) = "All" Then Filter.Language.Items.RemoveAt(0)
        If Filter.Country.Items.Count > 0 AndAlso Filter.Country.Items(0) = "All" Then Filter.Country.Items.RemoveAt(0)
        If Filter.Director.Items.Count > 0 AndAlso Filter.Director.Items(0) = "All" Then Filter.Director.Items.RemoveAt(0)
        If Filter.Actor.Items.Count > 0 AndAlso Filter.Actor.Items(0) = "All" Then Filter.Actor.Items.RemoveAt(0)

        Filter.Year.Sorted = True
        Filter.Genre.Sorted = True
        Filter.Language.Sorted = True
        Filter.Country.Sorted = True
        Filter.Director.Sorted = True
        Filter.Actor.Sorted = True

        Dim SacaArray() As String
        If Not Filter.Year.Items.Contains(Movies(cIndice).Year) And Movies(cIndice).Year <> "" And Movies(cIndice).Year <> "N/A" Then Filter.Year.Items.Add(Movies(cIndice).Year)
        SacaArray = Movies(cIndice).Genre.Split(", ") : For Cuenta2 = 0 To SacaArray.Length - 1
            If Not Filter.Genre.Items.Contains(Trim(SacaArray(Cuenta2))) And Trim(SacaArray(Cuenta2)) <> "" And Trim(SacaArray(Cuenta2)) <> "N/A" Then Filter.Genre.Items.Add(Trim(SacaArray(Cuenta2)))
        Next
        SacaArray = Movies(cIndice).Language.Split(", ") : For Cuenta2 = 0 To SacaArray.Length - 1
            If Not Filter.Language.Items.Contains(Trim(SacaArray(Cuenta2))) And Trim(SacaArray(Cuenta2)) <> "" And Trim(SacaArray(Cuenta2)) <> "N/A" Then Filter.Language.Items.Add(Trim(SacaArray(Cuenta2)))
        Next
        SacaArray = Movies(cIndice).Country.Split(", ") : For Cuenta2 = 0 To SacaArray.Length - 1
            If Not Filter.Country.Items.Contains(Trim(SacaArray(Cuenta2))) And Trim(SacaArray(Cuenta2)) <> "" And Trim(SacaArray(Cuenta2)) <> "N/A" Then Filter.Country.Items.Add(Trim(SacaArray(Cuenta2)))
        Next
        SacaArray = Movies(cIndice).Director.Split(", ") : For Cuenta2 = 0 To SacaArray.Length - 1
            If Not Filter.Director.Items.Contains(Trim(SacaArray(Cuenta2))) And Trim(SacaArray(Cuenta2)) <> "" And Trim(SacaArray(Cuenta2)) <> "N/A" Then Filter.Director.Items.Add(Trim(SacaArray(Cuenta2)))
        Next
        SacaArray = Movies(cIndice).Actor.Split(", ") : For Cuenta2 = 0 To SacaArray.Length - 1
            If Not Filter.Actor.Items.Contains(Trim(SacaArray(Cuenta2))) And Trim(SacaArray(Cuenta2)) <> "" And Trim(SacaArray(Cuenta2)) <> "N/A" Then Filter.Actor.Items.Add(Trim(SacaArray(Cuenta2)))
        Next

        Filter.Year.Sorted = False : Filter.Year.Items.Insert(0, "All")
        Filter.Language.Sorted = False : Filter.Language.Items.Insert(0, "All")
        Filter.Country.Sorted = False : Filter.Country.Items.Insert(0, "All")
        Filter.Director.Sorted = False : Filter.Director.Items.Insert(0, "All")
        Filter.Actor.Sorted = False : Filter.Actor.Items.Insert(0, "All")
        Filter.Genre.Sorted = False : Filter.Genre.Items.Insert(0, "All")

        Filter.Year.SelectedItem = CYearItem : Filter.Genre.SelectedItem = CGenreItem : Filter.Language.SelectedItem = CLanguageItem : Filter.Country.SelectedItem = CCountryItem : Filter.Director.SelectedItem = CDirectorItem : Filter.Actor.SelectedItem = CActorItem
        Filter.Year.EndUpdate() : Filter.Language.EndUpdate() : Filter.Country.EndUpdate() : Filter.Director.EndUpdate() : Filter.Actor.EndUpdate() : Filter.Genre.EndUpdate()
    End Sub

#End Region

#Region " Delete "

    Public Function Delete_Filter(cIndice As String) As Boolean
        If cIndice = -99 Then ' Clear Filters
            Filter.Year.BeginUpdate() : Filter.Year.Items.Clear() : Filter.Year.Sorted = True
            Filter.Genre.BeginUpdate() : Filter.Genre.Items.Clear() : Filter.Genre.Sorted = True
            Filter.Language.BeginUpdate() : Filter.Language.Items.Clear() : Filter.Language.Sorted = True
            Filter.Country.BeginUpdate() : Filter.Country.Items.Clear() : Filter.Country.Sorted = True
            Filter.Director.BeginUpdate() : Filter.Director.Items.Clear() : Filter.Director.Sorted = True
            Filter.Actor.BeginUpdate() : Filter.Actor.Items.Clear() : Filter.Actor.Sorted = True
            Filter.Rating_Mode = "Greater than" : Filter.Rating = "0.0"
            Filter.Duration_Mode = "Longer than" : Filter.Duration = "0 min"
            Filter.Search = "Search for a movie"
            Return True
        End If

        Dim RemoveYear As String = Movies(cIndice).Year
        Dim RemoveGenre As String = Movies(cIndice).Genre
        Dim RemoveLanguage As String = Movies(cIndice).Language
        Dim RemoveCountry As String = Movies(cIndice).Country
        Dim RemoveDirector As String = Movies(cIndice).Director
        Dim RemoveActor As String = Movies(cIndice).Actor
        Dim SacaArray() As String
        Dim FilterMoviesAfter As Boolean

        For Cuenta = 0 To Movies.Count - 1
            If Movies(Cuenta).Ruta <> Movies(cIndice).Ruta Then
                If RemoveYear <> "" AndAlso Movies(Cuenta).Year = RemoveYear Then RemoveYear = RemoveYear.Replace(RemoveYear, "")
                SacaArray = Movies(Cuenta).Genre.Split(", ") : For Cuenta2 = 0 To SacaArray.Count - 1
                    If SacaArray(Cuenta2) <> "" AndAlso RemoveGenre.Contains(Trim(SacaArray(Cuenta2))) Then RemoveGenre = RemoveGenre.Replace(SacaArray(Cuenta2), "")
                Next
                SacaArray = Movies(Cuenta).Language.Split(", ") : For Cuenta2 = 0 To SacaArray.Count - 1
                    If SacaArray(Cuenta2) <> "" AndAlso RemoveLanguage.Contains(Trim(SacaArray(Cuenta2))) Then RemoveLanguage = RemoveLanguage.Replace(SacaArray(Cuenta2), "")
                Next
                SacaArray = Movies(Cuenta).Country.Split(", ") : For Cuenta2 = 0 To SacaArray.Count - 1
                    If SacaArray(Cuenta2) <> "" AndAlso RemoveCountry.Contains(Trim(SacaArray(Cuenta2))) Then RemoveCountry = RemoveCountry.Replace(SacaArray(Cuenta2), "")
                Next
                SacaArray = Movies(Cuenta).Director.Split(", ") : For Cuenta2 = 0 To SacaArray.Count - 1
                    If SacaArray(Cuenta2) <> "" AndAlso RemoveDirector.Contains(Trim(SacaArray(Cuenta2))) Then RemoveDirector = RemoveDirector.Replace(SacaArray(Cuenta2), "")
                Next
                SacaArray = Movies(Cuenta).Actor.Split(", ") : For Cuenta2 = 0 To SacaArray.Count - 1
                    If SacaArray(Cuenta2) <> "" AndAlso RemoveActor.Contains(Trim(SacaArray(Cuenta2))) Then RemoveActor = RemoveActor.Replace(SacaArray(Cuenta2), "")
                Next
            End If
        Next

        Filter.Year.Items.Remove(RemoveYear)
        If Group = FGallery.Mode Then
            FGallery.CYear.Items.Remove(RemoveYear)
            If Filter.Year.Text = RemoveYear Then FilterMoviesAfter = True : Filter.Year.SelectedIndex = 0 : FGallery.CYear.SelectedIndex = 0 : FGallery.LCYear.Text = FGallery.CYear.Text
        End If
        SacaArray = RemoveGenre.Split(", ") : For Cuenta = 0 To SacaArray.Count - 1
            Filter.Genre.Items.Remove(Trim(SacaArray(Cuenta)))
            If Group = FGallery.Mode Then
                FGallery.CGenre.Items.Remove(Trim(SacaArray(Cuenta)))
                If Filter.Genre.Items.Count = 1 OrElse Filter.Genre.Text = Trim(SacaArray(Cuenta)) Then FilterMoviesAfter = True : Filter.Genre.SelectedIndex = 0 : FGallery.CGenre.SelectedIndex = 0 : FGallery.LCGenre.Text = FGallery.CGenre.Text
            End If
        Next
        SacaArray = RemoveLanguage.Split(", ") : For Cuenta = 0 To SacaArray.Count - 1
            Filter.Language.Items.Remove(Trim(SacaArray(Cuenta)))
            If Group = FGallery.Mode Then
                FGallery.CLanguage.Items.Remove(Trim(SacaArray(Cuenta)))
                If Filter.Language.Items.Count = 1 OrElse Filter.Language.Text = Trim(SacaArray(Cuenta)) Then FilterMoviesAfter = True : Filter.Language.SelectedIndex = 0 : FGallery.CLanguage.SelectedIndex = 0 : FGallery.LCLanguage.Text = FGallery.CLanguage.Text
            End If
        Next
        SacaArray = RemoveCountry.Split(", ") : For Cuenta = 0 To SacaArray.Count - 1
            Filter.Country.Items.Remove(Trim(SacaArray(Cuenta)))
            If Group = FGallery.Mode Then
                FGallery.CCountry.Items.Remove(Trim(SacaArray(Cuenta)))
                If Filter.Country.Items.Count = 1 OrElse Filter.Country.Text = Trim(SacaArray(Cuenta)) Then FilterMoviesAfter = True : Filter.Country.SelectedIndex = 0 : FGallery.CCountry.SelectedIndex = 0 : FGallery.LCCountry.Text = FGallery.CCountry.Text
            End If
        Next
        SacaArray = RemoveDirector.Split(", ") : For Cuenta = 0 To SacaArray.Count - 1
            Filter.Director.Items.Remove(Trim(SacaArray(Cuenta)))
            If Group = FGallery.Mode Then
                FGallery.CDirector.Items.Remove(Trim(SacaArray(Cuenta)))
                If Filter.Director.Items.Count = 1 OrElse Filter.Director.Text = Trim(SacaArray(Cuenta)) Then FilterMoviesAfter = True : Filter.Director.SelectedIndex = 0 : FGallery.CDirector.SelectedIndex = 0 : FGallery.LCDirector.Text = FGallery.CDirector.Text
            End If
        Next
        SacaArray = RemoveActor.Split(", ") : For Cuenta = 0 To SacaArray.Count - 1
            Filter.Actor.Items.Remove(Trim(SacaArray(Cuenta)))
            If Group = FGallery.Mode Then
                FGallery.CActor.Items.Remove(Trim(SacaArray(Cuenta)))
                If Filter.Actor.Items.Count = 1 OrElse Filter.Actor.Text = Trim(SacaArray(Cuenta)) Then FilterMoviesAfter = True : Filter.Actor.SelectedIndex = 0 : FGallery.CActor.SelectedIndex = 0 : FGallery.LCActor.Text = FGallery.CActor.Text
            End If
        Next
        Return FilterMoviesAfter
    End Function

#End Region

#Region " Filter "

    Public Sub FilterMovies(Optional UpdateGallery As Boolean = True)
        Dim Movies_Temp(0) As ST_Movie : Dim cIndice As Integer = -1

        If Movies Is Nothing OrElse Movies.Count = 0 Then
            If FGallery.Mode = Group AndAlso UpdateGallery = True Then FGallery.Gallery.UpdateGallery()
            Exit Sub
        End If
        For Cuenta = 0 To Movies.Count - 1
            If Movies(Cuenta).Year Is Nothing Then Movies(Cuenta).Year = ""
            If Movies(Cuenta).Genre Is Nothing Then Movies(Cuenta).Genre = ""
            If Movies(Cuenta).Language Is Nothing Then Movies(Cuenta).Language = ""
            If Movies(Cuenta).Country Is Nothing Then Movies(Cuenta).Country = ""
            If Movies(Cuenta).Director Is Nothing Then Movies(Cuenta).Director = ""
            If Movies(Cuenta).Actor Is Nothing Then Movies(Cuenta).Actor = ""
            If Movies(Cuenta).Rating Is Nothing Then Movies(Cuenta).Rating = "0.0"
            If Movies(Cuenta).ImdbID Is Nothing Then Movies(Cuenta).ImdbID = ""
            If Movies(Cuenta).Duration = Nothing Then Movies(Cuenta).Duration = 0

            If FGallery.CYear.Text <> "All" And Movies(Cuenta).Year Is Nothing Then GoTo Jump Else If FilterMovie("Year", Cuenta) = False Then GoTo Jump
            If FGallery.CGenre.Text <> "All" And Movies(Cuenta).Genre Is Nothing Then GoTo Jump Else If FilterMovie("Genre", Cuenta) = False Then GoTo Jump
            If FGallery.CLanguage.Text <> "All" And Movies(Cuenta).Language Is Nothing Then GoTo Jump Else If FilterMovie("Language", Cuenta) = False Then GoTo Jump
            If FGallery.CCountry.Text <> "All" And Movies(Cuenta).Country Is Nothing Then GoTo Jump Else If FilterMovie("Country", Cuenta) = False Then GoTo Jump
            If FGallery.CDirector.Text <> "All" And Movies(Cuenta).Director Is Nothing Then GoTo Jump Else If FilterMovie("Director", Cuenta) = False Then GoTo Jump
            If FGallery.CActor.Text <> "All" And Movies(Cuenta).Actor Is Nothing Then GoTo Jump Else If FilterMovie("Actor", Cuenta) = False Then GoTo Jump
            If FGallery.TSearch.Text <> "Search for a movie" And FGallery.TSearch.Text <> "" Then If FilterMovie("Title", Cuenta) = False Then GoTo Jump
            If Movies(Cuenta).Rating Is Nothing Or Movies(Cuenta).Rating = "N/A" Then GoTo Jump Else If FilterMovie("Rating", Cuenta) = False Then GoTo Jump
            If FilterMovie("Duration", Cuenta) = False Then GoTo Jump
            cIndice += 1 : ReDim Preserve Movies_Temp(cIndice) : Movies_Temp(cIndice) = Movies(Cuenta)
Jump:   Next : Movies_F = Movies_Temp

        SortGallery()
        If FGallery.Mode = Group AndAlso UpdateGallery = True Then FGallery.Gallery.UpdateGallery()
    End Sub

    Private Function FilterMovie(Filter As String, cIndice As Integer) As Boolean
        Dim cFilter As Boolean
        If Movies(cIndice).Title = "[BORRADO]" Then
            Return False
        End If
        Select Case Filter
            Case "Year" : If Movies(cIndice).Year IsNot Nothing AndAlso (Movies(cIndice).Year.ToLower.Contains(FGallery.CYear.Text.ToLower) Or FGallery.CYear.Text = "All") Then cFilter = True Else cFilter = False
            Case "Genre" : If Movies(cIndice).Genre IsNot Nothing AndAlso (Movies(cIndice).Genre.ToLower.Contains(FGallery.CGenre.Text.ToLower) Or FGallery.CGenre.Text = "All") Then cFilter = True Else cFilter = False
            Case "Language" : If Movies(cIndice).Language IsNot Nothing AndAlso (Movies(cIndice).Language.ToLower.Contains(FGallery.CLanguage.Text.ToLower) Or FGallery.CLanguage.Text = "All") Then cFilter = True Else cFilter = False
            Case "Country" : If Movies(cIndice).Country IsNot Nothing AndAlso (Movies(cIndice).Country.ToLower.Contains(FGallery.CCountry.Text.ToLower) Or FGallery.CCountry.Text = "All") Then cFilter = True Else cFilter = False
            Case "Director" : If Movies(cIndice).Director IsNot Nothing AndAlso (Movies(cIndice).Director.ToLower.Contains(FGallery.CDirector.Text.ToLower) Or FGallery.CDirector.Text = "All") Then cFilter = True Else cFilter = False
            Case "Actor" : If Movies(cIndice).Actor IsNot Nothing AndAlso (Movies(cIndice).Actor.ToLower.Contains(FGallery.CActor.Text.ToLower) Or FGallery.CActor.Text = "All") Then cFilter = True Else cFilter = False
            Case "Title" : If Movies(cIndice).Title IsNot Nothing AndAlso Movies(cIndice).Title.ToLower.Contains(Trim(FGallery.TSearch.Text.ToLower)) Then cFilter = True Else cFilter = False
            Case "Rating" : If Movies(cIndice).Rating Is Nothing Then cFilter = False
                If FGallery.TRating.Text = "" Then FGallery.TRating.Text = "0.0"
                Dim Rating As Decimal : If FGallery.TRating.Text = "10" Then Rating = "10.0" Else Rating = CDec(FGallery.TRating.Text)
                Select Case FGallery.LFRatingThan.Text
                    Case "Greater than" : If CDec(Movies(cIndice).Rating) >= Rating Then cFilter = True
                    Case "Smaller than" : If CDec(Movies(cIndice).Rating) <= Rating Then cFilter = True
                    Case "Equal to" : If CDec(Movies(cIndice).Rating) = Rating Then cFilter = True
                End Select
            Case "Duration"
                Select Case FGallery.LFDurationThan.Text
                    Case "Longer than"
                        If Movies(cIndice).Duration >= Integer.Parse(Text.RegularExpressions.Regex.Replace(FGallery.TDuration.Text, "[^\d]", "")) Then cFilter = True
                    Case "Shorter than"
                        If Movies(cIndice).Duration <= Integer.Parse(Text.RegularExpressions.Regex.Replace(FGallery.TDuration.Text, "[^\d]", "")) Then cFilter = True
                    Case "Equal to"
                        If Movies(cIndice).Duration = Integer.Parse(Text.RegularExpressions.Regex.Replace(FGallery.TDuration.Text, "[^\d]", "")) Then cFilter = True
                End Select
        End Select
        If FGallery.LTransfer.Tag = "Transfering" Then
            If cFilter = True Then If Movies(cIndice).Copying = True Then Return True Else Return False
        Else
            Return cFilter
        End If
        Return cFilter
    End Function

#End Region

#Region " Update Combos "

    Public Sub Filter_Update_Combos()

        If Filter.Year.SelectedIndex = -1 Then Filter.Year.SelectedIndex = 0
        If Filter.Genre.SelectedIndex = -1 Then Filter.Genre.SelectedIndex = 0
        If Filter.Language.SelectedIndex = -1 Then Filter.Language.SelectedIndex = 0
        If Filter.Country.SelectedIndex = -1 Then Filter.Country.SelectedIndex = 0
        If Filter.Director.SelectedIndex = -1 Then Filter.Director.SelectedIndex = 0
        If Filter.Actor.SelectedIndex = -1 Then Filter.Actor.SelectedIndex = 0

        Dim Items(Filter.Year.Items.Count - 1) As Object : Filter.Year.Items.CopyTo(Items, 0) : FGallery.CYear.Items.Clear() : FGallery.CYear.Items.AddRange(Items) : FGallery.CYear.SelectedIndex = Filter.Year.SelectedIndex : FGallery.LCYear.Text = FGallery.CYear.Text
        ReDim Items(Filter.Genre.Items.Count - 1) : Filter.Genre.Items.CopyTo(Items, 0) : FGallery.CGenre.Items.Clear() : FGallery.CGenre.Items.AddRange(Items) : FGallery.CGenre.SelectedIndex = Filter.Genre.SelectedIndex : FGallery.LCGenre.Text = FGallery.CGenre.Text
        ReDim Items(Filter.Language.Items.Count - 1) : Filter.Language.Items.CopyTo(Items, 0) : FGallery.CLanguage.Items.Clear() : FGallery.CLanguage.Items.AddRange(Items) : FGallery.CLanguage.SelectedIndex = Filter.Language.SelectedIndex : FGallery.LCLanguage.Text = FGallery.CLanguage.Text
        ReDim Items(Filter.Country.Items.Count - 1) : Filter.Country.Items.CopyTo(Items, 0) : FGallery.CCountry.Items.Clear() : FGallery.CCountry.Items.AddRange(Items) : FGallery.CCountry.SelectedIndex = Filter.Country.SelectedIndex : FGallery.LCCountry.Text = FGallery.CCountry.Text
        ReDim Items(Filter.Director.Items.Count - 1) : Filter.Director.Items.CopyTo(Items, 0) : FGallery.CDirector.Items.Clear() : FGallery.CDirector.Items.AddRange(Items) : FGallery.CDirector.SelectedIndex = Filter.Director.SelectedIndex : FGallery.LCDirector.Text = FGallery.CDirector.Text
        ReDim Items(Filter.Actor.Items.Count - 1) : Filter.Actor.Items.CopyTo(Items, 0) : FGallery.CActor.Items.Clear() : FGallery.CActor.Items.AddRange(Items) : FGallery.CActor.SelectedIndex = Filter.Actor.SelectedIndex : FGallery.LCActor.Text = FGallery.CActor.Text

        If FGallery.TSearch.IsFocused = False Then FGallery.TSearch.Text = Filter.Search : If FGallery.TSearch.Text = "" Then FGallery.TSearch.Text = "Search for a movie"
        FGallery.LFRatingThan.Text = Filter.Rating_Mode : If FGallery.TRating.IsFocused = False Then FGallery.TRating.Text = Filter.Rating
        FGallery.LFDurationThan.Text = Filter.Duration_Mode : If FGallery.TDuration.IsFocused = False Then FGallery.TDuration.Text = Filter.Duration
    End Sub

#End Region

#End Region

#Region " Sort Gallery "

    Public Sub SortGallery()

        Dim TempArray(Movies_F.Count - 1) As String
        Dim NewArray(Movies_F.Count - 1) As ST_Movie

        Select Case Filter.Sorted
            Case "Name", ""
                For Cuenta = 0 To Movies_F.Count - 1
                    TempArray(Cuenta) = Movies_F(Cuenta).Title + "|" + Movies_F(Cuenta).Ruta
                Next
                If Filter.NOaz = False Then Array.Sort(TempArray) Else Array.Reverse(TempArray)

                For Cuenta = 0 To TempArray.Count - 1
                    For Cuenta2 = 0 To Movies_F.Count - 1
                        If Movies_F(Cuenta2).Ruta = TempArray(Cuenta).Split("|").Last Then NewArray(Cuenta) = Movies_F(Cuenta2) : Exit For
                    Next
                Next : Movies_F = NewArray
            Case "Added"
                For Cuenta = 0 To Movies_F.Count - 1
                    TempArray(Cuenta) = Movies_F(Cuenta).Added.ToString("dd/MM/yyyy") + "|" + Movies_F(Cuenta).Ruta
                Next

                If Filter.NOaz = False Then
                    TempArray = TempArray.OrderByDescending(Function(name As String) Convert.ToDateTime(name.Split("|").First)).ToArray()
                Else
                    TempArray = TempArray.OrderBy(Function(name As String) Convert.ToDateTime(name.Split("|").First)).ToArray()
                End If

                For Cuenta = 0 To TempArray.Count - 1
                    For Cuenta2 = 0 To Movies_F.Count - 1
                        If Movies_F(Cuenta2).Ruta = TempArray(Cuenta).Split("|").Last Then NewArray(Cuenta) = Movies_F(Cuenta2) : Exit For
                    Next
                Next : Movies_F = NewArray
            Case "Rating"
                For Cuenta = 0 To Movies_F.Count - 1
                    TempArray(Cuenta) = (CDec(Movies_F(Cuenta).Rating) * 10).ToString + "|" + Movies_F(Cuenta).Ruta
                Next
                If Filter.NOaz = False Then
                    TempArray = TempArray.OrderByDescending(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                Else
                    TempArray = TempArray.OrderBy(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                End If

                For Cuenta = 0 To TempArray.Count - 1
                    For Cuenta2 = 0 To Movies_F.Count - 1
                        If Movies_F(Cuenta2).Ruta = TempArray(Cuenta).Split("|").Last Then NewArray(Cuenta) = Movies_F(Cuenta2) : Exit For
                    Next
                Next : Movies_F = NewArray
            Case "Duration"
                For Cuenta = 0 To Movies_F.Count - 1
                    TempArray(Cuenta) = Movies_F(Cuenta).Duration.ToString + "|" + Movies_F(Cuenta).Ruta
                Next
                If Filter.NOaz = False Then
                    TempArray = TempArray.OrderBy(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                Else
                    TempArray = TempArray.OrderByDescending(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                End If

                For Cuenta = 0 To TempArray.Count - 1
                    For Cuenta2 = 0 To Movies_F.Count - 1
                        If Movies_F(Cuenta2).Ruta = TempArray(Cuenta).Split("|").Last Then NewArray(Cuenta) = Movies_F(Cuenta2) : Exit For
                    Next
                Next : Movies_F = NewArray
            Case "Year"
                For Cuenta = 0 To Movies_F.Count - 1
                    TempArray(Cuenta) = Movies_F(Cuenta).Year.ToString + "|" + Movies_F(Cuenta).Ruta
                Next
                If Filter.NOaz = False Then
                    TempArray = TempArray.OrderBy(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                Else
                    TempArray = TempArray.OrderByDescending(Function(name As String) Convert.ToInt32(name.Split("|").First)).ToArray()
                End If

                For Cuenta = 0 To TempArray.Count - 1
                    For Cuenta2 = 0 To Movies_F.Count - 1
                        If Movies_F(Cuenta2).Ruta = TempArray(Cuenta).Split("|").Last Then NewArray(Cuenta) = Movies_F(Cuenta2) : Exit For
                    Next
                Next : Movies_F = NewArray
        End Select
    End Sub

#End Region

#Region " Get Movie Info "

    Public Sub GetMovieInfo()
        If My.Computer.Network.IsAvailable = False Or IMDB.Updating = True Then Exit Sub
        FGallery.Create_Folders()

        IMDB.IMDB_List.Items.Clear()
        Dim SacaNombres As String = Dir(MoviePath + "\*.*", FileAttribute.Normal)
        Do While SacaNombres <> ""
            If IO.Path.GetExtension(SacaNombres) = ".mp4" Or IO.Path.GetExtension(SacaNombres) = ".avi" Or IO.Path.GetExtension(SacaNombres) = ".m4v" Or IO.Path.GetExtension(SacaNombres) = ".mkv" Then
                If IO.File.Exists((MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(SacaNombres) + ".txt").Replace("\\", "\")) = False Then
                    IMDB.IMDB_List.Items.Add((MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(SacaNombres) + ".txt").Replace("\\", "\"))
                ElseIf IO.File.Exists((MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(SacaNombres) + ".jpg").Replace("\\", "\")) = False Then
                    If IO.File.Exists((MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(SacaNombres) + ".nojpg").Replace("\\", "\")) = False Then IMDB.IMDB_List.Items.Add((MoviePath + "\" + IO.Path.GetFileNameWithoutExtension(SacaNombres) + ".jpg").Replace("\\", "\"))
                End If
            End If : SacaNombres = Dir()
        Loop
        If IMDB.IMDB_List.Items.Count > 0 Then
            IMDB.Updating = True
            If FGallery.Mode = Group Then FGallery.LCountImdb.Text = "Updating 1 of " + IMDB.IMDB_List.Items.Count.ToString + " movies..." : FGallery.LCountImdb.Refresh() : FGallery.LCountImdb.Visible = True
            IMDB.IMDB_BWorker.RunWorkerAsync()
        End If
    End Sub

#End Region

#Region " IMDB Info "

    Private Sub IMDB_BWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        IMDB.IMDB_Worker = New IMDB_Worker : IMDB.IMDB_Worker.IMDB_Info(Me, e)
    End Sub

    Private Delegate Sub MoviesUpdateStatusCallback(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs)
    Private Sub IMDB_BWorker_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs)
        If FGallery.InvokeRequired Then : FGallery.Invoke(New MoviesUpdateStatusCallback(AddressOf IMDB_BWorker_ProgressChanged), New Object() {sender, e}) : Else
            If e.ProgressPercentage = 7000 Then
                If e.UserState = 7000 Then IMDB.Updating = False : If FGallery.Mode = Group Then FGallery.LCountImdb.Text = "Omdbapi.com not available" : FGallery.LCountImdb.Refresh() : FGallery.LCountImdb.Visible = True
                If e.UserState = 7500 And FGallery.Mode = Group Then IMDB.Updating = False : FGallery.LCountImdb.Visible = False
            End If
            'Update Gallery
            If e.ProgressPercentage < 5000 And FGallery.Mode = Group Then
                Dim Indice As Integer = e.UserState.ToString.Split("|")(0)
                Dim cProgress As Integer = e.UserState.ToString.Split("|")(1)
                FGallery.LCountImdb.Text = "Updating " + cProgress.ToString + " of " + e.ProgressPercentage.ToString + " movies..."
                FGallery.LCountImdb.Refresh() : FGallery.LCountImdb.Visible = True : Application.DoEvents()
                Filter_Update_Combos()
                If Indice < Movies.Count Then FGallery.Gallery.UpdateCover(Movies(Indice).Ruta)
                FGallery.SelectMovie(Selected)
            End If
        End If
    End Sub

#End Region

End Class

#End Region

#Region " IMDB "

#Region " IMDB Worker "

Public Class IMDB_Worker

#Region " Variables "

#Region " Structures "

    Structure ST_MovieList
        Dim Title As String
        Dim Year As String
        Dim Imdb As String
        Dim Type As String
        Dim CoverURL As String
        Dim Cover As Image
    End Structure

#End Region

#End Region

#Region " Functions "

    Private Function DownloadCover(URL As String, Path As String) As Boolean
        Try : Dim DownloadPoster As New Net.WebClient
            DownloadPoster.DownloadFile(URL, Path) : Return True
        Catch : Return False : End Try
    End Function

    Public Function CheckEmpty(Field As String, Optional Response As String = "")
        If Field Is Nothing Or Field = "" Or Field = "N/A" Then Return Response Else Return Field
    End Function

#End Region

#Region " IMDB Info "

    Public Sub IMDB_Info(ByRef cMovies As Movies_CL, e As System.ComponentModel.DoWorkEventArgs)
        Dim IMDB_APIKeys As New List(Of String) From {"&apikey=fa718ef5", "&apikey=2b89a6c7", "&apikey=7d98db3a"}
        Dim OMDB_APIKey As String : Randomize() : OMDB_APIKey = IMDB_APIKeys((IMDB_APIKeys.Count - 1) * Rnd())
        Dim Guardar As IO.StreamWriter

        For Cuenta = 0 To cMovies.IMDB.IMDB_List.Items.Count - 1
            Try
                'Movie updated
                Dim Indice As Integer = -1 : Dim Destination As String = (IO.Path.GetDirectoryName(cMovies.IMDB.IMDB_List.Items(Cuenta)) + "\" + IO.Path.GetFileNameWithoutExtension(cMovies.IMDB.IMDB_List.Items(Cuenta))).Replace("\\", "\")
                For Cuenta2 = 0 To cMovies.Movies.Count - 1
                    If IO.Path.GetFileNameWithoutExtension(cMovies.Movies(Cuenta2).Ruta) = IO.Path.GetFileNameWithoutExtension(cMovies.IMDB.IMDB_List.Items(Cuenta).ToString) Then Indice = Cuenta2 : Exit For
                Next : If Indice = -1 Then Continue For

                'Search movie by 'title'
                Dim Titulo As String = IO.Path.GetFileNameWithoutExtension(cMovies.IMDB.IMDB_List.Items(Cuenta).ToString) : If Titulo.Contains("(") = True Then Titulo = Trim(Titulo.Split("(")(0))
                Dim url As String = "http://www.omdbapi.com/?t=" + Titulo + OMDB_APIKey
                Dim wc As New Net.WebClient() : wc.Encoding = Text.Encoding.UTF8 : Dim json = wc.DownloadString(url)

                If json.Contains("Movie not found") Then 'Movie not found
                    'Search movie by 'search'
                    url = "http://www.omdbapi.com/?s=" + Titulo + OMDB_APIKey
                    wc = New Net.WebClient() : wc.Encoding = Text.Encoding.UTF8 : json = wc.DownloadString(url)

                    Dim Movies_Founds() As String = json.Replace("},{", "|").Split("|")
                    Dim Movie_Info() As String : Dim Movie As ST_MovieList : Movie.Title = "" : Movie.Year = "1990" : Movie.Imdb = ""
                    'Get movie from latest year
                    For Cuenta2 = 0 To Movies_Founds.Length - 1
                        Movie_Info = Movies_Founds(Cuenta2).Replace("{""Search"":[{", "").Replace(""",""", "|").Split("|")
                        If CheckEmpty(Movie_Info(0).Substring(9)) <> "" And IsNumeric(CheckEmpty(Movie_Info(1).Substring(7))) = True Then
                            If Convert.ToInt32(CheckEmpty(Movie_Info(1).Substring(7))) > Convert.ToInt32(Movie.Year) Then
                                If CheckEmpty(Movie_Info(3).Substring(7)) = "movie" And CheckEmpty(Movie_Info(4).Substring(9).Replace("""", "")).Contains(".jpg") Then
                                    Movie.Title = CheckEmpty(Movie_Info(0).Substring(9))
                                    Movie.Year = CheckEmpty(Movie_Info(1).Substring(7))
                                    Movie.Imdb = CheckEmpty(Movie_Info(2).Substring(9))
                                End If
                            End If
                        End If
                    Next
                    If Movie.Imdb <> "" Then url = "http://www.omdbapi.com/?i=" + Movie.Imdb + OMDB_APIKey Else url = "http://www.omdbapi.com/?t=" + Titulo + OMDB_APIKey
                    wc = New Net.WebClient() : wc.Encoding = Text.Encoding.UTF8 : json = wc.DownloadString(url)
                End If

                'Extract 'json' info
                Dim oJS As New Web.Script.Serialization.JavaScriptSerializer() : Dim obj As New ImdbEntity()
                obj = oJS.Deserialize(Of ImdbEntity)(json)
                If obj.Response = "True" Then

                    'Get JPG
                    If IO.File.Exists(Destination + ".jpg") = False Then
                        If obj.Poster.Contains(".jpg") Then
                            If DownloadCover(obj.Poster, Destination + ".jpg") = True Then
                                If IO.File.Exists(Destination + ".jpg") = True Then
                                    IO.File.SetAttributes(Destination + ".jpg", IO.FileAttributes.Hidden)
                                    Dim Destination_Cache As String = (IO.Path.GetDirectoryName(cMovies.IMDB.IMDB_List.Items(Cuenta)) + "\cache\" + IO.Path.GetFileNameWithoutExtension(cMovies.IMDB.IMDB_List.Items(Cuenta))).Replace("\\", "\")
                                    FGallery.CacheImage(FGallery.LoadImageClone(Destination + ".jpg"), 190, 135).Save(Destination_Cache + ".jpg")
                                    If IO.File.Exists(Destination_Cache + ".jpg") = True Then IO.File.SetAttributes(Destination_Cache + ".jpg", IO.FileAttributes.Hidden)
                                    cMovies.Movies(Indice).CoverCachedPath = Destination_Cache + ".jpg"
                                    cMovies.Movies(Indice).CoverCached = FGallery.LoadImageClone(Destination_Cache + ".jpg")
                                    cMovies.Movies(Indice).NoCover = False
                                End If
                            End If
                        End If
                    End If

                    'Get TXT
                    If IO.File.Exists(Destination + ".txt") = False Then
                        'Duration
                        If Text.RegularExpressions.Regex.Replace(obj.Runtime, "[^\d]", "") = "" Then obj.Runtime = "0"
                        cMovies.Movies(Indice).Duration = Integer.Parse(Text.RegularExpressions.Regex.Replace(obj.Runtime, "[^\d]", ""))
                        'Language
                        Dim LanguageTemp() As String
                        cMovies.Movies(Indice).Language = ""
                        If obj.Language.Contains(" Sign Language") Then
                            LanguageTemp = obj.Language.Split(", ") : For CuentaTemp = 0 To LanguageTemp.Count - 1
                                If cMovies.Movies(Indice).Language.Contains(LanguageTemp(CuentaTemp).Replace(" Sign Language", "")) = False Then cMovies.Movies(Indice).Language += ", " + LanguageTemp(CuentaTemp).Replace(" Sign Language", "")
                            Next : cMovies.Movies(Indice).Language = cMovies.Movies(Indice).Language.Substring(2)
                        Else : cMovies.Movies(Indice).Language = obj.Language
                        End If
                        'Rating
                        If obj.ImdbRating Is Nothing OrElse (obj.ImdbRating = "N/A" Or obj.ImdbRating = "") Then obj.ImdbRating = "0.0"
                        cMovies.Movies(Indice).Rating = obj.ImdbRating
                        cMovies.Movies(Indice).ImdbID = obj.ImdbID
                        cMovies.Movies(Indice).Year = obj.Year
                        cMovies.Movies(Indice).Country = obj.Country
                        cMovies.Movies(Indice).Genre = obj.Genre.Replace("Music", "Musical").Replace("Musicalal", "Musical")
                        cMovies.Movies(Indice).Director = obj.Director.Replace("(co-director)", "")
                        cMovies.Movies(Indice).Actor = obj.Actors
                        cMovies.Movies(Indice).Plot = obj.Plot

                        Guardar = New IO.StreamWriter(Destination + ".txt")
                        Guardar.Write(cMovies.Movies(Indice).ImdbID + "|" + cMovies.Movies(Indice).Rating + "|" + cMovies.Movies(Indice).Year + "|" + cMovies.Movies(Indice).Duration.ToString + "|" + cMovies.Movies(Indice).Language + "|" + cMovies.Movies(Indice).Country + "|" + cMovies.Movies(Indice).Genre + "|" + cMovies.Movies(Indice).Director + "|" + cMovies.Movies(Indice).Actor + "|" + cMovies.Movies(Indice).Plot + "|" + cMovies.Movies(Indice).NO_Auto_Watch.ToString + "|" + cMovies.Movies(Indice).NO_Auto_Pending.ToString) : Guardar.Close()
                        If IO.File.Exists(Destination + ".txt") = True Then IO.File.SetAttributes(Destination + ".txt", IO.FileAttributes.Hidden)
                    End If
                End If

                If IO.File.Exists(Destination + ".jpg") = False Then
                    Guardar = New IO.StreamWriter(Destination + ".nojpg") : Guardar.Write("NoCover") : Guardar.Close() : IO.File.SetAttributes(Destination + ".nojpg", IO.FileAttributes.Hidden)
                    cMovies.Movies(Indice).NoCover = True
                End If

                If IO.File.Exists(Destination + ".txt") = False Then
                    cMovies.Movies(Indice).Rating = "0.0"
                    cMovies.Movies(Indice).ImdbID = ""
                    cMovies.Movies(Indice).Year = ""
                    cMovies.Movies(Indice).Duration = "0"
                    cMovies.Movies(Indice).Language = ""
                    cMovies.Movies(Indice).Country = ""
                    cMovies.Movies(Indice).Genre = ""
                    cMovies.Movies(Indice).Director = ""
                    cMovies.Movies(Indice).Actor = ""
                    cMovies.Movies(Indice).Plot = ""
                    Guardar = New IO.StreamWriter(Destination + ".txt")
                    Guardar.Write(cMovies.Movies(Indice).ImdbID + "|" + cMovies.Movies(Indice).Rating + "|" + cMovies.Movies(Indice).Year + "|" + cMovies.Movies(Indice).Duration.ToString + "|" + cMovies.Movies(Indice).Language + "|" + cMovies.Movies(Indice).Country + "|" + cMovies.Movies(Indice).Genre + "|" + cMovies.Movies(Indice).Director + "|" + cMovies.Movies(Indice).Actor + "|" + cMovies.Movies(Indice).Plot + "|" + cMovies.Movies(Indice).NO_Auto_Watch.ToString + "|" + cMovies.Movies(Indice).NO_Auto_Pending.ToString) : Guardar.Close()
                    If IO.File.Exists(Destination + ".txt") = True Then IO.File.SetAttributes(Destination + ".txt", IO.FileAttributes.Hidden)
                End If

                For Cuenta2 = 0 To cMovies.Movies_F.Count - 1
                    If cMovies.Movies_F(Cuenta2).Ruta = cMovies.Movies(Indice).Ruta Then cMovies.Movies_F(Cuenta2) = cMovies.Movies(Indice) : Exit For
                Next

                cMovies.Add_Filter(Indice)
                cMovies.IMDB.IMDB_BWorker.ReportProgress(cMovies.IMDB.IMDB_List.Items.Count, Indice.ToString + "|" + (Cuenta + 1).ToString)

            Catch ex As Exception
                If ex.Message.ToLower.Contains("omdbapi") Then
                    e.Cancel = True : cMovies.IMDB.IMDB_BWorker.ReportProgress(7000, 7000) 'Omdbapi.com not available
                    Threading.Thread.Sleep(5000) : cMovies.IMDB.IMDB_BWorker.ReportProgress(7000, 7500) : cMovies.IMDB.Updating = False : Exit Sub 'Finish
                End If
            End Try

            If cMovies.IMDB.IMDB_BWorker.CancellationPending Then e.Cancel = True : Exit For
        Next : Threading.Thread.Sleep(1000) : cMovies.IMDB.IMDB_BWorker.ReportProgress(7000, 7500) : cMovies.IMDB.Updating = False  'Finish
    End Sub

#End Region

End Class

#End Region

#Region " Imdb Entity "

Public Class ImdbEntity

    Public Property Title As String
    Public Property Year As String
    Public Property Rated As String
    Public Property Released As String
    Public Property Runtime As String
    Public Property Genre As String
    Public Property Director As String
    Public Property Writer As String
    Public Property Actors As String
    Public Property Plot As String
    Public Property Language As String
    Public Property Country As String
    Public Property Awards As String
    Public Property Poster As String
    Public Property Metascore As String
    Public Property ImdbRating As String
    Public Property ImdbVotes As String
    Public Property ImdbID As String
    Public Property Type As String
    Public Property Response As String

End Class

#End Region

#End Region

#Region " Youtube "

#Region " Search "

Public Class YouTube


    Private APIKey As String = "AIzaSyAdTL7Yi1N_fYEOTvm1swQLVkJkN6K_Qb0"

    Public Function Search(ResultList As String) As JsVideo
        Try : Dim Results As JsVideo = New Web.Script.Serialization.JavaScriptSerializer().Deserialize(Of JsVideo)(New Net.WebClient() With {.Encoding = Text.Encoding.UTF8}.DownloadString("https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q=" + Net.WebUtility.UrlEncode(ResultList) + "&fields=items(id%2FvideoId%2Csnippet(channelId%2CchannelTitle%2Cdescription%2Cthumbnails%2Fdefault%2Furl%2Ctitle))%2CnextPageToken%2CpageInfo%2CprevPageToken&key=" + APIKey))
            'For Each Result In Results.Items
            '    Result.ID.VideoID = "https://www.youtube.com/v/" + Result.ID.VideoID
            'Next : Return Results
            Return Results
        Catch : End Try : Return Nothing
    End Function

End Class

#End Region

#Region " Clases "

Public Class JsVideo
    Public Property Items As New List(Of JsItems)
End Class

Public Class JsItems
    Public Property ID As JsID
    Public Property Snippet As JsSnippet
End Class

Public Class JsID
    Public Property VideoID As String
End Class

Public Class JsSnippet
    Public Property ChannelID As String
    Public Property Title As String
    Public Property Description As String
    Public Property ChannelTitle As String
End Class

#End Region

#End Region

#End Region
