<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(CommentFormatCodeFixProvider)), [Shared]>
Public Class VariableDeclarationCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Variable declaration"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(VariableDeclarationAnalyzer.DiagnosticId)
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
		Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().First(Function(f) TypeOf f Is FieldDeclarationSyntax OrElse TypeOf f Is LocalDeclarationStatementSyntax)

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, declaration As SyntaxNode, cancellationToken As CancellationToken) As Task(Of Document)
		Dim newDeclaration As SyntaxNode = Nothing

		Dim localDeclaration = TryCast(declaration, LocalDeclarationStatementSyntax)
		If (localDeclaration IsNot Nothing) Then newDeclaration = SyntaxFactory.LocalDeclarationStatement(localDeclaration.Modifiers, NewDeclarators(localDeclaration.Declarators, document))

		Dim fieldDeclaration = TryCast(declaration, FieldDeclarationSyntax)
		If (fieldDeclaration IsNot Nothing) Then newDeclaration = SyntaxFactory.FieldDeclaration(fieldDeclaration.AttributeLists, fieldDeclaration.Modifiers, NewDeclarators(fieldDeclaration.Declarators, document))

		Return Await CodeFixHelper.ReplaceNodeInDocument(document, declaration, newDeclaration, cancellationToken)
	End Function

	Private Function NewDeclarators(oldDelcarators As SeparatedSyntaxList(Of VariableDeclaratorSyntax), document As Document) As SeparatedSyntaxList(Of VariableDeclaratorSyntax)
		Dim result As New SeparatedSyntaxList(Of VariableDeclaratorSyntax)
		For Each declarator In oldDelcarators
			Dim typeOfDeclarator = DelcaratorType(declarator, document)
			If typeOfDeclarator IsNot Nothing Then
				If typeOfDeclarator.IsValueType Then
					' Clear the initializer. 
					result = result.Add(ModifyDelcaratorInitializer(declarator, Nothing))
				Else
					If declarator.Initializer Is Nothing Then
						' Add = Nothing initializer.
						result = result.Add(ModifyDelcaratorInitializer(declarator, EqualsNothings()))
					ElseIf TypeOf declarator.Initializer.Value Is ObjectCreationExpressionSyntax Then
						' Convert to AsNew syntax.
						result = result.Add(SyntaxFactory.VariableDeclarator(declarator.Names, SyntaxFactory.AsNewClause(DirectCast(declarator.Initializer.Value, NewExpressionSyntax)), Nothing))
					End If
				End If
			End If
		Next
		Return result
	End Function

	Private Shared Function ModifyDelcaratorInitializer(delcarator As VariableDeclaratorSyntax, initializer As EqualsValueSyntax) As VariableDeclaratorSyntax
		' Remove then add trivia to ensure it is preserved.
		Return delcarator.WithoutTrivia().WithInitializer(initializer).WithTriviaFrom(delcarator)
	End Function

	Private Shared Function EqualsNothings() As EqualsValueSyntax
		Return SyntaxFactory.EqualsValue(SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword)))
	End Function

	Private Shared Function DelcaratorType(declarator As VariableDeclaratorSyntax, document As Document) As ITypeSymbol
		' Check if it has an as clause.
		If (declarator.AsClause Is Nothing) Then Return Nothing

		' This only makes sense for simple as clauses (as opposed to As New syntax).
		Dim simpleAs = TryCast(declarator.AsClause, SimpleAsClauseSyntax)
		If (simpleAs Is Nothing) Then Return Nothing

		' Get info about type.
		Dim initInfo = document.GetSemanticModelAsync().Result.GetTypeInfo(simpleAs.Type)
		Return initInfo.Type
	End Function
End Class