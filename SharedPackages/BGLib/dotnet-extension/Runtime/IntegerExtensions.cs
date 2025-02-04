namespace BGLib.DotnetExtension {
    
    public static class IntegerExtensions {
        
        public static void ToUInt(this int number, out uint uNumber, out bool isNegative) {
        
            if (number < 0) {
                uNumber = (uint)((number == int.MinValue) ? number : -number);
                isNegative = true;
            }
            else {
                uNumber = (uint)number;
                isNegative = false;
            }
        }
    }
}
