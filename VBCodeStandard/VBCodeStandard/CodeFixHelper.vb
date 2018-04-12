Imports Microsoft.CodeAnalysis.Formatting

Public NotInheritable Class CodeFixHelper


	Public Shared Async Function RenameSymbolAsync(document As Document, root As SyntaxNode, declarationToken As SyntaxToken, newName As String, cancellationToken As CancellationToken) As Task(Of Solution)
		Dim annotatedRoot = root.ReplaceToken(declarationToken, declarationToken.WithAdditionalAnnotations(RenameAnnotation.Create()))
		Dim annotatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, annotatedRoot)
		Dim annotatedDocument = annotatedSolution.GetDocument(document.Id)
		annotatedRoot = Await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(False)
		Dim annotatedToken = annotatedRoot.FindToken(declarationToken.SpanStart)
		Dim semanticModel = Await annotatedDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(False)
		Dim symbol = semanticModel?.GetDeclaredSymbol(annotatedToken.Parent, cancellationToken)
		Dim newSolution = Await Renamer.RenameSymbolAsync(annotatedSolution, symbol, newName, Nothing, cancellationToken).ConfigureAwait(False)
		Return newSolution
	End Function

	''' <summary>
	''' Replaces a node and formats the document.
	''' </summary>
	''' <param name="document">Document to replace the node in.</param>
	''' <param name="oldNode">Node to replace.</param>
	''' <param name="newNode">New node to use in replacement.</param>
	''' <param name="cancellationToken">The cancellation token.</param>
	''' <returns>A properly formatted Document with the node replaced.</returns>
	Public Shared Async Function ReplaceNodeAndFormatDocument(document As Document, oldNode As SyntaxNode, newNode As SyntaxNode, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Document)
		Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
		Dim newRoot = oldRoot.ReplaceNode(oldNode, newNode)
		Return Await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), Nothing, cancellationToken)
	End Function

	''' <summary>
	''' Replaces a node and formats the document.
	''' </summary>
	''' <param name="document">Document to replace the node in.</param>
	''' <param name="oldNode">Node to replace.</param>
	''' <param name="newNode">New node to use in replacement.</param>
	''' <param name="cancellationToken">The cancellation token.</param>
	''' <returns>A properly formatted Document with the node replaced.</returns>
	Public Shared Async Function ReplaceNodeInDocument(document As Document, oldNode As SyntaxNode, newNode As SyntaxNode, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Document)
		Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
		Dim newRoot = oldRoot.ReplaceNode(oldNode, newNode)
		Return document.WithSyntaxRoot(newRoot)
	End Function
End Class
