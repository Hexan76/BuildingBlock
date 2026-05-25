namespace System
{
    public static class DecimalExtension
    {
        /// <summary>
        /// Converts a <see cref="decimal"/> number to a formatted string with thousand separators,
        /// while preserving any fractional part.
        /// </summary>
        /// <param name="number">The decimal number to format.</param>
        /// <returns>
        /// A formatted string representation of the number:
        /// - For integers → adds thousand separators (e.g. 1,200,000).  
        /// - For decimals → adds thousand separators and keeps fractional digits (e.g. 123,456.546).
        /// </returns>
        /// <example>
        /// <code>
        /// 1234567m.ToFormatted();     // "1,234,567"
        /// 123456.546m.ToFormatted();  // "123,456.546"
        /// </code>
        /// </example>
        public static string ToFormatted(this decimal number)
        {
            return number % 1 == 0
                ? number.ToString("#,0")
                : number.ToString("#,0.################");
        }
    }

}
