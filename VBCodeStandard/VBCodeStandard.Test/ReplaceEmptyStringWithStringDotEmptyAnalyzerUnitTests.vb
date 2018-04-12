Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VSDiagnostics.Test.Tests.Strings

	<TestClass>
	Public Class ReplaceEmptyStringWithStringDotEmptyTests
		Inherits CodeFixVerifier

		<TestMethod>
		Public Sub ReplaceEmptyStringsWithStringDotEmpty()
			Const original = "
Module Module1
	Sub Main()
		Dim a = """"
	End Sub
End Module"
			Const result = "
Module Module1
    Sub Main()
        Dim a = String.Empty
    End Sub
End Module"

			Dim expected = New DiagnosticResult() With {.Id = ReplaceEmptyStringWithStringDotEmptyAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

			VerifyBasicDiagnostic(original, expected)
			VerifyBasicFix(original, result)
		End Sub


		<TestMethod>
		Public Sub ReplaceEmptyStringsWithStringDotEmpty_NoDiag()
			Const test = "Dim a = ""sfsdfd"""
			VerifyBasicDiagnostic(test)
		End Sub

		Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
			Return New ReplaceEmptyStringWithStringDotEmptyCodeFixProvider()
		End Function

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New ReplaceEmptyStringWithStringDotEmptyAnalyzer()
		End Function
	End Class
End Namespace
