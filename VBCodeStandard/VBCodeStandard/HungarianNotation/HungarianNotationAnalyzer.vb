<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class HungarianNotationAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.HungarianNotation

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.HungarianNotationAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.HungarianNotationAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.HungarianNotationAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.LocalDeclarationStatement)
    End Sub

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim node = DirectCast(context.Node, LocalDeclarationStatementSyntax)

        For Each declarator In node.Declarators
            For Each identifier In declarator.Names
                Dim name = identifier.Identifier.Text
                If name.Length > 1 AndAlso Char.IsLower(name(0)) AndAlso Char.IsUpper(name(1)) Then
                    Dim diag = Diagnostic.Create(Rule, node.GetLocation(), node.GetText())
                    context.ReportDiagnostic(diag)
                End If
            Next
        Next
    End Sub
End Class