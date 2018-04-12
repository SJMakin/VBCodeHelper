<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(ConvertToSingleLineIfCodeFixProvider)), [Shared]>
Public Class ConvertToSingleLineIfCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Convert to single line If"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(ConvertToSingleLineIfAnalyzer.DiagnosticId)
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

		Dim trivia = multiLineIf.GetLeadingTrivia()
		Dim statements = New SyntaxList(Of StatementSyntax)
		For Each statement In multiLineIf.Statements
			statement.GetLeadingTrivia().Select(Function(f) trivia = trivia.Add(f))
			statements = statements.Add(statement.WithoutLeadingTrivia())
		Next

		Dim childSingleLineIf = TryCast(statements.First(), SingleLineIfStatementSyntax)
		If childSingleLineIf IsNot Nothing Then
			condition = SyntaxFactory.AndAlsoExpression(condition, childSingleLineIf.Condition)
			statements = childSingleLineIf.Statements
		End If

		Dim singleLineIf = SyntaxFactory.SingleLineIfStatement(condition, statements, Nothing).WithLeadingTrivia(trivia)

		Return Await CodeFixHelper.ReplaceNodeInDocument(document, multiLineIf, singleLineIf, cancellationToken)
	End Function
End Class