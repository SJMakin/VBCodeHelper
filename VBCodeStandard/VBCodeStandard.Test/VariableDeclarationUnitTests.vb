Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
	<TestClass>
	Public Class VariableDeclarationUnitTests
		Inherits CodeFixVerifier

		' No diagnostics expected to show up. 
		<TestMethod>
		Public Sub VaraibleDeclarationNoDiagnostic()
			Const BasicTest = "
		Module Module1
		    Sub Main()
				Dim a As Integer = 45
		    End Sub
		End Module"

			Const ValueTypeInLoop = "
		Module Module1
		    Sub Main()
				For i as Integer = 0 To 100
					Dim a As Integer = 0
				Next
		    End Sub
		End Module"

			VerifyBasicDiagnostic(BasicTest)
			VerifyBasicDiagnostic(ValueTypeInLoop)
		End Sub

		' Diagnostic And CodeFix both triggered And checked for. 
		<TestMethod>
		Public Sub VariableDeclaration()

			Const test1 = "
Module Module1
	Sub Main()
		Dim a As Integer = 0
	End Sub
End Module"
			Const test2 = "
Module Module1
	Sub Main()
		Dim a As ArgumentException
	End Sub
End Module"

			Const test3 = "
Module Module1
	Sub Main()
		Dim a As ArgumentException = New ArgumentException  
	End Sub
End Module"

			Dim expected = New DiagnosticResult() With {.Id = VariableDeclarationAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

			VerifyBasicDiagnostic(test1, expected)
			VerifyBasicDiagnostic(test2, expected)
			VerifyBasicDiagnostic(test3, expected)

			Const fixTest1 = "
Module Module1
    Sub Main()
        Dim a As Integer
    End Sub
End Module"

			Const fixTest2 = "
Module Module1
    Sub Main()
        Dim a As ArgumentException = Nothing
    End Sub
End Module"

			Const fixTest3 = "
Module Module1
    Sub Main()
        Dim a As New ArgumentException
    End Sub
End Module"

			VerifyBasicFix(test1, fixTest1, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test2, fixTest2, allowNewCompilerDiagnostics:=True)
			VerifyBasicFix(test3, fixTest3, allowNewCompilerDiagnostics:=True)
		End Sub

		Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
			Return New VariableDeclarationCodeFixProvider()
		End Function

		Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
			Return New VariableDeclarationAnalyzer()
		End Function

	End Class
End Namespace