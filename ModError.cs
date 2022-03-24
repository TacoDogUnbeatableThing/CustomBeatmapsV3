namespace CustomBeatmaps
{
    public struct ModError
    {
        public string ErrorText;

        public ModError(string errorText)
        {
            ErrorText = errorText;
        }

        public static implicit operator ModError(string errorText)
        {
            return new ModError(errorText);
        }

        public override string ToString()
        {
            return ErrorText;
        }
    }
}