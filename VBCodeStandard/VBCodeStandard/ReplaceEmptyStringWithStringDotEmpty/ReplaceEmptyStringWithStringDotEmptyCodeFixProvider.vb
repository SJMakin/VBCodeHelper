<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(StringEqualsCodeFixProvider)), [Shared]>
Public Class ReplaceEmptyStringWithStringDotEmptyCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Replace with String.Empty"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(ReplaceEmptyStringWithStringDotEmptyAnalyzer.DiagnosticId)
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
        Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of IdentifierNameSyntax)().First()

        ' Register a code action that will invoke the fix.
        context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

    Private Async Function Fix(document As Document, literalDeclaration As IdentifierNameSyntax, cancellationToken As CancellationToken) As Task(Of Document)
        Dim stringDotEmptyInvocation = SyntaxFactory.ParseExpression("String.Empty").WithTriviaFrom(literalDeclaration)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
        Dim newRoot = oldRoot.ReplaceNode(literalDeclaration, stringDotEmptyInvocation)

        Return document.WithSyntaxRoot(newRoot)
    End Function
End Class