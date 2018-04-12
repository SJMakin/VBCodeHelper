<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class ConvertToSingleLineIfAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticId = DiagnosticIdProvider.ConvertToSingleLineIf

	Private Const Category = "Naming"

	Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.ConvertToSingleLineIfRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.ConvertToSingleLineIfRuleAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.ConvertToSingleLineIfRuleAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
	Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

	Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Return ImmutableArray.Create(Rule)
		End Get
	End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.MultiLineIfBlock)
    End Sub

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim multiLineIf = DirectCast(context.Node, MultiLineIfBlockSyntax)
        If multiLineIf.ChildNodes().Count() = 3 AndAlso multiLineIf.Statements.Count() = 1 Then
            Dim statement = multiLineIf.Statements.First()
            Dim span = statement.GetLocation().GetLineSpan()
            If span.StartLinePosition.Line = span.EndLinePosition.Line Then
                Dim diag = Diagnostic.Create(Rule, multiLineIf.IfStatement.GetLocation(), context.Node.GetText())
                context.ReportDiagnostic(diag)
            End If
        End If
    End Sub
End Class