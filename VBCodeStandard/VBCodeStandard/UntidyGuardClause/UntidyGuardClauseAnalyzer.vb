<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class UntidyGuardClauseAnalyzer
	Inherits DiagnosticAnalyzer

	Public Const DiagnosticId = DiagnosticIdProvider.UntidyGuardClause

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.UntidyGuardClauseRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.UntidyGuardClauseRuleAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.UntidyGuardClauseRuleAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
		context.RegisterSyntaxNodeAction(AddressOf AnalyzeIf, SyntaxKind.MultiLineIfBlock)
	End Sub

	Private Sub AnalyzeIf(context As SyntaxNodeAnalysisContext)
		If (context.IsGenerated()) Then Return
		Dim multiLineIf = DirectCast(context.Node, MultiLineIfBlockSyntax)

		If (multiLineIf.ElseBlock Is Nothing OrElse multiLineIf.ElseIfBlocks.Count > 0) Then Exit Sub

		If (multiLineIf.Statements.Count = 1) Then
			Dim statement = multiLineIf.Statements.First()
			If TerminatingStatements.Contains(statement.GetType()) Then
				Dim diag = Diagnostic.Create(Rule, multiLineIf.IfStatement.GetLocation(), context.Node.GetText())
				context.ReportDiagnostic(diag)
				Exit Sub
			End If
		End If

		If multiLineIf.ElseBlock.Statements.Count = 1 Then
			Dim statement = multiLineIf.ElseBlock.Statements.First()
			If TerminatingStatements.Contains(statement.GetType()) Then
				Dim diag = Diagnostic.Create(Rule, multiLineIf.IfStatement.GetLocation(), context.Node.GetText())
				context.ReportDiagnostic(diag)
			End If
		End If


	End Sub

	Public Shared TerminatingStatements As ImmutableArray(Of Type) = ImmutableArray.Create(GetType(ReturnStatementSyntax),
																					 GetType(ThrowStatementSyntax),
																					 GetType(ExitStatementSyntax),
																					 GetType(YieldStatementSyntax))


End Class