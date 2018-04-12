Public MustInherit Class VbCodingStandardsAnalyzerBase
    Inherits DiagnosticAnalyzer

    Protected Sub Analyze(context As SyntaxNodeAnalysisContext)
        If context.IsGenerated Then Exit Sub
        CheckSyntax(context)
    End Sub

    Protected Sub Analyze(context As SyntaxTreeAnalysisContext)
        If context.IsGenerated Then Exit Sub
        CheckSyntaxTree(context)
    End Sub

    Protected Overridable Sub CheckSyntax(context As SyntaxNodeAnalysisContext)
    End Sub

    Protected Overridable Sub CheckSyntaxTree(context As SyntaxTreeAnalysisContext)
    End Sub
End Class
