<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class SingleLineIfStatementAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.SingleLineIfStatementParentheses

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.SingleLineIfStatementParenthesesAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.SingleLineIfStatementParenthesesAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.SingleLineIfStatementParenthesesAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.SingleLineIfStatement)
    End Sub

    Private Function ContainsUnparenthesizedBinaryExpression(node As SyntaxNode) As Boolean
		If TypeOf node Is ParenthesizedExpressionSyntax Then Return False
		If TypeOf node Is BinaryExpressionSyntax Then Return True
		For Each child In node.ChildNodes()
			If ContainsUnparenthesizedBinaryExpression(child) Then Return True
		Next
		Return False
	End Function

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim node = DirectCast(context.Node, SingleLineIfStatementSyntax)

        If ContainsUnparenthesizedBinaryExpression(node.Condition) Then
            Dim diag = Diagnostic.Create(Rule, node.Condition.GetLocation(), node.Condition.GetText())
            context.ReportDiagnostic(diag)
        End If
    End Sub
End Class