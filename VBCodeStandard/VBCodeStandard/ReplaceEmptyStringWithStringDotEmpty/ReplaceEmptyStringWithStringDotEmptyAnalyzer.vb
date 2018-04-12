<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class ReplaceEmptyStringWithStringDotEmptyAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.ReplaceEmptyStringWithStringDotEmpty

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.ReplaceEmptyStringWithStringDotEmptyAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.ReplaceEmptyStringWithStringDotEmptyAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.ReplaceEmptyStringWithStringDotEmptyAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.StringLiteralExpression)
    End Sub

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        If context.Node.AncestorsAndSelf().OfType(Of AttributeListSyntax).Any() Then Exit Sub

        Dim stringLiteral = DirectCast(context.Node, LiteralExpressionSyntax)
        If Not String.Equals(stringLiteral.Token.Text, """""", StringComparison.Ordinal) Then Exit Sub

        For Each node In stringLiteral.Ancestors()
            If node.IsKind(SyntaxKind.Parameter) Then Exit Sub
        Next

        Dim variableDeclaration = stringLiteral.Ancestors().OfType(Of FieldDeclarationSyntax).FirstOrDefault()
        If variableDeclaration IsNot Nothing Then
            For Each modifier In variableDeclaration.Modifiers
                If modifier.IsKind(SyntaxKind.ConstKeyword) Then Exit Sub
            Next
        End If

        If stringLiteral.AncestorsAndSelf().OfType(Of CaseStatementSyntax).Any() Then Exit Sub

        context.ReportDiagnostic(Diagnostic.Create(Rule, stringLiteral.GetLocation()))
    End Sub
End Class