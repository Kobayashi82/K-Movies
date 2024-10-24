
#Region " Settings "

Public Class FSettings

#Region " Variables "

    Public Cancelado As Boolean = True

#End Region

#Region " Eventos del Formulario "

#Region " Load "

    Private Sub FSettings_Load(sender As Object, e As EventArgs) Handles Me.Load
        Opacity = 0 : SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True) : SetStyle(ControlStyles.UserPaint, True) : Refresh() : Opacity = 1
    End Sub

    Private Sub FSettings_Shown(sender As Object, e As EventArgs) Handles Me.Shown

    End Sub

#End Region

#Region " KeyPress "

    Private Sub LFormTitle_MouseMove(sender As Object, e As MouseEventArgs) Handles LFormTitle.MouseMove
        If e.Button = MouseButtons.Left Then FGallery.ReleaseCapture() : FGallery.SendMessage(Handle, &H112&, &HF012&, 0)
    End Sub

    Private Sub FSettings_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        If Asc(e.KeyChar) = 13 Then : e.Handled = True
            If TMoviesPath.IsFocused = False And TPlayerPath.IsFocused = False And TMin.IsFocused = False Then Cancelado = False : Close() Else : THidden.Focus()
        End If
        If Asc(e.KeyChar) = 27 Then If TMoviesPath.IsFocused = True Or TPlayerPath.IsFocused = True Or TMin.IsFocused = True Then e.Handled = True : BAccept.Focus() Else Close()
    End Sub

    Private Sub TMin_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TMin.KeyPress
        If Not Char.IsNumber(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then e.Handled = True
        If Asc(e.KeyChar) = 13 Then e.Handled = True : THidden.Focus()
    End Sub

#End Region

#End Region

#Region " Controles "

    Private Sub BMoviesPath_Click(sender As Object, e As MouseEventArgs) Handles BMoviesPath.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Dim Carpeta As New FolderBrowserDialog
        Carpeta.SelectedPath = TMoviesPath.Text
        Carpeta.Description = "Select the Movies folder"
        If Carpeta.ShowDialog() = Windows.Forms.DialogResult.OK Then TMoviesPath.Text = Carpeta.SelectedPath
        Carpeta.Dispose()
    End Sub

    Private Sub BPlayerPath_Click(sender As Object, e As MouseEventArgs) Handles BPlayerPath.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Dim Archivo As New OpenFileDialog
        Archivo.CheckFileExists = True
        Archivo.CheckPathExists = True
        Archivo.DefaultExt = "*.exe"
        Archivo.Filter = ("Executable files (*.exe)|*.exe")
        Archivo.Title = "Select MKPlayer executable"
        Archivo.FileName = IO.Path.GetFileName(TPlayerPath.Text)
        If Archivo.ShowDialog() = Windows.Forms.DialogResult.OK Then TPlayerPath.Text = Archivo.FileName
        Archivo.Dispose()
    End Sub

    Private Sub AutoWatched_Click(sender As Object, e As EventArgs) Handles LAutoWatched.Click, LAutoWatched.DoubleClick
        CAutoWatched.Checked = Not CAutoWatched.Checked
    End Sub

    Private Sub DeleteImported_Click(sender As Object, e As EventArgs) Handles LDeleteImported.DoubleClick, LDeleteImported.Click
        CDeleteImported.Checked = Not CDeleteImported.Checked
    End Sub

#End Region

#Region " Buttons "

#Region " Accept "

    Private Sub BAccept_Click(sender As Object, e As MouseEventArgs) Handles BAccept.Click
        If e.Button <> MouseButtons.Left Then Exit Sub
        Cancelado = False : Close()
    End Sub

#End Region

#Region " Cancel / Close "

    Private Sub BCancel_Click(sender As Object, e As MouseEventArgs) Handles BCancel.Click, BClose.Click
        If e.Button = MouseButtons.Left Then Close()
    End Sub

    Private Sub Buttons_MouseEnter(sender As Object, e As EventArgs) Handles BClose.MouseEnter
        sender.ForeColor = Color.DarkGreen
    End Sub

    Private Sub Buttons_MouseLeave(sender As Object, e As EventArgs) Handles BClose.MouseLeave
        sender.ForeColor = Color.DarkGray
    End Sub

    Private Sub BCancel_Click(sender As Object, e As EventArgs) Handles BClose.Click, BCancel.Click

    End Sub

    Private Sub BMoviesPath_Click(sender As Object, e As EventArgs) Handles BMoviesPath.Click

    End Sub

    Private Sub BAccept_Click(sender As Object, e As EventArgs) Handles BAccept.Click

    End Sub

    Private Sub BPlayerPath_Click(sender As Object, e As EventArgs) Handles BPlayerPath.Click

    End Sub

#End Region

#End Region

End Class

#End Region