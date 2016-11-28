using System.CodeDom;

namespace ServiceGenerator
{
    /// <summary>
    ///     Build comment as sumary
    /// </summary>
    public static class SumaryProperty
    {
        /// <summary>
        ///     Add Sumary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="comment"></param>
        public static void Sumary(this CodeMemberProperty code, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
            }
            else
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary/>", true)));
            }
        }
    }

    public static class SumaryMethod
    {
        /// <summary>
        ///     Add Sumary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="comment"></param>
        public static void Sumary(this CodeMemberMethod code, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
            }
            else
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary/>", true)));
            }
        }
    }

    public static class SumaryField
    {
        /// <summary>
        ///     Add Sumary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="comment"></param>
        public static void Sumary(this CodeMemberField code, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
            }
            else
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary/>", true)));
            }
        }
    }

    public static class SumaryNamespace
    {
        /// <summary>
        ///     Add Sumary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="comment"></param>
        public static void Sumary(this CodeNamespace code, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
            }
            else
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary/>", true)));
            }
        }
    }

    public static class SumaryType
    {
        /// <summary>
        ///     Add Sumary
        /// </summary>
        /// <param name="code"></param>
        /// <param name="comment"></param>
        public static void Sumary(this CodeTypeMember code, string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
                code.Comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
            }
            else
            {
                code.Comments.Add(new CodeCommentStatement(new CodeComment("<summary/>", true)));
            }
        }
    }
}