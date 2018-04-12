Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
    <TestClass>
    Public Class ConvertToSinlgeLineIfUnitTests
        Inherits CodeFixVerifier

        ' No diagnostics expected to show up. 
        <TestMethod>
        Public Sub ConvertToSinlgeLineIfNoDiagnostic()
            Const test = "If (1 = 1) Then Return"
            VerifyBasicDiagnostic(test)
        End Sub

        ' Diagnostic And CodeFix both triggered And checked for. 
        <TestMethod>
        Public Sub ConvertToSinlgeLineIf()

            Const test = "
Module Module1
	Sub Main()
		If (1 = 1) Then
			Return
		End If
	End Sub
End Module"

            Const testIfMerge = "
Module Module1
	Sub Main()
		If (1 = 1) Then
			If 1 <> 1 Then Return
		End If
	End Sub
End Module"

            Dim expected = New DiagnosticResult() With {.Id = ConvertToSingleLineIfAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

            VerifyBasicDiagnostic(test, expected)
            VerifyBasicDiagnostic(testIfMerge, expected)

            Const fixtest = "
Module Module1
	Sub Main()
        If (1 = 1) Then Return
	End Sub
End Module"

            Const fixIfMerge = "
Module Module1
	Sub Main()
        If (1 = 1) AndAlso 1 <> 1 Then Return
	End Sub
End Module"
            VerifyBasicFix(test, fixtest, allowNewCompilerDiagnostics:=True)
            VerifyBasicFix(testIfMerge, fixIfMerge, allowNewCompilerDiagnostics:=True)
        End Sub

        Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
            Return New ConvertToSingleLineIfCodeFixProvider()
        End Function

        Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
            Return New ConvertToSingleLineIfAnalyzer()
        End Function

    End Class
End Namespace