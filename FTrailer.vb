
Public Class FTrailer

    Public VideoId As String

#Region " Resize Form "

    Protected Overrides Sub WndProc(ByRef message As Message)
        MyBase.WndProc(message)
        If message.Msg = &H84 Then
            If New Rectangle(0, 0, 10, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(13, IntPtr)
            ElseIf New Rectangle(ClientSize.Width - 10, 0, 10, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(14, IntPtr)
            ElseIf New Rectangle(0, ClientSize.Height - 10, 10, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(16, IntPtr)
            ElseIf New Rectangle(ClientSize.Width - 10, ClientSize.Height - 10, 10, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(17, IntPtr)
            ElseIf New Rectangle(0, 0, ClientSize.Width, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(12, IntPtr)
            ElseIf New Rectangle(0, 0, 10, ClientSize.Height).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(10, IntPtr)
            ElseIf New Rectangle(ClientSize.Width - 10, 0, 10, ClientSize.Height).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(11, IntPtr)
            ElseIf New Rectangle(0, ClientSize.Height - 10, ClientSize.Width, 10).Contains(PointToClient(Cursor.Position)) Then
                message.Result = CType(15, IntPtr)
            End If
        End If
    End Sub

#End Region

    Private Async Sub FMenu_Load(sender As Object, e As EventArgs) Handles Me.Load
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--autoplay-policy=no-user-gesture-required")
        Await WTrailer.EnsureCoreWebView2Async()

        Dim html As String = "<html><head><title>YouTube IFrame API Example</title><script async src=""https://www.youtube.com/iframe_api""></script></head><body><div id=""div_YouTube""></div></body><script>var player;function onYouTubeIframeAPIReady(){player=new YT.Player('div_YouTube',{videoId:'" + VideoId + "',playerVars:{'autoplay':1,'controls':1,'showinfo':0,'modestbranding':1,'loop':1,'fs':0,'cc_load_policty':0,'iv_load_policy':3},events:{'onReady':function(e){e.target.setVolume(30);}}});}</script><style>#div_YouTube{position:fixed;top:0;bottom:0;left:0;right:0;width:100%;height:100%;}</style></html>"
        WTrailer.NavigateToString(html)
    End Sub

    Private Async Function GetCurrentTimeAsync() As Threading.Tasks.Task(Of Double)
        Try : Dim result = Await WTrailer.ExecuteScriptAsync("player.pauseVideo();")
            Dim SacaTime As String = result.ToString()
            Dim posicion1 As Integer = InStr(SacaTime, """currentTime"":")
            If posicion1 > 0 Then
                Dim posicion2 As Integer = InStr(posicion1, SacaTime, ",""")
                If posicion2 > 0 Then SacaTime = SacaTime.Substring(posicion1 + 13, posicion2 - (posicion1 + 14))
            End If : Return SacaTime.Split(".")(0)
        Catch : Return 0 : End Try
    End Function

    Private Sub WTrailer_NavigationCompleted(sender As Object, e As Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs) Handles WTrailer.NavigationCompleted
        LTrailer.Visible = False
    End Sub

    Private Sub LTitle_MouseMove(sender As Object, e As MouseEventArgs) Handles LTitle.MouseMove, PLogo.MouseMove, PPanel.MouseMove
        If e.Button = MouseButtons.Left Then FGallery.ReleaseCapture() : FGallery.SendMessage(Handle, &H112&, &HF012&, 0)
    End Sub

#Region " Buttons "

    Private Async Sub PLogo_Click(sender As Object, e As MouseEventArgs) Handles PLogo.Click
        Dim currentTime = Await GetCurrentTimeAsync()
        If e.Button = MouseButtons.Left Then Process.Start(("https://www.youtube.com/v/" + VideoId).Replace("v/", "watch?v=") + "&t=" + currentTime.ToString + "s") : Close()
    End Sub


    Private Sub BClose_MouseEnter(sender As Object, e As EventArgs) Handles BClose.MouseEnter, BMax.MouseEnter, BMin.MouseEnter
        Dim CButton As FlatLabel = CType(sender, FlatLabel)
        CButton.ForeColor = Color.DarkGreen
    End Sub

    Private Sub BClose_MouseLeave(sender As Object, e As EventArgs) Handles BClose.MouseLeave, BMax.MouseLeave, BMin.MouseLeave
        Dim CButton As FlatLabel = CType(sender, FlatLabel)
        CButton.ForeColor = Color.SlateGray
    End Sub

    Private Sub BClose_Click(sender As Object, e As MouseEventArgs) Handles BClose.Click
        If e.Button = MouseButtons.Left Then Close()
    End Sub

    Private Sub BMax_Click(sender As Object, e As MouseEventArgs) Handles BMax.Click
        If e.Button = MouseButtons.Left Then
            If WindowState = FormWindowState.Maximized Then WindowState = FormWindowState.Normal : BMax.Text = "1" Else WindowState = FormWindowState.Maximized : BMax.Text = "2"
        End If
    End Sub

    Private Sub BMin_Click(sender As Object, e As MouseEventArgs) Handles BMin.Click
        If e.Button = MouseButtons.Left AndAlso WindowState <> FormWindowState.Minimized Then WindowState = FormWindowState.Minimized
    End Sub

    Private Sub LTitle_Click(sender As Object, e As MouseEventArgs) Handles LTitle.DoubleClick, PPanel.DoubleClick
        If e.Button = MouseButtons.Left Then
            If WindowState = FormWindowState.Maximized Then WindowState = FormWindowState.Normal : BMax.Text = "1" Else WindowState = FormWindowState.Maximized : BMax.Text = "2"
        End If
    End Sub

    Private Sub BMin_Click(sender As Object, e As EventArgs) Handles BMin.Click

    End Sub

    Private Sub BMax_Click(sender As Object, e As EventArgs) Handles BMax.Click

    End Sub

    Private Sub BClose_Click(sender As Object, e As EventArgs) Handles BClose.Click

    End Sub

#End Region

End Class