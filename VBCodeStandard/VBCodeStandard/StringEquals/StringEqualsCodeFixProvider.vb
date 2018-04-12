<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(StringEqualsCodeFixProvider)), [Shared]>
Public Class StringEqualsCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Fix string equals"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(StringEqualsAnalyzer.DiagnosticId)
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
		Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of BinaryExpressionSyntax)().First()

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedDocument:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, stringEquals As BinaryExpressionSyntax, cancellationToken As CancellationToken) As Task(Of Document)
		Dim newExpression As SyntaxNode = Nothing
		If IsEmptyStringLiteral(stringEquals.Right) Then
			newExpression = MakeStringIsNullOrEmptyExpression(stringEquals.Left)
		ElseIf IsEmptyStringLiteral(stringEquals.Left) Then
			newExpression = MakeStringIsNullOrEmptyExpression(stringEquals.Right)
		Else
			Dim stringEqualsCall = SyntaxFactory.SimpleMemberAccessExpression(SyntaxFactory.IdentifierName("String"), SyntaxFactory.IdentifierName("Equals"))

			Dim arg1 = SyntaxFactory.SimpleArgument(stringEquals.Left.WithoutTrivia())
			Dim arg2 = SyntaxFactory.SimpleArgument(stringEquals.Right.WithoutTrivia())
			Dim arg3 = SyntaxFactory.SimpleArgument(SyntaxFactory.SimpleMemberAccessExpression(SyntaxFactory.IdentifierName("StringComparison"), SyntaxFactory.IdentifierName("Ordinal")))
			Dim stringEqualsArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(Of ArgumentSyntax)({arg1, arg2, arg3}.ToList()))

			newExpression = SyntaxFactory.InvocationExpression(stringEqualsCall, stringEqualsArgument)
		End If
		If (stringEquals.Kind() = SyntaxKind.NotEqualsExpression) Then newExpression = SyntaxFactory.NotExpression(DirectCast(newExpression, ExpressionSyntax))

		Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
		Dim newRoot = oldRoot.ReplaceNode(stringEquals, newExpression)
		Return document.WithSyntaxRoot(newRoot)
	End Function

	Private Function IsEmptyStringLiteral(node As SyntaxNode) As Boolean
		Dim stringLiteral = TryCast(node, LiteralExpressionSyntax)
		If (stringLiteral IsNot Nothing AndAlso String.Equals(stringLiteral.Token.Text, """""", StringComparison.Ordinal)) Then Return True
		Dim stringEmpty = TryCast(node, MemberAccessExpressionSyntax)
		If (stringEmpty IsNot Nothing AndAlso String.Equals(stringEmpty.ToString(), "String.Empty", StringComparison.OrdinalIgnoreCase)) Then Return True
		Return False
	End Function

	Private Function MakeStringIsNullOrEmptyExpression(param As ExpressionSyntax) As InvocationExpressionSyntax
		Dim stringIsNullOrEmptyCall = SyntaxFactory.SimpleMemberAccessExpression(SyntaxFactory.IdentifierName("String"), SyntaxFactory.IdentifierName("IsNullOrEmpty"))
		Dim arg = SyntaxFactory.SimpleArgument(param.WithoutTrivia())
		Dim stringIsNullOrEmptyArgument = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(Of ArgumentSyntax)({arg}.ToList()))
		Return SyntaxFactory.InvocationExpression(stringIsNullOrEmptyCall, stringIsNullOrEmptyArgument)
	End Function
End Class