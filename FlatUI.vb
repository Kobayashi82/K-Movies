
#Region " FlatUI "

#Region " Theme Module "

Enum MouseState As Byte : None = 0 : Over = 1 : Down = 2 : Block = 3 : End Enum

Module Helpers

#Region " Variables "

    Friend G As Graphics, B As Bitmap
    Friend _FlatColor As Color = Color.FromArgb(56, 54, 53)
    Friend NearSF As New StringFormat() With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Near}
    Friend CenterSF As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}

#End Region

#Region " Functions "

    Public Function RoundRec(Rectangle As Rectangle, Curve As Integer) As Drawing2D.GraphicsPath
        Dim P As New Drawing2D.GraphicsPath() : Dim ArcRectangleWidth As Integer = Curve * 2
        P.AddArc(New Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90)
        P.AddArc(New Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90)
        P.AddArc(New Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90)
        P.AddArc(New Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90)
        P.AddLine(New Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), New Point(Rectangle.X, Curve + Rectangle.Y)) : Return P
    End Function

    Public Function RoundRect(x!, y!, w!, h!, Optional r! = 0.3, Optional TL As Boolean = True, Optional TR As Boolean = True, Optional BR As Boolean = True, Optional BL As Boolean = True) As Drawing2D.GraphicsPath
        Dim d! = Math.Min(w, h) * r, xw = x + w, yh = y + h : RoundRect = New Drawing2D.GraphicsPath
        If TL Then RoundRect.AddArc(x, y, d, d, 180, 90) Else RoundRect.AddLine(x, y, x, y)
        If TR Then RoundRect.AddArc(xw - d, y, d, d, 270, 90) Else RoundRect.AddLine(xw, y, xw, y)
        If BR Then RoundRect.AddArc(xw - d, yh - d, d, d, 0, 90) Else RoundRect.AddLine(xw, yh, xw, yh)
        If BL Then RoundRect.AddArc(x, yh - d, d, d, 90, 90) Else RoundRect.AddLine(x, yh, x, yh)
        RoundRect.CloseFigure()
    End Function

    Public Function DrawArrow(x As Integer, y As Integer, flip As Boolean) As Drawing2D.GraphicsPath
        Dim GP As New Drawing2D.GraphicsPath() : Dim W As Integer = 12 : Dim H As Integer = 6
        If flip Then GP.AddLine(x + 1, y, x + W + 1, y) : GP.AddLine(x + W, y, x + H, y + H - 1) Else GP.AddLine(x, y + H, x + W, y + H) : GP.AddLine(x + W, y + H, x + H, y)
        GP.CloseFigure() : Return GP
    End Function

    Private TextBitmap As Bitmap
    Private TextGraphics As Graphics
    Private CreateRoundPath As Drawing2D.GraphicsPath
    Private CreateRoundRectangle As Rectangle

    Sub New()
        TextBitmap = New Bitmap(1, 1) : TextGraphics = Graphics.FromImage(TextBitmap)
    End Sub

    Friend Function MeasureString(text As String, font As Font) As SizeF
        Return TextGraphics.MeasureString(text, font)
    End Function

    Friend Function MeasureString(text As String, font As Font, width As Integer) As SizeF
        Return TextGraphics.MeasureString(text, font, width, StringFormat.GenericTypographic)
    End Function

    Friend Function CreateRound(x As Integer, y As Integer, width As Integer, height As Integer, slope As Integer) As Drawing2D.GraphicsPath
        CreateRoundRectangle = New Rectangle(x, y, width, height) : Return CreateRound(CreateRoundRectangle, slope)
    End Function

    Friend Function CreateRound(r As Rectangle, slope As Integer) As Drawing2D.GraphicsPath
        CreateRoundPath = New Drawing2D.GraphicsPath(Drawing2D.FillMode.Winding)
        CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180.0F, 90.0F)
        CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270.0F, 90.0F)
        CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0.0F, 90.0F)
        CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90.0F, 90.0F)
        CreateRoundPath.CloseFigure() : Return CreateRoundPath
    End Function

#End Region

End Module

#Region " Theme "

Class FormSkin : Inherits ContainerControl

#Region " Properties "

    Private W, H As Integer
    Private Cap As Boolean = False
    Private _HeaderMaximize As Boolean = False
    Private MousePoint As New Point(0, 0)
    Private MoveHeight = 50

    Private TextColor As Color = Color.FromArgb(234, 234, 234)
    'Private _HeaderLight As Color = Color.FromArgb(171, 171, 172)
    'Private _BaseLight As Color = Color.FromArgb(196, 199, 200)
    Public TextLight As Color = Color.FromArgb(45, 47, 49)

#Region " Colors "

    Private _HeaderColor As Color = Color.FromArgb(45, 47, 49)
    <ComponentModel.Category("Colors")> Public Property HeaderColor() As Color
        Get
            Return _HeaderColor
        End Get
        Set(value As Color)
            _HeaderColor = value
        End Set
    End Property

    Private _BaseColor As Color = Color.FromArgb(60, 70, 73)
    <ComponentModel.Category("Colors")> Public Property BaseColor() As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _BorderColor As Color = Color.FromArgb(53, 58, 60)
    <ComponentModel.Category("Colors")> Public Property BorderColor() As Color
        Get
            Return _BorderColor
        End Get
        Set(value As Color)
            _BorderColor = value
        End Set
    End Property

    <ComponentModel.Category("Colors")> Public Property FlatColor() As Color
        Get
            Return _FlatColor
        End Get
        Set(value As Color)
            _FlatColor = value
        End Set
    End Property

#End Region

#Region " Options "

    <ComponentModel.Category("Options")> Public Property HeaderMaximize As Boolean
        Get
            Return _HeaderMaximize
        End Get
        Set(value As Boolean)
            _HeaderMaximize = value
        End Set
    End Property

#End Region

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e) : If e.Button = Windows.Forms.MouseButtons.Left And New Rectangle(0, 0, Width, MoveHeight).Contains(e.Location) Then Cap = True : MousePoint = e.Location
    End Sub

    Private Sub FormSkin_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Me.MouseDoubleClick
        If HeaderMaximize AndAlso (e.Button = Windows.Forms.MouseButtons.Left And New Rectangle(0, 0, Width, MoveHeight).Contains(e.Location)) Then
            Select Case FindForm.WindowState
                Case FormWindowState.Normal : FindForm.WindowState = FormWindowState.Maximized : FindForm.Refresh()
                Case FormWindowState.Maximized : FindForm.WindowState = FormWindowState.Normal : FindForm.Refresh()
            End Select
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e) : Cap = False
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : If Cap Then Parent.Location = MousePosition - MousePoint
    End Sub

    Protected Overrides Sub OnCreateControl()
        MyBase.OnCreateControl() : ParentForm.FormBorderStyle = FormBorderStyle.None : ParentForm.AllowTransparency = False : ParentForm.TransparencyKey = Color.Fuchsia : ParentForm.FindForm.StartPosition = FormStartPosition.CenterScreen : Dock = DockStyle.Fill : Invalidate()
    End Sub

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        DoubleBuffered = True : BackColor = Color.White : Font = New Font("Segoe UI", 12)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        B = New Bitmap(Width, Height) : G = Graphics.FromImage(B) : W = Width : H = Height
        Dim Base As New Rectangle(0, 0, W, H), Header As New Rectangle(0, 0, W, 50)
        G.SmoothingMode = 2 : G.PixelOffsetMode = 2 : G.TextRenderingHint = 5 : G.Clear(BackColor)

        '-- Base
        G.FillRectangle(New SolidBrush(_BaseColor), Base)
        '-- Header
        G.FillRectangle(New SolidBrush(_HeaderColor), Header)
        '-- Logo
        G.FillRectangle(New SolidBrush(Color.FromArgb(243, 243, 243)), New Rectangle(8, 16, 4, 18))
        G.FillRectangle(New SolidBrush(_FlatColor), 16, 16, 4, 18)
        G.DrawString(Text, Font, New SolidBrush(TextColor), New Rectangle(26, 15, W, H), NearSF)
        '-- Border
        G.DrawRectangle(New Pen(_BorderColor), Base)

        MyBase.OnPaint(e) : G.Dispose() : e.Graphics.InterpolationMode = 7 : e.Graphics.DrawImageUnscaled(B, 0, 0) : B.Dispose()
    End Sub

End Class

#Region " Close Button "

Class FlatClose : Inherits Control

#Region " Properties "

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e) : Size = New Size(18, 18)
    End Sub

#Region " Colors "

    Private _BaseColor As Color = Color.FromArgb(168, 35, 35)
    <ComponentModel.Category("Colors")> Public Property BaseColor As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _TextColor As Color = Color.FromArgb(243, 243, 243)
    <ComponentModel.Category("Colors")> Public Property TextColor As Color
        Get
            Return _TextColor
        End Get
        Set(value As Color)
            _TextColor = value
        End Set
    End Property

#End Region

#Region " Mouse States "

    'Private State As MouseState = MouseState.None

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e) ': State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e) ': State = MouseState.Down : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e) ': State = MouseState.None : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e) ': State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : Invalidate()
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
    End Sub

#End Region

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        DoubleBuffered = True : BackColor = Color.White : Size = New Size(18, 18) : Anchor = AnchorStyles.Top Or AnchorStyles.Right : Font = New Font("Marlett", 10)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim B As New Bitmap(Width, Height)
        Dim G As Graphics = Graphics.FromImage(B)
        Dim Base As New Rectangle(0, 0, Width, Height)
        G.SmoothingMode = 2 : G.PixelOffsetMode = 2 : G.TextRenderingHint = 5 : G.Clear(BackColor)

        '-- Base
        G.FillRectangle(New SolidBrush(_BaseColor), Base)
        '-- X
        G.DrawString("r", Font, New SolidBrush(TextColor), New Rectangle(0, 0, Width, Height), CenterSF)

        'Select Case State
        '    Case MouseState.Over
        '        .FillRectangle(New SolidBrush(Color.FromArgb(30, Color.White)), Base)
        '    Case MouseState.Down
        '        .FillRectangle(New SolidBrush(Color.FromArgb(30, Color.Black)), Base)
        'End Select

        MyBase.OnPaint(e) : G.Dispose() : e.Graphics.InterpolationMode = 7 : e.Graphics.DrawImageUnscaled(B, 0, 0) : B.Dispose()
    End Sub

End Class

#End Region

#Region " Maximize Button "

Class FlatMax : Inherits Control

#Region " Properties "

#Region " Colors "

    Private _BaseColor As Color = Color.FromArgb(45, 47, 49)
    <ComponentModel.Category("Colors")> Public Property BaseColor As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _TextColor As Color = Color.FromArgb(243, 243, 243)
    <ComponentModel.Category("Colors")> Public Property TextColor As Color
        Get
            Return _TextColor
        End Get
        Set(value As Color)
            _TextColor = value
        End Set
    End Property

#End Region

#Region " Mouse States "

    Private State As MouseState = MouseState.None

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        State = MouseState.Down : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        State = MouseState.None : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : Invalidate()
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        Select Case FindForm.WindowState
            Case FormWindowState.Maximized
                FindForm.WindowState = FormWindowState.Normal
            Case FormWindowState.Normal
                FindForm.WindowState = FormWindowState.Maximized
        End Select
    End Sub

#End Region

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e) : Size = New Size(18, 18)
    End Sub

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        DoubleBuffered = True : BackColor = Color.White : Size = New Size(18, 18) : Anchor = AnchorStyles.Top Or AnchorStyles.Right : Font = New Font("Marlett", 12)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim B As New Bitmap(Width, Height)
        Dim G As Graphics = Graphics.FromImage(B)
        Dim Base As New Rectangle(0, 0, Width, Height)
        G.SmoothingMode = 2 : G.PixelOffsetMode = 2 : G.TextRenderingHint = 5 : G.Clear(BackColor)

        '-- Base
        G.FillRectangle(New SolidBrush(_BaseColor), Base)
        '-- Maximize
        If FindForm.WindowState = FormWindowState.Maximized Then
            G.DrawString("1", Font, New SolidBrush(TextColor), New Rectangle(1, 1, Width, Height), CenterSF)
        ElseIf FindForm.WindowState = FormWindowState.Normal Then
            G.DrawString("2", Font, New SolidBrush(TextColor), New Rectangle(1, 1, Width, Height), CenterSF)
        End If

        Select Case State
            Case MouseState.Over : G.FillRectangle(New SolidBrush(Color.FromArgb(30, Color.White)), Base)
            Case MouseState.Down : G.FillRectangle(New SolidBrush(Color.FromArgb(30, Color.Black)), Base)
        End Select

        MyBase.OnPaint(e) : G.Dispose() : e.Graphics.InterpolationMode = 7 : e.Graphics.DrawImageUnscaled(B, 0, 0) : B.Dispose()
    End Sub

End Class

#End Region

#Region " Minimize Button "

Class FlatMini : Inherits Control

#Region " Properties "

#Region " Colors "

    Private _BaseColor As Color = Color.FromArgb(45, 47, 49)
    <ComponentModel.Category("Colors")> Public Property BaseColor As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _TextColor As Color = Color.FromArgb(243, 243, 243)
    <ComponentModel.Category("Colors")> Public Property TextColor As Color
        Get
            Return _TextColor
        End Get
        Set(value As Color)
            _TextColor = value
        End Set
    End Property

#End Region

#Region " Mouse States "

    Private State As MouseState = MouseState.None

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e) : State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e) : State = MouseState.Down : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e) : State = MouseState.None : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e) : State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : Invalidate()
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e) : Select Case FindForm.WindowState
            Case FormWindowState.Normal : FindForm.WindowState = FormWindowState.Minimized
            Case FormWindowState.Maximized : FindForm.WindowState = FormWindowState.Minimized
        End Select
    End Sub

#End Region

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e) : Size = New Size(18, 18)
    End Sub

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        DoubleBuffered = True : BackColor = Color.White : Size = New Size(18, 18) : Anchor = AnchorStyles.Top Or AnchorStyles.Right : Font = New Font("Marlett", 12)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim B As New Bitmap(Width, Height)
        Dim G As Graphics = Graphics.FromImage(B)
        Dim Base As New Rectangle(0, 0, Width, Height)
        G.SmoothingMode = 2 : G.PixelOffsetMode = 2 : G.TextRenderingHint = 5 : G.Clear(BackColor)

        '-- Base
        G.FillRectangle(New SolidBrush(_BaseColor), Base)
        '-- Minimize
        G.DrawString("0", Font, New SolidBrush(TextColor), New Rectangle(2, 1, Width, Height), CenterSF)

        Select Case State
            Case MouseState.Over : G.FillRectangle(New SolidBrush(Color.FromArgb(30, Color.White)), Base)
            Case MouseState.Down : G.FillRectangle(New SolidBrush(Color.FromArgb(30, Color.Black)), Base)
        End Select

        MyBase.OnPaint(e) : G.Dispose() : e.Graphics.InterpolationMode = 7 : e.Graphics.DrawImageUnscaled(B, 0, 0) : B.Dispose()
    End Sub

End Class

#End Region

#End Region

#End Region

#Region " Button "

Class FlatButton : Inherits Control

#Region " Variables "

    Private W, H As Integer
    Private _Rounded As Boolean = False
    Private State As MouseState = MouseState.None

#Region " Colors "

    Private _Hover As Boolean = True
    Public Property Hover As Boolean
        Get
            Return _Hover
        End Get
        Set(value As Boolean)
            _Hover = value
        End Set
    End Property

    Private _BaseColor As Color = _FlatColor
    <ComponentModel.Category("Colors")> Public Property BaseColor As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _TextColor As Color = Color.FromArgb(243, 243, 243)
    <ComponentModel.Category("Colors")> Public Property TextColor As Color
        Get
            Return _TextColor
        End Get
        Set(value As Color)
            _TextColor = value
        End Set
    End Property

    <ComponentModel.Category("Options")> Public Property Rounded As Boolean
        Get
            Return _Rounded
        End Get
        Set(value As Boolean)
            _Rounded = value
        End Set
    End Property

#End Region

#Region " Mouse States "

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        State = MouseState.Down : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        State = MouseState.Over : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        State = MouseState.Over : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        State = MouseState.None : Invalidate()
    End Sub

#End Region

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor, True)
        DoubleBuffered = True
        Size = New Size(106, 32)
        BackColor = Color.Transparent
        Font = New Font("Segoe UI", 12)
        Cursor = Cursors.Hand
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        B = New Bitmap(Width, Height) : G = Graphics.FromImage(B)
        W = Width - 1 : H = Height - 1
        Dim GP As Drawing2D.GraphicsPath
        Dim Base As New Rectangle(0, 0, W, H)
        G.SmoothingMode = 2
        G.PixelOffsetMode = 2
        G.TextRenderingHint = 5
        G.Clear(BackColor)
        If _Hover = False Then
            If Rounded Then
                GP = RoundRec(Base, 6)
                G.FillPath(New SolidBrush(_BaseColor), GP)
                G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
            Else
                G.FillRectangle(New SolidBrush(_BaseColor), Base)
                G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
            End If
        Else
            Select Case State
                Case MouseState.None
                    If Rounded Then
                        GP = RoundRec(Base, 6)
                        G.FillPath(New SolidBrush(_BaseColor), GP)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    Else
                        G.FillRectangle(New SolidBrush(_BaseColor), Base)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    End If
                Case MouseState.Over
                    If Rounded Then
                        GP = RoundRec(Base, 6)
                        G.FillPath(New SolidBrush(_BaseColor), GP)
                        G.FillPath(New SolidBrush(Color.FromArgb(20, Color.White)), GP)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    Else
                        G.FillRectangle(New SolidBrush(_BaseColor), Base)
                        G.FillRectangle(New SolidBrush(Color.FromArgb(20, Color.White)), Base)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    End If
                Case MouseState.Down
                    If Rounded Then
                        GP = RoundRec(Base, 6)
                        G.FillPath(New SolidBrush(_BaseColor), GP)
                        G.FillPath(New SolidBrush(Color.FromArgb(20, Color.Black)), GP)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    Else
                        G.FillRectangle(New SolidBrush(_BaseColor), Base)
                        G.FillRectangle(New SolidBrush(Color.FromArgb(20, Color.Black)), Base)
                        G.DrawString(Text, Font, New SolidBrush(_TextColor), Base, CenterSF)
                    End If
            End Select
        End If
        MyBase.OnPaint(e)
        G.Dispose()
        e.Graphics.InterpolationMode = 7
        e.Graphics.DrawImageUnscaled(B, 0, 0)
        B.Dispose()
    End Sub

End Class

#End Region

#Region " Context Menu "

Class FlatContextMenuStrip : Inherits ContextMenuStrip

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e) : Invalidate()
    End Sub

    Sub New()
        MyBase.New()
        Renderer = New ToolStripProfessionalRenderer(New TColorTable())
        ShowImageMargin = False
        ForeColor = Color.White
        Font = New Font("Segoe UI", 8)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.TextRenderingHint = 5
    End Sub

#Region " TColorTable "

    Class TColorTable : Inherits ProfessionalColorTable

#Region " Properties "

#Region " Colors "

        Private BackColor As Color = Color.FromArgb(45, 47, 49)
        <ComponentModel.Category("Colors")> Public Property C_BackColor As Color
            Get
                Return BackColor
            End Get
            Set(value As Color)
                BackColor = value
            End Set
        End Property

        Private CheckedColor As Color = _FlatColor
        <ComponentModel.Category("Colors")> Public Property C_CheckedColor As Color
            Get
                Return CheckedColor
            End Get
            Set(value As Color)
                CheckedColor = value
            End Set
        End Property

        Private BorderColor As Color = Color.FromArgb(53, 58, 60)
        <ComponentModel.Category("Colors")> Public Property C_BorderColor As Color
            Get
                Return BorderColor
            End Get
            Set(value As Color)
                BorderColor = value
            End Set
        End Property

#End Region

#End Region

#Region " Overrides "

        Public Overrides ReadOnly Property ButtonSelectedBorder As Color
            Get
                Return BackColor
            End Get
        End Property
        Public Overrides ReadOnly Property CheckBackground() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property CheckPressedBackground() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property CheckSelectedBackground() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientBegin() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientEnd() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientMiddle() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property MenuBorder() As Color
            Get
                Return BorderColor
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemBorder() As Color
            Get
                Return BorderColor
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelected() As Color
            Get
                Return CheckedColor
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorDark() As Color
            Get
                Return BorderColor
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripDropDownBackground() As Color
            Get
                Return BackColor
            End Get
        End Property

#End Region

    End Class

#End Region

End Class

#End Region

#Region " Label "

Class FlatLabel : Inherits Label

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e) : Invalidate()
    End Sub

    Sub New()
        SetStyle(ControlStyles.SupportsTransparentBackColor, True)
        Font = New Font("Segoe UI", 8)
        ForeColor = Color.White
        BackColor = Color.Transparent
        Text = Text
    End Sub

End Class

#End Region

#Region " ComboBox "

Class FlatComboBox : Inherits ComboBox

#Region " Variables "

#Region " Declarations "

    <Runtime.InteropServices.DllImport("user32.dll")> Public Shared Function GetWindowRect(hWnd As IntPtr, <Runtime.InteropServices.Out> ByRef lpRect As RECT) As Boolean : End Function
    <Runtime.InteropServices.DllImport("user32.dll")> Public Shared Function GetWindowDC(hWnd As IntPtr) As IntPtr : End Function
    <Runtime.InteropServices.DllImport("user32.dll")> Public Shared Function ReleaseDC(hWnd As IntPtr, hDC As IntPtr) As Integer : End Function
    <Runtime.InteropServices.DllImport("user32.dll")> Public Shared Function SetFocus(hWnd As IntPtr) As IntPtr : End Function
    <Runtime.InteropServices.DllImport("user32.dll")> Public Shared Function GetComboBoxInfo(hWnd As IntPtr, ByRef pcbi As COMBOBOXINFO) As Boolean : End Function
    <Runtime.InteropServices.DllImport("gdi32.dll")> Public Shared Function ExcludeClipRect(hdc As IntPtr, nLeftRect As Integer, nTopRect As Integer, nRightRect As Integer, nBottomRect As Integer) As Integer : End Function
    <Runtime.InteropServices.DllImport("gdi32.dll")> Public Shared Function CreatePen(enPenStyle As PenStyles, nWidth As Integer, crColor As Integer) As IntPtr : End Function
    <Runtime.InteropServices.DllImport("gdi32.dll")> Public Shared Function SelectObject(hdc As IntPtr, hObject As IntPtr) As IntPtr : End Function
    <Runtime.InteropServices.DllImport("gdi32.dll")> Public Shared Function DeleteObject(hObject As IntPtr) As Boolean : End Function
    <Runtime.InteropServices.DllImport("gdi32.dll")> Public Shared Sub Rectangle(hdc As IntPtr, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) : End Sub

#End Region

#Region " Structures "

    <Serializable, Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)> Public Structure RECT : Public Left As Integer : Public Top As Integer : Public Right As Integer : Public Bottom As Integer : End Structure
    <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)> Public Structure COMBOBOXINFO : Public cbSize As Int32 : Public rcItem As RECT : Public rcButton As RECT : Public buttonState As ComboBoxButtonState : Public hwndCombo As IntPtr : Public hwndEdit As IntPtr : Public hwndList As IntPtr : End Structure
    Public Enum PenStyles : PS_SOLID = 0 : PS_DASH = 1 : PS_DOT = 2 : PS_DASHDOT = 3 : PS_DASHDOTDOT = 4 : End Enum
    Public Enum ComboBoxButtonState : STATE_SYSTEM_NONE = 0 : STATE_SYSTEM_INVISIBLE = &H8000 : STATE_SYSTEM_PRESSED = &H8 : End Enum

#End Region

#Region " Mouse States "

    Public Enum STImageMode
        Normal = 0 : Fill = 1
    End Enum

    Public Enum MouseState As Byte
        None = 0 : Hover = 1 : Down = 2 : Block = 3
    End Enum

    Public State As MouseState = MouseState.None

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e) : If e.Button = Windows.Forms.MouseButtons.Left Then State = MouseState.Hover : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e) : If e.Button = Windows.Forms.MouseButtons.Left Then State = MouseState.Down : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : State = MouseState.Hover : Invalidate()
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        State = MouseState.None : Invalidate() : MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnDropDown(e As EventArgs)
        _dropDownCheck.Start() : MyBase.OnDropDown(e)
    End Sub

    Protected Overrides Sub OnDropDownClosed(e As EventArgs)
        MyBase.OnDropDownClosed(e) : _dropDownCheck.Stop() : State = MouseState.None : Invalidate()
    End Sub

    Public Event ItemHover(index As Integer)
    Private _dropDownCheck As New Timer

#End Region

#Region " Properties "

#Region " Colors "

#Region " BaseColor "

    Private _BackColor As Color = Color.Empty
    <ComponentModel.Browsable(False)> Overrides Property BackColor As Color
        Get
            Return _BackColor
        End Get
        Set(value As Color)
            _BackColor = value : Invalidate()
        End Set
    End Property

    Private _BaseColor1 As Color = Color.FromArgb(35, 35, 35)
    Property BaseColor1 As Color
        Get
            Return _BaseColor1
        End Get
        Set(value As Color)
            _BaseColor1 = value : Invalidate()
        End Set
    End Property

    Private _BaseColor2 As Color = Color.FromArgb(50, 50, 50)
    Property BaseColor2 As Color
        Get
            Return _BaseColor2
        End Get
        Set(value As Color)
            _BaseColor2 = value : Invalidate()
        End Set
    End Property

#End Region

#Region " ForeColor "

    Private _ForeColor As Color = Color.Empty
    <ComponentModel.Browsable(False)> Overrides Property ForeColor As Color
        Get
            Return _ForeColor
        End Get
        Set(value As Color)
            _ForeColor = value : Invalidate()
        End Set
    End Property

    Private _ForeColor1 As Color = Color.White
    Property ForeColor1 As Color
        Get
            Return _ForeColor1
        End Get
        Set(value As Color)
            _ForeColor1 = value : Invalidate()
        End Set
    End Property

    Private _ForeColor2 As Color = Color.Black
    Property ForeColor2 As Color
        Get
            Return _ForeColor2
        End Get
        Set(value As Color)
            _ForeColor2 = value : Invalidate()
        End Set
    End Property

    Private _HoverColor1 As Color = Color.White
    Property HoverColor1 As Color
        Get
            Return _HoverColor1
        End Get
        Set(value As Color)
            _HoverColor1 = value : Invalidate()
        End Set
    End Property

    Private _HoverColor2 As Color = Color.Black
    Property HoverColor2 As Color
        Get
            Return _HoverColor2
        End Get
        Set(value As Color)
            _HoverColor2 = value : Invalidate()
        End Set
    End Property
#End Region

#Region " ItemColor "

    Private _ItemColor As Color = Color.FromArgb(102, 102, 102)
    Property ItemColor As Color
        Get
            Return _ItemColor
        End Get
        Set(value As Color)
            _ItemColor = value : Invalidate()
        End Set
    End Property

    Private _ItemBackColor As Color = Color.FromArgb(55, 55, 55)
    Property ItemBackColor As Color
        Get
            Return _ItemBackColor
        End Get
        Set(value As Color)
            _ItemBackColor = value : Invalidate()
        End Set
    End Property

    Private _ItemFont As Font = Font
    Property ItemFont As Font
        Get
            Return _ItemFont
        End Get
        Set(value As Font)
            _ItemFont = value : Invalidate()
        End Set
    End Property

    Private _ItemHoverColor As Color = Color.FromArgb(102, 102, 102)
    Property ItemHoverColor As Color
        Get
            Return _ItemHoverColor
        End Get
        Set(value As Color)
            _ItemHoverColor = value : Invalidate()
        End Set
    End Property

    Private _ItemHoverBackColor As Color = Color.FromArgb(65, 65, 65)
    Property ItemHoverBackColor As Color
        Get
            Return _ItemHoverBackColor
        End Get
        Set(value As Color)
            _ItemHoverBackColor = value : Invalidate()
        End Set
    End Property

    Private _ItemHoverFont As Font = Font
    Property ItemHoverFont As Font
        Get
            Return _ItemHoverFont
        End Get
        Set(value As Font)
            _ItemHoverFont = value : Invalidate()
        End Set
    End Property

    Private _ItemSelectedColor As Color = Color.FromArgb(102, 102, 102)
    Property ItemSelectedColor As Color
        Get
            Return _ItemSelectedColor
        End Get
        Set(value As Color)
            _ItemSelectedColor = value : Invalidate()
        End Set
    End Property

    Private _ItemSelectedBackColor As Color = Color.Blue
    Property ItemSelectedBackColor As Color
        Get
            Return _ItemSelectedBackColor
        End Get
        Set(value As Color)
            _ItemSelectedBackColor = value : Invalidate()
        End Set
    End Property

    Private _ItemSelectedFont As Font = Font
    Property ItemSelectedFont As Font
        Get
            Return _ItemSelectedFont
        End Get
        Set(value As Font)
            _ItemSelectedFont = value : Invalidate()
        End Set
    End Property

#End Region

#Region " BorderColor "

    Private _BorderColor1 As Color = Color.FromArgb(35, 35, 35)
    Property BorderColor1 As Color
        Get
            Return _BorderColor1
        End Get
        Set(value As Color)
            _BorderColor1 = value : Invalidate()
        End Set
    End Property

    Private _BorderColor2 As Color = Color.FromArgb(65, 65, 65)
    Property BorderColor2 As Color
        Get
            Return _BorderColor2
        End Get
        Set(value As Color)
            _BorderColor2 = value : Invalidate()
        End Set
    End Property

    Private _BorderColor3 As Color = Color.Empty
    Property BorderColor3 As Color
        Get
            Return _BorderColor3
        End Get
        Set(value As Color)
            _BorderColor3 = value : Invalidate()
        End Set
    End Property

    Private _DropdownBorderColor As Color = Color.Gray
    Property DropdownBorderColor As Color
        Get
            Return _DropdownBorderColor
        End Get
        Set(value As Color)
            _DropdownBorderColor = value : Invalidate()
        End Set
    End Property

#End Region

#Region " Gradient "

    Private _GradientColor1 As Color = Color.FromArgb(60, 60, 60)
    Property GradientColor1 As Color
        Get
            Return _GradientColor1
        End Get
        Set(value As Color)
            _GradientColor1 = value : Invalidate()
        End Set
    End Property

    Private _GradientColor2 As Color = Color.FromArgb(55, 55, 55)
    Property GradientColor2 As Color
        Get
            Return _GradientColor2
        End Get
        Set(value As Color)
            _GradientColor2 = value : Invalidate()
        End Set
    End Property

    Private _GradientAngle As Integer = 90
    Property GradientAngle As Integer
        Get
            Return _GradientAngle
        End Get
        Set(value As Integer)
            If value > 360 Then value = 360
            If value < 1 Then value = 1
            _GradientAngle = value : Invalidate()
        End Set
    End Property

    Private _GradientTransparency As Integer = 0
    Property GradientTransparency As Integer
        Get
            Return _GradientTransparency
        End Get
        Set(value As Integer)
            If value > 255 Then value = 255
            If value < 0 Then value = 0
            _GradientTransparency = value : Invalidate()
        End Set
    End Property

#End Region

#End Region

#Region " Images "

    Private _BackgroundImageLayout As ImageLayout
    <ComponentModel.Browsable(False)> Overrides Property BackgroundImageLayout As ImageLayout
        Get
            Return _BackgroundImageLayout
        End Get
        Set(value As ImageLayout)
            _BackgroundImageLayout = value
        End Set
    End Property

    Private _BackgroundImage As Image
    <ComponentModel.Browsable(False)> Overrides Property BackgroundImage As Image
        Get
            Return _BackgroundImage
        End Get
        Set(value As Image)
            _BackgroundImage = value
        End Set
    End Property

    Private _Image As Image = Nothing
    Property Image As Image
        Get
            Return _Image
        End Get
        Set(value As Image)
            _Image = value : Invalidate()
        End Set
    End Property

    Private _ImageSize As New Size(19, 17)
    Property ImageSize As Size
        Get
            Return _ImageSize
        End Get
        Set(value As Size)
            _ImageSize = value : Invalidate()
        End Set
    End Property

    Private _ImageAlign As HorizontalAlignment = HorizontalAlignment.Center
    Property ImageAlign() As HorizontalAlignment
        Get
            Return _ImageAlign
        End Get
        Set(value As HorizontalAlignment)
            _ImageAlign = value : Invalidate()
        End Set
    End Property

    Private _ImageMode As STImageMode = STImageMode.Normal
    Property ImageMode() As STImageMode
        Get
            Return _ImageMode
        End Get
        Set(value As STImageMode)
            _ImageMode = value : Invalidate()
        End Set
    End Property

#End Region

#Region " Others "

    Private _TextAlign As HorizontalAlignment = HorizontalAlignment.Center
    Property TextAlign() As HorizontalAlignment
        Get
            Return _TextAlign
        End Get
        Set(value As HorizontalAlignment)
            _TextAlign = value : Invalidate()
        End Set
    End Property

    Private _ItemTextAlign As HorizontalAlignment = HorizontalAlignment.Left
    Property ItemTextAlign() As HorizontalAlignment
        Get
            Return _ItemTextAlign
        End Get
        Set(value As HorizontalAlignment)
            _ItemTextAlign = value : Invalidate()
        End Set
    End Property

    Private _DoubleText As Boolean = False
    Property DoubleText As Boolean
        Get
            Return _DoubleText
        End Get
        Set(value As Boolean)
            _DoubleText = value : Invalidate()
        End Set
    End Property

    Private _Rounded As Integer = 7
    Property Rounded As Integer
        Get
            Return _Rounded
        End Get
        Set(value As Integer)
            If value > 20 Then value = 20
            If value < 1 Then value = 1
            _Rounded = value : Invalidate()
        End Set
    End Property

#End Region

#End Region

    Public IsFocused As Boolean = False

#End Region

#Region " Events "

    Sub TextGotFocus(sender As Object, e As EventArgs)
        IsFocused = True
        Invalidate()
        OnGotFocus(e)
    End Sub

    Sub TextLostFocus(sender As Object, e As EventArgs)
        IsFocused = False
        SelectionLength = 0
        OnLostFocus(e)
        Invalidate()
    End Sub

#End Region

#Region " Constructor "

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor, True) : SetStyle(DirectCast(139286, ControlStyles), True) : SetStyle(ControlStyles.Selectable, False)
        DoubleBuffered = True : DrawMode = DrawMode.OwnerDrawFixed : DropDownStyle = ComboBoxStyle.DropDownList : Font = New Font("Corbel", 10)
        AddHandler _dropDownCheck.Tick, AddressOf DropDownCheck_Tick
    End Sub

#End Region

#Region " Paint "

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim G As Graphics = e.Graphics : G.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit : G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias : G.Clear(Color.Black)

        Dim GP1 As Drawing2D.GraphicsPath = CreateRound(0, 0, Width - 1, Height - 1, Rounded)
        Dim GP2 As Drawing2D.GraphicsPath = CreateRound(1, 1, Width - 3, Height - 3, Rounded)
        Dim GB1 As New Drawing2D.LinearGradientBrush(ClientRectangle, GradientColor1, GradientColor2, GradientAngle)
        G.SetClip(GP1) : G.FillRectangle(GB1, ClientRectangle) : G.ResetClip()

        G.DrawPath(New Pen(BorderColor1), GP1)
        G.DrawPath(New Pen(BorderColor2), GP2)

        G.DrawLine(New Pen(BorderColor1), Width - 22, 0, Width - 22, Height)
        G.DrawLine(New Pen(BorderColor2), Width - 23, 1, Width - 23, Height - 2)
        G.DrawLine(New Pen(BorderColor2), Width - 21, 1, Width - 21, Height - 2)

        'If DoubleText = True Then
        '    G.DrawLine(New Pen(ForeColor2, 2.0F), Width - 15, 10, Width - 11, 13)
        '    G.DrawLine(New Pen(ForeColor2, 2.0F), Width - 7, 10, Width - 11, 13)
        '    G.DrawLine(New Pen(ForeColor2), Width - 11, 13, Width - 11, 14)
        '    DrawText(G, Text, Font, ForeColor2, 1, 1, , , -23)
        'End If

        G.DrawLine(New Pen(ForeColor1, 2.0F), Width - 16, 9, Width - 12, 12)
        G.DrawLine(New Pen(ForeColor1, 2.0F), Width - 8, 9, Width - 12, 12)
        G.DrawLine(New Pen(ForeColor1), Width - 12, 12, Width - 12, 13)

        DrawText(G, Text, Font, ForeColor1, , , , , -23)

        'Select Case State
        '    Case MouseState.None, MouseState.Hover

        '        G.DrawPath(New Pen(BorderColor1), GP1)
        '        G.DrawPath(New Pen(BorderColor2), GP2)

        '        G.DrawLine(New Pen(BorderColor1), Width - 22, 0, Width - 22, Height)
        '        G.DrawLine(New Pen(BorderColor2), Width - 23, 1, Width - 23, Height - 2)
        '        G.DrawLine(New Pen(BorderColor2), Width - 21, 1, Width - 21, Height - 2)

        '        If DoubleText = True Then
        '            G.DrawLine(New Pen(ForeColor2, 2.0F), Width - 15, 10, Width - 11, 13)
        '            G.DrawLine(New Pen(ForeColor2, 2.0F), Width - 7, 10, Width - 11, 13)
        '            G.DrawLine(New Pen(ForeColor2), Width - 11, 13, Width - 11, 14)
        '            DrawText(G, Text, Font, ForeColor2, 1, 1, , , -23)
        '        End If

        '        G.DrawLine(New Pen(ForeColor1, 2.0F), Width - 16, 9, Width - 12, 12)
        '        G.DrawLine(New Pen(ForeColor1, 2.0F), Width - 8, 9, Width - 12, 12)
        '        G.DrawLine(New Pen(ForeColor1), Width - 12, 12, Width - 12, 13)

        '        DrawText(G, Text, Font, ForeColor1, , , , , -23)
        '    'Case MouseState.Block
        '    '    G.DrawPath(New Pen(BorderColor3), GP1)
        '    '    G.DrawPath(New Pen(BorderColor2), GP2)

        '    '    G.DrawLine(New Pen(BorderColor3), Width - 22, 0, Width - 22, Height)
        '    '    G.DrawLine(New Pen(BorderColor2), Width - 23, 1, Width - 23, Height - 2)
        '    '    G.DrawLine(New Pen(BorderColor2), Width - 21, 1, Width - 21, Height - 2)

        '    '    If DoubleText = True Then
        '    '        G.DrawLine(New Pen(HoverColor2, 2.0F), Width - 15, 10, Width - 11, 13)
        '    '        G.DrawLine(New Pen(HoverColor2, 2.0F), Width - 7, 10, Width - 11, 13)
        '    '        G.DrawLine(New Pen(HoverColor2), Width - 11, 13, Width - 11, 14)
        '    '        DrawText(G, Text, Font, ForeColor2, 1, 1, , , -23)
        '    '    End If

        '    '    G.DrawLine(New Pen(HoverColor1, 2.0F), Width - 16, 9, Width - 12, 12)
        '    '    G.DrawLine(New Pen(HoverColor1, 2.0F), Width - 8, 9, Width - 12, 12)
        '    '    G.DrawLine(New Pen(HoverColor1), Width - 12, 12, Width - 12, 13)
        '    '    DrawText(G, Text, Font, ForeColor1, , , , , -23)
        '    Case MouseState.Down
        '        G.DrawPath(New Pen(BorderColor3), GP1)
        '        G.DrawPath(New Pen(BorderColor2), GP2)

        '        G.DrawLine(New Pen(BorderColor3), Width - 22, 0, Width - 22, Height)
        '        G.DrawLine(New Pen(BorderColor2), Width - 23, 1, Width - 23, Height - 2)
        '        G.DrawLine(New Pen(BorderColor2), Width - 21, 1, Width - 21, Height - 2)

        '        If DoubleText = True Then
        '            G.DrawLine(New Pen(HoverColor2, 2.0F), Width - 15, 10, Width - 11, 13)
        '            G.DrawLine(New Pen(HoverColor2, 2.0F), Width - 7, 10, Width - 11, 13)
        '            G.DrawLine(New Pen(HoverColor2), Width - 11, 13, Width - 11, 14)
        '            DrawText(G, Text, Font, ForeColor2, 1, 1, , , -23)
        '        End If

        '        G.DrawLine(New Pen(HoverColor1, 2.0F), Width - 16, 9, Width - 12, 12)
        '        G.DrawLine(New Pen(HoverColor1, 2.0F), Width - 8, 9, Width - 12, 12)
        '        G.DrawLine(New Pen(HoverColor1), Width - 12, 12, Width - 12, 13)
        '        DrawText(G, Text, Font, ForeColor1, , , , , -23)
        'End Select

    End Sub

#Region " DrawItem "

    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        If e.Index <> -1 Then
            Dim TItem As String = Items(e.Index) : If TItem.Substring(0, 1) = "@" Then TItem = TItem.Substring(1)
            If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
                If TItem.Substring(0, 1) = "|" Then
                    Dim Header As New Drawing2D.LinearGradientBrush(New Rectangle(e.Bounds.X, e.Bounds.Y - 1, e.Bounds.Width, e.Bounds.Height), Color.FromArgb(25, 25, 25), Color.FromArgb(42, 42, 42), 270)
                    e.Graphics.FillRectangle(Header, New Rectangle(e.Bounds.X, e.Bounds.Y - 1, e.Bounds.Width, e.Bounds.Height))
                    Dim HeaderHatch As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.Trellis, Color.FromArgb(35, Color.Black), Color.Transparent)
                    e.Graphics.FillRectangle(HeaderHatch, New Rectangle(e.Bounds.X, e.Bounds.Y - 1, e.Bounds.Width, e.Bounds.Height))
                    Select Case ItemTextAlign
                        Case HorizontalAlignment.Left : e.Graphics.DrawString(TItem.Substring(1), ItemSelectedFont, New SolidBrush(ItemSelectedColor), e.Bounds)
                        Case HorizontalAlignment.Center : e.Graphics.DrawString(TItem.Substring(1), ItemSelectedFont, New SolidBrush(ItemSelectedColor), e.Bounds.Width / 2 - (MeasureString(TItem.Substring(1), ItemSelectedFont).Width / 2), e.Bounds.Y)
                        Case HorizontalAlignment.Right : e.Graphics.DrawString(TItem.Substring(1), ItemSelectedFont, New SolidBrush(ItemSelectedColor), e.Bounds.Width - (MeasureString(TItem.Substring(1), ItemSelectedFont).Width), e.Bounds.Y)
                    End Select
                Else : e.Graphics.FillRectangle(New SolidBrush(ItemHoverBackColor), e.Bounds.X + 1, e.Bounds.Y, e.Bounds.Width - 2, e.Bounds.Height - 1)
                    Select Case ItemTextAlign
                        Case HorizontalAlignment.Left : e.Graphics.DrawString(TItem, ItemHoverFont, New SolidBrush(ItemHoverColor), 5, e.Bounds.Y)
                        Case HorizontalAlignment.Center : e.Graphics.DrawString(TItem, ItemHoverFont, New SolidBrush(ItemHoverColor), e.Bounds.Width / 2 - (MeasureString(TItem, ItemHoverFont).Width / 2), e.Bounds.Y)
                        Case HorizontalAlignment.Right : e.Graphics.DrawString(TItem, ItemHoverFont, New SolidBrush(ItemHoverColor), e.Bounds.Width - (MeasureString(TItem, ItemHoverFont).Width), e.Bounds.Y)
                    End Select
                End If
            Else
                If TItem.Substring(0, 1) = "|" Then
                    Dim Header As New Drawing2D.LinearGradientBrush(New Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 1), Color.FromArgb(25, 25, 25), Color.FromArgb(42, 42, 42), 270)
                    e.Graphics.FillRectangle(Header, New Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 1))
                    Dim HeaderHatch As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.Trellis, Color.FromArgb(35, Color.Black), Color.Transparent)
                    e.Graphics.FillRectangle(HeaderHatch, New Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 1))
                    Select Case ItemTextAlign
                        Case HorizontalAlignment.Left : e.Graphics.DrawString(TItem.Substring(1), ItemHoverFont, New SolidBrush(ItemHoverColor), e.Bounds)
                        Case HorizontalAlignment.Center : e.Graphics.DrawString(TItem.Substring(1), ItemHoverFont, New SolidBrush(ItemHoverColor), e.Bounds.Width / 2 - (MeasureString(TItem.Substring(1), ItemHoverFont).Width / 2), e.Bounds.Y)
                        Case HorizontalAlignment.Right : e.Graphics.DrawString(TItem.Substring(1), ItemHoverFont, New SolidBrush(ItemHoverColor), e.Bounds.Width - (MeasureString(TItem.Substring(1), ItemHoverFont).Width), e.Bounds.Y)
                    End Select
                Else : e.Graphics.FillRectangle(New SolidBrush(ItemBackColor), e.Bounds.X + 1, e.Bounds.Y, e.Bounds.Width - 2, e.Bounds.Height - 1)
                    Select Case ItemTextAlign
                        Case HorizontalAlignment.Left : e.Graphics.DrawString(TItem, ItemFont, New SolidBrush(ItemColor), 5, e.Bounds.Y)
                        Case HorizontalAlignment.Center : e.Graphics.DrawString(TItem, ItemFont, New SolidBrush(ItemColor), e.Bounds.Width / 2 - (MeasureString(TItem, ItemFont).Width / 2), e.Bounds.Y)
                        Case HorizontalAlignment.Right : e.Graphics.DrawString(TItem, ItemFont, New SolidBrush(ItemColor), e.Bounds.Width - (MeasureString(TItem, ItemFont).Width), e.Bounds.Y)
                    End Select
                End If
            End If : RaiseEvent ItemHover(e.Index)
        End If
    End Sub

#End Region

#Region " DrawText "

    Private Sub DrawText(ByRef G As Graphics, DText As String, DFont As Font, DColor As Color, Optional XOffset As Integer = 0, Optional YOffset As Integer = 0, Optional DrawImage As Boolean = True, Optional CalcImage As Boolean = False, Optional WOffset As Integer = 0, Optional HOffset As Integer = 0)
        Dim TextLocation As PointF : Dim TextSize As New Size(G.MeasureString(DText, DFont).Width, G.MeasureString(DText, DFont).Height)
        If TextAlign = HorizontalAlignment.Left Then TextLocation = New PointF(5 + XOffset, ((Height + HOffset) \ 2 - G.MeasureString(DText, DFont).Height / 2) + YOffset)
        If TextAlign = HorizontalAlignment.Center Then TextLocation = New PointF(((((Width + WOffset) \ 2) - G.MeasureString(DText, DFont).Width / 2) + XOffset), ((Height + HOffset) \ 2 - G.MeasureString(DText, DFont).Height / 2) + YOffset)
        If TextAlign = HorizontalAlignment.Right Then TextLocation = New PointF((Width + WOffset - (G.MeasureString(DText, DFont).Width + 5)) + XOffset, ((Height + HOffset) \ 2 - G.MeasureString(DText, DFont).Height / 2) + YOffset)

        If ImageSize.Width > Width - 23 Then ImageSize = New Size(Width - 23, ImageSize.Height)
        If ImageSize.Height > Height - 4 Then ImageSize = New Size(ImageSize.Width, Height - 4)
        Dim ImageLocation As PointF
        If ImageAlign = HorizontalAlignment.Left Then ImageLocation = New PointF(3, ((Height - 1) / 2) - (ImageSize.Height / 2))
        If ImageAlign = HorizontalAlignment.Center Then ImageLocation = New PointF((((Width - WOffset) / 2) - (ImageSize.Width / 2) + WOffset), ((Height - 1) / 2) - (ImageSize.Height / 2))
        If ImageAlign = HorizontalAlignment.Right Then ImageLocation = New PointF(Width + WOffset - ImageSize.Width, ((Height - 1) / 2) - (ImageSize.Height / 2))

        If Image Is Nothing Or DrawImage = False And CalcImage = False Then
            G.DrawString(DText, DFont, New SolidBrush(DColor), TextLocation)
        ElseIf ImageMode = STImageMode.Fill Then
            G.DrawImage(Image, 1, 1 + HOffset, Width + WOffset, Height - 2 + HOffset) : G.DrawString(DText, DFont, New SolidBrush(DColor), TextLocation)
        Else : G.DrawImage(Image, ImageLocation.X, ImageLocation.Y, ImageSize.Width, ImageSize.Height)
            Select Case TextAlign
                Case HorizontalAlignment.Left : If ImageAlign = HorizontalAlignment.Left Then G.DrawString(DText, DFont, New SolidBrush(DColor), ImageLocation.X + ImageSize.Width + 4 + XOffset, TextLocation.Y) Else G.DrawString(DText, DFont, New SolidBrush(DColor), TextLocation)
                Case HorizontalAlignment.Center : G.DrawString(DText, DFont, New SolidBrush(DColor), TextLocation)
                Case HorizontalAlignment.Right : If ImageAlign = HorizontalAlignment.Right Then G.DrawString(DText, DFont, New SolidBrush(DColor), ImageLocation.X - TextSize.Width - 4 + XOffset, TextLocation.Y) Else G.DrawString(DText, DFont, New SolidBrush(DColor), TextLocation)
            End Select
        End If
    End Sub

#End Region

#End Region

#Region " DropDown Border "

    Private Sub DropDownCheck_Tick(sender As Object, e As EventArgs)
        Dim m As Message = GetControlListBoxMessage(Handle) : WndProc(m)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case &H134 : MyBase.WndProc(m) : DrawNativeBorder(m.LParam)
            Case Else : MyBase.WndProc(m)
        End Select
    End Sub

    Public Function GetControlListBoxMessage(handle As IntPtr) As Message
        Dim m As New Message() : Dim info As New COMBOBOXINFO()
        info.cbSize = Runtime.InteropServices.Marshal.SizeOf(info)
        m.LParam = If(GetComboBoxInfo(handle, info), info.hwndList, IntPtr.Zero)
        m.WParam = IntPtr.Zero : m.HWnd = handle : m.Msg = &H134 : m.Result = IntPtr.Zero : Return m
    End Function

    Public Sub DrawNativeBorder(handle As IntPtr)
        Dim controlRect As RECT
        GetWindowRect(handle, controlRect) : controlRect.Right -= controlRect.Left : controlRect.Bottom -= controlRect.Top : controlRect.Top = Assign(controlRect.Left, 0)
        Dim dc As IntPtr = GetWindowDC(handle) : Dim clientRect As RECT = controlRect
        clientRect.Left += 1 : clientRect.Top += 1 : clientRect.Right -= 1 : clientRect.Bottom -= 1
        ExcludeClipRect(dc, clientRect.Left, clientRect.Top, clientRect.Right, clientRect.Bottom)
        Dim border As IntPtr = CreatePen(PenStyles.PS_SOLID, 1, RGB(DropdownBorderColor.R, DropdownBorderColor.G, DropdownBorderColor.B))
        Dim borderPen As IntPtr = SelectObject(dc, border)
        Rectangle(dc, controlRect.Left, controlRect.Top, controlRect.Right, controlRect.Bottom)
        SelectObject(dc, borderPen) : DeleteObject(border) : ReleaseDC(handle, dc) : SetFocus(handle)
    End Sub

    Function Assign(Of T)(ByRef target As T, value As T) As T
        target = value : Return value
    End Function

#End Region

End Class

#End Region

#Region " NSTextBox "

<ComponentModel.DefaultEvent("TextChanged")> Class NSTextBox : Inherits Control

#Region " Variables "

    Private WithEvents Base As TextBox
    Public IsFocused As Boolean = False

#Region " Properties "

#Region " Colors "

    Private _BackColor As Color = Color.FromArgb(55, 55, 55)
    Overrides Property BackColor As Color
        Get
            Return _BackColor
        End Get
        Set(value As Color)
            _BackColor = value
            If Base IsNot Nothing Then Base.BackColor = value
            Invalidate()
        End Set
    End Property

    Private _BorderColor As Color = Color.FromArgb(55, 55, 55)
    Property BorderColor As Color
        Get
            Return _BorderColor
        End Get
        Set(value As Color)
            _BorderColor = value
            Invalidate()
        End Set
    End Property

    Private _ForeColorBackup As Color = Color.FromArgb(102, 102, 102)
    Private _ForeColor1 As Color = Color.FromArgb(102, 102, 102)
    Property ForeColor1 As Color
        Get
            Return _ForeColor1
        End Get
        Set(value As Color)
            _ForeColor1 = value
            If Base IsNot Nothing Then Base.ForeColor = value : _ForeColorBackup = value
            Invalidate()
        End Set
    End Property

    Private _ForeColor2 As Color = Color.FromArgb(102, 102, 102)
    Property ForeColor2 As Color
        Get
            Return _ForeColor2
        End Get
        Set(value As Color)
            _ForeColor2 = value
            Invalidate()
        End Set
    End Property

#End Region

#Region " Others "

    Private _TextAlign As HorizontalAlignment = HorizontalAlignment.Left
    Property TextAlign() As HorizontalAlignment
        Get
            Return _TextAlign
        End Get
        Set(ByVal value As HorizontalAlignment)
            _TextAlign = value
            If Base IsNot Nothing Then Base.TextAlign = value
        End Set
    End Property

    Private _MaxLength As Integer = 32767
    Property MaxLength() As Integer
        Get
            Return _MaxLength
        End Get
        Set(ByVal value As Integer)
            _MaxLength = value
            If Base IsNot Nothing Then Base.MaxLength = value
        End Set
    End Property

    Private _ReadOnly As Boolean
    Property [ReadOnly]() As Boolean
        Get
            Return _ReadOnly
        End Get
        Set(ByVal value As Boolean)
            _ReadOnly = value
            If Base IsNot Nothing Then Base.ReadOnly = value
        End Set
    End Property

    Private _WordWrap As Boolean
    Property [WordWrap]() As Boolean
        Get
            Return _WordWrap
        End Get
        Set(ByVal value As Boolean)
            _WordWrap = value
            If Base IsNot Nothing Then Base.WordWrap = value
        End Set
    End Property

    Private _ScrollBars As ScrollBars
    Property [ScrollBars]() As ScrollBars
        Get
            Return _ScrollBars
        End Get
        Set(ByVal value As ScrollBars)
            _ScrollBars = value
            If Base IsNot Nothing Then Base.ScrollBars = value
        End Set
    End Property

    Private _HideSelection As Boolean
    Property [HideSelection]() As Boolean
        Get
            Return _HideSelection
        End Get
        Set(ByVal value As Boolean)
            _HideSelection = value
            If Base IsNot Nothing Then Base.HideSelection = value
        End Set
    End Property

    Private _UseSystemPasswordChar As Boolean
    Property UseSystemPasswordChar() As Boolean
        Get
            Return _UseSystemPasswordChar
        End Get
        Set(ByVal value As Boolean)
            _UseSystemPasswordChar = value
            If Base IsNot Nothing Then Base.UseSystemPasswordChar = value
        End Set
    End Property

    Private _Multiline As Boolean
    Property Multiline() As Boolean
        Get
            Return _Multiline
        End Get
        Set(ByVal value As Boolean)
            _Multiline = value
            If Base IsNot Nothing Then
                Base.Multiline = value
                If value Then Base.Height = Height - 11 Else Height = Base.Height + 11
            End If
        End Set
    End Property

    Private _SelectionStart As Integer
    Property SelectionStart() As Integer
        Get
            Return Base.SelectionStart
        End Get
        Set(ByVal value As Integer)
            _SelectionStart = value
            If Base IsNot Nothing Then Base.SelectionStart = value
        End Set
    End Property

    Private _SelectionLength As Integer
    Property SelectionLength() As Integer
        Get
            Return Base.SelectionLength
        End Get
        Set(ByVal value As Integer)
            _SelectionLength = value
            If Base IsNot Nothing Then Base.SelectionLength = value
        End Set
    End Property

    Private _SelectedText As String
    Property SelectedText() As String
        Get
            Return Base.SelectedText
        End Get
        Set(ByVal value As String)
            _SelectedText = value
            If Base IsNot Nothing Then Base.SelectedText = value
        End Set
    End Property

    Private _ShortcutsEnabled As Boolean
    Property ShortcutsEnabled() As Boolean
        Get
            Return _ShortcutsEnabled
        End Get
        Set(ByVal value As Boolean)
            _ShortcutsEnabled = value
            If Base IsNot Nothing Then Base.ShortcutsEnabled = value
        End Set
    End Property

    Private _Rounded As Integer = 7
    Property Rounded As Integer
        Get
            Return _Rounded
        End Get
        Set(value As Integer)
            If value > 30 Then value = 30
            If value < 0 Then value = 0
            _Rounded = value
            Invalidate()
        End Set
    End Property

    Private _ControlCursor As Cursor = Cursors.Default
    Property ControlCursor() As Cursor
        Get
            Return _ControlCursor
        End Get
        Set(ByVal value As Cursor)
            _ControlCursor = value
            Cursor = value
        End Set
    End Property

    Overloads Property Enabled() As Boolean
        Get
            Return Base.Enabled
        End Get
        Set(ByVal value As Boolean)
            Base.Enabled = value
        End Set
    End Property

    Private _ContextMenuStrip As ContextMenuStrip
    Overrides Property [ContextMenuStrip]() As ContextMenuStrip
        Get
            Return _ContextMenuStrip
        End Get
        Set(ByVal value As ContextMenuStrip)
            _ContextMenuStrip = value
            If Base IsNot Nothing Then Base.ContextMenuStrip = value
        End Set
    End Property

    Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            MyBase.Text = value
            If Base IsNot Nothing Then Base.Text = value
        End Set
    End Property

    Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(ByVal value As Font)
            MyBase.Font = value
            If Base IsNot Nothing Then
                Base.Font = value
                Base.Location = New Point(5, 5)
                Base.Width = Width - 8
                If Not _Multiline Then Height = Base.Height + 11
            End If
        End Set
    End Property

#End Region

#End Region

#End Region

#Region " Constructor "

    Sub New()
        SetStyle(139286, True) : SetStyle(ControlStyles.Selectable, True)
        ForeColor = Color.FromArgb(102, 102, 102)
        Base = New TextBox
        Base.Font = Font
        Base.Text = Text
        Base.MaxLength = _MaxLength
        Base.Multiline = _Multiline
        Base.ReadOnly = _ReadOnly
        Base.WordWrap = _WordWrap
        Base.ScrollBars = ScrollBars
        Base.UseSystemPasswordChar = _UseSystemPasswordChar
        Base.ForeColor = Color.White
        Base.BackColor = Color.FromArgb(50, 50, 50)
        Base.BorderStyle = BorderStyle.None

        Base.Location = New Point(5, 3)
        Base.Width = Width - 10
        Base.Height = Height - 11
        'If _Multiline Then Base.Height = Height - 11 Else Height = Base.Height + 11
        AddHandler Base.TextChanged, AddressOf OnBaseTextChanged
        AddHandler Base.KeyDown, AddressOf OnBaseKeyDown
        AddHandler Base.KeyPress, AddressOf OnBaseKeyPress
        AddHandler Base.GotFocus, AddressOf TextGotFocus
        AddHandler Base.LostFocus, AddressOf TextLostFocus
        AddHandler GotFocus, AddressOf FocusBase
        AddHandler LostFocus, AddressOf UnfocusBase
    End Sub

#End Region

#Region " Events "

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        If Not Controls.Contains(Base) Then Controls.Add(Base)
        MyBase.OnHandleCreated(e)
    End Sub

    Private Sub OnBaseTextChanged(s As Object, e As EventArgs)
        Text = Base.Text
    End Sub

    Private Sub OnBaseKeyDown(s As Object, e As KeyEventArgs)
        MyBase.OnKeyDown(e)
    End Sub

    Private Sub OnBaseKeyPress(s As Object, e As KeyPressEventArgs)
        MyBase.OnKeyPress(e)
    End Sub

    Sub UnfocusBase(sender As Object, e As EventArgs)
        IsFocused = False
        Invalidate()
    End Sub

    Sub FocusBase(sender As Object, e As EventArgs)
        Base.Focus()
    End Sub

    Private alreadyFocused As Boolean

    Private Sub Base_MouseDown(sender As Object, e As MouseEventArgs) Handles Base.MouseDown
        If e.Button <> MouseButtons.Left Then Exit Sub
    End Sub

    Private Sub Base_MouseUp(sender As Object, e As MouseEventArgs) Handles Base.MouseUp
        If e.Button <> MouseButtons.Left Then Exit Sub
        'If Me.SelectionLength = 0 Then If Me.alreadyFocused = False Then Me.SelectionStart = 0 : Me.SelectionLength = Me.Text.Length Else Me.SelectionLength = 0
        alreadyFocused = True
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        'If MouseButtons = MouseButtons.None Then Me.alreadyFocused = True : Me.SelectionStart = 0 : Me.SelectionLength = Me.Text.Length
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        'If Me.SelectionLength = 0 Then If Me.alreadyFocused = False Then Me.SelectionStart = 0 : Me.SelectionLength = Me.Text.Length Else Me.SelectionLength = 0
        alreadyFocused = True
    End Sub

    Sub TextGotFocus(sender As Object, e As EventArgs)
        IsFocused = True
        Base.ForeColor = ForeColor1
        Invalidate()
        MyBase.OnGotFocus(e)
    End Sub

    Sub TextLostFocus(sender As Object, e As EventArgs)
        alreadyFocused = False
        IsFocused = False
        Base.ForeColor = ForeColor2
        SelectionLength = 0
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        Base.Location = New Point(5, 5)
        Base.Width = Width - 10
        Base.Height = Height - 11
        MyBase.OnResize(e)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        Base.Focus()
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnEnter(e As EventArgs)
        Base.Focus()
        Invalidate()
        MyBase.OnEnter(e)
    End Sub

    Protected Overrides Sub OnLeave(e As EventArgs)
        Invalidate()
        MyBase.OnLeave(e)
        alreadyFocused = False
    End Sub

    Public Sub ScrollToCaret()
        Base.SelectionStart = SelectionStart
        Base.ScrollToCaret()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        On Error Resume Next
        If keyData = Keys.Delete Then MyBase.OnKeyDown(New KeyEventArgs(keyData))
        'If (keyData = Keys.Delete) Then
        '    Return True
        'Else
        Return MyBase.ProcessCmdKey(msg, keyData)
        'End If
    End Function

    Public Function Lines() As Array
        Return Base.Lines
    End Function

    Public Sub SelectAll()
        Base.SelectAll()
    End Sub

#End Region

#Region " Paint "

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Base.Location = New Point(5, 3)
        Base.Width = Width - 10
        Base.Height = Height - 11
        Dim G As Graphics = e.Graphics : G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias : G.Clear(BackColor)
        Dim GP1 As New Drawing2D.GraphicsPath
        Dim GP2 As New Drawing2D.GraphicsPath
        If _Rounded > 0 Then
            GP1 = CreateRound(0, 0, Width - 1, Height - 1, _Rounded) : GP2 = CreateRound(1, 1, Width - 3, Height - 3, _Rounded)
        Else
            GP1.AddRectangle(New Rectangle(0, 0, Width - 1, Height - 1)) : GP2.AddRectangle(New Rectangle(1, 1, Width - 3, Height - 3))
        End If
        Dim PB1 As New Drawing2D.PathGradientBrush(GP1)
        PB1.CenterColor = Color.FromArgb(50, 50, 50)
        PB1.SurroundColors = {Color.FromArgb(45, 45, 45)}
        PB1.FocusScales = New PointF(0.9F, 0.5F)
        G.FillPath(PB1, GP1)
        G.DrawPath(New Pen(BorderColor), GP1)
        G.DrawPath(New Pen(Color.FromArgb(35, 35, 35)), GP2)
        G.FillRectangle(New SolidBrush(BorderColor), -1, -1, 2, 2)
        G.FillRectangle(New SolidBrush(BorderColor), Width - 2, -1, Width + 1, 2)
        G.FillRectangle(New SolidBrush(BorderColor), -1, Height - 2, 2, Height + 1)
        G.FillRectangle(New SolidBrush(BorderColor), Width - 2, Height - 2, Width + 1, Height + 1)
    End Sub

#End Region

End Class

#End Region

#Region " FlatText "

Class FlatText : Inherits Control

#Region " Properties "

#Region " Colors "

    Private _BaseColor As Color = Color.FromArgb(50, 50, 50)
    <ComponentModel.Category("Colors")> Public Property BaseColor As Color
        Get
            Return _BaseColor
        End Get
        Set(value As Color)
            _BaseColor = value
        End Set
    End Property

    Private _TextColor As Color = Color.DarkGray
    <ComponentModel.Category("Colors")> Public Property TextColor As Color
        Get
            Return _TextColor
        End Get
        Set(value As Color)
            _TextColor = value
        End Set
    End Property

    Private _TextHoverColor As Color = Color.DarkGray
    <ComponentModel.Category("Colors")> Public Property TextHoverColor As Color
        Get
            Return _TextHoverColor
        End Get
        Set(value As Color)
            _TextHoverColor = value
        End Set
    End Property

#End Region

#Region " Mouse States "

    Private State As MouseState = MouseState.None

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e) : State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e) : State = MouseState.Down : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e) : State = MouseState.None : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e) : State = MouseState.Over : Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e) : Invalidate()
    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
    End Sub

#End Region

#End Region

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer, True)
        DoubleBuffered = True : BackColor = Color.White : Size = New Size(17, 16) : Font = New Font("Corbel", 9)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim B As New Bitmap(Width, Height)
        Dim G As Graphics = Graphics.FromImage(B)
        Dim Base As New Rectangle(0, 0, Width, Height)
        G.SmoothingMode = 2 : G.PixelOffsetMode = 2 : G.TextRenderingHint = 5 : G.Clear(BackColor)

        '-- Base
        G.FillRectangle(New SolidBrush(_BaseColor), Base)

        Select Case State
            Case MouseState.Over
                G.DrawString(Text, Font, New SolidBrush(TextHoverColor), New Rectangle(0, 1, Width, Height), CenterSF)
            Case Else
                G.DrawString(Text, Font, New SolidBrush(TextColor), New Rectangle(0, 1, Width, Height), CenterSF)
        End Select

        MyBase.OnPaint(e) : G.Dispose() : e.Graphics.InterpolationMode = 7 : e.Graphics.DrawImageUnscaled(B, 0, 0) : B.Dispose()
    End Sub

End Class

#End Region

#Region " Separator "

Public Class LineSeparator : Inherits Control

    Public Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor, True) : DoubleBuffered = True
        Width = 350
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.DrawLine(New Pen(BackColor, 0.5), New Point(0, 0), New Point(Width, 0.5))
        g.DrawLine(New Pen(ForeColor, 0.5), New Point(0, 0.2), New Point(Width, 0.5))
    End Sub

End Class

#End Region

#Region " CheckBox "

<ComponentModel.DefaultEvent("CheckedChanged")> Class NSCheckBox : Inherits Control

    Event CheckedChanged(sender As Object)

    Sub New()
        SetStyle(DirectCast(139286, ControlStyles), True)
        SetStyle(ControlStyles.Selectable, False)

        P11 = New Pen(Color.FromArgb(50, 50, 50))
        P22 = New Pen(Color.FromArgb(35, 35, 35))
        P3 = New Pen(Color.Black, 2.0F)
        P4 = New Pen(Color.FromArgb(205, 150, 0), 2.0F)
    End Sub

    Private _Checked As Boolean
    Public Property Checked() As Boolean
        Get
            Return _Checked
        End Get
        Set(value As Boolean)
            _Checked = value
            RaiseEvent CheckedChanged(Me)

            Invalidate()
        End Set
    End Property

    '    Private GP1, GP2 As Drawing2D.GraphicsPath

    Private SZ1 As SizeF
    Private PT1 As PointF

    Private P11, P22, P3, P4 As Pen

    Private PB1 As Drawing2D.PathGradientBrush

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        G = e.Graphics : G.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        G.Clear(BackColor) : G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        'GP1 = CreateRound(0, 2, Height - 5, Height - 5, 5)
        'GP2 = CreateRound(1, 3, Height - 7, Height - 7, 5)
        Dim GP1 As New Drawing2D.GraphicsPath : GP1.AddRectangle(New RectangleF(0, 2, Height - 5, Height - 5))
        Dim GP2 As New Drawing2D.GraphicsPath : GP2.AddRectangle(New RectangleF(1, 3, Height - 7, Height - 7))

        P11 = New Pen(Color.FromArgb(50, 50, 50))

        PB1 = New Drawing2D.PathGradientBrush(GP1)
        PB1.CenterColor = Color.FromArgb(50, 50, 50)
        PB1.SurroundColors = {Color.FromArgb(45, 45, 45)}
        PB1.FocusScales = New PointF(0.3F, 0.3F)

        G.FillPath(PB1, GP1)
        G.DrawPath(P11, GP1)
        G.DrawPath(P22, GP2)

        If _Checked Then
            If Enabled = True Then
                P3 = New Pen(Color.Black, 2.0F)
                P4 = New Pen(Color.DarkGray, 2.0F)
            Else
                P3 = New Pen(Color.Black, 2.0F)
                P4 = New Pen(Color.FromArgb(102, 102, 102), 2.0F)
                Text = "Cambiado"
            End If
            G.DrawLine(P3, 5, Height - 9, 8, Height - 7)
            G.DrawLine(P3, 7, Height - 7, Height - 8, 7)

            G.DrawLine(P4, 4, Height - 10, 7, Height - 8)
            G.DrawLine(P4, 6, Height - 8, Height - 9, 6)
        End If

        SZ1 = G.MeasureString(Text, Font)
        PT1 = New PointF(Height - 3, Height \ 2 - SZ1.Height / 2)
        G.DrawString(Text, Font, Brushes.Black, PT1.X + 1, PT1.Y + 1)
        G.DrawString(Text, Font, New SolidBrush(ForeColor), PT1)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        Checked = Not Checked
    End Sub

End Class

#End Region


#End Region