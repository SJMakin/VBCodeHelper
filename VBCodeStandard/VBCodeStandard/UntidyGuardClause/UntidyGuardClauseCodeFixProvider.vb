<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(UntidyGuardClauseCodeFixProvider)), [Shared]>
Public Class UntidyGuardClauseCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Tidy guard clause"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(UntidyGuardClauseAnalyzer.DiagnosticId)
		End Get
	End Property

	Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
		Return WellKnownFixAllProviders.BatchFixer
	End Function

	Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
		Dim root = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

		Dim diagnostic = context.Diagnostics.First()
		Dim diagnosticSpan = diagnostic.Location.SourceSpan

		' Find the type statement identified by the diagnostic.
		Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of MultiLineIfBlockSyntax)().First()

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, multiLineIf As MultiLineIfBlockSyntax, cancellationToken As CancellationToken) As Task(Of Document)
		Dim condition = multiLineIf.IfStatement.Condition
		If (condition Is Nothing) Then Return document


		Dim trailingStatements As SyntaxList(Of StatementSyntax)
		Dim terminatingStatement As SyntaxList(Of StatementSyntax)
		If (multiLineIf.Statements.Count = 1 AndAlso UntidyGuardClauseAnalyzer.TerminatingStatements.Contains(multiLineIf.Statements.First().GetType())) Then
			terminatingStatement = multiLineIf.Statements
			trailingStatements = multiLineIf.ElseBlock.Statements
		ElseIf multiLineIf.ElseBlock.Statements.Count = 1 Then
			terminatingStatement = multiLineIf.ElseBlock.Statements
			trailingStatements = multiLineIf.Statements
			condition = SyntaxFactory.NotExpression(condition)
		Else
			Return document
		End If

		Dim trivia = multiLineIf.GetLeadingTrivia()
		Dim newTerminatingStatement As New SyntaxList(Of StatementSyntax)
		' Remove comments from the SingleLineIf statements part and put them before the SingleLineIf.
		For Each statement In terminatingStatement
			statement.GetLeadingTrivia().Select(Function(f) trivia = trivia.Add(f))
			newTerminatingStatement = newTerminatingStatement.Add(statement.WithoutLeadingTrivia())
		Next

		Dim singleLineIf = SyntaxFactory.SingleLineIfStatement(condition, newTerminatingStatement, Nothing).WithLeadingTrivia(trivia).AddStatements(trailingStatements.ToArray())

		Return Await CodeFixHelper.ReplaceNodeInDocument(document, multiLineIf, singleLineIf, cancellationToken)
	End Function
End Class