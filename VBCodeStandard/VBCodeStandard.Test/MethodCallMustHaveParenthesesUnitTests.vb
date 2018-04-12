Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
    <TestClass>
    Public Class MethodCallMustHaveParenthesessUnitTests
        Inherits CodeFixVerifier

        ' No diagnostics expected to show up. 
        <TestMethod>
        Public Sub MethodCallMustHaveParenthesesNoDiagnostic()
            Const test1 = "Dim a = 3.ToString()"
            VerifyBasicDiagnostic(test1)

            Const test2 = "
		Module Module1
		    Sub Main()
				Main()
		    End Sub
		End Module"

            VerifyBasicDiagnostic(test2)


            Const test3 = "
		Module Module1
		    Sub Main()
				Dim a As New Mutex()
		    End Sub
		End Module"

            VerifyBasicDiagnostic(test3)
        End Sub

        ' Diagnostic And CodeFix both triggered And checked for. 
        <TestMethod>
        Public Sub MethodCallMustHaveParentheses()
            ' Invokation expression.
            Const test = "
Module Module1
	Sub Main()
		Dim a = 3.ToString
	End Sub
End Module"

            Dim expected = New DiagnosticResult() With {.Id = MethodCallMustHaveParenthesesAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

            VerifyBasicDiagnostic(test, expected)

            Const fixtest = "
Module Module1
	Sub Main()
		Dim a = 3.ToString()
    End Sub
End Module"
            VerifyBasicFix(test, fixtest, allowNewCompilerDiagnostics:=True)

            ' Object creation syntax.

            Const test3 = "
Module Module1
	Sub Main()
		Dim a As New TestClass
	End Sub

	Public Class TestClass
	End Class
End Module"

            VerifyBasicDiagnostic(test3, expected)

            Const test3fix = "
Module Module1
	Sub Main()
		Dim a As New TestClass()
    End Sub

	Public Class TestClass
	End Class
End Module"
            VerifyBasicFix(test3, test3fix, allowNewCompilerDiagnostics:=True)

            ' Object creation syntax.

            Const test4 = "
Module Module1
	Sub Main()
		Dim output = Test(Test2)
	End Sub

	Function Test1(input As String) As String
		Return ""a""
	End Sub

	Function Test2() As String
		Return ""a""
	End Sub
End Module"

            VerifyBasicDiagnostic(test4, expected)

            Const test4fix = "
Module Module1
	Sub Main()
		Dim output = Test(Test2())
	End Sub

	Function Test1(input As String) As String
		Return ""a""
	End Sub

	Function Test2() As String
		Return ""a""
	End Sub
End Module"
            VerifyBasicFix(test4, test4fix, allowNewCompilerDiagnostics:=True)

        End Sub

        Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
            Return New MethodCallMustHaveParenthesesCodeFixProvider()
        End Function

        Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
            Return New MethodCallMustHaveParenthesesAnalyzer()
        End Function



    End Class
End Namespace