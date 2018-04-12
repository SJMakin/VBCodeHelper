<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(CommentFormatCodeFixProvider)), [Shared]>
Public Class CommentFormatCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Format comment"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(CommentFormatAnalyzer.DiagnosticId)
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
		Dim declaration = root.FindTrivia(diagnosticSpan.Start, True)

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, comment As SyntaxTrivia, cancellationToken As CancellationToken) As Task(Of Document)
		Dim commentText As String = comment.ToString().Substring(1)

		Dim oldToken = comment.Token
		Dim newToken = oldToken.ReplaceTrivia(comment, Nothing)

		Dim oldStatement As StatementSyntax = FindParentStatement(oldToken)
		Dim newStatement As StatementSyntax = Nothing

		If Not String.IsNullOrWhiteSpace(commentText) Then
			Dim newComment As String = "' " & commentText.Trim()
			If Not {".", "?", "!", ","}.Contains(newComment.ToCharArray().Last()) Then newComment &= ". "
			newComment = newComment.Substring(0, 3).ToUpper() & newComment.Substring(3)

			Dim isOnSameLine = oldToken.TrailingTrivia.Contains(comment)
			If isOnSameLine Then
				' Add the comment.
				newStatement = oldStatement.WithoutTrailingTrivia().WithLeadingTrivia(SyntaxFactory.CommentTrivia(newComment), SyntaxFactory.EndOfLineTrivia(Environment.NewLine)).WithTrailingTrivia(SyntaxFactory.EndOfLineTrivia(Environment.NewLine))
			Else
				newToken = newToken.WithLeadingTrivia(newToken.LeadingTrivia.Insert(newToken.LeadingTrivia.Count - 2, SyntaxFactory.CommentTrivia(newComment)))
			End If
		End If

		If (newStatement Is Nothing) Then newStatement = oldStatement.ReplaceToken(oldToken, newToken)

		Return Await CodeFixHelper.ReplaceNodeInDocument(document, oldStatement, newStatement, cancellationToken)
	End Function

	Private Function FindParentStatement(oldToken As SyntaxToken) As StatementSyntax
		Dim result As SyntaxNode = oldToken.Parent

		While result.Parent IsNot Nothing
			result = result.Parent
			If (TypeOf result Is StatementSyntax AndAlso result.DescendantTrivia().Any(Function(f) f.Kind() = SyntaxKind.EndOfLineTrivia)) Then Exit While
		End While

		Return TryCast(result, StatementSyntax)
	End Function

End Class