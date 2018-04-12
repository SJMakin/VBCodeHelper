Imports VBCodeStandard.Test.TestHelper
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBCodeStandard.Test
    <TestClass>
    Public Class UntidyGuardClauseUnitTests
        Inherits CodeFixVerifier

        ' Diagnostic And CodeFix both triggered And checked for. 
        <TestMethod>
        Public Sub UntidyGuardClause()

            Const test1 = "
Module Module1
	Sub Main()
		If 1 = 1 Then
			Dim a As Integer = 45
		Else
			Throw New Exception
		End If
	End Sub
End Module
"

            Const test2 = "
Module Module1
	Sub Main()
		If 1 = 1 Then
			Throw New Exception
		Else
			Dim a As Integer = 45
		End If
	End Sub
End Module
"
            ' TODO: Add exit statement


            '			Const test3 = "
            'Module Module1
            '	Sub Main()
            '		If 1 = 1 Then
            '			Dim a As Integer = 45
            '			Dim b As Integer = 45
            '		End If
            '	End Sub
            'End Module
            '"

            Dim expected = New DiagnosticResult() With {.Id = UntidyGuardClauseAnalyzer.DiagnosticId, .Severity = DiagnosticSeverity.Info}

            VerifyBasicDiagnostic(test1, expected)
            VerifyBasicDiagnostic(test2, expected)
            'VerifyBasicDiagnostic(test3, expected)

            Const fixtest = "
Module Module1
    Sub Main()
        If Not 1 = 1 Then Throw New Exception
        Dim a As Integer = 45
    End Sub
End Module
"

            VerifyBasicFix(test1, fixtest, allowNewCompilerDiagnostics:=True)
        End Sub

        ' No diagnostics expected to show up. 
        <TestMethod>
        Public Sub UntidyGuardClauseNoDiagnostic()
            Const test = "
		Module Module1
		    Sub Main()
				If 1 = 1 Then
					Dim a As Integer = 45
				Else
					Dim a As Integer = 45
					Throw New Exception
				End If
		    End Sub
		End Module"
            VerifyBasicDiagnostic(test)
        End Sub

        Protected Overrides Function GetBasicCodeFixProvider() As CodeFixProvider
            Return New UntidyGuardClauseCodeFixProvider()
        End Function

        Protected Overrides Function GetBasicDiagnosticAnalyzer() As DiagnosticAnalyzer
            Return New UntidyGuardClauseAnalyzer()
        End Function

    End Class
End Namespace