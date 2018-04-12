'<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
'Public Class UnreachableAnalyzer
'    Inherits VbCodingStandardsAnalyzerBase

'    Public Const DiagnosticId = DiagnosticIdProvider.Unreachable

'    Private Const Category = "Performance"

'    Private Shared ReadOnly Title As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
'    Private Shared ReadOnly MessageFormat As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
'    Private Shared ReadOnly Description As New LocalizableResourceString(NameOf(My.Resources.StringEqualsRuleAnalyzerDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
'    Private Shared Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, description:=Description)

'    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
'        Get
'            Return ImmutableArray.Create(Rule)
'        End Get
'    End Property

'    Public Overrides Sub Initialize(context As AnalysisContext)
'        context.RegisterSyntaxNodeAction(AddressOf Analyze, SyntaxKind.Function)
'    End Sub

'    Private Sub AnalyzeSemanticModel(ByVal context As SemanticModelAnalysisContext)

'        For i As Integer = 1 To 10
'            Exit For
'        Next

'        Exit Sub
'        Dim tree = context.SemanticModel.SyntaxTree
'        Dim semanticModel = context.SemanticModel
'        Dim cancellationToken = context.CancellationToken
'        Dim root = semanticModel.SyntaxTree.GetRoot(cancellationToken)
'        Dim diagnostics = semanticModel.GetDiagnostics(cancellationToken:=cancellationToken)
'        For Each diagnostic In diagnostics
'            cancellationToken.ThrowIfCancellationRequested()
'            If diagnostic.Id = CS0162 Then Stop
'        Next
'    End Sub
'End Class