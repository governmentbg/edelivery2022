namespace ED.Domain
{
    public static class SqlServerErrorCodes
    {
        public const int ViolationOfUniqueIndex = 2601;
        public const int ViolationOfPrimaryKeyConstraint = 2627;
        public const int Timeout = -2;
    }
}
