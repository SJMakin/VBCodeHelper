<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(MethodCallMustHaveParenthesesCodeFixProvider)), [Shared]>
Public Class MethodCallMustHaveParenthesesCodeFixProvider
    Inherits CodeFixProvider

    Private Const title As String = "Add parentheses"

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create(MethodCallMustHaveParenthesesAnalyzer.DiagnosticId)
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

    Private Async Function Fix(document As Document, identifierName As IdentifierNameSyntax, cancellationToken As CancellationToken) As Task(Of Document)
        Dim target = identifierName.Parent
        If (target Is Nothing) Then Return document

        Dim trailingTrivia = target.GetTrailingTrivia()

        Dim newPart As SyntaxNode = Nothing
        If TypeOf target Is MemberAccessExpressionSyntax Then
            Dim parent = DirectCast(target, MemberAccessExpressionSyntax)
            newPart = SyntaxFactory.InvocationExpression(parent.WithoutTrailingTrivia(), SyntaxFactory.ArgumentList()).WithTrailingTrivia(trailingTrivia)
        ElseIf TypeOf target Is ObjectCreationExpressionSyntax Then
            Dim parent = DirectCast(target, ObjectCreationExpressionSyntax)
            newPart = parent.WithoutTrailingTrivia().AddArgumentListArguments().WithTrailingTrivia(parent.GetTrailingTrivia())
        ElseIf TypeOf target Is ArgumentSyntax Then
            newPart = SyntaxFactory.SimpleArgument(SyntaxFactory.ParseExpression(identifierName.ToString() & "()"))

        End If

        Return Await CodeFixHelper.ReplaceNodeInDocument(document, target, newPart, cancellationToken)
    End Function
End Class