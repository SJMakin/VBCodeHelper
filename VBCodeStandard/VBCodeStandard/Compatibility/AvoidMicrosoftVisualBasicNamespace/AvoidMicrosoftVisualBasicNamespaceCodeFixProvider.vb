Imports VBCodeStandard.Compatibility

<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(AvoidMicrosoftVisualBasicNamespaceAnalyzer)), [Shared]>
Public Class AvoidMicrosoftVisualBasicNamespaceCodeFixProvider
    Inherits CodeFixProvider

    Private Const title As String = "Convert to .NET equivalent"

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create(AvoidMicrosoftVisualBasicNamespaceAnalyzer.DiagnosticWithFixId)
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

    Private Async Function Fix(document As Document, identifier As IdentifierNameSyntax, cancellationToken As CancellationToken) As Task(Of Document)
        Dim info = MethodReplacements.GetByName(identifier.Identifier.ToString())
        If info Is Nothing Then Throw New Exception("Unable to locate method signiture")

        Dim invokeExpr = TryCast(identifier.Parent, InvocationExpressionSyntax)
        If invokeExpr Is Nothing Then Throw New Exception("Indentiers parent is not an invokation")

        Dim args = invokeExpr.ArgumentList.Arguments

        Dim parentIsNot = (invokeExpr.Parent.Kind = SyntaxKind.NotExpression)

        Dim expr As String = If(parentIsNot, info.NewNotPattern, info.NewPattern).Replace("**expression**", args(0).ToString())
        Dim newnode = SyntaxFactory.ParseExpression(expr).WithTriviaFrom(invokeExpr)

        Dim oldRoot = Await document.GetSyntaxRootAsync(cancellationToken)
        Dim newRoot = oldRoot.ReplaceNode(If(parentIsNot, invokeExpr.Parent, invokeExpr), newnode)

        Return document.WithSyntaxRoot(newRoot)
    End Function
End Class