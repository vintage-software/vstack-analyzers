using System;

namespace Vstack.Analyzers
{
    public static class Extensions
    {
        public static void ValidateNotNullParameter(this object target, string parameterName, string message = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }
        }
    }
}
