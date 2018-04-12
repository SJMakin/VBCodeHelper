Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
	<TestClass>
	Public Class SingleLineIfUnitTests
		Inherits CodeFixVerifier

		' No diagnostics expected to show up. 
		<TestMethod>
		Public Sub SingleLineIfNoDiagnostic()
			Const test = "If (1 > 3) Then Return"
			VerifyBasicDiagnostic(test)
		End Sub

		' Diagnostic And CodeFix both triggered And checked for. 
		<TestMethod>
		Public Sub SingleLineIf()

			Const test = "
Module Module1
	Sub Main()
		If 1 > 3 Then Return
	End Sub
End Module"

			Dim expected = New DiagnosticResult() With {.Id = SingleLineIfStatementAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

			VerifyBasicDiagnostic(test, expected)

			Const test2 = "
Module Module1
	Sub Main()
		If Not 1 > 3 Then Return
	End Sub
End Module"

			VerifyBasicDiagnostic(test2, expected)


			Const fixtest = "
Module Module1
    Sub Main()
        If (1 > 3) Then Return
    End Sub
End Module"
			VerifyBasicFix(test, fixtest, allowNewCompilerDiagnostics:=True)
		End Sub

		Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
			Return New SingleLineIfStatementParenthesesCodeFixProvider()
		End Function

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New SingleLineIfStatementAnalyzer()
		End Function

	End Class
End Namespace