Imports VBCodeStandard.Compatibility

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class AvoidMicrosoftVisualBasicNamespaceAnalyzer
    Inherits VbCodingStandardsAnalyzerBase

    Public Const DiagnosticWithoutFixId = DiagnosticIdProvider.AvoidMicrosoftVisualBasicNamespace
    Public Const DiagnosticWithFixId = DiagnosticIdProvider.AvoidMicrosoftVisualBasicNamespaceWithFix

    Private Const Category = "Compatibility"

    Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.AvoidMicrosoftVisualBasicNamespaceAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.AvoidMicrosoftVisualBasicNamespaceAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.AvoidMicrosoftVisualBasicNamespaceAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly RuleWithoutFix As New DiagnosticDescriptor(DiagnosticWithoutFixId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)
    Private Shared ReadOnly RuleWithFix As New DiagnosticDescriptor(DiagnosticWithFixId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(RuleWithoutFix, RuleWithFix)
        End Get
    End Property

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.IdentifierName)
    End Sub

    Protected Overrides Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
        Dim node = DirectCast(context.Node, IdentifierNameSyntax)
        Dim symbolInfo = context.SemanticModel.GetSymbolInfo(node)
        If (symbolInfo.Symbol Is Nothing) Then Exit Sub

        If String.Equals(symbolInfo.Symbol.ContainingNamespace.ToDisplayString(), "Microsoft.VisualBasic", StringComparison.Ordinal) Then
            Dim descriptor = If(MethodReplacements.IsFound(symbolInfo.Symbol.Name), RuleWithFix, RuleWithoutFix)
            Dim diag = Diagnostic.Create(descriptor, node.GetLocation(), node.GetText())
            context.ReportDiagnostic(diag)
        End If
    End Sub
End Class