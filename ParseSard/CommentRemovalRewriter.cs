using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParseSard
{
    public class CommentRemovalRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            SyntaxTrivia returnValue;
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                returnValue = default(SyntaxTrivia);
            }
            else
            {
                returnValue = trivia;
            }
            return base.VisitTrivia(returnValue);
        }
    }
}
