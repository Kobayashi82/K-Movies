
#Region " MyApplication "

Namespace My

    Partial Friend Class MyApplication

#Region " Variables "

        Private Delegate Sub SafeApplicationThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
        Private WithEvents Domain As AppDomain = AppDomain.CurrentDomain

#End Region

#Region " Startup "

        Private Sub MyApplication_Startup(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, New ResolveEventHandler(AddressOf DomainCheck)
            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomain_UnhandledException
            AddHandler Windows.Forms.Application.ThreadException, AddressOf App_ThreadException
        End Sub

#Region " Domain Check "

        Private Function DomainCheck(sender As Object, e As ResolveEventArgs) As Reflection.Assembly Handles Domain.AssemblyResolve
            Try
                'If e.Name.Substring(0, e.Name.IndexOf(",")) = "Microsoft.Web.WebView2.Core" Then Return Reflection.Assembly.Load(Resources.Microsoft_Web_WebView2_Core)
                'If e.Name.Substring(0, e.Name.IndexOf(",")) = "Microsoft.Web.WebView2.WinForms" Then Return Reflection.Assembly.Load(Resources.Microsoft_Web_WebView2_WinForms)
                'If e.Name.Substring(0, e.Name.IndexOf(",")) = "WebView2Loader" Then Return Reflection.Assembly.Load(Resources.WebView2Loader)
            Catch : End Try : Return Nothing
        End Function

#End Region

#End Region

#Region " Debug Errors "

        Private Sub ShowDebug(ex As Exception)
            Dim Mensaje As String
            Try
                Mensaje = "K-Desktop " + FGallery.Version + vbCrLf + vbCrLf
                Mensaje += Computer.Info.OSFullName + " " + IIf(Environment.Is64BitOperatingSystem, "x64", "x86") + " (" + Computer.Info.OSVersion + ")" + vbCrLf
                Mensaje += FGallery.FileSize(Convert.ToInt64(Computer.Info.AvailablePhysicalMemory)) + " disponible de " + FGallery.FileSize(Convert.ToInt64(Computer.Info.TotalPhysicalMemory)) + vbCrLf + vbCrLf
                Mensaje += ex.Message
            Catch : If ex.ToString.Trim = "" Then Mensaje = "No se han encontrado errores" Else Mensaje = ex.ToString
            End Try : MsgBox(Mensaje)
        End Sub

        Private Sub App_ThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
            If MainForm.InvokeRequired Then MainForm.Invoke(New SafeApplicationThreadException(AddressOf App_ThreadException), New Object() {sender, e}) Else ShowDebug(e.Exception)
        End Sub

        Private Sub AppDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
            ShowDebug(DirectCast(e.ExceptionObject, Exception))
        End Sub

        Private Sub MyApplication_UnhandledException(sender As Object, e As ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            ShowDebug(e.Exception)
        End Sub

#End Region

    End Class

End Namespace

#End Region