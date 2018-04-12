Imports System.Text

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class CommentFormatAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.CommentFormat

    Private Const Category = "Naming"

    Private ReadOnly Punctuation As Char() = {"."c, "?"c, "!"c, ","c}

    Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.CommentFormatRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.CommentFormatAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.CommentFormatAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault:=True, description:=Description)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxTreeAction(AddressOf Analyze)
    End Sub

    Protected Overrides Sub CheckSyntaxTree(context As SyntaxTreeAnalysisContext)
        Dim root As SyntaxNode = context.Tree.GetCompilationUnitRoot()
        Dim commentNodes = From node In root.DescendantTrivia() Where node.IsKind(SyntaxKind.CommentTrivia) Select node
        If Not commentNodes.Any() Then Return

        For Each node In commentNodes
            Dim warningMessage As New StringBuilder()

            Dim commentText As String = node.ToString().Substring(1)
            Dim trimmedCommentText As String = commentText.Trim()

            If String.IsNullOrWhiteSpace(commentText) Then warningMessage.AppendLine("Empty comments should be removed")

            If node.Token.TrailingTrivia.Contains(node) Then warningMessage.AppendLine("Put comments on a separate line instead of at the end of a line of code")

            If (commentText.ToCharArray().TakeWhile(Function(c) c = " "c).Count() <> 1) Then warningMessage.AppendLine("Comment should have one space at the start")

            If Char.IsLower(trimmedCommentText.ToCharArray().FirstOrDefault()) Then warningMessage.AppendLine("Start comment text with an uppercase letter")

            If (trimmedCommentText.Length > 0 AndAlso Not Punctuation.Contains(trimmedCommentText.ToCharArray().Last())) Then warningMessage.AppendLine("Comment should end with punctuation")

            Dim msg As String = warningMessage.ToString().TrimEnd(Environment.NewLine.ToCharArray())

            If Not String.IsNullOrWhiteSpace(msg) Then
                Dim diag = Diagnostic.Create(Rule, node.GetLocation(), warningMessage)
                context.ReportDiagnostic(diag)
            End If
        Next
    End Sub
End Class