
#Region " Add Watched "

Public Class FAddWatched

#Region " Variables "

    Public Cancelado As Boolean = True
    Public Remove As Boolean

#End Region

#Region " Eventos del Formulario "

#Region " Load "

    Private Sub FAddWatched_Load(sender As Object, e As EventArgs) Handles Me.Load
        Opacity = 0 : SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True) : SetStyle(ControlStyles.UserPaint, True) : Refresh() : Opacity = 1
    End Sub

    Private Sub FAddWatched_Shown(sender As Object, e As EventArgs) Handles Me.Shown

    End Sub

#End Region

#End Region

#Region " Buttons "

#Region " Remove "

    Private Sub BRemove_Click(sender As Object, e As MouseEventArgs) Handles BRemove.Click
        If e.Button = MouseButtons.Left Then Cancelado = False : Remove = True : Close()
    End Sub

#End Region

#Region " Accept "

    Private Sub BAccept_Click(sender As Object, e As MouseEventArgs) Handles BAccept.Click
        If e.Button = MouseButtons.Left Then Cancelado = False : Remove = False : Close()
    End Sub

    Private Sub LTitle_TextChanged(sender As Object, e As EventArgs) Handles LTitle.TextChanged
        FGallery.SetFontSize(LTitle, 22, 5)
    End Sub

#End Region

#Region " Cancel / Close "

    Private Sub BClose_Click(sender As Object, e As MouseEventArgs) Handles BClose.Click, BCancel.Click
        If e.Button = MouseButtons.Left Then Close()
    End Sub

    Private Sub BClose_MouseEnter(sender As Object, e As EventArgs) Handles BClose.MouseEnter
        sender.ForeColor = Color.DarkGreen
    End Sub

    Private Sub BClose_MouseLeave(sender As Object, e As EventArgs) Handles BClose.MouseLeave
        sender.ForeColor = Color.SlateGray
    End Sub

#End Region

#End Region

End Class

#End Region