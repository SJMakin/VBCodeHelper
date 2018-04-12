<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class StringEqualsAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.StringEquals

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression)
    End Sub

    Protected Overrides Sub CheckSyntax(node As SyntaxNodeAnalysisContext)
        Dim equalsExpression = DirectCast(node.Node, BinaryExpressionSyntax)

        Dim leftPart = equalsExpression.Left
        Dim rightPart = equalsExpression.Right

        Dim leftTypeInfo = node.SemanticModel.GetTypeInfo(leftPart)
        Dim rightTypeInfo = node.SemanticModel.GetTypeInfo(rightPart)

        If (rightTypeInfo.Type Is Nothing OrElse leftTypeInfo.Type Is Nothing) Then Exit Sub

        If Not (leftTypeInfo.Type.SpecialType = SpecialType.System_String AndAlso rightTypeInfo.Type.SpecialType = SpecialType.System_String) Then Exit Sub

        Dim diag = Diagnostic.Create(Rule, node.Node.GetLocation(), node.Node.GetText())
        node.ReportDiagnostic(diag)
    End Sub
End Class