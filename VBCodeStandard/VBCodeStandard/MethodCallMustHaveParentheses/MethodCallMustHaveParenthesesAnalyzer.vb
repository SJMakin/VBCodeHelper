<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class MethodCallMustHaveParenthesesAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.MethodCallMustHaveParentheses

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.MethodCallMustHaveParenthesesRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.MethodCallMustHaveParenthesesAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.MethodCallMustHaveParenthesesAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.IdentifierName)
    End Sub

    Private Function IsPartOfAddressOf(node As SyntaxNode) As Boolean
		If (node.Kind() = SyntaxKind.AddressOfExpression) Then Return True
		If (TypeOf node Is MemberAccessExpressionSyntax AndAlso node.Parent.Kind() = SyntaxKind.AddressOfExpression) Then Return True
		Return False
	End Function

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim node = DirectCast(context.Node, IdentifierNameSyntax)
        Dim symbolInfo = context.SemanticModel.GetSymbolInfo(node)
        If (symbolInfo.Symbol Is Nothing) Then Exit Sub

        Dim target = node.Parent

        Select Case symbolInfo.Symbol.Kind
            Case SymbolKind.Method
                ' Check for method calls.
                If (TypeOf target Is AttributeSyntax OrElse IsPartOfAddressOf(target) OrElse target.Kind() = SyntaxKind.QualifiedName) Then Exit Sub
                If Not (TypeOf target Is InvocationExpressionSyntax) Then target = target.Parent
                If Not target.ChildNodes().OfType(Of ArgumentListSyntax).Any() Then
                    Dim diag = Diagnostic.Create(Rule, node.GetLocation(), node.GetText())
                    context.ReportDiagnostic(diag)
                End If
            Case SymbolKind.NamedType
                ' Check for constructor calls.
                If TypeOf target Is ObjectCreationExpressionSyntax AndAlso Not target.ChildNodes().OfType(Of ArgumentListSyntax).Any() Then
                    Dim diag = Diagnostic.Create(Rule, node.GetLocation(), node.GetText())
                    context.ReportDiagnostic(diag)
                End If
            Case Else
        End Select
    End Sub
End Class