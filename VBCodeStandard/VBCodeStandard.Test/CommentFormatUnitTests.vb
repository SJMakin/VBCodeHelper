Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
	<TestClass>
	Public Class CommentFormatUnitTests
		Inherits CodeFixVerifier

		' Diagnostic And CodeFix both triggered And checked for. 
		<TestMethod>
		Public Sub CommentFormat()

			Const test1 = "
Module Module1
	Sub Main()
		'    naughty comment
		Dim a = 1
	End Sub
End Module"


			Const test2 = "
Module Module1
	Sub Main()				
		Dim a = 1 ' This comment is not on its own line.
	End Sub
End Module"

			Dim expected = New DiagnosticResult() With {.Id = CommentFormatAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Hidden}

			VerifyBasicDiagnostic(test1, expected)

			Const fixtest1 = "
Module Module1
    Sub Main()
        ' Naughty comment. 
        Dim a = 1
    End Sub
End Module"
			VerifyBasicFix(test1, fixtest1, allowNewCompilerDiagnostics:=True)

			Const fixtest2 = "
Module Module1
    Sub Main()
        ' This comment is not on its own line.
        Dim a = 1
    End Sub
End Module"

			VerifyBasicFix(test2, fixtest2, allowNewCompilerDiagnostics:=True)
		End Sub

		' No diagnostics expected to show up. 
		<TestMethod>
		Public Sub CommentFormatNoDiagnostic()
			Const test = "
Module Module1
	Sub Main()
		' This is a correctly formatted comment.
		Dim a = 1
	End Sub
End Module"

			VerifyBasicDiagnostic(test)
		End Sub

		Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
			Return New CommentFormatCodeFixProvider()
		End Function

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New CommentFormatAnalyzer()
		End Function

	End Class
End Namespace