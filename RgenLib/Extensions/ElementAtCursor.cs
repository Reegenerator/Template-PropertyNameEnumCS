using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace RgenLib.Extensions {
    public static class ElementAtCursor {

        static public CodeElement GetCodeElementAtCursor(DTE dte, vsCMElement elementType) {

            try {
                CodeElement objCodeElement = null;

                var objCursorTextPoint = GetCursorTextPoint(dte);

                if ((objCursorTextPoint != null)) {
                    // Get the class at the cursor
                    objCodeElement = GetCodeElementAtTextPoint(elementType, dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements, objCursorTextPoint);
                }

                //if (objCodeElement == null) {
                //    MessageBox.Show("No matching elementType found at the cursor!");
                //}
                //else {
                //    MessageBox.Show("Class at the cursor: " + objCodeElement.FullName);
                //}
                return objCodeElement;
            }
            catch (Exception ex)
            {
                Debug.DebugHere();
            }
            return null;
        }

        static public CodeClass2 GetClassAtCursor(DTE dte)
        {
            return (CodeClass2)GetCodeElementAtCursor(dte, vsCMElement.vsCMElementClass);
        }


        public static EnvDTE.TextPoint GetCursorTextPoint(DTE dte) {

            VirtualPoint objCursorTextPoint = null;

            try {
                var objTextDocument = (TextDocument)dte.ActiveDocument.Object();
                objCursorTextPoint = objTextDocument.Selection.ActivePoint;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception ) {
                //swallow exception
            }

            return objCursorTextPoint;

        }

        private static CodeElement GetCodeElementAtTextPoint(vsCMElement eRequestedCodeElementKind, CodeElements colCodeElements, EnvDTE.TextPoint objTextPoint) {


            CodeElement objResultCodeElement = null;
            CodeElements colCodeElementMembers = default(CodeElements);
            CodeElement objMemberCodeElement = default(CodeElement);


            if ((colCodeElements != null)) {

                foreach (var objCodeElement in colCodeElements.Cast<CodeElement>()) {

                    if (objCodeElement.StartPoint.GreaterThan(objTextPoint)) {
                        // The code element starts beyond the point


                    }
                    else if (objCodeElement.EndPoint.LessThan(objTextPoint)) {
                        // The code element ends before the point

                        // The code element contains the point
                    }
                    else {

                        if (objCodeElement.Kind == eRequestedCodeElementKind) {
                            // Found
                            objResultCodeElement = objCodeElement;
                        }

                        // We enter in recursion, just in case there is an inner code element that also 
                        // satisfies the conditions, for example, if we are searching a namespace or a class
                        colCodeElementMembers = GetCodeElementMembers(objCodeElement);

                        objMemberCodeElement = GetCodeElementAtTextPoint(eRequestedCodeElementKind, colCodeElementMembers, objTextPoint);

                        if ((objMemberCodeElement != null)) {
                            // A nested code element also satisfies the conditions
                            objResultCodeElement = objMemberCodeElement;
                        }

                        break;

                    }

                }

            }

            return objResultCodeElement;

        }

        private static CodeElements GetCodeElementMembers(CodeElement objCodeElement) {

            CodeElements colCodeElements = default(CodeElements);


            // ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
            if (objCodeElement is CodeNamespace) {
                colCodeElements = ((CodeNamespace)objCodeElement).Members;


            }
            else if (objCodeElement is CodeType) {
                colCodeElements = ((CodeType)objCodeElement).Members;


            }
            else if (objCodeElement is CodeFunction) {
                colCodeElements = ((CodeFunction)objCodeElement).Parameters;

            }

            return colCodeElements;
            // ReSharper restore CanBeReplacedWithTryCastAndCheckForNull

        }
    }
}
