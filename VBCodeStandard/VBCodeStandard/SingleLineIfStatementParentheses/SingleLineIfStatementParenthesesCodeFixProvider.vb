<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(SingleLineIfStatementParenthesesCodeFixProvider)), [Shared]>
Public Class SingleLineIfStatementParenthesesCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Add parentheses"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(SingleLineIfStatementAnalyzer.DiagnosticId)
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
		Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of SingleLineIfStatementSyntax)().First()

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, singleLineIf As SingleLineIfStatementSyntax, cancellationToken As CancellationToken) As Task(Of Document)
		Dim perenthesizedExpression = SyntaxFactory.ParenthesizedExpression(singleLineIf.Condition)

		Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
		Dim newRoot = oldRoot.ReplaceNode(singleLineIf.Condition, perenthesizedExpression)

		Return document.WithSyntaxRoot(newRoot)
	End Function
End Class