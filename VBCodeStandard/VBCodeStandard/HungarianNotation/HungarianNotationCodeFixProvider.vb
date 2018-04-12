<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(HungarianNotationCodeFixProvider)), [Shared]>
Public Class HungarianNotationCodeFixProvider
	Inherits CodeFixProvider

	Private Const title As String = "Remove Hungarian Notation"

	Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
		Get
			Return ImmutableArray.Create(HungarianNotationAnalyzer.DiagnosticId)
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
		Dim declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of LocalDeclarationStatementSyntax)().First()

		' Register a code action that will invoke the fix.
		context.RegisterCodeFix(
			CodeAction.Create(
				title:=title,
				createChangedSolution:=Function(c) Fix(context.Document, declaration, c),
				equivalenceKey:=title),
			diagnostic)
	End Function

	Private Async Function Fix(document As Document, declaration As LocalDeclarationStatementSyntax, cancellationToken As CancellationToken) As Task(Of Solution)
		Dim sm As SemanticModel = Nothing
		document.TryGetSemanticModel(sm)
		If (sm Is Nothing) Then Return Nothing

		Dim result As Solution = document.Project.Solution

		Dim delcarations As New SeparatedSyntaxList(Of VariableDeclaratorSyntax)
		For Each declarator In declaration.Declarators
			For Each name In declarator.Names
				Dim varName As String = name.Identifier.Text
				If Not (varName.Length > 1 AndAlso Char.IsLower(varName(0)) AndAlso Char.IsUpper(varName(1))) Then Continue For

				Dim newName = varName.Substring(1)
				newName = newName.Substring(0, 1).ToLower() & newName.Substring(1)

				Dim nameInfo = sm.GetDeclaredSymbol(name, Nothing)
				If sm.LookupSymbols(declaration.SpanStart, Nothing, newName, False).Any() Then Continue For

				result = Await Renamer.RenameSymbolAsync(result, nameInfo, newName, Nothing, cancellationToken)
			Next
		Next

		Return result
	End Function
End Class