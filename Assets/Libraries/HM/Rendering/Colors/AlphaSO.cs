public class AlphaSO : PersistentScriptableObject {

    public float alphaValue;

    public static implicit operator float(AlphaSO obj) {

        return obj.alphaValue;
    }
}
